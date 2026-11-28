# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-11-28  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Agent-Orchester (feature-crew: Hauptsession · implementer · reviewer) mit Git-basiertem Workflow  
**Aktueller Branch**: `feat/uc9-uc10-lagerort-fk` (PR offen zur Abnahme) · PR #17 (WP5 Teil 2) ist gemergt

---

## ⏭️ NÄCHSTE SCHRITTE (in dieser Reihenfolge)

Offen zur Abnahme: **PR UC9→UC10-Integration** (`feat/uc9-uc10-lagerort-fk`). **Nach dem Merge die lokale `fooddatabase.db` löschen und mit `dotnet ef database update --project src/App` neu aufbauen.** Die Migration `LagerortFremdschluessel` hat keinen Datenumzug (User-Entscheidung: DB fängt neu an). Nächstes Paket laut Roadmap wäre WP6 (Dashboard), UC8 braucht dafür aber zuerst einen Service. Die Punkte unten sind kleiner und bauen vorher Altlasten ab.

**Korrektur zur Roadmap-Bezeichnung**: Bis 19.09.2026 stand hier „WP4 UC4". Laut `UI-PHASE-PLAN.md:156` besteht WP4 nur aus UC2/6/9/10 – **WP4 ist damit fertig** (UC3 blockiert). UC4 gehört zu **WP5** (`UI-PHASE-PLAN.md:164`).

1. **Zirkulärer FK zwischen `LebensmittelKatalog` und `Nährwert`** (gefunden 28.11.2026 beim SQLite-Integrationstest): In der DB verlangt `LebensmittelKatalog` eine `NährwertId`, und `Nährwert` verweist über `LebensmittelId` zurück. Gegen echtes SQLite lässt sich deshalb mit eingeschalteten Fremdschlüsseln kein Katalog-Eintrag anlegen. `ProduktInstanzLagerortSqliteTests` umgeht das mit `PRAGMA foreign_keys = OFF` im Arrange. Zuerst prüfen, ob die App auf einer frischen DB überhaupt Lebensmittel anlegen kann. Schema-Änderung → TDD.

2. **Zwei Diagramme sind kein gültiges XML** (vorbestehend, gefunden 28.11.2026): `diagrams/architecture-class-uc4.drawio` (`--` in einem Kommentar) und `diagrams/database-schema-uc4-update.drawio` (kaputte Umlaute). Gezielt reparieren, nicht neu schreiben.

3. **Kleinere Überträge** aus den drei UC10-Gates, jeweils beim nächsten Anfassen der Datei:
   - Tests: zwei `WaitForAssertion`-Wrapper um reine Interaktionen; eine Deckungslücke (eine Packungszeile vollständig prüfen).
   - Code: Filterleiste flackert beim Laden (`@if (!lädt)`); `GetMhdText(int)` statt `Formatiere(DateTime)` lässt die Tagesrechnung in beiden Views stehen.
   - Doku: UC3 bezeichnet im Repo zwei verschiedene Dinge („Nährwerte" fertig vs. „Lagerbestand Export" blockiert); einzelne Begründungen in der UC10-Filter-Sektion sind schwächer als die Aussage, die sie tragen.

4. **Aus WP5 Teil 1 vorgemerkt**: `LebensmittelForm.razor:99` bindet an die getrackte EF-Entity (gleicher Fehler wie im Rezept-Formular vor `4893dc8`: Duplikat-Prüfung greift nicht, abgebrochene Eingaben leaken ins nächste SaveChanges), und `:133` fängt wegen `_Imports.razor` vermutlich `System.ComponentModel.DataAnnotations.ValidationException` statt der Service-Exception · `RezeptService.cs:136/138/177` nutzt `== null`/`!= null` · `RezeptForm`: Ladefehler im Bearbeiten-Modus zeigt Fehler über leerem, klickbarem Formular · `diagrams/sequence-uc4-rezepte.drawio` ist seit `8d144bb` kein wohlgeformtes XML (unmaskiertes `&` Z. 129) und enthält keine mxCells · `requirements/use-cases.drawio`: UC7 grün, obwohl WP6-UI offen; Legende hat zwei weiße Kategorien · `sequence-uc5-nährwerte.drawio`: Überschrift „TEST COVERAGE (TDD Red-Phase)“ und Szenario 3 veraltet · UC5-Seite „Offene Fragen“ veraltet.

