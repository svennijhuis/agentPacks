# PBI-60 (AP60): Design for low-context agents: canonical example per concern
- Status: todo
- Area: squad
- Phase: H architecture
- Lane: Builder
- Depends on: [AP57](57-paved-path-size-guard.md) (paved-paths table)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-plan.sh tools/AgentPacks.Cli.Tests/Fixtures/plan/warn-no-example.md` prints one PLAN19 warning, and `dotnet test tools/AgentPacks.Cli.Tests --filter "PlanLintTests|SquadContractTests"` exits 0
- door: two-way
- triage: build (Sven must-cover themes 5 and 9)
- feedback: `PlanLintTests`
- harness: one manual low-context dry run (DoD)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 15:56-16:43, 19:53, 31:21

## Problem now

The implementer starts from a `file:line` list (open PR #77) and copies the nearest code. Nothing says which file is the right example.
If the nearest file holds a workaround, the agent copies the workaround.

## Why (plain words)

"The code base is really the best form of memory": agents extend what they see in their context, they don't refactor in every PR ([talk 15:04](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L47), [talk 15:56](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L49)).
Contributors are now designers, PMs and CEOs, so agents "piloted by busy people with not a lot of context" must still do well by default ([talk 19:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L59)). In a good codebase "even an agent with very little context ... can come in and actually write code that's good" ([talk 31:21](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L85)).
pstack `correct`: "Assume every contributor is an agent that sees only the files it opened, copies the nearest example" (pstack [`correct/SKILL.md:11`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L11)).

## What changes

1. `docs/paved-paths.md` ([AP57](57-paved-path-size-guard.md)) column `Example` must point to an existing file that passes all guards.
2. `squad-implementer`: before writing a new file, read the Example of its concern. Copy that, not the nearest file.
3. check-plan PLAN19 warn: a slice `Files:` line creates a file under a concern whose row has no Example.
4. Reviewer checklist line: "copied a non-canonical example" is a finding with Guard `architecture`.

## Example

Before: implementer adds `src/Api/RefundsController.cs` by copying `LegacyController.cs` (has a try/catch workaround).
After: paved-paths row `HTTP endpoint | src/Api/<Feature>Endpoints.cs | src/Api/OrdersEndpoints.cs`; implementer copies OrdersEndpoints.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-plan.sh tools/AgentPacks.Cli.Tests/Fixtures/plan/warn-no-example.md` prints `warn-no-example.md:31 PLAN19 warning: slice creates src/Api/X.cs but the concern has no Example (fix: add Example to docs/paved-paths.md)` and exits 0.
- [ ] `rg -n 'Example of its concern' plugins/squad/agents/squad-implementer.md` prints 1 line.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "PlanLintTests|SquadContractTests"` exits 0.
- [ ] One manual dry run, written in the PR body: implementer gets only the slice files plus the Example, and the result passes the reviewer.

## Technical design

- Module boundary: paved-paths.md is data; check-plan.sh reads it when present; implementer text names the step once.
- Rules: PLAN19.

## Sources

- Talk: [talk 15:04](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L47), [talk 15:56](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L49), [talk 19:04](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L57), [talk 19:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L59), [talk 29:44](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L81), [talk 31:21](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L85). Notes: [notes 4.2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L59), [notes 4.3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L60), [notes 4.5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L62), [notes 4.18](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L75), [notes 8.1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L110), [notes 8.2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L111).
- Podcast: [podcast about 46:12](https://www.youtube.com/watch?v=MN9dGgmLyso) (new hires productive from day one). Audit [audit M10](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L93).
- pstack pstack [`correct/SKILL.md:11`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L11).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/agents/squad-implementer.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-implementer.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh)
- edit: [`tools/AgentPacks.Cli.Tests/PlanLintTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/PlanLintTests.cs)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/plan/warn-no-example.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plan/warn-no-example.md)
