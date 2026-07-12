# Plan: UI-Shell reparieren (Layout-Bug + Bootstrap 5.3 + Mobile Off-Canvas)

> **Hinweis an den ausführenden Agenten:** Dieser Plan ist so geschrieben, dass er ohne Rückfragen abgearbeitet werden kann. Dateiinhalte sind **vollständig** angegeben — kopiere sie exakt, ergänze nichts, lasse nichts weg. Wenn ein Schritt fehlschlägt, brich ab und melde den Fehler, statt zu improvisieren.

---

## 1. Kontext: Was ist kaputt und warum

**Symptom:** Das Navigationsmenü steht links, aber der gesamte Seiteninhalt erscheint erst eine volle Bildschirmhöhe darunter. Man muss zu jedem Inhalt erst scrollen.

**Ursache:** In `src/App/wwwroot/app.css`, Zeile 86, steht:

```css
page { flex-direction: row; }
```

Es fehlt der Punkt. `page` ist ein Element-Selektor, aber ein HTML-Element `<page>` gibt es nicht. Die Regel greift nie. Deshalb bleibt `.page` bei `flex-direction: column` (Zeile 21–25), die Sidebar behält ihre `height: 100vh` (Zeile 92), und `<main>` wird darunter gestapelt statt daneben.

**Zwei Folgeprobleme, die dabei gefunden wurden:**

1. `src/App/wwwroot/bootstrap/bootstrap.min.css` ist **kein echtes Bootstrap**, sondern ein 4-KB-Stub mit ca. 40 handgeschriebenen Regeln. Die Seiten unter `src/App/Components/Pages/Lebensmittel/` benutzen aber `form-control`, `form-label`, `form-select`, `table`, `table-striped`, `modal-dialog`, `modal-content`, `modal-header`, `modal-body`, `modal-footer`, `modal-backdrop`, `fade`, `show`, `spinner-border`, `btn-secondary`, `btn-danger`, `btn-warning`, `btn-sm`, `btn-outline-secondary`, `col-md-4`, `col-md-6`, `col-md-8`, `d-flex`, `mb-3`, `me-2`, `ms-auto`, `gap-2`, `text-end`, `container-fluid`, `input-group`, `alert-info`, `alert-warning`. **Keine dieser Klassen ist im Stub definiert.** Formulare, Tabellen und der Lösch-Dialog sind ungestylt.
2. In `src/App/wwwroot/` liegt **kein Bootstrap-JavaScript**. Der `navbar-toggler`-Button in `NavMenu.razor` tut deshalb nichts.

**Ziel nach Abschluss:**
- Inhalt beginnt oben rechts neben der Sidebar, ohne Scrollen.
- Echtes Bootstrap 5.3.3 lokal eingebunden (CSS + JS). Formulare/Tabellen/Modals sind dadurch gestylt.
- Auf Bildschirmen unter 768 px: Navigation ist eingeklappt, ein Menü-Button oben links slidet sie von links herein.
- Ab 768 px: Navigation ist die feste Sidebar links.

**Nicht im Scope:** Der Inhalt der Lebensmittel-Seiten wird **nicht** angefasst. Deren Markup war immer schon für echtes Bootstrap geschrieben.

---

## 2. Reihenfolge (CLAUDE.md-Orchester-Workflow)

Branch `fix/ui-layout-shell`, abzweigen von `master`.

| # | Schritt | Agent |
|---|---|---|
| 0 | Plan im Repo ablegen | — |
| 1 | Bootstrap-Dateien beschaffen | — |
| 2 | Vorbedingung prüfen: Läuft die bUnit-Suite? | — |
| 3 | Tests schreiben | Test-Agent |
| 4 | Review der Tests (max. 3 Schleifen) | Review-Agent |
| 5 | Code umsetzen | Dev-Agent |
| 6 | Review des Codes (max. 3 Schleifen) | Review-Agent |
| 7 | Doku (4-Teil-Check) | Doc-Agent |
| 8 | Review der Doku (max. 3 Schleifen) | Review-Agent |
| 9 | Visuelle Verifikation im Browser | — |
| 10 | Merge Request | Orchestrator |

