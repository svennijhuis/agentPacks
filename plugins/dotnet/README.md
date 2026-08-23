# dotnet

The C# and .NET language pack for the [delivery loop](../delivery-loop/README.md). It supplies the
build, test, and review skills that the Loop discovers by exact name.

## What is in it

| Skill | Used by | Purpose |
|---|---|---|
| `dotnet-build` | implementer, simplifier | Inspect the solution, restore, build, implement, and format once at the end |
| `dotnet-test-patterns` | implementer, verifier | Choose the right test boundary, fixtures, packages, and commands |
| `dotnet-review` | correctness reviewer | Review C# correctness, API shape, async/error handling, resources, and testability |

Canonical standards live once under `standards/`:

- `csharp.md` — type and API design, nullability, resources, and formatting.
- `async-errors.md` — async, cancellation, and exception boundaries.
- `testing.md` — xUnit, integration tests, time, and verification.

`standards.source.json` maps each document to the skills that need it. Marketplace generation copies
only those documents into each consumer's `references/standards/` directory. Generated copies carry
a do-not-edit header; edit the canonical source here instead.

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

Authored: `plugin.json`, `standards.source.json`, `standards/`, and `skills/`.

Generated on the `marketplace` branch or in temporary validation output, never on `main`:
`.cursor-plugin/`, `.codex-plugin/`, `com.anthropic.claude-code/`, `com.openai.codex/`,
`com.github.copilot/`, and each skill's `references/standards/` directory.

See [ADD-LANGUAGE-PACK.md](../../docs/ADD-LANGUAGE-PACK.md) and
[ADD-SKILL.md](../../docs/ADD-SKILL.md).
