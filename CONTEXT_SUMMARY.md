# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-10-18  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Multi-Agent Orchestrated Development mit Git-basiertem Workflow  
**Aktueller Branch**: `feat/ui-rezepte-wp5-teil1` (PR offen zur Abnahme)

---

## ⏭️ NÄCHSTE SCHRITTE (in dieser Reihenfolge)

Offen zur Abnahme: die PR zu **WP5 Teil 1** (`feat/ui-rezepte-wp5-teil1`).

**Korrektur zur Roadmap-Bezeichnung**: Bis 19.09.2026 stand hier „WP4 UC4". Laut `UI-PHASE-PLAN.md:156` besteht WP4 nur aus UC2/6/9/10 – **WP4 ist damit fertig** (UC3 blockiert). UC4 gehört zu **WP5** (`UI-PHASE-PLAN.md:164`).

1. **WP5 Teil 2: Zutaten-CRUD auf `/rezepte/{id}`** — Hinzufügen/Bearbeiten/Löschen über `IRezeptZutatService` (`AddZutatAsync`/`UpdateZutatAsync`/`DeleteZutatAsync`), Lebensmittel-Dropdown, Einheiten `g, ml, Stück, TL, EL, Tasse, Prise`. Der Service verbietet das Löschen der letzten Zutat (`ValidationException`) — die UI muss das anzeigen. Dabei mitnehmen: `@key="Id"` an `<RezeptNährwertAnzeige>` in `RezeptDetail.razor:119`, damit nach Zutatenänderungen bzw. Rezeptwechsel neu geladen wird (die Komponente lädt nur in `OnInitializedAsync`); nach Zutatenänderung müssen die Nährwerte neu berechnet werden. Danach UC4 in der Doku auf fertig setzen. UI-Regel: Code zuerst, dann Tests.

2. **UC9→UC10-Integration** (FK statt `string Lagerort`). `FoodDatabaseContext.cs:64-65` vermerkt sie als ausstehend. Solange sie fehlt, sind die unter `/lagerorte` angelegten Lagerorte funktionslos. Seit 16.10.2026 akzeptiert UC9 Umlaute – die UC10-Konstanten „Kühlschrank"/„Tiefkühler" sind damit gültige UC9-Namen, eine Umbenennung ist für die Integration nicht nötig. Schema-Änderung → TDD.

3. **Zwei Diagramme widersprechen dem Code** (gefunden im Doku-Review vom 16.10.2026, außerhalb des damaligen Scopes):
   - `diagrams/database-schema.drawio`: ProduktInstanz-Tabelle zeigt die erfundenen Felder Einheit, MHD, KaufDatum, Barcode.
   - `diagrams/architecture-class.drawio`: Lagerort-Klasse hat Beschreibung, `GetProduktInstanzen()` und `Delete()` – gibt es im Code nicht, Löschen ist laut UC9 ausdrücklich nicht erlaubt.

4. **Kleinere Überträge** aus den drei UC10-Gates, jeweils beim nächsten Anfassen der Datei:
   - Tests: ein `var` in `LebensmittelDetailTests.cs:841`; zwei `WaitForAssertion`-Wrapper um reine Interaktionen; zwei Deckungslücken (eine Packungszeile vollständig prüfen, das Dropdown an `LagerortKonstanten.AlleWerte` binden).
   - Code: Filterleiste flackert beim Laden (`@if (!lädt)`); `GetMhdText(int)` statt `Formatiere(DateTime)` lässt die Tagesrechnung in beiden Views stehen.
   - Doku: UC3 bezeichnet im Repo zwei verschiedene Dinge („Nährwerte" fertig vs. „Lagerbestand Export" blockiert); einzelne Begründungen in der UC10-Filter-Sektion sind schwächer als die Aussage, die sie tragen.

5. **Aus WP5 Teil 1 vorgemerkt**: `LebensmittelForm.razor:99` bindet an die getrackte EF-Entity (gleicher Fehler wie im Rezept-Formular vor `4893dc8`: Duplikat-Prüfung greift nicht, abgebrochene Eingaben leaken ins nächste SaveChanges), und `:133` fängt wegen `_Imports.razor` vermutlich `System.ComponentModel.DataAnnotations.ValidationException` statt der Service-Exception · `RezeptService.cs:136/138/177` nutzt `== null`/`!= null` · `RezeptForm`: Ladefehler im Bearbeiten-Modus zeigt Fehler über leerem, klickbarem Formular · `diagrams/sequence-uc4-rezepte.drawio` ist seit `8d144bb` kein wohlgeformtes XML (unmaskiertes `&` Z. 129) und enthält keine mxCells · `requirements/use-cases.drawio`: UC7 grün, obwohl WP6-UI offen; Legende hat zwei weiße Kategorien · `sequence-uc5-nährwerte.drawio`: Überschrift „TEST COVERAGE (TDD Red-Phase)“ und Szenario 3 veraltet · UC5-Seite „Offene Fragen“ veraltet.

