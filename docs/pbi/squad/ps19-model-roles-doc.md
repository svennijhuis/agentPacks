# PS19: pstack model roles mapped to our tiers
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS04
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows), [FORKS](FORKS.md) (forks), [gh-aw](gh-aw/README.md)

## 1. Goal

**Problem on main (`6c59f49`).** [`models.source.json`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/models.source.json):

> "default": "inherit"

pstack names model roles; ours are tiers. Without a map, copied text mentions client models.

**Why we change it.**

- Sven: check setup-pstack for our plugins.

**What it gives the user.**

- Local (Cursor, Claude Code, CLI): One table maps pstack roles to our tiers.
- gh-aw: gh-aw uses `inherit`; table says so.

**Out of scope.** Changing tiers.

Current state details:

- Tiers in [`models.source.json`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/models.source.json); no role map.

## 2. Depends on

PS04. Start only when those PBIs are merged on main. Build order: [README](README.md#new-set-and-build-order).

## 3. What gets replaced

Only the files named in **Target structure** and **Steps for Builder**. No moves or renames.

## 4. Technical design

### 4.1 Tree before and after

Before (main):

```text
models.source.json
```

After:

```text
docs/model-roles.md   # NEW: pstack role to tier, per client
```

### 4.2 Skeleton of the new file

Shape rules: [SHAPES.md](SHAPES.md#summary).

```text
| pstack role | our tier | Cursor | Claude | Copilot | Codex | gh-aw |
|---|---|---|---|---|---|---|
```

### 4.3 Target detail

`docs/PSTACK-ROLES.md`: each role named by pinned or copied skills (explorer, explainer, candidate, interrogator, reviewer) to our tier per provider. No live calls, no spend, no `~/.cursor` setup on the shared box.

### 4.4 Interfaces and contracts

| Caller | Callee | Input | Output | State file |
|---|---|---|---|---|
| copy rule | model-roles.md | upstream role | tier | none |

### 4.5 Flow

```mermaid
flowchart LR
  R["pstack role"] --> T["tier"] --> C["client model via models.source.json"]
```

### 4.6 Choices

| Chosen | Rejected | Why |
|---|---|---|
| Doc table | New schema | Tiers already exist. |

### 4.7 Checks and error messages

| Rule | Checks | Error message |
|---|---|---|
| REF01 | link resolves | see PS03 |

### 4.8 Risks and what breaks

| Risk | Fix |
|---|---|
| Stale ids | table names tiers, ids come from models.source.json |

Also: Out: home-dir config.

### 4.9 Per environment

| Environment | What is different |
|---|---|
| Local (Cursor, Claude Code, CLI) | Tier mapping. |
| gh-aw | inherit. |

### 4.10 Subagents and what they load

- none

## 5. Steps

0. **Inventory (forks and upstream alike).** `git diff --name-status <basis>...HEAD -- plugins shared docs .github` (basis: the agentPacks commit you started from; on upstream itself the list is empty). Add every own standard, rule, skill, agent and instructions file to `docs/pbi/squad/fork-inventory.md` ([format](FORKS.md#step-0-inventory-every-pbi-before-any-edit)). Never delete or overwrite a listed file; unsure means keep it and mark the row `open`.
1. Branch from main: `git switch -c pbi/ps19-model-roles-doc origin/main`.
2. Save the base for the fork check: `git rev-parse HEAD > /tmp/base.sha`.
3. Read [SHAPES.md](SHAPES.md) for every kind in section 3, and each source in section 6.
4. Do each row of section 3 in order. Move or rename with `git mv`. If a row's target path already holds a fork file, follow the [conflict table](MIGRATION.md#fork-path). A copy keeps the `Source:` line and `## What we changed or added`.
5. Do these steps, nothing else:
   1. Copy + adapt setup-pstack into `plugins/pack-check/skills/pack-check-mode/playbooks/setup.md`: it writes the role to tier table into the repo (from `models.source.json`), never `~/.cursor/rules`.
   2. Write the doc.
6. Run every command in section 8. All must pass.
7. Open one PR titled `PS19: pstack model roles mapped to our tiers`. Body: link to this file, the section 3 table, the section 8 output.

## 6. Sources

| Step | Skill (repo, path, SHA) | Licence | Mode | Lives in | Reason |
|---|---|---|---|---|---|
| roles | [setup-pstack](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/setup-pstack/SKILL.md) | MIT (cursor/plugins, Lauren Tan) | copy + adapt | `plugins/pack-check/skills/pack-check-mode/playbooks/setup.md` | Our plugin setup (Sven). Upstream writes `~/.cursor/rules` (Cursor only, home dir); our copy writes the role to tier table into the repo from `models.source.json`. |

Route per external skill ([decision](README.md#decision-pin-copy--adapt-weave-or-not-used)):

- setup-pstack: **copy + adapt**. `plugins/pack-check/skills/pack-check-mode/playbooks/setup.md`.

Every copy + adapt here adds, beyond its own changes: `Source:` line with SHA, a Standards line, the Gates, the Autonomy line for gh-aw, `## Provider notes`, `## What we changed or added`, and a NOTICE line.

Upstream: pstack [cursor/plugins at ae5650f](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack) (MIT), Matt [mattpocock/skills at 4ae2317](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d) (MIT). Map: [skill map](sources/pstack-matt-skill-map.md).

## 7. Example

Good:

```markdown
| pstack role | Used by | Claude | Copilot | Cursor | Codex |
|---|---|---|---|---|---|
| candidate | arena (our copy) | tier-high | tier-high | tier-high | tier-high |
```

Bad:

Running setup-pstack on the shared box, which writes `~/.cursor/rules`.

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

- [ ] Every role named by a used skill is in the table.
- [ ] `/pack-check setup` writes only repo files.
- [ ] Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- [ ] gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- [ ] `/pack-check` locally gives the same result (same rules, one source).
- [ ] `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- [ ] No em dash, no tokens in markdown, FAKE fixture values only.
- [ ] Every row of `docs/pbi/squad/fork-inventory.md` is present in v2 (status `done` or `open` with a reason) and `/pack-check` is green, locally and in gh-aw. The fixture fork `tests/fixtures/fork-acme` (one own standard, one own rule) still passes after this PBI.

## 9. Evidence

- Doc, setup output.

## 10. For forks

Rules for every fork: [FORKS.md](FORKS.md). For this PBI: Fork tiers: add rows; keys we own are listed.
