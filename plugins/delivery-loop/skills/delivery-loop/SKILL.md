---
name: delivery-loop
description: Run a change through plan, implement, verify and review as separate phases, with a fixed handoff between them and a bounded fix round when review finds something. Use when a change is large enough that one pass will miss something, when work must be split across roles, or when asked to plan and then build something.
license: UNLICENSED
---

# Delivery loop

```
plan -> implement -> verify -> review -> (fix -> verify -> review) x 2 max -> hand off

                              review = loop-reviewer        ┐
                                       loop-security-reviewer ├ in parallel -> loop-orchestrator merges
                                       loop-simplifier      ┘
```

The loop ends at a hand-off summary for the human. **No phase commits, merges or pushes.** A review verdict of `pass` is a recommendation that the work is ready to look at, never permission for it to land.

## Use the lightest process that fits

| Work | Process |
|---|---|
| Typo, rename, one-line fix with an obvious check | Implement, verify, done. No plan file. |
| Anything touching behaviour, or more than two files | Full loop with a plan file. |
| Irreversible, cross-cutting, or a public interface | Full loop, plus a named human decision point before implementing. |

Planning a typo wastes the same attention that a real plan needs. Skipping the plan on a behaviour change means review has nothing to review against.

## Planning is an interview, not a first draft

Everything downstream is measured against the plan, so a question the planner answered on the user's behalf becomes a criterion nobody agreed to — and the loop then faithfully builds the wrong thing, verifies it against the wrong criterion, and passes it. The failure is invisible precisely because every phase did its job.

So planning is an interview. `loop-planner` runs it as a subagent; anyone driving the loop by hand runs the same protocol. It is defined here once:

1. **Map the decisions** the work requires — the points where it could reasonably go more than one way. Decisions hang off each other; that dependency is a tree.
2. **Ask the frontier** — every decision whose prerequisites are settled — in one round. Numbered, each with a recommended answer, then stop and wait. A question that depends on another still open belongs to a later round.
3. **Find facts yourself.** What the repository can answer is never a question for the user. "Vitest or Jest" is a fact to look up; "existing suite or its own" is a decision to ask.
4. **Recompute and repeat** until the frontier is empty, then confirm the shared understanding before writing anything.

Every question carries a recommendation. An interview that only asks makes the user do the planner's thinking; the recommendation is what makes a round cheap — confirm, correct, or pick differently.

**An open question is never a criterion.** Not as a guess written down as a decision, and not as a criterion vague enough to be true either way — that is worse, because it passes review while the change does the wrong thing. An assumption that genuinely cannot be resolved is written into the plan under its own heading, so the reviewer can challenge it, never laundered into a criterion.

The interview scales with the work. Grilling a typo is its own failure; a change to a public interface earns as many rounds as it takes.

## The plan artifact

Lives at `docs/plans/<slug>.md` in the repository being worked on, and changes with the work. Chat is not storage: a plan that exists only in a session is gone the next morning, and the reviewer cannot check a change against something it cannot read.

```markdown
# <outcome, as a sentence>

## Problem
What is wrong or missing now.

## Decisions
What was settled in the interview, and why — so a reviewer can tell a deliberate
choice from a default.

## Acceptance criteria
1. <observable, testable statement>
2. ...

## In scope
Files and behaviour this change touches.

## Out of scope
What is deliberately left alone, so a reviewer does not read it as an omission.

## Verification
The exact command that proves the criteria hold.
```

Each review round appends its merged list to the same file:

```markdown
## Fix list — round <n>

| # | Severity | Location | Problem | Fix | Found by |
|---|---|---|---|---|---|
```

A criterion is testable or it is not a criterion. "Handles errors well" cannot fail; "returns 400 with the field name when the payload is missing `id`" can.

## The handoff contract

Each phase ends in the shape the next phase consumes. Anything else is conversation.

| Phase | Ends with |
|---|---|
| Plan | The plan file, with the decisions that were settled, numbered acceptance criteria, and a verification command. No open question left inside it. |
| Implement | The diff, and which criteria it claims by number. Nothing about whether they pass. |
| Verify | Per criterion: the command run, its exact output, and pass or fail. |
| Review | One merged, deduplicated fix list ranked by severity, plus a verdict — `pass`, `fix` or `replan`. |