5. **Aus WP5 Teil 2 vorgemerkt**: `docs/features/UC4-Rezepte.html:8` hat einen vorbestehenden Inline-`<style>`-Block, obwohl `docs/css/uc4-rezepte.css` existiert (nicht verlinkt) — Profilregel „CSS nie inline“ · Titel in `diagrams/sequence-uc4-rezepte.drawio` (Zelle 1) nennt nur den Bearbeitungsmodus, das Diagramm enthält inzwischen auch CreateRezept und CalculateNährwerte · `RezeptDetail.razor`: die beiden voll qualifizierten catches im Speichern haben denselben Rumpf wie der generische `catch (Exception)` — zusammenfassbar.

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
✅ WP4 UC9: Lagerorte (LagerortListe + LagerortForm) – nur Liste + Neu; Service kann kein Update/Delete – Doku vollständig seit 19.09.2026 · Umlaute + ß erlaubt seit 16.10.2026 · per FK an ProduktInstanz seit 28.11.2026
✅ WP4 UC6: Verbrauch ausbuchen (VerbrauchListe + VerbrauchZeile) – Doku vollständig seit 11.09.2026
✅ WP4 UC10: MHD-Sichten (Filterleiste + Packungsliste) – Doku vollständig seit 19.09.2026
⚠️ WP4 UC3:  Lagerbestand exportieren – BLOCKIERT, kein Export-Service.
             Laut requirements/analysis.md außerhalb v1.0 (CSV/PDF erst Phase 2+).
