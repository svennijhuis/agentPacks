# Help, list, status, close, drop

These modes let the human see where a design stands and steer it. Help, list, and status change no file.

## Help

On `/tech-design help`, print this card and stop.

```text
/tech-design                        list every design with its state
/tech-design new <name> [types]     chart a new design. types: web api data integration
/tech-design <name>                 resume: show the board, then work one gap
/tech-design <name> grill [gap]     work one gap by question and answer
/tech-design <name> status          the board and what you need to do. changes nothing
/tech-design <name> close <gap>     you give the answer, the gap closes
/tech-design <name> drop <gap>      move a gap out of scope, with a reason
/tech-design <name> ready           strict gate. refuses while anything is open
/tech-design <name> ready anyway    ready with open gaps you accept one by one
/tech-design help                   this card
```

`<name>` matches the folder name with or without its date. `<gap>` is an id or a few words of the question.

## List

On `/tech-design` with no argument, read the header and `gaps.md` of every folder under `docs/design/`. Print one row per design, newest `Updated` first, and stop.

```text
Design              Size    Types        Status      Created     Updated     Open  Waiting
2026-03-02-billing  medium  api, data    discussion  2026-03-02  2026-03-09  4     2 (1 overdue)
```

With no designs, say so and show the `new` line from the card.

## Status

On `/tech-design <name> status`, print the board, then the to-do list, and stop. Change nothing.

### The board

```text
<name>   Status: discussion   Size: medium   Types: api, data
Created 2026-03-02   Updated 2026-03-09   Owner: <person>

Gaps: 14 total. 8 decided, 1 accepted-risk, 3 open, 2 waiting.
Fog:  2 lines under Not yet specified.
Sections filled: 9 of 14.
```

### What you need to do

Group the open work by who must act. Leave out a group that is empty. Name each gap by its question.

```text
Needs your answer (3)
  G-007  Which roles may refund an order?
Waiting on others (2)
  G-004  How does the billing service authenticate callers?   Sam, asked 2026-03-03, due 2026-03-06, OVERDUE
         sheet: questions/sam.md
The agent can do alone (1)
  G-011  What are the vendor rate limits?   research
Blocked (1)
  G-012  How are refunds retried?   waits on G-004
Next: /tech-design <name> grill G-007
```

End with one line saying how far ready is: the count of gaps and sections still to close.

## Close

On `/tech-design <name> close <gap>`, the human closes a gap directly.

1. Find the row. If the words match more than one, list them and ask which.
2. If the human gave no answer with the command, ask for it once.
3. Write the answer on the row. Set `decided`. Clear the waiting fields.
4. Write the result into the `design.md` section its category names, and add the gist to Decisions so far.
5. Add any gap the answer opens. Log one line. Stop.

Only the human's own answer closes a gap this way. Do not fill it in for them.

## Drop

On `/tech-design <name> drop <gap>`:

1. Ask for the reason if none was given.
2. Remove the row from `gaps.md`. Add one line under Out of scope: the question and the reason.
3. Clear its id from every `blocks` cell. Log one line. Stop.

Drop a gap only when it sits past the destination. A gap that is in scope but unanswered is not dropped. It is closed, or accepted as a risk at ready.
