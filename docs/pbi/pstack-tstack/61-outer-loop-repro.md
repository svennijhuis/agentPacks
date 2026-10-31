# PBI-61 (AP61): Outer loop: report, reproduce, then a Squad bug-fix
- Status: todo
- Area: squad
- Phase: J outer loop
- Lane: Builder (skill); Sven (routine wiring)
- Depends on: [AP52](52-feature-map.md), [AP40](40-same-surface-reverify.md), [AP56](56-trust-level.md) (T1 or higher)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter SquadContractTests` exits 0 with SQD06 green, and `rg -n "reproduced, not reproduced, needs info" plugins/squad/skills/squad-repro/SKILL.md` prints 1 line
- door: two-way for the skill; one-way for the routine (tokens, writes to Slack or GitHub; Sven yes, rollback: pause the routine)
- triage: build (Sven must-cover theme 10)
- feedback: `SquadContractTests` SQD06
- harness: the skill drives the Features map (AP52)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 32:08-35:10 and D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/SKILL.md#L9-L27

## Problem now

agentPacks has no skill a routine can call with a bug report. Today a human reads Slack, then starts `/squad`.
The gh-aw side has issue triage with repro ([agentic-workflows F72](https://github.com/svennijhuis/agentic-workflows/blob/5ef9b6add0f09dd8aea0388896a2b6674295db95/docs/pbi/agent-factory/F72-issue-triage-repro.md)), but that is for GitHub issues only.

## Why (plain words)

poteto: Grok Bot is the outer loop. Connectors (Slack, Datadog, Sentry) bring signals, routines subscribe to alerts, and cloud agents reproduce bug reports and open pull requests ([talk 32:08](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L87), [talk 32:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L89), [talk 33:39](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L91), [talk 34:25](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L93)). You don't need a "company brain" ([talk 32:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L89)).
pstack benny: reproduce through the real app, "No confirmed repro means no authored fix", prefer no ticket over a duplicate (pstack [`reproduce-and-fix-issues/SKILL.md:9-27`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/SKILL.md#L9-L27), pstack [`triage-issue-reports/SKILL.md:26`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/triage-issue-reports/SKILL.md#L26)).

## What changes

1. Hidden skill `plugins/squad/skills/squad-repro/` (no slash, `user-invocable: false`). Input: report text, optional screenshot, repo. Steps: match the report to a Features row (AP52), run its Drive on the default branch, write a verdict: `reproduced`, `not reproduced` or `needs info`, with evidence.
2. Only on `reproduced` and repo Level T1 or higher (AP56): start `/squad` with `Task type: bug-fix` and the `Repro:` line filled ([AP39](39-per-type-plan-gates.md)). The PR stays a draft; merge rules do not change.
3. Wiring (outside this repo): a Grok Bot routine on a Slack channel or Sentry alert calls the skill through a Cursor cloud agent. Named ask for Sven.

## Example

```text
Report: "export button does nothing ???" + screenshot
Match: Features row "Export CSV" (Reach: Orders page, Export)
Drive: ./scripts/verify drive export-csv (Kind: api, POST /orders/export)  -> 500, no file
Verdict: reproduced (main 34e1dec). Next: /squad Task type: bug-fix, Repro: api `./scripts/verify drive export-csv`
```

Bad: a draft PR opened with `Verdict: not reproduced`.

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter SquadContractTests` exits 0; SQD06 checks the skill exists, has `user-invocable: false`, and names the three verdicts.
- [ ] `rg -n "reproduced, not reproduced, needs info" plugins/squad/skills/squad-repro/SKILL.md` prints 1 line.
- [ ] `rg -c '^name:' plugins/squad/commands/*.md` count is unchanged (no new slash).
- [ ] Routine wiring: not in this PBI, a named one-way ask in the PR body.

## Technical design

- Module boundary: the skill decides reproduced or not; `/squad` does the fix. The routine is only a trigger.
- Rules: SQD06.

## Sources

- Talk: [talk 32:08](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L87), [talk 32:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L89), [talk 33:39](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L91), [talk 34:25](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L93), [talk 35:10](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L95). Notes: [notes 0.7](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L16), [notes 2.9](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L39), [notes 2.11](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L41), [notes 6.1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L92), [notes 6.3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L94), [notes 6.4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L95), [notes 6.5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L96).
- Podcast: [podcast about 36:12](https://www.youtube.com/watch?v=MN9dGgmLyso) (outer loop), [podcast about 37:58](https://www.youtube.com/watch?v=MN9dGgmLyso) (where am I the bottleneck). Audit [audit 14](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L58), [audit 15](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L59), [audit 17](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L61).
- pstack pstack [`reproduce-and-fix-issues/SKILL.md:9-27`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/reproduce-and-fix-issues/SKILL.md#L9-L27), pstack [`triage-issue-reports/SKILL.md:115`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/automations/benny/skills/triage-issue-reports/SKILL.md#L115).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- new (planned): [`plugins/squad/skills/squad-repro/SKILL.md`](../../../plugins/squad/skills/squad-repro/SKILL.md)
- edit: [`tools/AgentPacks.Cli.Tests/SquadContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/SquadContractTests.cs)
- edit: [`plugins/squad/README.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/README.md)
