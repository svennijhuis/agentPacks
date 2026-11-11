---
name: tech-design
description: When the user types /tech-design to start, resume, grill, or ready a technical design.
license: MIT
disable-model-invocation: true
user-invocable: false
---

# Tech design

The person types the command. Chart a technical design in `docs/design/<name>/`, close one gap, or run the ready gate. Then stop. The design is what developers build. Almost every gap is closed before handoff, so a later pass can turn it into tickets that take story points.

Load this skill with the Skill tool by exact name `tech-design`.

Write decisions, not the product. One session charts the folder, resolves one gap, or runs ready. It does not write the whole design, and it does not build the product. Research gaps are the exception at chart time: those subagents may run together. Stop when the next pull is to start building. That pull means the map is ready to hand off. Notes may carry execution for one effort. Otherwise hand off.

## Refer by name

In anything a person reads, name a gap by its question. Put the id inside that name. A list of bare ids is hard to scan.

## The map

The map is the folder `docs/design/<name>/`. `design.md` is the index. A decision lives in one gap row. The map keeps a one-line gist and a pointer to that row.

There is no issue tracker. Blocking, the claim, and the frontier live in `gaps.md`.

| File | Role |
| --- | --- |
| `design.md` | Status line, then the map block, then the fixed sections. Links the other files. Does not copy them |
| `glossary.md` | Domain language |
| `gaps.md` | One row per gap |
| `research/<topic>.md` | Findings for one topic, written when research runs |
| `input/<date>.md` | Input saved that day |
| `log.md` | One line per session |
| `openapi.yaml` | Only when the api type is in use. OpenAPI 3.1. Do not invent endpoints |
| `tickets.md` | Written by ready, after the gate passes |
| `wayfinder-brief.md` | Written by ready, after the gate passes |

The first line under the title is the document status: `draft`, `discussion`, `ready`, `committed`, or `abandoned`.

- `draft`: the new folder.
- `discussion`: work while gaps remain. Resume and grill repeat here.
- `ready`: the gap gate passed.
- `committed`: builders follow this design.
- `abandoned`: the work stopped.

The map block comes next. Then the fixed sections, in this order: user outcome, context and scope, goals, non-goals, glossary, design, dependencies, alternatives, cross-cutting concerns, type checklists, advice, risks, acceptance criteria, sign-off. User outcome is first. The design section holds an overview, a small diagram, boundaries and contracts, a data model, and flows as mermaid. Put each diagram next to the text it explains. Alternatives include do nothing. Cross-cutting concerns are security, privacy, observability, rollout, and testing. The advice section states the recommended choice, why, and what changes if a gap flips. Each waiting gap has a deadline in `design.md`. Link real files, symbols, and `openapi.yaml`. Do not paste them.

Record only types the person named. Do not invent a type checklist. Type playbooks arrive in TD2+. When api is named, keep `openapi.yaml`. An empty contract field is a gap.

On every edit of `design.md`, call the Skill tool with `writing-for-agents`. When `openapi.yaml` or a code snippet changes, call the Skill tool with `deslop`. When the design needs a state diagram, call the Skill tool with `statechart` (TD2+). The no-op pass deletes every line that does not change what the reader does.

### Map block

Load this block once per session. Open gaps are rows in `gaps.md`, not lines in this block.

```markdown
## Destination

<what reaching the end of this map looks like: the technical design this effort is finding its way to. One or two lines. Every session reads this before it chooses a gap.>

## Notes

<domain. <only the skills this design really needs, picked from Skill per step>. Standing preferences for this effort.>

## Decisions so far

<!-- one line per closed gap: the question, a pointer to its row, a one-line gist. The decision stays in the gap. -->

- <gap question> (pointer to its row in gaps.md): <one-line gist of the answer>

## Not yet specified

<!-- in-scope questions you cannot phrase yet. See Fog of war. -->

## Out of scope

<!-- work beyond the destination. Closed. It does not graduate. -->
```

### Gaps

Each gap is a row in `gaps.md`. Columns: id, question, type, status, owner, due, waiting-on note, source or answer, blocks. The question is the decision this gap resolves. End it with a hard done-when. The question never states the type. Read the type column.

A session claims a gap by setting `owner` before any work. An open gap with an empty owner is unclaimed. Concurrent sessions skip a claimed row.

`blocks` lists the ids that must be `decided` or `accepted-risk` before this row. An empty `blocks` cell is unblocked. The frontier is the rows whose status is `open`, that are unblocked, and that have no owner.

The answer is recorded on resolution, not in the question. Link assets from the row. Do not paste them in.

Statuses: `open`, `research`, `waiting-on-input`, `decided`, `accepted-risk`.

