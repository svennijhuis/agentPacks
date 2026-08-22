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

## The plan artifact

Lives at `docs/plans/<slug>.md` in the repository being worked on, and changes with the work. Chat is not storage: a plan that exists only in a session is gone the next morning, and the reviewer cannot check a change against something it cannot read.

```markdown
# <outcome, as a sentence>

## Problem
What is wrong or missing now.

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
| Plan | The plan file, with numbered acceptance criteria and a verification command. |
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

The scale exists to stop the two failures that kill review: a naming nit ranked next to an injection, and a real defect buried under twenty preferences. If everything is `high`, nothing is.

A verdict falls out of the merged list: any `high` or `medium` is `fix`; only `low` and `tiny` is `pass` with notes; a finding no fix to this diff can resolve is `replan`.

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

The order to review in — correctness, then security, then maintainability — is the `/code-review` skill in this pack. It is also what the `review-diff` command runs when a change arrives with no plan behind it.

This loop reviews one change against one plan. It is not threat modelling: a design nobody has threat-modelled is a `replan`, not a finding.

## Anti-patterns

- Planning work smaller than the plan.
- Implementing before criteria exist, then writing criteria that describe what was built.
- The implementer reporting its own work as verified.
- A reviewer that edits the code it is reviewing — the finding disappears and nobody learns of it.
- Running the reviewers one after another. They do not read each other's output; sequencing them buys nothing.
- Three separate finding lists handed to an implementer. Merging is the orchestrator's job precisely because nobody else can see the overlap.
- Grading everything `high` to make sure it gets fixed. It makes the list unrankable, which means nothing gets prioritised.
- Plan state that lives only in chat.
- Looping until it passes. The cap exists because the fourth attempt is rarely better than the second.
- An agent committing "so the work is not lost". The working tree is the hand-off.
