---
name: delivery-loop
description: User-invoked orchestrator for /squad or /build, and /review. Mediates grill-style planning, runs plan-bound implementation and verification, fans reviewers on two axes, and caps two fix rounds. Do not model-invoke; type the entrypoint.
license: UNLICENSED
disable-model-invocation: true
---

# Delivery loop

User-invoked thin orchestrator. Two entrypoints only: `/squad` or `/build` (same command), and
`/review`. Type the command. Do not wait for the model to pick it.

## Locked v1 flow

```text
/squad or /build (user-invoked orchestrator)
1. Read learnings.md (append-only)
2. Orient codebase (applicable stacks only)
3. Small change? → main agent only, spawn nobody → verify → append learnings → hand off uncommitted
4. Else grill/plan rounds (facts via subagent; decisions = human) → write plan
5. Gate spins: implementer → verifier → reviewers in parallel (correctness + plan/spec; security ONLY if trust boundary)
6. Orchestrator merges ≤2 fix rounds → hand off uncommitted → append learnings

/review
Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → append learnings

Always
models.source.json tiers (default inherit); load only contracted <lang>-* by Skill name; coworker docs = real dotnet test/validate on a fixture.

Not in v1
second skill pack, Matt catalog dump, eager fan-out, self-improve graphs, auto skill rewrite, redoing PR #6.
```

Step 1 is **Read and apply**. Apply the latest same-entrypoint entry (`/squad` and `/build` are the same). Prefer
its skips only when that run **passed**. A skip from a **failed** run is a must-run this time.
Prefer its model tier after a pass. After a fail, demote one tier (`frontier` → `standard` →
`fast` → `inherit`). Do not rewrite skills. Security also runs when learnings mark it must-run.

No phase commits, merges, or pushes. A `pass` verdict means ready for human review, not permission to land.

## Route the request

| Work | Route |
|---|---|
| Typo, rename, or one-line change with one obvious safe check | The main agent implements and verifies directly. Do not call `loop-planner`, `loop-implementer`, `loop-verifier`, or any review agent. Do not create a plan. Small changes spawn nobody. |
| Behavior change, more than two files, or meaningful design choice | Run the full loop. |
| Irreversible, cross-cutting, public-interface, migration, or trust-boundary change | Run the full loop and record the relevant human decision before implementation. |
| Existing diff with no confirmed plan | `/review`: PR, uncommitted, or vs main. Applicable reviewers in parallel; `loop-orchestrator` merges. No verifier report, plan write, verdict, or fix round. |

When uncertain, use the full loop. Once the small-change route is chosen, keep it small; discovering a design choice or wider impact promotes the work to the full loop before further edits.

Cost-first: keep every agent on `inherit` unless it is the implementer writing code, which may use
`standard`. Do not fan out specialists the route does not need.

## Load skills by exact name

This skill gates planners and reviewers. Load a skill only with the Skill tool and the exact name:

| Name | When |
|---|---|
| `delivery-loop` | This orchestrator (already loaded when the user invoked `/build` or `/review`) |
| `<lang>-build` | Implementer and simplifier, for each applicable stack |
| `<lang>-test-patterns` | Implementer and verifier, for each applicable stack |
| `<lang>-review` | Correctness reviewer, when the optional slot exists |
| `<lang>-security-review` | Security reviewer, when the gate ran and the optional slot exists |

Never write slash-prose (a slash plus the skill name) to load a skill. That looks like a slash
command and is not how the Skill tool resolves. A near-miss name is a silent miss.

Do not load an external grilling catalog or any skill that is not in the table above. Grill-style
planning is the frontier rounds inside this skill's planning phase.

Language-pack slot skills are internals (`metadata.audience: loop`). Do not present them as
user-facing entrypoints.

## Prepare the full loop

For a full loop, use `pack-check` status when available. Otherwise invoke `pack-check`; if it is
unavailable, detect .NET from `*.slnx`, `*.sln`, or `*.csproj` and Rust from `Cargo.toml`, then report
missing language slots without inventing standards. The small-change route may report existing pack
status but does not invoke `pack-check` as a prerequisite.

Select applicable stacks from target paths, the existing diff, and acceptance criteria. A Rust-only
scope loads Rust slots, a .NET-only scope loads .NET slots, and a cross-language scope loads both.
When a mixed repository's scope cannot safely distinguish them, load both. Detection alone does not
make a stack applicable: never request or load a pack for code outside the change.

