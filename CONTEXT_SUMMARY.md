# FoodDatabase – Entwicklungs-Status & Kontext

**Datum**: 2026-06-07  
**Status**: UC1 abgeschlossen & gemergt  
**Nächster Schritt**: UC2-UC9 implementieren (oder fortsetzen nach Unterbrechung)

---

## ✅ Abgeschlossen

### Phase 1: Orchestrator & Anforderungen
- ✅ Use-Case-Diagramm mit 10 UC (neutral color scheme)
- ✅ Anforderungs-Analyse mit detailliertem Datenmodell
- ✅ Tech-Stack definiert: ASP.NET Core 8 + Blazor + SQLite + TrueNAS Docker

### Phase 2: Doc-Agent – Diagramme
- ✅ Klassen-Diagramm (Domain Model: LebensmittelKatalog, ProduktInstanz ⭐, Nährwert, Rezept, Einkaufsliste)
- ✅ Komponenten-Diagramm (Layered Architecture: Blazor → Services → Repository → EF Core → SQLite)
- ✅ Deployment-Diagramm (TrueNAS Docker Container Setup)
- Dateien: `diagrams/architecture-class.drawio`, `architecture-components.drawio`, `architecture-deployment.drawio`

### Phase 3: UC1 – Lebensmittel-Katalog (✅ GEMERGT)
- ✅ Test-Agent: 20+ Unit Tests (LebensmittelServiceTests.cs)
- ✅ Dev-Agent: Implementierung (LebensmittelService, IRepository, LebensmittelKatalog Model)
- ✅ Verzeichnis-Struktur: Interfaces/ und Classes/ getrennt
- ✅ Review & User-Approval erhalten
- ✅ Gemergt zu `master` (commit 04e095a)

---

## 📋 Verbleibende Use-Cases (Phase 4+)

| UC | Feature | Status | Branch |
|----|---------|--------|--------|
| UC10 | Produktinstanzen mit MHD (zentral!) | ⏳ Nächstes | feat/uc10-produktinstanzen |
| UC2 | Lagerbestand aktualisieren | ⏳ TODO | feat/uc2-lagerbestand |
| UC3 | Nährwerte verwalten | ⏳ TODO | feat/uc3-nährwerte |
| UC4 | Rezepte verwalten | ⏳ TODO | feat/uc4-rezepte |
| UC5 | Nährwerte berechnen | ⏳ TODO | feat/uc5-rezept-nährwerte |
| UC6 | Verbrauchte Produkte ausbuchen | ⏳ TODO | feat/uc6-verbrauch |
| UC7 | Verfallsdatum-Warnungen | ⏳ TODO | feat/uc7-verfallsdatum |
| UC8 | Einkaufslisten generieren | ⏳ TODO | feat/uc8-einkaufslisten |
| UC9 | Lagerorte verwalten | ⏳ TODO | feat/uc9-lagerorte |

**Priorisierung**: UC10 zuerst (zentral für Lagerbestand mit MHD)

---

## 🏗️ Projekt-Struktur (aktuell)

```
src/
├── App/
│   ├── Models/
│   │   ├── LebensmittelKatalog.cs ✅
│   │   ├── ProduktInstanz.cs (benötigt)
│   │   ├── Nährwert.cs (benötigt)
│   │   ├── Rezept.cs (benötigt)
│   │   ├── RezeptZutat.cs (benötigt)
│   │   ├── Lagerort.cs (benötigt)
│   │   ├── VerfallsdatumWarnung.cs (benötigt)
│   │   └── Einkaufsliste.cs (benötigt)
│   └── Services/
│       ├── Interfaces/
│       │   ├── IRepository.cs ✅
│       │   ├── ILebensmittelService.cs ✅
│       │   ├── IProduktInstanzService.cs (benötigt)
│       │   ├── INährwertService.cs (benötigt)
│       │   └── ...
│       └── Classes/
│           ├── LebensmittelService.cs ✅
│           ├── ProduktInstanzService.cs (benötigt)
│           └── ...
├── Data/
│   └── FoodDatabaseContext.cs (TODO: Create with EF Core)
├── Components/
│   ├── Layout/ (TODO)
│   └── Pages/ (TODO)
└── Program.cs (TODO: Add DI + DbContext)
└── appsettings.json (TODO)

Tests/
├── Unit/
│   └── Services/
│       └── LebensmittelServiceTests.cs ✅
└── Integration/ (TODO)

diagrams/
├── architecture-class.drawio ✅
├── architecture-components.drawio ✅
└── architecture-deployment.drawio ✅

requirements/
├── use-cases.drawio ✅
└── analysis.md ✅

docs/
└── (HTML-Dokumentation – später Doc-Agent)
```

---

## 🔄 Workflow für nächste UC

Jede UC folgt diesem Pattern:

1. **Feature-Branch erstellen**: `git checkout -b feat/ucN-feature-name`
2. **Test-Agent schreiben**: Unit Tests (TDD Red)
3. **Dev-Agent implementieren**: Code (TDD Green)
4. **Review-Agent überprüfen**: Code Review
5. **User reviewen**: Local merge after approval
6. **Merge zu master**: `git merge feat/ucN-feature-name`
7. **Nächste UC starten**: Repeat

---

## 💡 Wichtige Erkenntnisse

- **ProduktInstanz ist zentral**: Jede gekaufte Produkteinheit hat eigenes MHD (nicht nur ein MHD pro Lebensmittel)
- **Verzeichnis-Struktur**: Interfaces/ und Classes/ getrennt (User-Anforderung)
- **Farben-Neutral**: Alle Diagramme mit schwarzer Schrift + weißem Hintergrund
- **TDD-Ansatz funktioniert**: Tests zuerst, dann Code
- **MR-Format**: Jeder UC → Feature-Branch → MR → User-Review → Merge

---

## 🎯 Nächste Sitzung

**Start**: UC10 implementieren (Produktinstanzen mit MHD)  
**Dauer**: ~2-3 Stunden für UC2-UC4  
**Priorität**: UC10 → UC2 → UC3 → UC4/UC5 → Rest

---

**Status**: Infrastruktur steht, Workflow etabliert, ready für Mass-Implementation! 🚀
