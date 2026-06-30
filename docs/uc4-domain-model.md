# UC4 (Rezepte verwalten) – Domain Model & Spezifikation

**Datum**: 2026-06-30 (Session 11)  
**Status**: 🔵 DOMAIN MODEL & DIAGRAMM-PLANUNG COMPLETE  
**Nächster Schritt**: Test-Agent startet TDD Red-Phase  
**Geschätzte Tests**: 40-50 Tests

---

## 1. Domain Model (Entities)

### 1.1 Rezept (Aggregate Root)

**Zweck**: Hauptentität für Rezepte mit Metadaten und Zutaten-Management

**Properties**:

```csharp
public class Rezept
{
    public int Id { get; set; }                                    // PK
    
    // Rezept-Metadaten
    public string Name { get; set; }                              // z.B. "Spaghetti Carbonara"
    public string? Beschreibung { get; set; }                     // Optional (z.B. "Italienische Klassiker")
    public string? Zubereitung { get; set; }                      // Optional (z.B. "Nudeln kochen, Ei schlagen, ...")
    
    // Portionen & Zeit
    public int Portionen { get; set; }                            // z.B. 4 (Anzahl Portionen)
    public int ZubereitungszeitMinuten { get; set; }             // z.B. 20 (in Minuten)
    
    // Schwierigkeitsgrad
    public string Schwierigkeitsgrad { get; set; }               // "Easy", "Medium", "Hard"
    
    // Zutaten-Management
    public ICollection<RezeptZutat> Zutaten { get; set; }       // 1..N Zutaten (Lazy Loading)
    
    // Audit Trail
    public DateTime CreatedAt { get; set; }                       // UTC Zeitstempel
    public DateTime UpdatedAt { get; set; }                       // UTC (aktualisiert bei jedem Update)
    
    // Soft-Delete
    public bool IsArchived { get; set; }                          // false = aktiv, true = gelöscht
}
```

**Constraints**:
- ✅ **Name**: Eindeutig (UNIQUE), max. 200 Zeichen, nicht null
- ✅ **Portionen**: Min. 1, Max. 100 (Validierung in Service)
- ✅ **ZubereitungszeitMinuten**: Min. 0, Max. 1440 (24h in Minuten)
- ✅ **Schwierigkeitsgrad**: Enum-like ("Easy", "Medium", "Hard") - Validierung in Service
- ✅ **Zutaten**: Min. 1 erforderlich (Empty-Check in Service)
- ✅ **IsArchived**: Soft-Delete Flag (wie UC3)

**Datenbankschema**:
```sql
CREATE TABLE Rezept (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL UNIQUE,
    Beschreibung TEXT,
    Zubereitung TEXT,
    Portionen INTEGER NOT NULL CHECK (Portionen >= 1 AND Portionen <= 100),
    ZubereitungszeitMinuten INTEGER NOT NULL CHECK (ZubereitungszeitMinuten >= 0),
    Schwierigkeitsgrad TEXT NOT NULL CHECK (Schwierigkeitsgrad IN ('Easy', 'Medium', 'Hard')),
    CreatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    UpdatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    IsArchived INTEGER NOT NULL DEFAULT 0
);
```

---

### 1.2 RezeptZutat (Value Object in Rezept-Aggregate)

**Zweck**: Zutat mit Menge + Einheit, Teil des Rezept-Aggregates

**Properties**:

```csharp
public class RezeptZutat
{
    public int Id { get; set; }                                    // PK (für DB-Abfragen)
    
    // Fremdschlüssel
    public int RezeptId { get; set; }                             // FK → Rezept
    public int LebensmittelId { get; set; }                       // FK → LebensmittelKatalog
    
    // Navigation Properties (EF Core)
    public Rezept Rezept { get; set; }                            // Bidirektionale Beziehung
    public LebensmittelKatalog Lebensmittel { get; set; }        // FK-Lazy-Loading
    
    // Zutat-Spezifikation
    public double Menge { get; set; }                             // z.B. 400.0
    public string Einheit { get; set; }                           // z.B. "g", "ml", "Stück", "TL", "EL"
    public string? Notizen { get; set; }                          // Optional (z.B. "gehackt", "klein geschnitten")
    public int Position { get; set; }                             // Reihenfolge in Zutatenliste (0-based)
    
    // Audit Trail
    public DateTime CreatedAt { get; set; }                        // UTC Zeitstempel
}
```

**Constraints**:
- ✅ **RezeptId**: FK zu Rezept, ON DELETE CASCADE (Zutaten werden gelöscht wenn Rezept gelöscht)
- ✅ **LebensmittelId**: FK zu LebensmittelKatalog, ON DELETE RESTRICT (Zutaten-Integrität!)
- ✅ **Menge**: Min. 0.01, Max. 10000 (z.B. 0.5 TL, aber auch 5000g möglich)
  - Validierung in Service: `if (Menge <= 0 || Menge > 10000) throw ValidationException`
