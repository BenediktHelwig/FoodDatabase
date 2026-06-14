# FoodDatabase – Anforderungsanalyse v1.0

**Datum**: 2026-06-07 (Initial Analysis)  
**Aktualisiert**: 2026-06-14  
**Status**: Aktuell gültig für UC1-UC10 MVP | Freigegeben zur Entwicklung  
**Benutzer**: Privater Anwender (max. 3 Geräte im Heimnetzwerk)

---

## 📋 Vision & Kontext

**Problem**: Überblick über Lebensmittel an verschiedenen Lagerorten verlieren, schwierige Suche, unbekannte Nährwerte bei Rezepten.

**Lösung**: FoodDatabase – Eine private, lokale Anwendung zur Verwaltung von Lebensmitteln, Nährwerten und Rezepten mit automatischer Einkaufslisten-Generierung.

**Nutzer**: Eine Person (privat), Zugriff von bis zu 3 Geräten (Windows, iOS, Android) im Heimnetzwerk.

---

## 🎯 Funktionale Anforderungen

### **UC1: Lebensmittel verwalten (Produktkatalog)**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Zweck**: Verwaltung des Produkt-Katalogs (Template für Produkte)
- **Erfassung**: Manuell eingeben ODER (später) Barcode-Scanning
- **Felder**:
  - Produktname
  - Kategorie (optional)
  - Standard-Einheit (g, ml, Stück)

### **UC2: Lagerbestand aktualisieren**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Verwaltung von Produktinstanzen**: Jedes gekaufte Produkt ist eine eigene Instanz mit eigenem MHD
- **Beispiel**: "Zwei Tüten Chips" = 2 separate Einträge mit unterschiedlichen Verfallsdaten
- **Felder pro Instanz**:
  - Lebensmittel (FK → Katalog)
  - Lagerort
  - Aktuelle Menge
  - Mindestbestand (Menge + Einheit)
  - Einkaufsdatum
  - **Verfallsdatum** ← Pro Instanz gespeichert!
- **Operationen**: Neu hinzufügen, Menge ändern, MHD anpassen, Instanz löschen

### **UC3: Nährwerte verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Eingabe**: Pro 100g / 100ml (Standard-Referenzmenge)
- **Nährwerte**:
  - Kalorien (kcal)
  - Kohlenhydrate (g)
  - Eiweiß (g)
  - Fett (g)
  - Ballaststoffe (g) – optional
  - Zucker (g) – optional
- **Verknüpfung**: Mit Lebensmittel-Katalog verlinkt
- **Berechnung**: Automatische Skalierung bei anderen Mengen

### **UC4: Rezepte verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Struktur**:
  - Rezeptname
  - Zutaten-Liste (Lebensmittel + Menge)
  - Zubereitung (Text/Beschreibung)
  - Portionen
- **Verknüpfung**: Mit UC3 (Nährwerte aus Katalog ziehen)

### **UC5: Nährwerte für Rezepte berechnen**
- **Automatisch**: Summation der Nährwerte aller Zutaten
- **Pro Portion**: Nährwerte berechnet basierend auf Portionszahl
- **Anpassbar**: Mengen-Anpassung sollte Nährwerte automatisch neu berechnen
- **Display**: Übersichtliche Anzeige der Gesamtnährwerte pro Rezept

### **UC6: Verbrauchte Produkte ausbuchen**
- **Trigger**: Manuell durch Benutzer (Scan oder manuelle Eingabe)
- **Operationen**:
  - Menge reduzieren (z.B. "von 500g auf 200g verbraucht")
  - Instanz vollständig löschen (wenn leer)
  - Optional: Barcode-Scan zur schnellen Erfassung (später implementieren)
- **Effekt**: Aktualisiert Lagerbestände, kann Einkaufslisten-Trigger auslösen

### **UC7: Verfallsdatum-Warnungen anzeigen**
- **Automatische Überwachung**: Zeige alle Produkte mit kritischem MHD
- **Kategorisierung**:
  - 🟡 **Gelb**: Läuft in den nächsten 3 Tagen ab
  - 🔴 **Rot**: Bereits abgelaufen
