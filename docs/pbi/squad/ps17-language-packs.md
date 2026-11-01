# PS17: Language packs restyle, import-boundary tests, typescript-best-practices
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B5
- Depends on: PS16
- Absorbs old: old 55 (import boundaries)
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: typescript-best-practices for the typescript plugin.
- Old 55: boundary rules as tests, still open on main (no boundary test found).

## Current state on main (`6c59f49`)

- dotnet, rust, typescript skills repeat "Internal. Do not run directly" boilerplate; no principle refs; no boundary tests.

## Target structure

Drop boilerplate, add index lines; pin typescript-best-practices in `plugins/typescript/external-skills.json`; one boundary-test template per language; review skills cite Coding Standards File.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| typescript | [typescript-best-practices](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/typescript-best-practices/SKILL.md) | pin | `plugins/typescript/external-skills.json` | Sven: add to the typescript plugin. |
| test | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |

## Example

Good (what the result looks like):

```text
csharp: test fails when Application references Azure.Storage.*
(import-boundary test, rule from standard Layers).
```

Bad (what we do not want):

A language pack restating the shared HTTP rule in its own words instead of the shared leaf.

## Subagents and what they load

- none

## Steps for Builder

1. One PR per language.

## DoD

- validate 0/0 per pack.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Per-pack output.

## Risks and out of scope

- Out: new standards text.
