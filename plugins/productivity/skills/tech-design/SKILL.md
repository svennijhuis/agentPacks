---
name: tech-design
description: When the user types /tech-design to start, resume, grill, or ready a technical design.
license: MIT
disable-model-invocation: true
user-invocable: false
---

# Tech design

Start only after the person types the command. In one session, chart `docs/design/<name>/`, resolve one gap, or run the ready gate, and then stop. At chart, research gaps are the exception: start one subagent per research gap.

Load this skill with the Skill tool by exact name `tech-design`. Do not write `/tech-design` as prose to load it. See [NOTICE.md](NOTICE.md).

This session records decisions. It does not write the whole design, and it does not build the product. Notes may allow a little execution for this one design.

Name a gap by its question. Keep the id as a pointer on that question.

## What this session loads

Chart loads this file, [the map template](references/map-template.md), and [gap types](references/gap-types.md).

Resume and grill: Load `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. Then load [gap types](references/gap-types.md) for the claimed gap. Load [the advice block](references/advice-block.md) when the type is grilling.

Ready loads `gaps.md` and [the ready gate](references/ready-gate.md).

Leave `research/` unread until the claimed gap links a file there. If the folder is missing, stop and tell the person to run `new`. If status is `ready`, `committed`, or `abandoned`, say so and stop.

## Skill per step

Call a pin with the Skill tool by exact name. Name a skill marked (TD2+) at the step. Call it only after that pack ships.

Fill Notes with `<only the skills this design really needs, picked from Skill per step>`.

| Step | Skills | Use | Source | Output |
| --- | --- | --- | --- | --- |
| Chart: destination and frontier | `grilling` + `domain-modeling` | grilling: pin. domain-modeling: pin. | [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling), [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) | design.md map, gaps.md, glossary.md |
| Research per gap (first) | `how` (TD2+) on the code, `research` on the docs, read input/ | how: copy + adapt. research: pin. | [how](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how), [research](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research) | `research/<topic>.md` |
| Draft advice | `architect` (TD2+) + `principle-exhaust-the-design-space` (TD2+) for options, `codebase-design` when a seam is in play, `why` (TD2+) for the source, `blast-radius` (TD2+) for the risk line | architect: copy + adapt. principle-exhaust-the-design-space: copy + adapt. codebase-design: pin. why: copy + adapt. blast-radius: copy + adapt. | [architect](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/architect), [principle-exhaust-the-design-space](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-exhaust-the-design-space), [codebase-design](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design), [why](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why), [blast-radius](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius) | advice block in the gap |
| Grill (one question) | `grilling` + `domain-modeling` | grilling: pin. domain-modeling: pin. | [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling), [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) | answer in the gap, log line |
| No answer | `to-questionnaire` (TD2+) | to-questionnaire: copy + adapt. | [to-questionnaire](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/to-questionnaire) | question in input/, waiting-on-input |
| State diagram | `statechart` (TD2+) | statechart: copy + adapt. | [statechart](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart) | mermaid state in design.md |
| Every edit of design.md | `writing-for-agents`, plus `deslop` on openapi.yaml or code | writing-for-agents: pin. deslop: pin. | [writing-for-agents](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/writing-for-agents), [deslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/cursor-team-kit/skills/deslop) | (none) |
| Ready | `interrogate` (TD2+) over the whole design, then `technical-writing` (TD2+), `unslop` (TD2+) and the no-op pass, then `to-spec` (TD2+) and `to-tickets` (TD2+) | interrogate: copy + adapt. technical-writing: copy + adapt. unslop: copy + adapt. to-spec: copy + adapt. to-tickets: copy + adapt. | [interrogate](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate), [technical-writing](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing), [unslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop), [to-spec](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/to-spec), [to-tickets](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/to-tickets) | tickets.md, wayfinder-brief.md |

## Chart

The person types `/tech-design new <name> [types]`.

1. Call the Skill tool with `grilling` and `domain-modeling`. Write Destination as the technical design developers can build. List the types the person named. Add a glossary row for each term you had to define. Done when Destination and those types are written.
2. Call the Skill tool with `grilling` and `domain-modeling` again. Cross the whole design before you go deep on one question. Fog is an in-scope question you cannot phrase yet. Done when those questions are listed. If there is no fog, stop and ask the person how to proceed.
3. Create the folder from [the map template](references/map-template.md). Call the Skill tool with `writing-for-agents` on `design.md`. Call the Skill tool with `deslop` when you add `openapi.yaml` or a code snippet. Name `statechart` (TD2+) when the design has statuses, and put the mermaid state diagram next to that text. Done when `design.md`, `glossary.md`, `gaps.md`, and `log.md` exist and status is `draft`.
4. Add the rows you can phrase now. Then fill `blocks` with real ids. Leave a question you cannot phrase in Not yet specified. Done when every `blocks` id exists. If the person named one sharp question, set status to `discussion`. Otherwise leave `draft`.
5. For each research gap, start a subagent with a fresh context. The subagent sets `owner`, calls the Skill tool with `how` (TD2+) on the code first, then `research` on the docs, and reads `input/`. It writes `research/<topic>.md` in the design folder and sets that row to `decided`. Do not open a pull request. Done when each research row links its file.
6. Add one `log.md` line for the chart session. Stop. You close no grilling, prototype, or task gap in this session.

A second `new` for an existing folder stops. Tell the person to resume.

## Resume or grill

The person types `/tech-design <name>` or `/tech-design <name> grill [gap]`.

1. Load the files named above.
2. Claim the named gap, or the first frontier row. The frontier is status `open`, owner empty, and every `blocks` id `decided` or `accepted-risk`. An empty `blocks` cell is unblocked. Done when `owner` is set, before any other work on the row.
3. Read [gap types](references/gap-types.md) and follow that type. The question does not name the type. On a grilling gap, do the research row first, draft [the advice block](references/advice-block.md), then call the Skill tool with `grilling` and `domain-modeling` and ask one question. `domain-modeling` updates `glossary.md` when a term is in play. You do not answer that question.
4. Write the answer on the row, one `log.md` line (date, the question, the id, what changed), and one gist under Decisions so far. Call the Skill tool with `writing-for-agents` on any `design.md` edit. Call the Skill tool with `deslop` when `openapi.yaml` or a code snippet changes. Name `statechart` (TD2+) when the design has statuses. Set `decided`, or `accepted-risk` only when the human accepts the risk you named. If there is no answer, call the Skill tool with `to-questionnaire` (TD2+), write the question in `input/`, and set `waiting-on-input` with an owner and a due date. Record that date under Risks. Advice alone never closes a gap.
5. Add rows the answer made phraseable, then wire `blocks`. Remove that text from Not yet specified. Delete a gap that sits past the destination and leave one Out of scope line. That line is not a decision. Update rows the decision invalidates. Add a file only when [the map template](references/map-template.md) lists it. Call the Skill tool with `writing-for-agents` on any `design.md` edit in this step, and with `deslop` when `openapi.yaml` or a code snippet changes.
6. Stop. Other sessions may edit the folder. A row with an owner is claimed.

## Ready

The person types `/tech-design <name> ready`.

Follow [the ready gate](references/ready-gate.md). When the gate passes, call the Skill tool with `interrogate` (TD2+) over the whole design, then `technical-writing` (TD2+), `unslop` (TD2+) and the no-op pass, then `to-spec` (TD2+) and `to-tickets` (TD2+).

## Sources

Upstream wayfinder, then each pin in `external-skills.json`.

- [wayfinder](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/wayfinder) (copy + adapt)
- [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) (pin)
- [codebase-design](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design) (pin)
- [research](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research) (pin)
- [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling) (pin)
- [writing-for-agents](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/writing-for-agents) (pin)
- [deslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/cursor-team-kit/skills/deslop) (pin)

Ready resolves no gap.

Good: one gap, a recommended option with a source, the human's answer recorded, then stop.
Bad: a full design in one session, a gap closed from advice, a dependency called to discover its contract.