6. **UC3 bleibt blockiert**, bis ein Export-Service existiert — laut `requirements/analysis.md` außerhalb v1.0.

---

## 📊 GESAMT-STATUS

### Service-Schicht (100 % ✅)
10/10 Use-Cases implementiert · alle Service-, Integrations- und UI-Tests grün (Zählung siehe Test-Status)

### UI-Schicht
```
✅ WP-Shell: Bootstrap 5.3.3, Off-Canvas Navigation
✅ WP3: UI Lebensmittel
✅ WP4 UC1: Lebensmittel-Katalog (LebensmittelListe/Form/Detail)
✅ WP4 UC2: Lagerbestand (LagerbestandBearbeiten + ProduktInstanzForm)
✅ WP4 UC9: Lagerorte (LagerortListe + LagerortForm) – nur Liste + Neu; Service kann kein Update/Delete – Doku vollständig seit 19.09.2026 · Umlaute + ß erlaubt seit 16.10.2026
✅ WP4 UC6: Verbrauch ausbuchen (VerbrauchListe + VerbrauchZeile) – Doku vollständig seit 11.09.2026
✅ WP4 UC10: MHD-Sichten (Filterleiste + Packungsliste) – Doku vollständig seit 19.09.2026
⚠️ WP4 UC3:  Lagerbestand exportieren – BLOCKIERT, kein Export-Service.
             Laut requirements/analysis.md außerhalb v1.0 (CSV/PDF erst Phase 2+).
✅ WP5 Teil 1: Rezepte-UI (RezeptListe/Form/Detail + RezeptNährwertAnzeige eingebettet, Portionen-Fix) – 18.10.2026
⏳ WP5 Teil 2: Zutaten-CRUD (UC4)
⏳ WP6: UI Dashboard (UC7/UC8) – UC8-Service noch TODO
```

### Test-Status (verifiziert 2026-10-18, `dotnet test` vom Repo-Root, gezählt in Testfällen)
```
Unit:          291 Testfälle ✅  (Services + Formatter)
Integration:     3 Testfälle ✅  (2 EfRepository + 1 LagerortService/SQLite)
UI (bUnit):    180 Testfälle ✅  (davon Rezept-UI 73)
────────────────────────────────────
TOTAL:         474 Testfälle ✅  (0 rot)
```
UI-Aufschlüsselung: UC1 51 · UC2 12 · UC6 10 · UC9 8 · UC10 17 · NavMenu 6 · MainLayout 3 · Rezept-UI 73

### Infrastruktur
```
✅ ASP.NET Core 8 + Blazor Server
✅ SQLite mit Entity Framework Core + Migrations
✅ 13 Business Services implementiert + Dependency Injection
✅ Bootstrap 5.3.3 lokal (kein CDN)
✅ Responsive Layout (768px Breakpoint)
✅ Code-Style Standards enforced (explizite Typen, is null/is not null)
```

---

## ⚠️ FALLSTRICKE (nicht wieder hineinlaufen)

- **`_Imports.razor` braucht `@using Microsoft.AspNetCore.Components.Web`.** Ohne diesen Import werden `@onclick`/`@oninput`-Handler zwar kompiliert, aber zur Runtime nie registriert — ein Silent Failure ohne Compilerfehler. Das war die Ursache von 22 roten UI-Tests (behoben 2026-08-08, Commit `a991f07`). Den Import niemals entfernen.
- **`core.ignorecase=true` im Repo.** Datei-Umbenennungen, die nur die Groß-/Kleinschreibung ändern, landen mit `git add -A` **nicht** im Index — git hängt die Datei still wieder unter dem alten Pfad ein. Solche Renames brauchen ein explizites `git mv` über einen Zwischennamen und einen eigenen Commit. Auf einem case-sensitiven Linux-/Docker-Checkout führt unbemerkte Drift sonst zu 404s (passiert bei `UC6-VerbrauchAusbuchen.html`, behoben in `96546da`).
- **bUnit MainLayout**: `Body` als Parameter setzen, nicht `.AddChildContent()`.
- **Agenten, die `.drawio`- oder HTML-Dateien über ein XML-Werkzeug neu schreiben, zerstören den Diff.** Passiert am 11.09.2026: Ein Durchlauf wurde mit `ElementTree` gespeichert statt gezielt editiert — Ergebnis waren 115 geänderte Zeilen, wo eine einzige zu ändern war, alle XML-Kommentare gelöscht, Zeilenenden von LF auf CRLF gekippt und ein doppelt escapetes `&amp;#10;` im Label. Der Auftrag an schreibende Agenten muss deshalb ausdrücklich sagen: gezielte String-Ersetzungen, kein Neuschreiben, XML-Werkzeuge nur zum Prüfen. Kontrolle: `git diff --stat` muss zur Größe der Aufgabe passen.
- **Testzahlen aus Commit-Messages sind keine Quelle.** Die UC9-Doku trug über ein Jahr „18 Tests",
  weil die Commit-Message von `0df33ea` das sagte — die Datei enthielt schon damals 24. Zahlen
  gehören gegen die Testdateien gezählt (`grep -c "\[Fact\]"`), nicht aus der Historie abgeschrieben.
