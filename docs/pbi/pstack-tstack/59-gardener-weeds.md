# PBI-59 (AP59): Gardener: weed log and sampled review of merged PRs
- Status: todo
- Area: squad
- Phase: G review loop
- Lane: Builder; Reviewer bot runs it (room charter)
- Depends on: [AP51](51-finding-guard-layer.md), [PBI-17](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/17-retro-repeat-to-check.md) (PR #80)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-retro.sh tools/AgentPacks.Cli.Tests/Fixtures/retro/bad-weeds.md` exits 1 with one RETRO04 line, and `rg -n "sample <n>" plugins/squad/commands/squad-review.md` prints 1 line
- door: two-way (report only, no code changes)
- triage: build (Sven must-cover theme 7)
- feedback: `PackCheckContractTests` good and bad weeds fixture
- harness: none (report only)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 24:16-25:11, D42 (proposed) https://www.youtube.com/watch?v=MN9dGgmLyso

## Problem now

`/squad-review` reviews one diff when someone asks ([`plugins/squad/skills/squad/SKILL.md:52`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L52)). Nobody looks at merged work for patterns, and there is no place to queue smells.
The audit found no sampling in agentPacks ([audit 20](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L64), [audit G3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L108)).

## Why (plain words)

"Every team really needs ... a gardener": someone who removes weeds and pests before they spread ([talk 24:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L69), [talk 25:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L71)).
Podcast: an agent appends banned patterns to a document instead of fixing each one, and every few days she sees "these are all the same thing" ([podcast about 47:41](https://www.youtube.com/watch?v=MN9dGgmLyso)). Review becomes sampling, and repeats change the kitchen, not the one cook ([podcast about 49:34](https://www.youtube.com/watch?v=MN9dGgmLyso), [podcast about 50:33](https://www.youtube.com/watch?v=MN9dGgmLyso)).

## What changes

- No new slash command (4-slash rule). `/squad-review sample <n>`: report-only review of the last `n` merged PRs on the default branch (`gh pr list --state merged --limit <n>`).
- Output: rows appended to `docs/weeds.md` in the consumer repo: date, PR, `file:line`, smell, Guard ([AP51](51-finding-guard-layer.md)), seen count.
- Same smell seen 2 or more times: the retro proposes the check ([PBI-17](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/17-retro-repeat-to-check.md) (PR #80), RETRO03). One-off: count stays 1, nothing to do.
- Who runs it: the Reviewer bot, weekly, `n = 3`. That is room charter, not repo code.

## Example

```markdown
| Date | PR | Where | Smell | Guard | Seen |
|---|---|---|---|---|---|
| 2026-10-26 | #81 | src/Api/Orders.cs:40 | catch and return null | lint | 2 |
```

Bad (RETRO04): a row without Guard or without `file:line`.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-retro.sh tools/AgentPacks.Cli.Tests/Fixtures/retro/bad-weeds.md` prints `bad-weeds.md:4 RETRO04 weeds row needs Date, PR, file:line, Smell, Guard, Seen (fix: fill the row)` and exits 1.
- [ ] `rg -n "sample <n>" plugins/squad/commands/squad-review.md` prints 1 line.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter PackCheckContractTests` exits 0.

## Technical design

- Module boundary: `/squad-review` writes; check-retro.sh checks; squad-retro reads counts. Report only, no fix round.
- Rules: RETRO04.
- Same idea on the gh-aw side: [agentic-workflows F74](https://github.com/svennijhuis/agentic-workflows/blob/5ef9b6add0f09dd8aea0388896a2b6674295db95/docs/pbi/agent-factory/F74-findings-buffer.md), [AO10](https://github.com/svennijhuis/agentic-workflows/blob/5ef9b6add0f09dd8aea0388896a2b6674295db95/docs/pbi/agent-ops/AO10-sampled-human-review.md). This PBI is the local Squad version.

## Sources

- Talk: [talk 24:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L69), [talk 25:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L71). Notes: [notes 4.9](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L66), [notes 7.1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L103), [notes 9.6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L123).
- Podcast: [podcast about 45:51](https://www.youtube.com/watch?v=MN9dGgmLyso) (gardening), [podcast about 47:41](https://www.youtube.com/watch?v=MN9dGgmLyso) (queue), [podcast about 49:34](https://www.youtube.com/watch?v=MN9dGgmLyso) (sampling), [podcast about 50:33](https://www.youtube.com/watch?v=MN9dGgmLyso). Audit [audit 18](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L62), [audit 19](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L63), [audit 20](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L64), [audit M5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L88), [audit G3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L108).
- tstack tstack [`modernize/SKILL.md:50`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/modernize/SKILL.md#L50) (two ways to do one thing).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-retro.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-retro.sh)
- edit: [`plugins/squad/agents/squad-retro.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-retro.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/retro/bad-weeds.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/retro/bad-weeds.md)
