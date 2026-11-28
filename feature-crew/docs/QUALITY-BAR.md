# Quality Bar

The one standard for feature work. The `implementer` works to it, the `reviewer` judges against
it, the main session signs off against it. The project profile (`docs/PROJECT-PROFILE.md` in the
target project) supplies the concrete conventions; this file supplies the method. On conventions
the profile wins, on method this file wins.

## 1. Test-driven

- **Tests first.** They are committed before any production code — the *red commit*.
- **Red for the right reason.** The tests fail because the implementation is missing — not because
  of a typo, an import error or a compile error. A red test that does not compile is a broken file.
- **Coverage.** Every acceptance criterion in the plan has at least one test. Happy path, error
  paths and boundaries are covered — a suite that only proves the good case is theatre.
- **Construction.** One test asserts one thing, and its name says which. Arrange, act, assert —
  visibly. No test depends on another test's leftovers or on run order. No assertion on incidental
  detail that makes a test brittle.
- **Tests are the specification.** After the red commit no test changes — not by relaxing an
  assertion, deleting a case or marking it skipped. **Single exception:** a reviewer defect that
  names the test. That change goes into its own commit `fix tests per review`, and the no-change
  rule counts from that commit on.
- **Green means the whole suite**, not just the new tests.

## 2. Fit the project

- Conventions come from the profile and from the code next to the change. For test style, existing
  tests win where they differ from the profile. Personal preference decides nothing.
- No new dependency, framework, test runner, assertion or mocking library. If one seems necessary,
  it is reported, not added.
- Errors are handled the way the project handles them — explicitly, never silently swallowed.
- The simplest thing that passes the tests. Not the most extensible, not a framework.
- Nothing changes outside the plan. Files the profile marks as out of bounds — generated, vendored
  — are never touched.

## 3. Safe

- No secret, credential or key in code.
- Input that crosses a boundary is validated; no injection path.
- Resources are released; no obvious N+1 query, no accidental quadratic loop.

## 4. Documentation

- In the format and at the location the profile names. Never an imposed format.
- It describes what the code does — checked against the code, not against the plan.
- The feature's status agrees everywhere it is mentioned. Done in the code and "planned" in an
  overview is a defect, not a detail.
- Code comments say *why*, never *what*.
- Links and references resolve.
- `docs/architecture/` belongs to the `software-architect` and is not touched.

## 5. Git and language

- One commit per phase — tests (red), implementation (green), documentation — plus one commit per
  fix round, in the commit style the profile names.
- Only the main session creates branches, pushes and opens pull requests. Nobody rewrites history.
- Everything the crew writes is in English.
