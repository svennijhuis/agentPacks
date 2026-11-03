---
name: squad-mode
description: When /squad or /squad-review runs, the main chat keeps the playbook and spawns one fresh delegate per step.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

Source: cursor/plugins@ae5650febf4320793844c12c4adaa937f417f8da

# Squad mode

`/squad` and `/squad-review` are the entrypoints. Type the command. This file is the hub. The main chat that receives `/squad` runs the playbook and keeps the todo list. It does not hand the playbook to a delegate.

## Non-negotiables

No em dash (U+2014) and no en dash (U+2013). Use `:` or `,`. Paraphrase quoted text that has one.

Explore with `git ls-files` and `rg`, never `find`. Load a skill with the Skill tool by exact name. Never write a slash command as prose to load it.

Load only with the Skill tool by exact name: `squad-mode`, `caveman`, `verify-harness`, `scenarios-md`, `<lang>-build`, `<lang>-test-patterns`, `<lang>-review`, `<lang>-security-review`, and `<lang>-solution` when it exists. Load `caveman` by exact name `caveman`. Never write `/caveman` as prose to load it. Language packs come from the change only.

Every plan edit (resume and stop too): write, run check-plan, continue only at exit 0, and the last edit exits 0 before any agent starts or the retro is written; a rejected linter call is not run, never a manual pass. Linter path, from this plugin root: `../pack-check/skills/pack-check/scripts/check-plan.sh` (or `check-plan.ps1`). Do not search the filesystem for it. After `squad-retro` writes, run `scripts/check-retro.sh` the same way.

Local-secrets rule: do not invent tokens. A hard secret left unrun is `blocked`, never `pass`. A `needs:`-tagged Doctor check that fails is `skipped (needs: <tag>)` (tag only). It does not block.

When the change touches app behavior and `docs/verify.md` is missing, ask once: `Make a runnable verify harness too? (yes/no)`. Yes → load `verify-harness` by exact Skill name. No → carry on; write no `docs/verify.md`.

Small change (typo, rename, one line): the main chat implements and verifies directly. Write a tiny plan (goal, decision, verify command). Small change: verify (verify cmd, then the wider unit suite once; no retro). Verify runs that command, then the wider unit suite once. Small change: spawn none of these; no retro. If this run evidenced a pack/skill/agent/command/contract change, append one suggestions entry. Do not apply it.

No phase commit. On PASS, one normal push (never force push). Rebased work goes to a branch whose name ends in `-v2`. Then a pull request. Never merge.

Hand off paths, not pasted bodies. Suggestions are human-apply only. Read [suggestions](references/suggestions.md). Do not apply them.

draft plan file from round 1 (check-plan it at once, fix findings). spawn passes the slice name, the plan path, and the `file:line` list;

## Subagents

The main chat spawns a fresh `squad-agent` with a role line for each step and each fix round. Never spawn an agent to run the playbook itself. Never resume a delegate across steps.

Routed skills keep their own subagent type and model. Those skills are interrogate, swarm, how, why, reflect, and blast-radius.

| Role | Skills | Model tier |
|---|---|---|
| planner | how, why, blast-radius | standard |
| architect | architect, statechart | frontier |
| implementer | tdd, language standards | standard |
| verifier | the repo verifier skill (see Verification contract) | standard |
| simplifier | no-comments | standard |

`squad-reviewer` is frontier, so it is a different model from the implementer. `squad-security-reviewer` and `squad-retro` are standard. No role uses `fast`.

Each spawn gets the goal, its role, its one step, the evidence it must return, the hub path, and file pointers. Do not inline the file bodies.

## Verification contract

Read [verification contract](references/verification-contract.md). The verifier uses the repo's own verifier skill. If there is none, run create-verification-skill first. The skill must cover LAUNCH, DOCTOR, DRIVE, EVIDENCE, and CLEANUP.

## Code review loop

Code-writing playbooks end here. The main chat runs it. Do not push before PASS.

1. verifier. Runs the contract. Result PASS, FAIL, or BLOCKED, with real output.
2. squad-reviewer (uses interrogate and blast-radius). PASS or FAIL, with a concrete fix list.
3. FAIL. A fresh implementer fixes, then verifier and reviewer again. Max 2 fix rounds. Still FAIL. No push. Report the open points.
4. BLOCKED (missing tool, access, or secret). Stop and report exactly what is missing. Do not push.
5. PASS. Normal push (never force push, rebased work goes to a `-v2` branch), then a pull request with the handoff. Never merge.

## Handoff

The pull request body is [handoff](references/handoff.md). Fields: tested_commit, diff summary, commands + results, evidence_paths, unverified_checks, remaining_risks.

## Prevent the next mistake

After a FAIL or a user correction, `squad-retro` runs reflect and correct. It encodes the lesson as the strongest guard it can. Architecture, then types or lint or CI, then tests, then a written rule.

## Swarm

Parallel checks use the swarm skill as-is. Workers are generalPurpose. Each brief stands alone. Reports are PASS, ISSUES, or BLOCKED, with evidence.

## Playbooks

Pick one. The top line of each playbook is the ownership rule.

- [bug-fix](playbooks/bug-fix.md)
- [feature](playbooks/feature.md)
- [domain](playbooks/domain.md)
- [perf](playbooks/perf.md)
- [investigation](playbooks/investigation.md)
- [plan](playbooks/plan.md)
- [review](playbooks/review.md)
- [pr](playbooks/pr.md)
- [retro](playbooks/retro.md)

## Session split

Read [session split](references/session-split.md). Before step 1: `/squad split <request>` → forced split (write `docs/squad-plan.md` even under the limits, no code, no retro, stop; a plan exists → ask once: replace or append). A plan exists → status refresh (Done-check exit 0 → `done`; a task waiting on a task not `done` is skipped with the reason), then always ask, also with no request: `Plan <outcome>: task 1 done, task 2 open. Shall I do task 2?` (`started` → half done, finish it?; all done → close it?, yes archives it). Yes → that one task only (set `started`; too big → split that task in place into `<n>.1`, `<n>.2`, no code; else run it, mark it `done` when its done-check passes; stop). A request that does not name the plan → ask once: task or new request; a new request leaves the plan untouched.

Never rewrite an existing plan. Normal run, after step 2: over any limit (more than one PBI or outcome, more than 3 steps, more than 10 files) → write `docs/squad-plan.md`, build no code, spawn nobody, stop, no retro; a plan already there → ask once: replace or append. Planner `Session split: <n> steps` on next-round or write-plan → the same forced-split exit; first delete this run's draft `docs/plans/` file. The next task runs on the next `/squad`, in a new session.

1. Print the request, the mode, and the playbook.
2. Orient codebase (applicable stacks only) before the fits-one-session check.

## Contracts

- [planning contract](references/planning-contract.md)
- [review contract](references/review-contract.md)
- [architect contract](references/architect-contract.md)
- [adr](references/adr.md)
- [retro](references/retro.md)
- [malformed re-ask](references/malformed-reask.md)
- [residual fixup](references/residual-fixup.md)
- [worktree](references/worktree.md)
- [advisor-lite](references/advisor-lite.md)
- [suggestions](references/suggestions.md)

## Provider notes

Cursor is the only client that loads `rules/review-checklist.mdc` by glob. Other clients do not receive that rule.
