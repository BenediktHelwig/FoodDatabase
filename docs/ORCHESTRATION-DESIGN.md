# Orchestrierungs-Design: feature-crew

## Problem

Ein testgetriebenes Orchester mit **fester Methode** und **angeeigneter Fachkenntnis**: Sprache,
Frameworks, Test-Runner und Konventionen kommen aus dem Projekt-Profil
([`PROJECT-PROFILE.md`](PROJECT-PROFILE.md)), nicht aus den Agent-Dateien. Es begann als fünf
Rollen in diesem Repository, gewachsen in den Stack von FoodDatabase.

Die zweite Überarbeitung (2026-11) zielte auf die Kosten. Ein Feature brauchte **acht Kaltstarts** —
`plan-writer`, `test-agent`, Review, `dev-agent`, Review, `doc-agent`, Review, wieder
`plan-writer` — und jeder las Profil und Plan neu. `haiku` schrieb echten Code und echte Tests, was
Korrekturrunden erzeugte, und jede Runde kostete ein weiteres Opus-Review. Das Ziel: weniger
Kaltstarts bei gleicher Qualität.

## Rollenschnitt

| Rolle | Modell | Schnittkriterium | Begründung |
| --- | --- | --- | --- |
| Hauptsession | opus | — | Spricht mit dem User, kann also kein Subagent sein. Plant, schreibt die Planungsdokumente, gibt frei, führt Branch und Pull Request. |
| `implementer` | sonnet | 3 · großer, in sich geschlossener Block für ein günstigeres Modell | Tests, Code und Doku berühren dieselben Dateien und gehören in einen Kontext. `sonnet` schreibt verlässlich echten Code; `haiku` brauchte zu viele Korrekturrunden. |
| `reviewer` | opus | 1 · frischer Blick, 4 · eingeschränkte Tools | Sieht den Diff, nicht das Gespräch. Ein anderes Modell als der Autor, damit es nicht dessen blinde Flecken teilt. Hat keine Schreib-Tools. |

Ein Feature kostet jetzt **zwei Kaltstarts** plus einen pro Korrekturrunde, und ein Opus-Review
statt drei.

### Wer wann auf welchem Modell arbeitet

| Phase | Wer | Modell |
| --- | --- | --- |
| Plan Mode: klären, planen | Hauptsession | opus |
| Freigabe | User: Shift+Tab, dann „los“ | — |
| Auto Mode: Tests, Code, Doku | `implementer` (Subagent) | sonnet |
| Review | `reviewer` (Subagent) | opus |
| Freigabe, Pull Request | Hauptsession | opus |

Den Plan schreibt nicht der `implementer`, sondern die Hauptsession. Der `implementer` wird erst
nach dem „los“ aufgerufen. Ein Subagent wechselt sein Modell nie mitten im Lauf — es steht fest im
Frontmatter.

## Ablauf

**Plan Mode:** Startup-Check → Resume-Check → Profil lesen → grill-me → Plan vorlegen.
Plan-Freigabe und Mode-Gate sind **ein** Schritt: Shift+Tab, dann „los“.

**Auto Mode:** Branch → Plan geschrieben von der Hauptsession → `implementer` (Test-Commit →
Implementierungs-Commit → Doku-Commit, bei UI Code vor Tests) → `reviewer` über den ganzen Diff →
Pull Request → Plan gelöscht → für das nächste Feature eine neue Sitzung empfohlen.

**Eskalation:** drei Versuche. Zwei leise, der dritte angekündigt, dann entscheidet der User. Ein
Mangel, der zwei Versuche überlebt, heißt: der Auftrag wird neu gefasst. Eine Blockade-Meldung des
implementers ist kein Versuch.

## TDD ohne eigene Test-Rolle

Der alte Schnitt garantierte, dass wer die Tests schrieb, sie nicht grün machte. Die Garantie kommt
jetzt aus Historie und Review: Der implementer committet die roten Tests vor jedem Code, und der
reviewer prüft, dass `git diff <rot-sha>..HEAD` über die Testdateien leer ist. Einzige Ausnahme ist
ein Test, den der reviewer selbst als mangelhaft benennt — er wird in einem eigenen Commit
`fix tests per review` korrigiert, und die Prüfung zählt ab dort.

Für die UI-Schicht (WP3–WP6) gilt laut Profil „Code zuerst, dann grüne Tests“. Dort prüft der
reviewer, dass der Test-Commit nach dem Code-Commit kommt und danach unverändert bleibt.

## Geteilte Dateien statt wiederholter Regeln

