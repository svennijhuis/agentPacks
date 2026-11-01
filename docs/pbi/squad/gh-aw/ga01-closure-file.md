# GA01: Closure file per pack for `skills:`
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: PS03, PS06
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- A gh-aw workflow gets only the skills listed in its `skills:` field. The hub reads `../<name>/SKILL.md`, so every sibling must be listed too (AW room, 2026-11-01).

## Target

The generator writes `plugins/<pack>/aw-closure.json`: the hub plus every skill REF01 finds from it, each as `svennijhuis/agentPacks/plugins/<pack>/skills/<name>`. Pinned upstream skills are vendored into the generated marketplace commit (same as caveman today), so one agentPacks pin covers them.

```json
{ "pack": "squad", "hub": "squad-md",
  "skills": ["squad-md", "tdd", "statechart-md", "no-comments", "standard-no-em-dash", "principle-fix-root-causes"] }
```

## Example

Good:

```yaml
skills:
  - svennijhuis/agentPacks/plugins/squad/skills/squad-md@v2.0.0
  - svennijhuis/agentPacks/plugins/squad/skills/tdd@v2.0.0
```

Bad:

Only the hub in `skills:`; the run starts but `../tdd/SKILL.md` is missing and the agent improvises.

## Steps for Builder

1. Generator step after REF01.
2. Test: closure equals REF01 result; no branch refs.
3. Vendored pins keep their 40-hex source in the generated record.

## DoD

- Local: Cursor, Claude Code and CLI still load the same files (generated manifests and fixture tests, offline).
- gh-aw: every fixture under `tests/fixtures/gh-aw/` passes `gh aw compile --strict` with the pinned gh-aw version. No workflow run, no secrets, no Actions minutes.
- `validate` 0 errors, 0 warnings; `/pack-check` the same; tests green; main green after merge.
- Personal svennijhuis only. No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Compile output of the fixtures, test names, closure diff.
