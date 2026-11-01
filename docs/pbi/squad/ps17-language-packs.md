# PS17: Language packs restyle, import-boundary tests, typescript-best-practices
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B5
- Depends on: PS16
- Absorbs old: old 55 (import boundaries)
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows), [FORKS](FORKS.md) (forks), [gh-aw](gh-aw/README.md)

## 1. Goal

**Problem on main (`6c59f49`).** [`plugins/rust/skills/rust-review/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/rust/skills/rust-review/SKILL.md):

> Read every file in `references/standards/`.

Language skills repeat boilerplate and read all standards always; no import-boundary tests; TypeScript lacks best practices.

**Why we change it.**

- Sven: typescript-best-practices for the typescript plugin.
- Old 55: boundary rules as tests, still open on main (no boundary test found).

**What it gives the user.**

- Local (Cursor, Claude Code, CLI): Shorter language skills, same standards via `standards.source.json`.
- gh-aw: Standards stay files in the pack, gh-aw import paths unchanged.

**Out of scope.** New languages.

Current state details:

- dotnet, rust, typescript skills repeat "Internal. Do not run directly" boilerplate; no principle refs; no boundary tests.

## 2. Depends on

PS16. Start only when those PBIs are merged on main. Build order: [README](README.md#new-set-and-build-order).

## 3. What gets replaced

| Kind | Now | After | Action |
|---|---|---|---|
| skill | `plugins/{dotnet,rust,typescript}/skills/*` | same | restyle, drop boilerplate |
| skill | (none) | `plugins/typescript/skills/typescript-best-practices/` | add (copy + adapt) |

Full list for all PBIs: [MIGRATION.md](MIGRATION.md#path-table).

## 4. Technical design

### 4.1 Tree before and after

Before (main):

```text
plugins/{dotnet,rust,typescript}/skills/*  standards.source.json
```

After:

```text
same paths, SHAPES skill shape
plugins/typescript/skills/typescript-best-practices/ (copy + adapt)
import-boundary tests per pack
```

### 4.2 Skeleton of the new file

Shape rules: [SHAPES.md](SHAPES.md#summary).

```text
(SHAPES skill)
```

### 4.3 Target detail

Drop boilerplate, add index lines; pin typescript-best-practices in `plugins/typescript/external-skills.json`; one boundary-test template per language; review skills cite Coding Standards File.

### 4.4 Interfaces and contracts

| Caller | Callee | Input | Output | State file |
|---|---|---|---|---|
| squad review | `<lang>-review` | files | findings | none |
| generator | standards.source.json | documents + consumers | references/standards/* | generated |

### 4.5 Flow

```mermaid
flowchart LR
  SS["standards.source.json"] --> G["generator"] --> R["references/standards"] --> L["lang-review"]
```

### 4.6 Choices

| Chosen | Rejected | Why |
|---|---|---|
| Keep standards.source.json | Move to leafs | It is the import contract for packs and forks. |

### 4.7 Checks and error messages

| Rule | Checks | Error message |
|---|---|---|
| LanguagePackContractTests | consumers resolve | `<pack>: consumer 'rust-review' names unknown document` |

### 4.8 Risks and what breaks

| Risk | Fix |
|---|---|
| Fork documents removed | never delete keys not in our v1 list |

Also: Out: new standards text.

### 4.9 Per environment

| Environment | What is different |
|---|---|
| Local (Cursor, Claude Code, CLI) | Same. |
| gh-aw | Same paths. |

### 4.10 Subagents and what they load

- none

## 5. Steps

0. **Inventory (forks and upstream alike).** `git diff --name-status <basis>...HEAD -- plugins shared docs .github` (basis: the agentPacks commit you started from; on upstream itself the list is empty). Add every own standard, rule, skill, agent, `custom-rules.md` and instructions file to `docs/pbi/squad/fork-inventory.md` ([format](FORKS.md#step-0-inventory-every-pbi-before-any-edit)). Never delete or overwrite a listed file; unsure means keep it and mark the row `open`.
1. Branch from main: `git switch -c pbi/ps17-language-packs origin/main`.
2. Save the base for the fork check: `git rev-parse HEAD > /tmp/base.sha`.
3. Read [SHAPES.md](SHAPES.md) for every kind in section 3, and each source in section 6.
4. Do each row of section 3 in order. Move or rename with `git mv`. If a row's target path already holds a fork file, follow the [conflict table](MIGRATION.md#fork-path). A copy keeps the `Source:` line and `## What we changed or added`.
5. Do these steps, nothing else:
   1. One PR per language.
6. Run every command in section 8. All must pass.
7. Open one PR titled `PS17: Language packs restyle, import-boundary tests, typescript-best-practices`. Body: link to this file, the section 3 table, the section 8 output.

## 6. Sources

| Step | Skill (repo, path, SHA) | Licence | Mode | Lives in | Reason |
|---|---|---|---|---|---|
| typescript | [typescript-best-practices](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/typescript-best-practices/SKILL.md) | MIT (cursor/plugins, Lauren Tan) | copy + adapt | `plugins/typescript/skills/typescript-best-practices/` | Sven: add to the typescript plugin. Sven 2026-11-01: copy, not pin. We add a Standards line, the gates, the Autonomy line for gh-aw and Provider notes. |
| test | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | MIT (cursor/plugins, Lauren Tan) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |

Route per external skill ([decision](README.md#decision-pin-copy--adapt-weave-or-not-used)):

- typescript-best-practices: **copy + adapt**. `plugins/typescript/skills/typescript-best-practices/`.
- tdd: **copy + adapt**. `plugins/squad/skills/tdd/`.

Every copy + adapt here adds, beyond its own changes: `Source:` line with SHA, a Standards line, the Gates, the Autonomy line for gh-aw, `## Provider notes`, `## What we changed or added`, and a NOTICE line.

Upstream: pstack [cursor/plugins at ae5650f](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack) (MIT), Matt [mattpocock/skills at 4ae2317](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d) (MIT). Map: [skill map](sources/pstack-matt-skill-map.md).

## 7. Example

Good:

```text
csharp: test fails when Application references Azure.Storage.*
(import-boundary test, rule from standard Layers).
```

Bad:

A language pack restating the shared HTTP rule in its own words instead of the shared leaf.

## 8. DoD (checkable)

```bash
dotnet run --project tools/AgentPacks.Cli -- validate                       # 0 errors, 0 warnings
dotnet test tools/AgentPacks.Cli.Tests                                      # green
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
dotnet run --project tools/AgentPacks.Cli -- migrate-check --base "$(cat /tmp/base.sha)"   # from PS03 on; 0 LOST
for f in tests/fixtures/gh-aw/*.md; do gh aw compile --strict "$f"; done   # from GA04 on; before: write "n/a, no fixture yet"
dotnet run --project tools/AgentPacks.Cli -- migrate-check --base <fixture base> --root tests/fixtures/fork-acme   # fixture fork, 0 LOST
dotnet run --project tools/AgentPacks.Cli -- validate --root tests/fixtures/fork-acme                              # fixture fork, 0 errors
grep -rnP "[\x{2013}\x{2014}]" plugins docs || echo "no dashes"
```

Then locally: install `/tmp/agentpacks-marketplace` in Cursor or Claude Code, run `/pack-check` (same result as `validate`) and each command this PBI touches once.

- [ ] validate 0/0 per pack.
- [ ] Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- [ ] gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- [ ] `/pack-check` locally gives the same result (same rules, one source).
- [ ] `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- [ ] No em dash, no tokens in markdown, FAKE fixture values only.
- [ ] Every row of `docs/pbi/squad/fork-inventory.md` is present in v2 (status `done` or `open` with a reason) and `/pack-check` is green, locally and in gh-aw. The fixture fork `tests/fixtures/fork-acme` (one own standard, one own rule) still passes after this PBI.

## 9. Evidence

- Per-pack output.

## 10. For forks

Rules for every fork: [FORKS.md](FORKS.md). For this PBI: Extra `documents` and `consumers` keys in your `standards.source.json` are kept; the PBI only edits keys it lists.
