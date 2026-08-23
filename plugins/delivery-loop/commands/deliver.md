---
name: deliver
description: Have the main agent run a visible delivery loop through mediated planning, implementation, verification, parallel review, merge, and hand-off without committing. Use --no-pack to continue a full loop after declining a missing language pack.
---

# Deliver a change

Load the `delivery-loop` skill, then read `references/planning-contract.md` and `references/review-contract.md`. The main agent is the workflow controller.

1. Print the request, size route, detected stack, pack status, workspace, standards, repository conventions, and next action. Missing facts remain explicit.
2. For an obvious small change, implement and verify directly. Do not call the planner, implementer, verifier, orchestrator, or reviewers, and do not create a plan.
3. For a full loop, enforce language-pack readiness as described by the skill. After an approved installation, stop for a reload before creating `docs/plans/`. Small work may continue after reporting missing pack skills because its direct route does not depend on the plan-bound agents.
4. Invoke `loop-planner` in `next-round` mode with the request, evidence, decisions, user answers, and frontier. Present exactly the returned numbered round to the user. Pass their answers and returned state into the next invocation. Repeat until the planner returns the shared-understanding confirmation.
5. After the user confirms, invoke `loop-planner` in `write-plan` mode. Do not create the plan before confirmation.
6. Invoke `loop-implementer`, then `loop-verifier`, announcing the round number.
7. Directly launch `loop-reviewer`, `loop-simplifier`, and the conditional `loop-security-reviewer` in parallel. Record why the security gate ran or was skipped.
8. Give the completed reports, verifier evidence, round number, plan path, and security decision to `loop-orchestrator` for merge and verdict only.
9. Route `fix`, `pass`, or `replan` as defined by the skill. Allow at most two fix rounds.
10. Hand off the plan path, files touched, criterion evidence, verdict, rounds, notes, pack status, workspace, and cleanup status. State that nothing was committed, merged, or pushed.

For worktrees, preserve the primary checkout and externally created worktrees. Remove only a clean worktree created by this loop, following the skill's exact lifecycle rules.
