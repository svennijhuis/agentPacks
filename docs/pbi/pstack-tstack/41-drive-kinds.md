# PBI-41 (AP41): verify-harness Drive kinds api and trace
- Status: todo
- Area: verify
- Phase: E verification
- Lane: Builder (second lane possible: files disjoint from 38 and 39)
- Depends on: [PBI-20](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/20-harness-drift-check.md) (PR #80) (same linter, VER08)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerifyHarnessContractTests"` exits 0, and `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-drive-no-kind.md` prints one VER05 line and exits 1
- door: two-way
- triage: build (Sven priority 2 on 2026-10-26; CLI and UI driving removed by Sven on 2026-11-01)
- feedback: `VerifyLintTests` red with VER05 and VER09 on their fixtures, green on `good-api-trace`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md

## Update 2026-11-01 (Sven)

Sven 2026-11-01: CLI and UI driving removed; API tests with setup and seed data belong in integration tests. Those run in the repo's own test project (for .NET: `WebApplicationFactory` plus Testcontainers, seeded in a fixture, run by `dotnet test`), not in a Drive entry. So `Kind: cli`, `Kind: ui`, VER06 and VER07 are gone (VER06 and VER07 are `retired` in [RULES.md](RULES.md)), and AP42 (`/scenarios` rows for CLI and UI) is dropped. Only `api` and `trace` stay.

## Problem now

`verify-harness` says "Drive defaults to API" ([`plugins/squad/skills/verify-harness/SKILL.md:26`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md#L26)), but a Drive entry has no fixed shape. Nothing says which kind of proof an entry gives, so the verifier cannot tell an API check from a performance measurement, and an entry without a kind passes the linter.

## Why (plain words)

Tester proves the real thing. Squad's verifier should do the same before the PR exists, and every Drive entry should say what it proves: a real API call, or a measured trace. One labeled entry per proof keeps the evidence readable and lintable.

## Decision applied (Sven, 2026-10-26, narrowed 2026-11-01)

Option (b): no `/verify-setup` or `/verify-upkeep`. `verify-harness` keeps driving APIs and adds one Kind line per Drive entry. Two kinds only: `api` and `trace`. No CLI or UI driving, no tmux or PTY loop, no browser tooling.

## Example docs/verify.md Drive entries

```markdown
## Drive

### list pets
Kind: api
Run: `curl -s localhost:5080/pets`
Expect: 200, JSON array
```

## Exact check-verify output

```text
docs/verify.md:14 VER05 Drive entry has no Kind: api or trace (fix: add 'Kind: api|trace')
```

## Before / after

| | Before | After |
|---|---|---|
| API Drive entry | free text, no kind | `Kind: api` with `Run:` and `Expect:` |
| Perf evidence | not driven | `Kind: trace` with `Artifact:` and `Metric:` (VER09) |
| Entry with no kind | passes | VER05 |

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerifyHarnessContractTests"` exits 0.
- [ ] Fixtures `bad-drive-no-kind` (VER05) and `bad-trace` (VER09) exit 1 with their id. `good-api-trace` exits 0.
- [ ] `check-verify` has no `cli` or `ui` kind and no VER06 or VER07 output.
- [ ] [`verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md) stays under its current length plus 8 lines; detail goes to `references/drive-kinds.md`.
- [ ] No em or en dashes.

## Technical design

- Module boundary: [`docs/verify.md`](../../../docs/verify.md) text in, check-verify findings out.
- Rules: VER05 and VER09. VER06 and VER07 are retired (2026-11-01). VER08 stays reserved for PBI-20.

## Update 2026-10-26 (poteto talk and podcast)

Sharpened with a fourth kind, `trace`, for performance evidence (audit [audit M12](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L95), [audit G6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L111)). poteto's first skill drove the app over the Chrome DevTools Protocol to take traces and heap snapshots ([talk 03:09](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L17), [talk 07:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L27), [talk 08:40](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L31)).

A `Kind: trace` Drive entry names `Artifact:` (the trace, profile or heap snapshot path) and `Metric:` (what is read from it). It feeds `## Budgets` ([AP54](54-measured-budgets.md)). VER05 accepts `api|trace`, so a valid trace entry passes VER05 and only VER09 checks its two lines.

```markdown
### open-chat (trace)
Kind: trace
Run: `./scripts/verify trace open-chat`
Artifact: `.verify/traces/open-chat.json`
Metric: longest renderer task in ms
```

Rule VER09 (error): `trace Drive entry needs Artifact: and Metric: (fix: add both)`. DoD addition: `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trace.md` prints one VER09 line and exits 1.

## Sources

- benny [`reproduce-and-fix-issues/references/control-adapter.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md) (one adapter per surface; here only api and trace); pstack [`poteto-mode/SKILL.md:30`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L30).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- edit: [`plugins/squad/skills/verify-harness/references/process.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/references/process.md)
- new: [`plugins/squad/skills/verify-harness/references/drive-kinds.md`](../../../plugins/squad/skills/verify-harness/references/drive-kinds.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.ps1`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.ps1)
- edit: [`tools/AgentPacks.Cli.Tests/VerifyLintTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerifyLintTests.cs)
- edit: [`tools/AgentPacks.Cli.Tests/VerifyHarnessContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerifyHarnessContractTests.cs)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-drive-no-kind.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-drive-no-kind.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/good-api-trace.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/good-api-trace.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trace.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trace.md)

## Links

- cursor/plugins (pstack): [`pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/references/control-adapter.md)
- cursor/plugins (pstack): [`pstack/skills/poteto-mode/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- agentPacks new file: [`docs/verify.md`](../../../docs/verify.md)
