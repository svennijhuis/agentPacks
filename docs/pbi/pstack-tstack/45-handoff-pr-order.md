# PBI-45 (AP45): Hand-off in PR-body order
- Status: todo
- Area: squad
- Phase: F playbooks
- Lane: Builder (Cursor CLI, easy: docs)
- Depends on: [PBI-38](38-playbook-files.md)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter SquadContractTests` exits 0, and the fixture `handoff.md` without `## Blast radius` fails SQD05
- door: two-way
- triage: build (Builder pastes Squad's hand-off into PR bodies by hand today)
- feedback: `SquadContractTests` SQD05 checks the 5 headings in `references/handoff.md`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/opening-a-pr.md#L15-L22 and D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/file-pr/SKILL.md#L13-L30

## Problem now

`/squad` step 10 hands off "plan path, files touched, criterion evidence, verdict, rounds, notes, pack status, workspace, and cleanup status" ([`commands/squad.md:34`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md#L34)).
That is a lab notebook. Builder rewrites it into a PR body.

## Why (plain words)

The reviewer reads the PR body in one minute.
It should say the problem first, what changed, what was left out, what could break, and how we proved it.
Squad still opens no PR. It only writes the hand-off in that order, so Builder can paste it.

## The hand-off (new file references/handoff.md)

```markdown
## Why
Expired tokens during save logged users out. Now the save retries once with a fresh token.

## What changed
- `TokenRefresher` retries once on 401.

## Scope
- Covers save. Leaves out background sync (follow-up).

## Blast radius
Small. Only the save path calls `TokenRefresher` (inferred: rg at 3c1d9e0). Safe because: the retry is bounded to 1 (reproduced).

## Verification
- api repro: base 500, tree 401 then 200 (measured).
- `dotnet test` 412 passed (measured).
```

Run details (plan path, rounds, cleanup) go below a `---` line as `Squad run:`, not in the body sections.

## Continuing someone else's work

When `/squad` continues another PR or branch, the hand-off adds `Credit: <original PR or author>` under `## Why`, keeps existing `Co-authored-by:` trailers in the suggested commit message, and treats the earlier hand-off as context, not proof (verifier re-runs the repro, AP40).

## Before / after

| | Before | After |
|---|---|---|
| First line | plan path | the problem |
| Blast radius | missing | one line + proof level (said, pointed, walked, ran, reproduced) |
| Labels | none | AP43 labels |

## Definition of Done

- [ ] `references/handoff.md` with the 5 headings and the example; [`commands/squad.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md) step 10 points to it.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter SquadContractTests` exits 0; SQD05 fixture fails.
- [ ] Still states "nothing was committed, merged, or pushed".
- [ ] No em or en dashes.

## Technical design

- Module boundary: one reference file; step 10 links it.
- Rules: SQD05.

## Sources

- pstack [`playbooks/opening-a-pr.md:15-22`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/opening-a-pr.md#L15-L22) (Why, What changed, Scope, Tradeoffs, Blast Radius, Verification), [`skills/blast-radius/SKILL.md:23-27`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/blast-radius/SKILL.md#L23-L27) (proof ladder), PO S15 (`url-learnings.md:421`).
- tstack [`file-pr/SKILL.md:13-30`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/file-pr/SKILL.md#L13-L30) (lead with the problem, not an inventory; title says why it matters), [`:41-42`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/file-pr/SKILL.md#L41-L42) (check for secrets); [`takeover/SKILL.md:14,21-25`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/takeover/SKILL.md#L14) (hand-off is context, not proof; keep authorship and co-author credit).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- new: [`plugins/squad/skills/squad/references/handoff.md`](../../../plugins/squad/skills/squad/references/handoff.md)
- edit: [`plugins/squad/commands/squad.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md)
- edit: [`tools/AgentPacks.Cli.Tests/SquadContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/SquadContractTests.cs)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/blast-radius/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/blast-radius/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/opening-a-pr.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/opening-a-pr.md)
- t3dotgg/tstack: [`skills/file-pr/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/file-pr/SKILL.md)
- t3dotgg/tstack: [`skills/takeover/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/takeover/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/commands/squad.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md)
