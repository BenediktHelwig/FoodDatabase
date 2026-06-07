# FoodDatabase – Multi-Agent Entwicklungs-Orchester

**Projekt**: C# / ASP.NET Core / Blazor Monolith mit SQLite auf TrueNAS  
**Status**: Agent-gesteuerte Entwicklung mit Git-basiertem Review-Workflow  
**Speicherort Agenten**: `C:\Users\bened\source\repos\FoodDatabase\claude\agents\`  
**Speicherort Outputs**: `C:\Users\bened\source\repos\FoodDatabase\`

---

## 🚀 WICHTIG: Vor jeder Sitzung lesen!

**Lese zuerst die `CONTEXT_SUMMARY.md`** (im Projekt-Root):
```bash
cat CONTEXT_SUMMARY.md
```

Diese Datei wird nach jeder abgeschlossenen Sitzung aktualisiert und enthält:
- ✅ Abgeschlossene Use-Cases
- ⏳ Verbleibende Aufgaben
- 📋 Aktueller Projekt-Status
- 🔄 Workflow & nächste Schritte

**Dies erspart dir, die ganze CLAUDE.md + Konversations-Historie zu lesen!**

---

## 🎼 Das Orchester-Team (5 Agenten)

Alle Agenten sind als spezialisierte Claude-Agent-Instanzen konfiguriert und arbeiten nach striktem Workflow:

### 1. **Orchestrator** (`orchestrator.md`)
- **Rolle**: Dirigent, Product Owner, Teamleiter
- **Verantwortung**:
  - Befragt User einmalig, umfassend (nutzt intensiven Fragestil)
  - Erstellt Use-Case-Diagramm (draw.io)
  - Evaluiert Best-Practice-Framework (ASP.NET Core Clean Architecture vs. Minimal APIs, etc.)
  - **Koordiniert den gesamten Workflow**
  - Erstellt Merge Requests für alle Agent-Outputs zum User-Review
- **Output-Format**: 
  - `requirements/use-cases.drawio` (draw.io XML)
  - Task-Assignments für Doc-Agent, Test-Agent, Dev-Agent
  - Pull Requests mit Konvention: `feat/[use-case-name]-orchestrated`

### 2. **Doc-Agent** (`doc-agent.md`)
- **Rolle**: Dokumentation, Diagramme, Architektur-Rationale
- **Verantwortung**:
  - Erstellt UML-Diagramme basierend auf Use-Case-Diagramm:
    - Klassen-Diagramm (Domänen-Modell)
    - Komponenten-Diagramm (Frontend, Backend, DB)
    - Deployment-Diagramm (Container auf TrueNAS)
    - Sequenz-Diagramme (komplexe Workflows)
  - Dokumentiert Code-Architektur und Decisions in HTML
  - Aktualisiert Diagramme kontinuierlich
  - Dokumentiert Tests und Produktivcode unmittelbar
- **Output-Format**:
  - `diagrams/[feature].drawio` (draw.io XML)
  - `docs/[feature].html` (mit eingebetteten Diagrammen)
  - `docs/architecture/[decision].md` (Architecture Decision Records)

### 3. **Test-Agent** (`test-agent.md`)
- **Rolle**: Test-Driven Development (TDD)
- **Verantwortung**:
  - Schreibt Tests **ZUERST** (Rot-Phase im TDD)
  - Dokumentiert Test-Logik unmittelbar
  - Wartet auf Diagramme vom Doc-Agent bevor Tests geschrieben werden
  - Validates Coverage und Edge-Cases
- **Output-Format**:
  - `src/Tests/[Feature]Tests.cs`
  - `reviews/test-report-[feature].md` (Test-Dokumentation)

### 4. **Dev-Agent** (`dev-agent.md`)
- **Rolle**: Produktivcode-Implementierung
- **Verantwortung**:
  - Implementiert Produktivcode basierend auf bestandenem Test (Grün-Phase im TDD)
  - Folgt Clean Code + KISS Prinzipien
  - Dokumentiert Code unmittelbar (Inline-Kommentare nur für NON-OBVIOUS)
  - Wartet auf Diagramme vom Doc-Agent bevor Code geschrieben wird
- **Output-Format**:
  - `src/App/[Domain]/[Feature].cs`
  - `src/App/[Domain]/[Feature].razor` (Blazor-Components)

### 5. **Review-Agent** (`review-agent.md`)
- **Rolle**: Qualitätssicherung
- **Verantwortung**:
  - Reviewt Test + Produktivcode
  - Prüfkriterien:
    - ✅ Clean Code (Naming, SOLID, Readability)
    - ✅ KISS Prinzip (Keine Over-Engineering)
    - ✅ Test-Coverage (>80%)
    - ✅ Fehlerbehandlung
    - ✅ Code-Dokumentation
    - ✅ Performance & Resource-Effizienz
    - ✅ Security (SQL Injection, XSS, Authentication)
  - **Eskalation**: 3 Fehlschläge → Escalation zu User (als Merge Request Comment)
- **Output-Format**:
  - `reviews/[feature]-review-[attempt].md` (Review-Feedback)

---

## 🔄 Entwicklungs-Prozess (Workflow)

### **Phase 1: Anforderungen & Planung**

```
1. Orchestrator befragt User
    ↓
