---
name: delivery-loop
description: Run a substantial change through main-agent-controlled planning, implementation, verification, parallel review, and at most two fix rounds. Use for behavior changes, multi-file work, or when asked to plan and build; bypass the phase agents for an obvious small change.
license: UNLICENSED
---

# Delivery loop

The main agent controls this workflow:

```text
plan -> implement -> verify -> parallel review -> merge -> (fix -> verify -> parallel review -> merge) x 2 max -> hand off
```

No phase commits, merges, or pushes. A `pass` verdict means ready for human review, not permission to land.

## Route the request

| Work | Route |
|---|---|
| Typo, rename, or one-line change with one obvious safe check | The main agent implements and verifies directly. Do not call `loop-planner`, `loop-implementer`, `loop-verifier`, or any review agent. Do not create a plan. |
| Behavior change, more than two files, or meaningful design choice | Run the full loop. |
| Irreversible, cross-cutting, public-interface, migration, or trust-boundary change | Run the full loop and record the relevant human decision before implementation. |
| Existing diff with no confirmed plan | Use the standalone diff-review route: the main agent runs applicable reviewers in parallel and gives completed reports to `loop-orchestrator` for a ranked merge. There is no verifier report, plan write, verdict, or fix round. |

When uncertain, use the full loop. Once the small-change route is chosen, keep it small; discovering a design choice or wider impact promotes the work to the full loop before further edits.

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

Only after the user confirms the shared understanding does the main agent invoke the planner in write mode. That invocation writes exactly `docs/plans/<slug>.md`; planning evidence and citations stay in that plan.

## Run plan-bound phases

`loop-implementer` and `loop-verifier` require the confirmed plan. The implementer edits against its criteria and reports claims; the verifier independently reports evidence per criterion. The small-change route bypasses both agents.

Before implementing a fix list, verifying, reviewing, or merging, read [the review contract](references/review-contract.md). It is the only definition of severity, finding identity, report shapes, and verdict gates.

## Review in parallel, then merge

The main agent directly launches all applicable reviewers against the same diff:

| Agent | Runs when | Question |
|---|---|---|
| `loop-reviewer` | Always in the full review phase | Does the change meet the plan and remain correct? |
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
- `fix`: send the confirmed plan and only the merged fix list—not raw reviewer or verifier reports—to `loop-implementer`, then verify and review again.
- `replan`: return to the mediated planning flow with the reason the confirmed plan cannot succeed.

The initial implementation, verification, and review are round 1. Each fix increments the round, so
round 2 is the first fix round and round 3 is the second; the reported fix-round count is `round - 1`.
Allow at most two fix rounds. Before a third fix, stop and report what was tried, what remains, and why the loop is not converging.

## Worktree and hand-off

Record whether the workspace is the primary checkout, an existing worktree, or a loop-created worktree. Preserve the primary checkout and externally created worktrees. Remove a loop-created worktree only when `git status --porcelain` is empty at its exact path, from outside that directory, using `git worktree remove <exact-path>` without `--force`, then `git worktree prune`. Preserve dirty worktrees and report cleanup as pending.

The final hand-off names the plan, files changed, verification evidence, review verdict, fix-round count, deferred notes, pack status, workspace, and cleanup status. State that the result is uncommitted.
