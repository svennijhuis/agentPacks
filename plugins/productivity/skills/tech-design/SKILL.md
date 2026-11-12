---
name: tech-design
description: When the user types /tech-design to start, resume, grill, or ready a technical design.
license: MIT
disable-model-invocation: true
---

A technical design developers can build is too big for one session. This skill charts that design as a shared map at `docs/design/<name>/`. Each session resolves one gap, until a developer can build from the design. See [NOTICE.md](NOTICE.md).

Name the destination first. The destination is that design. It fixes the scope of every gap.

## Plan, don't do

Each gap resolves a decision. The map is done when every gap is closed and a developer can build. Builders start after the map is done. Notes may say this effort also does the work. Otherwise stop at the decision.

## Refer by name

A gap's name is its question. Use the question when you talk to the human and in Decisions so far. Put the id in the link under the question.

## The map

`design.md` is the map. `gaps.md` holds the gaps. Each closed gap is one gist line on the map and a pointer to its row. The answer stays on the row.

The first line under the title is the status. The words are `draft`, `discussion`, `ready`, `committed`, and `abandoned`. `draft` is the new folder. `discussion` means gaps are still open. `ready` means the gate passed. `committed` means builders follow this design. `abandoned` means the work stopped.

Under the map, write the design a developer builds from. Link files and symbols. Do not paste them. When a type is api, write the contract in `design.md`: each endpoint, its auth level, and its errors, with a link to the real code. Settled terms go in `glossary.md` in this folder. A lifecycle with statuses follows the State diagram row. Draw it as a mermaid diagram next to the text it explains.

### The map body

Load this once per session. Open gaps stay in `gaps.md`.

```markdown
## Destination

<one or two lines. The technical design this effort ends on. Read this before you pick a gap.>

## Notes

Domain: <one line>
Skills: <only the skills this design needs>
Preferences: <preferences for this design>

## Decisions so far

- <gap question>, pointer to its row in gaps.md: <one-line gist>

## Not yet specified

<one line per in-scope question you cannot phrase yet>

## Out of scope

<one line per question past the destination, and why>
```

### Gaps

One row in `gaps.md` is one gap. Columns are id, question, type, status, owner, due, waiting-on note, source or answer, and blocks. The question fits one session. Its last line says when it is done.

The type is `research`, `prototype`, `grilling`, or `task`. Read the type column.

**Claim** a gap by setting `owner` to the person driving the map before any work. An open row with an empty owner is unclaimed.

`blocks` lists the ids that must be `decided` or `accepted-risk` first. An empty `blocks` cell is unblocked. The **frontier** is status `open`, unblocked, and unclaimed.

Write the answer on the row when the gap closes. Link assets from the row.

While you study a row, set status `research`. No answer sets `waiting-on-input`, with an owner and a due date, and records that date in `design.md`. The human's answer sets `decided`. Set `accepted-risk` only when the human accepts the risk you named. Advice alone never closes a gap.

When you save a note in `input/`, move `waiting-on-input` back to `research`. If that note answers the question, set `decided`.

Leave a question you cannot phrase under Not yet specified.

## Gap types

**HITL** means the human speaks for themselves and you work with them. **AFK** means you drive the gap alone. A HITL gap closes only through that exchange.

**Research** (AFK). Use when the fact sits outside this working tree. A subagent with a fresh context claims the gap, follows the Research row, and writes `research/<topic>.md`. The row links that file.

**Prototype** (HITL). Use when the question is how it should look or how it should behave. Make a rough artifact and link it from the row. The human's reaction is the answer. This pack does not pin a prototype skill.

**Grilling** (HITL). The default. Give advice first. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen. Follow the Draft advice row. Then call `grilling` and `domain-modeling`. Ask this gap's question, and stop. Leave every other gap for a later session. The question includes the recommended option and why. The human's answer closes the gap.

**Task** (HITL or AFK). Work that has to finish before a decision, when there is nothing to decide, prototype, or research. Drive it when you can. Otherwise give the human a checklist. The answer records what was done and the facts later gaps need. Make no live call to a dependency. Proof is a spec or a written answer in `input/`. A chat line is not proof.

## Fog of war

Not yet specified is the **fog of war**. List in-scope questions there when they are still too vague to be gaps. Resolving a gap lets you phrase the next questions. Write each question you can now phrase as a gap, and delete that line from Not yet specified.

A question you can phrase now is a gap, even when it is blocked. A question you cannot phrase stays one line in the fog. One line may become several gaps, or none.

Not yet specified leaves out Decisions so far, live gaps, and Out of scope.

## Out of scope

Work past the destination is out of scope. Record it under Out of scope. Start a new design if the destination changes and that work belongs in it.

When a gap sits past the destination, drop the row. Add one line under Out of scope with the question and why. Decisions so far records closed gaps.

## Skill per step

Call a pinned skill with the Skill tool by the exact name in the Skills column. A name marked (TD2+) is not pinned yet. Name it at that step. Call it once a later pack pins it. Until then, write that step yourself. On each edit of `design.md`, call `writing-for-agents`. When a code snippet changes, call `deslop`. At ready, `to-spec` (TD2+) checks that `design.md` meets the spec bar. `to-tickets` (TD2+) writes `tickets.md` from `design.md`.

