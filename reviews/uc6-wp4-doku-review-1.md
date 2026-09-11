# UC6 (WP4) Dokumentation Review – Versuch 1/3

**Datum**: 2026-09-11
**Phase**: Doc-Agent Phase Review (nachgeholt)
**Modus**: `docs`
**Reviewer**: Review-Agent
**Branch**: `docs/uc6-definition-of-done` (Commit `1758dbe` über `master` `95d5c30`)

---

## VERDICT: CHANGES REQUIRED
## ATTEMPT: 1 von 3

**Befunde**: 6 x MEDIUM (freigabe-blockierend) · 3 x LOW (nicht blockierend)
**Testlauf**: `dotnet test` -> **361 bestanden, 0 Fehler** (selbst ausgeführt, nicht aus dem Bericht übernommen)
**XML-Validität**: beide geänderten `.drawio`-Dateien parsen sauber, keine doppelten `id`

Dieses Review holt ein Gate nach, das bei UC6 übersprungen wurde: UC6 wurde am 08.08.2026 mit
PR #4 (`cd1764e`) nach `master` gemergt, ohne dass je ein Doku-Review stattfand. Die Doku ist
inhaltlich in weiten Teilen stark — die Code-Doku ist vorbildlich und das Sequence-Diagramm hat
mit `1758dbe` die fehlende UI-Ebene bekommen. Blockierend sind sechs Stellen, an denen die vier
Doku-Teile einander oder der Realität widersprechen. Genau diese Widersprüche sind der Grund,
warum es das Gate gibt.

---

## 4-Teil-Dokumentation: Prüfung im Einzelnen

### 1. Code-Dokumentation — bestanden, mit einem veralteten Validierungsreport

**`src/App/Components/Pages/Lager/VerbrauchListe.razor`** — vollständig dokumentiert.

Die beiden geforderten Nicht-Offensichtlichkeiten sind beide als Absicht kommentiert:

- **Nicht-Reset von `ergebnis`** (Zeilen 121-122):
  `// ergebnis wird hier bewusst NICHT zurückgesetzt: VerbrauchtAusbuchung ruft diese`
  `// Methode zum Neuladen auf, nachdem es die ServiceResult-Meldung gesetzt hat.`
  Der Kommentar sitzt an der richtigen Stelle — direkt bei `fehler = "";`, wo ein Leser den
  fehlenden Reset vermuten würde. Er nennt das WARUM, nicht das WAS. Verifiziert gegen
  `VerbrauchtAusbuchung()` (Zeilen 161-176): dort wird `ergebnis` gesetzt (Z. 168) und erst
  danach `AlleAbrufen()` gerufen (Z. 170) — der Kommentar beschreibt den tatsächlichen Ablauf.
  Abgesichert durch `Sollte_Erfolgs_Alert_Anzeigen_Bei_Verbrauch_Erfolg` und
  `Sollte_Fehler_Alert_Anzeigen_Bei_Verbrauch_Fehler` (`VerbrauchListeTests.cs:201, 246`), die
  beide nach dem Klick — also nach dem Neuladen — auf `alert-ergebnis` prüfen.
- **N+1-Service-Calls** (Zeile 115, im `<summary>` von `AlleAbrufen()`):
  `N+1 Aufrufe sind bewusst so gewollt (KISS, nur 3 Nutzer max gleichzeitig).`
  Auftragsgemäß **nicht** als Mangel gewertet — geprüft war nur, ob die Entscheidung
  kommentiert ist. Sie ist es, inklusive der Zielgröße als Begründung.

**`VerbrauchZeile.cs`** — alle 5 Properties mit `<summary>`, Zweck und Beispielwerten. KISS
eingehalten (reines View-Model, keine Logik). Keine Beanstandung.

**`reviews/uc6-wp4-documentation-validation.md`** — inhaltlich sorgfältig, aber **nicht mehr
aktuell**. Siehe Mangel 4.

### 2. Feature-HTML `docs/features/UC6-VerbrauchAusbuchen.html` — drei Mängel