2. Use-Case-Diagramm erstellt (requirements/use-cases.drawio)
    ↓
3. Doc-Agent erstellt UML-Diagramme (diagrams/)
    ↓
4. Orchestrator erstellt Merge Request: "feat/requirements-[date]"
    → User reviewt und approves
```

### **Phase 2: Iterative Entwicklung (pro Use-Case)**

```
FOR EACH USE-CASE:

  ┌─── Diagramm-Phase ────────────────────────┐
  │ 1. Use-Case definiert                     │
  │ 2. Alle UML-Diagramme vorhanden           │
  │ 3. Doc-Agent dokumentiert Architektur     │
  └───────────────────────────────────────────┘
           ↓
  ┌─── Test-Phase ────────────────────────────┐
  │ 1. Test-Agent schreibt Tests (TDD Rot)    │
  │ 2. Doc-Agent dokumentiert Tests           │
  │ 3. Merge Request: "test/[feature]"        │
  │    → Review-Agent überprüft               │
  │    → Max. 3 Fehlschläge → Eskalation      │
  └───────────────────────────────────────────┘
           ↓
  ┌─── Implementation-Phase ──────────────────┐
  │ 1. Dev-Agent implementiert Code (TDD Grün)│
  │ 2. Doc-Agent dokumentiert Code            │
  │ 3. Diagramme ggf. aktualisiert            │
  │ 4. Merge Request: "feat/[feature]"        │
  │    → Review-Agent überprüft               │
  │    → Max. 3 Fehlschläge → Eskalation      │
  └───────────────────────────────────────────┘
           ↓
  ┌─── User-Review-Phase ─────────────────────┐
  │ 1. User reviewt komplettes Feature        │
  │    - Code-Quality                         │
  │    - Funktionalität                       │
  │    - Dokumentation & Diagramme            │
  │ 2. User approved? → Merge & nächster UC   │
  │    User feedback? → Feedback an Agents    │
  └───────────────────────────────────────────┘
