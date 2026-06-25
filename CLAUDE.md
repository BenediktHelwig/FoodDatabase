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
- **Verantwortung**: **4-teilige Dokumentation nach jedem Code-Commit**
  1. **Code-Dokumentation** (Inline-Kommentare, Validierungsreports)
  2. **Feature-HTML** (z.B. `docs/features/UC2-Lagerbestand.html`)
  3. **Architecture-Overview.html** (Master-Seite mit Status aller UCs)
  4. **Diagramme** (use-cases.drawio, Sequence-Diagramme, ER-Diagramm)
  
  - Erstellt UML-Diagramme basierend auf Use-Case-Diagramm:
    - Klassen-Diagramm (Domänen-Modell)
    - Komponenten-Diagramm (Frontend, Backend, DB)
    - Deployment-Diagramm (Container auf TrueNAS)
    - Sequenz-Diagramme (komplexe Workflows)
  - Dokumentiert Code-Architektur und Decisions in HTML
  - **Aktualisiert Architecture-Overview.html mit UC-Status** (todo → done)
  - **Führt Konsistenz-Check durch** (4 Aspekte alle aktuell?)
- **Output-Format**:
  - `diagrams/[feature].drawio` (draw.io XML mit Status-Markierungen)
  - `docs/features/[feature].html` (Feature-Dokumentation)
  - `docs/architecture-overview.html` (Master-Übersicht - aktualisiert!)
  - `reviews/[feature]-documentation-validation.md` (Validierungsbericht)

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
- **Rolle**: Qualitätssicherung mit strukturiertem Feedback-Loop
- **Verantwortung**:
  - Validiert Tests nach Test-Agent-Commit (Testkonstruktion, Abdeckung, Edge-Cases)
  - Validiert Code nach Dev-Agent-Commit (Clean Code, Security, SOLID, KISS + Tests GRÜN?)
  - Validiert Dokumentation nach Doc-Agent 4-Teil-Check (Konsistenz aller 4 Aspekte)
  - **3-Schleifen-Feedback-Loop**: Max. 3× Feedback zwischen Review-Agent und dem betroffenen Agent
  - **Eskalation nach Schleife 3**: Falls nicht gelöst → User-Eskalation als Merge Request Comment
- **Test-Phase Review fokussiert auf**:
  - ✅ Testkonstruktion (Arrange/Act/Assert klar?)
  - ✅ Test-Abdeckung (Alle Szenarien bedacht?)
  - ✅ Edge-Cases (Fehlerfall, Boundary-Bedingungen?)
  - ✅ TDD-Logik (Tests als Spezifikation für Code?)
  - ⚠️ NICHT: "Sind Tests bestanden?" (Sie sind absichtlich rot in dieser Phase!)
- **Code-Phase Review fokussiert auf**:
  - ✅ Tests müssen ALLE GRÜN sein
  - ✅ Clean Code (Naming, SOLID, Readability)
  - ✅ KISS Prinzip (Keine Over-Engineering)
  - ✅ Fehlerbehandlung
  - ✅ Code-Dokumentation
  - ✅ Performance & Resource-Effizienz
  - ✅ Security (SQL Injection, XSS, Authentication)
- **Doku-Phase Review fokussiert auf**:
  - ✅ Code-Dokumentation konsistent
  - ✅ Feature-HTML vollständig
  - ✅ Architecture-Overview aktualisiert
  - ✅ Diagramme korrekt
