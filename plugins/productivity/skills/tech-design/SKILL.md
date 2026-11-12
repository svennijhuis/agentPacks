---
name: tech-design
description: When the user types /tech-design to start, resume, check, or ready a technical design.
license: MIT
disable-model-invocation: true
---

A technical design developers can build is too big for one session, and most of what it needs is not known yet. This skill charts that design as a shared map at `docs/design/<yyyy-mm-dd>-<name>/`, finds every gap, and closes them one per session until a developer or an agent can build from the design without guessing. See [NOTICE.md](NOTICE.md).

Name the destination first. The destination is that design. It fixes the scope of every gap.

## Two rules that outrank the rest

**An unknown is a gap. Never write it as an assumption.** When the design needs a fact or a decision and no source gives it, add a row to `gaps.md`. Do not fill it with a default, a guess, or wording that reads the same under two answers.

**An outside system opens its gaps at once.** The moment a system, source, or vendor outside this codebase is named, add its row to Dependencies and one gap per empty field: owner, access, credentials, environments, contract, limits, change notice, proof. See [dependency-contract.md](references/dependency-contract.md). How to connect and how to authenticate are never left for the builder to find out.

## Plan, don't do

Each gap resolves a decision. The map is done when every gap is closed and a developer can build. Builders start after the map is done. Notes may say this effort also does the work. Otherwise stop at the decision.

## Refer by name

A gap's name is its question. Use the question when you talk to the human and in Decisions so far. The id rides along with the question. It never stands in for it.

## The map

Every design has the same folder, the same files, and the same 14 sections in the same order. See [structure.md](references/structure.md) for all of it. Size and types change depth, never structure. See [sizes.md](references/sizes.md).

`design.md` is the spec. Its header carries the status, size, types, dates, owner, and approvers. Its first section is the map. `gaps.md` holds the gaps. Each closed gap is one gist line on the map. The answer stays on the row, and the result is written into the section its category names.

The status words are `draft`, `discussion`, `ready`, `committed`, and `abandoned`. `draft` is the new folder. `discussion` means gaps are still open. `ready` means the gate passed. `committed` means builders follow this design. `abandoned` means the work stopped.

Link files and symbols. Do not paste them. Read dates from the system clock.

### The map body

Load this once per session. Open gaps stay in `gaps.md`.

```markdown
## Map

### Destination

<one or two lines. The technical design this effort ends on. Read this before you pick a gap.>

### Notes

Domain: <one line>
Skills: <only the skills this design needs>
Preferences: <preferences for this design>

### Decisions so far

- <gap question> (<id>): <one-line gist>

### Not yet specified

<one line per in-scope question you cannot phrase yet, or None.>

### Out of scope

<one line per question past the destination, and why>
```

### Gaps

One row in `gaps.md` is one gap. The question fits one session and says when it is done. The type is `research`, `prototype`, `grilling`, or `task`. Read the type column.

**Claim** a gap by setting `owner` before any work. An open row with an empty owner is unclaimed. `blocks` lists the ids that must close first. The **frontier** is status `open`, unblocked, and unclaimed.

While a gap is studied, its status is `research`. No answer yet sets `waiting-on-input`, with who, when asked, and when due. The human's answer sets `decided`. Set `accepted-risk` only when the human accepts the risk you named. Advice alone never closes a gap.

## Gap types

**HITL** means the human speaks for themselves and you work with them. **AFK** means you drive the gap alone. A HITL gap closes only through that exchange. Never answer the human's side.

**Research** (AFK). A fact, not a preference. A subagent with a fresh context finds it and writes `research/<gap-id>-<topic>.md`. The triggers, the brief, and the file shape are in [research.md](references/research.md).

**Prototype** (HITL). Use when the question is how it should look or behave. Make a rough artifact and link it from the row. The human's reaction is the answer.

**Grilling** (HITL). The default. Give advice first: 2 to 3 options, the recommended one, why with a source, and the risk if it is left open. Call `codebase-design` for the options, then `grilling` and `domain-modeling`. Ask this gap's question, and stop. The human's answer closes the gap.

**Task** (HITL or AFK). Work that must finish before a decision can be made, such as requesting access. Drive it when you can. Otherwise give the human a checklist. Make no live call to a dependency. Proof is a spec or a written answer in `input/`. A chat line is not proof.

## Fog of war

