# PBI-55 (AP55): Import-graph boundaries as a test in each language pack
- Status: todo
- Area: dotnet
- Phase: H architecture
- Lane: Builder
- Depends on: none (soft: [AP62](62-patterns-catalog.md) PA05 names the pattern)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter LanguagePackContractTests` exits 0, and the fixture pack without a boundary template fails with BND01
- door: two-way
- triage: build (Sven must-cover theme 8)
- feedback: `LanguagePackContractTests` per pack
- harness: the boundary test runs in the consumer repo test suite
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 26:42-29:44

## Problem now

The .NET pack states the dependency rule in prose ([`plugins/dotnet/standards/layers.md:10-20`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/dotnet/standards/layers.md#L10-L20)) and where new code goes ([`plugins/dotnet/standards/layers.md:87-99`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/dotnet/standards/layers.md#L87-L99)).
No test fails when Application references Infrastructure. The reviewer catches it by reading, or not.

## Why (plain words)

Dune enforces its rings "through the import and dependency graph". A class of bad performance bugs (slow code imported into the Electron renderer) was "categorically eliminated through the architecture" ([talk 27:57](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L77), [talk 28:52](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L79)).
Prose is the weakest layer ([AP51](51-finding-guard-layer.md)). A test is the strongest one we can ship in a pack.

## What changes

Every language pack whose standards have a `## Dependency rule` ships a boundary test template the implementer copies once into the consumer repo:

| Pack | Template | How it reads the graph |
|---|---|---|
| dotnet | `plugins/dotnet/templates/BoundaryTests.cs` | parses `*.csproj` `ProjectReference` (XML, no new package) |

Today only [`plugins/dotnet/standards/layers.md:10`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/dotnet/standards/layers.md#L10) has `## Dependency rule`, so dotnet is the only pack in scope. typescript (`no-restricted-imports`) and rust (`cargo metadata`) templates wait until those packs state a dependency rule in their own PBI.

Contract: BND01 in `LanguagePackContractTests`.

## Example (dotnet, sketch)

```csharp
[Fact]
public void Application_does_not_reference_Infrastructure()
{
    var refs = ProjectGraph.Load("src").ReferencesOf("Orders.Application");
    Assert.DoesNotContain("Orders.Infrastructure", refs);
}
```

Before: a reviewer comment "Application must not use DbContext". After: `dotnet test` fails with the project names.

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter LanguagePackContractTests` exits 0.
- [ ] Fixture pack `Fixtures/packs/no-boundary-template/` (has `## Dependency rule`, no template) fails with `BND01 pack declares a dependency rule but ships no boundary test template`.
- [ ] `rg -n 'BoundaryTests' plugins/dotnet/standards/layers.md` prints 1 line (the standard points at the template).

## Technical design

- Module boundary: the pack ships a template; the consumer repo owns the copy. No runtime dependency on agentPacks.
- Rules: BND01.
- Only packs that already state a dependency rule. No new architecture rules in this PBI.

## Sources

- Talk: [talk 26:42](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L75), [talk 27:57](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L77), [talk 28:52](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L79), [talk 29:44](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L81). Notes: [notes 4.13](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L70), [notes 4.14](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L71), [notes 4.16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L73), [notes 9.5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L122).
- pstack pstack [`correct/SKILL.md:19`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L19) ("Hide internals so the wrong import fails"), pstack [`principle-boundary-discipline/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-boundary-discipline/SKILL.md).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/dotnet/standards/layers.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/dotnet/standards/layers.md)
- edit: [`tools/AgentPacks.Cli.Tests/LanguagePackContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/LanguagePackContractTests.cs)
- new (planned): [`plugins/dotnet/templates/BoundaryTests.cs`](../../../plugins/dotnet/templates/BoundaryTests.cs)
