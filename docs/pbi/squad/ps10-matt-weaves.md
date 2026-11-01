# PS10: Weave Matt skills into the playbooks and agents
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS09
- Absorbs old: old 45 (hand-off in PR-body order)
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven on code-review, diagnose-bugs, domain-modeling ("docs/adr better"), pr ("when asking to create pr ... or when pr already open"), research, retro ("improve my retro stap").

## Current state on main (`6c59f49`)

- [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/skills/squad/references/review-contract.md), `adr.md`, `retro.md`, [`plugins/squad/agents/squad-planner.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/agents/squad-planner.md).

## Target structure

One edit per Matt skill into its home (see reuse table), one NOTICE line each.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| review | [Matt code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | weave | `squad-reviewer`, `references/review-contract.md` | Two axes (Standards, Spec). Our severity words and CODING_STANDARDS.md stay. |
| bug | [Matt diagnosing-bugs](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/diagnosing-bugs/SKILL.md) | weave | `playbooks/bug-fix.md` | Phases 1 to 6 into the copied bug-fix playbook. |
| domain | [Matt domain-modeling](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling/SKILL.md) | weave | `references/adr.md`, `playbooks/domain.md` | Better `docs/adr/` and GLOSSARY, Sven's ask. |
| plan | [Matt research](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research/SKILL.md) | weave | `squad-planner` research step | Fresh-context research, cited file. |
| retro | [Matt retro](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/retro/SKILL.md) | weave | `squad-retro`, `references/retro.md` | Environment fixes into our retro. |
| pr | [Matt pr (Summary from humanlayer show-me, Apache-2.0)](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/pr/SKILL.md) | weave | `playbooks/pr.md` | Evidence and Merge Danger; Apache-2.0 credit in NOTICE. |

## Example

Good (what the result looks like):

```markdown
## Review (two axes, from Matt code-review)
- Standards axis: each finding names a CODING_STANDARDS.md rule.
- Spec axis: each finding names a PBI acceptance line.
Severity words stay ours: blocker, high, medium, low, tiny.
```

Bad (what we do not want):

Replacing our review contract with Matt's file, which loses our severity words and finding identity.

## Subagents and what they load

- reviewer two axes; planner research; retro Matt retro.

## Steps for Builder

1. One edit per row.
2. Fixtures: two-axis review, bug phases, PR body with Evidence and Merge Danger.

## DoD

- NOTICE line per weave; fixtures green.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture names.

## Risks and out of scope

- Out: architecture review (PS11).
