# UC4 Domain Model & Spezifikation – Validierungsbericht

**Datum**: 2026-06-30 (Session 11)  
**Status**: ✅ **COMPLETE & READY FOR TEST-AGENT**  
**Validiert von**: Doc-Agent  
**Nächster Schritt**: Test-Agent (TDD Red Phase)

---

## ✅ Validierungs-Checkliste

### 1. Domain Model Definition

#### Rezept Entity
- ✅ **Aggregate Root identifiziert**: Ja (mit Soft-Delete)
- ✅ **Alle Eigenschaften definiert**: Ja (10 Properties)
- ✅ **Constraints dokumentiert**: Ja (Name UNIQUE, Portionen 1-100, etc.)
- ✅ **Audit Trail**: Ja (CreatedAt, UpdatedAt, IsArchived)
- ✅ **Relationships klar**: Ja (1..N Zutaten)
- ✅ **Datenbankschema SQL**: Ja (mit CHECKs)

#### RezeptZutat Value Object
- ✅ **Value Object vs Entity korrekt identifiziert**: Ja
- ✅ **Alle Eigenschaften definiert**: Ja (8 Properties)
- ✅ **FK-Constraints dokumentiert**: Ja (ON DELETE CASCADE/RESTRICT)
- ✅ **Position für Ordering**: Ja (UNIQUE per Rezept)
- ✅ **Menge/Einheit Validierung**: Ja (Whitelists)
- ✅ **CreatedAt Timestamp**: Ja

### 2. Services Definition

#### RezeptService
- ✅ **CRUD-Methoden vollständig**: Ja (Get/GetAll/Search/Create/Update/Delete)
- ✅ **Async/Await Pattern**: Ja (alle Methoden async)
- ✅ **DTOs erwähnt**: Ja (RezeptDto, CreateRezeptRequest, UpdateRezeptRequest)
- ✅ **Validierungslogik dokumentiert**: Ja (5 Schritte gelistet)
- ✅ **Error-Handling**: Ja (ValidationException, DuplicateException, NotFoundException)

#### RezeptZutatService
- ✅ **Zutaten-Management Methoden**: Ja (Add/Update/Delete/Reorder)
- ✅ **Validierungslogik**: Ja (Rezept-Check, Lebensmittel-FK, Menge/Einheit)
- ✅ **Position Management**: Ja (Auto-Calculation + Reorder)
- ✅ **Error-Handling**: Ja (NotFoundException, ValidationException, InvalidOperation)
- ✅ **Minimum 1 Zutat Constraint**: Ja (dokumentiert)

#### NährwertCalculator (UC5-Integration)
- ✅ **Zwei Methoden**: Ja (CalculateRecipeNährwerte + CalculateProPortion)
- ✅ **Summation Logik**: Ja (mit Skalierungsfaktor Menge/100)
- ✅ **Dezimal Rounding**: Ja (erwähnt)
- ✅ **Edge Cases**: Ja (Missing Nährwerte, unterschiedliche Einheiten)
- ✅ **Abhängigkeiten klar**: Ja (RezeptService, RezeptZutatService, NährwertService)

### 3. Use-Case Spezifikation

#### UC4.1 – Rezept erstellen
- ✅ **Happy Path**: Ja (vollständig mit 7 Schritten)
- ✅ **Fehlerfall 1 – Duplikat-Name**: Ja
- ✅ **Fehlerfall 2 – Ungültige Portionen**: Ja
- ✅ **Fehlerfall 3 – Ungültige Zubereitungszeit**: Ja
- ✅ **Fehlerfall 4 – Ungültiger Schwierigkeitsgrad**: Ja
- ✅ **Fehlerfall 5 – Lebensmittel nicht vorhanden**: Ja (FK)
- ✅ **Fehlerfall 6 – Ungültige Zutat-Menge**: Ja
- ✅ **Fehlerfall 7 – Ungültige Einheit**: Ja
- ✅ **Fehlerfall 8 – Keine Zutaten**: Ja (Minimum 1)

