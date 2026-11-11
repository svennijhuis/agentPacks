# Map template

Copy this into `docs/design/<name>/design.md` when you chart, and follow it on every later edit of that file. Call `writing-for-agents` on every edit. Call `deslop` when `openapi.yaml` or a code snippet changes. A state diagram calls `statechart` (TD2+) and writes a mermaid state diagram in `design.md`, beside the text it explains.

Do not add a readme file in the design folder.

## Files

| File | Role |
| --- | --- |
| `design.md` | Status line, map block, then the fixed sections. Links out. Does not copy the other files |
| `glossary.md` | Domain language. `domain-modeling` updates it when a term is in play |
| `gaps.md` | One row per gap. This file is the frontier |
| `research/<topic>.md` | One topic, written when research runs |
| `input/<date>.md` | Input saved that day |
| `log.md` | One line per session |
| `openapi.yaml` | Only when the api type is named. OpenAPI 3.1. Do not invent endpoints |
| `tickets.md` | Written by ready, after the gate passes |
| `wayfinder-brief.md` | Written by ready, after the gate passes |

Leave `research/` and `input/` until the first note. Leave `tickets.md` and `wayfinder-brief.md` for ready.

## Status line

First line under the title: `Status: draft`, `discussion`, `ready`, `committed`, or `abandoned`.

- `draft`: the new folder.
- `discussion`: gaps remain. Resume and grill repeat here.
- `ready`: the gap gate passed.
- `committed`: builders follow this design.
- `abandoned`: the work stopped.

## Map block

```markdown
## Destination

<one or two lines. The technical design this effort is finding. Every session reads this before it picks a gap.>

## Notes

Domain: <one line>
Skills: <only the skills this design really needs, picked from Skill per step>
Preferences: <standing preferences for this effort>

## Decisions so far

- <gap question>, pointer to its row in gaps.md: <one-line gist>

## Not yet specified

<one line per in-scope patch you cannot phrase as a gap yet>

## Out of scope

<one line per ruled-out question, and why it is out>
```

A decision lives on the gap row. Decisions so far is the gist and the pointer.

Fog of war sits in Not yet specified. It is in scope and not sharp yet. A question you can phrase now is a gap row, even when `blocks` still holds it. One patch can become several gaps, or none. Do not pre-slice it. Clearing a patch means deleting that line once its gap rows exist.

Failure: five vague rows for one dim area. Write one line under Not yet specified instead.

Out of scope is beyond the destination. It does not graduate. Drop a row that sits past the destination. Leave one line here. Do not list it under Decisions so far. Non-goals points at that line and does not restate it.

Failure: the row stays `open` after you rule it out, so the frontier still offers it.

## Fixed sections

After the map block, in this order: user outcome, context and scope, goals, non-goals, glossary, design, dependencies, alternatives, cross-cutting concerns, type checklists, advice, risks, acceptance criteria, sign-off.

User outcome is first. The design section holds an overview, a small diagram, boundaries and contracts, a data model, and flows as mermaid. Put each diagram next to the text it explains. Alternatives include do nothing. Cross-cutting concerns are security, privacy, observability, rollout, and testing. The advice section states the recommended choice, why, and what changes if a gap flips. Each waiting gap has a deadline in `design.md`. Link real files, symbols, and `openapi.yaml`. Do not paste them.

Record named types. Do not invent a type checklist. Type playbooks are TD2+. An empty contract field is a gap.
