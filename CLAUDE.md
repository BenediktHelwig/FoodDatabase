# FoodDatabase – Projektregeln

**Projekt**: C# / ASP.NET Core / Blazor Monolith mit SQLite auf TrueNAS
**Agenten**: die unveränderte `feature-crew` unter [`feature-crew/`](feature-crew/)

Diese Datei enthält nur, was **dieses Projekt** von der Crew verlangt. Wie die Crew arbeitet,
steht in `feature-crew/CLAUDE.md` und `feature-crew/docs/`.

---

## ⚖️ Vorrang

**Diese Datei und [`docs/PROJECT-PROFILE.md`](docs/PROJECT-PROFILE.md) gehen
`feature-crew/CLAUDE.md` und `feature-crew/docs/*` vor.** Wo die Crew etwas anderes sagt — Sprache,
Testreihenfolge, Dokumentation, Git —, gilt die Projektregel. Wo das Projekt schweigt, gilt die
Crew.

## 🚀 Start

```powershell
cd feature-crew
claude
```

Die Sitzung lädt dann `feature-crew/CLAUDE.md` und diese Datei. **Vor jeder Sitzung
`CONTEXT_SUMMARY.md` lesen** — sie enthält erledigte Use-Cases, verbleibende Aufgaben und unter
„⏭️ NÄCHSTE SCHRITTE“ das nächste Feature. Gibt es keinen offenen Plan, geht es dort weiter.

## 📂 Projekt-Root

Das Projekt ist der **Repo-Root** (`FoodDatabase/`), nicht `feature-crew/`. Alle Projektpfade, die
die Crew nennt, gelten relativ zum Repo-Root — auch wenn die Sitzung in `feature-crew/` läuft:

| Was | Pfad |
| --- | --- |
| Projekt-Profil | `docs/PROJECT-PROFILE.md` — existiert, wird gelesen, nie neu entworfen |
| Feature-Pläne | `.agents/plans/<feature>.md` |
| PR-Text | `.agents/pull-request.md` |
| Projekt-Status | `CONTEXT_SUMMARY.md` |
| Code, Tests, Doku | `src/`, `docs/`, `diagrams/`, `requirements/` |

In Delegationen immer **absolute** Pfade übergeben. Quality Bar und Plan-Format liegen unter
`feature-crew/docs/`, das Profil unter `docs/`.

## 🔒 `feature-crew/` wird nie geändert

Die Dateien unter `feature-crew/` bleiben **englisch und unverändert** — nicht übersetzen, nicht
anpassen, nicht umformatieren. Sie werden im Quellprojekt gepflegt und hier nur als Ganzes ersetzt.
Projektspezifisches gehört in diese Datei oder ins Profil. (Eine Übersetzung hat schon einmal das
YAML-Frontmatter eines Agenten zerstört.)

## 🗣️ Sprache

Das Projekt ist **deutsch**: Antworten, Projekt-Dokumentation, Pläne, PR-Text und
`CONTEXT_SUMMARY.md`. Commit-Nachrichten sind englisch (siehe Profil). Die wörtlichen Sätze der
Crew (Startup-Check, Mode-Gate) sagt die Hauptsession sinngemäß auf Deutsch.

## 🧭 Abweichungen von der Methode

Stehen im Profil unter „Abweichungen von der Methode“ — vor allem die **Schicht-Regel** (UI: Code
zuerst, dann grüne Tests) und der **4-Teil-Check** der Dokumentation. Beim Klären legt die
Hauptsession fest, welche Schicht-Regel gilt, und der Plan nennt sie.

## ✅ Zusätzlich zur Freigabe-Checkliste der Crew

- [ ] Doku vollständig nach dem 4-Teil-Check des Profils
- [ ] Testreihenfolge nach der geltenden Schicht-Regel (TDD: rot vor grün · UI: Tests nach dem Code)
- [ ] `CONTEXT_SUMMARY.md` aktualisiert (Erledigtes raus, nächstes Feature rein) und von der
      Hauptsession committet

---

## 🔀 Git & Pull Request

Branch-Schema und Commit-Stil stehen im Profil. **Nie auf `master` pushen, nie force-pushen, nie
selbst mergen.** Review-Anmerkungen des Users kommen als zusätzlicher Commit auf denselben Branch.

**Die Pull Request wird über das Installation Access Token der GitHub App geöffnet, nicht über den
persönlichen `gh`-Login.** Das ersetzt den Weg über `gh auth status` aus `feature-crew/CLAUDE.md`.
Ausgeführt im Repo-Root:

```powershell
$env:GH_TOKEN = & "C:\Users\bened\.github-app\Get-GitHubAppToken.ps1"
git push -u origin HEAD
gh pr create --title "<Titel>" --body-file .agents/pull-request.md
```

Scheitert das Token-Skript (fehlender Key, App deinstalliert, kein Netz): Branch und Commits
bleiben lokal, der Text bleibt in `.agents/pull-request.md` zum manuellen Einfügen — kein Abbruch
wegen eines fehlenden Tools. **Kein Rückfall auf den persönlichen `gh`-Login** — eine PR unter dem
Namen des Users ist genau das Problem, das die App löst.

