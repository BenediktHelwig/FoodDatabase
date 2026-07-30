# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-07-30  
**Projekt**: C# / ASP.NET Core 8 / Blazor Server + SQLite (TrueNAS Docker)  
**Status**: Multi-Agent Orchestrated Development mit Git-basiertem Workflow

---

## 📊 GESAMT-STATUS

### Service-Schicht (100% ✅)
- 10/10 Use-Cases implementiert
- 269/269 Unit-Tests grün ✅
- 2/2 Integration-Tests grün ✅

### UI-Schicht
```
✅ WP-Shell: Bootstrap 5.3.3, Off-Canvas Navigation
✅ WP3: UI Lebensmittel
✅ WP4 UC1: Lebensmittel-Katalog (LebensmittelListe/Form/Detail)
✅ WP4 UC2: Lagerbestand (LagerbestandBearbeiten + ProduktInstanzForm)
✅ WP4 UC9: Lagerorte (LagerortListe + LagerortForm, 8 Tests) ← SOEBEN FERTIG
⏳ WP4: UC3/UC4/UC6/UC10 (noch zu implementieren)
⏳ WP5: UI Rezepte (UC4/UC5)
⏳ WP6: UI Dashboard (UC7/UC8)

UI-Tests: 80 gesamt → 58 GRÜN / 22 ROT ⚠️
```

### Gesamt Test-Status (verifiziert 2026-07-31 nach Phase 1c, `dotnet test` vom Repo-Root)
```
Service + Integration:  271 Tests ✅  (0 rot)
UI (bUnit):             80 Tests → 70 GRÜN / 10 ROT ⚠️
────────────────────────────────────────────────────
TOTAL:                  351 Tests → 341 GRÜN / 10 ROT (97%)
```

**Reparatur-Fortschritt (FIX-UI-TESTS-PLAN.md):**
- ✅ Phase 0 (Diagnose): 3 Cluster identifiziert
- ✅ Phase 1 (Implementation): 12/22 Tests fixiert (54.5% Reduktion)
  - ✅ Cluster B (MainLayout): 3/3 Tests grün — `.AddChildContent()` entfernt
  - ✅ Cluster C (Nährwert-Fehler): 1/1 Test grün — Component Error Handling refaktoriert
  - ✅ Cluster A (Form-Events): 8/18 Tests grün — `.Input()` → `.Change()` für `<InputText>`/`<InputNumber>`
  - ✅ Dev-Agent Phase 1b: LebensmittelListe.razor refaktoriert (plain `<input>` → `<InputText>`)
- ⏳ Verbleibend (Phase 2): 10 Button-Tests (LebensmittelListe `.Click()` auf `@onclick`)
  - **Nächster Schritt: Option 1 — Button-Refaktorierung mit Blazor-Komponenten** (nicht Test-Logik-Umschreiben)

**Ursachen der 22 ursprünglichen Fehler:**
- Cluster A (18): Blazor `@bind` + `@onclick` auf plain HTML nicht bUnit-kompatibel
- Cluster B (3): MainLayout falsche API (`.AddChildContent()` statt Body-Parameter)
- Cluster C (1): Component async error handling nicht korrekt

### Infrastruktur
```
✅ ASP.NET Core 8 + Blazor Server
✅ SQLite mit Entity Framework Core + Migrations
✅ 13 Business Services implementiert + Dependency Injection
✅ Bootstrap 5.3.3 lokal (kein CDN)
✅ Responsive Layout (768px Breakpoint)
✅ Code-Style Standards enforced (var-usage, is null/is not null)
```

---

## 🎯 AKTUELLER WORKFLOW

**Multi-Agent Orchestration** (siehe `CLAUDE.md` für Details):
- **Orchestrator**: Koordiniert Use-Cases und Anforderungen
- **Test-Agent**: Schreibt bUnit-Tests (TDD, Rot-Phase)
- **Dev-Agent**: Implementiert Produktivcode (TDD, Grün-Phase)
- **Doc-Agent**: Dokumentation (4-Teil-Check: Code + HTML + Overview + Diagramme)
- **Review-Agent**: 3-Loop Feedback-Prozess (max 3 Iterationen, dann User-Eskalation)

**Git Workflow**: Feature-Branches → Review → Merge zu master

---

## ✅ ZULETZT FERTIGGESTELLT (Session 28)

**WP4 UC1 Lebensmittel-Katalog – UI-Komponenten + Tests + Dokumentation**

### Was wurde gebaut:
- LebensmittelListe.razor (26 bUnit-Tests) – Tabelle mit Suche/Löschen
- LebensmittelForm.razor (19 bUnit-Tests) – Neu/Bearbeiten-Formular
- LebensmittelDetail.razor (25 bUnit-Tests) – Detail-View mit Nährwert-Integration

