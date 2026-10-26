# PBI-49 (AP49): Old PR disposition in /squad-review
- Status: todo
- Area: squad
- Phase: G review loop
- Lane: Builder (Cursor CLI, easy)
- Depends on: [PBI-46](46-review-loop-budget.md)
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0, and the fixture review of a PR 40 commits behind main without a disposition line prints one REV11 warning
- door: two-way
- triage: build (Sven has many old open PRs; daily PR advice asks the same question by hand)
- feedback: `ReviewLoopTests` warns REV11 on `bad-old-pr-no-disposition`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/modernize/SKILL.md#L56-L70

## Problem now

`/squad-review --pr` reviews the diff against its base ([`commands/squad-review.md:15-18`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md#L15-L18)).
For a PR that is weeks old it does not ask whether the PR still belongs: main may have solved it, moved the code, or changed the API.

## Why (plain words)

A clean rebase does not prove a PR is still worth landing.
Before reviewing details, say one word: keep it (MODERNIZE), rewrite it (REWORK) or close it (CLOSE), with evidence.
Squad never rebases or closes. It only says what it found.

## Rule

When the PR base is more than 20 commits or 14 days behind `origin/main`, the review file starts with:

```markdown
Disposition: REWORK
Why: main moved pin loading into ExternalSourceMaterializer (3c1d9e0); this PR edits the old ExternalSources.cs path. A rebase keeps the wrong shape.
Checked: git log origin/main --since=<merge-base date> -- tools/AgentPacks.Cli/Loading/ ; open PRs touching the same files: #88.
```

- MODERNIZE: goal still useful, shape still right.
- REWORK: goal still useful, shape wrong; name the smallest rewrite.
- CLOSE: superseded or not worth its cost; cite evidence. Age alone is not evidence.

Then the normal review follows (round 1) unless the disposition is CLOSE.

## Exact check-review output

```text
docs/reviews/2026-10-27-pr-62.md:1 REV11 warning: old PR reviewed without a disposition line (fix: add 'Disposition: MODERNIZE|REWORK|CLOSE' with Why and Checked)
```

## Before / after

| | Before | After |
|---|---|---|
| Old PR | line-level findings only | one-word disposition + evidence first |
| Close decision | implicit | Sven decides on a cited CLOSE |

## Definition of Done

- [ ] [`commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md) step 1 adds the age check and the disposition block (max 6 lines).
- [ ] `check-review.sh`: REV11 (warn).
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter ReviewLoopTests` exits 0.
- [ ] Squad still never rebases, pushes or closes.
- [ ] No em or en dashes.

## Technical design

- Module boundary: review file head; check-review reads it.
- Rules: REV11.

## Sources

- tstack [`modernize/SKILL.md:10-11`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/modernize/SKILL.md#L10-L11) (modernize the idea, not just the branch), [`:26-54`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/modernize/SKILL.md#L26-L54) (what changed around it), [`:56-70`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/modernize/SKILL.md#L56-L70) (MODERNIZE, REWORK, CLOSE; age alone is not evidence).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.sh`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.sh)
- new: [`plugins/pack-check/skills/pack-check/scripts/check-review.ps1`](../../../plugins/pack-check/skills/pack-check/scripts/check-review.ps1)
- new: [`tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs`](../../../tools/AgentPacks.Cli.Tests/ReviewLoopTests.cs)

## Links

- t3dotgg/tstack: [`skills/modernize/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/modernize/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md)