- **Übersicht**: Sortiert nach Dringlichkeit (abgelaufen zuerst, dann nach Datum)
- **Ziel**: Schnell sichtbar machen, welche Produkte bald weg müssen
- **Integration mit UC6**: Verbrauchte Produkte werden aus der Warnliste entfernt

### **UC8: Einkaufslisten generieren**
- **Trigger**: Automatisch, wenn Mindestbestand erreicht/unterschritten
- **Inhalt**: Alle Produktinstanzen unter Mindestbestand
- **Keine Gruppierung**: Einfache flache Liste
- **Verwaltung**: Abhaken von gekauften Produkten
- **Rücksetzen**: Nach dem Einkauf Bestände über UC2 aktualisieren

### **UC9: Lagerorte verwalten**
- **CRUD-Operationen**: Erstellen, Lesen, Aktualisieren, Löschen
- **Verwendung**: In UC2 zum Klassifizieren von Produktinstanzen
- **Beispiele**: Kühlschrank, Tiefkühler, Pantry, Keller, Balkon, etc.

### **UC10: Produktinstanzen mit MHD verwalten (zentral)**
- **Zentrale Datenentität**: Jede gekaufte Produktinstanz bekommt eigenes MHD
- **Kern-Unterscheidung**:
  - **Lebensmittel-Katalog (UC1)**: Was existiert alles? (Chips, Käse, Milch, etc.)
  - **Produktinstanz (UC10)**: Welche einzelnen Exemplare habe ich zu Hause? Mit welchen Daten?
- **Datenmodell**: Siehe Domänen-Modell unten

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

## 🏗️ Domänen-Modell (Überarbeitet)

### **Entities**

```
LebensmittelKatalog
├── ID (PK)
├── Name
├── Einheit (g, ml, Stück)
├── Kategorie (optional)
└── Nährwert (FK → Nährwert)

Nährwert
├── ID (PK)
├── LebensmittelKatalogID (FK)
├── Kalorien (pro 100g/ml)
├── Kohlenhydrate (g)
├── Eiweiß (g)
├── Fett (g)
├── Ballaststoffe (g) [optional]
├── Zucker (g) [optional]
└── ReferenzMenge (100)

ProduktInstanz (⭐ zentral für MHD-Tracking)
├── ID (PK)
├── LebensmittelKatalogID (FK)
├── Lagerort (FK → Lagerort)
├── AktuelleM enge
├── MindestbestandMenge
├── EinkaufsDatum
├── VerfallsDatum  ← PRO INSTANZ!
├── ErstelltAm
└── Status (Aktiv, Verbraucht, Abgelaufen)

Lagerort
├── ID (PK)
├── Name

VerfallsdatumWarnung
├── ID (PK)
├── ProduktInstanzID (FK)
├── Status (OK, WarningBaldAbgelaufen, Abgelaufen)
├── AblaufdatumIn Tagen
├── AktualisertAm

Rezept
├── ID (PK)
├── Name
├── Portionen
├── Zubereitung
└── ZutatenListe (M:N via RezeptZutat)

RezeptZutat
├── ID (PK)
├── RezeptID (FK)
├── LebensmittelKatalogID (FK)
├── Menge
├── Einheit

Einkaufsliste
├── ID (PK)
├── ErstelltAm
├── AktualisertAm
└── EinträgeID (FK → ProduktInstanz[], alle mit MindestbestandUnterschritten)

EinkaufsListenEintrag
├── ID (PK)
├── ProduktInstanzID (FK)
├── Gekauft (bool)
```

**Wichtige Unterscheidung**:
- **LebensmittelKatalog**: "Chips" ist ein Produkt-Template
- **ProduktInstanz**: "Diese Tüte Chips (gekauft 01.06.2026, MHD 30.07.2026)" ist eine Instanz
- **Mehrfach-Exemplare**: 2 Tüten Chips = 2 ProduktInstanzen mit unterschiedlichen Verfallsdaten ✅

---

## 🔄 User Workflows

