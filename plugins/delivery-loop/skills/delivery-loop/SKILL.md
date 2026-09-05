---
name: delivery-loop
description: User-invoked orchestrator for /squad or /build, and /review. Mediates grill-style planning, runs plan-bound implementation and verification, fans reviewers on two axes, and caps two fix rounds. Do not model-invoke; type the entrypoint.
license: UNLICENSED
disable-model-invocation: true
---

# Delivery loop

Two entrypoints only: `/squad` or `/build` (same command), and `/review`. Type the command.

## Locked v1 flow

```text
/squad or /build (user-invoked orchestrator)
1. Read and apply learnings.md (append-only): prefer passed skips/tiers; avoid what failed
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

Step 1 is **Read and apply**. Apply the latest same-entrypoint entry. Prefer passed skips and
tiers. A **failed** skip is a must-run. Fail demotes one tier. Do not rewrite skills. No graph.
`docs/learnings.md` is append-only.

No commits, merges, or pushes. `pass` is ready for human review, not permission to land.

## Route

| Work | Route |
|---|---|
| Typo, rename, one-line change | The main agent implements and verifies directly. Do not call `loop-planner`, `loop-implementer`, `loop-verifier`, or any review agent. Do not create a plan. Small changes spawn nobody. |
| Behavior or design change | Full loop. |
| Trust-boundary / irreversible | Full loop; record the human decision first. |
| Existing diff, no plan | `/review`. No verifier report, plan write, verdict, or fix round. |

Cost-first: default `inherit`. `loop-implementer` is `standard`. Other loop agents are `fast`.

## Skills

Load only with the Skill tool by exact name: `delivery-loop`, `<lang>-build`,
`<lang>-test-patterns`, `<lang>-review`, `<lang>-security-review`. Never write slash-prose.
Language-pack slots are internals (`metadata.audience: loop`).

Grill stays here. Do not load an external grilling catalog.

## Stacks

Select from target paths, the existing diff, and acceptance criteria. Rust-only loads Rust.
.NET-only loads .NET. A cross-language scope loads both. never request or load a pack for code outside the change.
Required slots: `<lang>-build`, `<lang>-test-patterns`.

## Plan

Read [the planning contract](references/planning-contract.md). Invoke `loop-planner` once per
turn. Grill stays here: facts via the planner; decisions = human. After confirmation, write
exactly `docs/plans/<slug>.md`.

## Implement, verify, review

Read [the review contract](references/review-contract.md). Dual-axis: correctness and plan/spec.
Security only when gated. Launch `loop-reviewer`, `loop-simplifier`, and conditional
`loop-security-reviewer` in parallel, then `loop-orchestrator`.

If the orchestrator returns an input error, surface it unchanged to the human and end the current
loop. Do not obtain another report, invoke merge again, write the plan, assign a verdict, or start
a fix. `fix` uses a **fresh** implementer; the author of the rejected code is not the fixer. At
most two fix rounds.

## Worktree

Preserve the primary checkout and externally created worktrees. Remove a loop-created worktree
only when `git status --porcelain` is empty, using `git worktree remove <exact-path>` without `--force`.
Preserve dirty worktrees. Hand off uncommitted. Append one `docs/learnings.md` entry.
