# PS06: squad-md hub with the new chapter structure
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS04
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "h1 Principles, Non-negotiables, Autonomy, Subagents, Writing the reply, Comments, Playbooks ... precisely the same structure and we of course also have standarts is also a hoofdstuk".
- Sven: the `Bundled skills live at ../<name>/SKILL.md` line is important.

## Current state on main (`6c59f49`)

- [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/skills/squad/SKILL.md) H2s: Locked v1 flow, Route, Gated agents, Skills, Stacks, Plan, Implement verify review, Advisor-lite, Worktree, plus #85 Session rules.
- Skill name equals command name `squad`.

## Target structure

gh-aw: hub `## Autonomy` gets: a run with no human takes the default answer and writes the open question into the report. Local only: yes or no asks, `docs/squad-plan.md` and retro writes. Rename `squad` to `squad-md` lands with the old dir as a pointer; the AW pin bump (path + closure + `@v2.0.0`) is one PR in the AW repo ([GA05](gh-aw/ga05-aw-catalog-bump.md)), see [MIGRATION](MIGRATION.md#what-breaks-from-v1).

```markdown
---
name: squad-md
description: When /squad or /squad-review runs.
license: UNLICENSED
user-invocable: false
---

# Squad

Bundled skills live at `../<name>/SKILL.md` from this file, so put this file's absolute path in every `squad-*` agent prompt.

## Non-negotiables
- Any behavior change with a local test target goes to the **tdd** skill.
- Any prose goes through **unslop**. No comment unless it says why (**no-comments**).
- Cite only principles and standards whose leaf was read this session.
- `docs/squad-plan.md` is the state (#85).

## Principles
Read the leaf skill in full for any principle you apply. ...
## Autonomy
## Subagents
## Writing the reply
## Comments
## Playbooks
## Standards
```

Flow sections move into these H2s or `references/`, no behavior change.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| hub | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |
| Non-negotiables | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |
| Non-negotiables | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pin | external-skills.json | Every prose output. |
| Comments | [no-comments](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/no-comments/SKILL.md) | copy + adapt | `plugins/squad/skills/no-comments/` | Spawns the `comment-sicko` plugin agent, which we cannot pin. We point it to `squad-simplifier`. |
| Principles | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |

## Example

Good (what the result looks like):

```markdown
## Playbooks
Pick by `Task type:` in the plan row.
- **bug** to playbooks/bug-fix.md
- **feature** to playbooks/feature.md
- none fits: **figure-it-out**
```

Bad (what we do not want):

```markdown
## Route
When the request is a bug, first reproduce, then write a failing test,
then fix it, then ... (40 lines of steps in the hub)
```
Steps belong in the playbook, the hub only routes.

## Subagents and what they load

- Subagents chapter: table filled in PS15. Fresh agent per fix round; hub re-reads the diff.

## Steps for Builder

1. Rename dir to `squad-md`; `squad.md` and `squad-review.md` load it.
2. Write with write-skill.
3. Move sections without behavior change; keep #85 Session behavior.
4. Update SquadContractTests names.

## DoD

- All squad fixtures green without expected-output change.
- Hub H2 order test.
- check-skill PASS.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Test diff = renames only.

## Risks and out of scope

- Large move: one PR, no behavior change.
- Out: standards (PS07), playbooks (PS09).
