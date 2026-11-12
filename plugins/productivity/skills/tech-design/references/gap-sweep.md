# Gap sweep

Grilling finds the gaps the human thinks of. The sweep finds the ones nobody thought of. Run it at chart, after the breadth-first grill, and again at ready.

## The rule

An unknown is a gap. Never write it as an assumption, a default, or a sentence that reads the same under two different answers. If the design needs a fact or a decision and no source gives it, add a row to `gaps.md`.

## How to sweep

Take each category in order. Ask its question against the destination and everything said so far. Each category ends in exactly one of three ways:

1. One or more gap rows, when you can phrase the question.
2. One line under Not yet specified, when you can tell a question is coming but cannot phrase it yet.
3. One `log.md` line `sweep <category>: n/a: <reason>`, when the category does not apply.

Find facts yourself before you add a gap. Read the code and the docs in the repo. A fact you can read is not a gap. A fact that needs a wide read or a source outside the repo is a research gap, see [research.md](research.md).

The sweep is done when all 18 categories have one of the three endings.

## Categories

| # | Category | Ask | Answers land in |
| --- | --- | --- | --- |
| 1 | actors | Is every human and every system that touches this named, with its permission level? | Actors |
| 2 | scope | What would a reasonable reader expect that this will not do? | Goals and non-goals, Out of scope |
| 3 | functional | Does every trigger have a stated response, including the unwanted inputs? | Requirements |
| 4 | data model | For each entity: who owns it, how many, how long kept, how it migrates? | Design, Data model |
| 5 | data flow | Can each piece of data be traced from source to sink, and what crosses each boundary? | Design, Flows |
| 6 | interfaces | Does every call have exact inputs, outputs, errors, idempotency, and version? | Design, Boundaries and contracts |
| 7 | state | Is every status and transition listed, and what happens on an illegal one? | Design, State |
| 8 | failure | For each step and dependency: what fails, how is it detected, what does the user see? | Failure modes |
| 9 | concurrency | What happens on duplicate, out of order, and simultaneous requests? | Failure modes, Requirements |
| 10 | security and privacy | Who may do what, how is each caller authenticated, which data is personal or sensitive? | Cross-cutting |
| 11 | performance and limits | Does every quality need have a number, a unit, and a way to measure it? | Requirements |
| 12 | rollout | How is it enabled, disabled, rolled back, and enabled again? | Cross-cutting |
| 13 | observability | Which signal says it works, and which says roll back? | Cross-cutting |
| 14 | testing | Which test proves each requirement, and at which level? | Acceptance criteria, Cross-cutting |
| 15 | dependencies | For each outside system: is every field of its contract filled? | Dependencies |
| 16 | assumptions | What is being taken as true without a source, and who owns checking it? | Assumptions and risks |
| 17 | business outcome | Which measure moves, by how much, and when is it read? | Summary, Goals and non-goals |
| 18 | terminology | Does each term mean one thing everywhere? | `glossary.md` |

## Gaps that are always added

Add these rows without asking whether to, as soon as the condition holds:

- **Outside system named.** Any system, source, service, or vendor outside this codebase gets a row in the Dependencies table. Each empty field in that row becomes its own gap. See [dependency-contract.md](dependency-contract.md). How to connect and how to authenticate are never left for the builder to find out.
- **Boundary crossed.** Any arrow in a diagram that crosses a boundary needs an auth level. An unknown auth level is a gap in category security and privacy.
- **Guessed number.** Any number tagged `guessed` is a gap in category performance and limits.
- **Person named as the source.** When the human says someone else knows, add the gap with status `waiting-on-input` and that person in `waiting on`. See [resume.md](resume.md).
- **Status word used.** When something is said to have statuses or stages, add a gap in category state for the full list of transitions.

## Sweep again at ready

At ready, run the 18 questions against the finished `design.md`. Any question the document cannot answer is a new gap, and ready stops.