| Step | Skills | Source |
| --- | --- | --- |
| Chart the destination and the frontier | `grilling`, `domain-modeling` | [grilling], [domain-modeling] |
| Research a gap | `how` (TD2+), `research` | (TD2+), [research] |
| Draft advice | `architect` (TD2+), `principle-exhaust-the-design-space` (TD2+), `codebase-design`, `why` (TD2+), `blast-radius` (TD2+) | (TD2+), [codebase-design] |
| Ask the grill question | `grilling`, `domain-modeling` | [grilling], [domain-modeling] |
| No answer | `to-questionnaire` (TD2+) | (TD2+) |
| State diagram | `statechart` (TD2+) | (TD2+) |
| Edit design.md | `writing-for-agents`, `deslop` | [writing-for-agents], [deslop] |
| Ready | `interrogate` (TD2+), `technical-writing` (TD2+), `unslop` (TD2+), `to-spec` (TD2+), `to-tickets` (TD2+) | (TD2+) |

[grilling]: https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling
[domain-modeling]: https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling
[research]: https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research
[codebase-design]: https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design
[writing-for-agents]: https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/writing-for-agents
[deslop]: https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/cursor-team-kit/skills/deslop

## Invocation

The person types `/tech-design`. Chart, then work. Ready runs once the design is specified. Resolve one gap per session, then stop. Research gaps may run together during chart. Chart and ready close no gap. A research subagent may close its own research gap.

### Chart the map

The person runs `/tech-design new <name> [types]`.

1. **Name the destination.** Call `grilling` and `domain-modeling`. The destination is a technical design developers can build. Record any types the person named under Notes.
2. **Map the frontier.** Call `grilling` and `domain-modeling` again, breadth-first. List the decisions you can phrase and the gaps you can work now. If you find no fog and one session can finish the design, stop and ask how to proceed.
3. **Create the folder** `docs/design/<name>/` with `design.md`, `glossary.md`, `gaps.md`, and `log.md`. Set status `draft`. Fill the destination and notes. Leave Decisions so far empty. Write the fog in Not yet specified. When a named type is api, write the contract in `design.md`: each endpoint, its auth level, and its errors, with a link to the real code. Include an endpoint only when a source already lists it. Call `writing-for-agents` on `design.md`. Leave `research/` and `input/` until the first note. Leave `tickets.md` for ready.
4. **Add the gaps you can phrase.** Each row starts `open`, with an empty owner and a type. Fill `blocks` in a second pass, using real ids. If the person already named one sharp question, set the document status to `discussion`.
5. **Start the research subagents.** For each research gap, start a subagent with a fresh context. It claims the gap and follows the Research row. The row links `research/<topic>.md`.
6. **Stop.** Add one `log.md` line. Charting is done when the folder exists and every non-research gap is still open.

A second `new` for a folder that already exists stops. Tell them to resume.

### Work through the map

The person runs `/tech-design <name>`. Grill is `/tech-design <name> grill`, and the gap is optional. With no gap, pick the next frontier gap. Grill uses this same path.

1. **Load the map.** Load the status line and the map sections in `design.md`, `gaps.md`, and the last `log.md` line. Skip the research notes. If the folder is missing, stop and tell them to run `new`. If the status is `ready`, `committed`, or `abandoned`, say so and stop.
2. **Choose the gap and claim it.** If the user named one, use it. Otherwise take the first frontier gap. Set `owner` before any work.
3. **Resolve it by type.** Read the type column. Read a related row when you need it. Call the skills Notes lists. On a grilling gap, advice comes before the one question.
4. **Record the resolution.** Write the answer on the row. Append one `log.md` line with the date, the question, the id, and what changed. Append a gist to Decisions so far. Set `decided`, or `accepted-risk` when the human accepts the risk you named. With no answer, follow the No answer row, then set `waiting-on-input`, an owner, and a due date. Advice alone never closes a gap. If `design.md` changed, call `writing-for-agents`. If a code snippet changed, call `deslop`.
5. **Update the map.** Add new gaps, then fill `blocks`. Delete fog you can now phrase. Drop a gap that sits past the destination and record it under Out of scope. Update rows the decision invalidates. Stop. The session is done when that one gap is claimed and either closed or `waiting-on-input`.

Other sessions may edit this folder at the same time.

### Ready

The person runs `/tech-design <name> ready`.

1. Read `gaps.md`. If any row is `open`, `research`, or `waiting-on-input`, name those questions and stop. Leave the status unchanged.
2. If no row is `decided` or `accepted-risk`, stop. If Not yet specified still lists in-scope work, stop.
3. Set status `ready`. Follow the Ready row. `design.md` is the spec. `to-spec` (TD2+) checks that `design.md` meets the spec bar. `to-tickets` (TD2+) writes `tickets.md` from `design.md`. Ready writes nothing else. Each ticket follows [the ready gate](references/ready-gate.md). Append one `log.md` line. Stop. Ready is done when `tickets.md` exists and every gap status is unchanged.
