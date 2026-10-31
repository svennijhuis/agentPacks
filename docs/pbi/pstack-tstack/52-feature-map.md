# PBI-52 (AP52): Feature map in docs/verify.md, Drive calls the verify CLI
- Status: todo
- Area: squad
- Phase: F verification
- Lane: Builder
- Depends on: [AP41](41-drive-kinds.md) (Kind column). Soft: AP33 (planned, not in the repo yet) for the CLI standard
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-no-features.md` prints one VER10 line, and `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerifyHarnessContractTests"` exits 0
- door: two-way
- triage: build (Sven must-cover theme 2)
- feedback: `VerifyLintTests` good and bad fixtures
- harness: verify-harness dry run on one feature (existing step 3)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 08:40-11:53 and D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/create-verification-skill/SKILL.md#L34-L36

## Problem now

`verify-harness` writes five H2s: Launch, Doctor, Drive, Evidence, Cleanup ([`plugins/squad/skills/verify-harness/SKILL.md:18`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md#L18)).
There is no list of features. When a report says "the export is broken", the agent has a way to run the app, but it guesses which Drive entry matches.
Drive entries can hold inline scripts. Each session writes the script a bit differently.

## Why (plain words)

poteto's verification skill has two parts: a CLI inside the skill folder, so every run is the same, and a feature map: what the app does, how a user reaches each feature, and what each feature does. She calls the map "materialized memory". It came from vague bug reports with a tiny screenshot and three question marks ([talk 08:40](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L31), [talk 09:30](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L33), [talk 10:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L35), [talk 11:02](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L37)).
pstack ships the same shape: one file per feature with How to get to it, Driving it, Gotchas (pstack [`create-verification-skill/SKILL.md:34-36`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/create-verification-skill/SKILL.md#L34-L36), pstack [`feature-map-example/README.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/create-verification-skill/references/feature-map-example/README.md)).
In the podcast: "every agent did it differently", so the CLI went into the skill ([podcast about 22:47](https://www.youtube.com/watch?v=MN9dGgmLyso), [audit 5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L49)).

## What changes

1. `docs/verify.md` gets a sixth H2, `## Features`, after `## Drive`. One table, one row per user-facing feature:

| Column | Content |
|---|---|
| Feature | short name |
| Reach | how a user gets there (route, command, menu, endpoint) |
| Drive | one command, through the verify CLI when the repo has one (AP33 standard, planned) |
| Proof | the observable end state |
| Source | code paths that implement it (used by [AP53](53-feature-map-upkeep.md)) |

2. Seed 3 to 5 rows on first run, from routes, commands or the README. Never weaken a Drive to make it pass; the reason goes in a Gotchas column note.
3. Drive entries longer than 3 lines of inline shell move into the verify CLI.

## Example

```markdown
## Features

| Feature | Reach | Drive | Proof | Source |
|---|---|---|---|---|
| Create order | POST /orders | `./scripts/verify drive create-order` | 201 + Location header; row in `orders` | src/Orders/, src/Api/OrdersEndpoints.cs |
| Export CSV | Orders page, "Export" button | `./scripts/verify drive export-csv` | file `orders-*.csv` with header line | src/Export/ |
```

Bad (VER10): `docs/verify.md` without `## Features`. Bad (VER11, warn): a Drive entry with a 12-line heredoc.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-no-features.md` prints `bad-no-features.md:1 VER10 warning: missing ## Features (fix: add the feature table)` and exits 0 (warn for one release, then error; AP50 decides the date).
- [ ] Same script on `bad-feature-row.md` (row without Proof) prints one VER10 line.
- [ ] Same script on `warn-inline-drive.md` prints one `VER11 warning:` line.
- [ ] `good-features.md` prints nothing, exit 0.
- [ ] `rg -n '## Features' plugins/squad/skills/verify-harness/SKILL.md` prints at least 1 line.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "VerifyLintTests|VerifyHarnessContractTests"` exits 0.

## Technical design

- Module boundary: verify-harness writes the table; check-verify.sh checks it; the verifier and [AP61](61-outer-loop-repro.md) read it.
- Rules: VER10, VER11.
- The map lives in the product repo (`docs/verify.md`), not in a skill folder, because Squad already treats `docs/verify.md` as the harness. Same idea as poteto's skill folder, one file less.

## Sources

- Talk: [talk 08:40](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L31) (control skill, CLI), [talk 09:30](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L33) (same CLI every time, empirical evidence), [talk 10:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L35) (feature map, vague screenshots), [talk 11:02](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L37) (materialized memory, lives in the skill dir). Notes: [notes 2.3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L33), [notes 2.5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L35), [notes 2.8](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L38), [notes 2.9](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L39), [notes 8.3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L112), [notes 9.2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L119).
- Podcast: [podcast about 21:03](https://www.youtube.com/watch?v=MN9dGgmLyso) (deterministic parts into a CLI), [podcast about 22:47](https://www.youtube.com/watch?v=MN9dGgmLyso) (rebuild the world each time). Audit [audit 4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L48), [audit 5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L49).
- pstack pstack [`create-verification-skill/SKILL.md:15-18`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/create-verification-skill/SKILL.md#L15-L18), pstack [`create-verification-skill/SKILL.md:34-36`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/create-verification-skill/SKILL.md#L34-L36).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.ps1`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.ps1)
- edit: [`tools/AgentPacks.Cli.Tests/VerifyLintTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/VerifyLintTests.cs)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-no-features.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-no-features.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-feature-row.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-feature-row.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/warn-inline-drive.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/warn-inline-drive.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/good-features.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/good-features.md)
