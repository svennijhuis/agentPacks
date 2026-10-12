---
name: squad
description: When the user types /squad or /squad-review.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# Squad

Two entrypoints only: `/squad` and `/squad-review`. Type the command.

## Locked v1 flow

```text
/squad (user-invoked orchestrator)
1. Newest docs/plans/ file still Status: draft with no matching retro → spawn squad-retro on it first (reason: interrupted). Then read the latest same-entrypoint docs/retros/ file: carry its Lessons and Improvements as facts; never auto-apply
2. Orient codebase (applicable stacks only)
3. Small change? → main agent only, spawn nobody → tiny plan (goal, decision, verify cmd) → verify (no retro; + suggestions if pack/skill/agent/command/contract evidenced) → hand off uncommitted
4. Else grill/plan rounds (facts via subagent; decisions = human); draft plan file from round 1, each round appended → architect rule fired? main agent spawns squad-architect for Design + Map (planner Design gap → ≤1 architect rerun) → confirm sets Status: confirmed
5. Gate spins: sequential implementers (missing/one ## Slices → one whole-plan implementer; else one fresh implementer per slice in order; Plan gap → planner next-round on that gap → fresh implementer redoes the slice; 2nd Plan gap on a slice → Replan) → then one verifier → reviewers in parallel (correctness + plan/spec; security ONLY if trust boundary)
6. Orchestrator merges ≤2 fix rounds; ≤1 re-ask per producer per review round then accept marker (non-blocking); optional residual fixup → hand off uncommitted → squad-retro writes the retro, also on stop or fail (+ suggestions if pack/skill/agent/command/contract evidenced)

/squad-review
Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → always save report to docs/reviews/<yyyy-mm-dd>-<slug>.md → squad-retro writes the retro from that report (+ suggestions if pack/skill/agent/command/contract evidenced)

Always
models.source.json tiers (default inherit); load only contracted <lang>-* by Skill name; coworker docs = real dotnet test/validate on a fixture.
suggestions.md is optional, human-apply only; never auto-rewrite.

Not in v1
second skill pack, Matt catalog dump, eager fan-out, self-improve graphs, auto skill rewrite, redoing PR #6.
```

Step 1 reads the latest retro ([retro](references/retro.md)) as input, not as orders. Improvements
are human-apply only. Do not rewrite skills. No graph. `docs/suggestions.md` is optional and human-apply only.
Hand-off between agents passes paths (plan `.md`, last SHA, retro), never pasted text.

No commits, merges, or pushes. `pass` is ready for human review, not permission to land.

Cross-agent API is contracts + plan + retro files only. No shared coordination or lock files between
agents. `docs/plans/<yyyy-mm-dd>-<slug>.md` run-scratch sections (`## Status`, `## Fix list`, `## Handoff notes`)
are rewritten in place; do not append forever inside the plan.

## Route

| Work | Route |
|---|---|
| Typo, rename, one-line change | The main agent implements and verifies directly. Do not call `squad-planner`, `squad-implementer`, `squad-verifier`, or any review agent. Write a tiny plan (goal, decision, verify command). Small changes spawn nobody. |
| Behavior or design change | Full loop. |
| Trust-boundary / irreversible | Full loop; record the human decision first. |
| Existing diff, no plan | `/squad-review`. No verifier report, plan write, verdict, or fix round. Always saves the report. |

Cost-first: default `inherit`. Every `squad-*` agent is `standard` except `squad-architect` (`frontier`); none uses `fast`. Copilot ids use dotted versions (`claude-sonnet-5.5`).

## Gated agents