Bei jedem Review gilt: Nach der 3. erfolglosen Schleife an den User eskalieren, nicht weitermachen.

---

## 3. Schritt 0 — Diesen Plan im Repo ablegen

Kopiere diese Plandatei nach:

```
C:\Users\bened\source\repos\FoodDatabase\requirements\ui-shell-layout-plan.md
```

---

## 4. Schritt 1 — Bootstrap 5.3.3 beschaffen

Lade genau zwei Dateien nach `src/App/wwwroot/bootstrap/`:

| Zieldatei | Quelle |
|---|---|
| `bootstrap.min.css` | `https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css` |
| `bootstrap.bundle.min.js` | `https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js` |

`bootstrap.min.css` **überschreibt** den bisherigen Stub. Das ist beabsichtigt.

Das Projektprinzip „keine Internet-Calls" aus `CLAUDE.md` betrifft die **Laufzeit** der App. Ein einmaliger Download von Build-Assets ist davon nicht berührt; nach dem Download läuft die App vollständig offline. Beide Dateien werden eingecheckt (`.gitignore` schließt `wwwroot/` nicht aus).

**Abbruchkriterium (unbedingt prüfen):** Beide Dateien müssen deutlich größer sein als der 4-KB-Stub (CSS ≈ 230 KB, JS ≈ 80 KB), und `bootstrap.min.css` muss die Zeichenkette `offcanvas-start` enthalten. Wenn nicht, wurde eine Fehlerseite gespeichert → **abbrechen und melden**, nicht weiterarbeiten.

Falls der Download aus der Agent-Umgebung blockiert ist: den User bitten, ihn selbst auszuführen. Nicht versuchen, Bootstrap von Hand nachzubauen.

---

## 5. Schritt 2 — Vorbedingung: Läuft die bUnit-Suite überhaupt?

`CONTEXT_SUMMARY.md` (Session 21) vermerkt, dass für die bUnit-Tests unter `src/Tests/Ui/` die „MSBuild-DI-Integration aussteht". Es ist also unklar, ob diese Tests aktuell ausgeführt werden.

Führe zuerst aus:

```bash
dotnet test src/FoodDatabase.sln --filter FullyQualifiedName~Ui
```

- **Tests laufen und sind grün** → weiter mit Schritt 3.
- **Tests laufen nicht / kompilieren nicht** → Das ist ein **Blocker**. Zuerst die Suite lauffähig machen, dann Schritt 3. Neue Tests in eine tote Suite zu schreiben ist sinnlos.

---

## 6. Schritt 3 — Test-Agent

### Wichtige Einschränkung, die du verstehen musst

bUnit rendert **nur HTML-Markup**. Es lädt keine Stylesheets, führt kein JavaScript aus und berechnet keine Element-Positionen.

Daraus folgt:
- Der Layout-Bug (`flex-direction`) ist **nicht** per bUnit testbar.
- Die Slide-Animation des Menüs ist **nicht** per bUnit testbar.

**Schreibe keinen Test, der behauptet, Scroll-Position, Sichtbarkeit oder Animation zu prüfen.** Solche Tests wären wertlos und irreführend. Die Tests sichern ausschließlich die **Markup-Struktur** und die **Attribut-Verdrahtung**, von der das Bootstrap-JS abhängt. Die visuelle Korrektheit wird in Schritt 9 im Browser geprüft.

### Datei: `src/Tests/Ui/NavMenuTests.cs` (neu)

Diese Tests schreiben:

