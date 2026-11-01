# PS01: pack-author-md hub and skill playbooks (write-skill first)
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B0
- Depends on: none
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

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
6. Reuse choice per upstream skill: pin, copy + adapt, weave or rewrite, with one reason line. Every copy starts with `Source: <upstream link at 40-hex SHA>, MIT, see NOTICE.` under the H1 and ends with `## What we changed` (one line per change). Scripts and assets stay byte-equal unless listed there.
7. unslop pass, no-comments pass, `validate`, then check-skill.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| hub style | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |
| write-skill | [upstream playbook authoring-a-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/authoring-a-skill.md) | copy + adapt | `pack-author-md/playbooks/write-skill.md` | Calls Cursor built-in create-skill. We replace it with our make steps and validate. Keeps our standards and gates (PS09). |
| step 7 | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pin | external-skills.json | Every prose output. |
| step 7 | [no-comments](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/no-comments/SKILL.md) | copy + adapt | `plugins/squad/skills/no-comments/` | Spawns the `comment-sicko` plugin agent, which we cannot pin. We point it to `squad-simplifier`. |
| principles | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |

## Subagents and what they load

- `pack-author-reviewer` scores against `skill-bars.md`. Loads **unslop**. Principles: Minimize Reader Load, Prove It Works.

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
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Fixture path, validate output, check-skill verdict.
- Test that fails when the hub loses an H2 or changes order.

## Risks and out of scope

- Hub over the 40-line router bar until PS03 adds the hub exception. Builder may land PS03 first.
- Out: agents (PS02), validator code (PS03).
