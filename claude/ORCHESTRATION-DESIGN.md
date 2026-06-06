# ORCHESTRATION-DESIGN: FoodDatabase Agent System

## Übersicht

Dieses Dokument beschreibt die Architektur des **FoodDatabase Agent Orchestration Systems** - ein spezialisiertes Multi-Agent-System zur Entwicklung einer C#-Web-Anwendung mit striktem TDD, Clean Code und kontinuierlicher Dokumentation.

**Projekt**: FoodDatabase  
**Tech-Stack**: C# + ASP.NET Core + Blazor + SQLite (Monolith auf TrueNAS)  
**Clients**: Windows, Android, iOS (Browser-basiert)  
**Datum**: 2026-06-06

---

## Agent-Team (5 spezialisierte Rollen)

### 1. Orchestrator (Dirigent / Product Owner)
**Verantwortung**: Anforderungsanalyse, Planung, Framework-Entscheidungen

- **Einmalige Befragung**: User wird vollständig befragt (Anforderungen, Use-Cases, Constraints)
- **Use-Case-Diagramm**: Erstellt draw.io Diagramm aller Use-Cases
- **Framework-Entscheidung**: Evaluiert ASP.NET Core Optionen (Blazor Server vs. WASM, etc.)
- **Roadmap-Planung**: Plant Reihenfolge der Use-Case-Entwicklung
- **Gatekeeper**: Entscheidet, was gebaut wird und in welcher Reihenfolge

**Kommunikation**: User (Befragung) → Doc-Agent (Übergabe Use-Case-Diagramm) → Review-Agent (bei architektonischen Fragen)

---

### 2. Doc-Agent (Dokumentation & Diagramme)
**Verantwortung**: UML-Diagramme, Architektur-Dokumentation, HTML

- **Diagramm-Erstellung**: Basierend auf Use-Case-Diagramm erstellt:
  - **Klassen-Diagramm**: Domänen-Modell (Entities, Services, Repositories)
  - **Komponenten-Diagramm**: Architektur (Frontend, Backend, DB, externe Services)
  - **Deployment-Diagramm**: Container auf TrueNAS, physische Infrastruktur
  - **Sequenz-Diagramme**: Komplexe Workflows, Request-Response-Zyklen

- **Kontinuierliche Dokumentation**: 
  - Dokumentiert Code während Entwicklung
  - Aktualisiert Diagramme bei Architektur-Änderungen
  - Speichert Decision-Rationale

- **HTML-Generierung**: Erstellt HTML-Dokumentation mit eingebetteten draw.io Diagrammen

**Kommunikation**: Orchestrator (erhält Use-Case-Diagramm) → Dev/Test-Agents (dokumentiert deren Outputs) → User (stellt finale HTML bereit)

---

### 3. Test-Agent (TDD-Spezialist)
**Verantwortung**: Test-Driven Development, Unit-Tests