- **Spitze Klammern in `<pre>`-Blöcken und `.drawio`-Labels escapen.** `DbSet<Lagerort>` rendert
  im Browser als „(DbSet)", ein unescapetes `<Lagerort>` in einer `.drawio` macht die Datei zu
  ungültigem XML, das draw.io nicht mehr öffnet. Beides lag seit Juni im Repo.
- **`@wert:Format` ist in Razor kein Format-Specifier.** Implizite Razor-Ausdrücke enden am
  Doppelpunkt; `@datum:D` gibt das Datum samt Uhrzeit aus und hängt ein literales „:D" an.
  Richtig ist `@datum.ToString("dd.MM.yyyy")`. Das Projekt hat keine Culture konfiguriert —
  `"D"` oder `"d"` zögen die OS-Culture des Servers und lieferten auf einem englischen Host
  englische Datumsnamen in einer deutschen Oberfläche.
- **Ein grüner Test beweist nichts, solange nicht geprüft ist, ob er rot werden kann.** Bei
  UC10 lief ein Test namens „IsCaseSensitive" durch einen Codezweig ohne Stringvergleich. Wo
  ein Test eine Designentscheidung absichern soll, lohnt die Gegenprobe: Entscheidung im Code
  umdrehen, prüfen, dass **genau** dieser Test fällt, Änderung zurücknehmen.
- **Der EF-InMemory-Provider verdeckt Übersetzungsfehler nach SQLite.** Er wertet LINQ in C# aus; SQLite übersetzt `ToLower()` in `lower()`, das nur ASCII faltet. `l.Name.ToLower().Contains("öl")` fand „Ölkeller" in den Unit-Tests, in der echten App nicht (behoben 16.10.2026, `5608ee6`). Alles, was von DB-seitigem String-Verhalten abhängt (Case, Collation, Umlaute), braucht einen Test gegen SQLite `:memory:` — Muster: `src/Tests/Integration/LagerortServiceSqliteTests.cs`.
- **Blazor-Formulare nie direkt an geladene Entities binden.** Der `FoodDatabaseContext` lebt pro Circuit; was das Formular in die Entity schreibt, ist für EF bereits eine Änderung — der Service sieht beim Update den neuen Wert als „alten“ (Duplikat-Prüfung wirkungslos), und nach Abbrechen landet die Eingabe mit dem nächsten `SaveChanges` irgendeines Services in der DB. Formular auf eine Kopie binden (Muster: `RezeptForm.razor`).

---

## 🎯 WORKFLOW

**Multi-Agent Orchestration** (Details in `CLAUDE.md`): Orchestrator · Test-Agent · Dev-Agent · Doc-Agent · Review-Agent (3-Loop-Feedback, dann User-Eskalation).

**Wichtige Abweichung für UI-Arbeit**: Laut `UI-PHASE-PLAN.md:144` gilt bei WP3–WP6 **Code zuerst, dann Tests** — Dev-Agent → Verifikation → Test-Agent → Review-Agent → Doc-Agent-4-Teil-Check → MR. TDD (Tests zuerst) gilt nur für die Service-Schicht.

**Git**: Feature-Branches → Review → Merge zu `master`.

---

## ✅ ZULETZT FERTIGGESTELLT: WP5 Teil 1 — Rezepte-UI (18.10.2026)

**Geliefert**: `/rezepte` (Liste, Suche, Soft-Delete-Modal), `/rezepte/neu` + `/{id}/bearbeiten` (Defaults Portionen 1/Medium, nach Speichern zum Detail), `/rezepte/{id}` (Stammdaten, Zutaten lesend, `RezeptNährwertAnzeige` eingebettet; ohne Zutaten Hinweis statt Nullwerten). Schwierigkeitsgrad deutsch angezeigt via `SchwierigkeitsgradAnzeige`, englisch gespeichert. **394 → 474 Tests.**

