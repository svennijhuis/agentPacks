---
name: review-diff
description: Review the current uncommitted diff and report findings ranked by severity, without going through the full loop.
---

# Review the current diff

For a change already written, when there is no plan to measure it against. The full loop is `plan -> implement -> verify -> review`; this is the review phase on its own.

1. Get the change under review: `git diff` for unstaged work, `git diff --staged` when something is staged, and `git diff <base>...HEAD` on a branch.
2. If the diff is empty, say so and stop.
3. Apply the `/code-review` skill to the change.
4. Delegate to `loop-security-reviewer` when the diff touches authentication, authorisation, untrusted input, file paths, shell commands, cryptography, dependencies or credentials.
5. Report findings ranked by severity, one line each: location, problem, fix.

There is no verdict here and no fix round — those need a plan. A finding that needs one is a signal the change should have had a plan.