## The review phase runs wide, not deep

Three reviewers read the same diff and answer different questions:

| Agent | Question |
|---|---|
| `loop-reviewer` | Does it do what the plan said, and is it correct? |
| `loop-security-reviewer` | Can it be abused? |
| `loop-simplifier` | Did it have to be this much code? |

**They run in parallel.** Nothing one produces is an input to another, so running them in sequence spends three times the wall clock for the same answer. `loop-orchestrator` fans them out in one batch and merges what comes back.

Merging is not concatenating. The same line found by two reviewers is one entry at the highest severity reported, naming both sources — a reader who sees it twice ranks it twice, and an implementer fixes it twice.

## Severity

One scale, used by every reviewer, so a merged list can be ranked at all.

| Severity | What it means | Effect |
|---|---|---|
| `high` | Wrong, exploitable, or loses data. An unmet acceptance criterion. A correctness defect on a path that runs. | Forces a fix round |
| `medium` | Right today, wrong under pressure. A behaviour change with no test. An unhandled error path. Real duplication of code that already exists. | Forces a fix round |
| `low` | Worth doing, costs nothing to defer. Naming that has drifted from what the code does, a nested block an early return would flatten, a comment that explains the wrong thing. | Follow-up note. Fixed only if the round already touches that code |
| `tiny` | True, and not worth a round on its own. Wording, ordering, a stray import. | Batched as notes, never the reason for a round |

A correctness bug outranks a naming nit, and reporting both at the same weight buries the bug. That is the whole reason the scale exists — it stops the two failures that kill review: a naming nit ranked beside an injection, and a real defect lost under twenty preferences. If everything is `high`, nothing is.

A verdict falls out of the merged list: any `high` or `medium` is `fix`; only `low` and `tiny` is `pass` with notes; a finding no fix to this diff can resolve is `replan`.

## Report formats

Every phase returns a fixed shape. This is not ceremony: `loop-orchestrator` merges three reviewer reports into one list, and it can only deduplicate by location and rank by severity if all three report location and severity the same way. Prose that has to be interpreted is prose that gets interpreted differently each round.

Rules that hold for every report below.

| Field | Rule |
|---|---|
| Location | `path:line`, or `path` when the finding is the whole file. Never "in the auth code". |
| Severity | Exactly one of `high`, `medium`, `low`, `tiny`. Lowercase. |
| Problem | One sentence, stating the defect. No rationale, no consequence clause. |
| Fix | Imperative and specific enough to act on without asking a question. |
| Empty | An empty table is a valid result. Write `No findings.` and say what you examined — never pad a table to look thorough. |

### Reviewer report

All three reviewers return this, and only this:

```markdown
## <agent-name> — round <n>

**Examined:** <what was in scope>
**Not examined:** <what was skipped, and why — omit the line if nothing was>

| # | Severity | Location | Problem | Fix |
|---|---|---|---|---|
| 1 | high | src/auth/session.ts:88 | Session token has no expiry, so a leaked token is valid forever. | Set `expiresAt` on issue and reject expired tokens in `verify`. |
| 2 | low | src/auth/session.ts:12 | `doCheck` no longer describes what the function does after the change. | Rename to `assertSessionActive`. |

**Replan:** <one line, only when no fix to this diff can resolve something — otherwise omit>
```

Rows are ordered most severe first. The `#` column is local to this report; the orchestrator renumbers on merge.

### Verifier report

```markdown
## loop-verifier — round <n>

| Criterion | Result | Command | Evidence |
|---|---|---|---|
| 1 | pass | `npm test -- session` | `12 passing` |
| 2 | fail | `npm test -- expiry` | `expected 401, got 200` |
| 3 | not verified | — | No command covers this; needs a manual probe. |

**Suite:** <the wider run, and whether anything failed that this change did not touch>
```

`not verified` is a first-class result. A criterion with no evidence is never reported as `pass`.

