---
name: tech-design
description: When the user types /tech-design to start, resume, grill, or ready a technical design.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# Tech design

The person typed the command. Load this skill by the exact name `tech-design`. Style credit is in [NOTICE.md](NOTICE.md).

Chart `docs/design/<name>/`, close one gap, or run the ready gate. Then stop. Write the decision. Do not build the product. Research gaps are the exception: chart may run those together. Advice alone never closes a gap. The human or the dependency owner records the answer.

`design.md` is an index. The decision lives on the gap row in `gaps.md`. The frontier is the open, unblocked, unclaimed rows. Name a gap by its question, and keep the id in the row.

## What this session loads

Chart loads [the map template](references/map-template.md) and [gap types](references/gap-types.md). It does not load research notes.

Work loads the one claimed row after the map. Open [the advice block](references/advice-block.md) only while drafting advice. Open one `research/` file only when that row points at it.

Ready loads [the ready gate](references/ready-gate.md) and `gaps.md`. Run the writing skills only after the gate passes.

## Skill per step

Call a pin with the Skill tool by exact name. Name a skill marked (TD2+) in that order, and call it once the copy is in the pack. `pin` means the skill is in `external-skills.json`. `copy + adapt` means a later copy from ADR 0003.

| Step | Skills | Use | Source | Output |
| --- | --- | --- | --- | --- |
| Chart: destination and frontier | `grilling` + `domain-modeling` | pin | [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling), [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) | design.md map, gaps.md, glossary.md |
| Research per gap (first) | `how` (TD2+) on the code, `research` on the docs, read `input/` | how: copy + adapt. research: pin | [how](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how), [research](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research) | research/<topic>.md |
| Draft advice | `architect` (TD2+) + `principle-exhaust-the-design-space` (TD2+) for options, `codebase-design` when a seam is in play, `why` (TD2+) for the source, `blast-radius` (TD2+) for the risk line | architect, principle-exhaust-the-design-space, why, blast-radius: copy + adapt. codebase-design: pin | [architect](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/architect), [principle-exhaust-the-design-space](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-exhaust-the-design-space), [codebase-design](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design), [why](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why), [blast-radius](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius) | advice block in the gap |
| Grill (one question) | `grilling` + `domain-modeling` | pin | [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling), [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) | answer in the gap, log line |
| No answer | `to-questionnaire` (TD2+) | copy + adapt | [to-questionnaire](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/to-questionnaire) | question in input/, waiting-on-input |
| State diagram | `statechart` (TD2+) | copy + adapt | [statechart](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart) | mermaid state in design.md |
| Every edit of design.md | `writing-for-agents`, plus `deslop` on openapi.yaml or code | pin | [writing-for-agents](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/writing-for-agents), [deslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/cursor-team-kit/skills/deslop) | (none) |
| Ready | `interrogate` (TD2+) over the whole design, then `technical-writing` (TD2+), `unslop` (TD2+) and the no-op pass, then `to-spec` (TD2+) and `to-tickets` (TD2+) | copy + adapt | [interrogate](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate), [technical-writing](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing), [unslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop), [to-spec](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/to-spec), [to-tickets](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/to-tickets) | tickets.md, wayfinder-brief.md |

## Chart the folder

Command: `/tech-design new <name> [types]`.

Done when `design.md`, `glossary.md`, `gaps.md`, and `log.md` exist, `blocks` is wired, the log has one chart line, and no grilling, prototype, or task row was closed.

1. Name the destination. Call `grilling` and `domain-modeling`. `domain-modeling` writes terms into `glossary.md`. Done when Destination is one or two lines and names a technical design developers can build. Record named types. Do not invent a checklist.
2. Map the frontier. Call `grilling` and `domain-modeling` again, breadth-first. Done when the open decisions are listed. If there is no fog of war, stop and ask how to proceed.
3. Create the folder from [the map template](references/map-template.md). Status starts at `draft`. The Notes skills line is `<only the skills this design really needs, picked from Skill per step>`. Call `writing-for-agents` on `design.md`. Call `deslop` when `openapi.yaml` or a code snippet changes.
4. Add rows you can phrase now. Types are in [gap types](references/gap-types.md). Wire `blocks` in a second pass. Leave the rest under Not yet specified. A sharp question the person already named sets status `discussion`.
5. For each research gap, start a subagent that sets `owner`. It names `how` (TD2+) on the code, calls `research` on the docs, and reads `input/`. It writes `research/<topic>.md`. When the fact is in that file, set the row `decided`.
6. Append one `log.md` line. Stop.

A second `new` on an existing folder stops. Tell them to resume.

## Work one gap

Command: `/tech-design <name>`, or `/tech-design <name> grill [gap]`.

Done when that row is `decided`, `accepted-risk`, or `waiting-on-input`, the log has one new line, and you asked at most one question.

1. Load `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. Missing folder: stop and tell them to run `new`. Status `ready`, `committed`, or `abandoned`: say so and stop.
2. Claim the named gap, or the first frontier row. Set `owner` before any other edit.
3. Follow the type in [gap types](references/gap-types.md). On a grilling gap, fill [the advice block](references/advice-block.md) before the question.
4. Write the answer on the row. Append one gist under Decisions so far when the row closes. No answer: `to-questionnaire` (TD2+) writes the question in `input/`, then set `waiting-on-input` with an owner and a due date. Call `writing-for-agents` on every `design.md` edit. Call `deslop` when `openapi.yaml` or a code snippet changes. A state diagram calls `statechart` (TD2+). Advice alone never closes a gap.
5. Add gaps the answer made specifiable. Clear each graduated patch from Not yet specified. Drop a row that sits past the destination and leave one Out of scope line. Update rows the decision invalidates.

## Run ready

Command: `/tech-design <name> ready`.

Done when you listed the blocking questions and stopped, or status is `ready` and both `tickets.md` and `wayfinder-brief.md` exist. This session closes nothing.

Follow [the ready gate](references/ready-gate.md).
