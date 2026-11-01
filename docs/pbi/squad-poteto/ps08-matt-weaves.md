# PS08: Weave Matt skills into squad
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven on code-review: "When CODING_STANDARDS.md maybe add this per repo ... check all bold things I like that all".
- Sven on diagnose-bugs: "please use some things in the potato". On domain-models: "use it to get docs/adr/* better". On pr: "when asking to create pr in ai interface or when pr already open". On research: "use this when researching or creating sub agent". On retro: "improve my retro stap".

## Current state on main (`d76fa14`)

- [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/squad/references/review-contract.md), [`plugins/squad/skills/squad/references/adr.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/squad/references/adr.md), [`plugins/squad/skills/squad/references/retro.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/skills/squad/references/retro.md), [`plugins/squad/agents/squad-planner.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/agents/squad-planner.md).

## Target structure

| Matt skill | Lands in | What we take |
|---|---|---|
| code-review | `squad-reviewer` + `references/review-contract.md` | Two axes (Standards, Spec) as two parallel reviews, never reranked across axes; Fowler smell baseline |
| diagnosing-bugs | `playbooks/bug-fix.md` | Phases 1 to 6: red feedback loop, reproduce and minimise, hypothesise, instrument, fix plus regression test, cleanup |
| domain-modeling | `references/adr.md` + GLOSSARY rule | Challenge terms, update GLOSSARY.md inline, ADRs lazily |
| research | squad-planner research step | Fresh-context background agent, primary sources, one cited file |
| retro | `squad-retro` + `references/retro.md` | Environment fixes: navigation, checks, standards, AGENTS.md, tool economy |
| pr | squad PR step | Summary (Apache-2.0 attribution), Evidence, Merge Danger; create PR or add to an open one |

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| review | [code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | [code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | weave (MIT line in NOTICE) | CODING_STANDARDS.md (PS10) |
| bug | [diagnosing-bugs](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/diagnosing-bugs/SKILL.md) | [diagnosing-bugs](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/diagnosing-bugs/SKILL.md) | weave (MIT line in NOTICE) | hitl-loop template, GLOSSARY.md |
| plan, docs | [domain-modeling](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling/SKILL.md) | [domain-modeling](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling/SKILL.md) | weave (MIT line in NOTICE) | GLOSSARY-FORMAT.md, ADR-FORMAT.md |
| plan | [research](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research/SKILL.md) | [research](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research/SKILL.md) | weave (MIT line in NOTICE) | none |
| retro | [retro](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/retro/SKILL.md) | [retro](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/retro/SKILL.md) | weave (MIT line in NOTICE) | writing-for-agents |
| PR | [pr](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/pr/SKILL.md) | [pr](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/pr/SKILL.md) | weave (MIT line in NOTICE) + Apache-2.0 | CREDITS.md |

## Subagents and what they load

- squad-reviewer: two-axis review. squad-planner: research step. squad-retro: Matt retro.

## Steps for Builder

1. One edit per row, NOTICE line per file.
2. Fixtures: two-axis review output, bug loop phases, PR body with Evidence and Merge Danger.

## DoD

- NOTICE line per weave.
- Fixtures green.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture names, NOTICE diff.

## Risks and out of scope

- Matt text is MIT: paraphrase or copy with the line. Out of scope: architecture review mode (PS09).
