# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-09-11  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Multi-Agent Orchestrated Development mit Git-basiertem Workflow  
**Aktueller Branch**: `docs/uc6-definition-of-done` (PR offen zur Abnahme)

---

## ⏭️ NÄCHSTE SCHRITTE (in dieser Reihenfolge)

UC6 ist vollständig abgeschlossen — Definition-of-Done am 11.09.2026 nachgeholt. Als Nächstes:

1. **WP4 UC10 (Produktinstanzen/MHD)** — der reguläre nächste Use-Case. `IProduktInstanzService`
   ist die Datenbasis für das spätere Dashboard UC7, und UC10 bleibt im Lager-Kontext, dessen
   Muster gerade frisch sind.
2. **UC9-Nachzug** (klein, kann davor oder parallel laufen): `docs/features/UC9-Lagerorte.html`
   nennt an sechs Stellen (181, 183, 239, 358, 386, 399) „18 Service / 26 gesamt" — tatsächlich
   sind es **24 Service + 8 UI = 32**. Außerdem fehlt für die UC9-UI bis heute ein
   `reviews/uc9-wp4-*.md`; der Status wurde seinerzeit beiläufig in einem UC6-Commit gesetzt.
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
✅ WP4 UC9: Lagerorte (LagerortListe + LagerortForm) – nur Liste + Neu; Service kann kein Update/Delete
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

---

## 🎯 WORKFLOW

**Multi-Agent Orchestration** (Details in `CLAUDE.md`): Orchestrator · Test-Agent · Dev-Agent · Doc-Agent · Review-Agent (3-Loop-Feedback, dann User-Eskalation).

**Wichtige Abweichung für UI-Arbeit**: Laut `UI-PHASE-PLAN.md:144` gilt bei WP3–WP6 **Code zuerst, dann Tests** — Dev-Agent → Verifikation → Test-Agent → Review-Agent → Doc-Agent-4-Teil-Check → MR. TDD (Tests zuerst) gilt nur für die Service-Schicht.

**Git**: Feature-Branches → Review → Merge zu `master`.

---

## ✅ ZULETZT FERTIGGESTELLT: UC6 Definition-of-Done nachgeholt (11.09.2026)

UC6 war seit dem 08.08.2026 gemergt, aber mit unvollständiger Doku: Das Sequence-Diagramm
kannte die UI-Ebene nicht, und das Doku-Review-Gate war komplett übersprungen worden. Beides
ist nachgeholt.

**Diagramme**: Sequence-Diagramm zeigt jetzt `VerbrauchListe.razor` (`/verbrauchen`), den Klick
als Auslöser und das Ergebnis-Alert in beiden Szenarien. Legende in `use-cases.drawio`
korrigiert (UC6 und UC9 wurden dort noch als „Service only" geführt).

**Doku-Review**: `reviews/uc6-wp4-doku-review-1.md` und `-2.md`. Neun Befunde, alle behoben,
Freigabe in Schleife 2 von 3. Der auffälligste: Die Feature-Seite listete unter „UI-Tests"
zusätzlich die zehn Service-Testnamen und behauptete gleichzeitig Badge „FERTIG" und „Merged
⏳ Pending".

**Prozess-Befund (LOW)**: Commit `d6676de` hatte den UC9-Status beiläufig in einem UC6-Commit
mitgesetzt. Inhaltlich richtig, aber ohne eigenes Review — so blieb eine falsche Testzahl über
einen Monat unbemerkt. Dokumentiert, nicht zurückgerollt.

**Zwischen 08.08. und 11.09.2026** lief ausschließlich Arbeit am Agenten-Orchester selbst
(PRs #5–#8: Orchestrator-Zuschnitt, GitHub-App-Token für PRs, Token-Gate), kein Produktivcode.

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
