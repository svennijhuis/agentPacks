# PBI-56 (AP56): Trust level per repo: autonomy follows evidence
- Status: todo
- Area: squad
- Phase: I trust ladder
- Lane: Builder (docs + script); Sven (T3 list)
- Depends on: [AP52](52-feature-map.md), [AP54](54-measured-budgets.md), [AP55](55-import-boundaries.md) (for T2), [PBI-15](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/15-door-field.md) (PR #80), [AP50](50-open-decisions.md) L5
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trust-t2.md` exits 1 with one VER14 line
- door: two-way for the rule; T3 itself is one-way (merges), only for repos Sven names
- triage: build (Sven must-cover theme 4)
- feedback: `VerifyLintTests`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 05:27-06:40 and 35:10, D42 (proposed) https://www.youtube.com/watch?v=MN9dGgmLyso

## Problem now

Squad routes by size and risk only ([`plugins/squad/skills/squad/SKILL.md:45-52`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L45-L52)). Nothing says how much a repo is trusted.
Room rules decide merges by hand per repo. A repo without a harness gets the same fan-out as a repo with full verification.

## Why (plain words)

poteto: going from 1 to 5 agents to about 100 without trust gives "a ton of slop requests", regressions and shipped bugs ([talk 06:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L25)). Trust comes from verification and the environment, step by step ([talk 05:27](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L23), [talk 35:10](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L95)).
Podcast: the trust ladder, and "start from the question ... how do I get to the point where my agent can merge its own code" ([podcast about 1:58](https://www.youtube.com/watch?v=MN9dGgmLyso), [podcast about 37:58](https://www.youtube.com/watch?v=MN9dGgmLyso), [audit M1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L84), [audit M6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L89)). Good verification turns one-way doors into two-way doors ([podcast about 56:28](https://www.youtube.com/watch?v=MN9dGgmLyso), [audit M13](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L96)).
A level written in the repo makes the next step visible: what evidence is missing for more autonomy.

## What changes

`docs/verify.md` gets `## Trust` with one line `Level: T0 | T1 | T2 | T3` and the evidence:

| Level | Needs | Allows |
|---|---|---|
| T0 | nothing | main agent + one implementer, sequential slices, Sven reviews every PR |
| T1 | `## Features` (AP52) passes check-verify | verifier must Drive; sequential slices |
| T2 | T1 + `## Budgets` (AP54) when perf matters + boundary test (AP55) + no open DIFF ratchet growth (AP58) | up to 2 parallel independent slices; sampled review ([AP59](59-gardener-weeds.md)) |
| T3 | T2 + Sven names the repo in the room charter | self-merge after full gates, only on `door: two-way` PRs ([PBI-15](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/15-door-field.md) (PR #80)) |

T3 is a decision, never a computed value. Today Sven allows it only for named repos.

## Example

```markdown
## Trust

Level: T1
Evidence: Features 4 rows (check-verify clean 2026-10-26). Budgets: none. Boundary test: none.
Next: add BoundaryTests (AP55) for T2.
```

Bad (VER14): `Level: T2` with no `## Budgets` and no boundary test named.

## Definition of Done

- [ ] `bash plugins/pack-check/skills/pack-check/scripts/check-verify.sh tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trust-t2.md` prints `bad-trust-t2.md:3 VER14 Level T2 needs Features, Budgets or a reason, and a boundary test (fix: lower the level or add the evidence)` and exits 1.
- [ ] `rg -n 'Level: T' plugins/squad/skills/squad/SKILL.md` prints the routing line that reads the level.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter VerifyLintTests` exits 0.

## Technical design

- Module boundary: verify.md states the level; check-verify.sh checks that the evidence sections exist; `/squad` reads the level for fan-out. Merge rights stay in the room charter.
- Rules: VER14.

## Sources

- Talk: [talk 05:27](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L23), [talk 06:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L25), [talk 35:10](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L95), [talk 36:48](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L99) ("your agents can just be free"). Notes: [notes 1.1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L22), [notes 1.2](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L23), [notes 1.3](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L24), [notes 1.4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L25), [notes 1.5](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L26), [notes 6.6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L97).
- Podcast: [podcast about 26:14](https://www.youtube.com/watch?v=MN9dGgmLyso), [podcast about 52:38](https://www.youtube.com/watch?v=MN9dGgmLyso) (autopilot), [podcast about 56:28](https://www.youtube.com/watch?v=MN9dGgmLyso). Audit [audit 22](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L66), [audit 23](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L67), [audit M1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L84), [audit M6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L89), [audit M13](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L96), [audit G1](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L106).
- pstack pstack [`playbooks/autopilot-full.md:3-17`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/autopilot-full.md#L3-L17) (one owner per PR, swarm verdict before merge).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit: [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
- edit: [`plugins/squad/skills/verify-harness/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/verify-harness/SKILL.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trust-t2.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/verify/bad-trust-t2.md)