### Implementer report

```markdown
## loop-implementer — round <n>

**Criteria claimed:** 1, 2, 4
**Fix list entries resolved:** 1, 2, 5 — <and which `low`/`tiny` entries were left, if any>
**Files touched:** <paths>
**Follow-ups noticed, not done:** <one line each, or `None`>
```

No statement about whether anything passes. That is the verifier's report, and duplicating it here is how an unverified claim enters the record.

### Orchestrator report

The merged list, written to the plan and repeated in the hand-off:

```markdown
## Fix list — round <n>

**Verdict:** fix | pass | replan

| # | Severity | Location | Problem | Fix | Found by |
|---|---|---|---|---|---|
| 1 | high | src/auth/session.ts:88 | ... | ... | loop-reviewer, loop-security-reviewer |

**Lowered:** <finding, and why — omit if none>
**Notes carried forward:** <`low` and `tiny` entries nobody acted on>
```

## Evidence, not claims

"Tests pass" is a claim. The command and its output are evidence. A phase that did not run something reports it as not run — an unverified criterion is a known unknown, and a criterion reported as passing without a command is a defect that has been laundered into a status update.

The implementer does not grade its own work. Verification is a separate phase because the person who wrote the code already believes it works.

## Looping back

`loop-orchestrator` picks one:

- **`pass`** — every criterion is met and evidenced. Hand off to the human.
- **`fix`** — findings that must be resolved. Back to implement, with the finding list as the new scope. Nothing else may be touched.
- **`replan`** — the plan was wrong: criteria that cannot be met, or that would not solve the problem. Back to plan, not to implement.

The severity scale decides this, not an argument each round. `high` and `medium` force the round; `low` and `tiny` ride along as notes and get fixed only where the round is already editing that code. Adjacent code the change did not touch, and improvements that need their own plan, are not findings at all — they are the next plan.

The fix list is the entire scope of the round. An implementer that fixes something not on the list has made the next review harder.

**Two fix rounds, maximum.** A third means the plan, the criteria or the diagnosis is wrong — not that a third attempt will land it. Escalate to the human with what was tried, what still fails, and what you now think the real problem is.

## The security gate

Acceptance review asks whether the change does what the plan said. Security review asks whether it can be abused. The second is not implied by the first, and no amount of passing criteria makes an injection safe.

Run the security gate whenever the change touches authentication, authorisation, untrusted input, file paths, shell commands, cryptography, dependencies, deserialisation, outbound requests, or anything handling credentials. When in doubt, run it: the cost is one review, and the cost of skipping it is discovered by someone else.

It walks the OWASP Top 10 and returns its own `pass` / `fix` / `replan`. A security `fix` outranks an acceptance `pass` — the fix round happens.

Findings are stated as attacks, not as category labels. A category name tells nobody what to change.

## Depth of review

Correctness, security and maintainability are not three passes over one checklist here — they are three agents, each with its own order of work, run at once. The `review-diff` command is the same three without a plan behind them.

This loop reviews one change against one plan. It is not threat modelling: a design nobody has threat-modelled is a `replan`, not a finding.

## Anti-patterns

- Planning work smaller than the plan.
- Implementing before criteria exist, then writing criteria that describe what was built.
- Filling in an open question to keep the plan moving. The loop's speed is not worth a criterion nobody agreed to.
- Asking the user something the repository already answers.
- Asking a whole tree of questions at once, including the ones whose answers depend on the answers you are still waiting for.
- The implementer reporting its own work as verified.
- A reviewer that edits the code it is reviewing — the finding disappears and nobody learns of it.
- Running the reviewers one after another. They do not read each other's output; sequencing them buys nothing.
- Three separate finding lists handed to an implementer. Merging is the orchestrator's job precisely because nobody else can see the overlap.
- Grading everything `high` to make sure it gets fixed. It makes the list unrankable, which means nothing gets prioritised.
- Plan state that lives only in chat.
- Looping until it passes. The cap exists because the fourth attempt is rarely better than the second.
- An agent committing "so the work is not lost". The working tree is the hand-off.
