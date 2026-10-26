# PBI-41 (AP41): verify-harness Drive kinds api, cli, ui
- Status: todo
- Area: verify
- Phase: E verification
- Lane: Builder (second lane possible: files disjoint from 38 and 39)
- Depends on: PBI-32 (AP32, planned, not in the repo yet) soft (works without it), [PBI-20](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/20-harness-drift-check.md) (PR #80) (same linter, VER08)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerifyHarnessContractTests"` exits 0, and `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-cli-sleep/docs/verify.md` prints one VER06 line and exits 1
- door: two-way
- triage: build (Sven priority 2 on 2026-10-26: CLI and UI testing)
- feedback: `VerifyLintTests` red with VER05, VER06, VER07 on their fixtures, green on `good-mixed-kinds`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-cli/SKILL.md#L18-L27

## Problem now

`verify-harness` drives APIs only: "Drive defaults to API" and "never install or vendor browser tooling" ([`plugins/squad/skills/verify-harness/SKILL.md:26,37`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md#L26)).
A CLI change or a UI change has no scripted proof in the Squad loop.
AP32 pins `control-cli` and `control-ui`, but nothing in Squad uses them.

## Why (plain words)

Tester proves the real thing: screenshots for UI, real commands for CLIs.
Squad's verifier should do the same before the PR exists.
The two upstream skills already say how: one action at a time, wait for a real signal, save the transcript or screenshot.

## Decision applied (Sven, 2026-10-26)

Option (b): no `/verify-setup` or `/verify-upkeep`. `verify-harness` loads `control-cli` or `control-ui` by exact Skill name when the AP32 verify plugin is installed. Without it, the rules below still apply and the agent uses tmux or a short PTY script, or the repo's own Playwright.

## Example docs/verify.md Drive entries

```markdown
## Drive

### list pets
Kind: api
Run: `curl -s localhost:5080/pets`
Expect: 200, JSON array

### validate fails on a bad pin
Kind: cli
Run: `agentpacks validate --root fixtures/bad-pin`
Wait for: `error EXT`
Expect: exit 1, stderr names the pin
Transcript: `$(git rev-parse --git-dir)/squad/verify/<slug>/<run-id>/validate-bad-pin.txt`

### add to cart
Kind: ui
Run: `npx playwright test e2e/cart.spec.ts --grep "add to cart"` (repo already has Playwright)
Action: click `getByRole('button', { name: 'Add to cart' })`
Expect: `getByTestId('cart-count')` shows 1
Screenshot: `$(git rev-parse --git-dir)/squad/verify/<slug>/<run-id>/add-to-cart-after.png`
```

## Exact check-verify output

```text
docs/verify.md:14 VER05 Drive entry has no Kind: api, cli or ui (fix: add 'Kind: api|cli|ui')
docs/verify.md:22 VER06 cli Drive entry waits with sleep (fix: use 'Wait for: <prompt or line>')
docs/verify.md:31 VER07 ui Drive entry installs browser tooling or has no screenshot path (fix: use the repo's Playwright; add 'Screenshot:')
```

## Before / after

| | Before | After |
|---|---|---|
| CLI change | "Drive uses the CLI or tests" (`SKILL.md:40`), no shape | `Kind: cli` with `Wait for:` and a transcript file |
| UI change | not driven | `Kind: ui` with one action and a screenshot, only with existing Playwright |
| `npm i -D playwright` in Drive | allowed by accident | VER07 |

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerifyHarnessContractTests"` exits 0.
- [ ] Fixtures `bad-drive-no-kind`, `bad-cli-sleep`, `bad-ui-install` exit 1 with VER05, VER06, VER07. `good-mixed-kinds` exits 0.
- [ ] [`verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md) stays under its current length plus 8 lines; detail goes to `references/drive-kinds.md`.
- [ ] No em or en dashes.

## Technical design

- Module boundary: [`docs/verify.md`](../../../docs/verify.md) text in, check-verify findings out. Skills reached by exact name, never by slash.
- Rules: VER05 to VER07. VER08 stays reserved for PBI-20.

## Update 2026-10-26 (poteto talk and podcast)

Sharpened with a fourth kind, `trace`, for performance evidence (audit [audit M12](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L95), [audit G6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L111)). poteto's first skill drove the app over the Chrome DevTools Protocol to take traces and heap snapshots ([talk 03:09](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L17), [talk 07:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L27), [talk 08:40](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L31)).

A `Kind: trace` Drive entry names `Artifact:` (the trace, profile or heap snapshot path) and `Metric:` (what is read from it). It feeds `## Budgets` ([AP54](54-measured-budgets.md)).

```markdown
### open-chat (trace)
Kind: trace
Run: `./scripts/verify trace open-chat`
Artifact: `.verify/traces/open-chat.json`
Metric: longest renderer task in ms
```

Rule VER09 (error): `trace Drive entry needs Artifact: and Metric: (fix: add both)`. DoD addition: `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trace.md` prints one VER09 line and exits 1.

## Sources

- cursor-team-kit [`control-cli/SKILL.md:18-27`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-cli/SKILL.md#L18-L27) (harness loop), [`:105`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-cli/SKILL.md#L105) (deterministic waits over sleeps); [`control-ui/SKILL.md:25`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md#L25) (stable selectors), [`:42`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md#L42) (do not add Playwright for a probe), [`:75-82`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md#L75-L82) (one action, fresh snapshot); pstack [`poteto-mode/SKILL.md:30`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L30).
- benny [`reproduce-and-fix-issues/references/control-adapter.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md) (one adapter per surface).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- edit: [`plugins/squad/skills/verify-harness/references/process.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/references/process.md)
- new: [`plugins/squad/skills/verify-harness/references/drive-kinds.md`](../../../plugins/squad/skills/verify-harness/references/drive-kinds.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.ps1`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.ps1)
- edit: [`tools/AgentPacks.Cli.Tests/VerifyLintTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerifyLintTests.cs)
- edit: [`tools/AgentPacks.Cli.Tests/VerifyHarnessContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerifyHarnessContractTests.cs)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`cursor-team-kit/skills/control-cli/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-cli/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`cursor-team-kit/skills/control-ui/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/control-ui/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- agentPacks new file: [`docs/verify.md`](../../../docs/verify.md)