| Kriterium | Befund |
|---|---|
| Status-Badge | OK `<span class="status-badge complete">FERTIG</span>` (Z. 20) |
| UI-Sektion mit `VerbrauchListe.razor` | OK eigene Sektion (Z. 140-238): Route, DI-Abhängigkeiten, alle 5 Komponenten-Felder, Happy Path, Fehlerfall, Edge Cases, View-Model, NavMenu-Update. Gegen den Code geprüft — stimmt durchweg |
| Alle 10 bUnit-Tests gelistet | FEHLER: die 10 UI-Tests stehen korrekt in Z. 264-275, direkt danach folgt ein zweiter Block mit den **Service**-Testnamen unter der UI-Überschrift -> **Mangel 1** |
| Link aufs Sequence-Diagramm | FEHLT -> **Mangel 2** |
| Implementation-Status | WIDERSPRUCH zum Badge und zum Merge-Stand -> **Mangel 3** |

Die CSS-Referenzen (`../css/shared.css`, `../css/uc6-verbrauchausbuchen.css`) und der
Rücklink auf die Overview wurden geprüft und stimmen.

### 3. Architecture-Overview `docs/architecture-overview.html` — UC6 korrekt, ein Fremdwiderspruch

**UC6-Karte** (Z. 160-165) ist korrekt und aussagekräftig:
`class="uc-card done"`, Badge `Fertig (Service + UI komplett)`, Beschreibung
`Service (10 Tests) + UI VerbrauchListe.razor (10 bUnit-Tests)`, Link
`features/UC6-VerbrauchAusbuchen.html` — Datei existiert unter exakt dieser Schreibweise
(die case-only-Drift wurde mit `96546da` behoben).

**Testzahl 361**: nachgerechnet, nicht nur gelesen.
`dotnet test` liefert 361. Z. 340 sagt `361/361 (Service-Layer: 271 | UI-Layer: 90)`.
Die 90 UI-Tests habe ich über alle Dateien in `src/Tests/Ui/` einzeln ausgezählt: 5 + 3 + 5 +
15 + 16 + 20 + 3 + 6 + 7 + 10 = **90**, damit 271 Service. UC6 steuert 10 Service + 10 UI
bei — beide Zahlen stimmen mit `VerbrauchAusbuchangServiceTests.cs` (10 `[Fact]`) und
`VerbrauchListeTests.cs` (10 `[Fact]`) überein. `CONTEXT_SUMMARY.md:52` nennt ebenfalls 361.
**Für UC6 ist die Zahl überall konsistent.** Einzige Reibung: Z. 343 nennt den historischen
Stand `351/351` unmittelbar unter Z. 340 -> **Mangel 7 (LOW)**.

Widerspruch bei den **UC9**-Zahlen zwischen Overview und der neuen Legende -> **Mangel 6**
(im Rahmen von Punkt A).

### 4. Diagramme — Sequence-Diagramm deutlich besser, ein Bruch bleibt

**`diagrams/sequence-uc6-verbrauchausbuchangung.drawio`** (geändert in `1758dbe`),
Abgleich gegen `VerbrauchListe.razor` als Quelle der Wahrheit:

| Diagramm-Element | Realität im Code | Urteil |
|---|---|---|
| `s1-ui`: `VerbrauchListe.razor / /verbrauchen` | `@page "/verbrauchen"` (Z. 1) | OK |
| `s1-msg0`: `Klick "Verbrauchen" (btn-verbrauchen-{id})` | `data-testid="btn-verbrauchen-@zeile.Id"` (Z. 62), Label "Verbrauchen" (Z. 63) | OK |
| `s1-msg1`: `VerbauchtProduktAsync(lebensmittelId)` | Z. 167 | OK (inkl. des Tippfehlers im echten Methodennamen) |
| `s1-msg6`: `ServiceResult(true, ...)` | Z. 167-169 | OK |
| `s1-msg7`: `alert-success anzeigen + AlleAbrufen() lädt Tabelle neu` | Z. 30 + Z. 170 | OK — bildet genau den Punkt ab, der den Nicht-Reset von `ergebnis` nötig macht |
| `s2-msg4/5`: `ServiceResult(false, ...)` -> `alert-warning` | Z. 30 (`ergebnisMeldungErfolgreich ? ... : "alert-warning"`) | OK |
| Auslösender Klick in **Szenario 2** | fehlt komplett | FEHLER -> **Mangel 5** |

