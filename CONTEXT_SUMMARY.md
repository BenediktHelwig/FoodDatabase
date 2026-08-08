# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-08-08  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Multi-Agent Orchestrated Development mit Git-basiertem Workflow  
**Aktueller Branch**: `feat/ui-verbrauch-uc6` (3 Commits, noch nicht nach `master` gemergt)

---

## ⏭️ NÄCHSTE SCHRITTE (in dieser Reihenfolge)

UC6 ist funktional fertig, aber die **Definition-of-Done ist noch nicht erfüllt**. Diese drei Schritte schließen sie ab — erst danach den nächsten Use-Case starten:

1. **Sequence-Diagramm vervollständigen** (Doc-Agent, Teil 4 des 4-Teil-Checks)  
   `diagrams/sequence-uc6-verbrauchausbuchangung.drawio` hat bereits eine Lifeline „UI (Blazor)". Es fehlt nur die konkrete Benennung (`VerbrauchListe.razor` / Route `/verbrauchen`) und die Rückgabe des Ergebnis-Alerts. 146 Zeilen XML, risikoarmer Edit.

2. **Review-Agent Doku-Phase** → `reviews/uc6-wp4-doku-review-1.md`  
   Wurde bei UC6 übersprungen. Dabei gezielt prüfen: Hat der Doc-Agent den **UC9-Status** im Architecture-Overview zu Recht auf „fertig" gesetzt? Das war nicht Teil seines Auftrags und wurde nebenbei mitgeändert.

3. **Merge Request** für `feat/ui-verbrauch-uc6` nach dem Template in `CLAUDE.md`

**Danach: WP4 UC10 (Produktinstanzen/MHD) vor UC4.** Begründung: `IProduktInstanzService` ist die Datenbasis für das spätere Dashboard UC7, und UC10 bleibt im Lager-Kontext, dessen Muster gerade frisch sind. UC4 wechselt in die Rezept-Domäne und passt besser direkt vor WP5.

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
✅ WP4 UC6: Verbrauch ausbuchen (VerbrauchListe + VerbrauchZeile) ← SOEBEN FERTIG
⏳ WP4 UC10: Produktinstanzen/MHD
⏳ WP4 UC4:  Rezept-Nährwerte anzeigen
⚠️ WP4 UC3:  Lagerbestand exportieren – BLOCKIERT, kein Export-Service.
             Laut requirements/analysis.md außerhalb v1.0 (CSV/PDF erst Phase 2+).
⏳ WP5: UI Rezepte (UC4/UC5)
⏳ WP6: UI Dashboard (UC7/UC8) – UC8-Service noch TODO
```

### Test-Status (verifiziert 2026-08-08, `dotnet test` vom Repo-Root)
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

---

## 🎯 WORKFLOW

**Multi-Agent Orchestration** (Details in `CLAUDE.md`): Orchestrator · Test-Agent · Dev-Agent · Doc-Agent · Review-Agent (3-Loop-Feedback, dann User-Eskalation).

**Wichtige Abweichung für UI-Arbeit**: Laut `UI-PHASE-PLAN.md:144` gilt bei WP3–WP6 **Code zuerst, dann Tests** — Dev-Agent → Verifikation → Test-Agent → Review-Agent → Doc-Agent-4-Teil-Check → MR. TDD (Tests zuerst) gilt nur für die Service-Schicht.

**Git**: Feature-Branches → Review → Merge zu `master`.

---

## ✅ ZULETZT FERTIGGESTELLT: WP4 UC6 – Verbrauch ausbuchen (UI)

Neue Seite `/verbrauchen`: listet je Lebensmittel die Gesamtmenge plus Badge bei unterschrittenem Mindestbestand; der „Verbrauchen"-Button ruft `IVerbrauchAusbuchangService.VerbauchtProduktAsync` auf, das nach **FIFO** die Packung mit dem frühesten Verfallsdatum löscht. Ergebnis erscheint als grünes bzw. gelbes Alert.

**Code**: `src/App/Components/Pages/Lager/VerbrauchListe.razor`, `VerbrauchZeile.cs`, NavMenu-Eintrag  
**Tests**: `src/Tests/Ui/VerbrauchListeTests.cs` (10 bUnit-Tests), NavMenuTests 7 → 8 Links  
**Doku**: `reviews/uc6-wp4-code-review-1.md` (freigegeben, 1/3), `reviews/uc6-wp4-documentation-validation.md`, `docs/features/UC6-VerbrauchAusbuchen.html`, `docs/architecture-overview.html`

### Prozess-Anmerkungen zu dieser Session
- **Bewusste Design-Entscheidung**: `AlleAbrufen()` darf das Feld `ergebnis` **nicht** zurücksetzen. `VerbrauchtAusbuchung()` ruft die Methode zum Neuladen auf, *nachdem* die ServiceResult-Meldung gesetzt wurde — ein Reset würde das Ergebnis-Alert verschlucken. Tests 6 und 7 sichern das ab.
- **N+1-Service-Calls** in `AlleAbrufen()` sind eine bewusste KISS-Entscheidung für die Zielgröße (3 gleichzeitige Nutzer, lokale SQLite) — nicht „optimieren".
- **Workflow-Abweichung**: Der `ergebnis`-Bug wurde direkt korrigiert statt über eine Feedback-Schleife an den Dev-Agent zurückgegeben. Der Review-Agent hat den Code danach begutachtet, ohne dass der Fehler je in einem Report auftauchte.

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