- **Output-Format**:
  - `reviews/[feature]-[phase]-review-[attempt].md` (z.B. `uc3-code-review-1.md`)

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

  ┌─── Diagramm-Phase ────────────────────────────────────┐
  │ 1. Use-Case definiert                                 │
  │ 2. Alle UML-Diagramme vorhanden                       │
  │ 3. Doc-Agent dokumentiert Architektur                 │
  └───────────────────────────────────────────────────────┘
           ↓
  ┌─── TEST-PHASE mit Review-Loop ────────────────────────────────────────────┐
  │ 1. Test-Agent schreibt Tests (TDD ROT – absichtlich!)                     │
  │         ↓                                                                  │
  │ 2. Review-Agent: Testkonstruktion validieren (Versuch 1/3)               │
  │    (Abdeckung? Edge-Cases? Logik? TDD-Spezifikation?)                   │
  │    NICHT: "Sind Tests bestanden?" (Sie sind rot!)                         │
  │         ↓                                                                  │
  │    ✅ Tests freigegeben | ⚠️ Feedback (Loop 1/3)                         │
  │         ↓ (falls Feedback)                                                │
  │    Test-Agent überarbeitet Tests → Review-Agent (Versuch 2/3)            │
  │         ↓ (falls noch nicht OK)                                           │
  │    Test-Agent überarbeitet Tests → Review-Agent (Versuch 3/3)            │
  │         ↓ (falls IMMER NOCH nicht OK)                                    │
  │    ⛔ ESKALATION ZU USER                                                  │
  │         ↓ (Tests freigegeben)                                             │
  │ 3. Doc-Agent dokumentiert (4-Teil-Check):                                │
  │    ☐ Test-Dokumentation                                                  │
  │    ☐ Feature-HTML Seite                                                  │
  │    ☐ Architecture-Overview.html                                           │
  │    ☐ Diagramme aktualisiert                                              │
  └────────────────────────────────────────────────────────────────────────────┘
           ↓
  ┌─── IMPLEMENTATION-PHASE mit Review-Loop ──────────────────────────────────┐
  │ 1. Dev-Agent implementiert Code (TDD GRÜN)                               │
  │    Dev-Agent: Tests müssen ALLE GRÜN sein                                │
  │         ↓                                                                  │
  │ 2. Review-Agent: Code + Tests validieren (Versuch 1/3)                  │
  │    (Tests GRÜN? Clean Code? Security? SOLID? KISS?)                     │
  │         ↓                                                                  │
  │    ✅ Code freigegeben | ⚠️ Feedback (Loop 1/3)                          │
  │         ↓ (falls Feedback)                                                │
  │    Dev-Agent überarbeitet Code → Review-Agent (Versuch 2/3)              │
  │         ↓ (falls noch nicht OK)                                           │
  │    Dev-Agent überarbeitet Code → Review-Agent (Versuch 3/3)              │
  │         ↓ (falls IMMER NOCH nicht OK)                                    │
  │    ⛔ ESKALATION ZU USER                                                  │
  │         ↓ (Code freigegeben)                                              │
  │ 3. Doc-Agent dokumentiert (4-Teil-Check):                                │
  │    ☐ Code-Dokumentation + Validierungsreport                            │
  │    ☐ Feature-HTML Seite erweitert                                        │
  │    ☐ Architecture-Overview.html Status aktualisiert                      │
  │    ☐ use-cases.drawio + Diagramme mit Status                             │
  │         ↓                                                                  │
  │ 4. Review-Agent: Doku-Konsistenz validieren (Versuch 1/3)               │
  │    (Alle 4 Aspekte konsistent? Status OK?)                               │
  │         ↓                                                                  │
  │    ✅ Doku freigegeben | ⚠️ Feedback (Loop 1/3)                          │
  │         ↓ (falls Feedback)                                                │
  │    Doc-Agent überarbeitet Doku → Review-Agent (Versuch 2/3)              │
  │         ↓ (falls noch nicht OK)                                           │
  │    Doc-Agent überarbeitet Doku → Review-Agent (Versuch 3/3)              │
  │         ↓ (falls IMMER NOCH nicht OK)                                    │
  │    ⛔ ESKALATION ZU USER                                                  │
  │         ↓ (Doku freigegeben)                                              │
  │ 5. Definition-of-Done erfüllt ✅                                          │
  └────────────────────────────────────────────────────────────────────────────┘
           ↓
  ┌─── User-Review-Phase ─────────────────────────────────────┐
  │ 1. User reviewt komplettes Feature (alle Reviews bestanden)  │
  │    - Code-Quality                                          │
  │    - Funktionalität                                        │
  │    - Dokumentation & Diagramme                             │
  │ 2. User approved? → Merge & nächster UC                    │
  │    User feedback? → Feedback an relevante Agents          │
  └──────────────────────────────────────────────────────────────┘
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