- ✅ **Einheit**: Whitelist ("g", "ml", "Stück", "TL", "EL", "Tasse", "Prise") - Enum-like
- ✅ **Position**: 0-basiert, eindeutig pro Rezept (wichtig für Sortierung!)
- ✅ **Eindeutigkeits-Constraint**: Ein Lebensmittel darf max. 1x mit gleicher Position in einem Rezept vorkommen
  - Aber: Same Lebensmittel mit unterschiedlichen Mengen/Notizen = erlaubt (z.B. "400g Spaghetti" + "200g Spaghetti-Wasser")

**Datenbankschema**:
```sql
CREATE TABLE RezeptZutat (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    RezeptId INTEGER NOT NULL,
    LebensmittelId INTEGER NOT NULL,
    Menge REAL NOT NULL CHECK (Menge > 0 AND Menge <= 10000),
    Einheit TEXT NOT NULL CHECK (Einheit IN ('g', 'ml', 'Stück', 'TL', 'EL', 'Tasse', 'Prise')),
    Notizen TEXT,
    Position INTEGER NOT NULL CHECK (Position >= 0),
    CreatedAt TEXT NOT NULL,  -- ISO 8601 UTC
    FOREIGN KEY (RezeptId) REFERENCES Rezept(Id) ON DELETE CASCADE,
    FOREIGN KEY (LebensmittelId) REFERENCES LebensmittelKatalog(Id) ON DELETE RESTRICT,
    UNIQUE (RezeptId, Position)  -- Eine Position pro Rezept
);
```

---

## 2. Services & Abhängigkeiten

### 2.1 RezeptService

**Verantwortung**: CRUD-Operationen für Rezepte (ohne Zutaten-Management)

**Schnittstelle**:

```csharp
public interface IRezeptService
{
    // Read
    Task<RezeptDto?> GetRezeptByIdAsync(int rezeptId);           // Null wenn nicht existiert oder archiviert
    Task<IEnumerable<RezeptDto>> GetAllRezepteAsync();            // Nur nicht-archivierte
    Task<IEnumerable<RezeptDto>> SearchRezepteAsync(string name); // Fuzzy-Search (contains)
    
    // Create
    Task<RezeptDto> CreateRezeptAsync(CreateRezeptRequest request); // Validierung im Service
    
    // Update
    Task<RezeptDto> UpdateRezeptAsync(int rezeptId, UpdateRezeptRequest request);
    
    // Delete (Soft-Delete)
    Task DeleteRezeptAsync(int rezeptId);                         // Setzt IsArchived=true
    
    // Helper
    Task<bool> RezeptExistsAsync(int rezeptId);                  // Prüft ob nicht-archiviert existiert
}
```

**Abhängigkeiten**:
- `IRezeptRepository` (für Datenbank-Zugriff)
- `ILebensmittelService` (für FK-Validierung)
- `ILogger<RezeptService>`

**Validierungslogik** (in dieser Reihenfolge):

```
CreateRezeptAsync:
  1. Null-Check: Name != null
  2. Format-Check: Name.Length > 0 && Name.Length <= 200
  3. Eindeutigkeits-Check: !ExistsWithNameAsync(name, isArchived=false)
  4. Bereichs-Check: Portionen >= 1 && Portionen <= 100
  5. Bereichs-Check: ZubereitungszeitMinuten >= 0 && ZubereitungszeitMinuten <= 1440
  6. Enum-Check: Schwierigkeitsgrad IN ('Easy', 'Medium', 'Hard')
  7. Success: CreateAsync
```

**Error-Handling**:
- `ValidationException` bei Format/Range/Enum-Verletzungen
- `DuplicateRezeptException` bei Duplikat-Name
- `NotFoundException` bei RezeptId nicht existiert (Update)

---

### 2.2 RezeptZutatService

**Verantwortung**: Management von Zutaten innerhalb eines Rezepts

**Schnittstelle**:

```csharp
public interface IRezeptZutatService
{
    // Read
    Task<IEnumerable<RezeptZutatDto>> GetZutatenAsync(int rezeptId);  // Sortiert nach Position
    Task<RezeptZutatDto?> GetZutatAsync(int rezeptId, int zutatId);   // Single Zutat
    
    // Create
    Task<RezeptZutatDto> AddZutatAsync(int rezeptId, AddZutatRequest request);
    
    // Update
    Task<RezeptZutatDto> UpdateZutatAsync(int rezeptId, int zutatId, UpdateZutatRequest request);
    
    // Delete
    Task DeleteZutatAsync(int rezeptId, int zutatId);
    
    // Reorder
    Task<IEnumerable<RezeptZutatDto>> ReorderZutatenAsync(int rezeptId, List<int> zutatIds); // Neue Positions-Reihenfolge
    
    // Helper
    Task<int> GetZutatCountAsync(int rezeptId);                  // Anzahl non-deleted Zutaten
}
```

**Abhängigkeiten**:
- `IRezeptRepository` (Rezept + Zutaten abrufen)
- `ILebensmittelService` (FK-Validierung)
- `ILogger<RezeptZutatService>`

**Validierungslogik** (in dieser Reihenfolge):