1. `RendersAllSevenNavLinks` — Es werden genau 7 Links gerendert, mit den `href`-Werten `/`, `/lebensmittel`, `/lagerbestand`, `/lagerorte`, `/rezepte`, `/warnungen`, `/einkaufsliste`.
2. `NavElementHasExpandMdClass` — Das `<nav>`-Element trägt die Klasse `navbar-expand-md`.
3. `TogglerTargetsOffcanvasPanel` — Der Toggler-Button trägt `data-bs-toggle="offcanvas"` und `data-bs-target="#navbarOffcanvas"`, und es existiert ein Element mit `id="navbarOffcanvas"`. *(Begründung: Ein Tippfehler in dieser Kopplung ist der wahrscheinlichste Fehler und bleibt im Browser völlig stumm — kein Fehler in der Konsole, der Button tut einfach nichts.)*
4. `PanelSlidesFromLeft` — Das Panel trägt **beide** Klassen `offcanvas` und `offcanvas-start`. *(Begründung: `offcanvas-end` würde von rechts einfahren. Die Slide-Richtung von links ist ausdrückliche Anforderung.)*
5. `EveryNavLinkDismissesOffcanvas` — **Jeder** der 7 Links trägt `data-bs-dismiss="offcanvas"`. Implementiere das als Schleife über alle gefundenen Links, nicht als 7 einzelne Asserts, damit ein später ergänzter achter Link nicht unbemerkt durchrutscht.
6. `HomeLinkUsesExactMatch` — Der Link auf `/` verwendet `Match="NavLinkMatch.All"`. *(Begründung: Sonst wäre „Start" auf jeder Unterseite als aktiv markiert, weil jeder Pfad mit `/` beginnt.)*

### Datei: `src/Tests/Ui/MainLayoutTests.cs` (neu)

1. `RendersSidebarAndMain` — Genau ein Element mit Klasse `sidebar` und genau ein `<main>`-Element.
2. `BodyIsRenderedInsideContentArticle` — Ein übergebenes `RenderFragment` landet innerhalb von `main > article.content`.
3. `DoesNotContainMicrosoftTemplateLink` — Das gerenderte Markup enthält **nicht** die Zeichenkette `learn.microsoft.com`. *(Regressionstest gegen den Rest aus dem Projekt-Template.)*

### Nicht anfassen

Die bestehenden Dateien `src/Tests/Ui/LebensmittelFormTests.cs`, `LebensmittelListeTests.cs`, `LebensmittelDetailTests.cs` (zusammen 15 Tests) bleiben unverändert. Sie müssen weiterhin kompilieren.

**Abbruchkriterium:** Die neuen Tests sind zu diesem Zeitpunkt **rot** (TDD-Rot-Phase). Das ist korrekt und erwartet. Nicht versuchen, sie grün zu machen.

---

## 7. Schritt 5 — Dev-Agent

Vier Dateien ändern. Alle Inhalte sind vollständig angegeben.

### 7.1 `src/App/wwwroot/app.css` — vollständig ersetzen durch:

```css
html, body {
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

a, .btn-link {
    color: #0071c3;
}

.btn-primary {
    color: #fff;
    background-color: #1b6ec2;
    border-color: #1861ac;
}

    .btn-primary:hover {
        color: #fff;
        background-color: #1c7ed4;
        border-color: #1861ac;
    }

.page {
    position: relative;
    display: flex;
    flex-direction: column;
}

main {
    flex: 1;
}

.sidebar {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}

/* Unter 768px ist das Offcanvas-Panel position:fixed und liegt damit ausserhalb
   der .sidebar-Flaeche. Ohne eigenen Hintergrund waere es durchsichtig. */
#navbarOffcanvas {
    background-image: linear-gradient(180deg, rgb(5, 39, 103) 0%, #3a0647 70%);
}

.content {
    padding-top: 1.1rem;
}

.navbar-toggler {
    background-color: rgba(255, 255, 255, 0.1);
}

@media (min-width: 768px) {
    .page {
        flex-direction: row;
    }

    .sidebar {
        width: 250px;
        height: 100vh;
        position: sticky;
        top: 0;
    }

    article {
        padding-left: 2rem !important;
        padding-right: 1.5rem !important;
    }
}
```

Änderungen gegenüber vorher, damit du sie beim Review wiedererkennst:
- `page` → `.page` (der eigentliche Bug-Fix)
- Alle `.top-row`-Regeln entfernt (die Leiste entfällt komplett)
- `.nav-scrollable` entfernt (wird nach dem Umbau nirgends mehr referenziert)
- `#navbarOffcanvas`-Hintergrund neu hinzugefügt

### 7.2 `src/App/Components/Layout/MainLayout.razor` — vollständig ersetzen durch:

```razor
@inherits LayoutComponentBase

<div class="page">
    <div class="sidebar">
        <NavMenu />
    </div>

    <main>
        <article class="content px-4">
            @Body
        </article>
    </main>
</div>
```

Die `top-row` samt Link auf `learn.microsoft.com` entfällt ersatzlos. Der Markenname steht bereits in der Sidebar. Dadurch beginnt `article.content` unmittelbar oben.

### 7.3 `src/App/Components/Layout/NavMenu.razor` — vollständig ersetzen durch:

```razor
@namespace FoodDatabase.App.Components.Layout

<nav class="navbar navbar-expand-md p-0 align-items-stretch flex-md-column" data-bs-theme="dark">
    <div class="d-flex w-100 align-items-center justify-content-between px-3 py-2">
        <a class="navbar-brand" href="/">FoodDatabase</a>
        <button class="navbar-toggler" type="button"
                data-bs-toggle="offcanvas" data-bs-target="#navbarOffcanvas"
                aria-controls="navbarOffcanvas" aria-label="Navigation öffnen">
            <span class="navbar-toggler-icon"></span>
        </button>
    </div>

    <div class="offcanvas offcanvas-start w-100" tabindex="-1"
         id="navbarOffcanvas" aria-labelledby="navbarOffcanvasLabel">

        @* Nur mobil sichtbar: ab 768px uebernimmt der navbar-brand oben diese Rolle. *@
        <div class="offcanvas-header d-md-none">
            <h5 class="offcanvas-title" id="navbarOffcanvasLabel">Navigation</h5>
            <button type="button" class="btn-close" data-bs-dismiss="offcanvas" aria-label="Schließen"></button>
        </div>

        <div class="offcanvas-body p-0">
            @* data-bs-dismiss auf jedem Link: Blazor navigiert ohne Seiten-Reload,
               sonst bliebe das Panel ueber der neuen Seite offen stehen. *@
            <ul class="navbar-nav flex-column w-100 px-3">
                <li class="nav-item">
                    <NavLink class="nav-link" href="/" Match="NavLinkMatch.All" data-bs-dismiss="offcanvas">Start</NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="/lebensmittel" data-bs-dismiss="offcanvas">Lebensmittel</NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="/lagerbestand" data-bs-dismiss="offcanvas">Lagerbestand</NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="/lagerorte" data-bs-dismiss="offcanvas">Lagerorte</NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="/rezepte" data-bs-dismiss="offcanvas">Rezepte</NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="/warnungen" data-bs-dismiss="offcanvas">Warnungen</NavLink>
                </li>
                <li class="nav-item">
                    <NavLink class="nav-link" href="/einkaufsliste" data-bs-dismiss="offcanvas">Einkaufsliste</NavLink>
                </li>
            </ul>
        </div>
    </div>
</nav>
```

**Warum dieses Markup so aussieht** (damit du beim Review nichts „vereinfachst"):

- `navbar-expand-md` + `offcanvas offcanvas-start` ist Bootstraps offizielles Muster für eine responsive Off-Canvas-Navigation. Unter 768 px ist das Panel `position: fixed` und liegt links außerhalb des Bildschirms; der Toggler slidet es herein. Ab 768 px hebt Bootstrap es automatisch in den statischen Fluss zurück (`position: static`, dauerhaft sichtbar) und blendet den Toggler aus.
- Slide-Animation, abgedunkelter Backdrop, Schließen per Klick daneben, Schließen per `ESC`, Fokus-Falle und die Beachtung der OS-Einstellung „Bewegung reduzieren" liefert `bootstrap.bundle.min.js` fertig mit. Deshalb **kein** eigenes CSS-`transform`, **kein** JS-Interop, **keine** `IJSRuntime`-Injektion.
- `data-bs-theme="dark"` statt des alten `navbar-dark bg-dark`: In Bootstrap 5.3 ist `navbar-dark` veraltet. `data-bs-theme` setzt die CSS-Variablen für alle Nachfahren, färbt also auch `navbar-toggler-icon` und `btn-close` korrekt weiß. Deshalb braucht `btn-close` hier **kein** `btn-close-white`.
- Kein `bg-dark` auf dem `<nav>`, damit der Farbverlauf aus `app.css` sichtbar bleibt.

**Falls beim visuellen Test etwas nicht stimmt:**
- Panel mobil zu breit → `w-100` am Offcanvas entfernen und stattdessen in `app.css` die Variable `--bs-offcanvas-width` auf `250px` setzen.
- Links ab 768 px nebeneinander statt untereinander → `flex-column` am `<ul>` durch `flex-md-column flex-column` ersetzen.
- `btn-close` unsichtbar (schwarz auf dunkel) → `btn-close-white` ergänzen.

### 7.4 `src/App/Components/App.razor` — nur diese eine Zeile ergänzen

Suche:

```razor
    <script src="_framework/blazor.web.js"></script>
```

Ersetze durch:

```razor
    <script src="bootstrap/bootstrap.bundle.min.js"></script>
    <script src="_framework/blazor.web.js"></script>
```

Reihenfolge ist wichtig: Bootstrap muss vor `blazor.web.js` geladen sein, damit die Komponenten beim ersten interaktiven Render bereitstehen. Sonst nichts an dieser Datei ändern.

### 7.5 `src/App/Components/_Imports.razor` — prüfen

`NavLink` und `NavLinkMatch` liegen in `Microsoft.AspNetCore.Components.Routing`. Prüfe, ob die Datei bereits `@using Microsoft.AspNetCore.Components.Routing` enthält. Falls nein: ergänzen. Falls ja: nichts tun.

**Abbruchkriterium für Schritt 5:** `dotnet build src/FoodDatabase.sln` läuft fehlerfrei durch, und `dotnet test src/FoodDatabase.sln` meldet **alle** Tests grün (271 bestehende + die neuen aus Schritt 3).

---

## 8. Schritt 7 — Doc-Agent (4-Teil-Check, alle vier verpflichtend)

1. **Code-Dokumentation** → `reviews/ui-layout-documentation-validation.md`
   Inline-Kommentare **nur** dort, wo die Logik nicht selbsterklärend ist. Der `.page`-Fix bekommt **keinen** Kommentar (offensichtlich). Kommentiert werden genau drei Stellen, die bereits im Code oben stehen: der `#navbarOffcanvas`-Hintergrund, das `d-md-none` am Offcanvas-Header, und das `data-bs-dismiss` an den Links.
2. **Feature-HTML** → neu: `docs/features/UI-Shell-Layout.html`, nach Vorlage `docs/features/TEMPLATE.html`.
   Inhalt: Aufbau der Shell, responsives Off-Canvas-Verhalten mit Breakpoint 768 px, Bootstrap-Version 5.3.3, Ablösung des Stubs. Status-Badge auf `done`.
3. **`docs/architecture-overview.html`** → Projekt-Status um Bootstrap-5.3.3-Einbindung und responsive UI-Shell ergänzen.
4. **Diagramme** → `diagrams/architecture-components.drawio`: Blazor-Shell ergänzen (`App` → `Routes` → `MainLayout` → `NavMenu` + `@Body`) sowie das lokale Bootstrap-Asset.

Danach den Konsistenz-Check aus `CLAUDE.md` durchlaufen: Sagen alle vier Aspekte dasselbe?

---

## 9. Schritt 9 — Verifikation

### Automatisiert

```bash
dotnet build src/FoodDatabase.sln
dotnet test src/FoodDatabase.sln
```

Erwartung: alle Tests grün.

### Visuell — das ist der eigentliche Nachweis

bUnit kann den CSS-Bug und die Animation nicht prüfen (siehe Schritt 3). Diese Liste muss ein Mensch oder ein Browser-Tool abarbeiten:

```bash
dotnet run --project src/App
```

Dann `http://localhost:5099/` öffnen.

**Desktop (Fenster ≥ 768 px):**
- [ ] Überschrift „FoodDatabase" steht **ohne Scrollen** oben rechts neben der Sidebar
- [ ] Startseite hat **keinen** vertikalen Scrollbalken
- [ ] Sidebar links, volle Höhe, dunkler Farbverlauf, alle 7 Links untereinander sichtbar
- [ ] Kein Menü-Button sichtbar
- [ ] Aktiver Menüpunkt ist hervorgehoben; „Start" ist **nur** auf `/` aktiv, nicht auf Unterseiten

**Mobil (Fenster < 768 px oder DevTools-Geräteemulation):**
- [ ] Menü-Button oben links sichtbar, die 7 Links zunächst verborgen
- [ ] Klick auf den Button: Panel **slidet von links nach rechts herein** — nicht: erscheint schlagartig; nicht: klappt nach unten auf
- [ ] Abgedunkelter Backdrop über dem Inhalt; Klick darauf schließt das Panel
- [ ] `ESC` schließt das Panel
- [ ] **Klick auf einen Nav-Link navigiert und schließt das Panel.** Das ist der kritische Blazor-Fall — ohne `data-bs-dismiss` bliebe es offen stehen.
- [ ] Fenster über 768 px vergrößern: Panel wird wieder zur statischen Sidebar, Button verschwindet, kein Backdrop-Rest bleibt in der Seite hängen
- [ ] Mit aktivierter OS-Einstellung „Bewegung reduzieren": Panel erscheint ohne Slide-Animation

**Bestehende Seiten (Bootstrap-Styling wird erstmals wirksam):**
- [ ] `/lebensmittel`: Tabelle mit Zebrastreifen, Buttons farbig
- [ ] `/lebensmittel/neu`: Eingabefelder gerahmt, Labels korrekt gesetzt
- [ ] `/lebensmittel`: Löschen-Button öffnet zentrierten Modal-Dialog mit abgedunkeltem Hintergrund
- [ ] Browser-Konsole zeigt keinen 404 auf `bootstrap.bundle.min.js`

### Erwartete, unkritische Abweichung

Echtes Bootstrap bringt einen vollständigen CSS-Reboot mit, der Stub nicht (Beispiel: `h1` ist dort `2.5rem` statt `2rem`). Leicht veränderte Abstände und Schriftgrößen auf bestehenden Seiten sind **erwartet und gewollt** — die Seiten wurden für echtes Bootstrap geschrieben. Das ist kein Regressionsfehler.

---

## 10. Schritt 10 — Merge Request

- MR-Template aus `CLAUDE.md` verwenden
- `--base master` (im Repo existiert **kein** `develop`-Branch, anders als in `CLAUDE.md` beschrieben)
- Titel: `fix/ui-layout-shell`
- Danach `CONTEXT_SUMMARY.md` um einen Session-Eintrag ergänzen und Memory `project_status.md` aktualisieren

---

## 11. Alle betroffenen Dateien auf einen Blick

| Datei | Was passiert |
|---|---|
| `requirements/ui-shell-layout-plan.md` | neu (dieser Plan) |
| `src/App/wwwroot/bootstrap/bootstrap.min.css` | Stub → echtes Bootstrap 5.3.3 |
| `src/App/wwwroot/bootstrap/bootstrap.bundle.min.js` | neu |
| `src/App/wwwroot/app.css` | `.page`-Fix, `.top-row` raus, Offcanvas-Hintergrund rein |
| `src/App/Components/Layout/MainLayout.razor` | `top-row` entfernt |
| `src/App/Components/Layout/NavMenu.razor` | Off-Canvas-Navbar, `NavLink` |
| `src/App/Components/App.razor` | Bundle-JS eingebunden |
| `src/App/Components/_Imports.razor` | ggf. `@using Microsoft.AspNetCore.Components.Routing` |
| `src/Tests/Ui/NavMenuTests.cs` | neu |
| `src/Tests/Ui/MainLayoutTests.cs` | neu |
| `docs/features/UI-Shell-Layout.html` | neu |
| `docs/architecture-overview.html` | Status ergänzt |
| `diagrams/architecture-components.drawio` | Blazor-Shell ergänzt |
| `reviews/ui-layout-*.md` | Review- und Validierungsberichte |