#### UC4.2 – Rezept bearbeiten
- ✅ **Happy Path (Metadaten)**: Ja
- ✅ **Happy Path (Zutaten)**: Ja (mit Reordering)
- ✅ **Fehlerfall 1 – Neuer Name ist Duplikat**: Ja
- ✅ **Fehlerfall 2 – Rezept ist archiviert**: Ja

#### UC4.3 – Rezept löschen
- ✅ **Soft-Delete Semantik**: Ja (IsArchived=true)
- ✅ **Verhalten nach Löschung**: Ja (nicht in GetAll, GetById=null)
- ✅ **Zutaten bleiben**: Ja (erwähnt)
- ✅ **Fehlerfall 1 – Rezept nicht existiert**: Ja
- ✅ **Fehlerfall 2 – Bereits archiviert**: Ja (idempotent)

#### UC4.4 – Rezept-Nährwerte anzeigen
- ✅ **Happy Path**: Ja (mit Gesamtwert + Pro-Portion)
- ✅ **Fehlerfall 1 – Keine Zutaten**: Ja (graceful)
- ✅ **Fehlerfall 2 – Keine Nährwertdaten**: Ja (graceful)
- ✅ **Fehlerfall 3 – Manche Zutaten mit/ohne Nährwerte**: Ja (Skipping)

### 4. UML-Diagramme Planung

- ✅ **Class Diagram**: Rezept + RezeptZutat + Services geplant
- ✅ **ER-Diagramm Update**: Neue Tabellen + Relationships dokumentiert
- ✅ **Sequence Diagrams**: CreateRecipe, AddIngredient, CalculateNutrition geplant
- ✅ **Component Diagram**: Frontend/Backend/DB Update geplant
- ✅ **Alle Diagramme als draw.io**: Ja (format konsistent)

### 5. Test-Struktur

#### RezeptServiceTests (~30 Tests)
- ✅ **GetRezeptByIdAsync**: 3 Tests (Valid, Invalid, Archived)
- ✅ **GetAllRezepteAsync**: 2 Tests (All, Exclude Archived)
- ✅ **SearchRezepteAsync**: 3 Tests (Fuzzy, Empty, Case-Insensitive)
- ✅ **CreateRezeptAsync**: 13 Tests (Valid + 10 Validierungen)
- ✅ **UpdateRezeptAsync**: 8 Tests (Valid + 5 Fehlerfälle)
- ✅ **DeleteRezeptAsync**: 3 Tests (Valid + Fehlerfälle)
- ✅ **RezeptExistsAsync**: 2 Tests

#### RezeptZutatServiceTests (~25 Tests)
- ✅ **GetZutatenAsync**: 2 Tests
- ✅ **GetZutatAsync**: 3 Tests
- ✅ **AddZutatAsync**: 12 Tests (Valid + 9 Validierungen)
- ✅ **UpdateZutatAsync**: 6 Tests
- ✅ **DeleteZutatAsync**: 3 Tests
- ✅ **ReorderZutatenAsync**: 4+ Tests
- ✅ **GetZutatCountAsync**: 1 Test

#### NährwertCalculatorTests (~12 Tests)
- ✅ **CalculateRezeptNährwerteAsync**: 8 Tests
- ✅ **CalculateProPortionAsync**: 3 Tests
- ✅ **Edge Cases**: 6 Tests (Minimum/Maximum Menge, Einheiten, Dezimalwerte)

### 6. Abhängigkeits-Validierung

- ✅ **UC1 (LebensmittelKatalog)**: ✅ FERTIG – FK vorhanden
- ✅ **UC3 (Nährwerte)**: ✅ FERTIG – NährwertCalculator nutzt es
- ✅ **UC5 (Nährwertberechnung)**: 🔵 Diese Phase (UC4 ist Dependency für UC5)
- ✅ **Keine fehlenden Dependencies**: Korrekt

### 7. Validierungs-Zusammenfassung