## ✅ Qualitäts-Gates & Definition-of-Done

### **Was muss bestanden werden, bevor ein MR gemergt wird?**

| Phase | Agent | Verantwortung | Review-Gate | Status |
|-------|-------|----------------|-------------|--------|
| **Test-Phase** | Test-Agent | Tests schreiben (TDD Rot) | Review-Agent: Testkonstruktion (3-Loop) | ✅ Automatisiert |
| **Implementation** | Dev-Agent | Code schreiben + Tests GRÜN (TDD Grün) | Review-Agent: Code-Quality (3-Loop) | ✅ Automatisiert |
| **Dokumentation** | Doc-Agent | 4-Teil-Check (Code/HTML/Overview/Diagramme) | Review-Agent: Konsistenz (3-Loop) | ✅ Automatisiert |
| **MR-Finalisierung** | Review-Agent | Alle Reviews bestanden? MR-Ready? | - | ✅ Automatisiert |
| **User-Review** | User (du) | Feature passt zu Use-Cases? | - | ⏳ Manuell |

**Legende**: 3-Loop = Max. 3 Feedback-Schleifen zwischen Review-Agent und Agent. Nach Schleife 3 → Eskalation zu User.

---

### **🔴 KRITISCH: Doc-Agent Definition-of-Done (4-teilig)**

Nach **JEDEM Code-Commit** (Test-Agent + Dev-Agent) muss der Doc-Agent ALLE 4 Aspekte aktualisieren:

#### **1️⃣ Code-Dokumentation**
- Inline-Kommentare nur für NON-OBVIOUS Logik
- Validierungsreports aktualisieren (z.B. `reviews/uc2-documentation-validation.md`)
- Fehlerbehandlung dokumentiert
- Keine fehlenden oder veralteten Kommentare

#### **2️⃣ Feature-HTML-Seite** (z.B. `docs/features/UC2-Lagerbestand.html`)
- Domain Model Sektion: Alle Entities + Felder dokumentiert
- Test-Coverage Sektion: Alle Test-Cases gelistet
- Workflow Sektion: Geschäftslogik erklärt
- Links zu Diagrammen aktuell
- Status-Badge aktualisiert (todo → done wenn fertig)

#### **3️⃣ Architecture-Overview.html** (Master-Seite)
- **Use-Cases Sektion**: UC-Karte Status aktualisiert
  - Von `<div class="uc-card todo">` → `<div class="uc-card done">` wenn fertig
  - Status-Badge: Von `⏳ Queue` → `✅ Fertig (gemergt)`
- **Domain Model Sektion**: Entity-Status aktualisiert
  - Von `⏳ UCN` → `✅ UCN` wenn fertig
- **Projekt-Status Sektion**: 
  - "Abgeschlossen" hinzufügen
  - "Nächste Schritte" aktualisieren

#### **4️⃣ Diagramme** (draw.io)
- **use-cases.drawio**: 
  - UC-Status mit Farbcodierung (✅ Grün = done, Weiß = todo)
  - Legend aktualisiert
- **Sequence-Diagramme** (z.B. `diagrams/sequence-uc2-lagerbestand.drawio`):
  - Neue Workflows hinzufügen
  - Beschreibungen aktualisieren
- **ER-Diagramm** (`diagrams/database-schema.drawio`):
  - Neue Entities/Relationen hinzufügen falls Model sich ändert

---

### **⚠️ Fehlerkontrolle: Ist die Dokumentation konsistent?**

