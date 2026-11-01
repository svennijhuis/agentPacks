# PS06: squad-md hub in poteto structure
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS04
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "please check all things h1 Principles, Non-negotiables, Autonomy, Subagents, Writing the reply, Comments, Playbooks. Please rethink our squad skill flow. So we have precisely the same structure and we of course also have standarts is also a hoofdstuk."
- Sven: "Bundled skills live at ../<name>/SKILL.md from this file, so put this file's absolute path in every poteto-agent prompt. This line also important."
- PO decision 2: rename `squad` to `squad-md`. PO decision 4: tdd in Non-negotiables (Sven: "We can use this really hard").

## Current state on main (`d76fa14`)

- [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/squad/SKILL.md) (about 85 lines after #85): H2s Locked v1 flow, Route, Gated agents, Skills, Stacks, Plan, Implement verify review, Advisor-lite, Worktree.
- Rules inline, no Non-negotiables, no Principles. Skill name equals command name `squad` (breaks the name rule).

## Target structure

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
- Any prose goes through the **unslop** skill.
- No comment unless it says why (**no-comments**).
- Cite only principles and standards whose leaf was read this session.
- Plan file `docs/squad-plan.md` is the state (Session rules from #85).

## Principles
Read the leaf skill in full for any principle you apply. ...
- **Sequence Work into Verifiable Units.** Multi-step work. Leaf: principle-sequence-verifiable-units.
- ... (24 lines, groups Core, Architecture, Verification, Delegation, Meta, same as poteto)

## Autonomy
## Subagents
## Writing the reply
## Comments
## Playbooks
- **Bug fix.** A reported defect. File: `playbooks/bug-fix.md`.
- ... (PS07)

## Standards
Read the leaf skill in full for any standard you apply. ... (PS14)
```

Locked v1 flow content (Route, Gated agents, Stacks, Plan, Implement verify review, Advisor-lite, Worktree) moves into Autonomy, Subagents and `references/` files, not deleted.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| hub | poteto-mode | [poteto-mode](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | structure copied as headings only; text is ours; skill pinned as-is (`external-skills.json`, 40-hex) | all leafs |
| Non-negotiables | tdd | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | correct, why |
| Non-negotiables | unslop | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | none |
| Comments | no-comments | [no-comments](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/no-comments/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, why, architect, 2 principles |
| Principles | 24 principle leafs | [principle-attack-the-premise](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-attack-the-premise/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | see PS04 |

## Subagents and what they load

- Subagents chapter lists each `squad-*` agent with its load list (filled in PS15). Hub passes its own absolute path in every spawn prompt. Fresh agent per fix round, hub re-reads the diff.

## Steps for Builder

1. Rename dir to `squad-md`, update command `squad.md` and `squad-review.md` to load `squad-md`.
2. Write the hub with write-skill (PS01).
3. Move flow sections into the H2s or `references/` without changing behavior.
4. Keep #85 Session behavior (split, status question, already done, half done, depends skip, close plus archive).
5. Update SquadContractTests for new names.

## DoD

- All existing squad fixtures green with no expected-output change.
- Hub H2 order test passes.
- check-skill PASS on the hub.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Test diff shows only renames.
- Hub H2 list in PR body.

## Risks and out of scope

- Big file move: keep behavior identical, one PR.
- Out of scope: playbook files (PS07), standard leafs (PS14).
