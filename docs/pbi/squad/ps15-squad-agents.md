# PS15: Squad agents rewritten with write-subagent (PR a and PR b)
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B3
- Depends on: PS02, PS07, PS14
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: subagents too. Each gets role, Non-negotiables, principles, standards and playbooks it uses, plus the absolute path line.

## Current state on main (`6c59f49`)

- 10 agents in `plugins/squad/agents/` (architect, implementer, indexer, orchestrator, planner, retro, reviewer, security-reviewer, simplifier, verifier). No principle or standard refs.

## Target structure

| Agent | Tier | Skills | Principles | Standards |
|---|---|---|---|---|
| squad-planner | standard | teach, technical-writing, blast-radius, statechart-md, Matt research | Sequence Verifiable Units, Subtract Before You Add | Coding Standards File |
| squad-architect | frontier | architect, codebase-design, statechart-md | Foundational Thinking, Model the Domain, Type System Discipline, Exhaust the Design Space | Coding Standards File, Layers |
| squad-implementer | standard | tdd, no-comments | Prove It Works, Test Behavior, Laziness Protocol | No Em Dash, No Secrets in Markdown |
| squad-reviewer | standard | interrogate, blast-radius, Matt two axes | Minimize Reader Load, Test Behavior | Review Contract, Coding Standards File |
| squad-simplifier | standard | no-comments, unslop | Subtract Before You Add, Laziness Protocol | none |
| squad-orchestrator | standard | arena | Prove It Works, Explain the Number | Review Contract |
| squad-verifier | standard | none | Prove It Works, Explain the Number | none |
| squad-retro | standard | correct, maintain-verification-skill, Matt retro | Encode Lessons in Structure | none |
| squad-security-reviewer | standard | none | Boundary Discipline | No Secrets in Markdown |
| squad-indexer | fast | none (cheap by design) | Guard the Context Window | none |

PR a: planner, architect, implementer, reviewer, simplifier. PR b: the rest.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| all | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |
| all | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |
| all | [arena](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/arena/SKILL.md) | copy + adapt | `plugins/squad/skills/arena/` | Picks models by setup-pstack roles (Cursor only). We map to our tiers, cap at 3 candidates and high-risk work only (token budget). |

## Example

Good (what the result looks like):

```markdown
| Agent | Loads | Standards | Returns |
|---|---|---|---|
| squad-implementer | tdd, no-comments, playbook | Coding Standards File | diff, tests |
```

Bad (what we do not want):

An agent prompt that inlines whole skills, or has no absolute hub path.

## Subagents and what they load

- This PBI is the agents. Same table goes in the hub Subagents chapter.

## Steps for Builder

1. Rewrite each with write-subagent.
2. Fill Subagents chapter.
3. gh-aw generator emits concrete models.

## DoD

- check-subagent PASS for all 10.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Verdict list.

## Risks and out of scope

- Out: security plugin agents (PS18).
