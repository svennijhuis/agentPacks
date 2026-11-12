# Ready gate

`design.md` is the spec. Ready proves a developer can build from it without guessing, then writes `tickets.md`. Ready writes nothing else and closes no gap.

## How to run the gate

Walk every check below, in order. Print one line per check: `pass` or `fail`, and for a fail the file, the line, and what is wrong. Run all checks even after a fail, so the human sees the whole list once.

A check with nothing to check is a fail, not a pass. No requirements, no acceptance criteria, or a required diagram that is not drawn fails every check that reads them. Write `fail: nothing to check`.

Any fail stops ready. Leave the status unchanged. Each fail that is a missing fact or decision becomes a gap row. End by saying how many checks failed and what to run next.

## Checks

### Structure

1. The folder holds every entry in [structure.md](structure.md).
2. The header has every field, in order, with a value.
3. All 14 sections are present, in order, with the exact headings.
4. No section is blank. Every `n/a` has a reason. No `n/a` sits in a section the size forbids, see [sizes.md](sizes.md).
5. `design.md` is within the line budget for its size.

### Gaps

6. No row in `gaps.md` is `open`, `research`, or `waiting-on-input`.
7. Every `accepted-risk` row has an owner and names the risk.
8. Not yet specified reads `None.`
9. The sweep in [gap-sweep.md](gap-sweep.md), run again against the finished document, finds no new gap.

### Wording

10. No placeholder remains: `TBD`, `TODO`, `???`, or an unfilled `<...>`.
11. No vague word stands without a number on a requirement or criterion line: fast, scalable, robust, secure, intuitive, user-friendly, efficient, appropriate, as needed, etc.
12. Every term used in two places means the same thing, and is in `glossary.md`.
13. No em dash or en dash.

### Requirements

14. Every requirement has an `R-` id, one behaviour, and MUST in capitals. No "should".
15. Every number in a requirement is tagged `measured` or `decided`. None is `guessed`.
16. Every `R-` id is cited by at least one `AC-`. Every `AC-` cites at least one `R-` and names its test kind.
17. Every acceptance criterion can fail: it names an observable result.

### Design

18. Every diagram whose trigger fires is present, see [diagrams.md](diagrams.md).
19. Every diagram has `accTitle`, a lead-in sentence, labelled one-way arrows, and a text twin that agrees with it.
20. Every arrow that crosses a boundary carries an auth level.
21. Every sequence diagram for type `api` or `integration` shows a failure path.
22. Every actor in a diagram is in Actors. Every outside system in a diagram is in Dependencies.
23. Every Dependencies row has every field filled and a proof file, see [dependency-contract.md](dependency-contract.md).
24. Every outside call and every dependency has a Failure modes row.
25. Alternatives includes "do nothing". Assumptions and risks has an owner on every row.
26. The type requirements in [sizes.md](sizes.md) are met for each type in the header.

## Ready anyway

`/tech-design <name> ready anyway` lets the human go ready while gaps are open. Use it only when the human types it. Never suggest it. Never accept a risk for them.

1. Run the whole gate first and print it.
2. Checks 1 to 5 and 10 to 26 still stop ready. Only the gap checks 6, 8, and 9 can be accepted.
3. For each open, research, or waiting gap, and each fog line, show: the question, what a builder would have to guess, and what goes wrong if the guess is wrong.
4. Ask the human to accept each one by name, one at a time. "Accept all" is not an answer.
5. For each accepted gap, set `accepted-risk`, with the human as owner, today's date, and the risk in the answer cell. A fog line they accept becomes a gap row first.
6. A gap they do not accept stops ready.
7. Add `Open at ready: <n>` to the header. Add a section `## Open at ready` after Sign-off: one line per accepted gap with the question, the risk, the owner, and the date.

## After the gate passes

1. Set `Status: ready` and `Updated`.
2. Write `tickets.md`.
3. Append one `log.md` line. Stop.

Ready is done when `tickets.md` exists and no gap status changed, except those the human accepted.

## Ticket

One ticket is one slice a developer can build and check alone. Every ticket in `tickets.md` has:

- a title
- an anchor link to the `design.md` section it comes from, such as `design.md#flows`
- the `R-` and `AC-` ids it delivers
- acceptance criteria, copied from those `AC-` lines
- story points
- blockers: the tickets that must land first

Every `R-` id is delivered by at least one ticket. A ticket that touches a gap listed under Open at ready is marked `blocked: <gap question>` and is not started until that gap closes.