### Dokumentation:
- ✅ Code-Dokumentation: `reviews/uc1-wp4-documentation-validation.md`
- ✅ Feature-HTML: `docs/features/UC1-LebensmittelKatalog.html`
- ✅ Architecture-Overview: `docs/architecture-overview.html` (UC1 Status)
- ✅ Diagramme: `requirements/use-cases.drawio` (UC1 Status grün)

### Code-Style:
- ✅ 70 Tests kompilieren fehlerlos
- ✅ 100% explizite Typ-Deklarationen (kein var außer LINQ)
- ✅ `is null` / `is not null` durchgehend

---

## 🚀 ROADMAP & NÄCHSTE SCHRITTE

### Phase 1: UI-Komponenten (aktuell)
**Status**: WP3 + WP4 UC1+UC2 fertig → **WP4 UC3/UC4/UC6 starten**

- ✅ WP-Shell (Navigations-Shell) – fertig
- ✅ WP3 (Lebensmittel Liste) – fertig
- ✅ WP4 UC1 (Lebensmittel-Katalog CRUD) – fertig
- ✅ WP4 UC2 (Lagerbestand aktualisieren) – fertig
- ✅ **WP4 UC9** (Lagerorte verwalten) – fertig (nur Liste + Neu; Service kann kein Update/Delete)
- ⚠️ **WP4 UC3** (Lagerbestand exportieren) – BLOCKIERT: kein Export-Service vorhanden, laut `requirements/analysis.md` außerhalb v1.0 (CSV/PDF erst Phase 2+). Service müsste zuerst gebaut werden.
- ⏳ **WP4 UC4** (Rezept-Nährwerte anzeigen) – TODO
- ⏳ **WP4 UC6** (Verbrauchte Produkte ausbuchen) – TODO
- ⏳ **WP4 UC10** (Produktinstanzen MHD) – TODO

### Phase 2: UI Rezepte (WP5)
**Abhängigkeiten**: Alle WP4 UC3/UC4/UC6 müssen fertig sein

- **WP5 UC4**: Rezepte CRUD UI (Liste, Form, Detail)
- **WP5 UC5**: Rezept-Nährwerte automatisch berechnen (UI)
- Service-Layer bereits implementiert ✅

### Phase 3: UI Dashboard (WP6)
**Abhängigkeiten**: WP4 + WP5 komplett

- **WP6 UC7**: Verfallsdatum-Warnungen Dashboard
- **WP6 UC8**: Einkaufslisten Generator UI
- Service-Layer teilweise implementiert (UC8 noch TODO)

### Phase 4: Infrastruktur & Deployment
**Abhängigkeiten**: Alle UI-Komponenten komplett

- **TrueNAS Docker-Integration**:
  - Dockerfile für ASP.NET Core 8 Blazor
  - Docker Compose (App + SQLite)
  - TrueNAS Container Deployment
  - Networking + SSL/TLS (optional: reverse proxy)
  - Referenz: `diagrams/architecture-deployment.drawio` (teilweise vorhanden)

- **Performance-Tuning**:
  - Database Query Optimization (Indexing)
  - UI Rendering Performance (Blazor SSR vs Server)
  - Caching Strategy (Output Cache, Service Cache)
  - Load Testing (3 concurrent users requirement)
  - Monitoring & Logging (optional)

---

## 📁 WICHTIGE DATEIEN

**Agent-Konfiguration**:
- `CLAUDE.md` – Orchester-Workflow, Agenten-Definitionen, Git-Konventionen
- `claude/agents/*.md` – Individuelle Agent-Definitionen

**Dokumentation**:
- `docs/architecture-overview.html` – Projekt-Übersicht (master doc)
- `docs/features/UC*.html` – Pro Use-Case detailliert
- `diagrams/use-cases.drawio` – Status-Diagramm

**Code**:
- `src/App/Services/Classes/*.cs` – Business Logic (alle fertig)
- `src/App/Components/Pages/*/*.razor` – UI-Komponenten
- `src/Tests/` – Unit + Integration + bUnit-Tests

---

**Last Updated**: 2026-07-31 (Phase 1 UI-Test-Reparatur: 341/351 Tests grün, 10 Button-Tests verbleibend)  
**Current Branch**: `master` (mit Phase 1a/b/c Commits: Dev-Agent + Test-Agent Refactorings)  
**Ready For (nächste Session)**: 
- **Option 1 (GEWÄHLT)**: 10 verbleibende Button-Tests via Blazor Button-Komponenten-Refaktorierung
- Oder: WP4 UC4/UC6/UC10 UI-Entwicklung (wenn Button-Reparatur ausgesetzt wird)