Not yet specified is the **fog of war**: in-scope questions still too vague to be gaps. A question you can phrase now is a gap, even when it is blocked. A question you cannot phrase stays one line in the fog. Resolving a gap lets you phrase the next ones. Write each as a gap and delete its line. One line may become several gaps, or none.

Fog is legal while the design is in `discussion`. It must be `None.` at ready.

## Out of scope

Work past the destination is out of scope. Drop the row and add one line under Out of scope with the question and why. It never comes back into this design. Start a new design if the destination changes.

## Companion skills

Call each by its exact name with the Skill tool: `grilling`, `domain-modeling`, `research`, `codebase-design`, `writing-for-agents`, `deslop`. On each edit of `design.md`, call `writing-for-agents`. When a code snippet changes, call `deslop`. If one is not installed, say so once, point to Setup in the pack readme, and do that step by the rules in this skill.

## Invocation

The person types `/tech-design`. Read the first word after it.

| Typed | Mode |
| --- | --- |
| nothing | List, see [help.md](references/help.md) |
| `help` | Help, see [help.md](references/help.md) |
| `new <name> [types]` | Chart |
| `<name>` or `<name> grill [gap]` | Work |
| `<name> status`, `close <gap>`, `drop <gap>` | See [help.md](references/help.md) |
| `<name> ready` or `<name> ready anyway` | Ready |

Resolve one gap per session, then stop. Research gaps run beside it. Chart and ready close no gap. Every session ends by writing state to disk, see [resume.md](references/resume.md).

### Chart the map

1. **Name the destination.** Call `grilling` and `domain-modeling`. Done when the destination is one or two lines the human agrees with.
2. **Pick the size.** Propose `small`, `medium`, or `big` with reasons from [sizes.md](references/sizes.md). Record the types the person named. Done when the human confirms the size.
3. **Map the frontier.** Grill again, breadth-first. List the decisions you can phrase. If you find no fog and one session can finish the design, stop and ask how to proceed.
4. **Sweep.** Run all 18 categories in [gap-sweep.md](references/gap-sweep.md). Done when every category is a gap, a fog line, or `n/a` with a reason in `log.md`.
5. **Create the folder** by [structure.md](references/structure.md): every entry, the header at `draft`, all 14 headings. Fill the map and what is already known. Every other section holds `n/a: <reason>` or one line naming the gaps it waits on. Add a Dependencies row for each outside system.
6. **Add the gaps.** Each row starts `open`, with a category and a type. Fill `blocks` in a second pass, with real ids. A gap whose answer sits with a named person starts `waiting-on-input`, with its question sheet. Set status `discussion`.
7. **Start the research subagents**, one per research gap, by [research.md](references/research.md).
8. **Stop.** Log one line. Tell the human the size, the gap count by type, who is being waited on, and the first frontier gap.

A second `new` for a name that already exists stops. Tell them to resume.

### Work through the map

1. **Load and catch up** by [resume.md](references/resume.md): read the map, take in new files under `input/`, print the board, flag overdue gaps, start new research. If the status is `ready`, `committed`, or `abandoned`, say so and stop.
2. **Choose the gap and claim it.** Use the one the person named. Otherwise take the first frontier gap. Set `owner` before any work.
3. **Resolve it by type.** Read a related row or research file when you need it. On a grilling gap, advice comes before the one question.
4. **Record the resolution.** Write the answer on the row and set `decided`, or `accepted-risk` when the human accepts the risk you named. With no answer, set `waiting-on-input` and write the question sheet. Advice alone never closes a gap.
5. **Write it into the design.** Put the result in the section its category names. Draw any diagram the answer now triggers, by [diagrams.md](references/diagrams.md). Add the gist to Decisions so far.
6. **Update the map.** Add the gaps this answer opens, then fill `blocks`. Apply both rules at the top. Delete fog you can now phrase. Raise the size if a larger one now holds. Update rows the decision invalidates.
7. **Stop.** Write state. The session is done when that one gap is claimed and either closed or `waiting-on-input`.

Other sessions may edit this folder at the same time.

### Ready

Follow [the ready gate](references/ready-gate.md). Walk every check and print pass or fail for each. Any fail stops ready and leaves the status unchanged. When all pass, set `ready` and write `tickets.md`. `ready anyway` exists only when the human types it. Each open gap is then accepted by name, one at a time, by the human.
