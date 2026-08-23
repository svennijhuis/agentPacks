# Planning contract

This file defines the turn-based handoff between the main agent and `loop-planner`.

## Ownership

The main agent owns user interaction and planning state. It supplies the planner with the request, repository evidence, settled decisions, previous user answers, open frontier, and requested mode. It presents planner questions to the user and passes the answers into a later invocation.

The planner handles one invocation and returns. It never addresses the user directly, waits for an answer, or carries state that was not returned to the main agent.

## Planner input

Every invocation supplies:

```markdown
Mode: next-round | write-plan
Request: <user request>
Repository evidence: <paths, configuration, standards, workspace facts, and cited primary sources>
Settled decisions: <numbered decisions and rejected alternatives>
Previous user answers: <latest answers, or None>
Open frontier: <known open decisions and dependencies>
Plan path: docs/plans/<slug>.md
User confirmation: <exact confirmation, required only for write-plan>
```

Missing evidence is explicit. The planner may inspect the repository or primary sources to fill factual gaps; it must not turn discoverable facts into user questions.

## Ask the frontier, in rounds

The **frontier** is every decision whose prerequisites are already settled: the questions that can be
asked now without guessing at an answer that has not been received yet.

Ask the whole frontier in one round. Number the questions and give a recommended answer to each. A
question whose answer depends on another question still open belongs to a later round. Every question
carries a recommendation so the user can confirm, correct, or choose differently without doing the
planner's analysis themselves.

The planner returns exactly one round and stops. The main agent presents that round to the user and
waits for the answers. It then supplies those answers and the returned state to a new planner
invocation. The planner itself never waits across turns or assumes an answer.

The user's answers reshape the decision tree. Settled decisions push the frontier outward and unblock
their dependants. Recompute the tree before returning the next round.

### `next-round` output

Return exactly one planning round and stop:

```markdown
## Planning round <n>

**Settled:** <compact numbered summary, or None>

❓ **Q1 — <title>:** <decision, viable options, and material trade-offs>
➡️ **Recommendation:** <choice and reason>

---

❓ **Q2 — <title>:** <...>
➡️ **Recommendation:** <...>

**State for next invocation**
- Settled decisions: <complete carried state>
- Inapplicable branches: <branch and reason>
- Open frontier: <remaining decisions and dependencies, or Empty>
```

Design consequential public interfaces, module boundaries, and seams in at least two viable shapes
before recommending one.

## Find facts yourself

Finding facts is the planner's job, never the user's. Inspect the repository, tests, Git history, and
tooling for anything they can answer. "Does this project use Vitest or Jest?" is a fact to discover;
"Should the new tests join the existing suite or use their own?" is a decision for the frontier.

Facts outside the repository come from primary sources such as official documentation, source,
standards, or specifications, and the plan cites the source beside the decision it settled. Recalled
facts without evidence remain unresolved.

A fact still being researched is an unsettled prerequisite only for decisions downstream of that
fact. Ask the rest of the current frontier now rather than blocking the whole round on one lookup.

## Stop when the frontier is empty

Planning ends only after every applicable branch has been visited and nothing remains silently
assumed. When the frontier becomes empty, return one final `next-round` response that states the
complete shared understanding as a numbered list and asks the user to confirm it. The main agent
presents that confirmation round and waits; the planner still writes nothing.

If the user says to stop asking and decide, treat that as an answer: use the planner's recommendations
as the chosen decisions and record that the user accepted them. This is an explicit decision, not
permission to silently fill unresolved branches.

## Write the plan

Write mode is valid only when the input includes the user's confirmation, every applicable decision branch is settled, and the open frontier is empty. Otherwise return the missing condition without writing.

On valid input, write exactly one file at `docs/plans/<slug>.md`. Do not write source code or a separate research artifact. Put repository evidence, primary-source citations, assumptions, decisions, and rejected alternatives in the plan.

The plan contains:

```markdown
# <outcome as a sentence>

## Problem
## Decisions
| # | Decision | Chosen | Why |
|---|---|---|---|

## Workspace
## Standards in force
| Standard | Source |
|---|---|

## Repository conventions observed
| Convention | Evidence |
|---|---|

## Assumptions
## Acceptance criteria
1. <observable, testable statement>

## In scope
## Out of scope
## Open questions
None.
## Verification
<exact commands or a criterion saying which check must be created>
```

An open question is never converted into an assumption or acceptance criterion. Completion means the user confirmed the shared understanding, `## Open questions` is exactly `None.`, and the planner returns the written plan path to the main agent.

Each acceptance criterion is observable and can fail. "Handles errors well" is not a criterion;
"returns 400 naming the missing field" is. State what is out of scope so review can distinguish a
deliberate boundary from an omission. Give the exact verification commands; when a check does not
exist yet, making that check is itself planned work.

The plan lives in the file. The implementer builds from it, the verifier checks it, and reviewers
measure the change against it.

## The rule that outranks the rest

**An open question is never a criterion.** Return it to the frontier. Never replace it with the
planner's unconfirmed guess or soften it into wording that can pass under several incompatible
answers. A vague criterion can make the loop verify and approve the wrong behavior.

An assumption is valid only when the answer genuinely cannot be resolved before implementation,
such as a value that exists only at runtime. Record it under `## Assumptions`, in the user's words
when available, with enough detail for a reviewer to challenge it. An assumption never substitutes
for an answer that the repository, a primary source, or the user can provide.
