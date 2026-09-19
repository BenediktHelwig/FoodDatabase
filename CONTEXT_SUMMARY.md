# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-09-19  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Multi-Agent Orchestrated Development mit Git-basiertem Workflow  
**Aktueller Branch**: `docs/uc9-wp4-nachzug` (PR offen zur Abnahme)

---

## ⏭️ NÄCHSTE SCHRITTE (in dieser Reihenfolge)

Der UC9-Doku-Nachzug ist am 19.09.2026 erledigt (PR offen). Als Nächstes:

1. **WP4 UC10 (Produktinstanzen/MHD)** — mit einem wichtigen Befund aus der Vorprüfung: Das
   UC10-**CRUD existiert bereits**, gebaut unter dem Label UC2. `LagerbestandBearbeiten.razor`
   (`/lagerbestand`) listet alle Instanzen über `GetNachVerfallsdatumSortiertAsync()` mit Löschen
   und Verfalls-Farbcodierung, `ProduktInstanzForm.razor` deckt Create und Update ab. Offen sind
   nur die **MHD-Sichten** — die vier Service-Methoden, die kein UI je aufruft:

   | Methode | Fehlende Sicht |
   |---|---|
   | `GetByLagerortAsync(string)` | Filter nach Lagerort |
   | `GetVerfallenenAsync(DateTime?)` | Sicht „bereits abgelaufen" |
   | `GetTagesBisVerfallAsync(int)` | Spalte „Tage bis MHD" |
   | `GetByLebensmittelAsync(int)` | Packungsliste auf `LebensmittelDetail` |

   UC10 ist damit deutlich kleiner als in der Roadmap veranschlagt.

2. **Drei Code-Aufgaben, die beim UC9-Review auffielen** und in den UC10-Branch gehören:
   - `LagerortForm.razor:27`: `placeholder="z.B. Kühlschrank"` verstößt gegen die eigene
     Validierung `^[A-Za-z]+$` (`LagerortService.cs:75`) — die App schlägt eine Eingabe vor, die
     sie selbst mit `ArgumentException` ablehnt.
   - `ILagerortService.cs:40`: Das XML-Doc beschreibt die Normalisierung als „First Letter Upper,
     Rest Lower"; für `"lagerA" → "LagerA"` stimmt das nicht.
   - Die **UC9→UC10-Integration** (FK statt `string Lagerort`) ist weiterhin offen.
     `FoodDatabaseContext.cs:64-65` vermerkt sie selbst als ausstehend.

3. **Danach UC4** (Rezept-Nährwerte). UC3 bleibt blockiert, bis ein Export-Service existiert —
   laut `requirements/analysis.md` außerhalb v1.0.

---

## 📊 GESAMT-STATUS

### Service-Schicht (100 % ✅)
10/10 Use-Cases implementiert · 269 Unit-Tests + 2 Integration-Tests, alle grün

### UI-Schicht
```
✅ WP-Shell: Bootstrap 5.3.3, Off-Canvas Navigation
✅ WP3: UI Lebensmittel
✅ WP4 UC1: Lebensmittel-Katalog (LebensmittelListe/Form/Detail)
✅ WP4 UC2: Lagerbestand (LagerbestandBearbeiten + ProduktInstanzForm)
✅ WP4 UC9: Lagerorte (LagerortListe + LagerortForm) – nur Liste + Neu; Service kann kein Update/Delete – Doku vollständig seit 19.09.2026
✅ WP4 UC6: Verbrauch ausbuchen (VerbrauchListe + VerbrauchZeile) – Doku vollständig seit 11.09.2026
⏳ WP4 UC10: Produktinstanzen/MHD
⏳ WP4 UC4:  Rezept-Nährwerte anzeigen
⚠️ WP4 UC3:  Lagerbestand exportieren – BLOCKIERT, kein Export-Service.
             Laut requirements/analysis.md außerhalb v1.0 (CSV/PDF erst Phase 2+).
⏳ WP5: UI Rezepte (UC4/UC5)
⏳ WP6: UI Dashboard (UC7/UC8) – UC8-Service noch TODO
```