```
AddZutatAsync:
  1. Rezept-Existenz-Check: RezeptExists(rezeptId) && !IsArchived
  2. Lebensmittel-Existenz-Check: LebensmittelExists(lebensmittelId)
  3. Menge-Check: Menge > 0 && Menge <= 10000
  4. Einheit-Check: Einheit IN ('g', 'ml', 'Stück', 'TL', 'EL', 'Tasse', 'Prise')
  5. Leer-Check: Rezept hat schon Zutaten? (nicht für AddZutat, aber später minimum-check)
  6. Position-Check: Neue Position = CurrentMaxPosition + 1
  7. Success: CreateAsync
```

**Error-Handling**:
- `NotFoundException` bei RezeptId nicht existiert
- `NotFoundException` bei LebensmittelId nicht existiert (FK-Constraint!)
- `ValidationException` bei Menge/Einheit-Verletzungen
- `InvalidOperationException` bei Rezept-Soft-Delete

---

### 2.3 NährwertCalculator (Service für UC5-Integration)

**Verantwortung**: Berechnung von Gesamtnährwerten für Rezepte

**Schnittstelle**:

```csharp
public interface INährwertCalculator
{
    // Berechnung
    Task<RezeptNährwerteDto> CalculateRezeptNährwerteAsync(int rezeptId);
    // Returns: Gesamt-Nährwerte + Pro-Portion Nährwerte
    
    Task<NährwerteProPortion> CalculateProPortionAsync(int rezeptId);
    // Returns: Alle Nährwerte / Portionen
}
```

**Pseudocode**:

```csharp
public async Task<RezeptNährwerteDto> CalculateRezeptNährwerteAsync(int rezeptId)
{
    // 1. Rezept laden
    var rezept = await rezeptService.GetRezeptByIdAsync(rezeptId);
    if (rezept == null) throw NotFoundException;
    
    // 2. Alle Zutaten laden
    var zutaten = await rezeptZutatService.GetZutatenAsync(rezeptId);
    
    // 3. Pro Zutat: Nährwert laden + skalieren
    var totalKalorien = 0;
    var totalFett = 0.0;
    var totalKohlenhydrate = 0.0;
    // ... etc
    
    foreach (var zutat in zutaten)
    {
        // Nährwert für Lebensmittel (pro 100g/ml)
        var nährwert = await nährwertService.GetNährwertByLebensmittelIdAsync(zutat.LebensmittelId);
        if (nährwert == null) continue; // Lebensmittel hat keine Nährwerte
        
        // Skalierung berechnen: Zutat.Menge / StandardMenge(100)
        var scaleFactor = ConvertToGramm(zutat.Menge, zutat.Einheit) / 100.0;
        
        // Zu Gesamtnährwerten addieren
        totalKalorien += (int)(nährwert.Kalorien * scaleFactor);
        totalFett += nährwert.Fett * scaleFactor;
        // ... etc
    }
    
    // 4. Pro Portion berechnen
    var proPortionKalorien = totalKalorien / rezept.Portionen;
    // ... etc
    
    // 5. Zurückgeben
    return new RezeptNährwerteDto
    {
        RezeptId = rezeptId,
        Gesamtnährwerte = new NährwerteDto { ... },
        ProPortionNährwerte = new NährwerteDto { ... }
    };
}
```

**Abhängigkeiten**:
- `IRezeptService` (Rezept laden)
- `IRezeptZutatService` (Zutaten laden)
- `INährwertService` (Nährwerte pro Lebensmittel)
- `IEinheitConverter` (g/ml Konvertierung)

**Edge-Cases**:
- ✅ Lebensmittel hat keine Nährwerte: Skip (nicht mitzählen)
- ✅ Einheit ist "Stück" / "TL" / "EL": Konvertierung zu Gramm (ca. 5g pro TL, 15g pro EL, etc.)
- ✅ Dezimalergebnis: Runden auf 1 Dezimalstelle (z.B. 123.5g Fett)

---

## 3. Use-Case Spezifikation

### UC4.1 – Rezept erstellen

**Akteur**: Benutzer  
**Vorbedingung**: -  
**Nachbedingung**: Rezept existiert mit mindestens 1 Zutat

**Happy Path**:
1. Benutzer öffnet "Neues Rezept" Dialog
2. Gibt ein:
   - Name: "Spaghetti Carbonara"
   - Portionen: 4
   - Zubereitungszeit: 20 (Minuten)
   - Schwierigkeitsgrad: "Medium"
   - Beschreibung: "Italienische Klassiker" (optional)
   - Zubereitung: "1. Wasser aufkochen..." (optional)
3. Klick: "Zutaten hinzufügen"
4. Erste Zutat hinzufügen:
   - Lebensmittel: "Spaghetti" aus Katalog
   - Menge: 400
   - Einheit: "g"
   - Notizen: "Hartweizen" (optional)
5. Zweite Zutat hinzufügen (analog)
6. Klick: "Rezept speichern"
7. **Result**: Rezept mit 2 Zutaten erstellt, ID zurückgegeben, UI aktualisiert

**Fehlerfall 1: Duplikat-Name**
- Benutzer gibt Name ein, der bereits existiert
- **Error**: ValidationException → Benutzer sieht "Rezept mit diesem Namen existiert bereits"

**Fehlerfall 2: Ungültige Portionen**
- Benutzer gibt Portionen=0 oder Portionen=200 ein
- **Error**: ValidationException → Benutzer sieht "Portionen müssen zwischen 1 und 100 liegen"

