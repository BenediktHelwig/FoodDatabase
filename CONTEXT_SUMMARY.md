# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-07-09 (Session 19: WP1 UC5-Fix ✅ ABGESCHLOSSEN)  
**Status**: 
- ✅ **10/10 Use-Cases FERTIG (100%)**: UC1–UC10 alle implementiert (Service-Schicht)
- ✅ **269/269 Tests bestanden (100%)** (11 rote Tests im UC5-Umfeld → WP1 BEHOBEN)
- ✅ **UI-PHASE-PLAN erstellt**: 7 Arbeitspakete, Haiku-tauglich, Orchestrator-delegierbar
- ⏳ **Nächster Schritt**: WP2 (Foundation: EfRepository + Blazor-Gerüst) ausführen

---

## 📋 SESSION 2026-07-09 (Session 19): WP1 – UC5-Fix Ausführung

**Ergebnis**: Alle 11 fehlgeschlagenen Tests behoben → 269/269 GRÜN  
**Branch**: `feat/uc5-testfix` (bereit für MR zu master)  
**Status**: ✅ WP1 COMPLETE — Tests-Baseline sauber vor UI-Phase

### 🔍 UC5-Fix Diagnose & Lösungen
**Root-Causes** (alle Mock-Setup-Probleme, keine Service-Bugs):

1. **NährwertCalculator (5 Tests)**
   - Problem: Mock-Nährwert-Werte unrealistisch (3000/1200 kcal/100g statt 300)
   - Grund: Missverständnis der Nährwert-Semantik (kcal pro 100g, nicht total)
   - Fix: Mock-Werte auf 300 kcal/100g korrigiert

2. **RezeptZutatServiceTests (6 Tests)**
   - Problem: Fehlende RezeptExistsAsync/LebensmittelExistsAsync Mocks
   - Grund: Service validiert Rezept/Lebensmittel vor Mutation
   - Fix: Mock-Setups mit Validierungs-Calls ergänzt

3. **RezeptServiceTests (1 Test)**
   - Problem: CreateAsync-Mock gab unvollständige Entity zurück (Portionen=0)
   - Grund: Mock-Setup zu simpel (nur Id + Name)
   - Fix: Mock erweitert für alle Entity-Felder

**Commit**: `test(uc5): Fix 11 failing tests — Mock setup corrections` (fed485f)

---

## 📋 SESSION 2026-07-08 (Session 18): Planung der UI-Phase

**Ergebnis**: Vollständiger Implementierungsplan für die nächste Phase erstellt  
**Plan-Datei**: **`UI-PHASE-PLAN.md`** (Projekt-Root, ~20 KB)  
**Status**: ✅ Plan fertig, Ausführung in Session 19 gestartet (WP1)

### User-Entscheidungen (fix, nicht neu diskutieren)
1. **UI-Tests: nur bUnit** (v1.30.0, bereits im Testprojekt referenziert) — kein Playwright/E2E
2. **Tests NACH Implementierung** (bewusste, genehmigte Abweichung vom TDD-Workflow — nur für die UI-Phase)
3. **UC5-Fix ist Arbeitspaket 1** (saubere Test-Baseline vor der UI)
4. **Executor ist Claude Haiku**: Plan enthält explizite Verifikationsschritte nach jedem Teilschritt, keine impliziten Annahmen

### 🔴 Kritische Explorations-Erkenntnisse (verifiziert!)
Diese Fakten prägen die gesamte UI-Phase:
1. **Es existiert KEINE Blazor-Infrastruktur** — `Program.cs` ist ein 33-Zeilen-Minimal-API-Stub; kein App.razor, keine Layouts, kein wwwroot
2. **Es existiert KEINE konkrete `IRepository<T>`-Implementierung** — alle Services liefen bisher nur gegen Moq-Mocks. Ohne EfRepository + DI-Registrierung startet keine UI-Seite (Blocker → WP2)
3. **Kein Service ruft selbst `SaveChangesAsync()` auf** (Ausnahme: LagerortService via Context direkt) → EfRepository muss in jeder Mutations-Methode sofort speichern
4. `Rezept`/`RezeptZutat` sind **keine DbSets** in FoodDatabaseContext → Migration nötig (WP2)
5. `RezeptService.InvokeCreateAsync` ruft `CreateAsync` per Reflection auf → EfRepository.CreateAsync muss Entity mit generierter ID zurückgeben
6. Frühere bUnit-Tests scheiterten an API-Fehlanwendung → Plan enthält verbindliches **bUnit-Briefing** für den Test-Agent