| Aspekt | Status | Notizen |
|--------|--------|---------|
| Domain Model | ✅ Complete | Rezept + RezeptZutat mit allen Constraints |
| Services | ✅ Complete | 3 Services, alle Methoden definiert |
| Use-Cases | ✅ Complete | UC4.1-UC4.4 mit Happy Path + Fehlerfälle |
| UML-Diagramme | ✅ Geplant | Class + ER + Sequence + Component |
| Test-Struktur | ✅ Complete | ~40-50 Tests, alle Kategorien gelistet |
| Abhängigkeiten | ✅ OK | UC1 + UC3 vorhanden, UC5 folgt später |
| Validierungslogik | ✅ Korrekt | Reihenfolge beachtet (Duplikat VOR FK-Check) |
| Soft-Delete | ✅ Konsistent | Wie UC3 (IsArchived Flag) |
| Error-Handling | ✅ Korrekt | ValidationException, NotFoundException, etc. |

---

## 📊 Geschätzte Test-Anzahl

```
RezeptServiceTests:        ~30 Tests ✅
RezeptZutatServiceTests:   ~25 Tests ✅
NährwertCalculatorTests:   ~12 Tests ✅
─────────────────────────────────────
GESAMT UC4:                ~40-50 Tests ✅ (größer als UC3 mit 23 Tests)
```

---

## 🎯 Handover an Test-Agent

### Was der Test-Agent JETZT tun soll:

1. **Datei lesen**: `docs/uc4-domain-model.md` (vollständige Spezifikation)
2. **Tests schreiben** (TDD Red Phase):
   - `src/Tests/Unit/Services/RezeptServiceTests.cs` (~30 Tests)
   - `src/Tests/Unit/Services/RezeptZutatServiceTests.cs` (~25 Tests)
   - `src/Tests/Unit/Services/NährwertCalculatorTests.cs` (~12 Tests)
3. **Test-Struktur**: siehe Abschnitt 5 der Domain Model Datei
4. **Framework**: xUnit + Moq (wie UC1-UC3)
5. **Assertion Library**: FluentAssertions (falls vorhanden)
6. **Target**: Alle Tests absichtlich ROT (NotImplementedException in Tests OK!)

### Key Points für Test-Agent:

- ✅ Tests sind **Spezifikation** – beschreiben was Code tun soll
- ✅ **Fehlerfälle sind wichtig**: Validierungslogik muss getestet sein
- ✅ **Soft-Delete Semantik**: IsArchived=true, aber nicht in GetAll/GetById
- ✅ **FK-Constraints**: LebensmittelId muss existieren, sonst NotFoundException
- ✅ **Unique Name**: Duplikat-Name soll DuplicateRezeptException werfen
- ✅ **Minimum 1 Zutat**: Rezept ohne Zutaten ist ungültig
- ✅ **Position Management**: Auto-Berechnung + Reordering
- ✅ **Nährwertberechnung**: Summation mit Skalierungsfaktor (Menge/100)

---

## ✅ Definition-of-Done für Doc-Agent UC4 Phase 1

- ✅ UC4 Domain Model dokumentiert (Rezept + RezeptZutat)
- ✅ Services & Abhängigkeiten spezifiziert (RezeptService + RezeptZutatService + NährwertCalculator)
- ✅ Use-Cases UC4.1-UC4.4 vollständig (Happy Path + Fehlerfälle)
- ✅ UML-Diagramme geplant (Class + ER + Sequence + Component)
- ✅ Test-Struktur für Test-Agent vorbereitet (~40-50 Tests)
- ✅ Abhängigkeiten validiert (UC1 + UC3 OK)
- ✅ Validierungslogik dokumentiert (mit Reihenfolge)
- ✅ Class-Diagram erstellt (diagrams/architecture-class-uc4.drawio)
- ✅ ER-Diagramm Update erstellt (diagrams/database-schema-uc4-update.drawio)
- ✅ Validierungsbericht geschrieben (diese Datei)

---

**Status**: 🟢 **UC4 PHASE 1 COMPLETE & VALIDATED**

**Blockies?**: KEINE 🚀 Test-Agent kann sofort starten!

**Nächster Schritt**: Test-Agent → UC4 Tests (TDD Red Phase)
