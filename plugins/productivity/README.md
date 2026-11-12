# productivity

A capability pack for a technical design developers can build. `/tech-design` charts a shared map in `docs/design/<name>/`, then resolves one gap per session. Ready writes `tickets.md` when every gap is closed.

The skill is `tech-design`. Claude, Cursor, and Codex run `/tech-design` (Codex: `$tech-design`). Copilot picker: `/productivity:tech-design`.

The person types the skill. The model does not start a design on its own.

Style inspired by Matt Pocock's wayfinder. [NOTICE](skills/tech-design/NOTICE.md).

## Start

`/tech-design new <name> [types]` charts the map.

1. Name the destination with `grilling` and `domain-modeling`. The destination is a technical design developers can build.
2. Grill again, breadth-first, and list the frontier. If there is no fog, stop and ask how to proceed.
3. Create `docs/design/<name>/` with `design.md` at status `draft`, `glossary.md`, `gaps.md`, and `log.md`. Record named types in Notes. The skills line lists only the skills this design needs.
4. Add the gaps you can phrase, then fill `blocks` in a second pass. Leave the rest in **Not yet specified**.
5. Start a research subagent for each research gap. Findings go in `research/<topic>.md`.
6. Stop. Charting leaves every non-research gap open.

A second `new` for a folder that already exists stops. Resume instead.

## Resume

`/tech-design <name>` loads the status line and the map sections in `design.md`, `gaps.md`, and the last `log.md` line.

If the status is `ready`, `committed`, or `abandoned`, say so and stop. Otherwise claim the first frontier gap. That row has status `open`, is unblocked, and has no owner. If the person named a gap, claim that one. Resolve that one gap, then stop.

`/tech-design <name> grill [gap]` is the same path. On a grilling gap, give advice first. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen. Then ask one question. Advice alone never closes a gap.

## Ready

`/tech-design <name> ready` stops while any gap is `open`, `research`, or `waiting-on-input`. It also stops while **Not yet specified** still lists in-scope work. When the gate passes, it sets status `ready`. `design.md` is the spec. `to-spec` checks that file. `to-tickets` writes `tickets.md` from it.

## Components

| Component | Name | Responsibility |
| --- | --- | --- |
| Skill | `tech-design` | The person types `/tech-design`. Arguments are new, resume, grill, and ready. |

Pins, loaded by exact Skill name: `domain-modeling`, `codebase-design`, `grilling`, `research`, `writing-for-agents`, `deslop`.

## Editing

Authored: `plugin.json`, `skills/`, and `external-skills.json`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client manifests.

Test on a feature branch without merging to `main`:
[ADD-SKILL.md: Test a skill locally](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-SKILL.md#test-a-skill-locally).