```

---

## 📂 Projektstruktur & Outputs

```
C:\Users\bened\source\repos\FoodDatabase\
│
├── CLAUDE.md                          ← Diese Datei
├── README.md                          ← Projekt-Übersicht für Entwickler
├── .gitignore                         ← GIT-Konfiguration
│
├── claude/                            ← Agent-Definitionen (NOT in GIT bei Output!)
│   ├── agents/
│   │   ├── orchestrator.md
│   │   ├── dev-agent.md
│   │   ├── test-agent.md
│   │   ├── review-agent.md
│   │   └── doc-agent.md
│   └── orchestration-config.json      ← Agent-Architektur & Endpoints
│
├── requirements/                      ← Use-Cases & Anforderungen
│   ├── use-cases.drawio               ← Use-Case-Diagramm (draw.io XML)
│   └── analysis.md                    ← Textuelle Anforderungs-Analyse
│
├── diagrams/                          ← Alle UML-Diagramme (draw.io)
│   ├── architecture-class.drawio      ← Klassen-Diagramm
│   ├── architecture-components.drawio ← Komponenten-Diagramm
│   ├── architecture-deployment.drawio ← Deployment (TrueNAS Container)
│   ├── workflow-[feature].drawio      ← Sequenz-Diagramme (optional)
│   └── ...
│
├── docs/                              ← HTML-Dokumentation
│   ├── index.html                     ← Landing-Page (Architecture Overview)
│   ├── features/
│   │   ├── [feature-name].html        ← Feature-Dokumentation (mit Diagrammen)
│   │   └── ...
│   ├── architecture/                  ← Architecture Decision Records (ADRs)
│   │   ├── adr-001-framework-choice.md
│   │   └── ...
│   └── api/                           ← API-Dokumentation (auto-generated oder manuell)
│
├── src/                               ← Produktivcode
│   ├── FoodDatabase.sln               ← Visual Studio Solution
│   ├── FoodDatabase.App/
│   │   ├── Program.cs                 ← ASP.NET Core Bootstrapping
│   │   ├── appsettings.json
│   │   ├── Components/                ← Blazor Components
│   │   │   ├── Layout/
│   │   │   ├── Pages/
│   │   │   └── ...
│   │   ├── Services/                  ← Business Logic
│   │   ├── Models/                    ← Domain Models
│   │   ├── Data/                      ← Database Context, Migrations
│   │   └── ...
│   │
│   └── FoodDatabase.Tests/
│       ├── Unit/
│       │   ├── Services/
│       │   ├── Models/
│       │   └── ...
│       ├── Integration/
│       │   └── ...
│       └── ...
│
├── reviews/                           ← Review-Reports
│   ├── feature-[name]-review-1.md     ← 1. Review-Versuch
│   ├── feature-[name]-review-2.md     ← 2. Review-Versuch (falls nötig)
│   ├── feature-[name]-review-3.md     ← 3. Review-Versuch (falls nötig)
│   └── escalations/                   ← Eskalierte Reviews
│       └── feature-[name]-escalation.md
│
└── .github/workflows/                 ← CI/CD-Pipelines (optional)
    ├── tests.yml                      ← Automatisierte Tests
    └── review.yml                     ← Code-Quality-Checks
```

---

## 🔄 Git & Merge Request Workflow

### **Merge Request Konventionen**

Alle MRs vom Orchestrator folgen dieser Struktur:

```
Title: feat/[use-case-name]-[phase]

Beispiele:
  - feat/requirements-food-database-initial
  - test/recipe-management-v1
  - feat/recipe-management-v1
  - docs/architecture-decisions-v1

Description Template:

## 📋 Überblick
[Kurze Beschreibung, was in diesem MR enthalten ist]

## 📊 Änderungen
- [Feature 1]
- [Feature 2]
- [Dokumentation/Diagramme]

## ✅ Checkliste
- [ ] Tests schreiben (TDD Rot-Phase)
- [ ] Produktivcode implementieren (TDD Grün-Phase)
- [ ] Tests bestanden
- [ ] Code-Review bestanden (Review-Agent)
- [ ] Dokumentation aktualisiert
- [ ] Diagramme aktualisiert

## 📎 Zugehörige Dateien
- requirements/use-cases.drawio
- diagrams/architecture-*.drawio
- docs/features/[feature-name].html
- src/Tests/[Feature]Tests.cs
- src/App/[Domain]/[Feature].cs

