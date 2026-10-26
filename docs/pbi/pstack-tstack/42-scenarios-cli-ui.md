# PBI-42 (AP42): /scenarios rows for CLI and UI
- Status: todo
- Area: squad
- Phase: E verification
- Lane: Builder
- Depends on: [PBI-41](41-drive-kinds.md)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter HttpScenariosContractTests` exits 0, and `bash plugins/pack-check/skills/pack-check/scripts/check-scenarios.sh tools/AgentPacks.Cli.Tests/Fixtures/scenarios/bad-cli-no-exit/docs/smoke/validate.md` prints one SCN02 line and exits 1
- door: two-way
- triage: build (Sven priority 2, 2026-10-26)
- feedback: `HttpScenariosContractTests` red with SCN01 to SCN03 on fixtures, green on `good-mixed`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md#L75-L82

## Problem now

`/scenarios` writes HTTP and timer rows only ([`plugins/squad/skills/scenarios-md/SKILL.md:3,17-34`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/scenarios-md/SKILL.md#L3)).
A CLI change or a UI change gets no rows, so Tester gets no list to test from.

## Why (plain words)

The smoke file is what a human tester or Tester bot runs.
If the change is a command or a screen, the file must say which command or which button, and what you should see.

## New rows (same table, same columns)

| # | Case | Kind | Request | Status | Expected | Why |
|---|---|---|---|---|---|---|
| 6 | bad pin fails | fail | CLI `agentpacks validate --root fixtures/bad-pin` | exit 1 | stderr names [`plugins/squad/external-skills.json`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/external-skills.json) | documented pin rule |
| 7 | help lists validate | happy | CLI `agentpacks --help` | exit 0 | stdout has `validate` | discoverability |
| 8 | add to cart | happy | UI `/products/42` click `getByRole('button', { name: 'Add to cart' })` | visible | `getByTestId('cart-count')` = 1 | core flow |
| 9 | empty cart message | edge | UI `/cart` with no items | visible | `getByText('Your cart is empty')` | empty is valid |

Request starts with `CLI` or `UI` for these rows. HTTP rows stay as today (`GET /pets`).
Kinds stay `happy, edge, fail, auth, biz, nothing-breaks`.

## Exact check-scenarios output

```text
docs/smoke/validate.md:12 SCN01 unknown Kind 'smoke' (fix: use happy, edge, fail, auth, biz, nothing-breaks)
docs/smoke/validate.md:13 SCN02 CLI row has no exit code (fix: fill Status with 'exit <n>')
docs/smoke/cart.md:9 SCN03 UI row has no selector (fix: add a role, label or data-* selector)
```

Secrets: `/scenarios` also runs `check-verify.sh` on the smoke file for VER02 and VER03. check-scenarios does not copy the secret rule.

## Before / after

| | Before | After |
|---|---|---|
| CLI change | no rows | rows with command and exit code |
| UI change | no rows | rows with route, one action, selector and visible result |
| HTTP change | rows | same rows |

## Definition of Done

- [ ] `check-scenarios.sh` + `.ps1` exist, print the README format, exit 0, 1 or 2.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter HttpScenariosContractTests` exits 0 with the 3 bad fixtures red.
- [ ] [`scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/scenarios-md/SKILL.md) description says "HTTP, timer, CLI or UI"; no new command; `/scenarios` text unchanged apart from the kinds.
- [ ] No em or en dashes.

## Technical design

- Module boundary: `docs/smoke/<slug>.md` table in, findings out. Drive entries in [`docs/verify.md`](../../../docs/verify.md) match rows by Case name (AP41).
- Rules: SCN01 to SCN03.

## Sources

- cursor-team-kit [`control-cli/SKILL.md:18-27`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-cli/SKILL.md#L18-L27), [`control-ui/SKILL.md:25,75-82`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md#L25); pstack `skills/create-verification-skill/references/feature-map-example/*.md` (user POV: how to reach it, what proves it).
- agentPacks [`plugins/squad/skills/scenarios-md/SKILL.md:17-34`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/scenarios-md/SKILL.md#L17-L34), [`plugins/squad/references/smoke-matrix.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/references/smoke-matrix.md).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/skills/scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/scenarios-md/SKILL.md)
- edit: [`plugins/squad/references/smoke-matrix.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/references/smoke-matrix.md)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-scenarios.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-scenarios.sh)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-scenarios.ps1`](../../../plugins/pack-check/skills/pack-check/scripts/check-scenarios.ps1)
- edit: [`tools/AgentPacks.Cli.Tests/HttpScenariosContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/HttpScenariosContractTests.cs)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`cursor-team-kit/skills/control-cli/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-cli/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`cursor-team-kit/skills/control-ui/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/external-skills.json`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/external-skills.json)
- agentPacks main (existing files): [`plugins/squad/references/smoke-matrix.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/references/smoke-matrix.md)
- agentPacks main (existing files): [`plugins/squad/skills/scenarios-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/scenarios-md/SKILL.md)
- agentPacks new file: [`docs/verify.md`](../../../docs/verify.md)
