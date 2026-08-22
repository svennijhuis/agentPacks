---
name: code-review
description: Review a change for correctness, security and maintainability, and report findings ranked by severity. Use when reviewing a diff, a pull request or a file, or when asked what is wrong with a change.
license: UNLICENSED
---

# Code review

Review in this order and stop at the first category with findings. A correctness bug outranks a naming nit, and reporting both at once buries the bug.

## 1. Correctness

- Boundary conditions: off-by-one, empty collections, the first and last iteration.
- Error paths: what happens when the call fails, not only when it succeeds.
- Concurrency: shared state written from more than one place.
- Behaviour the change did not intend to alter. A refactor that changes output is not a refactor.

## 2. Security

- Untrusted input reaching a shell, a query, a path or rendered output.
- Secrets in source, in logs, or in error messages.
- Authorisation checked at the boundary the caller actually crosses.

## 3. Maintainability

- Names that describe what the code does rather than how.
- Duplication that will drift apart.
- Comments that explain why, where the reason is not visible in the code.

## Reporting

One line per finding: where it is, what is wrong, and the fix. Rank by severity, most severe first. Say plainly when a category has nothing worth reporting rather than inventing filler.
