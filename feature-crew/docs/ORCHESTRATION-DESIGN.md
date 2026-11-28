# Orchestration Design: feature-crew

## Problem

A test-driven orchestra whose **method is fixed** and whose **expertise is acquired**: dropped into
any project, it learns that project's language, frameworks, test runner and conventions, and works
inside them. It began as five roles in the `FoodDatabase` repository, grown into that project's
stack; the first rework made it adoptable.

The second rework (2026-11) targeted cost. A feature needed **eight cold starts** — `plan-writer`,
`test-agent`, review, `dev-agent`, review, `doc-agent`, review, `plan-writer` again — each
re-reading profile and plan. `haiku` wrote real code and tests, which produced correction rounds,
and every round cost another Opus review. The goal: fewer cold starts at the same quality.

## Role Split

| Role | Model | Splitting criterion | Rationale |
| --- | --- | --- | --- |
| main session | opus | — | Talks to the user, so it cannot be a subagent. Profiles, plans, writes the planning documents, signs off, owns branch and pull request. |
| `implementer` | sonnet | 3 · large self-contained block for a cheaper model | Tests, code and documentation touch the same files and belong in one context. `sonnet` writes real code reliably; `haiku` needed too many correction rounds. |
| `reviewer` | opus | 1 · fresh view, 4 · restricted tools | Sees the diff, not the conversation. A different model from the author, so it does not share its blind spots. Has no write tools. |

A feature now costs **two cold starts** plus one per fix round, and one Opus review instead of
three.

## Flow

**Plan Mode:** startup check → resume check → profile read, or drafted from `docs/architecture/` →
grill-me → plan presented. Plan approval and mode gate are **one** step: Shift+Tab, then "go".

**Auto mode:** branch → profile (if new) and plan written by the main session → `implementer`
(red commit → green commit → documentation commit) → `reviewer` over the whole diff → pull request
→ plan deleted → a new session recommended for the next feature.

**Escalation:** three attempts. Two quiet, the third announced, then the user decides. A defect
that survives two attempts means the assignment is rewritten. A blocked report from the
implementer is not an attempt.

## TDD without a separate test role

The old split guaranteed that whoever wrote the tests did not make them pass. The guarantee now
comes from history and review: the implementer commits the red tests before any code, and the
reviewer checks that `git diff <red-sha>..HEAD` over the test files is empty. The single exception
is a test the reviewer itself names as defective — it is fixed in its own commit
`fix tests per review`, and the check counts from there.

## Shared files instead of repeated rules

The standard every role applies lives once, in `docs/QUALITY-BAR.md`; the plan layout and resume
mapping in `docs/PLAN-FORMAT.md`. Agent files hold only their role-specific way of working. The
files are read on demand and deliberately not `@import`ed into `CLAUDE.md`, which would put them in
the main context on every turn. Personas are one sentence of stance: generic seniority claims did
not change behaviour; concrete rules and examples do.

## Permission modes

The mode belongs to the session and is inherited by every subagent; it is not a role property.
There is one gate per session, before the first write, and the main session never asks to switch
back — the reviewer only reads, and a defect must be re-delegatable immediately. Because a session
stays in auto mode after the gate, the main session recommends a new session per feature: Plan Mode
protects the next clarification again, and the context starts empty.

## Persistence and resumability

| Artefact | Location | Lifetime |
| --- | --- | --- |
| Project profile | `docs/PROJECT-PROFILE.md` | committed, permanent |
| Feature plan | `.agents/plans/<feature>.md` | not committed, deleted on completion |
| Pull request message | `.agents/pull-request.md` | not committed |

Persist what is not cheaply reproducible: the profile carries Opus reasoning and the user's
corrections, the plan carries the clarification. Review reports are reproducible from the code and
stay reports. The plan's progress checklist is the resume point.

## Git

The main session creates the branch, commits the profile, pushes and opens the pull request. The
implementer commits its phases — it is the only role that writes project files, so no commit holds
half of somebody else's work. `gh` authenticated and a GitHub remote means push and `gh pr create`;
anything else keeps branch and commits local with the message at `.agents/pull-request.md`. Only
the *format* — branch naming, commit style — comes from the profile.

## Deliberately Not Included

- **`plan-writer`** (folded, 2026-11). It met no criterion: the main session composed every plan
  anyway, then paid a cold start for `haiku` to copy it to disk. The main session now writes the
  profile, plans and pull request message itself; the rule against writing applies to source,
  tests and project documentation only.
- **Separate `test-agent`, `dev-agent` and `doc-agent`** (folded into `implementer`, 2026-11). Split
  by job title on the same files — three cold starts re-reading the same context. Test independence
  is kept through the red commit and the reviewer's diff check.
- **`haiku` for code and tests** (2026-11). Too many correction rounds, each paid for with an Opus
  review.
- **Three review gates** (2026-11). One review over the whole diff checks tests, code, documentation
  and their sequence together. The accepted risk — a weak test surfacing only at the end — is small
  when acceptance criteria are concrete.
- **A `sonnet` reviewer.** Cheaper, but the same model as the author shares its blind spots; the
  fresh view is the reason the role exists.
- **A profiler role.** Profiling and planning are the same act of thought; the main session cannot
  plan without knowing the test runner.
- **Architecture documentation.** `docs/architecture/` belongs to the `software-architect`.
- **A fixed documentation format, a fixed branch convention.** Both come from the profile.
- **Review reports as files.** Reproducible from the code.
