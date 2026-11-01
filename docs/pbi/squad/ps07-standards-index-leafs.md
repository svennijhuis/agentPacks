# PS07: Standards index and standard-* leafs (same shape as principle leafs)
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS06
- Absorbs old: old 43 (verdict and severity words), 60 (canonical examples), 62 (patterns catalog), 63 (rules file)
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "met principle etc hoe poteto gedaan heeft hoe wij dan zelfde soort onze standarts moeten doen".
- Standards live in many places today; one shape makes them loadable per agent.

## Current state on main (`6c59f49`)

- [`shared/standards/engineering.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/shared/standards/engineering.md) (75), `design.md` (12), `testing-principles.md` (20), `http-api.md`, imported by gh-aw (#79) and by language packs via `standards.source.json`.
- [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/skills/squad/references/review-contract.md) has severity words and Finding identity.
- Room rules (no em dash, no tokens, squash, personal only) live only in chat and memory.

## Target structure

Index in `squad-md`:

```markdown
## Standards

Read the leaf skill in full for any standard you apply. Each entry names when it applies. Each leaf is the **standard-<title>** skill, title in kebab case.

- **No Em Dash.** Any prose. Leaf: standard-no-em-dash.
- **No Secrets in Markdown.** Any file, PR body or comment.
- **Squash Merge One by One.** Any merge step.
- **Personal Account Only.** Any GitHub action.
- **Coding Standards File.** Any review or new repo.
- **Review Contract.** Any finding: severity words, verdict words PASS, FAIL, BLOCKED, finding identity.
- **HTTP API Shape.** Any HTTP collection (named envelope).
- **Layers.** Any new module (from `design.md`).
- **Test Behavior.** Any test (from `testing-principles.md`, points to the principle leaf).
```

| poteto `principle-<x>/SKILL.md` | our `standard-<x>/SKILL.md` |
|---|---|
| frontmatter name, "Apply when ..." | same, plus `user-invocable: false` |
| H1 title | H1 title |
| the rule in a few lines | the rule |
| why | why (Sven's words and date) |
| when it applies, when not | same |
| examples | good and bad example (the canonical example, old 60) |
| (none) | how to check: command, test or validator rule id |

Leafs: `plugins/squad/skills/standard-<x>/SKILL.md`. `shared/standards/*.md` stays the gh-aw import source; each leaf points to it, so there is one text (no copy).

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| shape | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |
| index rule | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |

## Example

Good (what the result looks like):

```markdown
---
name: standard-no-em-dash
description: Apply when writing any prose, PR body or comment.
user-invocable: false
---
# No Em Dash
Never use the long dash. Use a period, comma or parentheses.
Why: Sven, 2026-09-06.
Good: "Fixed the bug. Tests pass."  Bad: the same with a long dash between.
Check: the em dash check in `validate` and check-plan.
```

Bad (what we do not want):

A standard that only lives in room chat or memory, or a leaf that copies the full text of `shared/standards/engineering.md` instead of pointing to it.

## Subagents and what they load

- Each agent names only its leafs (PS15).

## Steps for Builder

1. write-skill learns the standard kind.
2. Write the 9 leafs.
3. Index in squad-md.
4. REF03 checks index and leafs.
5. `shared/standards` keep working for gh-aw and language packs.

## DoD

- Every index entry resolves (REF03).
- Every leaf has the 7 parts.
- `StandardsGhAwImportContractTests` and `StandardsGenerationTests` green.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Leaf list, test output.

## Risks and out of scope

- Do not duplicate `shared/standards` text.
- Out: per-language standards (PS17).
