# PBI-39 (AP39): Per-type plan gates
- Status: todo
- Area: squad
- Phase: F playbooks
- Lane: Builder
- Depends on: [PBI-37](37-task-type.md), [PBI-16](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/16-repro-first-test.md) (PR #80) (this PBI is PBI-16's rule home)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter PlanLintTests` exits 0, and each of the 5 bad fixtures under `Fixtures/plans/` exits 1 with its own id (PLAN13 to PLAN17)
- door: two-way
- triage: build (Sven priority 1, 2026-10-26)
- feedback: `PlanLintTests` red with the expected id on every bad fixture, green on 4 good fixtures
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md#L7

## Problem now

PBI-16 asks for a repro step in bug-fix plans, but only as a contract test idea.
Nothing asks a refactor for a pin, a perf change for a baseline, or a feature for a data shape.

## Why (plain words)

The proof you need depends on the kind of work.
If the plan lacks it, the run should stop before any code is written, not after review.
A line in the plan is cheap to check by script.

## Example plan (bug-fix)

```markdown
Task type: bug-fix
Repro: api `curl -s -o /dev/null -w '%{http_code}' -X POST localhost:5080/orders -d @fixtures/expired-token.json` returns 500, expected 401
```

Other types:

```markdown
Task type: refactor
Pin: test `dotnet test --filter OrderTotalsCharacterizationTests`

Task type: perf
Baseline: trace `hyperfine -r 5 'agentpacks validate'` mean 4.1 s

Task type: feature
Data shape: `OrderStatus` state machine (Draft, Placed, Paid, Cancelled)
```

## Exact check-plan output

```text
docs/plans/2026-10-27-login-retry.md:3 PLAN13 bug-fix plan has no Repro: line (fix: add 'Repro: <api|test|trace> `<cmd>`')
docs/plans/2026-10-27-totals.md:3 PLAN14 refactor plan has no Pin: line (fix: add 'Pin: test `<cmd>`')
docs/plans/2026-10-27-validate-speed.md:3 PLAN15 perf plan has no Baseline: line (fix: add 'Baseline: <surface> `<cmd>` <metric>')
docs/plans/2026-10-27-order-status.md:3 PLAN16 feature plan has no Data shape: line (fix: add 'Data shape: <type or table>')
docs/plans/2026-10-27-login-retry.md:4 PLAN17 Repro: needs a surface and a command in backticks (fix: write '<api|test|trace> `<cmd>`')
```

Exit 1 when any line prints.

## Before / after

| Plan | Before | After |
|---|---|---|
| bug-fix, no Repro | passes | PLAN13, exit 1 |
| bug-fix, `Repro: it breaks sometimes` | passes | PLAN17, exit 1 |
| refactor with Pin | passes | passes |
| investigation | n/a (no plan) | n/a |

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter PlanLintTests` exits 0.
- [ ] Bad fixtures `bad-bugfix-no-repro` (PLAN13), `bad-refactor-no-pin` (PLAN14), `bad-perf-no-baseline` (PLAN15), `bad-feature-no-shape` (PLAN16), `bad-repro-no-command` (PLAN17) each print exactly their id and exit 1.
- [ ] Good fixtures `good-bug-fix-repro`, `good-refactor-pin`, `good-perf-baseline`, `good-feature-shape` print nothing and exit 0.
- [ ] PBI-16 gets one line: "Rule home: PLAN13 (AP39)". No second repro rule anywhere.
- [ ] `planning-contract.md` documents the 4 lines once.
- [ ] No em or en dashes.

## Technical design

- Module boundary: check-plan reads 4 line prefixes after `Task type:`. The planner writes them; the verifier reads them (AP40).
- Rules: PLAN13 to PLAN17.

## Update 2026-10-26 (poteto talk and podcast)

Sharpened for hill climbing (audit [audit G4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L109)). A perf plan may say `Loop: hillclimb`. Then it also needs `Target: <metric> <op> <number>`, `Max iterations: <n>` with n at most 5, and `Log: <path to decision.tsv>`. One change, one measurement, keep or revert; never claim a win from reading code.

Rule PLAN18 (error): `perf plan with Loop: hillclimb needs Target:, Max iterations: (<= 5) and Log: (fix: add the three lines)`.

```markdown
Task type: perf
Baseline: trace `./scripts/verify bench validate` mean seconds
Loop: hillclimb
Target: mean <= 2.5 s
Max iterations: 5
Log: docs/plans/2026-10-26-validate-speed.decision.tsv
```

DoD addition: `bash plugins/pack-check/skills/pack-check/scripts/check-plan.sh tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-hillclimb-no-target/2026-10-26-validate-speed.md` prints one PLAN18 line and exits 1.

Sources: [talk 03:56](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L19) (agent takes traces and hill climbs performance), [podcast about 18:01](https://www.youtube.com/watch?v=MN9dGgmLyso), pstack [`playbooks/hillclimb.md:3-16`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/hillclimb.md#L3-L16). CHECKLIST row 95 (hillclimb.md) changes from overslaan to aanpassen (PLAN18 only, no hillclimb playbook file).

## Sources

- pstack [`playbooks/bug-fix.md:7`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md#L7) (reproduce on the matching surface), [`refactoring.md:7`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/refactoring.md#L7) (pin first; type check and lint are not a pin), [`perf-issue.md:5`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/perf-issue.md#L5) (baseline first), [`feature.md:12`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/feature.md#L12) (named data shape), [`principle-model-the-domain/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-model-the-domain/SKILL.md).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-plan.ps1`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.ps1)
- edit: [`plugins/squad/skills/squad/references/planning-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/planning-contract.md)
- edit: [`tools/AgentPacks.Cli.Tests/PlanLintTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/PlanLintTests.cs)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-bugfix-no-repro/2026-10-27-login-retry.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-bugfix-no-repro/2026-10-27-login-retry.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-refactor-no-pin/2026-10-27-totals.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-refactor-no-pin/2026-10-27-totals.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-perf-no-baseline/2026-10-27-validate-speed.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-perf-no-baseline/2026-10-27-validate-speed.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-feature-no-shape/2026-10-27-order-status.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-feature-no-shape/2026-10-27-order-status.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-repro-no-command/2026-10-27-login-retry.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-repro-no-command/2026-10-27-login-retry.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-hillclimb-no-target/2026-10-26-validate-speed.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-hillclimb-no-target/2026-10-26-validate-speed.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/good-bug-fix-repro/2026-10-27-login-retry.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/good-bug-fix-repro/2026-10-27-login-retry.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/good-refactor-pin/2026-10-27-totals.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/good-refactor-pin/2026-10-27-totals.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/good-perf-baseline/2026-10-27-validate-speed.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/good-perf-baseline/2026-10-27-validate-speed.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/good-feature-shape/2026-10-27-order-status.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/good-feature-shape/2026-10-27-order-status.md)

Fixture layout is the AP37 one: one folder per case, one dated plan inside (check-plan needs the `<yyyy-mm-dd>-<slug>.md` name).

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/bug-fix.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/feature.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/feature.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/perf-issue.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/perf-issue.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/refactoring.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/refactoring.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/principle-model-the-domain/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-model-the-domain/SKILL.md)