**Zwei echte Fehler gefunden**: (1) Das Formular band an die EF-getrackte Entity — DbContext lebt pro Circuit, `UpdateRezeptAsync` bekommt per Identity Resolution dieselbe Instanz, die Eingaben stehen also schon drin, bevor der Service prüft. Lösung: Formular auf einer Kopie. `LebensmittelForm` hat denselben Fehler (vorgemerkt). (2) `RezeptNährwertAnzeige` startete fest bei 4 Portionen, der Service skaliert aber mit `rezept.Portionen` — fiel erst beim Einbetten auf, Fix `27dc466` per User-Entscheidung im selben PR.

**Prozess-Lehre**: Eine Annahme im Plan war falsch („`EfRepository` lädt `RezeptZutat.Lebensmittel` nicht“) — `FoodDatabaseContext.cs:110` hat `AutoInclude()`. Behauptungen über Ladeverhalten im Plan gegen den DbContext prüfen, nicht gegen das Repository. Außerdem: Grenztests hinter einer zweiten Schutzstufe können nicht rot werden — angezeigten Zustand prüfen, nicht nur ausbleibende Service-Aufrufe.

---

## ✅ VORLETZT FERTIGGESTELLT: Lagerort-Umlaute + UC10-Doku-Aufräumen (16.10.2026)

Anlass: Das UC9-Formular schlug „z.B. Kühlschrank" vor, die Validierung `^[A-Za-z]+$` lehnte es ab. **User-Entscheidung**: Umlaute und ß sind gültig und werden unverändert gespeichert (keine Umschreibung auf ae/oe/ue/ss — eine Rückumwandlung beim Lesen hätte ein Wörterbuch gebraucht: „Mauer" → „Maür"). Neue Regex `^[A-Za-zÄÖÜäöüß]+$`.

**Nebenbefund mit echtem Fehler**: Autocomplete filterte per SQLite-`lower()` (ASCII-only) — „öl" fand „Ölkeller" nicht. Jetzt In-Memory-Filter, abgesichert durch den ersten Lagerort-Test gegen echtes SQLite (siehe Fallstricke).
Außerdem: drei falsche Ist-Aussagen auf `UC10-Produktinstanzen.html` korrigiert (Domain-Tabelle, erfundenes Interface, Zeilenzahl). **383 → 394 Tests.**
**Gates**: Tests und Code im 1. Versuch, Doku im 3. Versuch. Lehre: Die Mängelliste eines Reviews deckt nicht automatisch alle Stellen ab, die dieselbe Aussage tragen — eine korrigierte Datumsangabe erzeugte einen Widerspruch zur zweiten Datumsangabe derselben Seite, die nicht auf der Liste stand. Bei Datum/Stand-Korrekturen im Auftrag alle Vorkommen per grep mitnennen.

---

## 🚀 ROADMAP (nach Abschluss von UC6)

### Phase 1: UI-Komponenten (aktuell)
WP4 fertig (UC2/6/9/10). UC3 blockiert (außerhalb v1.0). Nächstes Paket: WP5.

### Phase 2: UI Rezepte (WP5)
Teil 1 ✅ (18.10.2026), Teil 2 Zutaten-CRUD offen.
Abhängigkeit: WP4 fertig ✅ (UC3 nicht nötig — außerhalb v1.0).
- **UC4**: Rezepte CRUD UI (Liste, Form, Detail)
- **UC5**: Rezept-Nährwerte automatisch berechnen (UI)
- Service-Layer bereits implementiert ✅

### Phase 3: UI Dashboard (WP6)
Abhängigkeit: WP4 + WP5 komplett.
- **UC7**: Verfallsdatum-Warnungen Dashboard
- **UC8**: Einkaufslisten Generator UI (Service-Layer noch TODO)

### Phase 4: Infrastruktur & Deployment
Abhängigkeit: Alle UI-Komponenten komplett.
- **TrueNAS Docker**: Dockerfile (ASP.NET Core 8), Docker Compose (App + SQLite), Container-Deployment, Networking + optional SSL/Reverse Proxy. Referenz: `diagrams/architecture-deployment.drawio` (teilweise vorhanden)
- **Performance**: DB-Indexing, Blazor SSR vs. Server, Caching, Load Testing (3 gleichzeitige Nutzer), Monitoring/Logging (optional)

---

## 📁 WICHTIGE DATEIEN

**Steuerung**: `CLAUDE.md` (Orchester-Workflow) · `UI-PHASE-PLAN.md` (WP-Zuschnitt, UI-Reihenfolge) · `claude/agents/*.md`

**Dokumentation**: `docs/architecture-overview.html` (Master) · `docs/features/UC*.html` · `requirements/use-cases.drawio` (Status-Diagramm) · `requirements/analysis.md` (v1.0-Scope)

**Code**: `src/App/Services/Classes/*.cs` (alle fertig) · `src/App/Components/Pages/*/*.razor` · `src/Tests/` (Unit + Integration + bUnit)
