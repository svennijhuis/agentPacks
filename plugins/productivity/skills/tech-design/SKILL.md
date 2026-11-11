---
name: tech-design
description: When the user types /tech-design to start, resume, grill, or ready a technical design.
license: MIT
disable-model-invocation: true
user-invocable: false
---

# Tech design

User-invoked. The command is the slash. The model does not start a design on its own.

Load with the Skill tool by exact name `tech-design`. Never write `/tech-design` as prose to load it.

Source: `skills/engineering/wayfinder` in https://github.com/mattpocock/skills at `4ae2317ddb22b046e26455e4f9c9b3aaaf62212d`. See [NOTICE.md](NOTICE.md).

What we changed: the map is a design folder, not an issue tracker. Tickets are gaps. One gap per session, then stop. Advice never closes a gap. Start, resume, grill, and ready are modes of this command. No tracker setup, no prototype tickets, no research branches.

A design is too big for one session. This skill charts `docs/design/<name>/`, then takes one gap at a time until developers can build it. Almost all gaps are closed before handoff, so a later wayfinder pass can turn the design into tickets that take story points.

## Plan, don't do

Each session charts the folder, resolves one gap, or runs the ready gate. It does not write the whole design. It does not build the product. The pull to fill every gap is the signal to stop.

## Refer by question

Each gap has an id and a question. In anything the human reads, name the question. The id rides with the name. A wall of bare ids is illegible.

## The folder

`docs/design/<name>/` is the map. `design.md` links the other files. It does not copy them.

| File | Role |
| --- | --- |
| `design.md` | The design. Status and the map sections live here |
| `glossary.md` | Domain language |
| `gaps.md` | One row per gap |
| `research/<topic>.md` | Notes for one topic |
| `input/<date>.md` | Input saved that day |
| `log.md` | One line per session |
| `openapi.yaml` | Only when the api type is in use |
| `tickets.md` | Written by ready, after the gate passes |
| `wayfinder-brief.md` | Written by ready, after the gate passes |

`design.md` sections, in order: user outcome, context and scope, goals, non-goals, glossary, design, dependencies, alternatives, cross-cutting concerns, type checklists, advice, risks, acceptance criteria, sign-off. User outcome is first. The design section holds an overview, a small diagram, boundaries and contracts, a data model, and flows as mermaid. Put each diagram next to the text it explains. Alternatives include do nothing. Cross-cutting concerns are security, privacy, observability, rollout, and testing. Advice states the recommended choice, why, and what changes if a gap flips. Each waiting gap has a deadline in `design.md`. Link real files, symbols, and `openapi.yaml`. Do not paste them.

Document status is the first line under the title: `Status: draft`, `discussion`, `ready`, `committed`, or `abandoned`.

- `draft`: the new folder.
- `discussion`: work while gaps remain. Resume and grill repeat here.
- `ready`: the gap gate passed.
- `committed`: builders follow this design.
- `abandoned`: the work stopped.

## Gaps

One row: id, question, status, owner, due, waiting-on note, source or answer, blocks. The question ends with a hard done-when.

Statuses: `open`, `research`, `waiting-on-input`, `decided`, `accepted-risk`.

This session moves a gap to `research` while it studies it. No answer moves it to `waiting-on-input` with an owner and a due date. The human or the dependency owner moves it to `decided` by an answer you save. They move it to `accepted-risk` only when they accept the risk you named. Advice alone never closes a gap.

Saved input moves `waiting-on-input` back to `research`, or to `decided` when that input answers the question.

Do not add a gap you cannot phrase. Unphrased work stays in non-goals or a short note. Work past the destination is a non-goal, not a gap.

## One gap

Then stop.

1. Research. Read the code first. Call the Skill tool by exact name `research`. Read specs in `input/`. Check known patterns. `how` is coming in TD2+.
2. Draft advice. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen. `why` is coming in TD2+.
3. Grill. Call the Skill tool by exact name `grilling`. Ask one question. Include the recommended option and why.
4. The human or the dependency owner chooses. No answer: set `waiting-on-input`, an owner, and a due date.
5. Update `gaps.md`. Add one `log.md` line: date, gap id, question, what changed.
6. Stop.

Call `domain-modeling` when a term is in play and update `glossary.md`. Call `codebase-design` when a module seam is in play. Call `writing-for-agents` when you edit `design.md`. Call `deslop` when `openapi.yaml` or a code snippet changes. `technical-writing` and `unslop` are coming in TD2+. After them, a no-op pass deletes every line that does not change what the reader does.

No real calls to a dependency. Proof is the owner's spec, or their written answer, saved in `input/`. Chat text is not proof. An empty proof field stays a blocking gap. The dependency-contract playbook is coming in TD2+.

Type playbooks (web, api, data, integration) are coming in TD2+. Record named types on the design. When api is named, keep `openapi.yaml` (OpenAPI 3.1) and do not invent endpoints. An empty contract field is a gap.

## Invocation

The person types the command. Four modes. Never resolve more than one gap per session. Charting and ready resolve none.

### New

`/tech-design new <name> [types]`

1. Name the destination in the words the person used: a technical design developers can build.
2. Create the folder and the files in the table, except `openapi.yaml`, `tickets.md`, and `wayfinder-brief.md`. Status `draft`. Resume is `/tech-design <name>`.
3. Record any types the person named. Do not invent a checklist. Add `openapi.yaml` only when api is named.
4. If the person already named one sharp question, add that gap as `open` and set status `discussion`. Otherwise stay at `draft`.
5. Stop. Do not resolve a gap in this session.

A second `new` for a folder that already exists stops. Tell them to resume.

### Resume

`/tech-design <name>`

1. Load `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. Do not load every research note.
2. Missing folder: stop. Tell them to run `new`. Status `ready`, `committed`, or `abandoned`: say so and stop.
3. Pick the gap. If the person named one, use it. Otherwise take the first `open` or `research` gap that is not blocked.
4. Run [One gap](#one-gap). Stop.

### Grill

`/tech-design <name> grill [gap]`

Same as resume, for the named gap, or the next unblocked `open` or `research` gap if none was named. Research and advice come before the question. One question. Stop.

### Ready

`/tech-design <name> ready`

1. Read `gaps.md`. If any row is `open`, `research`, or `waiting-on-input`, list those rows and stop. Do not set `ready`.
2. `decided` and `accepted-risk` do not block. A folder with no `decided` or `accepted-risk` row is not ready. Empty sections are not ready.
3. When the gate passes, set status `ready`. Write `tickets.md`: buildable slices whose blockers are sized for story points. Write `wayfinder-brief.md`: destination, decided gaps by question, accepted risks, and links to `design.md` and to `openapi.yaml` when it exists.
4. Add one `log.md` line. Stop.

Good: one gap, a recommended option with a source, the human's answer recorded, then stop.
Bad: a full design in one session, a gap closed from advice, a dependency called to discover its contract.
