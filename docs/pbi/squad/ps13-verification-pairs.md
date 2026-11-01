# PS13: Verification and formal verification pairs
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS09
- Absorbs old: old 41, 52, 53, 59
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

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

## Subagents and what they load

- implementer runs create; retro runs maintain.

## Steps for Builder

1. Hooks.
2. Fixture with create then maintain.

## DoD

- Order fixture green.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Out: verify-harness changes (PS16).
