# PBI-44 (AP44): Missing engineering principles
- Status: todo
- Area: standards
- Phase: F playbooks
- Lane: Builder (Cursor CLI, easy: docs)
- Depends on: none
- proof: `dotnet run --project tools/AgentPacks.Cli -- validate && dotnet test tools/AgentPacks.Cli.Tests --filter "StandardsGenerationTests|SquadContractTests"` exits 0, and the fixture agent citing `## Build the levr` fails SQD04
- door: two-way
- triage: build (gap list from the per-file checklist: 7 principle files map to missing sections)
- feedback: `SquadContractTests` SQD04 checks every section name an agent cites exists in engineering.md
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/principle-build-the-lever/SKILL.md

## Problem now

[`shared/standards/engineering.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md) has 13 sections ([`:5-74`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md#L5-L74)). Seven pstack principles have no home:
build the lever, foundational thinking, migrate callers then delete, never block on the human, outcome-oriented execution, guard the context window (only [`squad-verifier.md:28`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L28)), type system discipline (partly [`:30`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L30)).
Reviewers cite sections by name ([`engineering.md:3`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md#L3)), so a missing section is a rule nobody can cite.

## Why (plain words)

Short named rules let a reviewer say "Build the lever" instead of a paragraph.
We write them in our own words, 2 to 3 bullets each, like the existing sections.
pstack principle skills cannot be pinned: they link back to poteto-mode (PO review, `url-learnings.md:352`).

## New sections (text sketch)

```markdown
## Build the lever
- For repeated or checkable work, write the script, codemod or check, then run it. The script is what the reviewer reruns.

## Foundational thinking
- Choose the data shape before the logic. Scaffold before features. Name what concurrent actors share.

## Migrate callers, then delete
- A new internal API ships with every caller moved and the old one deleted in the same change. No compatibility layer "for now".

## Outcome first
- In a planned migration, go to the target shape. Do not keep throwaway in-between states alive.

## Find facts, ask decisions
- Do not ask a person what a command can tell you. Ask only product or preference calls.

## Guard the context window
- Send bulk reads to a subagent or a file. Keep summaries, paths and line numbers in the main thread.

## Make illegal states unrepresentable
- Use types or enums so a wrong state cannot be built. Parse outside data at the edge.
```

Plus one line each:
- [`agents/squad-reviewer.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-reviewer.md) and `squad-architect.md`: "No is a valid answer: say when an option does not earn its place." (pstack `poteto-mode/SKILL.md:89)
- `rules/review-checklist.mdc`: "Comments explain a non-obvious why only; narration comments are a `low` finding." (pstack `no-comments`, [`agents/comment-sicko.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/agents/comment-sicko.md), `poteto-mode/SKILL.md:117-119)
- [`engineering.md:3`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md#L3): "Steer with section names." (pstack [`docs/guide/08-principles.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/docs/guide/08-principles.md))

## Before / after

| | Before | After |
|---|---|---|
| Sections | 13 | 20 |
| Reviewer can cite "Migrate callers, then delete" | no | yes, SQD04 keeps it valid |

## Definition of Done

- [ ] 7 sections added, each 1 to 3 bullets, paraphrased, `Source:` comment line at file end.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter "StandardsGenerationTests|SquadContractTests"` exits 0; SQD04 fixture fails.
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0.
- [ ] No em or en dashes.

## Technical design

- Module boundary: one standards file, cited by name from agents and rules.
- Rules: SQD04.

## Sources

- pstack `principle-build-the-lever`, `principle-foundational-thinking`, `principle-migrate-callers-then-delete-legacy-apis`, `principle-outcome-oriented-execution`, `principle-never-block-on-the-human`, `principle-guard-the-context-window`, `principle-type-system-discipline` (each `SKILL.md`), [`poteto-mode/SKILL.md:89,117-119`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L89), [`docs/guide/08-principles.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/docs/guide/08-principles.md).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`shared/standards/engineering.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md)
- edit: [`plugins/squad/agents/squad-reviewer.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-reviewer.md)
- edit: [`plugins/squad/agents/squad-architect.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-architect.md)
- edit: [`tools/AgentPacks.Cli.Tests/SquadContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/SquadContractTests.cs)
- edit: [`tools/AgentPacks.Cli.Tests/StandardsGenerationTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/StandardsGenerationTests.cs)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/agents/comment-sicko.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/agents/comment-sicko.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/docs/guide/08-principles.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/docs/guide/08-principles.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/agents/squad-reviewer.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-reviewer.md)
- agentPacks main (existing files): [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- agentPacks main (existing files): [`shared/standards/engineering.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md)
