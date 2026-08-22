---
name: review-diff
description: Review the current uncommitted diff and report findings ranked by severity.
---

# Review the current diff

1. Get the change under review: `git diff` for unstaged work, `git diff --staged` when something is staged, and `git diff <base>...HEAD` on a branch.
2. If the diff is empty, say so and stop.
3. Apply the `code-review` skill to the change.
4. Delegate to `security-reviewer` when the diff touches authentication, user input, file paths, shell commands or credentials.
5. Report findings ranked by severity, one line each: location, problem, fix.
