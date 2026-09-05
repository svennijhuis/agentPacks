---
name: dotnet-review
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. Supply .NET-specific findings for a C# file, diff, or pull request from the pack's canonical design, async/error, and testing standards.
license: UNLICENSED
metadata:
  audience: loop
---

# .NET review

When loaded by exact Skill tool name `dotnet-review` during review or build:
1. Read every file in `references/standards/` (authored tree: `../../standards/` before generation).
2. Standards in force: `csharp.md`, `async-errors.md`, `testing.md`.
3. Cite the document filename on each finding (`csharp.md`, not "the C# standard").

Inspect project configuration and repeated nearby patterns for choices the standards intentionally leave to the repository. Review every changed path; do not stop after the first category.

Process findings in this order:

1. reachable correctness defects and boundary conditions;
2. async, cancellation, resource, and error behavior;
3. public contracts, nullability, equality, and dependency seams;
4. missing behavior coverage and test-boundary mistakes;
5. maintainability issues that have a concrete cost.

For each finding, supply a precise location, impact-based severity recommendation, defect and cause, actionable fix, and the canonical standard or repository evidence that supports it. Syntax alone does not determine severity.

Return findings to the caller. Do not require or invent the Squad table: a Loop caller maps findings into its shared review contract, while a standalone review may use its own format. Do not restate unchanged code, report formatter preferences without a standard, edit the code, or commit, merge, or push.