This session sets `research` while it studies the gap. No answer sets `waiting-on-input`, with an owner and a due date. Call the Skill tool with `to-questionnaire` (TD2+) and save the question under `input/`. The human or the dependency owner sets `decided` by an answer you save. They set `accepted-risk` only when they accept the risk you named. Advice alone never closes a gap.

Saved input moves `waiting-on-input` back to `research`, or to `decided` when that input answers the question.

Leave an unphrased in-scope question in **Not yet specified**.

## Gap types

A gap is HITL or AFK. HITL means a human speaks for themselves. AFK means the agent drives it. A HITL gap closes only through that live exchange. The agent never answers its own grill question.

- **Research** (AFK). A fact the decision waits on, from docs, a third-party API, or a local source outside this working directory. A subagent calls the Skill tool with `how` (TD2+) on the code, then `research` on the docs, and reads `input/`. Findings go in `research/<topic>.md`.
- **Prototype** (HITL). A cheap artifact the human can react to: an outline, a rough take, a stub, or a short piece of UI or logic. Link it from the row. The human's reaction is the answer. This pack does not pin a prototype skill.
- **Grilling** (HITL). The default. Call the Skill tool with `grilling` and `domain-modeling`. Ask one question, the gap's question, then stop. The question includes the recommended option and why. When a term is in play, update `glossary.md`. Before the question, draft advice. Call the Skill tool with `architect` (TD2+) and `principle-exhaust-the-design-space` (TD2+) for the options, `codebase-design` when a seam is in play, `why` (TD2+) for the source, and `blast-radius` (TD2+) for the risk line. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen.
- **Task** (HITL or AFK). Work that must happen before a decision, and that does not itself decide the destination. The agent drives it where it can. Otherwise hand the human a precise checklist. The answer records what was done and the facts later gaps need. Make no real dependency call. Proof is the owner's spec, or their written answer, saved in `input/`. Chat text is not proof. An empty proof field stays a blocking gap. The dependency-contract playbook arrives in TD2+.

## Fog of war

Leave the map incomplete on purpose. Fog of war is the in-scope work you can see coming and cannot phrase yet, because it hangs on an open question. Resolving a gap clears the fog ahead of it. Graduate what you can now phrase into new gaps, one patch at a time, until no gaps remain.

Write that dim view under **Not yet specified**. The test is whether you can phrase the question now, not whether you can answer it.

- Phrase it now, even if it is blocked: add a gap row.
- You cannot phrase it yet: leave it in the fog. Do not pre-slice it. One patch may become several gaps, or none.

**Not yet specified** excludes decisions already made, live gaps, and work that is out of scope.

## Out of scope

Fog gathers toward the destination. Work past that destination is out of scope. It is not fog, and it does not graduate. It returns only if the destination is redrawn, as a new effort.

Ruling a live gap out of scope is a scope cut, not a decision on the route. Drop the row so it leaves the frontier. Add one line under **Out of scope**: the gist, why it is out, and the question. Leave it out of **Decisions so far**. The non-goals section points at that line and does not restate it.

## Skill per step

Call the Skill tool by the name in this table. `pin` means the skill is listed in `external-skills.json`. `copy + adapt` means the copy arrives in TD2+ per ADR 0003. Call a (TD2+) skill once that copy is in this pack. Until then, do the rest of the step and do not invent the missing skill.

| Step | Skills | Use | Source | Output |
| --- | --- | --- | --- | --- |
| Chart: destination and frontier | grilling, domain-modeling | pin | [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling), [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) | design.md map, gaps.md, glossary.md |
| Research per gap (first) | how (TD2+) on the code, research on the docs, read input/ | copy + adapt for how, pin for research | [how](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how), [research](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/research) | `research/<topic>.md` |
| Draft advice | architect (TD2+) and principle-exhaust-the-design-space (TD2+) for options, codebase-design when a seam is in play, why (TD2+) for the source, blast-radius (TD2+) for the risk line | copy + adapt for architect, principle-exhaust-the-design-space, why, and blast-radius. pin for codebase-design | [architect](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/architect), [principle-exhaust-the-design-space](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-exhaust-the-design-space), [codebase-design](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/codebase-design), [why](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why), [blast-radius](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius) | advice block in the gap |
| Grill (one question) | grilling, domain-modeling | pin | [grilling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/grilling), [domain-modeling](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/domain-modeling) | answer in the gap, log line |
| No answer | to-questionnaire (TD2+) | copy + adapt | [to-questionnaire](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/to-questionnaire) | question in input/, waiting-on-input |
| State diagram | statechart (TD2+) | copy + adapt | [statechart](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/statechart) | mermaid state in design.md |
| Every edit of design.md | writing-for-agents, plus deslop on openapi.yaml or code | pin | [writing-for-agents](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/productivity/writing-for-agents), [deslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/cursor-team-kit/skills/deslop) | (none) |
| Ready | interrogate (TD2+) over the whole design, then technical-writing (TD2+), unslop (TD2+) and the no-op pass, then to-spec (TD2+) and to-tickets (TD2+) | copy + adapt for interrogate, technical-writing, unslop, to-spec, and to-tickets. The no-op pass is this skill | [interrogate](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate), [technical-writing](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing), [unslop](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop), [to-spec](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/to-spec), [to-tickets](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d/skills/engineering/to-tickets) | tickets.md, wayfinder-brief.md |