Legende (`legend`, Z. 168) nennt Komponente, Route, Trigger-Methode, beide Alert-Varianten und
das Neuladen via `AlleAbrufen()`; der ursprüngliche FIFO-Satz ist erhalten.
XML valide, 29 Zellen, keine doppelte `id`.

**`requirements/use-cases.drawio`**: UC6-Ellipse (Z. 45) grün `#d4edda` mit Haken — korrekt.
Legende siehe Punkt B, geprüft und in Ordnung.

---

## Punkt A — Scope Creep beim UC9-Status

**Bewertung: bestätigt. Severity LOW, Prozess-Befund, kein Rollback, nicht freigabe-blockierend.**

Faktenlage selbst verifiziert:

- `src/App/Components/Pages/Lager/LagerortListe.razor` (`@page "/lagerorte"`) und
  `LagerortForm.razor` (`@page "/lagerorte/neu"`) existieren.
- `src/Tests/Ui/LagerortListeTests.cs` (5 `[Fact]`) + `src/Tests/Ui/LagerortFormTests.cs`
  (3 `[Fact]`) = **8 bUnit-Tests**, sämtlich Teil der 361 grünen Tests.
- Die Aussage "WP4 UC9 fertig" ist damit **inhaltlich richtig**. Die Einschränkung ist sogar
  ehrlich benannt: die Karte sagt "List + Neu-Form", also kein Update/Delete.

Prozessual bleibt es ein Fremdeingriff: `git show d6676de -- docs/architecture-overview.html`
zeigt, dass der UC6-Commit die Zeilen 337-338 und 356 zum **UC9**-Status geschrieben hat,
obwohl UC9 nicht Auftrag war. Die eigentliche UC9-Karte (Z. 181-186) stammt dagegen aus
`23eb5fc` (30.07.2026, UC9-UI-Commit) — der Scope Creep in `d6676de` betrifft also die
Projekt-Status-Listen, nicht die Karte.

Warum das zählt, obwohl die Aussage stimmt: Eine fremde Statusänderung in einem UC6-Commit
bekommt kein eigenes Review. Für die UC9-UI existiert bis heute kein WP4-Reviewreport — unter
`reviews/` liegen nur `uc9-doku-review-1.md` und `uc9-test-review-1.md`, beide aus der
Service-Phase (25.06.2026). Der Status wurde also über einen Umweg auf "fertig" gesetzt, den
das Qualitätsgate nie gesehen hat. Sichtbarer Schaden daraus: die falsche Testzahl in
**Mangel 6**, die genau so unbemerkt durchgerutscht ist.

**Empfehlung**: nicht zurückrollen. Stattdessen bei der nächsten UC9-Berührung ein
`reviews/uc9-wp4-*.md` nachziehen — das ist eine eigene Aufgabe, nicht Teil von UC6.

---

## Punkt B — Legende in `requirements/use-cases.drawio`

**Bewertung: Korrektur verifiziert, korrekt. Kein offener Mangel.**

`requirements/use-cases.drawio:112` lautet jetzt:

```
Status: Grün = Service + UI Complete (UC1: 19 Service + 51 UI | UC2: 29 Service + 12 UI |
UC6: 10 Service + 10 UI | UC9: 24 Service + 8 UI | UC3/UC10: Service only)
```

Gegengezählt, nicht übernommen:

| Angabe | Selbst gezählt | Quelle |
|---|---|---|
| UC6: 10 Service | **10** `[Fact]` | `src/Tests/Unit/Services/VerbrauchAusbuchangServiceTests.cs` |
| UC6: 10 UI | **10** `[Fact]` | `src/Tests/Ui/VerbrauchListeTests.cs` |
| UC9: 24 Service | **24** `[Fact]`, 0 `[Theory]`/`InlineData` | `src/Tests/Unit/Services/LagerortServiceTests.cs` |
| UC9: 8 UI | **3 + 5 = 8** `[Fact]` | `LagerortFormTests.cs`, `LagerortListeTests.cs` |

Alle vier Zahlen stimmen. Die sachlich falsche Aussage "UC3/UC6/UC9/UC10: Service only" ist
verschwunden, UC3 und UC10 bleiben korrekt als "Service only" stehen.

