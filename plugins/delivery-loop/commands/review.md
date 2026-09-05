---
name: review
description: User-invoked dual-axis review of an existing change — a PR, uncommitted work, or a diff versus main — without a plan, verdict, or fix round.
---

# Review an existing change

Load the `delivery-loop` skill with the Skill tool by exact name `delivery-loop`. Never write
`/delivery-loop` as prose to load it. Then read `references/review-contract.md`. Read
`docs/learnings.md` first when that file exists.

This command reviews an existing change with no plan. Dual-axis: correctness and the spec implied
by the diff. Security only when a trust boundary changed.

1. Resolve the diff under review:
   - default / `--uncommitted` — unstaged then staged work;
   - `--pr` — the current pull request against its base;
   - `--base main` or `vs main` — `<base>...HEAD`, defaulting to `main`.
   If the resolved diff is empty, say so and stop.
2. Decide whether the security gate applies and record the reason.
3. The main agent directly launches `loop-reviewer`, `loop-simplifier`, and, when applicable, `loop-security-reviewer` in parallel against the same diff.
4. After every report completes, pass the reports, security decision, `round number: 1`, `plan path: none`, and `verifier evidence: none` to `loop-orchestrator` for normalization and merge only. If it returns an input error, surface that error unchanged and stop without retrying or launching another agent.
5. Return the ranked merged list. Do not assign a delivery verdict, write a plan, or start a fix round.
6. Append one learnings entry.

Without a plan, `loop-reviewer` checks correctness but has no acceptance criteria or plan-specific standards. A high finding is still actionable; it does not retroactively create a delivery loop.
