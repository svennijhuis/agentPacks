# PS14: Review and lesson stack
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS09
- Absorbs old: old 51, 57, 58
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: arena "different models are in arena and so in end we have best of all"; interrogate "try to break it"; blast-radius in plan, task, work items, review; correct "not making the same mistakes"; teach and how in planning.

## Current state on main (`6c59f49`)

- One reviewer pass; no repeated-mistake check.

## Target structure

| Slot | Skill | When |
|---|---|---|
| plan | blast-radius, teach, how, why | every plan with code |
| review | interrogate, blast-radius | every review |
| review | arena (our copy, max 3) | high risk only: security, data loss, concurrency |
| retro | correct | every retro; repeated mistakes become checks (old 57, 58, 51) |
| any | figure-it-out | no playbook fits |

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| review | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pin | external-skills.json | Used as is in review. |
| review | [arena](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/arena/SKILL.md) | copy + adapt | `plugins/squad/skills/arena/` | Picks models by setup-pstack roles (Cursor only). We map to our tiers, cap at 3 candidates and high-risk work only (token budget). |
| plan, review | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | pin | external-skills.json | Used as is in plan, task and review. |
| retro | [correct](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/correct/SKILL.md) | pin | external-skills.json | Used as is in retro; covers old lint-first and guard ladder PBIs. |
| plan | [teach](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/teach/SKILL.md) | pin | external-skills.json | Used as is in plan. |
| plan | [how](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how/SKILL.md) | pin | external-skills.json | Used as is. |
| plan | [why](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why/SKILL.md) | pin | external-skills.json | Used as is. |
| fallback | [figure-it-out](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/figure-it-out/SKILL.md) | pin | external-skills.json | Fallback when no playbook fits. Resolves our arena copy by name. |

## Example

Good (what the result looks like):

```text
Retro: same missing-null-check found in 2 PRs ->
correct turns it into validator rule or standard leaf (one PR).
```

Bad (what we do not want):

A retro that only lists lessons in prose and never changes a check, test or leaf.

## Subagents and what they load

- reviewer, orchestrator (arena), planner, retro.

## Files to change

Only the files named in **Target structure** and **Steps for Builder**. No moves or renames.

## How to do it

1. Branch from main: `git switch -c pbi/ps14-review-and-lesson-stack origin/main`.
2. Read [SHAPES.md](SHAPES.md) for every kind in the table above, and the upstream source linked in **External skills per step**.
3. Do each row of **Files to change** in order. For a move or rename use `git mv`; for a copy keep the `Source:` line and `## What we changed`.
4. Follow **Steps for Builder** below. Nothing outside the table and the steps.
5. Run the checks in **Done check**. All must pass before you open the PR.
6. Open one PR titled `PS14: Review and lesson stack`, body: link to this file, the table, the check output.

## Done check

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
for f in tests/fixtures/gh-aw/*.md; do gh aw compile --strict "$f"; done   # from GA04 on; before GA04 write "n/a, no fixture yet"
grep -rnP "[\x{2013}\x{2014}]" plugins docs || echo "no dashes"
```

Local: install `/tmp/agentpacks-marketplace` in Cursor or Claude Code (README "Install the generated tree") and run the commands this PBI touches once.

## Steps for Builder

1. Hooks in review.md, plan.md, retro.md.
2. Risk flag decides arena.
3. Fixtures.

## DoD

- Arena only in the high-risk fixture.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output.

## Risks and out of scope

- Token cost: arena high risk only.
