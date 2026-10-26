# PBI-58 (AP58): Lint first: workaround comments and the guard ratchet
- Status: todo
- Area: pack-check
- Phase: H architecture
- Lane: Builder
- Depends on: [AP57](57-paved-path-size-guard.md) (check-diff.sh home)
- proof: `bash plugins/pack-check/skills/pack-check/scripts/check-diff.sh --base origin/main` in `Fixtures/diff-workaround/` exits 1 with one DIFF03 line; in `Fixtures/diff-ratchet/` exits 1 with one DIFF04 line
- door: two-way
- triage: build (Sven must-cover theme 6)
- feedback: `DiffLintTests`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555 21:00-26:40

## Problem now

The implementer prefers no new comments ([`plugins/squad/agents/squad-implementer.md:24`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-implementer.md#L24)) and the simplifier flags comments by reading ([`plugins/squad/agents/squad-simplifier.md:24`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-simplifier.md#L24)). That is guidance, not a check.
And a new guard has a problem: old code already breaks it. Today the choice is "fix everything now" or "no guard".

## Why (plain words)

poteto: one small workaround, or a comment that explains a workaround, gets copied "in a matter of a few days or a few weeks" until it is the de facto pattern ([talk 21:00](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L61)). Agents used comments to justify a band-aid instead of fixing the problem; Dune bans comments ([talk 22:36](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L65), [talk 23:23](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L67), [talk 24:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L69)).
"Whenever you see tech debt or bad patterns, your instinct should be, I need to write a lint rule against it ... at least stop the bleeding" ([talk 25:50](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L73)).
A ratchet is how you stop the bleeding without a big cleanup: the list of known violations may only shrink.

## What changes

In `check-diff.sh` ([AP57](57-paved-path-size-guard.md)):

- DIFF03 error: an added comment line contains `workaround`, `hack`, `temporary`, `for now`, `TODO`, `FIXME` or `XXX`, without `keep: <reason> <link>` on the same line. Public API doc comments are out of scope (same exception as the implementer rule).
- DIFF04 error: a guard baseline file `.agentpacks/baseline/<RULE>.txt` has more lines than on the base branch. Baselines may only shrink.

## Example

Bad (DIFF03):

```csharp
// workaround: retry twice because the API sometimes 500s
```

Good: a typed retry policy with a test, no comment. Or, if it truly must stay: `// keep: vendor bug https://example.com/issue/12`.

Ratchet: `.agentpacks/baseline/DIFF03.txt` lists 14 old lines on main. A PR that fixes 3 leaves 11: pass. A PR that adds one: DIFF04.

## Definition of Done

- [ ] In `tools/AgentPacks.Cli.Tests/Fixtures/diff-workaround/`: `bash plugins/pack-check/skills/pack-check/scripts/check-diff.sh --base origin/main` prints `src/Pay.cs:42 DIFF03 workaround comment added (fix: move the constraint into a test, type or lint, or add keep: <reason> <link>)` and exits 1.
- [ ] In `Fixtures/diff-ratchet/`: prints `.agentpacks/baseline/DIFF03.txt:1 DIFF04 baseline grew from 14 to 15 (fix: fix the new case instead)` and exits 1.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter DiffLintTests` exits 0.

## Technical design

- Module boundary: one script, git diff in, `file:line ID` out. The word list is one array at the top of the script.
- Rules: DIFF03, DIFF04.

## Sources

- Talk: [talk 21:00](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L61), [talk 21:48](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L63), [talk 22:36](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L65), [talk 23:23](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L67), [talk 24:16](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L69), [talk 25:50](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L73). Notes: [notes 4.6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L63), [notes 4.7](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L64), [notes 4.8](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L65), [notes 4.12](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L69), [notes 9.4](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L121), [notes 9.6](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L123).
- Podcast: [podcast, after 27:09](https://www.youtube.com/watch?v=MN9dGgmLyso). Audit [audit 8](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/fleet-podcast-audit.md#L52).
- pstack pstack [`agents/comment-sicko.md:12-22`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/agents/comment-sicko.md#L12-L22), pstack [`no-comments/SKILL.md:20-23`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/no-comments/SKILL.md#L20-L23), pstack [`correct/SKILL.md:20`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L20).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- edit (created by an earlier PBI in this set, planned): `plugins/pack-check/skills/pack-check/scripts/check-diff.sh`
- edit: [`plugins/squad/agents/squad-implementer.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-implementer.md)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/diff-workaround/`](../../../tools/AgentPacks.Cli.Tests/Fixtures/diff-workaround/)
- new (planned): [`tools/AgentPacks.Cli.Tests/Fixtures/diff-ratchet/`](../../../tools/AgentPacks.Cli.Tests/Fixtures/diff-ratchet/)