Ein Nebenbefund fällt dabei an: die 24 für UC9 sind seit dem allerersten Commit der Datei
korrekt (`git show 0df33ea:.../LagerortServiceTests.cs` -> 24 `[Fact]`, trotz der
Commit-Message "18 tests"). Die Zahl **18** in `docs/architecture-overview.html` war also nie
richtig und widerspricht jetzt der frisch korrigierten Legende -> **Mangel 6**.

---

## MÄNGEL (freigabe-blockierend)

### Mangel 1 — Service-Tests als UI-Tests ausgegeben · MEDIUM
**Datei**: `docs/features/UC6-VerbrauchAusbuchen.html`, Zeilen **278-291**
**Falsch**: Unter der Überschrift "UI-Tests (bUnit)" folgt nach der korrekten 10er-Liste
(Z. 264-275) und dem Verweis auf `VerbrauchListeTests.cs` ein **zweiter**
Checklisten-Block, der wortgleich die 10 **Service**-Testnamen aus Z. 247-258 wiederholt
("Happy Path: Verbrauch erfolgreich", "Fehler: LebensmittelId = 0", "FIFO: Korrekte Sortierung
nach Verfallsdatum" ...). Diese Namen existieren in `src/Tests/Ui/VerbrauchListeTests.cs` nicht.
**Konsequenz**: Der Leser zählt unter "UI-Tests" 20 Einträge und findet die Hälfte davon im
Code nicht wieder. Die Seite widerspricht damit ihrer eigenen Aussage "20/20 (10 Service +
10 UI)" in Z. 243 und Z. 294.
**Soll**: Den kompletten `<div class="checklist">`-Block Z. 278-291 ersatzlos entfernen. Die
Sektion endet dann korrekt mit dem Satz zur UI-Test-Datei in Z. 277.

### Mangel 2 — Kein Link auf das Sequence-Diagramm · MEDIUM
**Datei**: `docs/features/UC6-VerbrauchAusbuchen.html`
**Falsch**: Die Seite verweist nirgends auf ein Diagramm. Die einzige Erwähnung ist der
Checklisten-Eintrag Z. 328 "Diagramme aktualisiert – Sequence-Diagramm
(uc6-verbrauchausbuchangung.drawio)" — bloßer Dateiname ohne Pfad und ohne Link. Eine
Diagramm-Sektion fehlt ganz, anders als bei UC9 (`UC9-Lagerorte.html:432-434`), UC10
(`UC10-Produktinstanzen.html:349-369`) und UC4 (`UC4-Rezepte.html:713-716`).
**Konsequenz**: Teil 2 des Doc-Checks verlangt aktuelle Diagramm-Links. Die gerade erst
erweiterte UI-Ebene im Sequence-Diagramm ist von der Feature-Seite aus nicht auffindbar.
**Soll**: Eine Sektion "Diagramme" nach der Tests-Sektion einfügen, nach dem Muster von
`UC9-Lagerorte.html:432-434`, mit mindestens:
- Sequence-Diagramm: `<a href="../../diagrams/sequence-uc6-verbrauchausbuchangung.drawio"><code>diagrams/sequence-uc6-verbrauchausbuchangung.drawio</code></a>`
  — mit einem Satz, dass es seit `1758dbe` die UI-Ebene (Klick -> `VerbrauchtAusbuchung()` -> Alert) enthält
- Use-Case-Diagramm: `<a href="../../requirements/use-cases.drawio"><code>requirements/use-cases.drawio</code></a>`
  (UC6 grün, Legende "10 Service + 10 UI")

### Mangel 3 — Implementation-Status widerspricht dem Badge und dem Merge-Stand · MEDIUM
**Datei**: `docs/features/UC6-VerbrauchAusbuchen.html`, Zeilen **330, 331, 343**
**Falsch**:
- Z. 330 `<input type="checkbox"> User Review & Approval – Pending`
- Z. 331 `<input type="checkbox"> Merged zu Master – Pending`
- Z. 343 `<strong>Status</strong>: <strong>READY FOR USER REVIEW</strong>`

