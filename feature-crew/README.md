# Feature Crew

A test-driven delivery orchestra that adapts to the project it is dropped into. Its method is
fixed — tests first, implementation second, an independent review at the end. Its expertise is
acquired: language, frameworks, test runner and conventions come from the project itself.

It is the counterpart to the [`software-architect`](../software-architect/): that system designs,
this one builds what was designed.

## Agents

| Agent | Model | Task |
| --- | --- | --- |
| main session (this folder's `CLAUDE.md`) | opus | Talks to you, derives the project profile, plans, delegates, signs off, opens the pull request |
| `implementer` | sonnet | Tests → code → documentation, test-driven, one commit per phase |
| `reviewer` | opus | One read-only review of the whole change |

Why the crew is cut this way — and which roles were deliberately left out — is in
[`docs/ORCHESTRATION-DESIGN.md`](docs/ORCHESTRATION-DESIGN.md). The standard all three roles share
is [`docs/QUALITY-BAR.md`](docs/QUALITY-BAR.md); the plan layout is
[`docs/PLAN-FORMAT.md`](docs/PLAN-FORMAT.md).

## Starting it

```bash
cd agents/feature-crew
claude
```

## How a run proceeds

| Mode | What happens |
| --- | --- |
| **Plan Mode** (start) | Resume check, project profile read or drafted, feature clarified with you, plan presented |
| → | You approve the plan by pressing **Shift+Tab** and replying "go" — one step, once per session |
| **Auto mode** | Branch, profile and plan written, implementer builds, reviewer checks (up to three rounds, then you decide), pull request |
| Next feature | Start a **new session** — it begins in Plan Mode again with an empty context |

## Using it in another repository

Copy this folder into the target repository and start a session in it. The `grill-me` skill ships
with it under `.claude/skills/`.

On the first run it builds a project profile from `docs/architecture/`, so it works best on a
project the `software-architect` has documented. Check the profile and correct it — your
corrections are authoritative from then on. Then tell it what to build:

- *"Implement the following feature: …"*
- *"Continue the open plan."*

## What it writes into your project

| Artefact | Location | Lifetime |
| --- | --- | --- |
| Project profile | `docs/PROJECT-PROFILE.md` | committed, permanent |
| Feature plan | `.agents/plans/<feature>.md` | not committed, deleted when the feature is done |
| Pull request message | `.agents/pull-request.md` | not committed, for you to paste |

Add `.agents/` to the target project's `.gitignore`. Beyond that it writes exactly what you asked
for: tests, implementation and documentation.

## Git

A branch before the first change, a commit per phase so the red and green steps stay visible, and
a pull request at the end — if `gh` is installed, authenticated and the project is on GitHub.
Otherwise branch and commits stay local and the message waits at `.agents/pull-request.md`. You
review the whole change as one diff and merge it; remarks come back as an additional commit.
**It never merges, never force-pushes, and never pushes to a main branch.**

## Dependencies

| Skill | Origin | Purpose |
| --- | --- | --- |
| `grill-me` | shipped under `.claude/skills/grill-me/` | The main session's feature clarification |