**Fehlerfall 3: Ungültige Zubereitungszeit**
- Benutzer gibt ZubereitungszeitMinuten=-5 ein
- **Error**: ValidationException → Benutzer sieht "Zubereitungszeit darf nicht negativ sein"

**Fehlerfall 4: Ungültiger Schwierigkeitsgrad**
- Benutzer gibt Schwierigkeitsgrad="Ultrahard" ein
- **Error**: ValidationException → Benutzer sieht "Schwierigkeitsgrad muss Easy/Medium/Hard sein"

**Fehlerfall 5: Lebensmittel nicht vorhanden**
- Benutzer versucht, nicht-existierende LebensmittelId hinzuzufügen
- **Error**: NotFoundException → Benutzer sieht "Lebensmittel existiert nicht"

**Fehlerfall 6: Ungültige Zutat-Menge**
- Benutzer gibt Menge=0 oder Menge=50000 ein
- **Error**: ValidationException → Benutzer sieht "Menge muss zwischen 0.01 und 10000 liegen"

**Fehlerfall 7: Ungültige Einheit**
- Benutzer gibt Einheit="Kanister" ein (nicht in Whitelist)
- **Error**: ValidationException → Benutzer sieht "Einheit muss g/ml/Stück/TL/EL/Tasse/Prise sein"

**Fehlerfall 8: Keine Zutaten hinzugefügt**
- Benutzer klickt "Rezept speichern" ohne Zutaten
- **Error**: ValidationException → Benutzer sieht "Rezept muss mindestens 1 Zutat haben"

---

### UC4.2 – Rezept bearbeiten

**Akteur**: Benutzer  
**Vorbedingung**: Rezept existiert und ist nicht archiviert  
**Nachbedingung**: Rezept-Metadaten und/oder Zutaten aktualisiert

**Happy Path (Metadaten)**:
1. Benutzer öffnet Rezept "Spaghetti Carbonara"
2. Klick: "Bearbeiten"
3. Ändert:
   - Name: "Spaghetti alla Carbonara" (neu)
   - Portionen: 6 (statt 4)
   - Zubereitungszeit: 25 (statt 20)
4. Klick: "Speichern"
5. **Result**: Rezept-Metadaten aktualisiert, UpdatedAt Timestamp gesetzt

**Happy Path (Zutaten)**:
1. Benutzer öffnet Rezept
2. Klick: "Zutaten bearbeiten"
3. Ändert Zutat 1:
   - Menge: 500g (statt 400g)
4. Löscht Zutat 2 (z.B. "Guanciale")
5. Fügt neue Zutat hinzu: "Ei (large)" × 3 Stück
6. Reorder: Zutaten in neue Reihenfolge ziehen
7. Klick: "Speichern"
8. **Result**: Zutaten reordered (neue Position), Metadaten aktualisiert

**Fehlerfall 1: Neuer Name ist Duplikat**
- Benutzer ändert Name zu existierendem anderen Rezept
- **Error**: DuplicateRezeptException

**Fehlerfall 2: Rezept ist archiviert**
- Benutzer versucht, archiviertesRezept zu bearbeiten
- **Error**: InvalidOperationException → "Archivierte Rezepte können nicht bearbeitet werden"

---

### UC4.3 – Rezept löschen (Soft-Delete)

**Akteur**: Benutzer  
**Vorbedingung**: Rezept existiert  
**Nachbedingung**: IsArchived=true, Zutaten bleiben (ON DELETE CASCADE wird NICHT ausgelöst!)

**Happy Path**:
1. Benutzer öffnet Rezept "Spaghetti Carbonara"
2. Klick: "Löschen"
3. Bestätigung: "Wirklich löschen?"
4. Klick: "Ja, löschen"
5. **Result**: IsArchived=true, Rezept in Listen nicht mehr sichtbar

**Verhalten nach Löschung**:
- ✅ Rezept nicht in GetAllRezepteAsync() (nur nicht-archivierte)
- ✅ GetRezeptByIdAsync(id) → null (soft-deleted)
- ✅ Zutaten bleiben in DB (für Referenzintegrität)
- ✅ Später Restore möglich (optional, für v1.0 nicht gefordert)

**Fehlerfall 1: Rezept nicht existiert**
- **Error**: NotFoundException

**Fehlerfall 2: Rezept ist bereits archiviert**
- **Error**: InvalidOperationException oder Silent Success (idempotent)

---

### UC4.4 – Rezept-Nährwerte anzeigen (UC5-Preview)

**Akteur**: Benutzer  
**Vorbedingung**: Rezept existiert mit mindestens 1 Zutat mit Nährwerte  
**Nachbedingung**: -

**Happy Path**:
1. Benutzer öffnet Rezept "Spaghetti Carbonara"
2. Klick: "Nährwerte anzeigen"
3. System berechnet:
   - Gesamtnährwerte (alle Zutaten summiert)
   - Pro-Portion Nährwerte (Gesamt / Portionen)