- **Test-First**: Schreibt Tests VOR Code (TDD-Reihenfolge)
- **Anforderungs-Übersetzung**: Setzt Use-Cases in Akzeptanzkriterien um
- **Abdeckung**: Happy Paths, Error Paths, Edge Cases
- **Framework**: xUnit oder NUnit (C#-Standard)

**Wichtig**: Wartet auf UML-Diagramme vom Doc-Agent bevor Tests geschrieben werden. Tests sind die Spezifikation für den Dev-Agent.

**Kommunikation**: Orchestrator (erhält Spezifikation) → Doc-Agent (wartet auf Diagramme) → Dev-Agent (übergibt Tests)

---

### 4. Dev-Agent (C#-Developer)
**Verantwortung**: Produktivcode, Clean Code, KISS

- **Code-Implementierung**: Implementiert Produktivcode zum Bestehen der Tests
- **Clean Code**: Strikte Einhaltung von Naming, Funktionsgröße, DRY
- **KISS-Prinzip**: Keine Over-Engineering, keine zu vielen Patterns
- **ASP.NET Core Stack**: Backend-APIs, Blazor Components, Datenbank-Zugriff

**Wichtig**: Wartet auf UML-Diagramme vom Doc-Agent bevor Code geschrieben wird. Tests sind die Spec.

**Kommunikation**: Test-Agent (erhält Tests) → Doc-Agent (wartet auf Diagramme) → Review-Agent (Code wird reviewt)

---

### 5. Review-Agent (Quality Assurance)
**Verantwortung**: Code-Review, Quality Gates, Eskalation

- **Strikte Review**: Prüft alle 7 Kriterien (siehe unten)
- **Feedback**: Detailliertes Feedback bei Fehlschlag
- **Eskalation**: Nach 3 Fehlschlägen → User wird konsultiert
- **Decision**: Bestanden (→ Doc-Agent dokumentiert) oder Fehlgeschlagen (→ Feedback an Dev/Test)

**Review-Kriterien** (alle müssen erfüllt sein):
1. Clean Code (Naming, Function Size, DRY)
2. KISS Principle (keine Over-Engineering)
3. Test Coverage (>80%)
4. Error Handling (explizit, klar)
5. Code Documentation (Inline-Comments)
6. Performance (keine N+1 Queries)
7. Security (Input Validation, SQL Injection, XSS, Secrets)

**Kommunikation**: Dev-Agent + Test-Agent (erhält Code/Tests) → Review (prüft) → Doc-Agent (bei Bestanden) oder zurück zu Dev/Test (bei Fehlschlag)

---

## Workflow (Phase-basiert)

### Phase 1: Requirements & Planning
```
1. User stellt Anforderung
2. Orchestrator befragt intensiv (einmalig!)
3. Orchestrator erstellt Use-Case-Diagramm (draw.io)
4. Doc-Agent erstellt weitere Diagramme:
   - Klassen-Diagramm
   - Komponenten-Diagramm
   - Deployment-Diagramm
   - Sequenz-Diagramme (falls nötig)
5. Orchestrator plant Entwicklungs-Roadmap
   (z.B. Authentifizierung → User-Management → Features)
```

### Phase 2: Iterative Entwicklung (pro Use-Case)
```
For Each Use-Case in Roadmap:
  
  Step 1: Diagramme sind finalisiert
    - Alle relevanten UML-Diagramme vorhanden
    - Test-Agent kann beginnen
  
  Step 2: Test-Agent schreibt Tests (TDD)
    - Basierend auf Use-Case + Diagrammen
    - Happy Path, Error Paths, Edge Cases
    - Doc-Agent dokumentiert Tests
  
  Step 3: Dev-Agent implementiert Code
    - Basierend auf Tests
    - Clean Code + KISS strikt
    - Doc-Agent dokumentiert Code
    - Diagramme werden ggf. aktualisiert
  
  Step 4: Review-Agent reviewt (max. 3 Versuche)
    - Prüft alle 7 Kriterien
    - Bei Bestanden → Step 5
    - Bei Fehlschlag (1-2x) → Feedback an Dev/Test, retry
    - Bei 3x Fehlschlag → User-Eskalation
  
  Step 5: Doc-Agent finalisiert Dokumentation
    - HTML mit eingebetteten Diagrammen
    - Architecture Decision Log aktualisiert
  
  Step 6: User-Review
    - User reviewt Feature (Code + Dokumentation + Diagramme)
    - Passt? → Go für nächsten Use-Case
    - Passt nicht? → Feedback an Orchestrator/Dev-Agent
```

---

## Kommunikations-Protokoll

**Typ**: API-basiert (nicht dateibasiert)  
**Format**: JSON  
**Prinzip**: "So viel nötig, so wenig wie möglich" - kurz und prägnant

### API-Endpoints (Übersicht)

#### Orchestrator
```
POST /orchestrator/analyze-requirements
Input:  { user_requirements: string }
Output: { use_case_diagram, framework_recommendation, roadmap }
```

#### Doc-Agent
```
POST /doc/create-diagrams
Input:  { use_case_diagram, specification, tech_stack }
Output: { class_diagram, component_diagram, deployment_diagram }

POST /doc/document
Input:  { code, tests, changes, decision_rationale }
Output: { html_documentation, updated_diagrams }
```

#### Test-Agent
```
POST /test/write-test
Input:  { use_case, specification, class_diagram, acceptance_criteria }
Output: { test_code, test_documentation, coverage_estimate }
```

#### Dev-Agent
```
POST /dev/implement-code
Input:  { test_code, class_diagram, specification, framework, database }
Output: { implementation_code, inline_documentation, status }
```

#### Review-Agent
```
POST /review/assess
Input:  { test_code, implementation_code, use_case, attempt_number }
Output: { status, criteria_checks, feedback, escalate_to_user, next_action }
```

---

## Quality Gates

### Review-Kriterien (strikte Einhaltung)

1. **Clean Code**
   - Aussagekräftige Namensgebung (Classes, Methods, Variables)
   - Funktionsgröße max. 20 Zeilen
   - Keine Magic Numbers/Strings (→ Constants/Enums)
   - DRY-Prinzip (Don't Repeat Yourself)

2. **KISS Principle**
   - Keine Over-Engineering
   - Nicht zu viele Abstraktionen/Patterns
   - Einfache, verständliche Lösung bevorzugt

3. **Test Coverage**
   - Alle relevanten Pfade getestet
   - Coverage >80% angestrebt
   - Tests sind aussagekräftig (nicht nur "grün")

4. **Error Handling**
   - Exceptions werden explizit gehandhabt
   - Null-Checks vorhanden wo nötig
   - Aussagekräftige Error Messages

5. **Code Documentation**
   - Code selbsterklärend oder mit Inline-Comments
   - Komplexe Logiken dokumentiert (WHY, nicht WHAT)

6. **Performance**
   - Keine N+1 Queries (DB-Optimierung)
   - Keine ineffizienten Loops/Algorithmen
   - Ressourcen (Memory, Connections) werden freigegeben

7. **Security**
   - Input Validation vorhanden
   - Keine SQL Injection Risiken (Parametrized Queries)
   - XSS/CSRF Schutz im Frontend
   - Secrets nicht hardcoded
   - Authentication/Authorization korrekt

### Eskalations-Logik

- **Versuch 1-2**: Fehlschlag → Detailliertes Feedback an Dev/Test-Agent
- **Versuch 3**: Fehlschlag → Feedback + User-Benachrichtigung
- **Nach Versuch 3**: Eskalation an User für Entscheidung (Rework nötig oder genehmigen als Exception?)

---

## Speicherstruktur

```
C:\Users\bened\source\repos\FoodDatabase\claude\
├── agents/
│   ├── orchestrator.md          # Orchestrator Profile
│   ├── dev-agent.md             # Dev-Agent Profile
│   ├── test-agent.md            # Test-Agent Profile
│   ├── review-agent.md          # Review-Agent Profile
│   └── doc-agent.md             # Doc-Agent Profile
├── orchestration-config.json    # Konfiguration (JSON)
├── ORCHESTRATION-DESIGN.md      # Dieses Dokument
└── orchestration/
    ├── inputs/
    │   └── requirements.json     # User-Anforderungen
    └── outputs/
        ├── diagrams/            # draw.io Dateien
        │   ├── use-case.drawio
        │   ├── class.drawio
        │   ├── component.drawio
        │   ├── deployment.drawio
        │   └── sequences/       # Sequenz-Diagramme
        ├── code/                # Generated Code
        ├── tests/               # Generated Tests
        ├── documentation/       # HTML + Decision Log
        └── reports/             # Review Reports, Coverage
```

---

## Wichtige Prinzipien

### 1. Einmalige Anforderungssammlung
- Orchestrator befragt User zu Beginn **einmalig und umfassend**
- **Keine Anforderungs-Änderungen** während Entwicklung
- Änderungen kommen nach Go-Live (Weiterentwicklung)

### 2. Diagramme ZUERST
- Alle relevanten UML-Diagramme müssen vor Entwicklung existieren
- Planung vor Implementierung
- Test- und Dev-Agents basieren auf Diagrammen

### 3. Test-Driven Development
- **Reihenfolge**: Test → Code (nicht Code → Test)
- Tests definieren die Spezifikation
- Dev-Agent implementiert zum Bestehen der Tests

### 4. Kontinuierliche Dokumentation
- Nicht erst am Ende, sondern parallel zur Entwicklung
- Test dokumentiert → Code dokumentiert → Diagramme aktualisiert

### 5. Clean Code + KISS (strikte Einhaltung)
- Keine Over-Engineering
- Einfache, verständliche Lösungen
- Wartbarkeit über Cleverness

### 6. Architektur-Rationale dokumentieren
- Nicht nur Was, sondern auch Warum
- Decision Log für alle architektonischen Entscheidungen
- Hilft zukünftigen Maintainern zu verstehen, warum so entschieden wurde

---

## Wie man das System verwendet

### 1. Projekt starten
```bash
# Start mit Orchestrator
POST /orchestrator/analyze-requirements
Input: { user_requirements: "Beschreibung der App" }
```

### 2. Diagramme erstellen
```bash
# Doc-Agent erstellt Diagramme
POST /doc/create-diagrams
Input: { use_case_diagram: ..., specification: ..., tech_stack: {...} }
```

### 3. Für jeden Use-Case iterieren
```bash
# Test schreiben
POST /test/write-test
Input: { use_case: "...", specification: "...", class_diagram: "..." }

# Code implementieren
POST /dev/implement-code
Input: { test_code: "...", class_diagram: "...", specification: "..." }

# Review
POST /review/assess
Input: { test_code: "...", implementation_code: "...", attempt_number: 1 }

# Dokumentieren (bei Bestanden)
POST /doc/document
Input: { code: "...", tests: "...", changes: "..." }

# User-Review
# User reviewt Feature + Dokumentation
# Go für nächsten Use-Case oder Feedback
```

### 4. Nach Use-Case-Entwicklung
- Feature ist komplett (Code + Tests + Dokumentation + Diagramme)
- User reviewt und gibt Go für nächsten Use-Case

---

## Fehlerszenarien & Lösungen

### Review fehlgeschlagen (Versuch 1-2)
- Review-Agent gibt detailliertes Feedback
- Dev-Agent oder Test-Agent reworked
- Retry → Nächster Review-Versuch

### Review fehlgeschlagen (Versuch 3)
- User wird konsultiert
- User entscheidet: Rework (zurück zu Dev/Test) oder Exception genehmigen

### User-Review fehlgeschlagen
- Feedback an Orchestrator/Dev-Agent
- Rework oder Scope-Anpassung
- Nächster Versuch

---

## Nächste Schritte

1. **Orchestrator starten**: User gibt Anforderungen ein
2. **Use-Case-Diagramm**: Orchestrator erstellt draw.io Diagramm
3. **Weitere Diagramme**: Doc-Agent erstellt Klassen-, Komponenten-, Deployment-Diagramme
4. **Erste Iteration**: Test-Agent → Dev-Agent → Review-Agent → Doc-Agent → User-Review
5. **Iterative Entwicklung**: Nächste Use-Cases folgen demselben Schema

---

**System ist bereit zur Verwendung!**

Kontakt: mc.bcb86@googlemail.com
