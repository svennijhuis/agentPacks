---
name: delivery-loop
description: Run a change through plan, implement, verify and review as separate phases, with a fixed handoff between them and a bounded fix round when review finds something. Use when a change is large enough that one pass will miss something, when work must be split across roles, or when asked to plan and then build something.
license: UNLICENSED
---

# Delivery loop

```
plan -> implement -> verify -> review (+ security gate) -> (fix -> verify -> review) x 2 max -> hand off
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

A criterion is testable or it is not a criterion. "Handles errors well" cannot fail; "returns 400 with the field name when the payload is missing `id`" can.

## The handoff contract

Each phase ends in the shape the next phase consumes. Anything else is conversation.

| Phase | Ends with |
|---|---|
| Plan | The plan file, with numbered acceptance criteria and a verification command. |
| Implement | The diff, and which criteria it claims by number. Nothing about whether they pass. |
| Verify | Per criterion: the command run, its exact output, and pass or fail. |
| Review | A verdict — `pass`, `fix` or `replan` — and one line per finding: location, problem, fix. |

When the change touches a trust boundary, the review phase has two reviewers and returns the **worse** of their verdicts. A change can do exactly what the plan said and still be exploitable.

## Evidence, not claims

"Tests pass" is a claim. The command and its output are evidence. A phase that did not run something reports it as not run — an unverified criterion is a known unknown, and a criterion reported as passing without a command is a defect that has been laundered into a status update.

The implementer does not grade its own work. Verification is a separate phase because the person who wrote the code already believes it works.

## Looping back

The reviewer picks one:

- **`pass`** — every criterion is met and evidenced. Hand off to the human.
- **`fix`** — findings that must be resolved. Back to implement, with the finding list as the new scope. Nothing else may be touched.
- **`replan`** — the plan was wrong: criteria that cannot be met, or that would not solve the problem. Back to plan, not to implement.

What forces a fix round: an unmet acceptance criterion, a correctness defect, a behaviour change with no test, an un-evidenced claim, or scope creep beyond the plan. What becomes a follow-up note instead: style preferences, adjacent code the change did not touch, and improvements that need their own plan.

**Two fix rounds, maximum.** A third means the plan, the criteria or the diagnosis is wrong — not that a third attempt will land it. Escalate to the human with what was tried, what still fails, and what you now think the real problem is.

## The security gate

Acceptance review asks whether the change does what the plan said. Security review asks whether it can be abused. The second is not implied by the first, and no amount of passing criteria makes an injection safe.

Run the security gate whenever the change touches authentication, authorisation, untrusted input, file paths, shell commands, cryptography, dependencies, deserialisation, outbound requests, or anything handling credentials. When in doubt, run it: the cost is one review, and the cost of skipping it is discovered by someone else.

It walks the OWASP Top 10 and returns its own `pass` / `fix` / `replan`. A security `fix` outranks an acceptance `pass` — the fix round happens.

Findings are stated as attacks, not as category labels. A category name tells nobody what to change.

## Depth of review

This loop reviews one change against one plan. It is not a substitute for a standing review practice or for threat modelling a whole system — the `code-review` capability pack in this marketplace carries the first, and a design nobody has threat-modelled is a `replan`, not a finding.

## Anti-patterns

- Planning work smaller than the plan.
- Implementing before criteria exist, then writing criteria that describe what was built.
- The implementer reporting its own work as verified.
- A reviewer that edits the code it is reviewing — the finding disappears and nobody learns of it.
- Plan state that lives only in chat.
- Looping until it passes. The cap exists because the fourth attempt is rarely better than the second.
- An agent committing "so the work is not lost". The working tree is the hand-off.
