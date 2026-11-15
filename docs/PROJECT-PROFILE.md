# Projekt-Profil: FoodDatabase

Die verbindlichen Konventionen dieses Projekts für das Agent-Orchester. Die Methode steht in
[`QUALITY-BAR.md`](QUALITY-BAR.md); dieses Profil liefert die konkreten Regeln. Bei Konventionen
gewinnt das Profil, bei der Methode die Quality Bar. Korrekturen des Users sind maßgeblich.

## Stack

| Bereich | Festlegung |
| --- | --- |
| Sprache | C# auf .NET 8 (`net8.0`) |
| Frameworks | ASP.NET Core 8, Blazor Server, EF Core 8 mit SQLite |
| Build | `dotnet build FoodDatabase.sln` |
| Pakete | NuGet, keine neuen Pakete ohne Rückfrage |
| Deployment | Docker auf TrueNAS |

## Tests

| Bereich | Festlegung |
| --- | --- |
| Runner | xUnit (`[Fact]`, `[Theory]` + `[InlineData]`) |
| Assertions | xUnit-`Assert` — keine FluentAssertions |
| Mocking | Moq; für Repository-Integration EF Core InMemory bzw. SQLite |
| UI-Tests | bUnit, Selektoren über `data-testid` |
| Befehl | `dotnet test FoodDatabase.sln` vom Repo-Root — grün heißt die ganze Suite |
| Ablage | Produktivcode in `src/App/`, Tests in `src/Tests/`: `Unit/Services/`, `Unit/`, `Integration/`, `Ui/` |
| Benennung | Datei `<Klasse>Tests.cs`, Methode `Methode_Szenario_Erwartung` (z. B. `GetMhdText_ReturnsCorrectLocalizedText`); `// Arrange`, `// Act`, `// Assert` sichtbar |
| Hilfen | `src/Tests/Ui/TestHelpers/`, `src/Tests/Unit/Helpers/` (z. B. `FixedTimeProvider`) wiederverwenden |

## Schicht-Regel — welche Reihenfolge gilt

| Schicht | Reihenfolge | Bedeutung für implementer und reviewer |
| --- | --- | --- |
| **Service-Schicht** (`src/App/Services/`, Models) | **TDD** — Tests zuerst | Roter Test-Commit vor jedem Produktivcode, wie in der Quality Bar §1 |
| **UI-Schicht** (WP3–WP6, `src/App/Components/`, siehe `UI-PHASE-PLAN.md`) | **Code zuerst, dann Tests** | Code-Commit, danach ein Test-Commit, der **grün** ist. Ab dem Test-Commit gilt die Unveränderlichkeit der Tests genauso |

Der Feature-Plan nennt im Abschnitt „Reihenfolge“ ausdrücklich, welche Regel gilt. Fehlt die
Angabe, gilt TDD.

## Code-Konventionen (verbindlich)

| Regel | Richtig | Falsch |
| --- | --- | --- |
| Null-Prüfung mit `is null` / `is not null` | `if (obj is null)` | `if (obj == null)` |
| Explizite Typen | `List<Rezept> rezepte = new();` | `var rezepte = new List<Rezept>();` |
| `var` nur, wo der Typ sich aus einer Abfrage ergibt (LINQ) | `var aktive = rezepte.Where(r => r.Aktiv).ToList();` | `var rezept = new Rezept();` |
| Eine Klasse bzw. ein Interface pro Datei | `Rezept.cs` enthält nur `class Rezept` | zwei Typen in einer Datei |

Weiter, aus dem bestehenden Code:
- Domänenbegriffe deutsch, auch mit Umlauten (`NährwertService`, `RezeptNährwertAnzeige`).
- Services hinter Interfaces (`I<Name>Service`), per DI registriert.
- UI-Seiten mit Lade- und Fehlerzustand nach Vorbild `src/App/Components/RezeptNährwertAnzeige.razor`;
  vor dem Aufruf eines Service dessen Interface-Datei lesen.
- XML-Doc-Kommentare auf Deutsch.

## Dokumentation — der 4-Teil-Check

Ein Feature ist erst dokumentiert, wenn **alle vier** Teile stimmen. Teilaktualisierungen erzeugen
Inkonsistenz und sind ein Mangel.

1. **Code-Dokumentation** — Kommentare nur für nicht offensichtliche Logik; Fehlerbehandlung
   dokumentiert; nichts Veraltetes. Vorhandene Validierungsberichte unter `reviews/` zum Feature
   (z. B. `reviews/uc2-documentation-validation.md`) nachziehen.
2. **Feature-HTML** `docs/features/<UC>.html` (Vorlage `docs/features/TEMPLATE.html`) — Domain
   Model mit allen Entities und Feldern, Test-Coverage mit allen Testfällen, Workflow,
   Diagramm-Links, Status-Badge (`todo` → `done`).
3. **Master-Seite** `docs/architecture-overview.html` — UC-Karte (`⏳ Queue` → `✅ Fertig`),
   Entity-Status (`⏳ UCN` → `✅ UCN`), Projekt-Status „Abgeschlossen“ und „Nächste Schritte“.
4. **Diagramme** (draw.io) — `requirements/use-cases.drawio` (Status-Farbe, Legende),
   Sequenzdiagramme für neue Workflows, `diagrams/database-schema.drawio` für neue Entities und
   Relationen.

Konsistenz-Check: Sagt der Code „fertig“ und irgendeine dieser Stellen „todo“, ist das ein Mangel.

## Git

| Bereich | Festlegung |
| --- | --- |
| Branches | `feat/<thema>`, `fix/<thema>`, `docs/<thema>`, `refactor/<thema>`, `chore/<thema>` — nie auf `master` |
| Commits | Conventional Commits, **englisch**, Imperativ, Scope = Use-Case: `test(uc4): Cover the recipe pages`, `feat(uc4): Add …`, `docs(uc4): Document …` |
| Autor | `claudecodeagents[bot]` — repo-lokal in `.git/config`, nichts extra setzen |
| Push und PR | nur die Hauptsession, über das GitHub-App-Token (siehe `CLAUDE.md`) |

## Sprache

Alles, was das Orchester schreibt — Dokumentation, Pläne, Agent-Dateien, Kommentare, PR-Text — ist
**deutsch**. Ausnahmen: Commit-Nachrichten (englisch, siehe oben) und bestehende Code-Bezeichner.

## Tabu

- `backup/`, `bin/`, `obj/`, `*.db` — nie anfassen
- Abgeschlossene Review-Berichte unter `reviews/` (außer dem Validierungsbericht des aktuellen
  Features, siehe 4-Teil-Check) und historische Pläne (`UI-PHASE-PLAN.md`,
  `FIX-UI-TESTS-PLAN.md`, `docs/DOC-AGENT-PLAN.md`) — nicht umschreiben
- `C:\Users\bened\.github-app\` — außerhalb des Repos, der Private Key wird nie gelesen