4. UI zeigt:
   ```
   Spaghetti Carbonara (4 Portionen)
   
   Gesamtnährwerte:
   - Kalorien: 1200 kcal
   - Fett: 45.0g
   - Kohlenhydrate: 120.0g
   - Protein: 48.0g
   
   Pro Portion:
   - Kalorien: 300 kcal
   - Fett: 11.3g
   - Kohlenhydrate: 30.0g
   - Protein: 12.0g
   ```
5. **Result**: Benutzer sieht vollständige Nährwertinformation

**Fehlerfall 1: Rezept hat keine Zutaten**
- **Result**: "Rezept hat keine Zutaten" (graceful, kein Error)

**Fehlerfall 2: Keine Zutaten mit Nährwerten**
- Alle Zutaten haben keine Nährwert-Einträge
- **Result**: "Keine Nährwertdaten für Zutaten verfügbar" (graceful)

**Fehlerfall 3: Manche Zutaten haben Nährwerte, manche nicht**
- Beispiel: Spaghetti hat Nährwerte, aber "Prise Salz" nicht
- **Result**: Berechnung mit verfügbaren Zutaten (Spaghetti zählt, Salz wird ignoriert)

---

## 4. UML-Diagramme (Planung)

### 4.1 Class Diagram

**Ziel**: Zeige Rezept-Aggregate, Relationships zu LebensmittelKatalog, Services

**Struktur**:

```
┌─────────────────────┐
│      Rezept         │  (Aggregate Root)
├─────────────────────┤
│ - Id: int           │
│ - Name: string      │
│ - Beschreibung: str │
│ - Zubereitung: str  │
│ - Portionen: int    │
│ - ZubereitungZeit   │
│ - Schwierigkeit     │
│ - CreatedAt: DateTime
│ - UpdatedAt: DateTime
│ - IsArchived: bool  │
│ - Zutaten: ICollection
├─────────────────────┤
│ + GetDetails()      │
│ + Archive()         │
└─────────────────────┘
           ▲ 1
           │ 1..N
           │ contains
           │
┌─────────────────────┐
│   RezeptZutat       │  (Value Object)
├─────────────────────┤
│ - Id: int           │
│ - RezeptId: int     │
│ - LebensmittelId    │
│ - Menge: double     │
│ - Einheit: string   │
│ - Notizen: string   │
│ - Position: int     │
│ - CreatedAt: DateTime
├─────────────────────┤
│ + Validate()        │
└─────────────────────┘
           │
           │ references
           │ N
           ▼ 1
┌──────────────────────────┐
│ LebensmittelKatalog      │  (Existing)
├──────────────────────────┤
│ - Id: int                │
│ - Name: string           │
│ - Einheit: string        │
│ - Kategorie: string      │
│ - IsArchived: bool       │
└──────────────────────────┘
```

**Services** (nebeneinander):

```
┌───────────────────────────────────────────────────┐
│             Services (Implementation)             │
├───────────────────────────────────────────────────┤
│ ┌──────────────┐  ┌──────────────┐  ┌──────────┐ │
│ │RezeptService │  │RezeptZutat   │  │NährwertC │ │
│ │              │  │Service       │  │alculator │ │
│ ├──────────────┤  ├──────────────┤  ├──────────┤ │
│ │+ GetRecipeById
│ │+ GetAll      │  │+ GetZutaten  │  │+ Calc... │ │
│ │+ Create      │  │+ AddZutat    │  │+ CalcPro │ │
│ │+ Update      │  │+ RemoveZutat │  │          │ │
│ │+ Delete      │  │+ Reorder     │  │          │ │
│ └──────────────┘  └──────────────┘  └──────────┘ │
└───────────────────────────────────────────────────┘
     ▼ uses              ▼ uses             ▼ uses
┌──────────────────────────────────────────────────┐
│             Repositories & Dependencies           │
├──────────────────────────────────────────────────┤
│ IRezeptRepository, ILebensmittelService,        │
│ INährwertService, IEinheitConverter             │
└──────────────────────────────────────────────────┘
```

**Diagramm wird erstellt in**: `diagrams/architecture-class-uc4.drawio`

---

### 4.2 ER-Diagramm Update

**Ziel**: Zeige neue Tabellen (Rezept, RezeptZutat) im bestehenden Schema

**Tabellen zu aktualisieren**:

```
Bestehend (grün = fertig):
├── LebensmittelKatalog (✅ UC1)
├── ProduktInstanz (✅ UC2/UC10)
├── Lagerort (✅ UC9)
├── Nährwert (✅ UC3)

Neu:
├── Rezept (⏳ UC4)
└── RezeptZutat (⏳ UC4)
```

**Schema-Änderungen**:

```
Rezept (neu)
├── Id (PK)
├── Name (UNIQUE)
├── Beschreibung
├── Zubereitung
├── Portionen
├── ZubereitungszeitMinuten
├── Schwierigkeitsgrad
├── CreatedAt
├── UpdatedAt
└── IsArchived

RezeptZutat (neu)
├── Id (PK)
├── RezeptId (FK → Rezept, ON DELETE CASCADE)
├── LebensmittelId (FK → LebensmittelKatalog, ON DELETE RESTRICT)
├── Menge
├── Einheit
├── Notizen
├── Position (UNIQUE per RezeptId)
└── CreatedAt
```

