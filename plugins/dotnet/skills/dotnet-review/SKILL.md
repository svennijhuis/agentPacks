---
name: dotnet-review
description: When reviewing C# files, diffs, or pull requests.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# .NET review

Loop-only; not as a user entrypoint.

When loaded by exact Skill tool name `dotnet-review` during review or build:
1. Read every file in `references/standards/`.
2. Standards in force: `csharp.md`, `async-errors.md`, `testing.md`, `layers.md`.
3. Cite the document filename on each finding (`csharp.md`, not "the C# standard").

Finding cites: [fire-and-forget](references/examples/fire-and-forget.md), [culture](references/examples/culture.md).
Process findings per [the checklist](references/checklist.md).

Good: cite `async-errors.md` on a fire-and-forget `Task`.
Bad: "looks fine" with no location or standard.
