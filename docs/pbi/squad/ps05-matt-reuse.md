# PS05: Matt reuse and NOTICE
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B1
- Depends on: PS03
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "van matt vaak wel herschrijven of waven in het ook prima".
- Sven on codebase-design: design it twice, deep vs shallow.

## Current state on main (`6c59f49`)

- No Matt skill used. `style.md` already uses Matt vocabulary without NOTICE.

## Target structure

Pin `codebase-design` (path `skills/engineering/codebase-design`, `4ae2317ddb22b046e26455e4f9c9b3aaaf62212d`). NOTICE: MIT, Copyright (c) 2026 Matt Pocock, per used path; Apache-2.0 block for the humanlayer show-me derived `pr` Summary. Matt `tdd` and `teach` not used (pstack names win).

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| pin | [Matt codebase-design](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design/SKILL.md) | pin | external-skills.json | Sven likes design it twice and deep vs shallow; use as is. |
| weave later | [Matt code-review](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/code-review/SKILL.md) | weave | `squad-reviewer`, `references/review-contract.md` | Two axes (Standards, Spec). Our severity words and CODING_STANDARDS.md stay. |
| weave later | [Matt pr (Summary from humanlayer show-me, Apache-2.0)](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/pr/SKILL.md) | weave | `playbooks/pr.md` | Evidence and Merge Danger; Apache-2.0 credit in NOTICE. |
| parked | [Matt chief-of-staff](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/in-progress/chief-of-staff/SKILL.md) | not used | parked | Waits for Sven. |

## Example

Good (what the result looks like):

```text
NOTICE
mattpocock/skills at 4ae2317, MIT, Copyright (c) 2026 Matt Pocock.
  woven: code-review, diagnosing-bugs, domain-modeling, pr, research, retro
  rewritten: improve-codebase-architecture
humanlayer show-me (pr Summary), Apache-2.0, credit kept.
```

Bad (what we do not want):

Woven Matt text with no NOTICE line, or the Apache-2.0 Summary without credit.

## Subagents and what they load

- squad-architect loads codebase-design (PS15).

## Steps for Builder

1. Add the record.
2. NOTICE blocks in `plugins/squad/NOTICE.md` and `plugins/pack-author/NOTICE.md`.
3. Test: every Matt path named in a skill has a NOTICE line.

## DoD

- NOTICE complete.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- NOTICE diff, test output.

## Risks and out of scope

- Matt renames often; bumps are one PR per source.
