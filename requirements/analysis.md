# FoodDatabase – Anforderungsanalyse v1.0

**Datum**: 2026-06-07  
**Status**: Freigegeben zur Entwicklung  
**Benutzer**: Privater Anwender (max. 3 Geräte im Heimnetzwerk)

---

## 📋 Vision & Kontext

**Problem**: Überblick über Lebensmittel an verschiedenen Lagerorten verlieren, schwierige Suche, unbekannte Nährwerte bei Rezepten.

**Lösung**: FoodDatabase – Eine private, lokale Anwendung zur Verwaltung von Lebensmitteln, Nährwerten und Rezepten mit automatischer Einkaufslisten-Generierung.

**Nutzer**: Eine Person (privat), Zugriff von bis zu 3 Geräten (Windows, iOS, Android) im Heimnetzwerk.

---

## 🎯 Funktionale Anforderungen

### **UC1: Lebensmittel verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Lagerort-Tracking**: Lebensmittel mit Lagerort speichern (z.B. "Kühlschrank", "Tiefkühler", "Pantry")
- **Erfassung**: Manuell eingeben ODER (später) Barcode-Scanning
- **Felder**:
  - Produktname
  - Lagerort
  - Mindestbestand (Menge + Einheit: g, ml, Stück)
  - Aktuelle Menge
  - Einkaufsdatum (optional)
  - Verfallsdatum (optional)

### **UC2: Nährwerte verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Eingabe**: Pro 100g / 100ml (Standard-Referenzmenge)
- **Nährwerte**:
  - Kalorien (kcal)
  - Kohlenhydrate (g)
  - Eiweiß (g)
  - Fett (g)
  - Ballaststoffe (g) – optional
  - Zucker (g) – optional
- **Verknüpfung**: Mit Lebensmittel-Einträgen verlinkt
- **Berechnung**: Automatische Skalierung bei anderen Mengen

### **UC3: Rezepte verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Struktur**:
  - Rezeptname
  - Zutaten-Liste (Lebensmittel + Menge)
  - Zubereitung (Text/Beschreibung)
  - Portionen
- **Verknüpfung**: Mit UC2 (Nährwerte aus Lebensmitteln ziehen)

### **UC4: Nährwerte für Rezepte berechnen**
- **Automatisch**: Summation der Nährwerte aller Zutaten
- **Pro Portion**: Nährwerte berechnet basierend auf Portionszahl
- **Anpassbar**: Mengen-Anpassung sollte Nährwerte automatisch neu berechnen
- **Display**: Übersichtliche Anzeige der Gesamtnährwerte pro Rezept

### **UC5: Einkaufslisten generieren**
- **Trigger**: Automatisch, wenn Mindestbestand erreicht/unterschritten
- **Inhalt**: Alle Produkte unter Mindestbestand
- **Keine Gruppierung**: Einfache flache Liste
- **Verwaltung**: Abhaken von gekauften Produkten
- **Rücksetzen**: Nach dem Einkauf Bestände aktualisieren

### **UC6: Lagerorte verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Verwendung**: In UC1 zum Klassifizieren von Lebensmitteln
- **Beispiele**: Kühlschrank, Tiefkühler, Pantry, Keller, Balkon, etc.

---

## ⚙️ Technische Anforderungen

| Anforderung | Spezifikation |
|-------------|---------------|
| **Backend** | ASP.NET Core 8+ |
| **Frontend** | Blazor Server (Single Page App) |
| **Datenbank** | SQLite (lokal im Container) |
| **Deployment** | Docker Container auf TrueNAS |
| **Netzwerk** | Lokales Heimnetzwerk (LAN) |
| **Clients** | Windows, iOS, Android (Browser-basiert) |
| **Benutzer** | Max. 3 parallele Nutzer |
| **Auth** | Keine (lokales Netzwerk, vertraut) |
| **Performance** | Anwenderfreundlich, keine High-Speed-Anforderungen |
| **Compliance** | Keine DSGVO-Anforderungen |
| **Offline** | Nicht erforderlich (LAN-only) |
| **Export/Import** | Nicht für v1.0 erforderlich |

---

## 🏗️ Domänen-Modell (Voraussichtlich)

### **Entities**

