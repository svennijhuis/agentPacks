# PBI-40 (AP40): Verifier reruns the repro on the same surface
- Status: todo
- Area: squad
- Phase: F playbooks
- Lane: Builder
- Depends on: [PBI-39](39-per-type-plan-gates.md)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter "VerificationEvidenceTests|SquadContractTests"` exits 0, and the fixture verifier report with only an "after" run fails SQD03
- door: two-way
- triage: build (Sven priority 1 and 2: same-surface repro, prove it works)
- feedback: `VerificationEvidenceTests` rejects a bug-fix report without the before and after pair
- harness: none (script) for the contract; the verifier itself runs the repo's real surface
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md#L10 and D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/full-send/SKILL.md#L65-L80

## Problem now

`squad-verifier` runs the criterion command and the wider suite ([`agents/squad-verifier.md:32`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L32)).
For a bug fix it never runs the original repro on the old code.
So "the test passes" can mean the test never failed.

## Why (plain words)

A fix is proven when the same repro fails before and passes after, on the same surface the user hit.
A unit test that was green before the fix proves nothing about the bug.
pstack and tstack both say this in nearly the same words, so this is one PBI with both sources.

## What changes

[`agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md), new constraint:
"Task type bug-fix: run `Repro:` on the base (`git stash` is banned; use `git worktree add <tmp> <base>` read-only) and on the tree. Quote both. Base must fail, tree must pass. Task type perf: run `Baseline:` on both, 5 runs each, alternating. Missing pair = `not verified`."

[`references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md), verifier rows: one row for the pair rule.

## Example verifier report rows

```markdown
| # | Criterion | Command | Result | Evidence |
|---|---|---|---|---|
| 1 | expired token returns 401 | Repro (api) | pass | base `9f1c2ab`: 500; tree: 401 (both from `curl ... -w '%{http_code}'`) |
| 2 | validate is faster | Baseline (cli) | pass | base mean 4.1 s (5 runs, range 3.9 to 4.3); tree mean 2.7 s (5 runs, range 2.6 to 2.9); limiter: disk read |
```

Bad (fails SQD03 in the fixture): `| 1 | expired token returns 401 | Repro | pass | tree: 401 |`.

## Before / after

| | Before | After |
|---|---|---|
| Bug-fix evidence | criterion test green | repro output on base and tree, same command |
| Wrong surface (unit test for a UI bug) | can pass | `not verified: wrong surface` |

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "VerificationEvidenceTests|SquadContractTests"` exits 0.
- [ ] Fixture `Fixtures/verifier/bad-bugfix-after-only.md` fails SQD03; `good-bugfix-pair.md` passes.
- [ ] The verifier never edits the base worktree and removes it in Cleanup (no `--force`).
- [ ] No em or en dashes.

## Technical design

- Module boundary: plan lines (`Repro:`, `Baseline:`) in, verifier report rows out. The orchestrator already treats `not verified` as not a pass.
- Rules: SQD03.

## Sources

- pstack [`playbooks/bug-fix.md:7,10,11`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md#L7), [`perf-issue.md:5,19`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/perf-issue.md#L5), [`principle-prove-it-works/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-prove-it-works/SKILL.md), [`benchmark-checklist/SKILL.md:25`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/benchmark-checklist/SKILL.md#L25) (5 runs alternating).
- tstack [`full-send/SKILL.md:65-80`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/full-send/SKILL.md#L65-L80) (reproduce, fix, watch the same reproduction pass; mocked green does not prove UX), [`takeover/SKILL.md:14,33-35`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/takeover/SKILL.md#L14).
- benny [`reproduce-and-fix-issues/references/verify-existing-fix.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/verify-existing-fix.md).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- edit: [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
- edit: [`tools/AgentPacks.Cli.Tests/VerificationEvidenceTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerificationEvidenceTests.cs)
- edit: [`tools/AgentPacks.Cli.Tests/SquadContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/SquadContractTests.cs)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/verifier/bad-bugfix-after-only.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verifier/bad-bugfix-after-only.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/verifier/good-bugfix-pair.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verifier/good-bugfix-pair.md)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/automations/benny/skills/reproduce-and-fix-issues/references/verify-existing-fix.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/verify-existing-fix.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/benchmark-checklist/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/benchmark-checklist/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/bug-fix.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/perf-issue.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/perf-issue.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/principle-prove-it-works/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-prove-it-works/SKILL.md)
- t3dotgg/tstack: [`skills/full-send/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/full-send/SKILL.md)
- t3dotgg/tstack: [`skills/takeover/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/takeover/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