**Relationships**:

```
Rezept 1 ──── N RezeptZutat
RezeptZutat N ──── 1 LebensmittelKatalog
```

**Diagramm wird aktualisiert**: `diagrams/database-schema.drawio` (Teil 2 nach Dev-Agent)

---

### 4.3 Sequence Diagrams

**Geplant** (wird nach UC4-Implementation erstellt):

1. **Sequence: CreateRecipe (Happy Path)**
   - Benutzer → UI → RezeptService.CreateAsync
   - Service → Validation
   - Service → Repository.SaveAsync
   - Result: Rezept mit ID zurückgegeben

2. **Sequence: AddIngredient**
   - Benutzer → UI → RezeptZutatService.AddZutatAsync
   - Service → FK-Validierung (Lebensmittel)
   - Service → Position-Berechnung
   - Service → Repository.SaveAsync
   - Result: RezeptZutat mit ID zurückgegeben

3. **Sequence: CalculateRecipeNutrition (UC5-Integration)**
   - Benutzer → UI → NährwertCalculator.CalculateRezeptNährwerteAsync
   - Calculator → RezeptService.GetRecipeById
   - Calculator → RezeptZutatService.GetZutaten
   - Calculator → NährwertService.GetNährwert (pro Zutat)
   - Calculator → Summation + Skalierung
   - Result: RezeptNährwerteDto mit Gesamtwert + ProPortionWert

**Diagramme werden erstellt in**: `diagrams/sequence-uc4-rezepte.drawio` (nach Dev-Phase)

---

### 4.4 Component Diagram Update

**Geplant** (wird nach UC4-Implementation aktualisiert):

- Frontend: "Rezept Management" Component
- Backend: "RezeptService + RezeptZutatService" Component
- Database: "Rezept + RezeptZutat" Tables
- External: "LebensmittelKatalog" (Dependency), "NährwertService" (UC5-Integration)

**Diagramm wird aktualisiert**: `diagrams/architecture-components.drawio` (nach Doc-Phase)

---

## 5. Test-Struktur (für Test-Agent)

### 5.1 RezeptServiceTests

**Datei**: `src/Tests/Unit/Services/RezeptServiceTests.cs`

**Test-Kategorien**:

#### GetRezeptByIdAsync (3 Tests)
- ✅ Valid Recipe Returns (mit allen Feldern)
- ✅ Invalid RecipeId Returns Null
- ✅ Archived Recipe Returns Null (Soft-Delete-Filter)

#### GetAllRezepteAsync (2 Tests)
- ✅ All Non-Archived Recipes Returned (Sorted by Name)
- ✅ Archived Recipes Excluded

#### SearchRezepteAsync (3 Tests)
- ✅ Fuzzy Search by Name Works (contains)
- ✅ No Results Returns Empty List
- ✅ Case-Insensitive Search

#### CreateRezeptAsync (10+ Tests)
- ✅ Valid Recipe Created (all fields set)
- ✅ CreatedAt & UpdatedAt Timestamp Set (UTC)
- ✅ IsArchived Default to False
- ✅ Duplicate Name Throws DuplicateRezeptException
- ✅ Null Name Throws ValidationException
- ✅ Empty Name Throws ValidationException
- ✅ Name Too Long (>200) Throws ValidationException
- ✅ Portionen < 1 Throws ValidationException
- ✅ Portionen > 100 Throws ValidationException
- ✅ ZubereitungszeitMinuten < 0 Throws ValidationException
- ✅ ZubereitungszeitMinuten > 1440 Throws ValidationException
- ✅ Invalid Schwierigkeitsgrad Throws ValidationException (not Easy/Medium/Hard)
- ✅ Optional Fields (Beschreibung, Zubereitung) Can Be Null

#### UpdateRezeptAsync (8+ Tests)
- ✅ Valid Update Changes All Fields
- ✅ UpdatedAt Timestamp Updated (UTC)
- ✅ Recipe Not Found Throws NotFoundException
- ✅ Archived Recipe Throws InvalidOperationException
- ✅ New Name Duplicate Throws DuplicateRezeptException
- ✅ Invalid Range Throws ValidationException (same as Create)
- ✅ Updating Portionen Only (other fields unchanged)
- ✅ Updating Beschreibung to Null (clearing optional field)

#### DeleteRezeptAsync (Soft-Delete) (3 Tests)
- ✅ Valid Delete Sets IsArchived=True
- ✅ Deleted Recipe Not in GetAllRezepteAsync
- ✅ Recipe Not Found Throws NotFoundException

#### RezeptExistsAsync (2 Tests)
- ✅ Existing Non-Archived Recipe Returns True
- ✅ Non-Existing or Archived Recipe Returns False

**Gesamt: ~30 Tests für RezeptService**

---

### 5.2 RezeptZutatServiceTests

**Datei**: `src/Tests/Unit/Services/RezeptZutatServiceTests.cs`

**Test-Kategorien**:

#### GetZutatenAsync (2 Tests)
- ✅ All Zutaten Returned Sorted by Position
- ✅ Empty Recipe Returns Empty List

