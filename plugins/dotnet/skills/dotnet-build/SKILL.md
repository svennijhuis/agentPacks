---
name: dotnet-build
description: When building, restoring, or formatting a .NET repository.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# .NET build

Loop-only; not as a user entrypoint. The facts an agent needs before it touches a `.csproj`. Read the repository's own files first — the common layout is not a guarantee.
When opening a solution or package graph, load `dotnet-solution` by exact Skill tool name. Use local `dotnet sln list` / `dotnet list package`. Never a remote service.

When loaded by exact Skill tool name `dotnet-build` during implement or review:
1. Read every file in `references/standards/`.
2. Standards in force: `csharp.md`, `async-errors.md`, `layers.md`.
3. Cite the document filename on each edit (`csharp.md`, not "the C# standard").

Read [commands, packages, and failures](references/commands.md).

Good: version in `Directory.Packages.props`, Version-less `PackageReference`.
Bad: put `Version` on the csproj under CPM.