UC6 **ist** gemergt: PR #4, Merge-Commit `cd1764e`, 08.08.2026. Der Header derselben Seite
sagt in Z. 20 "FERTIG", die Architecture-Overview sagt "Fertig (Service + UI komplett)",
`CONTEXT_SUMMARY.md:38` sagt fertig.
**Konsequenz**: Exakt der Fehler, den der Konsistenz-Check aus `CLAUDE.md` abfangen soll —
"Code sagt fertig, aber irgendeine Stelle sagt todo". Drei Stellen auf einer Seite.
**Soll**: Beide Checkboxen auf `checked` setzen und konkretisieren, z. B.
`<input type="checkbox" checked> User Review & Approval – PR #4` und
`<input type="checkbox" checked> Merged zu Master – 2026-08-08 (cd1764e)`.
Z. 343 auf `<strong>Status</strong>: <strong>GEMERGT (PR #4, 2026-08-08)</strong>` ändern.

### Mangel 4 — Validierungsreport beschreibt eine Lücke, die es nicht mehr gibt · MEDIUM
**Datei**: `reviews/uc6-wp4-documentation-validation.md`, Zeilen **300-307** und **323-324**
**Falsch**: Der Report sagt zum Sequence-Diagramm "Aktuell: Nur Service-Layer dokumentiert",
"Empfehlung: Sollte UI-Layer (VerbrauchListe.razor) hinzufügen", "**Status**: NICHT MODIFIZIERT
(XML-Struktur zu komplex, Risiko > Nutzen...)" und führt unter "Offene Punkte" auf:
"Sequence-Diagram sollte UI-Layer erweitert werden". Genau das ist mit `1758dbe` erledigt —
`VerbrauchListe.razor` und `/verbrauchen` stehen jetzt im Diagramm, samt Klick und beiden
Alert-Rückwegen. Zusätzlich behauptet Z. 301 "requirements/use-cases.drawio: UC6 bereits
korrekt" — zum Zeitpunkt des Reports nannte die Legende UC6 "Service only", war also falsch.
**Konsequenz**: Der Report ist die Quelle, aus der ein späterer Leser den Doku-Stand abliest.
Er dokumentiert eine offene Baustelle, die geschlossen ist, und einen Diagramm-Zustand, der
zweimal nicht stimmt. Teil 1 des Doc-Checks verlangt aktuelle Validierungsreports.
**Soll**:
- Z. 302-307: Status auf erledigt umstellen — UI-Ebene ergänzt in `1758dbe` (Aktor-Lifeline,
  `s1-msg0` Klick, `s1-msg7` / `s2-msg5` Alert-Rückwege, Legende), Ergebnis "vollständig".
- Z. 301: den damaligen Legenden-Fehler benennen und als in `1758dbe` behoben markieren
  (neu: `UC6: 10 Service + 10 UI`).
- Z. 323-324 "Offene Punkte": Eintrag entfernen bzw. durch "keine" ersetzen.
- Datum/Stand im Kopf (Z. 3) und im Fuß (Z. 329) auf den neuen Stand ziehen.

### Mangel 5 — Szenario 2 im Sequence-Diagramm hat keinen Auslöser · MEDIUM
**Datei**: `diagrams/sequence-uc6-verbrauchausbuchangung.drawio`, Zellen `s2-msg1` (Z. 126-131)
bis `s2-msg5` (Z. 160-165)
**Falsch**: Szenario 2 beginnt bei `s2-msg1` mit einem Pfeil von der UI (x=170) zum Service —
ohne vorangehenden Pfeil `Benutzer -> UI`. Der Rückweg `s2-msg5` (`alert-warning anzeigen`)
zeigt dagegen auf den Benutzer bei x=65. Es gibt also eine Antwort an einen Aktor, der in
diesem Szenario nie etwas getan hat.
**Konsequenz**: Verstößt gegen das Abnahmekriterium des eigenen Feature-Plans (Schritt 1,
"je ein Pfeil Benutzer->UI und UI->Benutzer in **beiden** Szenarien") und macht das Szenario
asymmetrisch zu Szenario 1: der Leser sieht nicht, dass auch der Fehlerfall durch denselben
Klick auf `btn-verbrauchen-{id}` ausgelöst wird.
**Soll**: Analog zu `s1-msg0` eine neue Zelle `s2-msg0` einfügen, y=570, Quelle x=65
(Benutzer) -> Ziel x=170 (UI), Text `Klick "Verbrauchen" (btn-verbrauchen-999)`, Style wie
`s1-msg0` (`edgeStyle=orthogonalEdgeStyle;...;fontSize=9;`, **ohne** `dashed=1`, da Hinweg).
Die übrigen Szenario-2-Zellen behalten ihre y-Werte (600/630/660/670/730/760), zwischen
Titel (y=540) und `s2-msg1` (y=600) ist Platz. Danach erneut auf XML-Validität und eindeutige
`id` prüfen.

