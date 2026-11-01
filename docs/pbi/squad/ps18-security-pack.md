# PS18: Security pack restyle
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B6
- Depends on: PS17
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows), [FORKS](FORKS.md) (forks), [gh-aw](gh-aw/README.md)

## 1. Goal

**Problem on main (`6c59f49`).** [`plugins/security/skills/security-audit-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/security/skills/security-audit-md/SKILL.md):

> Launch `security-recon` once. Hunters receive its architecture summary verbatim.

Security pack predates the v2 shapes.

**Why we change it.**

- Same style everywhere.

**What it gives the user.**

- Local (Cursor, Claude Code, CLI): Same audit, v2 shapes.
- gh-aw: Report-only fits gh-aw; tools within allowlist.

**Out of scope.** New attack classes.

Current state details:

- [`plugins/security/skills/security-audit-md/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/security/skills/security-audit-md/SKILL.md) plus security-recon, -hunter, -validator.

## 2. Depends on

PS17. Start only when those PBIs are merged on main. Build order: [README](README.md#new-set-and-build-order).

## 3. What gets replaced

| Kind | Now | After | Action |
|---|---|---|---|
| command | `plugins/security/commands/security-audit.md` | same | edit |
| skill | `plugins/security/skills/security-audit-mode/SKILL.md` | same | restyle |
| agent | `plugins/security/agents/security-{recon,hunter,validator}.md` | same | restyle |

Full list for all PBIs: [MIGRATION.md](MIGRATION.md#path-table).

## 4. Technical design

### 4.1 Tree before and after

Before (main):

```text
plugins/security/skills/security-audit-md/  agents/security-*.md
```

After:

```text
same paths, hub and subagent shapes
```

### 4.2 Skeleton of the new file

Shape rules: [SHAPES.md](SHAPES.md#summary).

```text
(SHAPES hub and subagent)
```

### 4.3 Target detail

Steps to a playbook, Non-negotiables, interrogate and blast-radius refs; agents restyled with write-subagent.

### 4.4 Interfaces and contracts

| Caller | Callee | Input | Output | State file |
|---|---|---|---|---|
| security-audit-mode | recon, hunter, validator | scope | report | report file |

### 4.5 Flow

```mermaid
flowchart LR
  A["security-audit-mode"] --> R["recon"] --> H["hunters in parallel"] --> V["validator"] --> REP["report"]
```

### 4.6 Choices

| Chosen | Rejected | Why |
|---|---|---|
| Restyle only | Merge into squad | Separate command and owners. |

### 4.7 Checks and error messages

| Rule | Checks | Error message |
|---|---|---|
| SecurityPackContractTests | unchanged behavior | existing |

### 4.8 Risks and what breaks

| Risk | Fix |
|---|---|
| Hunter count in gh-aw | cap by max-turns |

Also: Out: new attack classes.

### 4.9 Per environment

| Environment | What is different |
|---|---|
| Local (Cursor, Claude Code, CLI) | Same. |
| gh-aw | Report only. |

### 4.10 Subagents and what they load

- validator: interrogate; recon: how.

## 5. Steps

0. **Inventory (forks and upstream alike).** `git diff --name-status <basis>...HEAD -- plugins shared docs .github` (basis: the agentPacks commit you started from; on upstream itself the list is empty). Add every own standard, rule, skill, agent, `custom-rules.md` and instructions file to `docs/pbi/squad/fork-inventory.md` ([format](FORKS.md#step-0-inventory-every-pbi-before-any-edit)). Never delete or overwrite a listed file; unsure means keep it and mark the row `open`.
1. Branch from main: `git switch -c pbi/ps18-security-pack origin/main`.
2. Save the base for the fork check: `git rev-parse HEAD > /tmp/base.sha`.
3. Read [SHAPES.md](SHAPES.md) for every kind in section 3, and each source in section 6.
4. Do each row of section 3 in order. Move or rename with `git mv`. If a row's target path already holds a fork file, follow the [conflict table](MIGRATION.md#fork-path). A copy keeps the `Source:` line and `## What we changed or added`.
5. Do these steps, nothing else:
   1. Restyle 1 skill, 3 agents.
6. Run every command in section 8. All must pass.
7. Open one PR titled `PS18: Security pack restyle`. Body: link to this file, the section 3 table, the section 8 output.

## 6. Sources

| Step | Skill (repo, path, SHA) | Licence | Mode | Lives in | Reason |
|---|---|---|---|---|---|
| review | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | MIT (cursor/plugins, Lauren Tan) | pin | `plugins/squad/external-skills.json` | We change nothing and it has no Cursor-only parts (scanned at ae5650f), so it stays external at the 40-hex SHA. In gh-aw the generator vendors it into the commit and `aw-closure.json` lists it. |
| review | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | MIT (cursor/plugins, Lauren Tan) | pin | `plugins/squad/external-skills.json` | We change nothing and it has no Cursor-only parts (scanned at ae5650f), so it stays external at the 40-hex SHA. In gh-aw the generator vendors it into the commit and `aw-closure.json` lists it. |

Route per external skill ([decision](README.md#decision-pin-copy--adapt-weave-or-not-used)):

- interrogate: **pin**. `plugins/squad/external-skills.json`.
- blast-radius: **pin**. `plugins/squad/external-skills.json`.

Every copy + adapt here adds, beyond its own changes: `Source:` line with SHA, a Standards line, the Gates, the Autonomy line for gh-aw, `## Provider notes`, `## What we changed or added`, and a NOTICE line.

Upstream: pstack [cursor/plugins at ae5650f](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/pstack) (MIT), Matt [mattpocock/skills at 4ae2317](https://github.com/mattpocock/skills/tree/4ae2317ddb22b046e26455e4f9c9b3aaaf62212d) (MIT). Map: [skill map](sources/pstack-matt-skill-map.md).

## 7. Example

Good:

```text
squad-security-reviewer: finding high, src/Upload.cs:22,
path from user input reaches File.Open. Proven by interrogate step.
```

Bad:

A security finding with no file:line or no proof, or a full-repo audit started from a PR review.

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

- [ ] check-subagent PASS for 3.
- [ ] Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- [ ] gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- [ ] `/pack-check` locally gives the same result (same rules, one source).
- [ ] `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- [ ] No em dash, no tokens in markdown, FAKE fixture values only.
- [ ] Every row of `docs/pbi/squad/fork-inventory.md` is present in v2 (status `done` or `open` with a reason) and `/pack-check` is green, locally and in gh-aw. The fixture fork `tests/fixtures/fork-acme` (one own standard, one own rule) still passes after this PBI.

## 9. Evidence

- Verdicts.

## 10. For forks

Rules for every fork: [FORKS.md](FORKS.md). For this PBI: Fork attack classes in `references/attack-classes.md` below `## Local classes` stay.