Vor MR-Erstellung muss folgende Prüfliste bestanden werden:

```
KONSISTENZ-CHECK:

☐ Code-Dokumentation:
  ☐ Alle Services/Models dokumentiert
  ☐ Alle Tests beschrieben
  ☐ Validierungsreport aktualisiert

☐ Feature-HTML:
  ☐ Status-Badge aktuell (todo/done)
  ☐ Domain Model vollständig
  ☐ Tests dokumentiert
  ☐ Verweise zu Diagrammen gültig

☐ Architecture-Overview:
  ☐ UC-Karte Status aktualisiert
  ☐ Domain Model Status aktualisiert
  ☐ Projekt-Status Sektion aktualisiert
  ☐ Status ist KONSISTENT mit Code

☐ Diagramme:
  ☐ use-cases.drawio: UC-Status mit Farben
  ☐ Sequence-Diagramme: Workflows dokumentiert
  ☐ ER-Diagramm: Neue Entities eingetragen

☐ KONSISTENZ-CHECK:
  ☐ Code sagt "fertig", aber Feature-HTML sagt "todo"? → FEHLER!
  ☐ Architecture-Overview zeigt falsche Status? → FEHLER!
  ☐ Diagramme zeigen alte Struktur? → FEHLER!
  
  → KEINE FEHLER = Doc-Agent fertig
```

**Folge**: Wenn nur EINZELNE Aspekte aktualisiert werden (z.B. nur Diagramme, aber nicht Overview), entsteht Inkonsistenz wie in UC2/UC10. Das ist NICHT akzeptabel.

---

### **🔄 Doc-Agent Workflow (Pseudocode)**

```
NACH Test-Agent Commit:
  1. Test-Dokumentation schreiben
  2. Diagramme prüfen (noch aktuell?)
  3. Feature-HTML aktualisieren
  4. commit("docs(feature): Add test documentation")

NACH Dev-Agent Commit:
  1. Code-Dokumentation schreiben
  2. Feature-HTML aktualisieren (Domain Model, Workflows)
  3. Diagramme ggf. erweitern
  4. architecture-overview.html aktualisieren (Status!)
  5. use-cases.drawio aktualisieren (Farben)
  6. Validierungsreport schreiben
  7. commit("docs(feature): Add code documentation and diagrams")

FINAL CHECK:
  → Alle 4 Aspekte aktuell?
  → Keine Inkonsistenzen?
  → Dann: READY FOR MR
```

---

### **🔄 Review-Agent Workflow & 3-Schleifen-Feedback**

Der Review-Agent arbeitet mit strukturiertem Feedback-Loop nach jeder Agent-Arbeit:

#### **Test-Phase Review**

```
TRIGGER: Nach Test-Agent-Commit

Review fokussiert auf (NICHT auf Bestanden-Status, Tests sind absichtlich ROT!):
  1. Testkonstruktion: Arrange/Act/Assert klar und sinnvoll?
  2. Test-Abdeckung: Werden alle Szenarien abgedeckt?
  3. Edge-Cases: Fehlerfall, Boundary-Bedingungen, Sonderfälle?
  4. TDD-Logik: Dienen Tests als Spezifikation für den Dev-Agent?

FEEDBACK-LOOP:
  Versuch 1/3: Review-Agent prüft Tests
    → Tests OK? ✅ Freigabe für Doc-Agent
    → Probleme? ⚠️ Feedback an Test-Agent

  Versuch 2/3: Test-Agent überarbeitet → Review-Agent schaut nochmal
    → Tests OK? ✅ Freigabe für Doc-Agent
    → Immer noch Probleme? ⚠️ Feedback an Test-Agent

  Versuch 3/3: Test-Agent überarbeitet → Review-Agent schaut nochmal
    → Tests OK? ✅ Freigabe für Doc-Agent
    → Immer noch nicht OK? ⛔ ESKALATION ZU USER
       Message: "Test-Konstruktion nach 3 Feedback-Schleifen nicht akzeptabel.
                 User-Entscheidung erforderlich."

OUTPUT: reviews/[feature]-test-review-[attempt].md
```