### Das Token gehört in jeden einzelnen Aufruf

`GH_TOKEN` muss in **jedem Tool-Aufruf** neu gesetzt werden, der pusht oder eine PR öffnet —
Shell-Variablen überleben den Aufruf nicht. Ein Aufruf ohne Token fällt still auf das persönliche
Konto zurück, und die PR ist dann nicht freigebbar. Ein PreToolUse-Hook weist solche Aufrufe ab —
er lässt sie nur durch, wenn im selben Kommando das Token-Skript steht. Er steht in
`.claude/settings.json` (Start im Root) und in der lokalen, nicht versionierten
`feature-crew/.claude/settings.local.json` (Start in `feature-crew/`). Er prüft auf `push` zusammen
mit `git` sowie auf `pr create`, deckt also auch `git -C <pfad> push` ab. Das ist bewusst grob: Ein
harmloses `git log --grep=push` wird mitblockiert. Setz dann einfach den Token-Abruf davor.

### Identität — wer commitet, wer die PR öffnet

- **Commits** laufen auf `claudecodeagents[bot]`. Das ist in `.git/config` repo-lokal hinterlegt
  und gilt automatisch — auch für die Commits des implementers.
- **PR-Autor** ist die App. Nur das entscheidet über die Freigabe: GitHub blockt Self-Approval am
  PR-Autor. Weil die PR vom Bot kommt, ist der User regulärer Reviewer statt Autor.
- **Das Token** ist 60 Minuten gültig und wird nie in eine Datei, eine Remote-URL oder in
  `.git/config` geschrieben. Es lebt nur in dem einen Kommando, das es setzt.
- **Den Private Key liest du nie.** Er wird ausschließlich vom Token-Skript verwendet; eine
  `deny`-Regel in `~/.claude/settings.json` sperrt den Lesezugriff zusätzlich technisch.

### Selbstprüfung — wenn du den Aufbau kontrollierst

`git config --get credential.helper` zeigt nur `manager` (GCM aus der System-gitconfig). Das ist
**kein** Hinweis auf ein Problem: Für `github.com` gilt ein host-spezifischer Eintrag in der
User-gitconfig, dessen erster, leerer Wert die geerbte Helper-Liste verwirft und danach `gh`
einträgt. Prüfe deshalb die Kette, die `git push` wirklich nimmt:

```powershell
"protocol=https`nhost=github.com`n`n" | git credential fill
```

Mit gesetztem `GH_TOKEN` muss dort `username=x-access-token` und ein `ghs_`-Passwort stehen. Kommt
`BenediktHelwig` mit einem `gho_`-Passwort, fehlt das Token im Aufruf.

---

## 📝 Prinzipien

| Prinzip | Beschreibung |
| --- | --- |
| **Einmalige Anforderungssammlung** | Keine Änderungen während der Entwicklung (später: Weiterentwicklung möglich) |
| **Dokumentation kontinuierlich** | Teil jedes Features, nicht erst am Ende |
| **Clean Code + KISS** | Kein Over-Engineering |
| **Lokale Ausführung** | Keine Internet-Calls außer `gh` und dem Token-Endpoint der GitHub App |

## 📂 Projektstruktur

```
FoodDatabase/
├── CLAUDE.md                    ← Diese Datei (Projektregeln, Vorrang vor der Crew)
├── CONTEXT_SUMMARY.md           ← Aktueller Projekt-Status, vor jeder Sitzung lesen
├── feature-crew/                 ← Agenten, englisch, unverändert — hier starten
│   ├── CLAUDE.md                 ← Hauptsession der Crew
│   ├── .claude/agents/           ← implementer, reviewer
│   ├── .claude/skills/grill-me/  ← Klärungs-Skill
│   └── docs/                     ← QUALITY-BAR, PLAN-FORMAT, ORCHESTRATION-DESIGN
├── .claude/settings.json         ← Token-Hook für Sitzungen im Root
├── .agents/                      ← NICHT versioniert: plans/, pull-request.md
├── requirements/                 ← use-cases.drawio, analysis.md
├── diagrams/                     ← Klassen-/Komponenten-/Deployment-/Sequenz-/ER-Diagramme
├── docs/
│   ├── PROJECT-PROFILE.md         ← Projektkonventionen, Abweichungen von der Methode
│   ├── architecture-overview.html ← Master-Übersicht, Status aller UCs
│   └── features/[UC].html         ← Feature-Dokumentation
├── src/
│   ├── App/                       ← Produktivcode (Services, Models, Components/Pages)
│   └── Tests/                     ← Unit-/Integration-/bUnit-Tests
└── reviews/                       ← Review-Berichte des alten Orchesters (Historie)

C:\Users\bened\.github-app\        ← außerhalb des Repos, nicht versioniert
├── claudecodeagents.private-key.pem ← Private Key der GitHub App (nie lesen)
├── Get-GitHubAppToken.ps1         ← liefert das Installation Access Token für Push + PR
└── README.md                      ← Permissions, Key-Rollover, weitere Repos anbinden
```
