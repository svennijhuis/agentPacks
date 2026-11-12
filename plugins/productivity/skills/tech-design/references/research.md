# Research

Research finds facts. It never makes a decision. A research gap is worked by a subagent with a fresh context, alone, while the human does something else.

## When a gap is research

Set type `research` when any trigger holds:

| Trigger | Example |
| --- | --- |
| The answer is a fact, not a preference | "Does the vendor API support webhooks?" |
| The fact lives outside this repo | Vendor docs, limits, pricing, how a library behaves |
| The fact lives in this repo but needs a wide read | "How do the existing services authenticate to each other?" |
| Advice needs two or more options compared with sources | Queue A against queue B |
| A dependency contract field is empty and public docs exist | Rate limits, auth method |
| The human answers "I don't know" and a source could answer | The grilling gap becomes a research gap |

A gap is not research when:

- The answer is a preference or a business decision. That is `grilling`.
- Only a person can answer. That is `waiting-on-input`, see [resume.md](resume.md).
- Someone must do something first, such as request access. That is `task`.

A fact you can read in one or two files is not a gap at all. Read it and write it down.

## When subagents start

1. **End of chart.** Start one subagent per research gap, all at once.
2. **Start of a work session.** Start one subagent for each research gap added since the last session and still `open`.
3. **During a session.** When resolving a gap turns up a research gap, add the row and start its subagent before you stop.

Subagents never block the session. Go on with the gap in hand. Read a finished research file when a later gap needs it.

## The brief

Give each subagent this, and nothing from the conversation:

```markdown
Question: <the gap question, word for word>
Gap: <id>, in docs/design/<folder>/gaps.md
Destination: <the Destination lines from design.md>
Why it matters: <the decision that waits on this fact>
Look in: <repo paths, or the kind of outside source>
Write: docs/design/<folder>/research/<gap-id>-<topic>.md
Do not: decide, recommend a product choice as final, call a live system, edit design.md
```

The subagent claims the gap by setting `owner` and status `research`, then calls `research` when that skill is installed.

## The research file

`research/<gap-id>-<topic>.md`, always in this shape:

```markdown
# <gap question>

Gap: <id>
Date: <yyyy-mm-dd>

## Answer

<two or three lines. The fact, stated plainly.>

## Facts

- <fact>. Source: <link or file path>. Confidence: <high | medium | low>.

## Options

<only when the question compares options: one row per option, with cost and risk>

## Still unknown

- <what no source answered, and who could answer it>
```

## Closing a research gap

- When the Answer is backed by a source, the subagent writes the Answer on the row, links the file, and sets `decided`. This is the only gap an agent may close alone.
- Each line under Still unknown becomes a new gap. A person named there makes it `waiting-on-input`.
- A decision that waited on the fact stays `open`. Research feeds the advice. The human still decides.
- Confidence `low` on the Answer does not close the gap. Leave it `research` and say what is missing.
