# PS02: Subagent playbooks in pack-author-md
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B0
- Depends on: PS01
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "maar let op wij hebben sub agents ook".
- Sven: two playbooks, one for skills and one for subagents.

## Current state on main (`6c59f49`)

- Agent half of [`plugins/pack-author/skills/pack-author-md/references/make.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/pack-author/skills/pack-author-md/references/make.md).
- [`plugins/pack-author/agents/pack-author-reviewer.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/pack-author/agents/pack-author-reviewer.md) scores one file against the old bars.
- No check playbook for agents.

## Target structure

```text
plugins/pack-author/skills/pack-author-md/
  playbooks/write-subagent.md   # NEW (agent half of make.md, extended)
  playbooks/check-subagent.md   # NEW
  references/subagent-bars.md   # NEW
```

write-subagent steps:
1. One role, one job. Name = filename.
2. Tier from `models.source.json` (`inherit`, `fast`, `standard`, `frontier`), never a client model id.
3. Closed tool list; `readonly: true` forbids write and edit.
4. Name the skills, principle leafs and standard leafs it loads, and say it receives the absolute path of the parent hub.
5. Required return fields. The main agent owns and re-reads the result.
6. `validate`, then check-subagent.

```markdown
---
name: squad-reviewer
description: Reviews a change against its plan and evidence. Use in every review phase.
model: standard
readonly: true
tools: [read, grep, glob, bash]
---

You receive the absolute path of `squad-md/SKILL.md`. Bundled skills live at `../<name>/SKILL.md` from it.

## Non-negotiables
- Every finding cites path:line and a standard or principle.

Load: **interrogate**, **blast-radius**. Principles: **Minimize Reader Load**. Standards: **Review Contract**.
Return: findings with path:line, severity, cited standard or principle.
```

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| subagent rules | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |
| principles | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |

## Example

Good (what the result looks like):

```markdown
---
name: squad-verifier
description: Runs the plan's verify commands and reports numbers.
---
Load `/abs/path/plugins/squad/skills/squad-md/SKILL.md` first.
Read only: **Prove It Works**, **Explain the Number**, **Review Contract**.
Return: PASS or FAIL, command, run count, numbers.
```

Bad (what we do not want):

```markdown
You are a helpful verifier. Load all skills you think are useful
and check if the code looks fine.
```
No hub path, no named leafs, no return shape.

## Subagents and what they load

- `pack-author-reviewer` (standard, readonly) reads `skill-bars.md` or `subagent-bars.md` by target kind.

## Steps for Builder

1. Write the 3 files.
2. Rewrite `pack-author-reviewer.md` with write-subagent (dogfood).
3. Hub routes `agents/*.md` targets to the subagent playbooks.
4. Update `docs/ADD-AGENT.md`.
5. Fixture `tests/fixtures/pack-author/new-agent/` plus a negative fixture (`readonly` with `write`).

## DoD

- Fixture agent passes validate 0/0 and check-subagent PASS.
- Negative fixture fails.
- gh-aw output gets a concrete model (gh-aw does not validate agent files).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture output, both verdicts.

## Risks and out of scope

- Codex has no plugin agents; generated TOML carries the path in prompt text.
- Out: squad agents (PS15).