Required language slots are `<lang>-build` and `<lang>-test-patterns`; `<lang>-review` and
`<lang>-security-review` are optional. Resolve them for every applicable stack and group missing packs
into one approval round. For a full loop, a missing required slot stops planning unless the user
explicitly chose `--no-pack`. An approved installation stops for a client reload. The small-change
route may continue after reporting the gap.

Record repository evidence, standards, and workspace ownership. Repository conventions require configuration or a repeated local pattern. A conflict between repository evidence and a plugin standard is a planning decision, not something to resolve silently.

## Plan through the main agent

Read [the planning contract](references/planning-contract.md). The main agent invokes `loop-planner` once per turn, presents the returned numbered round to the user, and passes the answers plus settled state into the next invocation. The planner never talks to the user or waits for answers itself.

Grill stays here: one frontier round at a time, each question with a recommended answer. Facts are
the planner's job (a subagent inspects the repository). Decisions are the human's. Do not open a
separate grilling skill.

Only after the user confirms the shared understanding does the main agent invoke the planner in write mode. That invocation writes exactly `docs/plans/<slug>.md`; planning evidence and citations stay in that plan. The plan's Decisions table is the drop-box; do not create `decisions.md`.

## Run plan-bound phases

`loop-implementer` and `loop-verifier` require the confirmed plan. The implementer stays tiny: failing
test first, then the change, then stop. It does not run the full suite. The verifier independently
reports evidence per criterion and runs the wider suite once. The small-change route bypasses both agents.

Before implementing a fix list, verifying, reviewing, or merging, read [the review contract](references/review-contract.md). It is the only definition of severity, finding identity, report shapes, and verdict gates. Verification pass/fail is the evaluator in that contract, not a hopeful reading of the report.

## Review in parallel, then merge

The main agent directly launches all applicable reviewers against the same diff. Dual-axis like a
Matt-style code review: correctness and plan/spec. Security only when gated.

| Agent | Runs when | Question |
|---|---|---|
| `loop-reviewer` | Always in the full review phase | Is it correct, and does it match the plan or spec? |
| `loop-simplifier` | Always in the full review phase | Is the implementation needlessly complex? |
| `loop-security-reviewer` | The change touches a trust boundary, or the main agent is unsure | Can the change be abused? |

Trust boundaries include authentication, authorization, untrusted input, file paths, shell commands, cryptography, dependencies, deserialization, outbound requests, and credentials. Record why security ran or was skipped.

After all reviewer reports complete, the main agent sends those reports, verifier evidence, round number, plan path, and the security-gate decision to `loop-orchestrator`. The orchestrator only validates, deduplicates, ranks, assigns the verdict, and appends the merged fix list. It does not launch agents or route subsequent work.

If the orchestrator returns an input error, surface it unchanged to the human and end the current
loop. Do not obtain another report, invoke merge again, write or amend the plan, assign a verdict,
start a fix round, or route work to another agent. A later continuation requires an explicit new
user request.

The main agent routes the result:

- `pass`: allowed only when every criterion has adequate verifier evidence and the merged report has no blocking finding; hand off to the human.
- `fix`: send the confirmed plan and only the merged fix list—not raw reviewer or verifier reports—to a **fresh** `loop-implementer` invocation. The author of the rejected code is not the fixer; the reviewer never edits. Then verify and review again.
- `replan`: return to the mediated planning flow with the reason the confirmed plan cannot succeed.

The initial implementation, verification, and review are round 1. Each fix increments the round, so
round 2 is the first fix round and round 3 is the second; the reported fix-round count is `round - 1`.
Allow at most two fix rounds. Before a third fix, stop and report what was tried, what remains, and why the loop is not converging.

## Worktree and hand-off

Record whether the workspace is the primary checkout, an existing worktree, or a loop-created worktree. Preserve the primary checkout and externally created worktrees. Remove a loop-created worktree only when `git status --porcelain` is empty at its exact path, from outside that directory, using `git worktree remove <exact-path>` without `--force`, then `git worktree prune`. Preserve dirty worktrees and report cleanup as pending.

The final hand-off names the plan, files changed, verification evidence, review verdict, fix-round count, deferred notes, pack status, workspace, and cleanup status. State that the result is uncommitted.

Then append one entry to the append-only `docs/learnings.md` using [the learnings contract](references/learnings.md).
Do not rewrite earlier entries. Do not invent a skill or graph from the log.
