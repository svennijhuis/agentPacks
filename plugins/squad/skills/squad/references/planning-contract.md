# Planning contract

This file defines the turn-based handoff between the main agent and `squad-planner`.

## Ownership

The main agent owns user interaction and planning state. It supplies the planner with the request, repository evidence, settled decisions, previous user answers, open frontier, and requested mode. It presents planner questions to the user and passes the answers into a later invocation.

The planner handles one invocation and returns. It never addresses the user directly, waits for an answer, or carries state that was not returned to the main agent.

Grill-style planning is this contract: frontier rounds with a recommended answer on every question.
Facts are discovered by the planner (a subagent). Decisions stay with the human. Do not load a
separate grilling skill or an external skills catalog. The main agent never grills itself — it
presents the planner's round.

Optional `docs/decisions.md` is a human-readable drop-box, not eager memory. Read it when present
and treat its entries as settled human decisions. Never create, edit, or append that file. The
plan's Decisions table still records this run.

## Planner input

Every invocation supplies:

```markdown
Mode: next-round | write-plan
Request: <user request>
Repository evidence: <paths, configuration, standards, workspace facts, and cited primary sources>
Facts to check: <numbered facts the main agent wants found, or None>
Settled decisions: <numbered decisions and rejected alternatives>
Human drop-box: <docs/decisions.md contents when that file exists, or None>
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

**Facts found:** <numbered fact, answer, and source path or citation; or None>
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

## Decision branches

Visit every applicable branch before the frontier is empty. Mark a branch inapplicable with a reason
rather than silently skipping it.

| Branch | Ask when | Prefer constraints like |
|---|---|---|
| Outcome | Always | What done looks like; what is explicitly out of scope |
| Scope / non-goals | Always on behavior change | Must not deepen obscure edges when the goal is MVP-complete |
| Interface | Public API, module, or seam changes | Two shapes; must not break named callers |
| Data | Persistence or schema | Migration / compatibility bound |
| Failure | User-visible or trust-boundary paths | Exact error contract |
| Compatibility | Existing clients or formats | Must not change wire without a version story |
| Security | Trust boundary | Deny-by-default; must not widen auth surface without a decision |
| Dependency philosophy | New package, scaffold, or "from scratch" work | Allowed libs; forbidden shortcuts; implement vs reuse |
| Performance / resources | Latency, throughput, memory, or timeouts matter | Concrete bounds; "fast enough" is not a bound |
| Architecture fitness | New subsystem or consequential seam | Shape must still work if scope grows one notch |
| Verification | Always | Exact commands; test-plan matrix |
| Rollout | Deployed or irreversible | Rollback / feature-flag bound |
| Worktree | Parallel checkout needed | Exact path; preserve primary checkout |

Recommendations prefer **constraints** ("must not…", "out of scope…", "timeout ≤ …") over long
do-lists. Do not invent performance or dependency questions when the change cannot touch them; mark
those branches inapplicable.

## Find facts yourself

Finding facts is the planner's job, never the user's. Inspect the repository, tests, Git history, and
tooling for anything they can answer. "Does this project use Vitest or Jest?" is a fact to discover;
"Should the new tests join the existing suite or use their own?" is a decision for the frontier.

Facts outside the repository come from primary sources such as official documentation, source,
standards, or specifications, and the plan cites the source beside the decision it settled. Recalled
facts without evidence remain unresolved.

Name facts, do not dig for them. The main agent runs on the user's model; the planner runs on the
`fast` tier. When the main agent needs a fact to route, answer the user, or shape the next round,
it lists that fact under `Facts to check` and lets the planner find it, instead of reading the
repository itself. The planner answers every listed fact under `**Facts found:**` with its source
before asking anything, and carries the answers forward as repository evidence. A fact it could not
find is reported as not found with what was searched, never as a question to the user.

The grill is the product of the round. **Facts found** is a prelude so the main agent does not dig;
it never replaces the frontier round. After facts, ask the whole current frontier with a
recommendation each. Only the empty-frontier confirmation round may return no decision questions.
The planner finds facts with its granted tools this turn. It does not spawn `squad-*` agents.

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

If effort is more than one session, say so and stop.

Write mode is valid only when the input includes the user's confirmation, every applicable decision branch is settled, and the open frontier is empty. Otherwise return the missing condition without writing.

On valid input, write exactly one file at `docs/plans/<slug>.md`. Do not write source code or a separate research artifact. Put repository evidence, primary-source citations, assumptions, decisions, and rejected alternatives in the plan.

The plan contains:

```markdown
# <outcome as a sentence>

## Request
<the user's ask, verbatim — not a paraphrase>

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

## Test plan matrix
| Criterion | Happy | Edge | Fail | Kind | Seam |
|---|---|---|---|---|---|
| 1 | <happy path> | <edge> | <fail> | unit or integration | <pre-agreed test seam> |

## In scope
## Out of scope
## Non-goals
## Dependency philosophy
<allowed / forbidden libs and implement-vs-reuse, or `N/A — no new dependencies`>
## Performance / resource bounds
<concrete bounds, or `N/A — not performance-sensitive`>
## Open questions
None.
## Status
planning | implementing | verifying | reviewing | fix-round <n> | fixup | hand-off
## Fix list
<rewritten each merge; empty until first review>
## Handoff notes
<rewritten each phase; deviations, concerns, open risks>
## Verification
<exact commands or a criterion saying which check must be created>
```

`## Status`, `## Fix list`, and `## Handoff notes` are **run scratch**: the main agent and
`squad-orchestrator` rewrite those sections in place during the loop. Do not append forever inside
the plan. Learnings stay append-only elsewhere.

`## Request` is the user's words as typed; the title is the planner's outcome sentence. Do not expand
or narrow the ask there — scope belongs in `## In scope`, `## Out of scope`, and `## Non-goals`. It is
there so a later reader, human or `/squad-review` on the same branch, can see drift between the ask
and the plan without the chat transcript.

An open question is never converted into an assumption or acceptance criterion. Completion means the user confirmed the shared understanding, `## Open questions` is exactly `None.`, and the planner returns the written plan path to the main agent.

Each acceptance criterion is observable and can fail. "Handles errors well" is not a criterion;
"returns 400 naming the missing field" is. State what is out of scope so review can distinguish a
deliberate boundary from an omission. Give the exact verification commands; when a check does not
exist yet, making that check is itself planned work.

A criterion that states a number — a bound, count, size, or latency — is **quantitative**. Write it
as metric, operator, bound (`p95 ≤ 200 ms`, `bundle ≤ 2.4 MB`) and name the command that measures
it under `## Verification`. The verifier re-measures and quotes `<measured> <op> <bound>`. A green test name is not a measurement.

Every business criterion in `## Acceptance criteria` requires one `## Test plan matrix` row: a
happy path, an edge case, a failure case, whether that check is a unit or an integration test,
and a named Seam — the pre-agreed test surface. Blank or unconfirmed Seam is `not verified`,
never `pass`. Tests that hit internals not named in the Seam column fail.
The implementer TDDs that matrix. The verifier proves the matrix is covered; happy-path-only
coverage is rejected. Deployed/API smoke cites
[smoke-matrix](../../../references/smoke-matrix.md): happy/edge/fail/auth/timeout/5xx. Kind is
`smoke`. URLs and secrets stay in the app repo.

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
