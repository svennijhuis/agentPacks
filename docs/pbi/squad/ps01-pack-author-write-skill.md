# PS01: pack-author-md hub and skill playbooks (write-skill first)
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B0
- Depends on: none
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "skills schrijven en die commando die we hebben om write skill herschrijven".
- Sven: "we hebben al playbooks voor skills want check, write etc": restyle what exists, no duplicates.
- Every later PBI writes with this writer, so it goes first.

## Current state on main (`6c59f49`)

- [`plugins/pack-author/skills/pack-author-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/pack-author/skills/pack-author-md/SKILL.md) router with modes make, review, change.
- [`plugins/pack-author/skills/pack-author-md/references/make.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/pack-author/skills/pack-author-md/references/make.md) (119 lines), `review.md` (25), `change.md` (12), `style.md` (54).
- Templates produce the old style: no Non-negotiables, no Principles or Standards index, no playbooks.

## Target structure

```text
plugins/pack-author/skills/pack-author-md/
  SKILL.md                  # hub, poteto H2 order
  playbooks/write-skill.md  # from references/make.md (skill half) + poteto authoring-a-skill (copy + adapt)
  playbooks/check-skill.md  # from references/review.md
  playbooks/change.md       # from references/change.md
  references/skill-bars.md  # from references/style.md + Matt writing-for-agents bits
```

Hub H2 order: Non-negotiables, Principles, Autonomy, Subagents, Writing the reply, Comments, Playbooks, Standards.
Under the H1: "Bundled skills live at `../<name>/SKILL.md` from this file, so put this file's absolute path in every `pack-author-*` agent prompt."

write-skill steps:
1. Owning pack from `docs/PLAN.md`. Name = dir, kebab, never equal to a command or plugin name.
2. Use-when frontmatter only.
3. Non-negotiables first: triggers that route to skills by exact name.
4. Principles and Standards as index lines (bold title, when it applies, leaf name). Never restate a leaf.
5. Any step list over 5 lines goes to `playbooks/<kebab>.md`, starting at H3.
6. Reuse choice per upstream skill: pin, copy + adapt, weave or rewrite, with one reason line. Every copy starts with `Source: <upstream link at 40-hex SHA>, MIT, see NOTICE.` under the H1 and ends with `## What we changed` (one line per change). Scripts and assets stay byte-equal unless listed there. Cursor-only parts (model slugs, `poteto-agent`, Cursor Task, `/loop`, control skills) are removed, or moved under a last `## Provider notes` section with one H3 per provider, so the main text works the same in Cursor, Claude Code, Copilot, Codex and gh-aw.
7. unslop pass, no-comments pass, `validate`, then check-skill.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| hub style | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |
| write-skill | [upstream playbook authoring-a-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/authoring-a-skill.md) | copy + adapt | `pack-author-md/playbooks/write-skill.md` | Calls Cursor built-in create-skill. We replace it with our make steps and validate. Keeps our standards and gates (PS09). |
| step 7 | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pin | external-skills.json | Every prose output. |
| step 7 | [no-comments](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/no-comments/SKILL.md) | copy + adapt | `plugins/squad/skills/no-comments/` | Spawns the `comment-sicko` plugin agent, which we cannot pin. We point it to `squad-simplifier`. |
| principles | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |

## Example

Good (what the result looks like):

```markdown
---
name: retry-policy
description: When code calls a remote service and needs retries.
---
# Retry policy

## Non-negotiables
- Any behavior change goes to the **tdd** skill.

## Principles
- **Fix Root Causes.** A retry hides a bug. Leaf: principle-fix-root-causes.
```

Bad (what we do not want):

```markdown
## Principles
Fix root causes means you should always look for the real cause of a bug
before you add a workaround, because workarounds pile up and ...
```
Restates the leaf text in the skill (two copies drift). The index line only names it.

## Subagents and what they load

- `pack-author-reviewer` scores against `skill-bars.md`. Loads **unslop**. Principles: Minimize Reader Load, Prove It Works.

## Files to change

| Kind | Now | After | Action |
|---|---|---|---|
| command | `plugins/pack-author/commands/pack-author.md` | same | edit: playbook routing |
| skill | `plugins/pack-author/skills/pack-author-md/SKILL.md` | same | restyle hub |
| skill | `pack-author-md/references/make.md` | `pack-author-md/playbooks/write-skill.md + write-subagent.md` | split |
| skill | `pack-author-md/references/review.md` | `pack-author-md/playbooks/check-skill.md + check-subagent.md` | split |
| skill | `pack-author-md/references/change.md` | `pack-author-md/playbooks/change.md` | move |
| skill | `pack-author-md/references/style.md` | `pack-author-md/references/skill-bars.md + subagent-bars.md` | rename and split; the "do not copy an external skill set" line becomes the copy rule |

Full list for all PBIs: [MIGRATION.md](MIGRATION.md#path-table).

## How to do it

1. Branch from main: `git switch -c pbi/ps01-pack-author-write-skill origin/main`.
2. Read [SHAPES.md](SHAPES.md) for every kind in the table above, and the upstream source linked in **External skills per step**.
3. Do each row of **Files to change** in order. For a move or rename use `git mv`; for a copy keep the `Source:` line and `## What we changed`.
4. Follow **Steps for Builder** below. Nothing outside the table and the steps.
5. Run the checks in **Done check**. All must pass before you open the PR.
6. Open one PR titled `PS01: pack-author-md hub and skill playbooks (write-skill first)`, body: link to this file, the table, the check output.

## Done check

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
for f in tests/fixtures/gh-aw/*.md; do gh aw compile --strict "$f"; done   # from GA04 on; before GA04 write "n/a, no fixture yet"
grep -rnP "[\x{2013}\x{2014}]" plugins docs || echo "no dashes"
```

Local: install `/tmp/agentpacks-marketplace` in Cursor or Claude Code (README "Install the generated tree") and run the commands this PBI touches once.

## Steps for Builder

1. Move skill half of `make.md` to `playbooks/write-skill.md` (agent half moves in PS02).
2. `review.md` to `playbooks/check-skill.md` with the new bars (Non-negotiables first, index lines resolve, 5-line rule, deps closed, copy rule).
3. `change.md` to `playbooks/change.md`.
4. `style.md` to `references/skill-bars.md`; Matt bits with MIT line in `plugins/pack-author/NOTICE.md`.
5. Rewrite `SKILL.md` as the hub.
6. Update `docs/ADD-SKILL.md`, `docs/PACK-AUTHOR.md`.
7. Fixture `tests/fixtures/pack-author/new-skill/` written by following write-skill, plus test.

## DoD

- Fixture skill passes validate 0/0 and check-skill PASS first try.
- No step lists left in `references/`.
- `/pack-author` still routes to `pack-author-md`.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture path, validate output, check-skill verdict.
- Test that fails when the hub loses an H2 or changes order.

## Risks and out of scope

- Hub over the 40-line router bar until PS03 adds the hub exception. Builder may land PS03 first.
- Out: agents (PS02), validator code (PS03).