Der Maßstab, den jede Rolle anlegt, steht einmal in [`QUALITY-BAR.md`](QUALITY-BAR.md); das
Plan-Layout samt Übertragung alter Pläne in [`PLAN-FORMAT.md`](PLAN-FORMAT.md); die
Projektkonventionen im Profil. Agent-Dateien enthalten nur ihre rollenspezifische Arbeitsweise. Die
Dateien werden bei Bedarf gelesen und bewusst nicht per `@import` in `CLAUDE.md` eingebunden — das
würde sie in jeder Runde in den Hauptkontext laden. Personas sind ein Satz Haltung: allgemeine
Seniorität-Behauptungen änderten das Verhalten nicht, konkrete Regeln und Beispiele schon.

## Permission-Modes

Der Modus gehört der Sitzung und wird von jedem Subagenten geerbt; er ist keine Rolleneigenschaft.
Es gibt ein Gate pro Sitzung, vor dem ersten Schreiben, und die Hauptsession bittet nie um den
Rückwechsel — der reviewer liest nur, und ein Mangel muss sofort neu delegierbar sein. Weil eine
Sitzung nach dem Gate im Auto Mode bleibt, empfiehlt die Hauptsession eine neue Sitzung pro Feature:
Der Plan Mode schützt dann wieder die nächste Klärung, und der Kontext beginnt leer.

## Persistenz und Wiederaufnahme

| Artefakt | Ort | Lebensdauer |
| --- | --- | --- |
| Projekt-Profil | `docs/PROJECT-PROFILE.md` | committet, dauerhaft |
| Projekt-Status | `CONTEXT_SUMMARY.md` | committet, nach jedem Feature aktualisiert |
| Feature-Plan | `.agents/plans/<feature>.md` | nicht committet, bei Abschluss gelöscht |
| PR-Text | `.agents/pull-request.md` | nicht committet |

Persistiert wird, was nicht billig reproduzierbar ist: Das Profil trägt Opus-Urteile und die
Korrekturen des Users, der Plan trägt die Klärung. Review-Berichte sind aus dem Code reproduzierbar
und bleiben Antworten, keine Dateien. Die Fortschritts-Checkliste des Plans ist der Resume-Punkt.

## Git

Die Hauptsession legt den Branch an, pusht und öffnet die Pull Request — über das Installation
Access Token der GitHub App, damit die PR vom Bot stammt und der User sie regulär freigeben kann
(Details in `CLAUDE.md`). Der implementer committet seine Phasen — er ist die einzige Rolle, die
Projektdateien schreibt, also enthält kein Commit die halbe Arbeit eines anderen. Nur das *Format* —
Branch-Namen, Commit-Stil — kommt aus dem Profil.

## Bewusst nicht enthalten

- **`plan-writer`** (aufgelöst, 2026-11). Erfüllte kein Kriterium: Die Hauptsession verfasste jeden
  Plan ohnehin und zahlte dann einen Kaltstart, damit `haiku` ihn auf die Platte kopierte. Die
  Hauptsession schreibt Plan, PR-Text und `CONTEXT_SUMMARY.md` jetzt selbst; das Schreibverbot gilt
  nur für Quellcode, Tests und Projektdokumentation.
- **Getrennte `test-agent`, `dev-agent` und `doc-agent`** (in `implementer` zusammengelegt,
  2026-11). Nach Berufsbezeichnung auf denselben Dateien geschnitten — drei Kaltstarts, die
  denselben Kontext neu lasen. Die Unabhängigkeit der Tests bleibt über den roten Commit und die
  Diff-Prüfung des reviewers erhalten.
- **`haiku` für Code und Tests** (2026-11). Zu viele Korrekturrunden, jede mit einem Opus-Review
  bezahlt.
- **Drei Review-Gates** (2026-11). Ein Review über den ganzen Diff prüft Tests, Code, Doku und ihre
  Reihenfolge zusammen. Das akzeptierte Risiko — ein schwacher Test fällt erst am Ende auf — ist
  klein, wenn die Akzeptanzkriterien konkret sind.
- **Ein `sonnet`-Reviewer.** Günstiger, aber dasselbe Modell wie der Autor teilt dessen blinde
  Flecken; der frische Blick ist der Grund, weshalb es die Rolle gibt.
- **Eine Profiler-Rolle.** Profilieren und Planen sind derselbe Denkakt; die Hauptsession kann nicht
  planen, ohne den Test-Runner zu kennen.
- **Review-Berichte als Dateien.** Aus dem Code reproduzierbar. Die Berichte unter `reviews/`
  stammen aus der Zeit des alten Orchesters und bleiben als Historie liegen.
