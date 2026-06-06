# Agenten-Orchester Projekt - FoodDatabase

## Projekt-Übersicht
**Ziel**: Multi-Agent-System zur Entwicklung einer C#-Web-App (Monolith), die als Container auf TrueNAS läuft.

**Clients**: Windows, Android, iOS Geräte greifen via Browser zu (keine Native Apps).

**Speicherort**: `C:\Users\bened\source\repos\FoodDatabase\claude`

---

## Orchester-Team (5 Agenten)

### 1. Orchestrator (Dirigent/Product Owner/Teamleiter)
- Befragt User einmalig, umfassend
- Erstellt Use-Case-Diagramm (draw.io)
- Leitet Anforderungen an Doc-Agent weiter für weitere Diagramme
- Entscheidet Best-Practice-Framework basierend auf Hosting, Use-Case-Typ, Komplexität

### 2. Doc-Agent (Dokumentation & Diagramme)
- Erstellt basierend auf Use-Case-Diagramm weitere UML-Diagramme (draw.io):
  - Klassen-Diagramm (Domänen-Modell)
  - Komponenten-Diagramm (Architektur: Frontend, Backend, DB)
  - Deployment-Diagramm (Container auf TrueNAS)
  - Sequenz-Diagramme (für komplexe Workflows)
- Dokumentiert Code, Architektur, Decisions in HTML (mit eingebetteten draw.io Diagrammen)
- Aktualisiert Diagramme kontinuierlich während Entwicklung

### 3. Test-Agent (TDD - Tests schreiben)
- Schreibt Tests ZUERST (Test-Driven Development)
- Dokumentiert Tests unmittelbar
- Wartet auf Diagramme vom Doc-Agent bevor Tests geschrieben werden

### 4. Dev-Agent (Produktivcode)
- Implementiert Produktivcode basierend auf bestandenem Test
- Folgt Clean Code + KISS Prinzipien
- Dokumentiert Code unmittelbar
- Wartet auf Diagramme vom Doc-Agent bevor Code geschrieben wird

### 5. Review-Agent (Qualitätssicherung)
- Reviewt Test + Produktivcode
- Prüfkriterien: 
  - Clean Code
  - KISS Prinzip
  - Test-Coverage
  - Fehlerbehandlung
  - Code-Dokumentation
  - Performance
  - Security
- 3 Fehlschläge → Eskalation an User

---

## Entwicklungs-Prozess (Workflow)

### Phase 1: Anforderungen & Planung
1. Orchestrator befragt User einmalig, umfassend
2. Orchestrator erstellt Use-Case-Diagramm (draw.io)
3. Doc-Agent erstellt weitere UML-Diagramme basierend darauf (Klassen, Komponenten, Deployment, Sequenz)

### Phase 2: Iterative Entwicklung (pro Use-Case)
1. **Diagramm-Phase** (Orchestrator + Doc-Agent):
   - Use-Case definiert
   - Alle relevanten UML-Diagramme vorhanden

2. **Test-Phase** (Test-Agent):
   - Schreibt Test basierend auf Diagrammen
   - Doc-Agent dokumentiert Test

3. **Implementation-Phase** (Dev-Agent):
   - Implementiert Produktivcode basierend auf Test
   - Doc-Agent dokumentiert Code
   - Diagramme werden ggf. aktualisiert

4. **Review-Phase** (Review-Agent):
   - Reviewt Test + Code
   - Bestanden? → Nächster Use-Case
   - Fehlgeschlagen? → Feedback, max. 3 Versuche
   - 3x fehlgeschlagen? → User-Eskalation

5. **User-Review**:
   - User reviewt komplettes Feature (Code + Dokumentation + Diagramme)
   - Passt? → Go für nächsten Use-Case
   - Passt nicht? → Feedback an Orchestrator/Dev-Agent

---

## Tech-Stack
- **Sprache**: C#
- **Backend**: ASP.NET Core
- **Frontend**: Blazor (integriert im Monolith)
- **Architektur**: Monolith (Backend + Frontend im selben Container)
- **Datenbank**: SQLite
- **Hosting**: Container auf TrueNAS
- **Framework-Entscheidung**: Orchestrator evaluiert und empfiehlt
- **Diagramm-Tool**: draw.io (XML-basiert, versionierbar, HTML-einbettbar)
- **Dokumentation**: HTML mit eingebetteten draw.io Diagrammen

---

## Kommunikation zwischen Agenten
- **Protokoll**: API-basiert (nicht dateibasiert)
- **Prinzip**: "So viel nötig, so wenig wie möglich" - kurz und prägnant
- **Tentative API-Endpoints**:
  - `POST /orchestrator/analyze-requirements` → Anforderungen analysieren, Use-Cases planen
  - `POST /doc/create-diagrams` → UML-Diagramme erstellen
  - `POST /test/write-test` → Test schreiben
  - `POST /dev/implement-code` → Produktivcode implementieren
  - `POST /review/assess` → Review durchführen
  - `POST /doc/document` → Dokumentation (HTML + Diagramme)

---

## Speicherstruktur
```
C:\Users\bened\source\repos\FoodDatabase\claude\
├── agents/
│   ├── orchestrator.md
│   ├── dev-agent.md
│   ├── test-agent.md
│   ├── review-agent.md
│   └── doc-agent.md
├── orchestration-config.json     # Architektur, Endpoints, Flow
├── ORCHESTRATION-DESIGN.md       # Übersicht und Dokumentation
└── orchestration/
    └── outputs/                  # Agenten-Outputs
```

---

## Wichtige Constraints & Prinzipien
- **Einmalige Anforderungssammlung**: Keine Änderungen während Entwicklung (später: Weiterentwicklung)
- **Diagramme ZUERST**: Planung vor Implementierung
- **TDD**: Test → Code (nicht Code → Test)
- **Dokumentation kontinuierlich**: Nicht erst am Ende
- **Clean Code + KISS**: Keine Over-Engineering
- **Architektur-Rationale**: Gründe für Entscheidungen dokumentieren
- **Review-Eskalation**: 3 Fehlschläge → User-Beteiligung

---

**Status**: Bereit zur Generierung der Agent-Profile und Konfiguration.
