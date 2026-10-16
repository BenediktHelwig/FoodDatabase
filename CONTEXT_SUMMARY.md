# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-10-16  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Multi-Agent Orchestrated Development mit Git-basiertem Workflow  
**Aktueller Branch**: `fix/lagerort-umlaute-aufraeumen` (PR offen, gestapelt auf PR #12 – #12 zuerst mergen)

---

## ⏭️ NÄCHSTE SCHRITTE (in dieser Reihenfolge)

Offen zur Abnahme: **PR #12** (WP4 UC10) und darauf gestapelt die PR für `fix/lagerort-umlaute-aufraeumen`. #12 zuerst mergen.

**Korrektur zur Roadmap-Bezeichnung**: Bis 19.09.2026 stand hier „WP4 UC4". Laut `UI-PHASE-PLAN.md:156` besteht WP4 nur aus UC2/6/9/10 – **WP4 ist damit fertig** (UC3 blockiert). UC4 gehört zu **WP5** (`UI-PHASE-PLAN.md:164`).

1. **WP5: UI Rezepte (UC4 + UC5)** — nächstes Arbeitspaket. `RezeptListe`, `RezeptForm`, `RezeptDetail` mit Zutaten-CRUD; die fertige `RezeptNährwertAnzeige` wird nur eingebettet. Services fertig. UI-Regel: Code zuerst, dann Tests. Deutlich größer als die bisherigen WP4-Schritte.

2. **UC9→UC10-Integration** (FK statt `string Lagerort`). `FoodDatabaseContext.cs:64-65` vermerkt sie als ausstehend. Solange sie fehlt, sind die unter `/lagerorte` angelegten Lagerorte funktionslos. Seit 16.10.2026 akzeptiert UC9 Umlaute – die UC10-Konstanten „Kühlschrank"/„Tiefkühler" sind damit gültige UC9-Namen, eine Umbenennung ist für die Integration nicht nötig. Schema-Änderung → TDD.

3. **Zwei Diagramme widersprechen dem Code** (gefunden im Doku-Review vom 16.10.2026, außerhalb des damaligen Scopes):
   - `diagrams/database-schema.drawio`: ProduktInstanz-Tabelle zeigt die erfundenen Felder Einheit, MHD, KaufDatum, Barcode.
   - `diagrams/architecture-class.drawio`: Lagerort-Klasse hat Beschreibung, `GetProduktInstanzen()` und `Delete()` – gibt es im Code nicht, Löschen ist laut UC9 ausdrücklich nicht erlaubt.

4. **Kleinere Überträge** aus den drei UC10-Gates, jeweils beim nächsten Anfassen der Datei:
   - Tests: ein `var` in `LebensmittelDetailTests.cs:841`; zwei `WaitForAssertion`-Wrapper um reine Interaktionen; zwei Deckungslücken (eine Packungszeile vollständig prüfen, das Dropdown an `LagerortKonstanten.AlleWerte` binden).
   - Code: Filterleiste flackert beim Laden (`@if (!lädt)`); `GetMhdText(int)` statt `Formatiere(DateTime)` lässt die Tagesrechnung in beiden Views stehen.
   - Doku: UC3 bezeichnet im Repo zwei verschiedene Dinge („Nährwerte" fertig vs. „Lagerbestand Export" blockiert); einzelne Begründungen in der UC10-Filter-Sektion sind schwächer als die Aussage, die sie tragen.

5. **UC3 bleibt blockiert**, bis ein Export-Service existiert — laut `requirements/analysis.md` außerhalb v1.0.

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
⏳ WP5: UI Rezepte (UC4/UC5) ← als Nächstes
⏳ WP6: UI Dashboard (UC7/UC8) – UC8-Service noch TODO
```

### Test-Status (verifiziert 2026-10-16, `dotnet test --list-tests` vom Repo-Root, gezählt in Testfällen)
```
Unit:          284 Testfälle ✅  (Services + Formatter)
Integration:     3 Testfälle ✅  (2 EfRepository + 1 LagerortService/SQLite)
UI (bUnit):    107 Testfälle ✅
────────────────────────────────────
TOTAL:         394 Testfälle ✅  (0 rot)
```
UI-Aufschlüsselung (unverändert seit 19.09.2026): UC1 51 · UC2 12 · UC6 10 · UC9 8 · UC10 17 · NavMenu 6 · MainLayout 3

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

---

## 🎯 WORKFLOW

**Multi-Agent Orchestration** (Details in `CLAUDE.md`): Orchestrator · Test-Agent · Dev-Agent · Doc-Agent · Review-Agent (3-Loop-Feedback, dann User-Eskalation).

**Wichtige Abweichung für UI-Arbeit**: Laut `UI-PHASE-PLAN.md:144` gilt bei WP3–WP6 **Code zuerst, dann Tests** — Dev-Agent → Verifikation → Test-Agent → Review-Agent → Doc-Agent-4-Teil-Check → MR. TDD (Tests zuerst) gilt nur für die Service-Schicht.

**Git**: Feature-Branches → Review → Merge zu `master`.

---

## ✅ ZULETZT FERTIGGESTELLT: Lagerort-Umlaute + UC10-Doku-Aufräumen (16.10.2026)

Anlass: Das UC9-Formular schlug „z.B. Kühlschrank" vor, die Validierung `^[A-Za-z]+$` lehnte es ab. **User-Entscheidung**: Umlaute und ß sind gültig und werden unverändert gespeichert (keine Umschreibung auf ae/oe/ue/ss — eine Rückumwandlung beim Lesen hätte ein Wörterbuch gebraucht: „Mauer" → „Maür"). Neue Regex `^[A-Za-zÄÖÜäöüß]+$`.

**Nebenbefund mit echtem Fehler**: Autocomplete filterte per SQLite-`lower()` (ASCII-only) — „öl" fand „Ölkeller" nicht. Jetzt In-Memory-Filter, abgesichert durch den ersten Lagerort-Test gegen echtes SQLite (siehe Fallstricke).

Außerdem: drei falsche Ist-Aussagen auf `UC10-Produktinstanzen.html` korrigiert (Domain-Tabelle, erfundenes Interface, Zeilenzahl). **383 → 394 Tests.**

**Gates**: Tests und Code im 1. Versuch, Doku im 3. Versuch. Lehre: Die Mängelliste eines Reviews deckt nicht automatisch alle Stellen ab, die dieselbe Aussage tragen — eine korrigierte Datumsangabe erzeugte einen Widerspruch zur zweiten Datumsangabe derselben Seite, die nicht auf der Liste stand. Bei Datum/Stand-Korrekturen im Auftrag alle Vorkommen per grep mitnennen.

---

## ✅ VORLETZT FERTIGGESTELLT: WP4 UC10 — MHD-Sichten (19.09.2026)

UC10 stand als voller Use-Case in der Roadmap. Tatsächlich existierte das CRUD schon, gebaut
unter dem UC2-Label — offen waren nur die MHD-Sichten. Das Feature wurde dadurch deutlich
kleiner: **kein Service, kein Schema, nur UI**.

**Geliefert**: Filterleiste auf `/lagerbestand` (Status + Lagerort, ein Service-Aufruf je
Kombination), Spalte „Tage bis MHD", Packungsliste auf `/lebensmittel/{id}`, dazu
`MhdTextFormatter` als gemeinsame Textquelle. **361 → 383 Tests**.

**Zwei Entscheidungen im Code**: Eine heute ablaufende Packung gilt **nicht** als abgelaufen
(„mindestens haltbar bis"), und die UI sortiert nach jedem Filter nach, weil zwei der drei
Abfragemethoden reine `Where`-Filter sind. Beide sind von je einem Test bewacht, dessen
Wirksamkeit durch Umschalten des Produktivcodes belegt wurde: Jeder Eingriff ließ **genau**
den vorhergesagten Test fallen.

**Sieben Review-Durchläufe über drei Gates.** Die wertvollsten Befunde waren Aussagen, die
plausibel aussahen und falsch waren:
- `@instanz.Verfallsdatum:D` ist in Razor **kein** Format-Specifier — `:D` landete als Literal
  in der Ausgabe. Zwei der drei Stellen waren Altbestand.
- Ein Test namens `LagerortFilter_IsCaseSensitive` prüfte **keine** Case-Sensitivität: Er lief
  durch den Zweig, in dem die Seite gar nicht vergleicht.
- Die Sequence-Lifeline `Blazor UI (ProduktInstanzPage)` zeigte auf eine Komponente, die **nie
  existiert hat** — und die erste Korrektur darauf war ebenfalls falsch.

**Prozess-Lehre**: Zwei Befunde des zweiten Doku-Gates waren Folgeschäden von Korrektionen, die
das erste Gate verlangt hatte. Bei einer Testzahl wurde dreimal nachgebessert, weil der Auftrag
nie sagte, **welche Menge** die Überschrift benennen soll — erst die wörtliche Vorgabe saß.
Wenn ein Review eine Ersatzformulierung verlangt, muss diese die Bezugsmenge mitnennen, nicht
nur die Zahl.

---

## 🚀 ROADMAP (nach Abschluss von UC6)

### Phase 1: UI-Komponenten (aktuell)
WP4 fertig (UC2/6/9/10). UC3 blockiert (außerhalb v1.0). Nächstes Paket: WP5.

### Phase 2: UI Rezepte (WP5)
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
