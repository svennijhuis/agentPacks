---
name: squad-review
description: Report-only review of a PR / uncommitted / vs main.
---

# Review an existing change

Load the `squad` skill with the Skill tool by exact name `squad`. Never write `/squad` as prose
to load it. Then read `references/review-contract.md`. Read
`docs/learnings.md` first when that file exists and **apply** the latest `/squad-review` entry
when gating reviewers.

This command reviews an existing change with no plan. Dual-axis: correctness and the spec implied
by the diff. Security only when a trust boundary changed.

1. Resolve the diff under review:
   - default / `--uncommitted` — unstaged then staged work;
   - `--pr` — the current pull request against its base;
   - `--base main` or `vs main` — `<base>...HEAD`, defaulting to `main`.
   Pin the product diff: omit run files from the reviewer payload (review contract) only when they
   appear in the diff, and list those under Not examined. Product docs that are the work stay in.
   A missing run file is not a finding. If the product diff is empty, say so and stop.
2. Decide whether the security gate applies and record the reason.
3. The main agent directly launches `squad-reviewer`, `squad-simplifier`, and, when applicable, `squad-security-reviewer` in parallel against the product diff.
4. After every report completes, pass the reports, security decision, `round number: 1`, `plan path: none`, and `verifier evidence: none` to `squad-orchestrator` for normalization and merge only. If it returns an input error for a missing or malformed report, re-ask that producer **once** (hard cap: one re-ask per producer per review round), then merge again with the **same round number**. Never a second re-ask for the same producer in the same round. Never "keep fixing the report until it parses". Do not increment the round for parse repair. After a failed re-ask, or if the budget is spent, merge **once** with that axis marked `accepted — malformed after re-ask` **replacing** the bad payload (**non-blocking**) — do not spawn that producer again. If input-error still returns after the marker was already supplied, stop and surface — no further merge. The orchestrator itself never retries or launches agents.
5. Return the ranked merged list in the IDE/CLI. Do not assign a Squad verdict, write a plan, or start a fix round.
6. Append one learnings entry.
7. Ask exactly one question: Save report as markdown?
   Yes — write the merged review to `docs/reviews/<slug>.md` and still show the findings in the IDE/CLI. Never write `docs/decisions.md`.
   No — IDE/CLI only; write no report file.
   Do not ask anything else. Do not start a fix round.

Without a plan, `squad-reviewer` checks correctness but has no acceptance criteria or plan-specific standards. A high finding is still actionable; it does not retroactively create a Squad run.
