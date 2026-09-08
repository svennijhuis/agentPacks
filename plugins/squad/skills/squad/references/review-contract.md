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
with adequate evidence for every acceptance criterion. Only `squad-orchestrator` assigns the verdict.
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
| Confidence | Score 0–100. Only report findings with confidence ≥ 80. Drop low-confidence noise. Do not emit a confidence column |

## Confidence

Only report findings with confidence ≥ 80. Drop low-confidence noise. A guess, style nit without
a standard, or an unevidenced "looks wrong" is below the bar and is omitted, not listed as `tiny`.

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
## squad-verifier — round <n>

| Criterion | Result | Command | Evidence |
|---|---|---|---|
| 1 | pass | `dotnet test App.slnx --no-restore` | `10 passed, 0 failed` |
| 2 | fail | `dotnet test App.slnx --no-restore` | `expected 401, got 200` |
| 3 | not verified | — | No automated or safe manual check covers this criterion. |

**Suite:** <wider run; `pass` or `fail` plus `this-change` or `pre-existing` when it failed>
**Stacks:** <dotnet | rust | both>
**Boundary:** <covered | not covered; required when both stacks apply>
**Coverage:** <behavioral and edge cases named, or `happy-path only`>
**Evidence gaps:** <criteria or paths lacking a safe check, or `None`>
**Assumptions challenged:** <plan assumptions that evidence undermines, or `None`>
```

No evidence means `not verified`, never `pass`. The suite, stacks, boundary, coverage, evidence-gaps,
and assumptions-challenged lines after the table are part of the pass gate, not narration.
Evidence gaps and challenged assumptions are required fields (`None` when empty).

A verified `pass` is the outcome of the evaluator, not a hopeful reading of the table:

| Gate | Outcome |
|---|---|
| No confirmed plan | Stop. Do not verify. Not a pass. |
| A criterion command is `—`, empty, or never covers the criterion | That row is `not verified`. Not a pass, even if the row says `pass`. |
| Plan command passes and the wider suite fails | Not a verified pass. |
| A failure is not classified `this-change` or `pre-existing` | Not a pass. Classification is required whenever a command fails. |
| Mixed .NET and Rust, but only one suite ran, or the boundary was not checked | Not a pass. |
| Agent-written tests are happy-path only | Not a pass. Name behavioral and edge coverage. |
| `Evidence gaps` is not `None` and the gap is not already a merged finding with the same cause | Not a pass. Orchestrator synthesizes a finding attributed to `squad-verifier`. |
| `Assumptions challenged` is not `None` and the challenge is not already a merged finding with the same cause | Not a pass. Orchestrator synthesizes a finding attributed to `squad-verifier`. |

A `fail` or `not verified` row blocks `pass`. During merge, the orchestrator turns any such row that
is not already represented by a reviewer finding with the same cause into a finding attributed to
`squad-verifier`. Use the supplied plan path as `Location`; name the criterion number and command
evidence in `Problem` and `Fix`. This preserves the verifier report fields and requires no new search.
This is the one exception to normal finding identity: a reviewer finding with the same cause covers
the verifier row even though its source location differs from the synthesized plan-path location.
The same synthesis applies to non-`None` **Evidence gaps** and **Assumptions challenged**.

## Implementer report

```markdown
## squad-implementer — round <n>

