---
name: review-diff
description: Review the current uncommitted diff and report findings ranked by severity, without going through the full loop.
---

# Review the current diff

For a change already written, when there is no plan to measure it against. The full loop is `plan -> implement -> verify -> review`; this is the review phase on its own.

1. Get the change under review: `git diff` for unstaged work, `git diff --staged` when something is staged, and `git diff <base>...HEAD` on a branch.
2. If the diff is empty, say so and stop.
3. Delegate to `loop-orchestrator`, which runs `loop-reviewer`, `loop-simplifier` and — when the diff touches a trust boundary — `loop-security-reviewer` in parallel, then merges their findings into one list.
4. Report the merged list ranked by severity.

Same three reviewers as the loop, minus the plan. What changes without one: `loop-reviewer` has no acceptance criteria to check, so it reviews correctness only, and nothing is written to `docs/plans/`.

There is no verdict here and no fix round — both need a plan. A `high` finding that has nowhere to go is the signal that this change should have had one.
