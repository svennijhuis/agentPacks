# PBI-48 (AP48): Review line in the PR body and the Reviewer reply
- Status: todo
- Area: squad
- Phase: G review loop
- Lane: Builder (Cursor CLI, easy)
- Depends on: [PBI-47](47-review-ledger.md)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0, and `bash plugins/pack-check/skills/pack-check/scripts/check-review.sh --pr-body tools/AgentPacks.Cli.Tests/Fixtures/reviews/bad-pr-line/body.md` prints one REV10 line and exits 1
- door: two-way
- triage: build (Sven asked 2026-10-26: PR body gets rounds + verdict)
- feedback: `ReviewLoopTests` red on `bad-pr-line`, green on `good-pr-line`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L110-L119

## Problem now

A PR body does not say how it was reviewed, how many rounds, or what is still open.
Reviewer replies in the room have no fixed shape, so Product Owner reads prose to find the verdict.

## Why (plain words)

One line in the PR body tells anyone, later, how hard this change was reviewed.
One fixed reply shape lets PO and Builder find the verdict and the open ids at a glance.

## PR body line (goes in AP45's `## Verification`)

`Reviewed in <n> rounds (budget <b>) by Reviewer. Verdict: <PASS|BLOCKED (out of budget)|BLOCKED (escalated F02)>. Open: <none|F03 nit, declined>.`

Example: `Reviewed in 2 rounds (budget 5, auth) by Reviewer. Verdict: PASS. Open: F03 nit, declined.`

No model name, no tool name. "Reviewer" is our Reviewer bot.

## Reviewer reply (one message per round, in the room or on the PR)

```text
@Builder Round 2/5 (auth) on 7a2f4b1. FAIL.
Scope: changed since 5e8d0c2 + open F01, F02.
F01 resolved.
F02 still open: retry also fires on 403 (TokenRefresher.cs:51, test RetryOn403 red). Fix: narrow the condition to 401.
F05 [should] Auth.cs:90 cache never cleared on logout. Fix: delete the cache, call the store.
I think we need to fix: F02, F05. Nits: none.
```

PASS reply: `Round 3/5 (auth) on 9b0c1d2. PASS. Open: F03 nit, declined. PR line ready.`
Out of budget: `Round 5/5 on ... BLOCKED (out of budget). F02: <reviewer side>. <builder side>. Sven decides.`

Comments posted for Sven follow the room rule: lead with the verdict, cite code, separate fact from judgment.

## Exact check-review output

```text
body.md:12 REV10 PR body review line has the wrong shape (fix: 'Reviewed in <n> rounds (budget <b>) by Reviewer. Verdict: <...>. Open: <...>.')
```

## Before / after

| | Before | After |
|---|---|---|
| PR body | no review trace | one line: rounds, budget, verdict, open ids |
| Reviewer reply | free text | round, SHA, verdict, ids, fix list |

## Definition of Done

- [ ] `references/review-loop.md` documents the line and the reply.
- [ ] `check-review.sh --pr-body <file>` checks REV10.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0.
- [ ] Room charter note for Reviewer and Builder (Sven applies it; not a repo change).
- [ ] No em or en dashes.

## Technical design

- Module boundary: one line in the PR body; check-review reads it.
- Rules: REV10.

## Sources

- tstack [`sol-loop/SKILL.md:110-119`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L110-L119) (push only after agreement; PR line with rounds and verdict; report rounds, fixed and declined), [`council/SKILL.md:23-40`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/council/SKILL.md#L23-L40) (rounds table in the PR description, never invent a score), [`leaving-pr-comments/SKILL.md:35-40`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/leaving-pr-comments/SKILL.md#L35-L40) (lead with conclusion, cite, facts vs judgment).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- new: [`plugins/squad/skills/squad/references/review-loop.md`](../../../plugins/squad/skills/squad/references/review-loop.md)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.sh)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.ps1`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.ps1)
- new: [`tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs`](../../../tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs)

## Links

- t3dotgg/tstack: [`skills/council/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/council/SKILL.md)
- t3dotgg/tstack: [`skills/leaving-pr-comments/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/leaving-pr-comments/SKILL.md)
- t3dotgg/tstack: [`skills/sol-loop/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md)
