---
name: dotnet-build
description: How a .NET repository is laid out, restored and built — solution files, Central Package Management, SDK pinning, lock files, and the exact build, run and format commands. Use before editing C# in an unfamiliar .NET repository, when adding a project or a package reference, or when a restore or build fails.
license: UNLICENSED
---

# .NET build

The facts an agent needs before it touches a `.csproj`. Read the repository's own files first — the layout below is the common one, not a guarantee.

Before writing C#, read every file in `references/standards/`. In the authored source tree, before
marketplace generation, the same canonical documents are under `../../standards/`.

## Find the shape before building

| File | What it means |
|---|---|
| `global.json` | The SDK version is pinned. A different installed SDK fails at restore, not at build |
| `Directory.Packages.props` | Central Package Management is on. **Versions live here, not in the `.csproj`** |
| `Directory.Build.props` | Settings applied to every project — nullable context, analyzers, target framework |
| `*.slnx` | The XML solution format. Newer than `.sln`, and what `dotnet` commands should target |
| `packages.lock.json` | Restore is locked. A new package needs the lock file regenerated, or CI fails |

```bash
ls global.json Directory.*.props *.slnx *.sln 2>/dev/null
```

## Commands

```bash
dotnet restore <solution>
```

```bash
dotnet build <solution> -c Release
```

```bash
dotnet run --project <project> -- <args>
```

Target the solution, not the directory. `dotnet build` with no argument searches the current folder and picks whatever it finds first, which in a repository with a `tools/` solution and a `src/` solution is a coin flip.

## Adding a package under Central Package Management

Two edits, and both are required:

1. `Directory.Packages.props` gains `<PackageVersion Include="Foo" Version="1.2.3" />`.
2. The consuming `.csproj` gains `<PackageReference Include="Foo" />` — **with no `Version` attribute.**

A `Version` on the `PackageReference` is an error under CPM, not an override. If the repository has `packages.lock.json` files, regenerate them:

```bash
dotnet restore <solution> --force-evaluate
```

A change that adds a package and does not touch the lock file passes locally and fails in CI, where restore runs locked.

## Adding a project

1. Create it under the same root as its siblings, matching their naming.
2. Add it to the solution: `dotnet sln <solution> add <project>`. In `.slnx`, this is a one-line `<Project Path="..." />` element and hand-editing is fine.
3. Inherit from `Directory.Build.props` rather than repeating nullable, analyzer or target-framework settings in the new `.csproj`. A project that sets its own is a project that drifts.

## Formatting

Format the changed C# files once after implementation, using the repository's `.editorconfig` and
the documented [`--include` option](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format):

```bash
dotnet format <solution> --no-restore --include <changed-file-1.cs> <changed-file-2.cs>
```

Pass paths relative to the workspace and include only changed C# files. Inspect the resulting diff so
formatting did not expand the requested scope. The verifier still checks the whole solution with
`dotnet format <solution> --no-restore --verify-no-changes` and does not rewrite it.

## When the build fails

Read the first error, not the last. The C# compiler cascades: one missing type produces twenty errors, and nineteen of them are noise.

| Symptom | Usually |
|---|---|
| `NU1101` / package not found | Missing feed, or a package name typo in `Directory.Packages.props` |
| `NU1004` / lock file out of date | `--force-evaluate` was not run after a package change |
| `NETSDK1045` | `global.json` pins an SDK that is not installed |
| A version attribute error on `PackageReference` | CPM is on; the version belongs in `Directory.Packages.props` |
