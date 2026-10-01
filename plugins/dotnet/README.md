# dotnet

The C# and .NET language pack for [Squad](../squad/README.md). It supplies the
build, test, and review skills that the Loop discovers by exact name.

Slot skills are Squad internals. Do not run them directly — Squad loads each by exact Skill name.

## What is in it

| Skill | Used by | Purpose |
|---|---|---|
| `dotnet-build` | implementer, simplifier | Internal loop skill. Inspect the solution, restore, build, implement, and format once at the end. Central package versions in `Directory.Packages.props`; SDK in `global.json`; TFM and `LangVersion` in `Directory.Build.props` |
| `dotnet-test-patterns` | implementer, verifier | Internal loop skill. Choose the right test boundary, fixtures, packages, and commands |
| `dotnet-review` | correctness reviewer | Internal loop skill. Review C# correctness, API shape, async/error handling, resources, and testability |
| `dotnet-solution` | implementer | Internal loop skill. Read-only `.sln` / `.slnx` / package graph via `dotnet sln` / `dotnet list` |

Canonical standards live once under `standards/`:

- `csharp.md` — type and API design, nullability, resources, formatting, and central SDK / C# / package version pins.
- `async-errors.md` — async, cancellation, and exception boundaries.
- `testing.md` — xUnit, integration tests, time, and verification.
- `layers.md` — Web / Application / Infrastructure boundaries when the solution already uses them.
- [`http-api.md`](../../shared/standards/http-api.md) — named collection envelope for list APIs, not a root JSON array (shared across language packs).

`standards.source.json` maps each document to the skills that need it. Generation copies selected
documents into `references/standards/` — see [ADD-LANGUAGE-PACK.md](../../docs/ADD-LANGUAGE-PACK.md).

## Standards and repository conventions

The pack contains the complete standards source. The Loop records every applied skill and canonical
document under `## Standards in force`; it does not look for another standards directory.

Repositories still have a shape. The planner inspects project and formatter configuration,
directory layout, test framework, existing tests, and nearby code, then records supported patterns
under `## Repository conventions observed` with their evidence paths. Those conventions specialize
choices the pack leaves open — such as the test framework already in use — while the plugin remains
the source of standards.

## Formatting

There is no global after-edit hook. Formatting every partial edit is noisy, expensive, and can
rewrite files outside the intended change. `dotnet-build` formats the selected solution once after
implementation with `dotnet format <solution> --no-restore --include <changed .cs files>`; verification
checks the whole solution with `--verify-no-changes`. See the
[`dotnet format` command reference](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-format).

## Editing this pack

Authored: `plugin.json`, `mcp.json` (empty scaffold), `standards.source.json`, `standards/`, and `skills/`.

v1 ships no MCP server. Agents use `dotnet sln list` and `dotnet list <csproj> package` on the machine. See [ADD-MCP.md](../../docs/ADD-MCP.md) to add a server later.

Generated on `marketplace` / `marketplace-beta` or in temporary validation output, never on `main`:
`.cursor-plugin/`, `.codex-plugin/`, `com.anthropic.claude-code/`, `com.openai.codex/`,
`com.github.copilot/`, and each skill's `references/standards/` directory.

See [ADD-LANGUAGE-PACK.md](../../docs/ADD-LANGUAGE-PACK.md) and
[ADD-SKILL.md](../../docs/ADD-SKILL.md).
