# PBI-63 (AP63): Unified rules file: shared/standards/rules.md (and move docs/pbi RULES.md into it)
- Status: todo
- Area: standards
- Phase: K catalogs
- Lane: Builder (docs)
- Depends on: [AP62](62-patterns-catalog.md) (same test class). Rows link PBIs that own the machine rules
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter StandardsCatalogTests` exits 0 (RUL01 to RUL03), and `rg -c "^\| R[0-9]{2} " shared/standards/rules.md` prints 20
- door: two-way
- triage: build (Sven asked 2026-10-26: a clean rules file as its own PBI)
- feedback: `StandardsCatalogTests`
- harness: none (docs + test)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D41 (proposed) https://x.com/poteto/status/2102050467505430555, D39, D40 (proposed)

## Problem now

Authoring rules (naming, file structure, coding) are spread over CONTRIBUTE, testing-principles, agents and room rules. The machine rule table lives in a PBI folder ([RULES.md](RULES.md)), which is not shipped to consumers.

## Why (plain words)

One file a human can read top to bottom: the rule, good, bad, and what checks it. The machine ids stay the same; only the home moves. Reviewer and check-plan cite one place.

## What changes

1. New file `shared/standards/rules.md` with two parts:
   - Part 1, authoring rules R01 to R20 (table below).
   - Part 2, machine rules: the rule table from [RULES.md](RULES.md#rule-table), moved as is. Ids do not change.
2. After the move, `docs/pbi/pstack-tstack/RULES.md` keeps one line: "Moved to shared/standards/rules.md". PBIs keep their links working through that line.
3. Each row is enforceable: a rule id, a contract test, or a Reviewer line. Nothing prose only.

## Part 1 (goes into shared/standards/rules.md)

| Id | Rule | Good | Bad | Enforcement | Source |
|---|---|---|---|---|---|
| R01 | Dated file names for plans, retros, weeds | `docs/plans/2026-10-26-export-csv.md` | `docs/plans/export.md` | PLAN01, RETRO01 | [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh) |
| R02 | No em or en dashes in authored markdown | `Done. Next: AP52.` | a long dash between clauses | PLAN02, VER02, RETRO02 + Reviewer | RULES.md |
| R03 | Rule ids `<PREFIX><NN>`, never reused or renumbered | `DIFF03` | `lint-7b` | RUL02 (StandardsCatalogTests) | [RULES.md ID scheme](RULES.md#id-scheme) |
| R04 | One home per rule; other files link to it | check-plan.sh owns PLAN ids | the same rule text in 3 agents | RUL03 + Reviewer | [`shared/standards/engineering.md:54-57`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md#L54-L57) |
| R05 | Check output `<file>:<line> <ID> <message> (fix: ...)` | `a.md:3 REV12 finding has no Guard: (fix: ...)` | `error!` | PackCheckContractTests | [RULES.md output](RULES.md#output-format-every-script) |
| R06 | Thin SKILL.md, detail in references/ | SKILL.md under 60 lines | one 400-line SKILL.md | pack-author-reviewer bar | [`docs/CONTRIBUTE.md:98`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/docs/CONTRIBUTE.md#L98) |
| R07 | Call skills by exact name; no catalog dumps | load `verify-harness` by name | "load all Matt skills" | Reviewer + SquadContractTests | [`docs/EXTERNAL-SKILLS.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/docs/EXTERNAL-SKILLS.md) |
| R08 | No new slash commands beyond the fixed set | `/squad-review sample 3` | `/garden` | SquadContractTests (count) | README of this folder (Sven decision 2026-10-26) |
| R09 | Pin external refs to a SHA or tag, never a branch | `@9e88dad` | `@main` | PBI-05 contract test (planned) | [PBI-5](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/05-plugin-pin-rule.md) (PR #80) |
| R10 | Tests check behavior; no tautological tests | assert the literal result | `Assert.NotNull(result)` alone | Reviewer line | [`shared/standards/testing-principles.md:15-20`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/testing-principles.md#L15-L20) |
| R11 | No unit test that a doc contains a phrase | test the parser output | `Assert.Contains("never", readme)` | Reviewer line | [`shared/standards/testing-principles.md:12`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/testing-principles.md#L12) |
| R12 | New comments only with a reason a test, type or lint cannot hold | `// keep: vendor bug <link>` | `// workaround: retry` | DIFF03 (AP58); simplifier line | [`plugins/squad/agents/squad-simplifier.md:24`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-simplifier.md#L24) |
| R13 | New code goes on the paved path | `src/Features/Export/` | `tools/helpers/x.cs` | DIFF05 (AP57) | [AP57](57-paved-path-size-guard.md) |
| R14 | Files stay under the size limit (400, AP50 L4) | split by feature | one 1,200-line file | DIFF01, DIFF02 (AP57) | [AP57](57-paved-path-size-guard.md) |
| R15 | No secrets in files; placeholders or `$(...)` | `$(az account get-access-token ...)` | a pasted token | VER03 | [`plugins/pack-check/skills/pack-check/scripts/check-verify.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-verify.sh) |
| R16 | Never force push; a new `-v2` branch instead | `git push origin feat-v2` | `git push --force` | ForcePushGuardTests (PBI-02, planned) | [PBI-2](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/02-release-tags-no-force-push.md) (PR #80) |
| R17 | Every PBI has `proof:` and `door:` | `door: two-way` | no door line | TPL01 (AP36, planned) | [PBI-15](https://github.com/svennijhuis/agentPacks/blob/88d34614e6e3e46672a2e1e8f6506d9c29c7ebab/docs/pbi/15-door-field.md) (PR #80) |
| R18 | Agent names `squad-<role>`, skills kebab-case | `squad-repro` | `ReproAgent` | validate (closed tool list, name checks) | [`docs/ADD-AGENT.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/docs/ADD-AGENT.md) |
| R19 | One table for verdict words and evidence labels | `measured`, `observed`, `inferred` | three sets of words | Reviewer + AP43 table | [AP43](43-verdict-labels.md) |
| R20 | Every finding names its Guard | `Guard: lint` | no Guard | REV12 (AP51) | [AP51](51-finding-guard-layer.md) |

Count: 20 authoring rules. Machine rules: see [RULES.md](RULES.md#rule-table) (moved unchanged).

## Example

Good Reviewer finding: `F02 [low] docs/plans/export.md:1 file name has no date (R01, PLAN01). Fix: rename to 2026-10-26-export.md. Guard: lint`.
Bad: `naming is off`.

## Definition of Done

- [ ] `rg -c "^\| R[0-9]{2} " shared/standards/rules.md` prints 20.
- [ ] `rg -c "^\| (PLAN|VER|RETRO|SCN|REV|SQD|DIFF|BND|PAT|RUL)[0-9]{2} " shared/standards/rules.md` equals the same count in RULES.md before the move.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter StandardsCatalogTests` exits 0. RUL01: every R row has 6 cells. RUL02: ids unique across both parts. RUL03: Enforcement is a known rule id, a test class name, or starts with `Reviewer`.
- [ ] `wc -l < docs/pbi/pstack-tstack/RULES.md` prints 1 after the move.

## Technical design

- Module boundary: rules.md is the one home for rule text; scripts own the check code; patterns.md (AP62) links rule ids, never copies text.
- Rules: RUL01, RUL02, RUL03.

## Sources

- Talk: [talk 25:05](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L71) (single paved path), [talk 25:50](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-transcript.md#L73) (lint first). Notes [notes 4.11](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L68), [notes 4.12](https://github.com/svennijhuis/agentPacks/blob/f537b585ee09b291afcd78c64cf5c62e8c93f1d6/docs/pbi/pstack-tstack/sources/poteto-2500prs-notes.md#L69).
- pstack pstack [`correct/SKILL.md:30`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/correct/SKILL.md#L30) (table that pairs each rule with what enforces it).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet, planned).

- new (planned): [`shared/standards/rules.md`](../../../shared/standards/rules.md)
- edit (created by an earlier PBI in this set, planned): `tools/AgentPacks.Cli.Tests/StandardsCatalogTests.cs`
- edit: [`docs/pbi/pstack-tstack/RULES.md`](../../../docs/pbi/pstack-tstack/RULES.md) (this PR)