✅ WP5 Teil 1: Rezepte-UI (RezeptListe/Form/Detail + RezeptNährwertAnzeige eingebettet, Portionen-Fix) – 18.10.2026
✅ WP5 Teil 2: Zutaten-CRUD (UC4) – Bearbeitungsmodus auf /rezepte/{id}, leeres Rezept → Löschen-Modal – 27.11.2026
⏳ WP6: UI Dashboard (UC7/UC8) – UC8-Service noch TODO
```

### Test-Status (verifiziert 2026-11-28, `dotnet test` vom Repo-Root, gezählt in Testfällen)
```
Unit:          303 Testfälle ✅  (Services + Formatter)
Integration:     6 Testfälle ✅  (2 EfRepository + 1 LagerortService/SQLite + 3 ProduktInstanz-Lagerort/SQLite)
UI (bUnit):    233 Testfälle ✅  (davon Rezept-UI 108)
────────────────────────────────────
TOTAL:         542 Testfälle ✅  (0 rot)
```
UI-Aufschlüsselung: UC1 51 · UC2 26 · UC6 10 · UC9 8 · UC10 21 · NavMenu 6 · MainLayout 3 · Rezept-UI 108

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
- **Eine Mängelliste deckt nicht alle Stellen ab, die dieselbe Aussage tragen.** Bei UC9/UC10 (16.10.2026) erzeugte eine korrigierte Datumsangabe einen Widerspruch zur zweiten Angabe derselben Seite, die nicht auf der Liste stand. Bei Datum-/Stand-Korrekturen im Auftrag alle Vorkommen per grep mitnennen.
- **Ladeverhalten steht im DbContext, nicht im Repository.** `FoodDatabaseContext.cs:110` hat `AutoInclude()` für `RezeptZutat.Lebensmittel` — eine Plan-Annahme in WP5 Teil 1 war daran falsch.
- **Blazor-Formulare nie direkt an geladene Entities binden.** Der `FoodDatabaseContext` lebt pro Circuit; was das Formular in die Entity schreibt, ist für EF bereits eine Änderung — der Service sieht beim Update den neuen Wert als „alten“ (Duplikat-Prüfung wirkungslos), und nach Abbrechen landet die Eingabe mit dem nächsten `SaveChanges` irgendeines Services in der DB. Formular auf eine Kopie binden (Muster: `RezeptForm.razor`).

---

## 🎯 WORKFLOW

**Agent-Orchester feature-crew** (Start: `cd feature-crew; claude` · Crew unverändert und englisch in `feature-crew/` · Projektregeln mit Vorrang in `CLAUDE.md`, Konventionen in `docs/PROJECT-PROFILE.md`): Hauptsession (opus, plant und gibt frei) · `implementer` (sonnet, Tests → Code → Doku) · `reviewer` (opus, ein Review über den ganzen Diff; bis zu 3 Versuche, dann User-Eskalation).

**Wichtige Abweichung für UI-Arbeit**: Laut Schicht-Regel in `docs/PROJECT-PROFILE.md` gilt bei WP3–WP6 **Code zuerst, dann Tests** — `implementer`: Code-Commit → grüner Test-Commit → 4-Teil-Doku-Commit, danach `reviewer` → PR. TDD (Tests zuerst) gilt nur für die Service-Schicht. Der Plan nennt die geltende Schicht-Regel.

**Git**: Feature-Branch → Pull Request über die GitHub App `claudecodeagents[bot]` → Review und Merge durch den User. Nie direkt auf `master`.

---

## ✅ ZULETZT FERTIGGESTELLT: UC9→UC10-Integration — Lagerort als Fremdschlüssel (28.11.2026)

**Geliefert**: `ProduktInstanz` verweist über `LagerortId` (FK, `Restrict`, `AutoInclude`) auf die UC9-Tabelle `Lagerorte`, der String und `LagerortKonstanten` sind weg. Migration `LagerortFremdschluessel` ohne Seed und ohne Datenumzug (User-Entscheidung: DB fängt neu an). `ProduktInstanzService` und `LagerbestandService` nehmen `int lagerortId`, unbekannte oder archivierte Lagerorte werfen `ArgumentException`. Das Packungs-Formular hat ein Dropdown mit den vorhandenen Lagerorten plus „Neuer Lagerort…“ (Textfeld, angelegt über `GetOrCreateAsync`). Ist die Liste leer, ist „Neu“ vorausgewählt. Lagerort-Fehler stehen am Feld und sperren den Speichern-Button nicht. Filter auf `/lagerbestand` und die Packungszeilen auf `/lebensmittel/{id}` nutzen die echten Lagerorte. Nebenbei bereinigt: `database-schema.drawio` und `architecture-class.drawio` zeigen die Code-Felder (alter Punkt 2). **509 → 542 Tests.**

**Lauf**: Versuch 1 lieferte vier Doku-Mängel (Status, Verweis, Testzahl, Zeilenzahl), Versuch 2 war `PASS`. Ein Formalverstoß blieb, vom User akzeptiert und in der PR benannt: Der neue SQLite-Test wurde zwischen rotem und grünem Commit im Arrange geändert (zirkulärer FK, siehe Nächste Schritte Punkt 1).

**Prozess-Lehren**:
- Scheitert ein roter Commit am Compiler, sind auch die neuen Tests nie gelaufen. Ein Fehler im Arrange fällt dann erst im grünen Commit auf, und die Korrektur verletzt die Unveränderlichkeit. Bei neuen Integrationstests den Arrange vorab gegen den alten Stand prüfen, soweit er kompiliert.
---

## ✅ VORLETZT FERTIGGESTELLT: WP5 Teil 2 — Zutaten-CRUD (27.11.2026, PR #17)

Bearbeitungsmodus für Zutaten auf `/rezepte/{id}`: lokal hinzufügen, ändern und entfernen, gesammelt speichern (Add → Update → Delete). Ein leeres Rezept wird nach Rückfrage archiviert. UC4 ist in allen vier Doku-Teilen fertig. **474 → 509 Tests.** Lehre: Wer ein Diagramm „neu aufbaut“, muss im Auftrag ausdrücklich gesagt bekommen, dass bestehende Inhalte erhalten bleiben.

---

## 🚀 ROADMAP (nach Abschluss von UC6)

### Phase 1: UI-Komponenten (aktuell)
WP4 fertig (UC2/6/9/10). UC3 blockiert (außerhalb v1.0). WP5 fertig. Nächstes Paket: WP6.

### Phase 2: UI Rezepte (WP5)
Teil 1 ✅ (18.10.2026), Teil 2 Zutaten-CRUD ✅ (27.11.2026) — WP5 fertig.
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

**Steuerung**: `feature-crew/` (unveränderte Crew, englisch: `CLAUDE.md`, `.claude/agents/`, `docs/QUALITY-BAR.md`, `docs/PLAN-FORMAT.md`) · `CLAUDE.md` (Projektregeln, Vorrang) · `docs/PROJECT-PROFILE.md` · `UI-PHASE-PLAN.md` (WP-Zuschnitt, UI-Reihenfolge)

**Dokumentation**: `docs/architecture-overview.html` (Master) · `docs/features/UC*.html` · `requirements/use-cases.drawio` (Status-Diagramm) · `requirements/analysis.md` (v1.0-Scope)

**Code**: `src/App/Services/Classes/*.cs` (alle fertig) · `src/App/Components/Pages/*/*.razor` · `src/Tests/` (Unit + Integration + bUnit)