#### GetZutatAsync (3 Tests)
- ✅ Valid Zutat Returns with All Fields
- ✅ Invalid ZutatId Returns Null
- ✅ Invalid RezeptId Throws NotFoundException

#### AddZutatAsync (10+ Tests)
- ✅ Valid Zutat Added (all fields set)
- ✅ Position Auto-Calculated (= MaxPosition + 1)
- ✅ CreatedAt Timestamp Set (UTC)
- ✅ Recipe Not Found Throws NotFoundException
- ✅ Lebensmittel Not Found Throws NotFoundException (FK)
- ✅ Menge <= 0 Throws ValidationException
- ✅ Menge > 10000 Throws ValidationException
- ✅ Invalid Einheit Throws ValidationException (not in whitelist)
- ✅ Optional Notizen Can Be Null
- ✅ Adding Zutat to Archived Recipe Throws InvalidOperationException
- ✅ First Zutat Added (Position = 0)
- ✅ Second Zutat Added (Position = 1)

#### UpdateZutatAsync (6+ Tests)
- ✅ Valid Update Changes Menge/Einheit/Notizen
- ✅ Zutat Not Found Throws NotFoundException
- ✅ Invalid Menge Throws ValidationException (same as Create)
- ✅ Invalid Einheit Throws ValidationException
- ✅ Recipe Archived Throws InvalidOperationException
- ✅ Position Cannot Be Changed (immutable after creation)

#### DeleteZutatAsync (3 Tests)
- ✅ Valid Delete Removes Zutat
- ✅ Zutat Not Found Throws NotFoundException (idempotent or error?)
- ✅ Deleting Last Zutat Throws ValidationException (Rezept needs min 1 Zutat!)

#### ReorderZutatenAsync (4+ Tests)
- ✅ Valid Reorder Changes Position
- ✅ All Zutaten in New Order Have Updated Positions
- ✅ Invalid ZutatId in List Throws NotFoundException
- ✅ Incomplete List (missing Zutat) Throws ValidationException
- ✅ Duplicate ZutatIds Throws ValidationException

#### GetZutatCountAsync (1 Test)
- ✅ Returns Correct Count of Zutaten

**Gesamt: ~25 Tests für RezeptZutatService**

---

### 5.3 NährwertCalculatorTests

**Datei**: `src/Tests/Unit/Services/NährwertCalculatorTests.cs`

**Test-Kategorien**:

#### CalculateRezeptNährwerteAsync (8+ Tests)
- ✅ Valid Recipe Returns Complete Nährwerte (Gesamt + ProPortion)
- ✅ Recipe Not Found Throws NotFoundException
- ✅ Single Zutat Calculation (simple case)
- ✅ Multiple Zutaten Summation (200g Spaghetti + 100g Speck)
- ✅ Zutat Without Nährwert Ignored (graceful)
- ✅ Alle Zutaten Without Nährwert Returns Zero Nährwerte
- ✅ Pro Portion = Gesamt / Portionen (correct division)
- ✅ Dezimal Rounding (z.B. 10.567 → 10.6)

#### CalculateProPortionAsync (3 Tests)
- ✅ Correct Division (Gesamt / Portionen)
- ✅ Dezimal Values (z.B. 1200 kcal / 4 = 300 kcal)
- ✅ Large Rezept (1000+ Zutaten, correct calculation)

#### Edge Cases (6+ Tests)
- ✅ Menge = 0.01 (minimum) Calculated Correctly
- ✅ Menge = 10000 (maximum) Calculated Correctly
- ✅ Einheit "TL" Converted to Gramm (~5g)
- ✅ Einheit "EL" Converted to Gramm (~15g)
- ✅ Decimal Nährwerte (z.B. Fett=10.5g) Summed Correctly
- ✅ Zero Protein (0.0) Handled (not null)

**Gesamt: ~12 Tests für NährwertCalculator**

---

### 5.4 Integrations Tests (optional, aber empfohlen)

**Datei**: `src/Tests/Integration/RezeptIntegrationTests.cs` (optional für UC4 v1.0)

**Test-Szenarien**:
- ✅ Full Workflow: Create Recipe → Add Zutaten → Calculate Nährwerte
- ✅ Update Recipe: Change Portionen → Nährwerte Pro Portion Updated
- ✅ Delete & Restore: Soft-Delete → Not in GetAll → Restore (wenn implementiert)

**Gesamt: ~5 Tests (optional)**

---

## 6. Abhängigkeits-Übersicht

### 6.1 Interne Abhängigkeiten (Projekt)

```
UC4 (Rezepte)
  ├── depends on UC1 (LebensmittelKatalog)
  │   └── FK: RezeptZutat.LebensmittelId → LebensmittelKatalog.Id
  │
  ├── depends on UC3 (Nährwerte)
  │   └── NährwertCalculator nutzt INährwertService
  │   └── FK: LebensmittelId → Nährwert.LebensmittelId
  │
  └── included by UC5 (Nährwertberechnung)
      └── UC5 nutzt RezeptService + NährwertCalculator
```

### 6.2 Externe Abhängigkeiten (Packages)

