# Gap types

Read the type column. The question does not name the type. Columns: id, question, type, status, owner, due, waiting-on note, source or answer, blocks.

The question fits one session and ends with a done-when. Link assets from the row. Do not paste them.

Set `owner` to the person driving the map before any work. An open row with an empty owner is unclaimed.

`blocks` lists the ids that must be `decided` or `accepted-risk` first. An empty `blocks` cell is unblocked. The frontier is status `open`, unblocked, and no owner.

Statuses: `open`, `research`, `waiting-on-input`, `decided`, `accepted-risk`.

This session sets `research` while it studies the row. The human or the dependency owner sets `decided` by an answer you save. They set `accepted-risk` only when they accept the risk you named. Saved input moves `waiting-on-input` back to `research`, or to `decided` when that input answers the question.

HITL means the human speaks for themselves. AFK means the agent drives the work alone. A HITL row closes through that exchange.

## Research

AFK. Use when the fact is outside this working tree.

The subagent sets `owner`. It names `how` (TD2+) on the code, then calls the Skill tool with `research` on the docs, and reads `input/`. It writes `research/<topic>.md` and links that file from the row. When the fact is in that file, set the row `decided`.

## Prototype

HITL. Use when the question is how it should look or behave.

Make a cheap artifact. Link it from the row. The human's reaction is the answer. This pack does not pin a prototype skill.

## Grilling

HITL. The default. Ask this row's question once. Then stop.

The human answers it. Research first: `how` (TD2+) on the code, then `research` on the docs, then `input/`. Fill [the advice block](advice-block.md) before the question. Then call the Skill tool with `grilling` and `domain-modeling`. `domain-modeling` updates `glossary.md` when a term is in play. The question includes the recommended option and why.

## Task

HITL or AFK. Work that unblocks a decision, when there is nothing to decide, prototype, or research, and the discussion waits on it.

Drive it when you can. Otherwise hand the human a checklist. The answer records what was done and the facts later gaps need. Make no real call to a dependency. Proof is the owner's spec, or their written answer in `input/`. Chat text is not proof. An empty proof field stays a blocking gap. The dependency-contract playbook is TD2+.

## No answer

`to-questionnaire` (TD2+) writes the question in `input/`. Set `waiting-on-input`, an owner, and a due date. Record the deadline in `design.md`.

Do not add a row you cannot phrase. That work stays under Not yet specified.
