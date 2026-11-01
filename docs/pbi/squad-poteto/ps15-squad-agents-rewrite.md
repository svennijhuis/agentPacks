# PS15: Rewrite squad agents with write-subagent (2 PRs)
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B3
- Depends on: PS02, PS13, PS14
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "wij hebben sub agents ook dus denk daar ook aan". PO: each agent gets role, non-negotiables, principles and playbooks it uses, and the absolute path line.

## Current state on main (`d76fa14`)

- 10 agents in [`plugins/squad/agents/`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/agents/squad-planner.md): architect, implementer, orchestrator, planner, retro, reviewer, security-reviewer, simplifier, verifier (plus squad-indexer per map). No principle refs, no abs-path line.

## Target structure

| Agent | Tier | Loads skills | Principles | Standards |
|---|---|---|---|---|
| squad-planner | standard | teach, technical-writing, blast-radius, statechart (stateful), research (woven) | Sequence Verifiable Units, Subtract Before You Add | Coding Standards File |
| squad-architect | frontier | architect (+ red flags), codebase-design | Foundational Thinking, Model the Domain, Type System Discipline, Exhaust the Design Space | Coding Standards File |
| squad-implementer | standard | tdd, no-comments | Prove It Works, Sequence Verifiable Units, Test Behavior, Laziness Protocol | No Em Dash, No Secrets in Markdown |
| squad-reviewer | standard | interrogate, blast-radius, two-axis review (woven) | Minimize Reader Load, Test Behavior | Review Contract, Coding Standards File |
| squad-simplifier | standard | no-comments, unslop | Laziness Protocol, Subtract Before You Add, Minimize Reader Load | none |
| squad-orchestrator | standard | arena (high risk) | Prove It Works, Explain the Number | Review Contract |
| squad-verifier | standard | none | Prove It Works, Explain the Number | none |
| squad-retro | standard | correct, maintain pairs, retro (woven) | Encode Lessons in Structure | none |
| squad-security-reviewer | standard | none | Boundary Discipline | No Secrets in Markdown |
| squad-indexer | fast | none (cheap by design) | Guard the Context Window | none |

PR a: planner, architect, implementer, reviewer, simplifier. PR b: orchestrator, verifier, retro, security-reviewer (light). indexer unchanged.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| all | see table | [pstack/skills](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/) | pinned as-is (`external-skills.json`, 40-hex) | PS04 |
| subagent rules | poteto-mode Subagents | [poteto-mode](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | rides with the poteto-mode pin | none |

## Subagents and what they load

- This PBI is the agents. The hub Subagents chapter gets the same table.

## Steps for Builder

1. Rewrite each agent with write-subagent.
2. Fill the hub Subagents chapter.
3. gh-aw generator emits concrete models.
4. Two PRs, two docs rows (PS15a, PS15b).

## DoD

- check-subagent PASS for every agent.
- validate 0/0, tests green.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Per-agent verdict list.

## Risks and out of scope

- Tier words pass verbatim to Copilot in gh-aw (not validated): generator must resolve. Out of scope: security plugin agents (PS18).
