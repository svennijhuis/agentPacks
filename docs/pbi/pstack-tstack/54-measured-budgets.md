# PBI-54 (AP54): Measured evidence: budgets in docs/verify.md
- Status: todo
- Area: squad
- Phase: F verification
- Lane: Builder
- Depends on: [AP52](52-feature-map.md), [AP41](41-drive-kinds.md) (Kind trace). Soft: [AP53](53-feature-map-upkeep.md) (Source overlap)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-budget.md` exits 1 with one VER13 line, and `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerificationEvidenceTests"` exits 0
- door: two-way
- triage: build (Sven must-cover theme 3)
- feedback: `VerificationEvidenceTests` rejects a budget row quoted without runs and spread
- harness: verifier runs budgets touched by the diff
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 03:09, 09:30, 14:16, 28:25

## Problem now

The verifier re-measures a quantitative criterion only when the plan names one ([`plugins/squad/agents/squad-verifier.md:22`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L22)). `engineering.md` says how to report a number ([`shared/standards/engineering.md:24-27`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md#L24-L27)).
A repo has no standing budgets. A slow change passes when no plan asked about speed.

## Why (plain words)

poteto started with a wall of PRs and "no idea whether or not the performance of the app would be regressing" ([talk 03:09](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L17)). Verification gives "empirical evidence ... that your performance bar is being met" ([talk 09:30](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L33)) and real metrics ([talk 14:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L45)). Dune has hard numbers: no long task over 16 ms (60 fps) or 8 ms (120 fps) on the renderer ([talk 27:57](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L77)).
A budget written once is checked on every run, not only when someone remembers.

## What changes

`docs/verify.md` optional `## Budgets` table (only when the repo has a performance goal):

| Column | Content |
|---|---|
| Metric | what is measured |
| Limit | `<op> <number> <unit>` |
| Command | one command, Drive or verify CLI |
| Runs | at least 3 (5 for perf plans, AP40) |
| Source | paths; the verifier runs the row when the diff touches them |

The verifier quotes `<measured mean> <op> <limit> (n runs, range a to b, limiter: <x>)`. Over the limit = `not verified`. Label is `measured` (see [AP43](43-verdict-labels.md)).

## Example

```markdown
## Budgets

| Metric | Limit | Command | Runs | Source |
|---|---|---|---|---|
| GET /orders p95 | <= 200 ms | `./scripts/verify bench orders-list` | 5 | src/Orders/ |
| renderer long task | <= 16 ms | `./scripts/verify trace open-chat` | 5 | src/renderer/ |
```

Good evidence: `GET /orders p95 142 ms <= 200 ms (5 runs, range 131 to 155, limiter: db)`. Bad: `fast enough`.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-budget.md` (Runs = 1) prints `bad-budget.md:7 VER13 budget row needs Metric, Limit, Command and Runs >= 3 (fix: fill the row)` and exits 1.
- [ ] `good-budget.md` exits 0 with no output.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerificationEvidenceTests"` exits 0.

## Technical design

- Module boundary: verify.md owns the numbers; check-verify.sh checks the shape; the verifier measures. No budget numbers in skills.
- Rules: VER13.

## Sources

- Talk: [talk 03:09](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L17), [talk 09:30](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L33), [talk 14:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L45), [talk 27:57](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L77), [talk 28:52](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L79). Notes: [notes 0.6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L15), [notes 2.7](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L37), [notes 2.14](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L44), [notes 4.15](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L72), [notes 9.1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L118).
- Podcast: [podcast about 18:01](https://www.youtube.com/watch?v=MN9dGgmLyso) (hill climbing against a score). Audit [audit 2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L46).
- pstack pstack [`principle-explain-the-number/SKILL.md:9-11`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-explain-the-number/SKILL.md#L9-L11), pstack [`benchmark-checklist/SKILL.md:25`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/benchmark-checklist/SKILL.md#L25).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- edit: [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh)
- edit: [`tools/AgentPacks.Cli.Tests/VerificationEvidenceTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerificationEvidenceTests.cs) (budget row without runs and spread is rejected)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-budget.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-budget.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/good-budget.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/good-budget.md)
