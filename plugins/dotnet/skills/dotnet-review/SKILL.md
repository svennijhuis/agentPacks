---
name: dotnet-review
description: Supply .NET-specific findings for a C# file, diff, or pull request by applying the pack's canonical design, async/error, and testing standards. The caller chooses the final report format.
license: UNLICENSED
---

# .NET review

Read every file in `references/standards/` before reviewing. In the authored source tree, before marketplace generation, the same canonical documents are under `../../standards/`.

Inspect project configuration and repeated nearby patterns for choices the standards intentionally leave to the repository. Review every changed path; do not stop after the first category.

Process findings in this order:

1. reachable correctness defects and boundary conditions;
2. async, cancellation, resource, and error behavior;
3. public contracts, nullability, equality, and dependency seams;
4. missing behavior coverage and test-boundary mistakes;
5. maintainability issues that have a concrete cost.

For each finding, supply a precise location, impact-based severity recommendation, defect and cause, actionable fix, and the canonical standard or repository evidence that supports it. Syntax alone does not determine severity.

Return findings to the caller. Do not require or invent the delivery-loop table: a Loop caller maps findings into its shared review contract, while a standalone review may use its own format. Do not restate unchanged code, report formatter preferences without a standard, edit the code, or commit, merge, or push.