### Test-Status (verifiziert 2026-09-11, `dotnet test` vom Repo-Root)
```
Service + Integration:  271 Tests ✅
UI (bUnit):              90 Tests ✅
────────────────────────────────────
TOTAL:                  361 Tests ✅  (0 rot)
```
UI-Aufschlüsselung (nachgerechnet, geht auf): UC1 51 · UC2 12 · UC6 10 · UC9 8 · NavMenu 6 · MainLayout 3

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

---

## 🎯 WORKFLOW

**Multi-Agent Orchestration** (Details in `CLAUDE.md`): Orchestrator · Test-Agent · Dev-Agent · Doc-Agent · Review-Agent (3-Loop-Feedback, dann User-Eskalation).

**Wichtige Abweichung für UI-Arbeit**: Laut `UI-PHASE-PLAN.md:144` gilt bei WP3–WP6 **Code zuerst, dann Tests** — Dev-Agent → Verifikation → Test-Agent → Review-Agent → Doc-Agent-4-Teil-Check → MR. TDD (Tests zuerst) gilt nur für die Service-Schicht.

**Git**: Feature-Branches → Review → Merge zu `master`.

---

## ✅ ZULETZT FERTIGGESTELLT: UC9 Doku-Nachzug (19.09.2026)

UC9 kam am 30.07.2026 mit `23eb5fc` als Direkt-Commit auf `master` — ohne PR und ohne
Doku-Review. Das Gate ist nachgeholt, in zwei Durchläufen
(`reviews/uc9-wp4-doku-review-1.md` → CHANGES REQUESTED mit 2 HIGH + 7 MEDIUM, `-2.md` → PASS).

**Der gewichtigste Befund**: Die Feature-Seite und das ER-Diagramm beschrieben seit Juni eine
Foreign-Key-Beziehung `ProduktInstanz → Lagerort` samt `ALTER TABLE`-Migration. Die gibt es
nicht — `grep -rn "LagerortId" src/` liefert null Treffer, und `FoodDatabaseContext.cs:64-65`
vermerkt die Integration selbst als ausstehend. Jetzt überall als geplant gekennzeichnet.

**Testzahlen**: acht Stellen sagten „18 Service + 8 UI = 26", richtig sind **24 + 8 = 32**. Die
18 war nie korrekt: `0df33ea` enthielt schon 24 Tests, seine Commit-Message sagte „18 tests",
und die Seite hat das übernommen.

**WP4-UI**: erstmals dokumentiert — Routen, DI, Komponentenfelder, alle vier Anzeigezustände,
NavMenu-Eintrag, ein gegen den Code formulierter Workflow, dazu ein Sequence-Szenario mit
`LagerortForm.razor` als eigener Lifeline (nach dem UC6-Vorbild `1758dbe`).

**Zwei Altlasten nebenbei behoben**: `sequence-uc9-lagerorte.drawio` war kein gültiges XML
(unescapetes `<Lagerort>`, als einziges von 15 Diagrammen), und `DbSet<Lagerort>` stand
unescaped in einem `<pre>` der Feature-Seite, was der Browser als „(DbSet)" rendert.

**Prozess-Lehre aus Review 1**: Der erste Nachbesserungsauftrag war als Liste von Zeilennummern
formuliert — korrigiert wurde genau das, was in der Liste stand, und nichts darüber hinaus. Der
zweite war inhaltlich gefasst („die Seite behauptet keine Integration, die es nicht gibt") und
führte zum Erfolg. Aufträge an schreibende Agenten gehören inhaltlich formuliert.

---

## 🚀 ROADMAP (nach Abschluss von UC6)

### Phase 1: UI-Komponenten (aktuell)
Verbleibend in WP4: **UC10**, dann **UC4**. UC3 bleibt blockiert, bis ein Export-Service existiert.

### Phase 2: UI Rezepte (WP5)
Abhängigkeit: UC4 + UC10 fertig (nicht UC3 — der ist außerhalb v1.0).
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