### Mangel 6 — UC9-Testzahl in der Overview widerspricht der korrigierten Legende · MEDIUM
**Datei**: `docs/architecture-overview.html`, Zeilen **184** und **337**
**Falsch**:
- Z. 184: `Service (18 Tests) + UI (8 bUnit-Tests)` mit Badge `Service + UI Complete (26/26)`
- Z. 337: `UC9: Lagerorte (18 Service-Tests + Code + 4-Teil-Dokumentation)`

Tatsächlich enthält `src/Tests/Unit/Services/LagerortServiceTests.cs` **24** `[Fact]` (keine
`[Theory]`, keine `InlineData`) — und zwar seit dem ersten Commit `0df33ea`. Die in `1758dbe`
korrigierte Legende in `requirements/use-cases.drawio:112` sagt jetzt korrekt
`UC9: 24 Service + 8 UI`.
**Konsequenz**: Teil 3 und Teil 4 der Dokumentation widersprechen sich direkt — und zwar in
einer Zahl, die der UC6-Commit `d6676de` selbst in Z. 337 geschrieben hat (siehe Punkt A).
Dass die Legende nun richtig ist, macht den Widerspruch für jeden Leser sichtbar.
**Soll**: Z. 184 auf `Service (24 Tests) + UI (8 bUnit-Tests) — Dynamische Lagerorte mit List +
Neu-Form.` und Badge auf `Service + UI Complete (32/32)`. Z. 337 auf
`UC9: Lagerorte (24 Service-Tests + Code + 4-Teil-Dokumentation)`. Gesamtzahl 361 bleibt
unberührt — es geht um die Aufteilung, nicht um die Summe.

---

## Befunde ohne Freigabe-Wirkung

### Mangel 7 — `351/351` direkt unter `361/361` · LOW
**Datei**: `docs/architecture-overview.html`, Zeile **343**
Der Eintrag "UI-Fix Phase 2 FERTIG: @onclick Event-Handler Bug-Fix — **351/351** Tests GRÜN"
steht drei Zeilen unter der Gesamtangabe "**361/361**" (Z. 340). Beide stimmen für sich: 351
war der Stand vor den 10 UC6-UI-Tests. Nebeneinander gelesen wirkt es wie ein Widerspruch.
**Soll**: Z. 343 als historischen Stand kennzeichnen, z. B. "... — 351/351 Tests GRÜN (Stand
vor WP4 UC6)".

### Mangel 8 — Falscher Button-Name im Workflow · LOW
**Datei**: `docs/features/UC6-VerbrauchAusbuchen.html`, Zeile **90**
Der Service-Workflow sagt `3. Klickt "Ausbuchen"`. Der Button heißt in
`VerbrauchListe.razor:63` "Verbrauchen"; die UI-Sektion derselben Seite (Z. 194) schreibt es
korrekt.
**Soll**: "Ausbuchen" -> "Verbrauchen".

### Mangel 9 — Punkt A als Prozess-Befund · LOW
Siehe Abschnitt "Punkt A". Kein Rollback, keine Änderung im Rahmen von UC6. Festzuhalten ist:
der UC9-Status wurde in einem UC6-Commit gesetzt und hat deshalb nie ein eigenes Review
gesehen; ein `reviews/uc9-wp4-*.md` fehlt bis heute.

---

## HINWEISE (keine Mängel)

- **Sequence-Diagramm zeigt den Initial-Load nicht.** `AlleAbrufen()` ruft beim
  Komponenten-Start `ILebensmittelService.GetAllLebensmittelAsync()` sowie pro Lebensmittel
  `GetGesamtmengeAsync()` und `CheckMindestbestandUnterschrittenAsync()` auf. Im Diagramm
  taucht nur der Ausbuchungs-Pfad auf. Das ist legitim — das Diagramm heißt "Verbrauch
  Ausbuchen" — und die N+1-Charakteristik ist im Code und im Validierungsreport dokumentiert.
  Falls das Diagramm später die Ladephase zeigen soll, wäre das ein eigenes Szenario 0,
  kein Nachtrag in Szenario 1.
