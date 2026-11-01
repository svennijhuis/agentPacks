# PS03: Validator: paths, index names, playbook pointers, copy rule
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B0
- Depends on: PS01
- Absorbs old: none
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Release: squad v2 ([README](README.md#squad-v2))
- Docs: [STRUCTURE](STRUCTURE.md) (where), [SHAPES](SHAPES.md) (how it looks), [FLOWS](FLOWS.md) (flow per command), [MIGRATION](MIGRATION.md) (v1 to v2 rows for this PBI), [gh-aw](gh-aw/README.md)
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "check all feedback so we will accept this all and don't give warnings". 0 warnings must mean a bad pointer fails and a good one passes.

## Current state on main (`6c59f49`)

- `SkillReferenceValidator` only checks backticked `/name`.
- No check for `../<name>/SKILL.md`, index entries, playbook pointers, or copies.

## Target structure

gh-aw (from AW room): REF07 fails when a skill in a pack's `aw-closure.json` names a tool not in the fixture workflow's `tools.bash`, unless it gives a fallback (`rg`, else `grep -rn`). Pins: 40-hex or tag only, never a branch. Detail: [GA01](gh-aw/ga01-closure-file.md), [GA03](gh-aw/ga03-provider-neutral-unattended.md).

| Rule | Check | Severity |
|---|---|---|
| REF01 | `../<name>/SKILL.md` resolves to a plugin skill or an `external-skills.json` record | error |
| REF02 | Bold entry in `## Principles` resolves to a `principle-<kebab>` leaf | error |
| REF03 | Bold entry in `## Standards` resolves to a `standard-<kebab>` leaf | error |
| REF04 | `playbooks/<x>.md` pointer resolves | error |
| REF05 | Client built-ins allowlist (`/loop` only while some pinned file still names it) | info |
| REF06 | A copy has the `Source:` line with a 40-hex SHA, a NOTICE entry, and `## What we changed` | error |
| HUB01 | A skill with the hub H2 set may pass 40 lines | none |

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| kebab rule | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |

## Example

Good (what the result looks like):

```text
REF04 error plugins/squad/skills/squad-md/SKILL.md:88
  playbook pointer playbooks/perf.md does not exist
REF06 error plugins/squad/skills/tdd/SKILL.md:3
  copy has no "Source: ... at <40-hex SHA>" line
```

Bad (what we do not want):

```text
warning: some links might be broken
```
No rule id, no file:line, not a hard failure.

## Subagents and what they load

- none

## Steps for Builder

1. Add REF01 to REF06 and HUB01.
2. One passing and one failing fixture per rule.
3. Mutation probe per rule.

## DoD

- Each rule bites in its probe.
- Local: works in Cursor, Claude Code and CLI (Copilot CLI, Codex). Proof offline: generated client manifests and fixture tests (`ClientManifestTests`, `ComponentGenerationTests`).
- gh-aw: the fixture workflow under `tests/fixtures/gh-aw/` that uses this PBI's files passes `gh aw compile --strict` (pinned gh-aw, no run, no secrets, no Actions minutes). See [gh-aw README](gh-aw/README.md).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green; main stays green and all 7 commands still load after merge ([MIGRATION](MIGRATION.md)).
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Test names, probe output.

## Risks and out of scope

- REF02 tests use FAKE local leafs until PS04 lands.
- Out: pins (PS04).
