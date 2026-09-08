---
name: squad
description: User-invoked orchestrator for /squad and /squad-review. Mediates grill-style planning, runs plan-bound implementation and verification, fans reviewers on two axes, and caps two fix rounds. Do not model-invoke; type the entrypoint.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# Squad

Two entrypoints only: `/squad` and `/squad-review`. Type the command.

## Locked v1 flow

```text
/squad (user-invoked orchestrator)
1. Read and apply learnings.md (append-only): prefer passed skips/tiers; avoid what failed
2. Orient codebase (applicable stacks only)
3. Small change? → main agent only, spawn nobody → verify → append learnings → hand off uncommitted
4. Else grill/plan rounds (facts via subagent; decisions = human) → write plan
5. Gate spins: implementer → verifier → reviewers in parallel (correctness + plan/spec; security ONLY if trust boundary)
6. Orchestrator merges ≤2 fix rounds; one reviewer re-ask on malformed; optional residual fixup → hand off uncommitted → append learnings

/squad-review
Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → append learnings → one save-markdown ask

Always
models.source.json tiers (default inherit); load only contracted <lang>-* by Skill name; coworker docs = real dotnet test/validate on a fixture.

Not in v1
second skill pack, Matt catalog dump, eager fan-out, self-improve graphs, auto skill rewrite, redoing PR #6.
```

Step 1 is **Read and apply**. Apply the latest same-entrypoint entry. Prefer passed skips and
tiers. A **failed** skip is a must-run. Fail demotes one tier. Do not rewrite skills. No graph.
`docs/learnings.md` is append-only.

No commits, merges, or pushes. `pass` is ready for human review, not permission to land.

Cross-agent API is contracts + plan + learnings only. No shared coordination or lock files between
agents. `docs/plans/<slug>.md` run-scratch sections (`## Status`, `## Fix list`, `## Handoff notes`)
are rewritten in place; do not append forever inside the plan.

## Route

| Work | Route |
|---|---|
| Typo, rename, one-line change | The main agent implements and verifies directly. Do not call `squad-planner`, `squad-implementer`, `squad-verifier`, or any review agent. Do not create a plan. Small changes spawn nobody. |
| Behavior or design change | Full loop. |
| Trust-boundary / irreversible | Full loop; record the human decision first. |
| Existing diff, no plan | `/squad-review`. No verifier report, plan write, verdict, or fix round. One save-markdown ask at the end. |

Cost-first: default `inherit`. `squad-implementer` is `standard`. Other squad agents are `fast`.

## Gated agents

| Agent | When | Job |
|---|---|---|
| `squad-planner` | full change only | grill/plan |
| `squad-implementer` | full change only | build |
| `squad-verifier` | after implement/fix | evidence; `not verified` is not a pass |
| `squad-reviewer` | every review phase | correctness + plan/spec |
| `squad-simplifier` | every review phase | reuse, quality, efficiency in one spawn |
| `squad-security-reviewer` | trust boundary only | OWASP gate |
| `squad-orchestrator` | merge only | verdict / ≤2 fixes |

Small change: spawn none of these. Do not add a tester agent. Do not split simplifier.
`squad-orchestrator` never launches agents, edits product code, or plans.

## Skills

Load only with the Skill tool by exact name: `squad`, `caveman`, `<lang>-build`,
`<lang>-test-patterns`, `<lang>-review`, `<lang>-security-review`, and `<lang>-solution` when it exists.
Load `caveman` with the Skill tool by exact name `caveman`. Never write `/caveman` as prose to load it.
Never write slash-prose. Language-pack slots are internals (`metadata.audience: loop`).
Authored `mcp.json` files are empty scaffolds. No shipped MCP server. No new command. No credentials. No remote service.
Local-secrets rule: unauthorized Key Vault / secret store on local is often expected — not a product fail. Mention it and ask: continue? (workaround / mock / skip secret path). Do not invent tokens. Do not hard-fail the whole run without ask.

Grill stays here. Do not load an external grilling catalog.

## Stacks

Select from target paths, the existing diff, and acceptance criteria. Rust-only loads Rust.
.NET-only loads .NET. TypeScript-only loads TypeScript. A cross-language scope loads both. never request or load a pack for code outside the change.
Required slots: `<lang>-build`, `<lang>-test-patterns`.

## Plan

Read [the planning contract](references/planning-contract.md). Invoke `squad-planner` once per
turn. Grill stays here: facts via the planner; decisions = human. Prefer constraint-shaped
recommendations. Read `docs/decisions.md` when it exists; do not write it. After confirmation,
write exactly `docs/plans/<slug>.md`.

## Implement, verify, review

Read [the review contract](references/review-contract.md). Dual-axis: correctness and plan/spec.
Security only when gated. Launch `squad-reviewer`, `squad-simplifier`, and conditional
`squad-security-reviewer` in parallel, then `squad-orchestrator`.

### Malformed report (one re-ask, hard cap)

If the orchestrator returns an input error for a missing or malformed report, the **main agent**
may re-ask that producer **once** with the contract shape, then merges again. **Hard cap:** at most
one re-ask **per producer per review round**. Never a second re-ask for the same producer in the same
round. Never "keep fixing the report until it parses". After a failed re-ask (or if the budget is
already spent), merge with that axis marked `not verified — malformed after re-ask` (blocking) —
do not spawn that producer again. A repeated input-error for the same producer is not a new grant;
supply the marker. Do not hard-stop the whole run on the first malformed report when other reports
are usable. The orchestrator itself never retries or launches agents.

`fix` uses a **fresh** implementer; the author of the rejected code is not the fixer. At most two fix rounds.

### Optional residual fixup

After `pass`, or after two fix rounds when only residual `low`/`tiny` notes and known verifier nits
remain on the merge report: the main agent may run **one** fixup pass (main agent or a fresh
implementer) limited to those residual notes. No third full reviewer fan-out by default. If code
changed, run a verifier spot-check only. Still hand off uncommitted; do not land the branch.

## Advisor-lite

`/squad` only. Not a slash command. Do not add an advisor command.

At plan-confirm, when stuck, and before handoff: one read-only consult on a stronger
`models.source.json` tier than the current default. Order: inherit < fast < standard < frontier.
Default `inherit` consults `standard`. Read-only: no edits, no extra reviewer fan-out, no new loop.
Skip on `/squad-review`.

## Worktree

Preserve the primary checkout and externally created worktrees. Remove a squad-created worktree
only when `git status --porcelain` is empty, using `git worktree remove <exact-path>` without `--force`.
Preserve dirty worktrees. Hand off uncommitted. Append one `docs/learnings.md` entry.
