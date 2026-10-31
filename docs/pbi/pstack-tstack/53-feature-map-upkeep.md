# PBI-53 (AP53): Feature map upkeep: Source paths and a scheduled pass
- Status: todo
- Area: squad
- Phase: F verification
- Lane: Builder (docs + script); Sven (schedule yes)
- Depends on: [AP52](52-feature-map.md), [PBI-20](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/20-harness-drift-check.md) (PR #80)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-dead-source.md` exits 1 with one VER12 line
- door: two-way for the rule; one-way for the scheduled pass (tokens + schedule, Sven yes, rollback: delete the routine)
- triage: build
- feedback: `VerifyLintTests`
- harness: verify-harness upkeep mode on the fixture repo
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 11:02-12:41 and D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/maintain-verification-skill/SKILL.md#L9-L21

## Problem now

After [AP52](52-feature-map.md) the map exists, but nothing keeps it true. [PBI-20](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/20-harness-drift-check.md) (PR #80) checks the harness commands, not the features.
A map that is wrong is worse than no map, because the agent trusts it.

## Why (plain words)

poteto: the feature map is stored with the skill "and we have an automation that maintains this feature map as well". The control skills became "critical infrastructure" that the team keeps maintaining ([talk 11:02](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L37), [talk 11:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L39)).
pstack `maintain-verification-skill`: "A feature map rots the moment the app changes", outcome clean, changed or blocked, never edit product code during the pass (pstack [`maintain-verification-skill/SKILL.md:9-21`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/maintain-verification-skill/SKILL.md#L9-L21)).

## What changes

1. Rule: every Features row has `Source` paths, and each path exists (VER12).
2. In `/squad`: when the diff touches a path listed in a row's Source, the verifier runs that row's Drive and reports `feature map stale: <row>` under Evidence gaps when Reach or Proof changed. The verifier stays read-only ([`squad-verifier.md:5`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L5) `readonly: true`, [`:13`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L13), [`:34`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L34) never writes `docs/verify.md`). The main agent then runs verify-harness in upkeep mode, which is the only writer: it updates that row in `docs/verify.md` and nothing else.
3. Scheduled pass (outside the repo, Sven yes needed): a Grok Bot routine or Cursor automation runs verify-harness in upkeep mode once a week. Result `clean`, `changed` (one docs PR) or `blocked` (a note to the room). No product code edits.

## Example

```markdown
| Export CSV | Orders page, "Export" button | `./scripts/verify drive export-csv` | file `orders-*.csv` | src/Export/ |
```

After `src/Export/` moved to `src/Features/Export/`: VER12 `docs/verify.md:14 VER12 Source path src/Export/ does not exist (fix: update the row)`.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-dead-source.md` prints `bad-dead-source.md:9 VER12 Source path src/Export/ does not exist (fix: update the row)` and exits 1.
- [ ] `rg -n 'clean, changed or blocked' plugins/squad/skills/verify-harness/references/process.md` prints 1 line.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter VerifyLintTests` exits 0.
- [ ] Scheduled pass: not built in this PBI. A named ask for Sven in the PR body (one-way).

## Technical design

- Module boundary: check-verify.sh checks paths relative to the repo root. The verifier reads and reports a stale row; verify-harness upkeep mode is the only writer of `docs/verify.md`. squad-verifier.md keeps `readonly: true` and its L13 and L34 lines; its edit here is one report line (`feature map stale: <row>`).
- Rules: VER12.

## Sources

- Talk: [talk 11:02](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L37), [talk 11:53](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L39). Notes: [notes 2.10](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L40), [notes 2.12](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L42).
- Podcast: [podcast about 16:14](https://www.youtube.com/watch?v=MN9dGgmLyso) (every app has an auto-maintained verification skill). Audit [audit 3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L47), [audit M15](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L98), [audit G5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L110).
- pstack pstack [`maintain-verification-skill/SKILL.md:9-27`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/maintain-verification-skill/SKILL.md#L9-L27).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- edit: [`plugins/squad/skills/verify-harness/references/process.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/references/process.md)
- edit: [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-dead-source.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-dead-source.md)