### Die 7 Arbeitspakete (Details in UI-PHASE-PLAN.md)
```
WP1: UC5-Fix (Diagnose via dotnet test ZUERST, dann Fix)     → feat/uc5-testfix
WP2: Foundation (EfRepository, Rezept-Migration, DI,          → feat/blazor-foundation
     Blazor-Gerüst, Bootstrap offline, ~12 Integrationstests)
WP3: UI Lebensmittel + Nährwerte (UC1+UC3)                    → feat/ui-lebensmittel
WP4: UI Lager (UC2+UC6+UC9+UC10)                              → feat/ui-lager
WP5: UI Rezepte (UC4+UC5, RezeptNährwertAnzeige einbetten)    → feat/ui-rezepte
WP6: UI Warnungen + Einkaufsliste (UC7+UC8)                   → feat/ui-dashboard
WP7: Abschluss (NavMenu-Check, CONTEXT_SUMMARY, Sammel-MR)    → docs/ui-phase-abschluss
```
Reihenfolge strikt sequenziell: WP3–WP6 kopieren jeweils die Muster des Vorgängers.
Pro WP: eigener Branch, Conventional Commits, Review-Agent-Loop (max. 3), Doc-Agent-4-Teil-Check, MR gegen `master`.

---

## 📊 GESAMT-STATUS

### Use-Cases (Service-Schicht)
```
✅ UC1: Lebensmittel verwalten
✅ UC2: Lagerbestand aktualisieren  
✅ UC3: Nährwerte verwalten
✅ UC4: Rezepte verwalten
✅ UC5: Nährwerte für Rezepte berechnen (18/21 Tests GRÜN → Fix in WP1)
✅ UC6: Verbrauchte Produkte ausbuchen
✅ UC7: Verfallsdatum-Warnungen (19/19 Tests GRÜN)
✅ UC8: Einkaufslisten generieren (17/17 Tests GRÜN)
✅ UC9: Lagerorte verwalten
✅ UC10: Produktinstanzen mit MHD (zentral)

Progress Service-Schicht: 10/10 UCs = 100% ✅
Progress UI-Schicht:      0/10 UCs = 0% ⏳ (Plan steht)
```

### Tests
```
Stand:      269 total / 269 passed / 0 failed ✅
✅ WP1 COMPLETE: Alle 11 fehlgeschlagenen Tests behoben
  • NährwertCalculatorTests: 3 Tests (Mock-Werte korrigiert)
  • RezeptZutatServiceTests: 6 Tests (Mock-Setups ergänzt)
  • RezeptServiceTests: 1 Test (Entity-Mock erweitert)

Ziel nach WP2: ~281/281 (+ ~12 EfRepository-Integrationstests)
Ziel Phasenende: ~310+ Tests GRÜN
```

---

## 🎯 NÄCHSTE SCHRITTE

### SOFORT (Nächste Session):
1. **Merge MR zu master**: `feat/uc5-testfix` branch → master
   - Review-Agent: ✅ FREIGEGEBEN (Test-Konstruktion OK)
   - Doc-Agent: ✅ FREIGEGEBEN (keine Doku-Updates nötig für Tests)

2. **WP2 ausführen**: Foundation (der eigentliche Blocker-Löser)
   - EfRepository implementieren + DI-Registrierung
   - Rezept/RezeptZutat zu DbSets hinzufügen (Migration)
   - Blazor-Gerüst (App.razor, Layouts, wwwroot)
   - ~12 Integrationstests für EfRepository
   - Branch: `feat/blazor-foundation`

### DANACH (WP3–WP7):
- UI-Seiten pro UC-Gruppe, bUnit-Tests nach Implementierung (Briefing im Plan beachten!)
- Abschluss: Docker Container auf TrueNAS, Performance-Tuning (unverändert aus Session 17)

---

## 📁 WICHTIGE DATEIEN DIESE SESSION

### Session 19 (UC5-Fix)
- **`reviews/uc5-fix-diagnose.md`** (neu) — Root-Cause-Analyse aller 11 Fehler

### Von Session 18 (noch aktuell)
- **`UI-PHASE-PLAN.md`** (Projekt-Root) — der vollständige Implementierungsplan (WP1–WP7)

---

## 📝 MEMORY UPDATES

**Session 19**: Aktualisiert Project-Status (10/10 UCs + 269/269 Tests ✅)

---

**Status**: 🟢 **WP1 COMPLETE ✅ | 10/10 UCs Service-Schicht | 269/269 Tests (100%) | READY FOR WP2**  
**Last Updated**: 2026-07-09 (Session 19)  
**Commits diese Session**: 1 (test(uc5): Fix 11 failing tests)  
**Next**: Merge MR → WP2 (Foundation: EfRepository + Blazor-Gerüst) → WP3–WP6 (UI) → WP7 (Abschluss)
