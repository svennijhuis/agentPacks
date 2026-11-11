# Ready gate

Read every row in `gaps.md`.

Stop, and do not set `ready`, when any row is `open`, `research`, or `waiting-on-input`. List those rows by question.

`decided` and `accepted-risk` do not block. A folder with no `decided` or `accepted-risk` row is not ready. Empty sections are not ready.

When the gate passes, set status `ready`. Then run this order:

1. `interrogate` (TD2+) over the whole design.
2. `technical-writing` (TD2+).
3. `unslop` (TD2+), then the no-op pass. Delete every line that does not change what the reader does.
4. `to-spec` (TD2+), then `to-tickets` (TD2+).

Call `writing-for-agents` on the `design.md` edit. Call `deslop` when `openapi.yaml` or a code snippet changes.

Write `tickets.md` with buildable slices whose blockers are sized for story points. Write `wayfinder-brief.md` with the destination, decided gaps by question, accepted risks, and links to `design.md` and to `openapi.yaml` when it exists.

Append one `log.md` line. Stop. Ready closes nothing.