## 🎯 Nächste Schritte
[Was kommt nach diesem MR?]
```

### **Branching-Strategie**

```
main (Production-ready)
  ↑
develop (Integration-Branch)
  ↑
├─ feature/[use-case-name]-[phase]  ← Orchestrator erstellt Branches hier
├─ test/[feature-name]               ← Test-Agent commitet
├─ feat/[feature-name]               ← Dev-Agent commitet
└─ docs/[feature-name]               ← Doc-Agent commitet

GIT-Flow:
  develop ← test/[feature] (PR erstellen)
  develop ← feat/[feature] (PR erstellen)
  develop ← docs/[feature] (PR erstellen)
  main ← develop (Release)
```

### **Commit-Konventionen**

Alle Agents folgen **Conventional Commits**:

```
test(feature-name): Add tests for recipe creation
→ Test-Agent

feat(feature-name): Implement recipe creation logic
→ Dev-Agent

docs(feature-name): Add recipe feature documentation
→ Doc-Agent

docs(diagrams): Update class diagram for recipe domain
→ Doc-Agent

refactor(feature-name): Extract recipe validator logic
→ Dev-Agent (nur wenn Test-Agent zustimmt)
```

---

## 🛠️ Wie der Orchestrator MRs erstellt

### **Schritt 1: Orchestrator-Analyse**
```
Orchestrator.Analyze(UserRequirements)
  → Use-Case-Diagramm erstellen
  → Architecture-Entscheidungen treffen
  → Agentur-Tasks definieren
```

### **Schritt 2: Branch erstellen & Agenten-Arbeit**
```bash
# Orchestrator erstellt Feature-Branch
git checkout -b feat/[use-case-name]-phase1
git push -u origin feat/[use-case-name]-phase1

# Test-Agent commitet
git commit -m "test(feature): Add tests for [use-case]"
git push

# Dev-Agent commitet
git commit -m "feat(feature): Implement [use-case]"
git push

# Doc-Agent commitet
git commit -m "docs(feature): Add documentation and diagrams"
git push
```

### **Schritt 3: Orchestrator erstellt Merge Request**
```bash
# Orchestrator erstellt MR über CLI oder GitHub Web
gh pr create \
  --title "feat/[use-case-name]-v1" \
  --body "$(cat <<'EOF'
## 📋 Use-Case: [Name]
[Beschreibung]

## ✅ Was wurde erledigt
- [x] Requirements erfasst
- [x] Diagramme erstellt
- [x] Tests geschrieben (TDD Rot)
- [x] Code implementiert (TDD Grün)
- [x] Code-Review bestanden
- [x] Dokumentation aktualisiert

## 📎 Zugehörige Dateien
- requirements/use-cases.drawio
- diagrams/architecture-*.drawio
- src/Tests/[Feature]Tests.cs
- src/App/[Domain]/[Feature].cs
- docs/features/[feature].html

## 🎯 Nächste Schritte
[Weitere Use-Cases oder Integration]
EOF
)" \
  --base develop \
  --head feat/[use-case-name]-phase1
```

### **Schritt 4: User-Review & Merge**
```
User-Review im GitHub Web oder CLI:
  → gh pr view [PR-Number]
  → gh pr comment [PR-Number] -b "Approved" (oder Feedback)
  → Orchestrator mergt: gh pr merge [PR-Number]
```

---

## 🎯 Agenten-Konfiguration

### **Orts-Verweise für Agenten**

Wenn Agenten ihre Outputs generieren, nutzen sie:

```json
{
  "project_root": "C:\\Users\\bened\\source\\repos\\FoodDatabase",
  "agent_definitions": "C:\\Users\\bened\\source\\repos\\FoodDatabase\\claude\\agents",
  "outputs": {
    "requirements": "requirements/",
    "diagrams": "diagrams/",
    "documentation": "docs/",
    "source_code": "src/App/",
    "tests": "src/Tests/",
    "reviews": "reviews/"
  }
}
```

### **Agent-Invocation (Beispiel)**

```bash
# Orchestrator starten
claude agent --load "C:\Users\bened\source\repos\FoodDatabase\claude\agents\orchestrator.md"

