# PS05: Matt pins and NOTICE (codebase-design pinned, rest woven)
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B1
- Depends on: PS03
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "van matt vaak wel herschrijven of waven in het ook prima".
- Sven: "/codebase-design ... I like that twice it design way also Deep vs shallow".
- PO decision 5: keep what Matt's `pr` took from humanlayer, with attribution in NOTICE.

## Current state on main (`d76fa14`)

- No Matt skill is used today.
- Our `style.md` already uses Matt's Leading words and Prune vocabulary without a NOTICE line.

## Target structure

- `plugins/squad/external-skills.json`: `codebase-design` at `4ae2317ddb22b046e26455e4f9c9b3aaaf62212d`, path `skills/engineering/codebase-design`, MIT (brings `DESIGN-IT-TWICE.md`, `DEEPENING.md`).
- `plugins/squad/NOTICE.md`: MIT text plus "Copyright (c) 2026 Matt Pocock" for every pinned or woven Matt file, listed by path.
- Apache-2.0 block for the humanlayer `show-me` derived Summary text used by the PR step (PS08), with the source repo named.
- `plugins/pack-author/NOTICE.md`: MIT line for writing-for-agents bits (PS01).
- Not pinned: Matt `tdd`, `teach` (name clash, pstack wins). `agents/openai.yaml` dropped when woven.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| plan, review | codebase-design | [codebase-design](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | DESIGN-IT-TWICE.md, DEEPENING.md, target GLOSSARY.md |
| PR step | pr (Summary from humanlayer show-me) | [pr](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/pr/SKILL.md) | weave (MIT line in NOTICE) + Apache-2.0 attribution | CREDITS.md, GLOSSARY.md |

## Subagents and what they load

- squad-architect loads **codebase-design** (PS15).

## Steps for Builder

1. Add the record.
2. Write the NOTICE blocks.
3. Test: NOTICE lists every Matt path referenced by any skill file (grep-based).

## DoD

- NOTICE covers every Matt file used.
- No Matt `tdd` or `teach` record.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- NOTICE diff.
- Test output.

## Risks and out of scope

- Matt renames skills often (write-a-skill to writing-great-skills to writing-for-agents). Bumps are one PR per source.
- Out of scope: the weaves (PS08).
