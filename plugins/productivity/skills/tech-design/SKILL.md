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

What we changed:

- The map is the design folder `docs/design/<name>/`, not an issue on a tracker. No tracker setup, no `wayfinder:map` label, and no child issues.
- Tickets are gaps: rows in `gaps.md`. The name is the question. Blocking is the `blocks` column. The claim is `owner`, set before any work. The frontier is the open, unblocked, unclaimed gaps.
- Destination, Notes, Decisions so far, Not yet specified, and Out of scope live at the top of `design.md` and are loaded once per session. Decisions so far is an index: one gist line and a pointer to the gap row. The decision lives in exactly one place, the gap.
- Research subagents write `research/<topic>.md`. No throwaway branch and no pull request.
- Prototype stays a HITL gap. This pack does not pin a prototype skill, so the session makes the cheap artifact and links it from the gap row.
- A task gap makes no real dependency call. Proof is the owner's spec, or a written answer in `input/`.
- A grilling gap researches first, drafts advice, then asks one question and stops. Advice alone never closes a gap. Which skill runs at which step is [Skill per step](#skill-per-step).
- Gap statuses are `open`, `research`, `waiting-on-input`, `decided`, and `accepted-risk`. Document status on `design.md` is `draft`, `discussion`, `ready`, `committed`, or `abandoned`.
- Fixed design sections follow the map block in `design.md`.
- Resume and grill load the status line and the map sections in `design.md`, `gaps.md`, and the last `log.md` line.
- Modes are chart (`/tech-design new <name> [types]`), work (resume and grill), and ready. Ready writes `tickets.md` and `wayfinder-brief.md`.

A loose idea has arrived, too big for one agent session, and wrapped in fog: the way from here to the **destination** isn't visible yet. Tech design is about finding that way, not charging at the destination. This skill charts the way as a **shared map** in `docs/design/<name>/`, then works its **gaps** (questions whose resolution is a decision, not slices of a build to execute) one at a time until the route is clear.

The destination varies per effort, and naming it is the first act of charting: it shapes every gap. Here it is a technical design developers can build. Almost all gaps are closed before handoff, so a later wayfinder pass can turn the design into tickets that take story points.

## Plan, don't do

Tech design is **planning** by default: each gap resolves a decision, and the map is done when the way is clear, with nothing left to decide before someone goes and does the thing. The pull to just do the work is usually the signal you've reached the edge of the map and it's time to hand off. An effort can override this in its **Notes**, carrying execution into the map itself, but absent that, produce decisions, not deliverables.

Each session charts the folder, resolves one gap, or runs the ready gate. It does not write the whole design. It does not build the product.

## Refer by name

Every gap has a **name**: its question. In everything the human reads (narration, the map's Decisions so far), refer to it by that question, never by a bare id, number, or slug. A wall of `G1, G2, G3` is illegible; names read at a glance. The id doesn't vanish; the question wraps a pointer to its row, but the id rides _inside_ the name, never stands in for it.

## The Map

The map is the design folder `docs/design/<name>/`. `design.md` is the canonical artifact. Its gaps are rows in `gaps.md`.

The map is an **index**, not a store. It lists the decisions made and points at the gaps that hold their detail; a decision lives in exactly one place, its gap, so the map never restates it, only gists it and links.

There is no issue tracker. Blocking, claims, and the frontier live in `gaps.md`.

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

Document status is the first line under the title: `Status: draft`, `discussion`, `ready`, `committed`, or `abandoned`.

- `draft`: the new folder.
- `discussion`: work while gaps remain. Resume and grill repeat here.
- `ready`: the gap gate passed.
- `committed`: builders follow this design.
- `abandoned`: the work stopped.

The map block comes next, then the fixed sections, in order: user outcome, context and scope, goals, non-goals, glossary, design, dependencies, alternatives, cross-cutting concerns, type checklists, advice, risks, acceptance criteria, sign-off. User outcome is first among those sections. The design section holds an overview, a small diagram, boundaries and contracts, a data model, and flows as mermaid. Put each diagram next to the text it explains. Alternatives include do nothing. Cross-cutting concerns are security, privacy, observability, rollout, and testing. The advice section states the recommended choice, why, and what changes if a gap flips. Each waiting gap has a deadline in `design.md`. Link real files, symbols, and `openapi.yaml`. Do not paste them.

Record named types on the design. Do not invent a type checklist. Type playbooks (web, api, data, integration) are coming in TD2+. When api is named, keep `openapi.yaml`. An empty contract field is a gap. A state diagram uses `statechart` (TD2+) and writes the mermaid state in `design.md`.

### The map body

The whole map at low resolution, loaded once per session. These sections are the map block at the top of `design.md`. Open gaps are **not** listed: they are rows in `gaps.md`, found by reading that file.

```markdown
## Destination

<what reaching the end of this map looks like: the technical design this effort is finding its way to. One or two lines; every session orients to it before choosing a gap.>

## Notes

<domain; only the skills this design really needs, picked from Skill per step; standing preferences for this effort>

## Decisions so far

<!-- the index: one line per closed gap, enough to judge relevance, then zoom the gap row for the detail the gap holds. A decision lives in exactly one place, the gap. The map never restates it. -->

- <gap question> (pointer to its row in gaps.md): <one-line gist of the answer>

## Not yet specified

<!-- see "Fog of war": in-scope fog you can't write as a gap yet; graduates as the frontier advances -->

## Out of scope

<!-- see "Out of scope": work ruled beyond the destination; closed, never graduates -->
```

### Gaps

Each gap is a **row** in `gaps.md`. Columns: id, question, type, status, owner, due, waiting-on note, source or answer, blocks. The question is the decision or investigation this gap resolves, sized to one 100K token agent session, and it ends with a hard done-when.

Each gap carries a type, one of `research`, `prototype`, `grilling`, `task` (see [Gap types](#gap-types)). The question never states the type. Read the type column.

A session **claims** a gap by setting `owner` to the person driving the map, **first**, before any work, so concurrent sessions skip it. That owner _is_ the claim: an open gap with an empty owner is unclaimed.

Blocking uses the `blocks` column: the ids of the gaps that must be resolved before this one. A gap is **unblocked** when every id in `blocks` is `decided` or `accepted-risk` (an empty `blocks` column is unblocked). The **frontier** is the rows whose status is `open`, that are unblocked, and that have no owner: the edge of the known.

The answer isn't part of the question; it's recorded on resolution (see [Work through the map](#work-through-the-map)). Assets created while resolving a gap are linked from the row, not pasted in.

Statuses: `open`, `research`, `waiting-on-input`, `decided`, `accepted-risk`.

This session moves a gap to `research` while it studies it. No answer: `to-questionnaire` (TD2+) writes the question in `input/`, then the row moves to `waiting-on-input` with an owner and a due date. The human or the dependency owner moves it to `decided` by an answer you save. They move it to `accepted-risk` only when they accept the risk you named. Advice alone never closes a gap.

Saved input moves `waiting-on-input` back to `research`, or to `decided` when that input answers the question.

Do not add a gap you cannot phrase. Unphrased in-scope work stays in **Not yet specified**.

## Gap types

Every gap is either **HITL** (human in the loop, worked _with_ a human who speaks for themselves) or **AFK**, driven by the agent alone. A HITL gap only resolves through that live exchange; the agent never stands in for the human's side of it (a grilling agent that answers its own questions has broken this). The agent never answers its own grill question.

- **Research** (AFK): Reading documentation, third-party APIs, or local resources like knowledge bases to surface a fact a decision waits on. Resolved by a subagent: `how` (TD2+) on the code, then call the Skill tool with `research` on the docs, and read `input/`. Findings go in `research/<topic>.md`. Use when knowledge outside the current working directory is required.
- **Prototype** (HITL): Raise the fidelity of the discussion by making a cheap, rough, concrete artifact to react to (an outline, a rough take, a stub, or UI/logic code). Link the prototype from the gap row. Use when "how should it look" or "how should it behave" is the key question. Resolved only through the live human reaction to that artifact.
- **Grilling** (HITL): Conversation. The default case. This session asks one question, the gap's question, then stops. Do not open another gap's question in the same session. The agent never answers its own grill question. Research first: `how` (TD2+) on the code, then call the Skill tool with `research` on the docs, and read `input/`. Draft advice: `architect` (TD2+) and `principle-exhaust-the-design-space` (TD2+) for options, call the Skill tool with `codebase-design` when a seam is in play, `why` (TD2+) for the source, and `blast-radius` (TD2+) for the risk line. Offer 2 to 3 options, the recommended one, why with a source, and the risk if `accepted-risk` is chosen. Then call the Skill tool with `grilling` and `domain-modeling`. The question includes the recommended option and why.
- **Task** (HITL or AFK): Manual work that must happen before a _decision_ can be made: nothing to decide, prototype, or research, but the discussion is blocked until it's done. Signing up for a service so its API can be judged, provisioning access, moving data so its shape can be seen. This is the one type that _does_ rather than decides, and it earns its place by unblocking a decision, not by delivering the destination. The agent drives it alone where it can (AFK); otherwise it hands the human a precise checklist (HITL). Resolved when the work is done; the answer records what was done and any resulting facts (credentials location, new URLs, row counts) later gaps depend on. No real dependency calls. Proof is the owner's spec, or their written answer, saved in `input/`. Chat text is not proof. An empty proof field stays a blocking gap. The dependency-contract playbook is coming in TD2+.


## Fog of war

The map is _deliberately_ incomplete: don't chart what you can't yet see. Beyond the live gaps lies the **fog of war**: the dim view of decisions and investigations you can tell are coming but can't yet pin down, because they hang on questions still open. Resolving a gap clears the fog ahead of it, graduating whatever's now specifiable into fresh gaps, one at a time, until the way to the destination is clear and no gaps remain.

The map's **Not yet specified** section is where that dim view is written down: the suspected question, the area to revisit later. It's the undiscovered frontier _toward_ the destination: everything here is in scope, just not sharp enough to write as a gap. Write as loosely or as fully as the view allows; it doubles as a signpost for collaborators reading where the effort is headed.

**Fog or ticket?** The test is whether you can state the question precisely now, _not_ whether you can answer it now.

- **Ticket when** the question is already sharp, even if it's blocked and you can't act on it yet. Write that ticket as a gap row.
- **Not yet specified when** you can't yet phrase it that sharply. Don't pre-slice the fog into gap-sized pieces: it's coarser than a gap, and one patch may graduate into several gaps, or none, once the frontier reaches it.

**Not yet specified** excludes what's already decided (Decisions so far), what's already a live gap, and what's out of scope (the next section).

## Out of scope

Fog only ever gathers _toward_ the destination. The destination fixes the scope, so work beyond it is **out of scope**: it isn't fog, and it doesn't belong in **Not yet specified**. It gets its own **Out of scope** section on the map: work you've consciously ruled out of _this_ effort. Scope, not sharpness, lands it here.

Out-of-scope work never graduates (the frontier stops at the destination), so it returns only if the destination is redrawn, and then as a fresh effort, not a resumption.

Ruling something out of scope is a scoping act, not a step on the route. When a gap that already exists turns out to sit past the destination (mis-scoped in while charting, or exposed by a resolution), **close it** (a closed gap is unambiguously off the frontier: drop the row, do not leave it `open`) and leave one line in the **Out of scope** section: the gist plus why it's out of scope, naming the question. It stays out of **Decisions so far**, which records the route actually walked; a scope boundary isn't a step on it. The non-goals section points at that line. It does not restate it.

## Skill per step

Call a pinned skill with the Skill tool by exact name. A name marked (TD2+) ships in a later pack: name it at the step, and leave the call until that pack pins it.

| Step | Skills | Output |
| --- | --- | --- |
| Chart: destination and frontier | `grilling` + `domain-modeling` | design.md map, gaps.md, glossary.md |
| Research per gap (first) | `how` (TD2+) on the code, `research` on the docs, read input/ | research/<topic>.md |
| Draft advice | `architect` (TD2+) + `principle-exhaust-the-design-space` (TD2+) for options, `codebase-design` when a seam is in play, `why` (TD2+) for the source, `blast-radius` (TD2+) for the risk line | advice block in the gap |
| Grill (one question) | `grilling` + `domain-modeling` | answer in the gap, log line |
| No answer | `to-questionnaire` (TD2+) | question in input/, waiting-on-input |
| State diagram | `statechart` (TD2+) | mermaid state in design.md |
| Every edit of design.md | `writing-for-agents`, plus `deslop` on openapi.yaml or code | (none) |
| Ready | `interrogate` (TD2+) over the whole design, then `technical-writing` (TD2+), `unslop` (TD2+) and the no-op pass, then `to-spec` (TD2+) and `to-tickets` (TD2+) | tickets.md, wayfinder-brief.md |

## Invocation

The person types the command. Chart and work are the two modes. Ready is the gate after the way is clear. Either way, **never resolve more than one gap per session**, with the exception of research gaps. Charting and ready resolve nothing by hand.

### Chart the map

User invokes with `/tech-design new <name> [types]`.

1. **Name the destination.** Call the Skill tool with `grilling` and `domain-modeling`, to pin down what this map is finding its way to: a technical design developers can build. The destination fixes the scope, so it's settled first. Record any types the person named. Do not invent a checklist.
2. **Map the frontier.** Call the Skill tool with `grilling` and `domain-modeling` again, **breadth-first** this time: fan out across the whole space rather than deep on any one thread, surfacing the open decisions and the first steps takeable now. **If this surfaces no fog** (the way to the destination is already clear, the whole journey small enough for one session), a design map is not needed. Stop and ask the user how they'd like to proceed.
3. **Create the folder** `docs/design/<name>/`: `design.md` (status `draft`, Destination and Notes filled in, Decisions so far empty, the fog sketched into **Not yet specified**), `glossary.md`, `gaps.md`, and `log.md`. Add `openapi.yaml` only when api is named. Call the Skill tool with `writing-for-agents` on this `design.md` edit, and with `deslop` when `openapi.yaml` or a code snippet changes. Leave `research/` and `input/` until there is a note to save. Leave `tickets.md` and `wayfinder-brief.md` for ready.
4. **Create the gaps you can specify now** as rows in `gaps.md` (status `open`, owner empty, type set), then wire `blocks` in a **second pass** (rows need ids before they can reference each other). Write cross-references in that pass too, with real ids. Wiring sorts them into the frontier and the blocked; everything you can't yet specify stays in the fog: the **Not yet specified** section. If the person already named one sharp question, that row is `open` and document status becomes `discussion`. Otherwise stay at `draft`. Call the Skill tool with `writing-for-agents` on that status edit.
5. **Fire the research subagents.** For each `research` gap you just created, spin up a subagent with a fresh context window that claims it (sets `owner`). `how` (TD2+) on the code, then call the Skill tool with `research` on the docs, and read `input/`. Findings go in `research/<topic>.md`, with a context pointer from the gap row. Open no PR.
6. Stop: charting is one session's work; it resolves nothing by hand. Add one `log.md` line for the chart session.

A second `new` for a folder that already exists stops. Tell them to resume.

### Work through the map

User invokes with `/tech-design <name>` (resume) or `/tech-design <name> grill [gap]`. A gap is **optional**: without one, you pick the next decision, not the user. Grill is this same path for the named gap, or for the first frontier gap if none was named.

1. Load the **map** at low resolution, not every research note. Load `design.md` (the status line and the map sections), `gaps.md`, and the last `log.md` line. Missing folder: stop. Tell them to run `new`. Status `ready`, `committed`, or `abandoned`: say so and stop.
2. Choose the gap. If the user named one, use it. Otherwise take the first frontier gap in order. **Claim it**: set `owner` before any work.
3. Resolve it as the type its type column names (see [Gap types](#gap-types)). Read the type, not just the question: the question never states the type. **Zoom as needed**: read any related or closed gap row on demand; call the Skill tool for whichever skills the `## Notes` block names. On a grilling gap, research and advice come before the question.
4. Record the resolution: write the answer in the gap row, append one `log.md` line (date, the question, the id beside that name, what changed), and append a gist line to **Decisions so far** (the question, a pointer to the row, one line). Call the Skill tool with `writing-for-agents` on that `design.md` edit, and with `deslop` when `openapi.yaml` or a code snippet changes. Status `decided`, or `accepted-risk` only when the human accepts the risk you named. No answer: `to-questionnaire` (TD2+) writes the question in `input/`, then set `waiting-on-input`, an owner, and a due date, and record the deadline in `design.md`. Advice alone never closes a gap.
5. Add newly surfaced gaps (create, then wire `blocks`); graduate any fog the answer has made specifiable, clearing each graduated patch from **Not yet specified** so it lives only as its new gap. If the answer reveals that a gap (this one or another) sits beyond the destination, **rule it out of scope** rather than resolving it on the route. If the decision invalidates other parts of the map, update those gaps. Call the Skill tool with `writing-for-agents` on any `design.md` edit in this step, and with `deslop` when `openapi.yaml` or a code snippet changes.

The user may run unblocked gaps in parallel, so expect other sessions to be editing the folder concurrently.

### Ready

User invokes with `/tech-design <name> ready`.

1. Read `gaps.md`. If any row is `open`, `research`, or `waiting-on-input`, list those rows by question and stop. Do not set `ready`.
2. `decided` and `accepted-risk` do not block. A folder with no `decided` or `accepted-risk` row is not ready. Empty sections are not ready.
3. When the gate passes, set status `ready`. Run `interrogate` (TD2+) over the whole design, then `technical-writing` (TD2+), `unslop` (TD2+) and the no-op pass, then `to-spec` (TD2+) and `to-tickets` (TD2+). The no-op pass deletes every line that does not change what the reader does. Call the Skill tool with `writing-for-agents` on the `design.md` edit, and with `deslop` when `openapi.yaml` or a code snippet changes. Write `tickets.md`: buildable slices whose blockers are sized for story points. Write `wayfinder-brief.md`: destination, decided gaps by question, accepted risks, and links to `design.md` and to `openapi.yaml` when it exists.
4. Add one `log.md` line. Stop. Ready resolves nothing.

Good: one gap, a recommended option with a source, the human's answer recorded, then stop.
Bad: a full design in one session, a gap closed from advice, a dependency called to discover its contract.
