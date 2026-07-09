# 📊 Projekt-Statistik FoodDatabase

**Stand**: 2026-07-08 (nach UC8-Merge, Session 17 — Service-Schicht 10/10 UCs fertig, vor UI-Phase)

---

## Code-Umfang

| Metrik | Wert |
|---|---|
| C#-Dateien gesamt | 70 |
| Zeilen C#-Code gesamt | 9.515 |
| davon Produktivcode (`src/App`) | 3.986 Zeilen in 57 Dateien |
| davon Tests (`src/Tests`) | 5.529 Zeilen in 13 Dateien |
| davon EF-Migrations (generiert) | 652 Zeilen |
| Blazor-Komponenten (`.razor`) | 1 Datei, 224 Zeilen |

> **Test-zu-Produktivcode-Verhältnis: ca. 1,4 : 1** — mehr Testcode als Produktivcode,
> typisch und gesund für ein streng nach TDD entwickeltes Projekt.

## Typen

| Typ | Anzahl |
|---|---|
| Klassen | 52 |
| Interfaces | 16 |
| Enums | 1 |
| Records | 0 |

**Aufschlüsselung der Klassen:**
- 13 Service-Implementierungen (`Services/Classes`)
- 9 DTOs (`Services/Dtos`)
- 8 Domain-Models (`Models`)
- 3 Custom Exceptions (`Services/Exceptions`)
- 1 DbContext (`Data`)
- 12 Test-Klassen + 1 Test-Helper (`FixedTimeProvider`)
- Rest: Migrations-Klassen (generiert)

Die Regel „eine Klasse/Interface pro Datei" wird durchgehend eingehalten.

## Tests

| Metrik | Wert |
|---|---|
| Testmethoden (`[Fact]`/`[Theory]`) | **260** in 12 Test-Klassen |
| Größte Test-Suite | RezeptServiceTests (33 Tests) |
| Kleinste Test-Suite | VerbrauchAusbuchangServiceTests (10 Tests) |

**Tests pro Suite:**

| Test-Klasse | Tests |
|---|---|
| RezeptServiceTests | 33 |
| LagerbestandServiceTests | 29 |
| ProduktInstanzServiceTests | 29 |
| RezeptZutatServiceTests | 27 |
| LagerortServiceTests | 24 |
| RezeptNährwertServiceTests | 21 |
| VerfallsdatumWarnungServiceTests | 19 |
| NährwertServiceTests | 19 |
| EinkaufslistenServiceTests | 17 |
| LebensmittelServiceTests | 17 |
| NährwertCalculatorTests | 15 |
| VerbrauchAusbuchangServiceTests | 10 |

## Dokumentation & Projekt

| Metrik | Wert |
|---|---|
| Kommentarzeilen im Produktivcode | 1.165 (≈ 29 % der App-Zeilen) |
| HTML-Dokumentationsseiten | 12 |
| draw.io-Diagramme | 15 |
| Review-Reports | 25 |
| Git-Commits | 88 |

## Gesamtbild

Ein kompaktes, sauber strukturiertes Projekt mit ~10.000 Zeilen Code, sehr hoher
Testabdeckung (260 Tests für 13 Services, alle 10 Use-Cases) und ungewöhnlich
vollständiger Dokumentation. Der UI-Anteil ist mit nur einer Razor-Komponente noch
minimal — passend zum aktuellen Stand: Service-Schicht fertig, UI-Phase
(siehe `UI-PHASE-PLAN.md`) steht als Nächstes an.

---

*Erhoben am 2026-07-08 auf Branch `master` (Commit-Stand: 8b7d191). Zahlen ohne `obj/`/`bin/`-Verzeichnisse.*