#### **Code-Phase Review**

```
TRIGGER: Nach Dev-Agent-Commit (wenn Tests alle GRÜN sind)

Review fokussiert auf:
  1. Tests GRÜN? (TDD Grün-Phase erfolgreich?)
  2. Clean Code: Naming, Leserlichkeit, SOLID-Prinzipien?
  3. Security: SQL Injection? XSS? Authentication/Authorization?
  4. KISS: Keine Over-Engineering, Simplicitas?
  5. Error Handling: Fehlerbehandlung korrekt?
  6. Performance: Resource-Effizienz, Skalierbarkeit?
  7. Code-Dokumentation: Inline-Kommentare für NON-OBVIOUS Logik?

FEEDBACK-LOOP:
  Versuch 1/3: Review-Agent prüft Code + Tests
    → Code OK? ✅ Freigabe für Doc-Agent
    → Probleme? ⚠️ Feedback an Dev-Agent

  Versuch 2/3: Dev-Agent überarbeitet → Review-Agent schaut nochmal
    → Code OK? ✅ Freigabe für Doc-Agent
    → Immer noch Probleme? ⚠️ Feedback an Dev-Agent

  Versuch 3/3: Dev-Agent überarbeitet → Review-Agent schaut nochmal
    → Code OK? ✅ Freigabe für Doc-Agent
    → Immer noch nicht OK? ⛔ ESKALATION ZU USER
       Message: "Code-Quality nach 3 Feedback-Schleifen nicht akzeptabel.
                 User-Entscheidung erforderlich."

OUTPUT: reviews/[feature]-code-review-[attempt].md
```

#### **Dokumentation-Phase Review**

```
TRIGGER: Nach Doc-Agent 4-Teil-Check (komplett)

Review fokussiert auf (alle 4 Aspekte zusammen):
  1. Code-Dokumentation: Alle Services/Models/Tests dokumentiert?
  2. Feature-HTML: Status-Badge, Domain Model, Tests, Diagramme aktuell?
  3. Architecture-Overview: UC-Status, Entity-Status, Projekt-Status aktualisiert?
  4. Diagramme: use-cases.drawio mit Farben, Sequence-Diagramme, ER-Diagramm?
  5. Konsistenz: Sagen alle 4 Aspekte das gleiche? Oder widersprechen sie sich?

FEEDBACK-LOOP:
  Versuch 1/3: Review-Agent prüft Doku-Konsistenz
    → Alles OK? ✅ Freigabe für MR-Erstellung
    → Probleme? ⚠️ Feedback an Doc-Agent

  Versuch 2/3: Doc-Agent überarbeitet → Review-Agent schaut nochmal
    → Alles OK? ✅ Freigabe für MR-Erstellung
    → Immer noch Probleme? ⚠️ Feedback an Doc-Agent

  Versuch 3/3: Doc-Agent überarbeitet → Review-Agent schaut nochmal
    → Alles OK? ✅ Freigabe für MR-Erstellung
    → Immer noch nicht OK? ⛔ ESKALATION ZU USER
       Message: "Dokumentation nach 3 Feedback-Schleifen nicht konsistent.
                 User-Entscheidung erforderlich."

OUTPUT: reviews/[feature]-doku-review-[attempt].md
```

#### **MR-Finalisierung Review**

```
TRIGGER: Vor User-Review (optional, falls alle anderen Reviews bestanden)

Review prüft:
  1. Sind alle vorherigen Reviews (Test/Code/Doku) bestanden?
  2. Ist MR-Beschreibung vollständig und korrekt?
  3. Sind alle Änderungen dokumentiert und nachverfolgbar?

OUTPUT: reviews/[feature]-mr-final-check.md (oder kurz im MR-Comment)
```

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
