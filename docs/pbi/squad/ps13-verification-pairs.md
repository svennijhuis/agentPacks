# PS13: Verification and formal verification pairs
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS09
- Absorbs old: old 41, 52, 53, 59
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: create-verification-skill plus its maintain follow-up, and the formal pair.

## Current state on main (`6c59f49`)

- [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/skills/verify-harness/SKILL.md) writes `docs/verify.md`. No feature map, no gardener.

## Target structure

feature and domain playbooks call create-verification-skill when the repo has none; retro calls maintain-verification-skill (one session, at most one PR). Risky code: the formal pair. verify-harness stays (different deliverable). Old Drive kinds api and trace (41) live in the generated verification skill.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| build | [create-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-verification-skill/SKILL.md) | pin | external-skills.json | Used as is; covers old feature map PBIs. |
| retro | [maintain-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/maintain-verification-skill/SKILL.md) | pin | external-skills.json | Used as is in retro. |
| build | [create-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-formal-verification-skill/SKILL.md) | pin | external-skills.json | Risky code only. |
| retro | [maintain-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/maintain-formal-verification-skill/SKILL.md) | pin | external-skills.json | Retro, risky code only. |

## Example

Good (what the result looks like):

```text
Repo has no verification skill -> create-verification-skill writes
.cursor/skills/verify-orders/ with 3 checks; verifier runs them.
Formal pair only for code marked risky in the plan.
```

Bad (what we do not want):

Writing a formal verification skill for every small change (token cost, no gain).

## Subagents and what they load

- implementer runs create; retro runs maintain.

## Steps for Builder

1. Hooks.
2. Fixture with create then maintain.

## DoD

- Order fixture green.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Out: verify-harness changes (PS16).
