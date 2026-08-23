---
name: review-diff
description: Have the main agent review an existing diff in parallel and merge findings, without requiring a plan or starting a fix round.
---

# Review the current diff

Load the `delivery-loop` skill and read `references/review-contract.md`. This command reviews an existing change with no plan.

1. Resolve the diff under review: unstaged, staged, or `<base>...HEAD`. If it is empty, say so and stop.
2. Decide whether the security gate applies and record the reason.
3. The main agent directly launches `loop-reviewer`, `loop-simplifier`, and, when applicable, `loop-security-reviewer` in parallel against the same diff.
4. After every report completes, pass the reports, security decision, `round number: 1`, `plan path: none`, and `verifier evidence: none` to `loop-orchestrator` for merge only.
5. Return the ranked merged list. Do not assign a delivery verdict, write a plan, or start a fix round.

Without a plan, `loop-reviewer` checks correctness but has no acceptance criteria or plan-specific standards. A high finding is still actionable; it does not retroactively create a delivery loop.
