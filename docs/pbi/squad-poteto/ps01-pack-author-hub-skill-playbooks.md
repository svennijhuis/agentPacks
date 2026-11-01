# PS01: pack-author-md hub and skill playbooks (write-skill first)
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B0
- Depends on: none
- Lane: Builder, Cursor CLI (docs + markdown skill files)
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "en skills schrijven en die commando die we hebben om write skill herschrijven dan".
- Sven: "we hebben al playbooks voor skills want check, write etc". So we restyle what exists, no duplicates.
- Every later PBI writes skills with this writer, so it must produce the new style first. Fix the factory before the products.

## Current state on main (`d76fa14`)

- [`plugins/pack-author/skills/pack-author-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/skills/pack-author-md/SKILL.md) is a 25-line router with modes make, review, change.
- [`plugins/pack-author/skills/pack-author-md/references/make.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/skills/pack-author-md/references/make.md) (119 lines) writes a skill stub or an agent stub in one file.
- [`plugins/pack-author/skills/pack-author-md/references/review.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/skills/pack-author-md/references/review.md) (25), [`plugins/pack-author/skills/pack-author-md/references/change.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/skills/pack-author-md/references/change.md) (12), [`plugins/pack-author/skills/pack-author-md/references/style.md`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/pack-author/skills/pack-author-md/references/style.md) (54, scoring bars).
- No principles, no Non-negotiables, templates produce the old style. Description bar: starts `When `, about 120 chars, router about 40 lines.

## Target structure

```text
plugins/pack-author/skills/pack-author-md/
  SKILL.md                  # hub, poteto H2 order
  playbooks/write-skill.md  # from references/make.md (skill half)
  playbooks/check-skill.md  # from references/review.md
  playbooks/change.md       # from references/change.md, shared by skills and agents
  references/skill-bars.md  # from references/style.md + poteto bars + Matt writing-for-agents bits
```

Hub H2 order (same as poteto-mode): `## Non-negotiables`, `## Principles`, `## Autonomy`, `## Subagents`, `## Writing the reply`, `## Comments`, `## Playbooks`, then our `## Standards` (index, PS14).
Line under the H1, verbatim pattern: "Bundled skills live at `../<name>/SKILL.md` from this file, so put this file's absolute path in every `pack-author-*` agent prompt."

Skill frontmatter the writer emits:

```markdown
---
name: <kebab-name>
description: When <trigger>.
license: UNLICENSED
user-invocable: false
---
```

write-skill steps (target file, about 7 lines):
1. Pick the owning pack (`docs/PLAN.md`). Name = dir, kebab, not equal to any command or plugin name.
2. Use-when frontmatter, nothing else in it.
3. Non-negotiables first: triggers that route to skills by exact name.
4. Principles index: one bold line per principle with when it applies. Link the leaf by name, never restate it. Same for Standards.
5. Any step list over about 5 lines moves to `playbooks/<kebab>.md` (H3 start, `### Title`).
6. List skill deps. Every upstream one gets an `external-skills.json` record (closure).
7. unslop pass, no-comments pass, `validate`, then check-skill.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| hub style | poteto-mode | [poteto-mode](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | structure reference only, pinned as-is (`external-skills.json`, 40-hex) in PS04 | 24 principle leafs, playbooks |
| write-skill model | [poteto authoring-a-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/authoring-a-skill.md) | [poteto authoring-a-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/authoring-a-skill.md) | rides with the poteto-mode pin | Cursor built-in create-skill, Opening a PR |
| write-skill step 7 | unslop | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | none |
| write-skill step 7 | no-comments | [no-comments](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/no-comments/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | comment-sicko agent role, how, why, architect |
| skill-bars | Matt writing-for-agents (context pointers, two loads, info hierarchy, leading words, pruning) + SKILL-MECHANICS | [skills tree](https://github.com/mattpocock/skills/blob/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/) | weave (MIT line in NOTICE) | none |
| principles cited | principle-encode-lessons-in-structure, principle-subtract-before-you-add, principle-minimize-reader-load, principle-guard-the-context-window, principle-prove-it-works | [principle-prove-it-works](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) (PS04) | show-me-your-work for prove-it-works |

## Subagents and what they load

- `pack-author-reviewer` scores against `references/skill-bars.md` (rewritten in PS02 for both bar sets). Loads **unslop**. Principles: **Minimize Reader Load**, **Prove It Works**.

## Steps for Builder

1. Branch off main after #85 lands. One PR, this PBI only.
2. Move `references/make.md` skill half to `playbooks/write-skill.md`, restyle to the 7 steps above. Leave the agent half for PS02 (temporary pointer line).
3. Move `references/review.md` to `playbooks/check-skill.md`, add the new bars (Non-negotiables first, principles by name, playbooks under 5-line rule, deps closed).
4. Move `references/change.md` to `playbooks/change.md` unchanged in logic, restyle text.
5. Split `style.md` into `references/skill-bars.md`. Keep Leading words and Prune. Add Matt bits with the MIT line in `plugins/pack-author/NOTICE.md`.
6. Rewrite `SKILL.md` as the hub in poteto H2 order. Hub picks playbook by target: `SKILL.md` = write-skill or check-skill.
7. Update `docs/ADD-SKILL.md` and `docs/PACK-AUTHOR.md` to the new file names.
8. Add fixture `tests/fixtures/pack-author/new-skill/` made by following write-skill, plus a test that it passes validate and the check-skill bars.

## DoD

- A fixture skill written by write-skill passes `validate` 0/0 and check-skill PASS on the first try.
- No file under `references/` still describes a step list (all moved to `playbooks/`).
- `/pack-author` command still routes to `pack-author-md` (no new slash).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture path and validate output in the PR body.
- check-skill verdict line for the fixture.
- Test name that fails if the hub loses an H2 or the order changes.

## Risks and out of scope

- Hub is over 40 lines: needs the hub-length exception from PS03. Until PS03 lands, the bar warns. So PS03 can go in the same PR if Builder prefers, still one PBI each in the docs.
- Out of scope: agent writing (PS02), validator code (PS03), squad skill (PS06).
