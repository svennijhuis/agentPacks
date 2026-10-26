# PBI-51 (AP51): Every review finding names its guard layer (correction ladder)
- Status: todo
- Area: squad
- Phase: G review loop
- Lane: Builder
- Depends on: [AP47](47-review-ledger.md) (finding line, check-review.sh), [AP50](50-open-decisions.md) L2 (severity words); feeds [PBI-17](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/17-retro-repeat-to-check.md) (PR #80)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-review.sh tools/AgentPacks.Cli.Tests/Fixtures/review/bad-no-guard.md` exits 1 with one REV12 line, and `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0
- door: two-way
- triage: build (talk closing slide, Sven must-cover theme 1)
- feedback: `ReviewLoopTests` runs check-review on a good and a bad fixture
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 15:04-18:44 and 35:58-36:55

## Problem now

A finding has Location, Severity, Problem, Fix and Confidence ([`plugins/squad/skills/squad/references/review-contract.md:25-34`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L25-L34)).
Nothing in the finding says where the lesson should live after the fix.
The retro has the enforcer ladder ([`plugins/squad/skills/squad/references/retro.md:47-53`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/retro.md#L47-L53)), but it only looks at one run.
So a reviewer fixes the same kind of mistake again and again, and the only "guard" is the reviewer's memory. That is the style-guide layer, the weakest one.

## Why (plain words)

poteto's one-slide takeaway: every time you correct an agent, ask which layer stops it for good. In this order: architecture and data structures, static analysis (lint, compiler, CI), then rules and skills. The style guide (a human in review) is only the place where you find what is still missing ([talk 15:04](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L47), [talk 16:43](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L51), [talk 17:29](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L53), [talk 18:18](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L55), [talk 35:58-36:48](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L97-L99)).
pstack `correct` uses the same order (pstack [`correct/SKILL.md:19-22`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L19-L22)). The podcast says the same: watch the agent, and turn each mistake into something in the environment ([podcast, after 27:09](https://www.youtube.com/watch?v=MN9dGgmLyso), [audit 8](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L52), [audit M4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L87)).
One extra word per finding is cheap. It turns review output into data that [PBI-17](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/17-retro-repeat-to-check.md) (PR #80) can count.

## What changes

[`review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md) `## Field rules`, new row:

| Field | Rule |
|---|---|
| Guard | Exactly one of `architecture`, `type`, `lint`, `test`, `rule`, `skill`, `none (one-off)`. Where this lesson should live so it cannot come back. A proposal for the retro, never part of Fix. |

This keeps the existing rule that a review does not invent a new lint as the Fix ([`plugins/squad/skills/squad/references/review-contract.md:41-49`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L41-L49)). Guard is a note for later, Fix stays the code change in this diff.
`squad-retro` copies every Guard that is not `none (one-off)` into `## Improvements`. Same Guard plus same cause twice: RETRO03 ([PBI-17](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/17-retro-repeat-to-check.md) (PR #80)).

## Example

Before:

```markdown
- F03 [medium] src/Orders/OrderService.cs:88 Catches HttpRequestException and returns null; caller treats null as "not found". Fix: let it throw; map to 502 in the API filter.
```

After:

```markdown
- F03 [medium] src/Orders/OrderService.cs:88 Catches HttpRequestException and returns null; caller treats null as "not found". Fix: let it throw; map to 502 in the API filter. Guard: lint (ban catch-and-return-null in Application, analyzer rule)
```

Bad (REV12): the "before" line in a review file. Bad (REV13, warn): `Guard: rule` with no `why not lint:` reason.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-review.sh tools/AgentPacks.Cli.Tests/Fixtures/review/bad-no-guard.md` prints `bad-no-guard.md:3 REV12 finding has no Guard: (fix: add Guard: <layer>)` and exits 1.
- [ ] Same script on `good-guard.md` prints nothing and exits 0.
- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-review.sh tools/AgentPacks.Cli.Tests/Fixtures/review/warn-rule-no-reason.md` prints one `REV13 warning:` line and exits 0.
- [ ] `rg -n '^\| Guard \|' plugins/squad/skills/squad/references/review-contract.md` prints 1 line.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0.
- [ ] No em or en dashes: `rg -n '[\u2013\u2014]' plugins/squad` prints nothing.

## Technical design

- Module boundary: review-contract owns the field; check-review.sh (AP47) checks it; squad-retro reads it. No agent re-derives the ladder.
- Rules: REV12 (error), REV13 (warn). See [RULES.md](RULES.md#rule-table).
- Order of layers is the same list as retro.md `## Enforcer ladder`, so there is one ladder, written once.

## Sources

- Talk: [talk 15:04](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L47) (code base is the best memory), [talk 16:43](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L51) (lint or refactor so the mistake is impossible), [talk 17:29](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L53) (rules and skills can be skipped), [talk 18:18](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L55) (style guide only humans enforce), [talk 35:58-36:48](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L97-L99) (closing slide, the order). Notes: [notes 5.1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L82), [notes 5.2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L83), [notes 5.3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L84), [notes 5.5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L86), [notes 5.6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L87).
- Podcast: [podcast, after 27:09](https://www.youtube.com/watch?v=MN9dGgmLyso) ("how do I turn this into a lint rule"), [podcast about 50:33](https://www.youtube.com/watch?v=MN9dGgmLyso) (fix the environment, not the agent). Audit [audit 8](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L52), [audit M4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L87), [audit M5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L88).
- pstack pstack [`correct/SKILL.md:19-22`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L19-L22), pstack [`principle-encode-lessons-in-structure/SKILL.md:9-11`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-encode-lessons-in-structure/SKILL.md#L9-L11).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
- edit: [`plugins/squad/agents/squad-retro.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-retro.md)
- new (planned): [`plugins/pack-check/skills/pack-check/scripts/check-review.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.sh)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/review/bad-no-guard.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/review/bad-no-guard.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/review/good-guard.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/review/good-guard.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/review/warn-rule-no-reason.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/review/warn-rule-no-reason.md)
