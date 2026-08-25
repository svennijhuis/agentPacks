# Review contract

This file defines the shared wire format between Loop agents.

## Severity

| Severity | Meaning | Effect |
|---|---|---|
| `high` | Wrong, exploitable, loses data, or leaves an acceptance criterion unmet on a path that runs | Forces a fix round |
| `medium` | Fails under realistic pressure, leaves an error path unhandled, changes behavior without a test, or duplicates existing behavior materially | Forces a fix round |
| `low` | Real but safe to defer, such as misleading naming or avoidable nesting | Follow-up; fix only when the round already touches that code |
| `tiny` | True but not worth a round, such as wording, ordering, or a stray import | Note only |

In a planned loop, any `high` or `medium` means `fix`; only `low` and `tiny` can pass with notes. A
problem no change to this diff can resolve means `replan`. `pass` additionally requires a `pass` row
with adequate evidence for every acceptance criterion. Only `loop-orchestrator` assigns the verdict.
Standalone diff review has no verdict or fix-round effect; severity ranks its findings only.

## Finding identity

Two reports describe the same finding only when both the location and underlying defect/cause match.
Keep distinct defects on one line separate. Merge the same defect found twice at the highest severity
and name every source.

## Field rules

| Field | Rule |
|---|---|
| Location | `path:line`, or `path` for a whole-file finding |
| Severity | Exactly `high`, `medium`, `low`, or `tiny` |
| Problem | One sentence stating the defect and its cause when needed to identify it |
| Fix | Imperative and specific enough to execute without another question |
| Empty | Write `No findings.` and state what was examined; never add filler findings |

## Input normalization

A completed report is noncanonical but usable when every required semantic field is present and
unambiguous, but its heading, table layout, column order, labels, or equivalent prose differs from
the shapes below. The orchestrator normalizes that presentation in memory and merges it in the same
invocation. It does not ask the producing agent to rewrite the report and does not emit or persist an
intermediate repaired report.

A report is malformed only when merge would require invention: the report is absent; a required
semantic field is missing; severity or verifier result is outside the contract or ambiguous; an
empty result does not state what was examined; or a finding's identity, location, problem, or fix
cannot be recovered. Presentation differences alone are not malformed. The orchestrator may
normalize formatting, never meaning.

## Reviewer report

```markdown
## <agent-name> — round <n>

**Examined:** <scope>
**Not examined:** <omissions and reason; omit when empty>

| # | Severity | Location | Problem | Fix |
|---|---|---|---|---|

**Replan:** <only when no fix to this diff can resolve the problem>
```

Order most severe first. Report numbers are local; the orchestrator renumbers after merging.
When there are no findings, omit the table and write `No findings.` after the scope fields.

## Verifier report

```markdown
## loop-verifier — round <n>

| Criterion | Result | Command | Evidence |
|---|---|---|---|
| 1 | pass | `dotnet test App.slnx --no-restore` | `10 passed, 0 failed` |
| 2 | fail | `dotnet test App.slnx --no-restore` | `expected 401, got 200` |
| 3 | not verified | — | No automated or safe manual check covers this criterion. |

**Suite:** <wider run and unrelated failures>
```

No evidence means `not verified`, never `pass`.

A `fail` or `not verified` row blocks `pass`. During merge, the orchestrator turns any such row that
is not already represented by a reviewer finding with the same cause into a finding attributed to
`loop-verifier`. Use the supplied plan path as `Location`; name the criterion number and command
evidence in `Problem` and `Fix`. This preserves the verifier report fields and requires no new search.
This is the one exception to normal finding identity: a reviewer finding with the same cause covers
the verifier row even though its source location differs from the synthesized plan-path location.

## Implementer report

```markdown
## loop-implementer — round <n>

**Criteria claimed:** <numbers>
**Fix list entries resolved:** <numbers and deferred low/tiny entries>
**Standards followed:** <standard and source, or `None recorded`>
**Files touched:** <paths>
**Follow-ups noticed, not done:** <items or `None`>
```

The implementer does not claim verification.

## Orchestrator report

The main agent supplies completed reviewer reports, verifier evidence, round number, plan path, and
the recorded security-gate decision. The orchestrator normalizes usable inputs, validates their
semantics, and merges them; it does not launch reviewers, retry malformed reports, or route the verdict.

Round 1 is the initial implementation review. Round 2 is the first fix review and round 3 the second.
When the merged list is empty, replace its table with `No findings.`.

Malformed or missing input returns this shape and does not write the plan or assign a verdict:

```markdown
## Orchestrator input error — round <n>

**Missing or malformed:** <report and violated requirement>
**Action:** Surface this error unchanged and end the current loop. No plan write, verdict, retry, fix round, or agent handoff is allowed.
```

```markdown
## Fix list — round <n>

**Verdict:** fix | pass | replan
**Security gate:** ran — <reason> | skipped — no trust boundary changed

| # | Severity | Location | Problem | Fix | Found by |
|---|---|---|---|---|---|

**Lowered:** <finding and reason; omit when empty>
**Notes carried forward:** <unresolved low/tiny entries>
```

## Standalone merge report

For standalone diff review, use round 1 and return this shape instead of the orchestrator report above:

```markdown
## Merged review — round 1

**Security gate:** ran — <reason> | skipped — no trust boundary changed

| # | Severity | Location | Problem | Fix | Found by |
|---|---|---|---|---|---|

**Lowered:** <finding and reason; omit when empty>
```

There is no `Verdict`, plan append, verifier evidence, or fix round in this shape.
