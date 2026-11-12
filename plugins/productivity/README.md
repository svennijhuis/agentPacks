# productivity

A capability pack for a technical design developers can build. `/tech-design` charts a shared map in `docs/design/<yyyy-mm-dd>-<name>/`, finds every gap, and closes one gap per session. Ready writes `tickets.md` when a developer or an agent can build from the design without guessing.

The skill is `tech-design`. Claude, Cursor, and Codex run `/tech-design` (Codex: `$tech-design`). Copilot picker: `/productivity:tech-design`. There is no command file. The slash comes from the skill.

The person types the skill. The model does not start a design on its own.

This pack stands alone. It needs no other pack from this repository.

Style inspired by Matt Pocock's wayfinder. [NOTICE](skills/tech-design/NOTICE.md).

## Setup

`tech-design` calls six companion skills by name: `grilling`, `domain-modeling`, `research`, `codebase-design`, `writing-for-agents`, `deslop`. They are pinned in `external-skills.json` and ship with the published pack, so installing the pack installs them.

Check that your client lists all six next to `tech-design`. If one is missing, the skill says so once and does that step by its own rules. Nothing else needs to be configured.

## Start

`/tech-design new <name> [types]` charts the map. Types are `web`, `api`, `data`, and `integration`.

1. Name the destination. It is a technical design developers can build.
2. Pick the size: `small`, `medium`, or `big`. The skill proposes one and you confirm.
3. Grill breadth-first for the decisions that can be phrased.
4. Sweep 18 categories so no area is skipped. Each ends as a gap, a line of fog, or a reason it does not apply.
5. Create the folder. It always holds `design.md`, `gaps.md`, `glossary.md`, `log.md`, `research/`, `questions/`, and `input/`. `design.md` always has the same 14 sections.
6. Add the gaps. Any outside system gets its gaps at once: owner, access, credentials, environments, contract, limits, change notice, proof.
7. Start a research subagent for each research gap.
8. Stop.

A second `new` for a name that already exists stops. Resume instead.

## Resume

`/tech-design <name>` picks up from the files alone. It takes in new answers from `input/`, shows the board, flags overdue gaps, then claims the first frontier gap and resolves that one gap. `/tech-design <name> grill [gap]` is the same path for a gap you name.

A gap that waits on someone else is set to `waiting-on-input` with who, when asked, and when due. The question is written to `questions/<person>.md`, ready to paste into a chat or a mail. A gap may wait for days or weeks. The rest of the design goes on. When the answer comes, paste it or drop the file in `input/`.

On a grilling gap, the skill gives advice first: 2 to 3 options, the recommended one, why, and the risk. Then it asks one question. Advice alone never closes a gap.

## Status and help

| Type | Get |
| --- | --- |
| `/tech-design` | Every design with its size, status, dates, and open and waiting counts |
| `/tech-design help` | The card of all modes |
| `/tech-design <name> status` | The board, and what you need to do: gaps that need your answer, people to chase, what the agent can do alone |
| `/tech-design <name> close <gap>` | Close a gap with your own answer |
| `/tech-design <name> drop <gap>` | Move a gap out of scope, with a reason |

## Ready

`/tech-design <name> ready` walks a checklist and prints pass or fail per line. It stops while any gap is open, any section is blank, any requirement has no id or no test, any dependency field is empty, any required diagram is missing, or any vague word stands without a number. When every check passes, it sets status `ready` and writes `tickets.md`. `design.md` is the spec.

`/tech-design <name> ready anyway` goes ready with gaps still open. The skill shows each open gap with what a builder would have to guess. You accept each one by name. The design then lists them under Open at ready, and tickets that touch them are marked blocked. The skill never offers this path itself.

## Components

| Component | Name | Responsibility |
| --- | --- | --- |
| Skill | `tech-design` | The person types `/tech-design`. Modes are list, help, new, resume, grill, status, close, drop, and ready. |

Reference files under `skills/tech-design/references/`: `structure`, `sizes`, `gap-sweep`, `dependency-contract`, `research`, `resume`, `diagrams`, `help`, `ready-gate`.

## Editing

Authored: `plugin.json`, `skills/`, and `external-skills.json`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client manifests.

Test on a feature branch without merging to `main`:
[ADD-SKILL.md: Test a skill locally](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-SKILL.md#test-a-skill-locally).