# Test-Agent starten
claude agent --load "C:\Users\bened\source\repos\FoodDatabase\claude\agents\test-agent.md"

# Dev-Agent starten
claude agent --load "C:\Users\bened\source\repos\FoodDatabase\claude\agents\dev-agent.md"

# Doc-Agent starten
claude agent --load "C:\Users\bened\source\repos\FoodDatabase\claude\agents\doc-agent.md"

# Review-Agent starten
claude agent --load "C:\Users\bened\source\repos\FoodDatabase\claude\agents\review-agent.md"
```

---

## ✅ Qualitäts-Gates

### **Was muss bestanden werden, bevor ein MR gemergt wird?**

| Phase | Gatekeeper | Anforderung | Status |
|-------|-----------|-------------|--------|
| **Tests** | Test-Agent | Tests schreiben + bestehen | ✅ Automatisiert |
| **Code** | Dev-Agent | Code implementieren + Best Practices | ✅ Automatisiert |
| **Review** | Review-Agent | Code-Quality + Security + Performance | ✅ Automatisiert (3 Fehlschläge → Eskalation) |
| **Doku** | Doc-Agent | HTML + Diagramme aktualisiert | ✅ Automatisiert |
| **User** | User (du) | Feature passt zu Use-Cases | ⏳ Manuell |

---

## 🚀 Erste Schritte

### **1. Agenten initialisieren**
```bash
cd C:\Users\bened\source\repos\FoodDatabase

# Agenten-Dateien überprüfen
ls claude/agents/

# orchestration-config.json überprüfen
cat claude/orchestration-config.json
```

### **2. Git initialisieren** (falls noch nicht geschehen)
```bash
git init
git config user.name "Orchestrator"
git config user.email "mc.bcb86@googlemail.com"
git add .
git commit -m "Initial commit: Agent Orchestration setup"
git branch -M main
```

### **3. Orchestrator starten**
```bash
# Requirements erfassen
claude agent --load "claude/agents/orchestrator.md"

# Orchestrator wird User befragen und:
# 1. Use-Case-Diagramm erstellen (requirements/use-cases.drawio)
# 2. Merge Request erstellen für Review
```

---

## 📝 Wichtige Constraints & Prinzipien

| Prinzip | Beschreibung |
|---------|-------------|
| **Einmalige Anforderungssammlung** | Keine Änderungen während Entwicklung (später: Weiterentwicklung möglich) |
| **Diagramme ZUERST** | Planung vor Implementierung (Whiteboarding) |
| **Test-Driven Development** | Test → Code (nicht Code → Test) |
| **Dokumentation kontinuierlich** | Nicht erst am Ende der Entwicklung |
| **Clean Code + KISS** | Keine Over-Engineering, Simplicitas vor Komplexität |
| **Architektur-Rationale** | Gründe für Entscheidungen dokumentieren (ADRs) |
| **Review-Eskalation** | 3 Fehlschläge → User-Beteiligung + Diskussion |
| **Lokale Ausführung** | Keine Internet-Calls, alles läuft auf deinem System |

---

## 📞 Support & Troubleshooting

**Problem**: Agenten-Dateien nicht gefunden  
**Lösung**: Überprüfe den Pfad `C:\Users\bened\source\repos\FoodDatabase\claude\agents\`

**Problem**: Merge Request schlägt fehl  
**Lösung**: Stelle sicher, dass Git konfiguriert ist (`git config --list`)

**Problem**: Review-Agent eskaliert nach 3 Fehlschlägen  
**Lösung**: User überprüft das Feedback und gibt Agenten neue Anweisungen

---

**Status**: Bereit. Orchestrator kann mit User-Befragung starten. 🎼
