# PBI-47 (AP47): Finding ids and the review ledger
- Status: todo
- Area: squad
- Phase: G review loop
- Lane: Builder
- Depends on: [PBI-46](46-review-loop-budget.md)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0, and the 5 bad ledger fixtures exit 1 with REV05 to REV09
- door: two-way
- triage: build (Sven asked 2026-10-26, sol-loop based)
- feedback: `ReviewLoopTests` red per rule id, green on `good-two-rounds`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L74-L101

## Problem now

Review findings are numbered per report ([`review-contract.md:118`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L118) `| # | Severity | ...`), so round 2 cannot say "finding 3 from round 1 is fixed".
Builder answers in chat. There is no record of what was fixed, what was disputed, and why.

## Why (plain words)

A finding keeps one id from the round it was found until it closes.
Builder answers every id in a ledger: fixed, disagree or declined, each with proof.
Then the next round only checks the open ids, and nobody agrees just to finish.

## Finding line

`F<nn> [<blocker|should|nit>] <file>:<line> <claim>. Fix: <smallest fix>`

- `blocker`: the intended case is wrong.
- `should`: a real bug callers will hit, inside the intent.
- `nit`: everything else: other paths, rare cases, small perf, extra compat, style.
- The reviewer prefers the smallest fix: delete or narrow, before a new lock, cache, helper or option.
- Ids never change and are never reused. A new problem in round 2 gets the next free id.

## Ledger (Builder writes it, in the same review file)

```markdown
## Ledger after round 1

| Id | Severity | Outcome | Evidence |
|---|---|---|---|
| F01 | blocker | fixed | 7a2f4b1; repro base 500, tree 401 (measured) |
| F02 | should | disagree | `TokenRefresher.cs:44` already retries only on 401; test `RetryOnlyOn401` passes on 3c1d9e0 |
| F03 | nit | declined | out of scope: logging format; follow-up PBI candidate |
| F04 | should | fixed | deleted the extra cache in `Auth.cs:88` (narrowed, not added) |
```

Rules:
- Every open id gets exactly one row per round: `fixed`, `disagree` or `declined`.
- Evidence is a SHA, a `file:line`, a test name or command output. Never "done".
- `disagree` and `declined` are allowed with evidence. Never agree just to end the loop.
- If review made the diff bigger, Builder cuts first (`git diff --shortstat <base>` in the ledger head).
- A disputed id after 2 replies from each side gets `Escalate: Sven` with both sides in 2 to 3 lines. The loop stops for that id.

Reviewer, next round, marks each old id `resolved` or `still open: <reason>`.

## Exact check-review output

```text
docs/reviews/2026-10-27-login-retry.md:18 REV05 finding without id, severity or file:line (fix: write 'F<nn> [<severity>] <file>:<line> <claim>')
docs/reviews/2026-10-27-login-retry.md:33 REV06 finding id F02 reused for a new claim (fix: keep the first id; use F05)
docs/reviews/2026-10-27-login-retry.md:47 REV07 ledger row F03 without outcome fixed, disagree or declined (fix: add the outcome)
docs/reviews/2026-10-27-login-retry.md:48 REV08 ledger row F01 without evidence (fix: add a SHA, file:line, test or command output)
docs/reviews/2026-10-27-login-retry.md:61 REV09 F02 disputed after 2 replies and no 'Escalate: Sven' (fix: add the escalation)
```

## Before / after

| | Before | After |
|---|---|---|
| Finding id | row number per report | `F01` stable across rounds |
| Builder answer | chat | ledger row with evidence |
| Endless dispute | possible | after 2 replies: Sven |

## Definition of Done

- [ ] `review-contract.md`: finding line and ledger shape (one section, links L2 alias table).
- [ ] `check-review.sh` + `.ps1`: REV05 to REV09.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0, 5 bad fixtures red.
- [ ] No Sol, T3 or model names in shipped files.
- [ ] No em or en dashes.

## Technical design

- Module boundary: the review file. The orchestrator writes findings with ids; Builder appends the ledger; check-review checks both.
- Rules: REV05 to REV09.

## Sources

- tstack [`sol-loop/SKILL.md:74-77`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L74-L77) (blocker, should, nit; stable ids), [`:80-92`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L80-L92) (check each finding yourself; smallest fix; prefer delete or narrow; disagree with evidence; decline nits in one line; cut if review grew the diff; do not agree just to end the loop), [`:99-101`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L99-L101) (ledger fixed, disagree, declined; resolved or still open), [`:106-107`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L106-L107) (disputed after two replies, both sides, user decides).
- pstack [`poteto-mode/references/bugbot-triage.md:7-12`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/references/bugbot-triage.md#L7-L12) (fix, dismiss, ask), [`interrogate/references/lead-judgment.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/interrogate/references/lead-judgment.md) (Dismissed with a reason), PO S7.

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.sh)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.ps1`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.ps1)
- new: [`tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs`](../../../tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/interrogate/references/lead-judgment.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/interrogate/references/lead-judgment.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/references/bugbot-triage.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/references/bugbot-triage.md)
- t3dotgg/tstack: [`skills/sol-loop/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