### **Workflow 1: Neues Produkt in den Haushalt**
```
1. Benutzer öffnet FoodDatabase
2. Klick: "Neues Lebensmittel" oder "Produkt hinzufügen"
3. Eingabe:
   - Produkt aus Katalog auswählen (oder neues Produkt + Nährwerte)
   - Lagerort auswählen
   - Aktuelle Menge
   - Mindestbestand
   - **Verfallsdatum (für diese Instanz!)**
   - Einkaufsdatum
4. Speichern → ProduktInstanz erstellt
5. Übersicht aktualisiert
```

### **Workflow 2: Verfallsdatum-Überwachung**
```
1. Benutzer öffnet "Warnungen" oder Startseite
2. System zeigt:
   - 🟡 Gelb: Produkte mit MHD in den nächsten 3 Tagen
   - 🔴 Rot: Bereits abgelaufen (sollten verbraucht/gelöscht werden)
3. Benutzer kann direkt auf verbrauchtes Produkt zugreifen (UC6)
```

### **Workflow 3: Verbrauchte Produkte ausbuchen**
```
1. Benutzer öffnet "Lagerbestand" oder Scan-Interface
2. Scan oder manuelle Suche: "Chips" → Alle Instanzen anzeigen
3. Auswahl: "Tüte Chips (MHD 30.07.2026)"
4. Aktion: "Verbraucht" oder "Menge reduzieren" (z.B. 500g → 200g)
5. Speichern → Bestand aktualisiert
6. Falls Mindestbestand unterschritten → Automatisch auf Einkaufsliste
```

### **Workflow 4: Rezept erstellen & Nährwerte anzeigen**
```
1. Benutzer öffnet "Rezepte"
2. Klick: "Neues Rezept"
3. Eingabe: Name, Portionen
4. Zutaten hinzufügen (aus Lebensmittel-Katalog + Menge)
5. System berechnet automatisch Nährwerte (pro 100g * Menge)
6. Benutzer sieht Gesamtnährwerte pro Rezept + pro Portion
```

### **Workflow 5: Einkaufsliste generieren**
```
1. Benutzer öffnet "Einkaufsliste"
2. System zeigt automatisch alle Produkte unter Mindestbestand
3. Benutzer kauft Produkte
4. Benutzer fügt neue ProduktInstanzen hinzu (mit Menge + MHD)
5. Einkaufsliste wird geleert/aktualisiert
```

---

## 🎯 Priorisierung & Phasen

### **Phase 1 (MVP): v1.0 – Alle Use-Cases mit MHD-Tracking**
- ✅ UC1: Lebensmittel-Katalog verwalten (CRUD)
- ✅ UC2: Lagerbestand/Produktinstanzen aktualisieren (CRUD mit MHD pro Instanz)
- ✅ UC3: Nährwerte verwalten (CRUD)
- ✅ UC4: Rezepte verwalten (CRUD + Zutaten)
- ✅ UC5: Nährwerte für Rezepte berechnen (automatisch)
- ✅ UC6: Verbrauchte Produkte ausbuchen (Scan vorbereitet, aber manuell implementiert)
- ✅ UC7: Verfallsdatum-Warnungen anzeigen (🟡 Gelb / 🔴 Rot)
- ✅ UC8: Einkaufslisten generieren (Mindestbestand-Trigger)
- ✅ UC9: Lagerorte verwalten (CRUD)
- ✅ UC10: Produktinstanzen mit MHD verwalten (zentrale Datenentität)
- ✅ Manuelles Eingeben aller Daten
- ✅ SQLite Datenbank mit optimiertem Schema
- ✅ Blazor Frontend (responsive)
- ✅ Docker Deployment auf TrueNAS

### **Phase 2+ (Zukünftig)**
- ⏳ Barcode-Scanning Integration (Grundlage in UC6/UC2 vorhanden)
- ⏳ Erweiterte Berichte (Nährwert-Analyse, Verbrauchshistorie)
- ⏳ Export/Import (CSV, PDF)
- ⏳ Bestandsverlauf-Tracking
- ⏳ Weitere Nährwert-Details
- ⏳ Benachrichtigungen (z.B. Push auf Smartphone bei Verfallsdatum)

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