**Criteria claimed:** <numbers>
**Fix list entries resolved:** <numbers and deferred low/tiny entries>
**Standards followed:** <standard and source, or `None recorded`>
**Files touched:** <paths>
**Deviations:** <plan departures and why, or `None`>
**Concerns:** <risks or doubts for the next phase, or `None`>
**Open risks:** <unresolved hazards still in the diff, or `None`>
**Follow-ups left:** <items or `None`>
```

Every field after **Files touched** is required; write `None` when empty. The implementer does not
claim verification. These handoff fields are not merge-validated as a separate Require list item:
the main agent forwards them into the merge input as **Handoff concerns** prose. Missing required
fields on the implementer report itself make that report malformed if it is submitted to merge.

## Orchestrator report

The main agent supplies completed reviewer reports, verifier evidence, round number, plan path, and
the recorded security-gate decision. The orchestrator normalizes usable inputs, validates their
semantics, and merges them.

**Invariant — merge only.** The orchestrator never launches agents, never retries a producer, never
edits product code, never plans, and never routes the next phase. It is not a global integrator or
quality gate beyond the merge report. Retry belongs to the main agent (one re-ask), not here.

Round 1 is the initial implementation review. Round 2 is the first fix review and round 3 the second.
When the merged list is empty, replace its table with `No findings.`.

### Malformed input and main-agent re-ask

When a required report is missing or malformed **and** no replacing axis marker was supplied for that
producer, the orchestrator returns the input-error shape below and does not write the plan or assign
a verdict.

**Hard cap (anti-loop):** at most **one** re-ask **per producer per review round**. That is the entire
budget. Do not coach the producer through rewrite attempts. Do not re-ask a second time for the same
producer in the same round, even if the replacement is still wrong. Do not start a "fix the report
until it parses" loop.

**Same round number:** the re-ask and the following merge keep the **same** `round number` as the
merge that returned the input-error. Do not increment the round for parse repair. Incrementing the
round resets the re-ask budget and is forbidden for this path.

**Axis marker replaces the bad report:** when the budget is spent, the main agent passes
`<producer>: not verified — malformed after re-ask` **instead of** the malformed/missing payload —
not in addition to it. The marker is a conforming stand-in for that producer. The orchestrator must **not** emit input-error for an axis that has a marker; it synthesizes a blocking `high` finding
(plan path as `Location`) so the verdict cannot be `pass`, then completes the merge.

Sequence:

1. First malformed/missing report for producer P this round → main agent may re-ask P **once** with
   the contract shape, then invoke merge again with the **same round number** and the replacement
   (or the axis marker if P returned nothing usable).
2. If P is still missing/malformed after that single re-ask, or if the re-ask budget for P is already
   spent → main agent **must not** re-ask again. Invoke merge **once** with the same round number and
   the axis marker **replacing** P's report.
3. Orchestrator accepts the marker, synthesizes the blocking `high`, merges other usable reports.
4. If an input-error is still returned **after** the marker for P was already supplied this round →
   **stop and surface** that error to the human. No further merge attempts. No further re-ask. End
   the loop for this review phase.

A later input-error for the same producer in the same round is not a new re-ask grant. If the marker
was not yet supplied, supply the marker and merge once. If the marker was already supplied, stop —
never spawn P again this round and never merge-loop.

Malformed or missing input on a merge attempt (no replacing marker for that producer) returns:

```markdown
## Orchestrator input error — round <n>

**Missing or malformed:** <report and violated requirement>
**Re-ask budget:** one per producer per this round number — if this producer was already re-asked this round, do not re-ask; merge once with replacing axis marker `not verified — malformed after re-ask`. If the marker was already supplied, stop and surface — no further merge.
**Action:** Re-ask only if this producer has not already been re-asked this round number. Otherwise marker merge once (same round number, marker replaces payload) or stop. Do not interpret this line as a new grant. Never a second re-ask. Never "keep fixing the report". Orchestrator must not retry, launch, or hand off.
```

```markdown
## Fix list — round <n>

**Verdict:** fix | pass | replan
**Security gate:** ran — <reason> | skipped — no trust boundary changed

| # | Severity | Location | Problem | Fix | Found by |
|---|---|---|---|---|---|

**Lowered:** <finding and reason; omit when empty>
**Notes carried forward:** <unresolved low/tiny entries>
**Handoff concerns:** <implementer deviations/concerns/open risks and verifier evidence gaps / assumptions challenged that are not already rows above, or `None`>
```

When appending to the plan, rewrite `## Fix list` and `## Handoff notes` in place (run scratch).
Do not invent Fix-list severity from handoff prose. Promote a handoff concern into the Fix list
table only when it already matches a reviewer or verifier finding identity (location + cause).
Otherwise keep it under **Handoff concerns** / plan `## Handoff notes` only.

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

After this shape is returned, the main agent shows it in the IDE/CLI and asks exactly one question: Save report as markdown? Yes writes `docs/reviews/<slug>.md` (never `docs/decisions.md`) and still shows the findings in the IDE/CLI. No writes no report file. No other question, grill, verdict, or fix round.
