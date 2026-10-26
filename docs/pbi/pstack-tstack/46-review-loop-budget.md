# PBI-46 (AP46): Review loop budget
- Status: todo
- Area: squad
- Phase: G review loop
- Lane: Builder
- Depends on: [PBI-45](45-handoff-pr-order.md); Sven decision L1 (two kinds of round) and L2 (severity aliases), see README
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0, and `bash plugins/pack-check/skills/pack-check/scripts/check-review.sh tools/AgentPacks.Cli.Tests/Fixtures/reviews/bad-round-4/docs/reviews/2026-10-27-login-retry.md` prints one REV01 line and exits 1
- door: two-way (the contract); the room charter change is Sven's call (L1)
- triage: build (Sven asked on 2026-10-26: Reviewer to Builder loop based on sol-loop, without Sol or T3)
- feedback: `ReviewLoopTests` red with REV01 to REV04 on fixtures, green on `good-two-rounds`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L15-L25

## Problem now

On a PR the room runs Reviewer PASS/FAIL, Builder fixes, Reviewer checks again.
How many times is not written down. Neither is when to stop, or what a later round looks at.
Inside `/squad` there is a hard cap: "At most two fix rounds" ([`skills/squad/SKILL.md:108`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L108)).
`/squad-review` is one shot: "Do not ... start a fix round" ([`commands/squad-review.md:27-28`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md#L27-L28)).

## Why (plain words)

A review loop without a budget either stops too early or never ends.
Each new round tends to grow the PR one small fix at a time.
A fixed budget, a clear stop rule, and "later rounds only look at what changed" keep the PR small and the loop short.
Running out of rounds does not mean the code is good. It means Sven decides.

## Conflict with today's rules (decision L1 for Sven)

| Rule today | Where | Conflict | Proposal |
|---|---|---|---|
| Max 2 fix rounds | [`skills/squad/SKILL.md:108`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L108), [`commands/squad.md:35`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md#L35), room charter "max 2 fix rounds under /squad" | sol-loop budget is 3 (5 for risk) | Keep both, named apart. **Fix round**: inside one `/squad` run, automatic, uncommitted, max 2, unchanged. **Review round**: on a PR, Reviewer bot vs Builder, budget 3 or 5. A review round may contain one `/squad` fix run; its 2 fix rounds do not count against the review budget. |
| Reviewer: one PASS/FAIL, no re-spot spam | room charter (Reviewer card) | sol-loop has several rounds | Each review round ends in exactly one PASS/FAIL message. Round 2+ only covers changed code and open ids, so it is not a re-spot. |
| `/squad-review` never starts a fix round | [`commands/squad-review.md:27-28`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md#L27-L28) | the loop needs a second review | Still true. `/squad-review --round <n> --review <file>` reviews again; it never fixes. Builder fixes. |

## The contract (new file references/review-loop.md)

- Budget: 3 review rounds. 5 only when the diff touches security, permissions, auth, data loss, or state and concurrency, and a `blocker` or `should` is still open with evidence. Or when Sven says so. The review file states it: `Budget: 3` or `Budget: 5 (auth)`.
- Round 1 reviews the whole change. Round 2+ reviews only code changed since the last round plus the open ids.
- Stop early when a round returns only `nit`. Declined nits and comment, doc or format-only fixes need no new round.
- Start a new round only if budget remains and a `blocker` or `should` is open, behavior code changed, or the last APPROVE does not cover the current behavior diff.
- Out of budget is not approval: the verdict is `BLOCKED (out of budget)`, both sides in 2 to 3 lines each, Sven decides.
- One reviewer per round. The reviewer never edits, commits, pushes, or starts other reviewers.

Severity words (decision L2): `blocker` = `high`, `should` = `medium`, `nit` = `low` or `tiny` ([`review-contract.md:5-14`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L5-L14)).

## Example review file head

```markdown
# Review: login retry (PR #91)
Budget: 5 (auth)
Base: 3c1d9e0  Head: 7a2f4b1
## Round 2
Scope: changed since 5e8d0c2 + open ids F01, F03
VERDICT: CHANGES
```

## Exact check-review output

```text
docs/reviews/2026-10-27-login-retry.md:41 REV01 round 4 above budget 3 (fix: stop the loop; escalate to Sven)
docs/reviews/2026-10-27-login-retry.md:2 REV02 budget 5 without a risk reason (fix: add 'Budget: 5 (security, auth, data loss or concurrency)')
docs/reviews/2026-10-27-login-retry.md:44 REV03 out-of-budget review marked PASS (fix: mark it BLOCKED)
docs/reviews/2026-10-27-login-retry.md:52 REV04 warning: round 2 finding F07 outside changed code and not an open id (fix: drop it or open a new id with reason)
```

## Before / after

| | Before | After |
|---|---|---|
| Rounds on a PR | unbounded | 3, or 5 with a named risk |
| Round 3 reviewer reads | whole diff again | changed code + open ids |
| Round budget spent, still CHANGES | ambiguous | BLOCKED, Sven decides |

## Definition of Done

- [ ] `references/review-loop.md` (max 30 lines) + [`commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md) accepts `--round <n> --review <file>`.
- [ ] `check-review.sh` + `.ps1` with REV01 to REV04, README output format, exit 0, 1, 2.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0 with 4 bad fixtures red.
- [ ] `SquadContractTests` still asserts "At most two fix rounds" in [`skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md) (fix rounds unchanged).
- [ ] No mention of Sol, T3, `delegate_task` or a model name in any shipped file.
- [ ] No em or en dashes.

## Technical design

- Module boundary: the review file `docs/reviews/<yyyy-mm-dd>-<slug>.md` is the interface between Reviewer and Builder. check-review lints it. Agents never share chat.
- Rules: REV01 to REV04.

## Sources

- tstack [`sol-loop/SKILL.md:12-13`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L12-L13) (minimal changes, push back), [`:15-25`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L15-L25) (budget, stop on nits, one reviewer, ledger), [`:94-101`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L94-L101) (next round only when needed, open ids and changed code), [`:103-108`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L103-L108) (agreed vs stuck; budget end is not approval); [`council/SKILL.md:50`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/council/SKILL.md#L50) (fight scope creep); [`babysit-pr/SKILL.md:95-100`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/babysit-pr/SKILL.md#L95-L100) (review loops grow a PR).
- agentPacks [`plugins/squad/skills/squad/SKILL.md:108`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L108), [`commands/squad-review.md:27-28`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md#L27-L28), [`references/review-contract.md:5-14,245`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L5-L14).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- new: [`plugins/squad/skills/squad/references/review-loop.md`](../../../plugins/squad/skills/squad/references/review-loop.md)
- edit: [`plugins/squad/commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.sh)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.ps1`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.ps1)
- new: [`tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs`](../../../tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs)
- edit: [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)

## Links

- t3dotgg/tstack: [`skills/babysit-pr/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/babysit-pr/SKILL.md)
- t3dotgg/tstack: [`skills/council/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/council/SKILL.md)
- t3dotgg/tstack: [`skills/sol-loop/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md)
- agentPacks main (existing files): [`plugins/squad/commands/squad.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
