# productivity

A capability pack for a technical design developers can build. `/tech-design` charts a shared map, then closes gaps one at a time, until a later wayfinder pass can turn the design into tickets that take story points.

Claude, Cursor, and Codex run `/tech-design` (Codex: `$tech-design`). Copilot picker: `/productivity:tech-design`. The skill is `tech-design`.

The person types the command. The model does not start a design on its own. One session charts the folder, takes one gap, or runs the ready gate, then stops. Research gaps are the exception: chart can fire those subagents together.

Adapted from Matt Pocock's wayfinder skill (`skills/engineering/wayfinder` at `4ae2317ddb22b046e26455e4f9c9b3aaaf62212d`). [NOTICE](skills/tech-design/NOTICE.md).

## Which external skill when

Step to skill. When and which file: [When to use which skill](skills/tech-design/SKILL.md#when-to-use-which-skill).

| Step | Skill |
| --- | --- |
| chart: destination grill | `grilling` |
| chart: breadth-first grill | `grilling` |
| research gap | `research` |
| grilling gap advice | `research` |
| grilling gap question | `grilling` |
| a term is in play | `domain-modeling` |
| a module seam is in play | `codebase-design` |
| editing design.md | `writing-for-agents` |
| openapi.yaml or code snippet changes | `deslop` |
| ready | TD2 |

## Start

`/tech-design new <name> [types]` charts the map.

1. Name the destination. Call the Skill tool with `grilling`: a technical design developers can build. The destination fixes the scope.
2. Call the Skill tool with `grilling`, breadth-first, to map the frontier. If that surfaces no fog, a design map is not needed. Stop and ask how to proceed.
3. Create `docs/design/<name>/` with `design.md` (status `draft`, map block filled, Decisions so far empty), `glossary.md`, `gaps.md`, and `log.md`. Call the Skill tool with `writing-for-agents`. `research/` and `input/` appear when the first note is saved. `openapi.yaml` is added only when the api type is in use. This folder has no README of its own. This file is the owner guide.
4. Add the gaps you can specify now, then wire the `blocks` column in a second pass. Sketch the rest in **Not yet specified**.
5. Fire a research subagent for each research gap. Call the Skill tool with `research`. Findings go in `research/<topic>.md`.
6. Stop. Charting resolves nothing by hand.

A second `new` for a folder that already exists stops. Resume instead.

## Resume

`/tech-design <name>` loads `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. It does not load every research note.

Status `ready`, `committed`, or `abandoned`: say so and stop. Otherwise claim the first frontier gap (status `open`, nothing in `blocks` still open, no owner) before any work, or the gap the person named. Resolve that one gap by its type, then stop.

`/tech-design <name> grill [gap]` is the same path. Research and advice come before the one question.

`/tech-design <name> ready` runs the gap gate. It sets document status `ready` only when no gap is `open`, `research`, or `waiting-on-input`. The skill for this gate is TD2. It then writes `tickets.md` and `wayfinder-brief.md`.

## Loop

One gap, then stop. Never more than one gap in a session, except research gaps.

Claim it first (`owner` set). The frontier is the open, unblocked, unclaimed rows. Refer to a gap by its question, not by a bare id.

- **Research** (AFK): a subagent calls the Skill tool with `research`. Findings go in `research/<topic>.md`.
- **Prototype** (HITL): a cheap artifact to react to, linked from the gap row. The human's reaction is the answer.
- **Grilling** (HITL): call the Skill tool with `grilling`, then the flowchart below. The agent never answers its own grill question.
- **Task** (HITL or AFK): work that unblocks a decision. No real call to a dependency. Proof is the owner's spec, or their written answer, saved in `input/`.

A grilling gap, in this order:

1. Research. Call the Skill tool with `research`. Read the code first, read specs in `input/`, and check known patterns.
2. Draft advice. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen.
3. Grill. Ask one question with the recommended option.
4. The human or the dependency owner chooses.
5. Update the gap row, add one `log.md` line, and add a gist line under **Decisions so far**.
6. Add gaps the answer made specifiable, graduate fog, rule out-of-scope gaps out, and update invalidated gaps.
7. Stop.

Advice alone never closes a gap.

```mermaid
flowchart TD
  research["Research: code, research, input specs, known patterns"] --> advice["Draft: 2 to 3 options, recommended, why and source, risk if accepted-risk"]
  advice --> grill["Grill: one question"]
  grill --> choice["Human or dependency owner chooses"]
  choice --> record["Update gaps.md and one log.md line"]
  record --> halt["Stop"]
  advice -.-> never["Advice alone never closes a gap"]
```

## Fog, decisions, and out of scope

The map block sits at the top of `design.md`, under the status line. It is an index, loaded once per session. The decision lives in the gap row. The map only gists it.

**Decisions so far** is one line per closed gap: the question, a pointer to the row, and a one-line gist.

**Not yet specified** is the fog of war: in-scope questions you cannot phrase yet. Fog or ticket: if you can state the question precisely now, it is a gap, even when it is blocked. If you cannot phrase it yet, it stays fog. Fog graduates as the frontier advances. Do not pre-slice it.

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

Point to code. `design.md` links real files, symbols, and `openapi.yaml`.

Keep it short. Detail goes to `research/`. Every gap has a hard done-when.

Glossary. Each design folder has `glossary.md`.

No-op pass. The last pass removes every line that does not change what the reader does.

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
