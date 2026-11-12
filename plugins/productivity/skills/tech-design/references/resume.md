# Stop and resume

A design takes days or weeks. Some gaps wait on a person. Every session must be safe to stop, and every new session must be able to start cold from the files alone.

## Waiting on a person

When a gap cannot close because someone else has the answer:

1. Set status `waiting-on-input`. Fill `waiting on` with the person, `asked` with today, and `due` with the date the human names. If they name none, ask for one.
2. Append the question to `questions/<person>.md`, see below.
3. Tell the human the sheet is ready to send. Do not send it yourself.
4. Go on. A waiting gap blocks only the rows that list it under `blocks`. Take the next frontier gap, or stop.

A design may hold any number of waiting gaps for any length of time.

## The question sheet

`questions/<person>.md` is written so the human can paste it into a chat or a mail without editing. The person reading it has not seen the design.

```markdown
# Questions for <person>

Design: <name>. <the Destination in one line.>

## Open

### <gap id>: <question>

Asked: <yyyy-mm-dd>. Needed by: <yyyy-mm-dd>.
Why we ask: <the decision that waits on this, in one line>
What we think: <the recommended answer, or "no view yet">
Answer with: <the exact form needed: a yes or no, a number with unit, a document, a name>

## Answered

### <gap id>: <question>

Answered: <yyyy-mm-dd>. See input/<file>.
```

Move a question from Open to Answered when its gap closes.

## When the answer comes back

The human pastes the answer, or drops a file. Then:

1. Save it under `input/<yyyy-mm-dd>-<person>-<gap id>.md`, word for word, with who said it and when.
2. If it answers the question, write the answer on the row, set `decided`, and clear `waiting on`, `asked`, and `due`.
3. If it answers part, close what it answers and add a new gap for the rest.
4. If it raises new questions, add them as gaps.

An answer the human relays from memory counts. Record it in `input/` with "relayed by <human>".

## End of every session

Before you stop, in any mode, write state to disk. Nothing may live only in the chat.

1. The gap row holds its new status and, when closed, its answer.
2. `log.md` has one new dated line.
3. `Updated` in the header is today.
4. The last thing you say is three lines: what changed, what waits on whom, and the next frontier gap by its question.

## Start of every work session

1. Read the header, the map, `gaps.md`, and the last five `log.md` lines. Do not read `research/` or `input/` yet.
2. Check `input/` for files newer than the last log line. Process each as above.
3. Print the board from [help.md](help.md).
4. Flag every waiting gap past its `due` date and ask the human once: chase, extend the date, or accept the risk.
5. Start research subagents for new research gaps, see [research.md](research.md).
6. Then choose one gap.

## More than one design

Each design is its own dated folder. Sessions never mix them. When the human gives no name and more than one design is not `committed` or `abandoned`, list them and ask which.