| Agent | When | Job |
|---|---|---|
| `squad-planner` | full change only | grill/plan |
| `squad-architect` | full change crossing layers, new project/package, or public API | read-only `## Design` and `## Map` ([contract](references/architect-contract.md)) |
| `squad-implementer` | full change only | build |
| `squad-verifier` | after implement/fix | evidence; `not verified` is not a pass; `blocked` is unproven, not a pass |
| `squad-reviewer` | every review phase | correctness + plan/spec |
| `squad-simplifier` | every review phase | reuse, quality, efficiency in one spawn |
| `squad-security-reviewer` | trust boundary only | OWASP gate |
| `squad-orchestrator` | merge only | verdict / ≤2 fixes |
| `squad-retro` | end of a full run or `/squad-review`, also on stop or fail | one `docs/retros/` file ([retro](references/retro.md)) |

Small change: spawn none of these; no retro. Do not add a tester agent. Do not split simplifier. `squad-orchestrator` never launches agents, edits product code, or plans.

## Skills

Load only with the Skill tool by exact name: `squad`, `caveman`, `<lang>-build`,
`<lang>-test-patterns`, `<lang>-review`, `<lang>-security-review`, and `<lang>-solution` when it exists.
Load `caveman` with the Skill tool by exact name `caveman`. Never write `/caveman` as prose to load it.
Never write slash-prose. Language-pack slots are internals (`metadata.audience: loop`).
Authored `mcp.json` files are empty scaffolds. No shipped MCP server. No new command. No credentials. No remote service.
Local-secrets rule: unauthorized Key Vault / secret store on local is often expected — not a product fail. Mention it and ask: continue? (workaround / mock / skip secret path). Do not invent tokens. Do not hard-fail the whole run without ask. A criterion the skipped secret path leaves unrun is `blocked`, never `pass`.

Grill stays here. Do not load an external grilling catalog.

## Stacks

Select from target paths, the existing diff, and acceptance criteria. Rust-only loads Rust.
.NET-only loads .NET. TypeScript-only loads TypeScript. A cross-language scope loads both. never request or load a pack for code outside the change.
Required slots: `<lang>-build`, `<lang>-test-patterns`.

## Plan

Read [the planning contract](references/planning-contract.md). Invoke `squad-planner` once per
turn. Grill stays here: facts via the planner; decisions = human. The main agent never grills
itself. Name facts, do not dig: list them under `Facts to check` and let the `standard`-tier planner
find them. Prefer constraint-shaped recommendations. Read `docs/decisions.md` when it exists; do not write it. The planner keeps the draft
`docs/plans/<yyyy-mm-dd>-<slug>.md` from round 1; confirmation sets `Status: confirmed`. Hard decisions get `docs/adr/` records ([ADR](references/adr.md)).

## Implement, verify, review

Read [the review contract](references/review-contract.md). Sequential implementers first: if `## Slices` is missing or
has one entry, one fresh `squad-implementer` on the whole plan; otherwise one fresh `squad-implementer` per entry in
order (spawn passes the slice name and the plan path; each slice names its files in `## Slices`, so no research is redone; same branch; no peer spawn). Then one `squad-verifier`. Then dual-axis: correctness and
plan/spec. Security only on a trust-boundary diff (Security gate). Launch `squad-reviewer`, `squad-simplifier`, and
conditional `squad-security-reviewer` in parallel, then `squad-orchestrator`. Do not launch a full-repo audit.

Malformed report (one re-ask, hard cap): read [malformed re-ask](references/malformed-reask.md). Never resume an agent across phases; resume only for that re-ask.
`fix` uses a **fresh** implementer (spawn names `fix-list`); the author of the rejected code is not the fixer. At most two fix rounds.
Optional residual fixup: read [residual fixup](references/residual-fixup.md).

## Advisor-lite

`/squad` only. Skip on `/squad-review`. Read [advisor-lite](references/advisor-lite.md).

## Worktree

Read [worktree](references/worktree.md). Hand off uncommitted. Spawn `squad-retro` once with paths; it writes only `docs/retros/`.
If this run evidenced a pack/skill/agent/command/contract change, append one
`docs/suggestions.md` entry as defined by [suggestions](references/suggestions.md). Do not apply it.
