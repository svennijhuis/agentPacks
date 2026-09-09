---
name: dotnet-test-patterns
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. How tests are written and run in a .NET repository — unit vs integration, xUnit fixtures, WebApplicationFactory, Testcontainers, and the exact test command.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# .NET test patterns

How a test is written *in this stack*. What deserves a test at all is a separate question, and it is not a .NET one.

When loaded by exact Skill tool name `dotnet-test-patterns` during implement or verify:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `testing.md`.
4. Cite `testing.md` when choosing a fixture, boundary, or command.

## Find the shape before writing

```bash
rg --files -g '*.csproj' -g '*Tests*'
rg -l 'xunit|nunit|TUnit|MSTest' -g '*.csproj' .
```

Match what is there. A repository on NUnit does not want its first xUnit test, and a repository that puts integration tests in `tests/Integration` does not want them beside the unit tests because that was easier.

Treat this skill and its canonical references as the standard. Use project files, test dependencies,
directory layout, and repeated nearby patterns to select the repository's established variant; a
single isolated example is not enough evidence to introduce a new convention.

## The boundary

| | Unit | Integration |
|---|---|---|
| Touches | One type, its collaborators faked | The composed system — host, container, database |
| Costs | Milliseconds | Seconds, and a Docker daemon |
| Fails when | The logic is wrong | The wiring is wrong |
| Lives in | `<Project>.Tests` | `<Project>.IntegrationTests`, or `tests/Integration` |

Put it where it fails usefully. Logic with branches is a unit test; a route that returns the wrong status code, a mapping that drops a column, or a migration that does not apply is an integration test, and no amount of mocking finds any of the three.

## xUnit lifetimes — the part that is usually wrong

| Construct | Created | Use for |
|---|---|---|
| Constructor + `IDisposable` | **Once per test** | Cheap per-test state |
| `IClassFixture<T>` | Once per test class | An expensive object one class shares |
| `ICollectionFixture<T>` + `[Collection("name")]` | Once per collection, across classes | A container, a host, a database |

Tests in the same collection do not run in parallel; different collections do. Standing up a
Testcontainers database in a constructor starts one container per test.

Concrete fixtures and commands: [xunit-unit](references/examples/xunit-unit.md),
[http-integration](references/examples/http-integration.md),
[deployed-smoke](references/examples/deployed-smoke.md).

`Program` must be reachable from the test project (`public partial class Program;` or
`InternalsVisibleTo`). Prefer a real dependency in a container over `UseInMemoryDatabase`.

## Running them

Check formatting without rewriting the verifier's input:

```bash
dotnet format <solution> --no-restore --verify-no-changes
dotnet test <solution>
```

### Targeted verify first

Prefer filter or project scope that covers the change before the full suite:

```bash
dotnet test <test-project> --no-restore --filter FullyQualifiedName~<CaseName>
dotnet test <solution> --no-restore
```

Implement uses the narrow command while iterating. Verifier runs the criterion command, then one
wider suite. Do not clean `TestResults/` mid-loop unless diagnosing a stale-output failure.

### Contention constraints

- Do not run overlapping full-suite `dotnet test` on the same solution from concurrent agents.
- Prefer `--no-restore` after a single restore. Do not invent a shared coordination lock file.

Report the command and its output. `dotnet test` exits non-zero on failure, and a test run whose output was not read is not evidence.

Good: `IClassFixture` for a shared factory; filtered test while implementing.
Bad: start Testcontainers in the constructor; full-suite every TDD cycle.