```
Bestehend (von UC1-UC3):
├── Microsoft.EntityFrameworkCore
├── Microsoft.EntityFrameworkCore.Sqlite
├── xUnit + Moq (für Tests)
└── ...

Neu für UC4:
├── Keine neuen Packages nötig!
│   (Alle Dependencies bereits vorhanden)
└── Ggf. Enum-Parsing Helper für Schwierigkeitsgrad
```

---

## 7. Validierungs-Zusammenfassung

### Rezept-Validierung

| Feld | Validierung | Error-Type |
|------|-------------|-----------|
| Name | Not Null, Length 1-200, Unique | ValidationException / DuplicateException |
| Portionen | 1-100 (int) | ValidationException |
| ZubereitungszeitMinuten | 0-1440 (int) | ValidationException |
| Schwierigkeitsgrad | "Easy" / "Medium" / "Hard" | ValidationException |
| Beschreibung | Optional (nullable) | - |
| Zubereitung | Optional (nullable) | - |
| IsArchived | Default = false | - |
| CreatedAt/UpdatedAt | UTC Timestamp (auto) | - |

### RezeptZutat-Validierung

| Feld | Validierung | Error-Type |
|------|-------------|-----------|
| RezeptId | FK to Rezept, exists, not archived | NotFoundException / InvalidOperation |
| LebensmittelId | FK to LebensmittelKatalog, exists | NotFoundException |
| Menge | 0.01-10000 (double) | ValidationException |
| Einheit | Whitelist (g, ml, Stück, TL, EL, Tasse, Prise) | ValidationException |
| Position | 0-based, unique per Rezept | ValidationException (on reorder) |
| Notizen | Optional (nullable) | - |
| CreatedAt | UTC Timestamp (auto) | - |

---

## 8. Performance & Scalability Überlegungen

### 8.1 Datenbank-Indizes (für UC4)

```sql
-- Für schnelle Lookups
CREATE INDEX idx_rezept_name ON Rezept(Name);
CREATE INDEX idx_rezept_archived ON Rezept(IsArchived);
CREATE INDEX idx_rezeptzutat_rezeptid ON RezeptZutat(RezeptId);
CREATE INDEX idx_rezeptzutat_lebensmittelid ON RezeptZutat(LebensmittelId);

-- Für Soft-Delete Queries
CREATE INDEX idx_rezeptzutat_position ON RezeptZutat(RezeptId, Position);
```

### 8.2 Lazy Loading vs Eager Loading

- **GetRezeptByIdAsync**: Lazy Load Zutaten (nur bei Bedarf)
- **GetAllRezepteAsync**: Eager Load (keine Zutaten, nur Meta-Daten)
- **GetZutatenAsync**: Eager Load LebensmittelKatalog (braucht Name für UI)
- **NährwertCalculator**: Eager Load alles (komplexe Query)

### 8.3 Caching (optional für v1.0)

- Rezepte sind relativ statisch → Caching möglich
- Nährwertberechnung ist read-heavy → Cache pro Rezept möglich
- Invalidation bei Update/Delete
- Aber: First implementation ohne Cache (KISS)

---

## 9. Definition-of-Done UC4

### Test-Phase ✅
- [ ] 30+ Tests geschrieben (TDD Red)
- [ ] Testkonstruktion validiert (Review-Agent)
- [ ] Edge-Cases abgedeckt

### Dev-Phase ✅
- [ ] RezeptService implementiert
- [ ] RezeptZutatService implementiert
- [ ] NährwertCalculator implementiert
- [ ] Alle 30+ Tests GRÜN (TDD Green)
- [ ] Validierungslogik korrekt
- [ ] FK-Constraints respektiert
- [ ] Soft-Delete funktioniert

### Doc-Phase ✅
- [ ] Code-Dokumentation (XML-Docs)
- [ ] Feature-HTML (UC4-Rezepte.html)
- [ ] Architecture-Overview aktualisiert
- [ ] Sequence-Diagramm erstellt
- [ ] ER-Diagramm aktualisiert
- [ ] use-cases.drawio aktualisiert

### Review-Phase ✅
- [ ] Code-Review bestanden (Review-Agent)
- [ ] Test-Review bestanden (Review-Agent)
- [ ] Doku-Review bestanden (Review-Agent)

### User-Review ✅
- [ ] Feature passt zu UC4-Requirements
- [ ] Keine kritischen Bugs
- [ ] Performance akzeptabel

---

## 10. Geschätzte Test-Anzahl

```
RezeptServiceTests:        ~30 Tests
RezeptZutatServiceTests:   ~25 Tests
NährwertCalculatorTests:   ~12 Tests
IntegrationTests:          ~5 Tests (optional)
─────────────────────────────────────
GESAMT UC4:                ~40-50 Tests
```

**Vergleich zu UC3**: UC3 hatte 23 Tests → UC4 wird größer (2 Services + Calculator)

---

**Status**: 🔵 **Domain Model & Spezifikation COMPLETE**

**Nächster Schritt**: Test-Agent schreibt UC4 Tests (TDD Red-Phase) basierend auf dieser Spezifikation

**Abhängigkeitscheck**: ✅ UC1 + UC3 bereits fertig, UC4 kann sofort starten
