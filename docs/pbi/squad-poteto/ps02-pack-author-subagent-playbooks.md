# PS02: Subagent playbooks in pack-author-md
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B0
- Depends on: PS01
- Lane: Builder, Cursor CLI
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "maar let op wij hebebn sub agents ook dus denk daar ook aan".
- Sven: "doe onderzoek in write skills ofzo die we hebben dat we ook twee playbooks hebben voor of sub agents of skills".
- The writer must make agent files in the same style, so all 14 agents (PS15) come out right.

## Current state on main (`d76fa14`)

- Agent half of [`plugins/pack-author/skills/pack-author-md/references/make.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/skills/pack-author-md/references/make.md) (tier, tools, stub).
- [`plugins/pack-author/agents/pack-author-reviewer.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/agents/pack-author-reviewer.md) (27 lines) scores one file against the old bars.
- [`docs/ADD-AGENT.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/docs/ADD-AGENT.md); tiers in [`models.source.json`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/models.source.json); neutral tool names in `Generation/NeutralTools.cs`.
- No check playbook for agents exists.

## Target structure

```text
plugins/pack-author/skills/pack-author-md/
  playbooks/write-subagent.md   # NEW, agent half of make.md, extended
  playbooks/check-subagent.md   # NEW, mirrors check-skill with agent bars
  references/subagent-bars.md   # NEW
plugins/pack-author/agents/pack-author-reviewer.md  # scores both bar sets
```

write-subagent steps:
1. One role, one job. Name = filename, `squad-*` or `<plugin>-*`.
2. Tier from `models.source.json` (`inherit`, `fast`, `standard`, `frontier`). Never a client model id.
3. Closed tool list (`read write edit grep glob bash webfetch websearch`). `readonly: true` forbids write and edit.
4. Name the skills, principle leafs and standard leafs it loads by exact name, and the absolute path of the parent SKILL.md it receives.
5. State what it returns to the main agent (required fields). The main agent owns and re-reads the output.
6. `validate`, then check-subagent.

Target agent file:

```markdown
---
name: squad-reviewer
description: Reviews a change against its plan and evidence. Use in every review phase.
model: standard
readonly: true
tools:
  - read
  - grep
  - glob
  - bash
---

You receive the absolute path of `squad-md/SKILL.md`. Bundled skills live at `../<name>/SKILL.md` from it.

## Non-negotiables
- Every finding cites path:line and a standard or principle.

Load: **interrogate**, **blast-radius**. Principles: **Minimize Reader Load**, **Test Behavior, Not Implementation**. Standards: **standard-review-contract**.

Return: findings with path:line, severity, the standard or principle cited.
```

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| subagent rules | poteto-mode Subagents section | [poteto-mode](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | rides with the poteto-mode pin | setup-pstack roles (we use our tiers instead) |
| check pair model | create-verification-skill / maintain-verification-skill pattern | [create-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-verification-skill/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) (PS04) | feature map refs |
| principles cited | principle-guard-the-context-window, principle-boundary-discipline, principle-separate-before-serializing-shared-state, principle-prove-it-works | [principle-guard-the-context-window](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-guard-the-context-window/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | none |

## Subagents and what they load

- `pack-author-reviewer` (standard, readonly, read grep glob): loads **unslop**, reads `skill-bars.md` or `subagent-bars.md` by target kind. Returns PASS, WARN or FAIL per bar with path:line.

## Steps for Builder

1. Split the agent half of `make.md` into `playbooks/write-subagent.md` with the 6 steps.
2. Write `playbooks/check-subagent.md`: tier valid, tools in closed list, readonly without write and edit, name = filename, load list resolves, return fields present, abs-path line present.
3. Write `references/subagent-bars.md`.
4. Rewrite `pack-author-reviewer.md` with write-subagent itself (dogfood).
5. Hub picks write-subagent or check-subagent for `agents/*.md` targets.
6. Update `docs/ADD-AGENT.md`.
7. Fixture `tests/fixtures/pack-author/new-agent/` plus a test.

## DoD

- A fixture agent written by write-subagent passes `validate` 0/0 and check-subagent PASS.
- `pack-author-reviewer` itself passes check-subagent.
- gh-aw copy of any agent gets a concrete model (agent files are not validated by gh-aw).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture path, validate output, check-subagent verdict.
- Negative fixture with `readonly: true` plus `write` fails.

## Risks and out of scope

- poteto Subagents hard-codes Cursor slugs and `poteto-agent`. We keep our tiers; Claude, Copilot and Codex would fail on the slugs.
- Codex has no plugin agents; generated `.codex/agents` TOML carries the path in prompt text.
- Out of scope: rewriting squad agents (PS15).
