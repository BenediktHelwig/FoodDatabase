# FoodDatabase – Lebensmittel-Verwaltungssystem

**Status**: ✅ UC1 & UC10 implementiert | 51/51 Tests grün | Ready for UC2-UC9

---

## 📋 Projekt-Übersicht

FoodDatabase ist eine ASP.NET Core 8 Blazor Server Anwendung zur intelligenten Verwaltung von Lebensmitteln mit Verfallsdatum-Tracking (MHD - Mindesthaltbarkeitsdatum).

### Kern-Features
- **UC1**: Lebensmittel-Katalog (CRUD)
- **UC10**: ProduktInstanzen mit individuallem Verfallsdatum (FIFO-Logik)
- **UC2-UC9**: Lagerbestand, Nährwerte, Rezepte, Verbrauch, Warnungen, Einkaufslisten, Lagerorte

---

## 🚀 Quick Start

### Voraussetzungen
- .NET 8 SDK
- Visual Studio 2022 / VS Code

### Installation & Tests

```bash
# Repository klonen
git clone https://github.com/user/FoodDatabase.git
cd FoodDatabase

# Tests ausführen
dotnet test FoodDatabase.sln

# App starten
cd src/App
dotnet run
```

**Erwartet**: 51/51 Tests GRÜN ✅

---

## 📁 Projekt-Struktur

```
FoodDatabase/
├── src/
│   ├── App/                          # ASP.NET Core 8 Application
│   │   ├── Models/                  # Domain Models (LebensmittelKatalog, ProduktInstanz, etc.)
│   │   ├── Services/
│   │   │   ├── Interfaces/          # Service Contracts (IRepository, ILebensmittelService, etc.)
│   │   │   └── Classes/             # Service Implementations
│   │   ├── Data/                    # EF Core Context (TODO)
│   │   └── Components/              # Blazor Components (TODO)
│   └── Tests/                        # xUnit Test Suite
│       └── Unit/Services/           # Service Tests
├── diagrams/                         # UML Diagramme (draw.io)
│   ├── use-cases.drawio
│   ├── architecture-class.drawio
│   ├── architecture-components.drawio
│   ├── architecture-deployment.drawio
│   └── database-schema.drawio
├── docs/                             # HTML Dokumentation + CSS
│   ├── architecture-overview.html
│   ├── css/                         # Separate Stylesheets pro Feature
│   └── features/                    # Feature-spezifische Dokumentation
├── requirements/                     # Use-Case Analyse
├── reviews/                          # Merge Request Dokumentationen
├── CONTEXT_SUMMARY.md               # Status & Kontext für nächste Session
├── CLAUDE.md                        # Agent-Orchestration
└── .gitignore
```

---

## 🏗️ Tech Stack

| Komponente | Technologie |
|-----------|------------|
| Framework | ASP.NET Core 8 |
| UI | Blazor Server |
| Datenbank | SQLite + EF Core 8 |
| Testing | xUnit + Moq |
| Deployment | TrueNAS Docker |

---

## 📊 Test-Status

```
Total Tests: 51/51 GRÜN ✅

UC1 - Lebensmittel-Katalog:    19 Tests ✅
UC10 - ProduktInstanzen:        32 Tests ✅

Test Duration: ~86ms
Coverage Target: >80%
```

---

## 🔄 Development Workflow

### Pro Use-Case:
1. **Branch erstellen**: `git checkout -b feat/ucN-feature-name`
2. **Tests schreiben** (TDD Red): 20+ Unit Tests
3. **Code implementieren** (TDD Green): Service + Model
4. **Review**: Code-Quality überprüfen
5. **MR dokumentieren**: Reviews/UCN-*.md
6. **Merge**: `git merge feat/ucN-feature-name` → `master`
7. **Context updaten**: CONTEXT_SUMMARY.md

### Naming Conventions
- Branches: `feat/ucN-feature-name`
- Commits: `feat(uc-name): description` oder `test(uc-name): description`
- MR-Docs: `reviews/UCN-FeatureName-MR.md`

---

## 📚 Dokumentation

- **CONTEXT_SUMMARY.md** – Status & Kontext (für Session-Start lesen!)
- **CLAUDE.md** – Agent-Orchestration & Workflow
- **docs/architecture-overview.html** – Architecture Overview
- **reviews/*.md** – Merge Request Dokumentationen
- **diagrams/*.drawio** – UML & ER-Diagramme

---

## 🎯 Nächste Use-Cases (UC2-UC9)

| UC | Name | Status | Priority |
|----|------|--------|----------|
| 2 | Lagerbestand aktualisieren | ⏳ TODO | 🔴 High |
| 3 | Nährwerte verwalten | ⏳ TODO | 🔴 High |
| 4 | Rezepte verwalten | ⏳ TODO | 🟡 Medium |
| 5 | Nährwerte berechnen | ⏳ TODO | 🟡 Medium |
| 6 | Verbrauch ausbuchen | ⏳ TODO | 🔴 High |
| 7 | Verfallsdatum-Warnungen | ⏳ TODO | 🟡 Medium |
| 8 | Einkaufslisten generieren | ⏳ TODO | 🟡 Medium |
| 9 | Lagerorte verwalten | ⏳ TODO | 🟡 Medium |

---

## 💻 Development Tools

```bash
# Tests ausführen
dotnet test FoodDatabase.sln

# Mit Details
dotnet test FoodDatabase.sln --verbosity detailed

# Mit Coverage
dotnet test FoodDatabase.sln /p:CollectCoverage=true

# Build
dotnet build FoodDatabase.sln

# App starten
dotnet run --project src/App/FoodDatabase.App.csproj
```

---

## 📞 Support & Dokumentation

- **Bug Reports**: Issues in GitHub
- **Architecture Decisions**: `docs/architecture/`
- **Code Examples**: Siehe UC1 (`src/App/Services/Classes/LebensmittelService.cs`)

---

**Last Updated**: 2026-06-14  
**Status**: ✅ Production-Ready (UC1 & UC10) | 🚀 Ready for UC2-UC9
