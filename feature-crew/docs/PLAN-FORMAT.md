# Plan Format

A feature plan lives at `.agents/plans/<feature>.md` in the target project. The main session
writes it after the mode gate, the `implementer` ticks its lines, and a new session resumes from
it. It is not committed and is deleted once the pull request is open.

## Structure

```markdown
# <Feature>

## Goal
<Observable behaviour, two or three sentences.>

## Acceptance criteria
1. <Concrete enough to write a test against>

## Out of scope
- <What this feature explicitly does not do>

## Files to touch
- <path> — <what changes>

## Test cases
- Criterion 1: <happy path, error paths, boundaries>

## Order of work
<Anything the implementer must do in a particular order.>

## Open questions
<Only if any remain — otherwise omit the section.>

## Progress
- [ ] Tests written and red for the right reason (implementer) — red commit: <sha>
- [ ] Implementation complete, whole suite green (implementer)
- [ ] Documentation updated (implementer)
- [ ] Review passed (reviewer) — attempt <n> of 3
- [ ] Pull request opened (or message written, if gh is unavailable)
```

## Who ticks what

| Line | Ticked by | When |
| --- | --- | --- |
| Tests written and red | `implementer` | after the red commit, with its sha filled in |
| Implementation complete | `implementer` | after the green commit |
| Documentation updated | `implementer` | after the documentation commit |
| Review passed | main session | on `VERDICT: PASS`, with the attempt number |
| Pull request opened | main session | once the PR is open or the message is on disk |

A ticked line is never unticked. The first unticked line is the next step.

## Resuming a plan in the old format

Plans written before the crew had one `implementer` carry the old seven-line checklist. Map it,
rewrite the `Progress` section in the new format, then continue at the first unticked line:

| Old line(s) | New line |
| --- | --- |
| Tests written (test-agent) · Tests approved (review-agent) | Tests written and red — sha from `git log` |
| Implementation complete, tests green (dev-agent) · Implementation approved | Implementation complete |
| Documentation updated (doc-agent) | Documentation updated |
| Documentation approved (review-agent) | Review passed — only if every old review line was ticked |
| Pull request opened | Pull request opened |

Partial approvals from the old gates do not replace the final review: unless all three old review
lines were ticked, the one review over the whole diff still runs.