```
Lebensmittel
├── ID (PK)
├── Name
├── Lagerort (FK → Lagerort)
├── MindestbestandMenge
├── MindestbestandEinheit
├── AktuelleM enge
├── EinkaufsDatum
├── VerfallsDatum
└── Nährwerte (FK → Nährwert)

Nährwert
├── ID (PK)
├── LebensmittelID (FK)
├── Kalorien
├── Kohlenhydrate
├── Eiweiß
├── Fett
├── Ballaststoffe (optional)
├── Zucker (optional)
└── ReferenzMenge (100)

Rezept
├── ID (PK)
├── Name
├── Portionen
├── Zubereitung
└── ZutatenListe (M:N via RezeptZutat)

RezeptZutat
├── ID (PK)
├── RezeptID (FK)
├── LebensmittelID (FK)
├── Menge
├── Einheit

Lagerort
├── ID (PK)
└── Name

Einkaufsliste
├── ID (PK)
├── EinträgeID (FK → Lebensmittel[])
└── ErstelltAm

EinkaufsListenEintrag
├── ID (PK)
├── LebensmittelID (FK)
├── Menge
├── Gekauft (bool)
```

---

## 🔄 User Workflows

### **Workflow 1: Lebensmittel erfassen**
```
1. Benutzer öffnet FoodDatabase
2. Klick: "Neues Lebensmittel"
3. Eingabe: Name, Lagerort, Nährwerte, Menge, Mindestbestand
4. Speichern → In Datenbank gespeichert
5. Übersicht aktualisiert
```

### **Workflow 2: Rezept erstellen & Nährwerte anzeigen**
```
1. Benutzer öffnet "Rezepte"
2. Klick: "Neues Rezept"
3. Eingabe: Name, Portionen
4. Zutaten hinzufügen (Lebensmittel aus Katalog auswählen + Menge)
5. System berechnet automatisch Nährwerte
6. Benutzer sieht Gesamtnährwerte pro Rezept
```

### **Workflow 3: Einkaufsliste generieren**
```
1. Benutzer öffnet "Einkaufsliste"
2. System zeigt alle Produkte unter Mindestbestand
3. Benutzer kauft Produkte
4. Benutzer aktualisiert Bestände (Menge eingeben)
5. Einkaufsliste geleert (wenn alles gekauft & eingegeben)
```

---

## 🎯 Priorisierung & Phasen

### **Phase 1 (MVP): v1.0**
- ✅ UC1: Lebensmittel verwalten (CRUD)
- ✅ UC2: Nährwerte verwalten (CRUD)
- ✅ UC3: Rezepte verwalten (CRUD)
- ✅ UC4: Nährwerte berechnen
- ✅ UC5: Einkaufslisten generieren
- ✅ UC6: Lagerorte verwalten (CRUD)
- ✅ Manuelles Eingeben aller Daten
- ✅ SQLite Datenbank
- ✅ Blazor Frontend
- ✅ Docker Deployment auf TrueNAS

### **Phase 2+ (Zukünftig)**
- ⏳ Barcode-Scanning Integration
- ⏳ Erweiterte Berichte (Nährwert-Analyse, Verbrauchshistorie)
- ⏳ Export/Import (CSV, PDF)
- ⏳ Bestandsverlauf-Tracking
- ⏳ Weitere Nährwert-Details

---

## ✅ Akzeptanzkriterien

| Kriterium | Status |
|-----------|--------|
| Alle 6 Use-Cases implementiert | [ ] |
| CRUD für alle Entities | [ ] |
| Nährwert-Berechnung funktioniert | [ ] |
| Einkaufslisten-Generierung funktioniert | [ ] |
| Datenbankschema normalisiert | [ ] |
| Responsive Design (Windows/iOS/Android) | [ ] |
| Tests schreiben (TDD >80% Coverage) | [ ] |
| Dokumentation vollständig | [ ] |
| Merge Request genehmigt | [ ] |
| Erfolgreich auf TrueNAS deployed | [ ] |

---

## 📞 Offene Fragen / Klärungen

Keine. Anforderungen sind vollständig und klar.

---

**Nächster Schritt**: Merge Request für Phase 1 erstellen → User-Genehmigung → Agenten starten
