# .NET review checklist

Inspect project configuration, `.editorconfig`, analyzer / `Directory.Build.props` settings,
repeated nearby patterns, and the repo's own check: `dotnet format`, build analyzers, test command,
pre-commit hook, CI job. Review every changed path; do not stop after the first category.

Classify each defect before writing Fix (squad review contract: Mechanical vs judgment):

- **Mechanical** — banned API, nullability suppression, missing `ConfigureAwait` where the repo
  analyzer already flags it, import shape, file location. If an existing analyzer, `dotnet format`,
  or CI job would have caught it, Fix names that check. Do not invent a new prose rule.
- **Judgment call** — layering, public-contract intent, surrounding style, test-boundary choice.
  Cite `csharp.md` / `async-errors.md` / `testing.md` / `layers.md` / `http-api.md`.

Process findings in this order:

1. reachable correctness defects and boundary conditions;
2. async, cancellation, resource, and error behavior;
3. public contracts, nullability, equality, and dependency seams;
4. missing behavior coverage and test-boundary mistakes;
5. maintainability issues that have a concrete cost.

For each finding, supply a precise location, impact-based severity recommendation, defect and cause, actionable fix, and the canonical standard or repository evidence that supports it. Syntax alone does not determine severity.

Return findings to the caller. Do not require or invent the Squad table: a Loop caller maps findings into its shared review contract, while a standalone review may use its own format. Do not restate unchanged code, report formatter preferences without a standard, edit the code, or commit, merge, or push.
