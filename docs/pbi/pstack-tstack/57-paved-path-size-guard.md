# PBI-57 (AP57): One paved path: paved-paths table and a size guard
- Status: todo
- Area: squad, pack-check
- Phase: H architecture
- Lane: Builder
- Depends on: [AP62](62-patterns-catalog.md) (PA02, PA04), [AP63](63-rules-catalog.md) (R13, R14), [AP50](50-open-decisions.md) L4 (size number)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-diff.sh --base origin/main` in the fixture repo prints one DIFF01 warning and one DIFF05 warning, exit 0; `dotnet test tools/AgentPacks.Cli.Tests --filter DiffLintTests` exits 0
- door: two-way
- triage: build (Sven must-cover theme 5)
- feedback: `DiffLintTests` (new) on a fixture git repo
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 25:05-25:50 and 26:42

## Problem now

Only the .NET pack has a "where new code goes" table ([`plugins/dotnet/standards/layers.md:87-99`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/dotnet/standards/layers.md#L87-L99)). A consumer repo has no single list of where each kind of code lives.
No check flags a god file. The audit found no PBI for it ([audit 9](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L53), [audit G2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L107)).

## Why (plain words)

Dune principle 2: "one conventional way to do some things so that agents don't really need to guess", with enough guidance in code, CI and lint ([talk 25:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L71), [talk 25:50](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L73)). Features are co-located in one folder ([talk 26:42](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L75)).
Podcast: "there's really only one way to do something", and early Grok Bot was "eight god files ... at least 10,000 lines long" ([podcast, after 27:09](https://www.youtube.com/watch?v=MN9dGgmLyso)).

## What changes

1. Consumer repo file `docs/paved-paths.md`, seeded by `squad-planner` on the first full run when missing:

| Concern | Path | Example |
|---|---|---|
| HTTP endpoint | `src/Api/<Feature>Endpoints.cs` | `src/Api/OrdersEndpoints.cs` |
| Use case | `src/Application/<Feature>/` | `src/Application/Orders/CreateOrder.cs` |

2. New script `plugins/pack-check/skills/pack-check/scripts/check-diff.sh` (and `.ps1`), prefix DIFF, runs on `git diff --base`:
   - DIFF01 warn: new file over 400 lines.
   - DIFF02 warn: file grew past 400 lines in this diff.
   - DIFF05 warn: new file in a top-level folder that no paved-paths row covers.
3. `squad-verifier` runs check-diff.sh with the other pack-check scripts.

## Example output

```text
src/Orders/OrderService.cs:1 DIFF02 warning: file grew from 380 to 455 lines (limit 400) (fix: split by feature folder)
tools/helpers/x.cs:1 DIFF05 warning: tools/ is not in docs/paved-paths.md (fix: move the file or add a row)
```

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-diff.sh --base origin/main` in `tools/AgentPacks.Cli.Tests/Fixtures/diff-repo/` prints exactly the two lines above and exits 0.
- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-diff.sh --base nope` exits 2 (usage).
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter DiffLintTests` exits 0.
- [ ] Number 400 comes from AP50 L4; `rg -n '400' plugins/pack-check/skills/pack-check/scripts/check-diff.sh` prints 1 line (one constant).

## Technical design

- Module boundary: check-diff.sh reads git only; paved-paths.md is consumer data. One constant for the size limit.
- Rules: DIFF01, DIFF02, DIFF05. DIFF03 and DIFF04 come from [AP58](58-lint-first-ratchet.md) in the same script.

## Sources

- Talk: [talk 25:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L71), [talk 25:50](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L73), [talk 26:42](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L75). Notes: [notes 4.4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L61), [notes 4.11](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L68), [notes 4.13](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L70).
- Podcast: [podcast, after 27:09](https://www.youtube.com/watch?v=MN9dGgmLyso). Audit [audit 9](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L53), [audit G2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L107).
- pstack pstack [`correct/SKILL.md:19`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L19) ("each task one supported way"), pstack [`principle-minimize-reader-load/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-minimize-reader-load/SKILL.md).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/agents/squad-planner.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-planner.md)
- edit: [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- new (planned): [`plugins/pack-check/skills/pack-check/scripts/check-diff.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-diff.sh)
- new (planned): [`plugins/pack-check/skills/pack-check/scripts/check-diff.ps1`](../../../plugins/pack-check/skills/pack-check/scripts/check-diff.ps1)
- new (planned): [`tools/AgentPacks.Cli.Tests/DiffLintTests.cs`](../../../tools/AgentPacks.Cli.Tests/DiffLintTests.cs)