- **Szenario 2 hat keine eigenen Lifelines** (die aus Szenario 1 enden bei y=500, Szenario 2
  beginnt bei y=600). Die Pfeile schweben dort frei. Das ist der seit jeher bestehende Stil
  dieser Datei ("Same Participants (no need to repeat)", Z. 125) und war schon vor `1758dbe`
  so — kein Regressionsbefund, aber ein Kandidat für eine spätere Überarbeitung.
- **Commit-Liste der Feature-Seite** (Z. 336-342) führt Platzhalter statt Hashes
  ("[Service] feat(uc6): ..."). Die realen Commits sind `d6676de`, `96546da` und `1758dbe`.
  Kosmetisch, aber beim nächsten Anfassen der Seite leicht mitzunehmen.
- **`mxfile modified="2026-06-20"`** im Sequence-Diagramm wurde bei der Änderung nicht
  mitgezogen. draw.io setzt das Attribut beim nächsten Speichern selbst; nicht relevant.
- **Außerhalb dieses Reviews**: `docs/architecture-overview.html:218` führt die Entity
  `Nährwert` als "UC3 offen", während die UC3-Karte (Z. 140-144) "Fertig (gemergt)" sagt. Das
  betrifft UC3, nicht UC6, und gehört in ein eigenes Gate.

---

## Bewertung je Doku-Teil

| Teil | Gegenstand | Urteil |
|---|---|---|
| 1 | Code-Dokumentation (`VerbrauchListe.razor`, `VerbrauchZeile.cs`) | BESTANDEN |
| 1 | Validierungsreport `uc6-wp4-documentation-validation.md` | VERALTET (Mangel 4) |
| 2 | Feature-HTML `UC6-VerbrauchAusbuchen.html` | MÄNGEL 1, 2, 3 (+ 8) |
| 3 | Architecture-Overview — UC6-Karte, Testzahl 361 | BESTANDEN |
| 3 | Architecture-Overview — UC9-Zahlen | MANGEL 6 (+ 7) |
| 4 | `sequence-uc6-verbrauchausbuchangung.drawio` | MANGEL 5 (sonst deutlich verbessert) |
| 4 | `requirements/use-cases.drawio` (Punkt B) | KORREKTUR VERIFIZIERT |

---

## Fazit

**Mängelliste — Freigabe noch nicht erteilt.** 6 blockierende Befunde (MEDIUM), 3 nicht
blockierende (LOW). Keiner davon betrifft den Produktivcode; die 361 Tests bleiben grün und
sind von diesen Korrekturen nicht berührt.

Die Substanz stimmt: Die Code-Dokumentation ist vorbildlich — beide nicht-offensichtlichen
Entscheidungen (`ergebnis`-Nicht-Reset, N+1) sind genau dort erklärt, wo ein Leser stolpert.
Das Sequence-Diagramm ist mit `1758dbe` von einer generischen "UI (Blazor)"-Box zu einer
belastbaren Darstellung des echten Ablaufs geworden, und die Legenden-Korrektur in
`use-cases.drawio` hält meiner eigenen Nachzählung stand.

Was fehlt, ist die Konsistenz zwischen den Teilen: eine Feature-Seite, die sich selbst
widerspricht (fertig gegen "Merged Pending"), ein Validierungsreport, der eine inzwischen
geschlossene Lücke als offen führt, ein Diagramm-Szenario ohne Auslöser und eine UC9-Zahl, die
Teil 3 gegen Teil 4 stellt. Genau das ist der Fehlertyp, für den dieses Gate existiert — und
der Grund, warum das Überspringen bei UC6 kein Formfehler war.

**Nächster Schritt**: Mängel 1-9 an den doc-agent, Versuch 2/3. Die Beschreibungen enthalten
Datei, Zeile und Soll-Zustand und sollten ohne Rückfrage abarbeitbar sein.

---

**Reviewer**: Review-Agent
**Datum**: 2026-09-11
**Versuch**: 1 von 3
**Urteil**: **CHANGES REQUIRED** (Freigabe nicht erteilt)
