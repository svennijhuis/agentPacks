---
name: dotnet-test-patterns
description: How tests are written and run in a .NET repository — the unit and integration boundary, xUnit fixtures and collections, WebApplicationFactory, Testcontainers, and the exact test command. Use when adding or changing a C# test, when deciding whether a change needs one, or when a test needs a real database or HTTP host.
license: UNLICENSED
---

# .NET test patterns

How a test is written *in this stack*. What deserves a test at all is a separate question, and it is not a .NET one.

Before writing or reviewing tests, read every file in `references/standards/`. In the authored source
tree, before marketplace generation, the same canonical documents are under `../../standards/`.

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

Tests in the same collection do not run in parallel; different collections do. That is the lever: a shared database goes in a collection fixture so it is started once, and the tests that share it are serialised against each other and nothing else.

Standing up a Testcontainers database in a constructor starts one container per test. On a twenty-test class that is twenty containers and several minutes, and it will look like flakiness rather than a design mistake.

## An HTTP integration test

`WebApplicationFactory<TEntryPoint>` boots the real host in-process — real routing, real middleware, real DI — with no port and no network.

```csharp
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            // Replace only what must not be real. Everything else stays wired.
        });
}
```

Replace the narrowest thing that works. A factory that stubs out the whole data layer is testing the test double.

`Program` must be reachable from the test project. Microsoft documents two alternatives for a
top-level-statements host: add `public partial class Program;` to `Program.cs`, **or** keep it internal
and grant the test assembly access with `InternalsVisibleTo`. Choose the shape that matches the
repository; do not require both. See [ASP.NET Core integration tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests).

## A real dependency

Testcontainers gives a real database per collection, disposed at the end.

```csharp
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder().Build();

    public string ConnectionString => container.GetConnectionString();

    public Task InitializeAsync() => container.StartAsync();
    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
```

That signature is xUnit v2. xUnit v3 uses `ValueTask`; inspect the installed package version before
choosing the interface implementation. See the [xUnit v3 migration guidance](https://xunit.net/docs/getting-started/v3/migration).

Use `IAsyncLifetime`, not the constructor: starting a container is I/O, and a constructor cannot await it.

Prefer a real dependency in a container over an in-memory substitute. `UseInMemoryDatabase` does not enforce constraints, does not run migrations and does not speak the provider's SQL — it passes on the queries most likely to break in production.

## Running them

Check formatting without rewriting the verifier's input:

```bash
dotnet format <solution> --no-restore --verify-no-changes
```

```bash
dotnet test <solution>
```

```bash
dotnet test <solution> --filter "FullyQualifiedName~Integration"
```

Report the command and its output. `dotnet test` exits non-zero on failure, and a test run whose output was not read is not evidence.

The canonical testing standard owns test naming, observable behavior, deterministic time, fixture
lifetime, and behavior-change coverage. Apply it rather than restating a second local checklist here.
