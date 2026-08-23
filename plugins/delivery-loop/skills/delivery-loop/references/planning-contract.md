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

## `next-round` output

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

Ask only the current frontier: decisions whose prerequisites are settled. Design consequential public interfaces, module boundaries, and seams in at least two viable shapes before recommending one. Every question includes a recommendation.

When the frontier becomes empty, the round asks the user to confirm the complete shared understanding. It still returns without writing a file.

## `write-plan` behavior

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
