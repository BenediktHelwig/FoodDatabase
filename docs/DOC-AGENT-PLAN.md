# Doc-Agent Dokumentations-Plan

**Status**: 🚀 Vor UC2-Start müssen diese Dokumentationen erstellt werden

---

## ✅ ABGESCHLOSSEN

- [x] Use-Case-Diagramm (requirements/use-cases.drawio)
- [x] Anforderungs-Analyse (requirements/analysis.md)
- [x] Klassen-Diagramm (diagrams/architecture-class.drawio)
- [x] Komponenten-Diagramm (diagrams/architecture-components.drawio)
- [x] Deployment-Diagramm (diagrams/architecture-deployment.drawio)
- [x] UC1 MR-Dokumentation (reviews/UC1-LebensmittelKatalog-MR.md)
- [x] HTML-Übersicht (docs/architecture-overview.html)

---

## 🔄 IN ARBEIT (Doc-Agent Phase 2)

### 1. Feature-spezifische Dokumentation (Pro UC)

**Struktur**: `docs/features/UC<N>-<FeatureName>.html`

Für jede UC erstellen (UC2-UC10):
- [ ] UC2: Lagerbestand aktualisieren
- [ ] UC3: Nährwerte verwalten
- [ ] UC4: Rezepte verwalten
- [ ] UC5: Nährwerte berechnen (abhängig von UC4)
- [ ] UC6: Verbrauch ausbuchen
- [ ] UC7: Verfallsdatum-Warnungen
- [ ] UC8: Einkaufslisten generieren
- [ ] UC9: Lagerorte verwalten
- [ ] UC10: Produktinstanzen mit MHD

**Inhalt pro Feature-Seite**:
```html
docs/features/UC<N>-FeatureName.html
├── Übersicht (Was? Warum?)
├── Domain Model (Entities, Relationships)
├── Workflows (User-Flow, Diagramm)
├── API-Beispiele (CRUD-Operationen)
├── Tests-Info (Anzahl, Coverage)
├── Implementation-Status
└── Dependencies (Welche anderen UCs braucht diese?)
```

---

### 2. Sequenz-Diagramme (Komplexe Workflows)

**Struktur**: `diagrams/sequence-<workflow-name>.drawio`

**Priorität High**:
- [ ] `sequence-verfallsdatum-check.drawio` - Verfallsdatum-Überwachung (UC7)
- [ ] `sequence-einkaufsliste-generate.drawio` - Einkaufslisten-Generierung (UC8)
- [ ] `sequence-rezept-nährwert.drawio` - Nährwert-Berechnung (UC5)

**Inhalt**: Zeitliche Abfolge von Operationen mit Actors, Messages, Aktivierungen

---

### 3. Architecture Decision Records (ADRs)

**Struktur**: `docs/architecture/ADR-<NR>-<Decision>.md`

**Zu dokumentieren**:
- [ ] ADR-001: Warum ProduktInstanz zentral ist (statt einzelnes MHD)
- [ ] ADR-002: Repository Pattern Choice
- [ ] ADR-003: Blazor Server (statt Client-Side)
- [ ] ADR-004: SQLite (statt externe DB)
- [ ] ADR-005: Kaskaden-Delete für Produktinstanzen
- [ ] ADR-006: Async/Await durchgehend
- [ ] ADR-007: Interfaces/Classes Separation

**Format pro ADR**:
```markdown
# ADR-001: ProduktInstanz als zentrale Entity

## Context
[Problem beschreiben]

## Decision
[Entscheidung treffen]

## Consequences
[Vor- und Nachteile]

## Alternatives
[Was war anders möglich?]
```

---

### 4. Entity-Relationship-Diagramm (ER-Diagramm)

**Struktur**: `diagrams/database-schema.drawio`

**Inhalt**:
- Alle Tabellen mit Spalten
- Primary Keys, Foreign Keys
- Relationships (1:1, 1:N, N:M)
- Indexes
- Constraints (Unique, Check)

---

### 5. Data Dictionary

**Struktur**: `docs/data-dictionary.html` oder `docs/data-dictionary.md`

**Inhalt**: Für jede Entity
- Spalten-Namen
- Datentypen
- Beschreibungen
- Validierungsregeln
- Default-Werte

---

### 6. API-Dokumentation (Später, aber planen)

**Struktur**: `docs/api/`

- [ ] Swagger/OpenAPI Schema (auto-generated)
- [ ] REST Endpoints pro UC
- [ ] Request/Response-Beispiele
- [ ] Error-Handling

---

## 📋 PRIORITÄTS-REIHENFOLGE (für Doc-Agent Phase 2)

1. **🔴 Hoch**: Feature-Seiten (UC2-UC10) - Template-Struktur
2. **🔴 Hoch**: ER-Diagramm (Datenbank-Schema)
3. **🟡 Mittel**: Sequenz-Diagramme (Top 3)
4. **🟡 Mittel**: ADRs (Top 3)
5. **🟢 Niedrig**: Data Dictionary
6. **🟢 Niedrig**: API-Dokumentation (später)

---

## 🎯 ZIEL

**Nach Doc-Agent Phase 2**:
- ✅ Zentrale HTML-Übersichtsseite (architecture-overview.html)
- ✅ Feature-Seiten für alle 9 UCs (als leere Templates)
- ✅ ER-Diagramm (Datenbank-Schema)
- ✅ Top 3 Sequenz-Diagramme
- ✅ Top 3 ADRs
- ✅ Links & Navigation alles zusammen

**Dann können die anderen Agenten** UC2-UC10 implementieren, **ohne dass Dokumentation gestaut wird**.

---

## 📝 HINWEIS

Diese Dokumentationen **wachsen mit der Implementierung**:
- Feature-Seiten → Werden aktualisiert, wenn Code implementiert wird
- ADRs → Werden erstellt, wenn Entscheidungen getroffen werden
- Diagramme → Werden aktualisiert bei Design-Änderungen

**Kein "Dokumentation am Ende"-Problem!** 🎉

---

**Status**: Ready für Doc-Agent Session 🚀