## Modes

Chart and work are the two modes. Ready is the gate after the way is clear. Resolve at most one gap per session. Research gaps are the exception. Charting and ready resolve nothing by hand.

### Chart the map

The person runs `/tech-design new <name> [types]`.

1. Name the destination. Call the Skill tool with `grilling` and `domain-modeling`. Pin the technical design developers will build. The destination fixes the scope, so settle it first. Record any types the person named. Update `glossary.md` when a term is in play.
2. Grill again, breadth-first. Cover the whole space before you go deep on one thread. Name the open decisions and the first steps that are takeable now. If no fog shows up, and the whole journey fits one session, a design map is not needed. Stop and ask how to proceed.
3. Create `docs/design/<name>/` with `design.md` (status `draft`, Destination and Notes filled, Decisions so far empty, fog sketched under **Not yet specified**), `glossary.md`, `gaps.md`, and `log.md`. Add `openapi.yaml` only when api is named. Leave `research/` and `input/` until there is a note to save. Leave `tickets.md` and `wayfinder-brief.md` for ready.
4. Add the gaps you can phrase now as rows (status `open`, owner empty, type set). Then wire `blocks` in a second pass, after the rows have ids. Use real ids in that pass. What you cannot phrase yet stays under **Not yet specified**. If the person already named one sharp question, that row is `open` and document status becomes `discussion`. Otherwise stay at `draft`.
5. For each research gap, start a subagent with a fresh context window. It claims the row, calls the Skill tool with `how` (TD2+) on the code, then `research`, and writes `research/<topic>.md`. The row points at that file. Open no pull request.
6. Stop. Charting resolves nothing by hand. Add one `log.md` line for the chart session.

A second `new` for a folder that already exists stops. Tell them to resume.

### Work through the map

The person runs `/tech-design <name>` (resume) or `/tech-design <name> grill [gap]`. A gap is optional. With none named, pick the next frontier gap. Grill is this same path for the named gap, or for the first frontier gap if none was named.

1. Load `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. Do not load every research note. Missing folder: stop. Tell them to run `new`. Status `ready`, `committed`, or `abandoned`: say so and stop.
2. If the user named a gap, use it. Otherwise take the first frontier gap in order. Claim it: set `owner` before any work.
3. Resolve it by the type column. See [Gap types](#gap-types). Read the type, not only the question. Read a related row when you need it. Call the Skill tool for the skills the Notes block names. If in doubt, call the Skill tool with `grilling` and `domain-modeling`. On a grilling gap, draft advice before the one question.
4. Write the answer on the gap row. Append one `log.md` line: date, the question, the id beside that name, and what changed. Append a gist line under **Decisions so far**. Status `decided`, or `accepted-risk` only when the human accepts the risk you named. No answer: call the Skill tool with `to-questionnaire` (TD2+), set `waiting-on-input`, an owner, and a due date, save the question under `input/`, and record the deadline in `design.md`. Advice alone never closes a gap.
5. Add gaps the answer made phraseable, then wire `blocks`. Clear each graduated patch from **Not yet specified**. If a gap sits past the destination, rule it out of scope. If the decision invalidates other rows, update those rows.

Other sessions may edit unblocked gaps at the same time.

### Ready

The person runs `/tech-design <name> ready`.

1. Read `gaps.md`. If any row is `open`, `research`, or `waiting-on-input`, list those rows by question and stop. Do not set `ready`.
2. `decided` and `accepted-risk` do not block the gate. A folder with no `decided` or `accepted-risk` row is not ready. Empty sections are not ready.
3. When the gate passes, call the Skill tool with `interrogate` (TD2+) over the whole design, then `technical-writing` (TD2+), `unslop` (TD2+), and the no-op pass, then `to-spec` (TD2+) and `to-tickets` (TD2+). Set status `ready`. Write `tickets.md`: buildable slices whose blockers are sized for story points. Write `wayfinder-brief.md`: destination, decided gaps by question, accepted risks, and links to `design.md` and to `openapi.yaml` when it exists.
4. Add one `log.md` line. Stop. Ready resolves nothing.

One gap, a recommended option with a source, the human's answer recorded, then stop.
