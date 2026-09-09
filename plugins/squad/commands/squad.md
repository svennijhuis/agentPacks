---
name: squad
description: Plan → build → verify → review (gated). Uncommitted hand-off.
---

# Squad

Load the `squad` skill with the Skill tool by exact name `squad`. Never write `/squad` as prose
to load it. Then read `references/planning-contract.md` and `references/review-contract.md`. Read
`docs/learnings.md` first when that file exists and **apply** the latest same-entrypoint entry
when gating and spinning.

The main agent is the thin workflow controller. Specialists own their context.

1. Print the request, size route, every detected stack, the stacks applicable to this change, pack
   status, workspace, standards, repository conventions, and next action. Missing facts remain explicit.
2. For an obvious small change, implement and verify directly. Do not call the planner, implementer, verifier, orchestrator, or reviewers, and do not create a plan.
3. For a full loop, enforce language-pack readiness as described by the skill. Use `--no-pack` to continue a full loop after declining a missing language pack. After an approved installation, stop for a reload before creating `docs/plans/`. Small work may continue after reporting missing pack skills because its direct route does not depend on the plan-bound agents.
4. Invoke `squad-planner` in `next-round` mode with the request, evidence, decisions, user answers, and frontier. Present exactly the returned numbered round to the user. Pass their answers and returned state into the next invocation. Repeat until the planner returns the shared-understanding confirmation. Grill stays in these rounds; do not load a grilling catalog.
5. After the user confirms, one Advisor-lite consult as defined by the skill, then invoke `squad-planner` in `write-plan` mode. Do not create the plan before confirmation. When stuck, one Advisor-lite consult; do not fan out.
6. Invoke `squad-implementer` (TDD, then stop — no full suite), then `squad-verifier`, announcing the round number.
7. Directly launch `squad-reviewer`, `squad-simplifier`, and the conditional `squad-security-reviewer` in parallel. Record why the security gate ran or was skipped.
8. Give the completed reports, verifier evidence, round number, plan path, and security decision to
   `squad-orchestrator` for normalization, merge, and verdict only. If it returns an input error for a
   missing or malformed report, re-ask that producer **once** (hard cap: one re-ask per producer per review round), then merge again with the **same round number**. Never a second re-ask for the same producer in the same round.
   Never "keep fixing the report until it parses". Do not increment the round for parse repair. After a failed re-ask, or if the budget is spent,
   merge **once** with that axis marked `accepted — malformed after re-ask` **replacing** the bad payload (**non-blocking**) — do not spawn that
   producer again. If input-error still returns after the marker was already supplied, stop and surface — no further merge. The orchestrator itself never retries or launches agents.
9. Route `fix`, `pass`, or `replan` as defined by the skill. A `fix` uses a fresh implementer
   invocation; the author of the rejected code is not the fixer. Allow at most two fix rounds.
   After `pass`, or after two rounds with only residual `low`/`tiny` notes, at most one residual
   fixup may run as defined by the skill (never a third fan-out; failed spot-check → hand off).
10. Before handoff, one Advisor-lite consult as defined by the skill. Then hand off the plan path, files touched, criterion evidence, verdict, rounds, notes, pack status, workspace, and cleanup status. State that nothing was committed, merged, or pushed. Append one learnings entry. Ensure `.git/info/exclude` lists `docs/learnings.md` and `docs/agentpacks-feedback.md`. If a plugin component failed to load or produced an unusable report, append one marketplace-feedback entry as defined by the skill. Do not ask about that file.
