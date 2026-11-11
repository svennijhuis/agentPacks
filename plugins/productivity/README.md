# productivity

A capability pack for a technical design developers can build. `/tech-design` charts a shared map, then closes gaps one at a time, until a later wayfinder pass can turn the design into tickets that take story points.

Claude, Cursor, and Codex run `/tech-design` (Codex: `$tech-design`). Copilot picker: `/productivity:tech-design`. The skill is `tech-design`.

The person types the command. The model does not start a design on its own. One session charts the folder, takes one gap, or runs the ready gate, then stops. Research gaps are the exception: chart may fire those subagents together.

Style inspired by Matt Pocock's wayfinder. [NOTICE](skills/tech-design/NOTICE.md).

## Which external skill when

Same table as [Skill per step](skills/tech-design/SKILL.md#skill-per-step).

In Use, `pin` means the skill is listed in `external-skills.json`. `copy + adapt` means ADR 0003 copies that skill. `(TD2+)` marks that later copy. Reading `input/` and the no-op pass are steps in this pack.

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

## Start

`/tech-design new <name> [types]` charts the map.

1. Name the destination with `grilling` and `domain-modeling`: a technical design developers can build. The destination fixes the scope.
2. Call them again across the whole space, not deep on one thread. If that surfaces no fog, a design map is not needed. Stop and ask how to proceed.
3. Create `docs/design/<name>/` with `design.md` (status `draft`, map filled, Decisions so far empty), `glossary.md`, `gaps.md`, and `log.md`. Notes list only the skills this design needs. `research/` and `input/` appear when the first note is saved. `openapi.yaml` is added only when api is named. Do not invent endpoints. This folder has no README of its own. This file is the owner guide.
4. Add the gaps you can phrase now, then wire the `blocks` column in a second pass. Leave the rest in **Not yet specified**.
5. Fan out one research subagent per research gap. It sets `owner`, calls `research`, and writes `research/<topic>.md`.
6. Stop. Charting resolves nothing by hand.

A second `new` for a folder that already exists stops. Resume instead.

## Resume

`/tech-design <name>` loads `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. It does not load every research note.

Status `ready`, `committed`, or `abandoned`: say so and stop. Otherwise claim the first frontier gap (status `open`, nothing in `blocks` still open, no owner) before any work, or the gap the person named. Resolve that one gap by its type, then stop.

`/tech-design <name> grill [gap]` is the same path. Advice comes before the one question.

`/tech-design <name> ready` refuses while any gap is `open`, `research`, or `waiting-on-input`. When the gate passes it sets document status `ready` and writes `tickets.md`.

## Loop

One gap, then stop. Never more than one gap in a session. Research gaps may fan out.

Claim it first (`owner` set). The frontier is the open, unblocked, unclaimed rows. Refer to a gap by its question, not by a bare id.

- **Research** (AFK): a subagent calls `research`. Findings go in `research/<topic>.md`.
- **Prototype** (HITL): a cheap artifact to react to, linked from the gap row. The human's reaction is the answer.
- **Grilling** (HITL): advice first, then one question. Call `grilling` and `domain-modeling`. The agent never answers its own grill question.
- **Task** (HITL or AFK): work that unblocks a decision. No live call to a dependency. Proof is the owner's spec, or their written answer, saved in `input/`.

A grilling gap, in this order:

1. Read the code and the sources the decision waits on.
2. Draft advice. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen.
3. Grill. Ask one question with the recommended option. Then stop.
4. The human or the dependency owner chooses.
5. Update the gap row, add one `log.md` line, and add a gist line under **Decisions so far**.
6. Add gaps the answer made phraseable, graduate fog, rule out-of-scope gaps out, and update invalidated gaps.
7. Stop.

Advice alone never closes a gap.

```mermaid
flowchart TD
  research["Research: code, sources, input"] --> advice["Draft: 2 to 3 options, recommended, why and source, risk if accepted-risk"]
  advice --> grill["Grill: one question"]
  grill --> choice["Human or dependency owner chooses"]
  choice --> record["Update gaps.md and one log.md line"]
  record --> halt["Stop"]
  advice -.-> never["Advice alone never closes a gap"]
```

## Fog, decisions, and out of scope

The map block sits at the top of `design.md`, under the status line. It is an index, loaded once per session. The decision lives in the gap row. The map only gists it.

**Decisions so far** is one line per closed gap: the question, a pointer to the row, and a one-line gist.

**Not yet specified** is the fog of war: in-scope questions you cannot phrase yet. If you can state the question now, it is a gap, even when it is blocked. If you cannot phrase it yet, it stays fog. Fog graduates as the frontier advances. Do not pre-slice it.

**Out of scope** is work beyond the destination. It never graduates. Closing a gap that sits past the destination leaves one line here, and that line is not a decision.

## Gap status

A row holds id, question, type, status, owner, due, waiting-on note, source or answer, and blocks. `blocks` lists the gaps that must be `decided` or `accepted-risk` first. A `waiting-on-input` row has an owner and a due date. `decided` and `accepted-risk` do not block the ready gate.

```mermaid
stateDiagram-v2
  state "waiting-on-input" as waiting
  state "accepted-risk" as risk
  [*] --> open
  open --> research: this session studies it
  open --> waiting: no answer
  open --> decided: answer recorded
  open --> risk: risk accepted
  research --> waiting: no answer
  research --> decided: answer recorded
  research --> risk: risk accepted
  waiting --> research: input saved
  waiting --> decided: input answers it
```

## Document status

Status lives in `design.md`. `draft` is the new folder. `discussion` is work while gaps remain. `ready` means the gap gate passed. `committed` means builders follow this design. `abandoned` means the work stopped.

```mermaid
stateDiagram-v2
  [*] --> draft: new folder
  draft --> discussion: work begins
  discussion --> discussion: one gap per session
  discussion --> ready: gap gate passed
  discussion --> abandoned: work stopped
  ready --> committed: builders follow this design
  committed --> [*]
  abandoned --> [*]
```

## Writing for humans and agents

Two readers. The agent researches and drafts. The human writes the decisions. Each grill question carries the recommended answer and why. The agent asks only what research cannot settle. The human confirms or overrides. That is the decision.

Point to code. `design.md` links real files, symbols, and `openapi.yaml`. Call `writing-for-agents` when you edit `design.md`, and `deslop` when `openapi.yaml` or a code snippet changes.

Keep it short. Detail goes to `research/`. Every gap has a hard done-when.

Glossary. Each design folder has `glossary.md`.

## Components

| Component | Name | Responsibility |
| --- | --- | --- |
| Skill | `tech-design` | User-invoked routing for new, resume, grill, and ready. Not model-invoked. |
| Command | `tech-design` | Runs the mode. Copilot picker: `/productivity:tech-design` |

Pins, loaded by exact Skill name: `domain-modeling`, `codebase-design`, `grilling`, `research`, `writing-for-agents`, `deslop`.

## Editing

Authored: `plugin.json`, `skills/`, `commands/`, and `external-skills.json`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client manifests.

Test on a feature branch without merging to `main`:
[ADD-SKILL.md: Test a skill locally](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-SKILL.md#test-a-skill-locally).
