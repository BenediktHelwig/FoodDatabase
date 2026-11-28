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
├── CLAUDE.md                        # Agent-Orchester (Hauptsession)
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

## 🎼 Agent-Orchester (feature-crew)

Features entstehen über die [`feature-crew`](feature-crew/), ein testgetriebenes Orchester aus
Claude-Code-Agents. Sie liegt unverändert und englisch unter `feature-crew/` und wird nur als
Ganzes aus ihrem Quellprojekt ersetzt. Die Methode ist fest: Tests zuerst (bei UI-Arbeit Code
zuerst), dann die Implementierung, am Ende ein unabhängiges Review. Was dieses Projekt abweichend
verlangt, steht in den Projektregeln [`CLAUDE.md`](CLAUDE.md) und im Profil
[`docs/PROJECT-PROFILE.md`](docs/PROJECT-PROFILE.md) — beide haben Vorrang vor der Crew.

### Agents

| Agent | Modell | Aufgabe |
| --- | --- | --- |
| Hauptsession ([`feature-crew/CLAUDE.md`](feature-crew/CLAUDE.md)) | opus | Spricht mit dir, plant, delegiert, gibt frei, öffnet die Pull Request |
| `implementer` | sonnet | Tests → Code → Dokumentation, ein Commit pro Phase |
| `reviewer` | opus | Ein Review der ganzen Änderung, nur lesend |

Warum das Orchester so geschnitten ist, steht in
[`feature-crew/docs/ORCHESTRATION-DESIGN.md`](feature-crew/docs/ORCHESTRATION-DESIGN.md). Der
gemeinsame Maßstab ist [`feature-crew/docs/QUALITY-BAR.md`](feature-crew/docs/QUALITY-BAR.md), der
Plan-Aufbau [`feature-crew/docs/PLAN-FORMAT.md`](feature-crew/docs/PLAN-FORMAT.md).

### Starten

```bash
cd FoodDatabase/feature-crew
claude
```

Die Sitzung lädt `feature-crew/CLAUDE.md` und die Projektregeln aus der Root-`CLAUDE.md`. Token-Hook
und Freigaben für diese Sitzung stehen in der lokalen, nicht versionierten
`feature-crew/.claude/settings.local.json`.

Dann sagen, was gebaut werden soll:

- *„Setze folgendes Feature um: …“*
- *„Mach den offenen Plan weiter.“*

### Ablauf

| Modus | Was passiert | Modell |
| --- | --- | --- |
| **Plan Mode** (Start) | Resume-Check, Profil gelesen, Feature mit dir geklärt, Plan vorgelegt — alles durch die Hauptsession | opus |
| → | Du gibst den Plan frei: **Shift+Tab**, dann „los“ — ein Schritt, einmal pro Sitzung | — |
| **Auto Mode** | Branch und Plan geschrieben, `implementer` baut, `reviewer` prüft (bis zu drei Runden, dann entscheidest du), Pull Request | sonnet (Bau), opus (Review) |
| Nächstes Feature | **Neue Sitzung** starten — sie beginnt wieder im Plan Mode mit leerem Kontext | — |

### Was es ins Projekt schreibt

| Artefakt | Ort | Lebensdauer |
| --- | --- | --- |
| Projekt-Profil | `docs/PROJECT-PROFILE.md` | committet, dauerhaft |
| Projekt-Status | `CONTEXT_SUMMARY.md` | committet, nach jedem Feature aktualisiert |
| Feature-Plan | `.agents/plans/<feature>.md` | nicht committet, gelöscht, wenn das Feature fertig ist |
| PR-Text | `.agents/pull-request.md` | nicht committet |

Darüber hinaus schreibt es genau das, worum du gebeten hast: Tests, Implementierung und
Dokumentation (4-Teil-Check, siehe Profil).

### Git

Ein Branch vor der ersten Änderung, ein Commit pro Phase, damit die roten und grünen Schritte
sichtbar bleiben, und am Ende eine Pull Request — geöffnet über die GitHub App
`claudecodeagents[bot]`, damit du sie regulär freigeben kannst. Ohne Token bleiben Branch und
Commits lokal, und der Text wartet in `.agents/pull-request.md`. Du prüfst die ganze Änderung als
einen Diff und mergst sie; Anmerkungen kommen als zusätzlicher Commit zurück.
**Das Orchester mergt nie, force-pusht nie und pusht nie auf `master`.**

### Abhängigkeiten

| Skill | Herkunft | Zweck |
| --- | --- | --- |
| `grill-me` | `feature-crew/.claude/skills/grill-me/` | Feature-Klärung durch die Hauptsession |

### Naming Conventions
- Branches: `feat/…`, `fix/…`, `docs/…`, `refactor/…`, `chore/…`
- Commits: Conventional Commits auf Englisch, z. B. `feat(uc4): Add the recipe list`
- Details: [`docs/PROJECT-PROFILE.md`](docs/PROJECT-PROFILE.md)

---

## 📚 Dokumentation

- **CONTEXT_SUMMARY.md** – Status & Kontext (für Session-Start lesen!)
- **CLAUDE.md** – Agent-Orchester (Hauptsession), siehe Abschnitt „Agent-Orchester“
- **docs/PROJECT-PROFILE.md** – Projektkonventionen für das Orchester
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
