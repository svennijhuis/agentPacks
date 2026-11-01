# GA02: Pin strategy: tag on the generated commit, never a branch
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: GA01
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- `gh aw compile --strict` rejects branch refs. `marketplace-beta` is force-pushed. `plugins:` at a main SHA loads zero agents (AW C12).

## Target

| Field | Pin | Why |
|---|---|---|
| `imports:` (command file) | `@v2.0.0` or its 40-hex SHA | strict compile |
| `skills:` | same pin as imports | one version per workflow |
| `plugins:` (subagents) | `@v2.0.0` tag only | main SHA loads zero agents |

The AW room owns how pins are bumped there (tags or 40-hex only).

## Example

Good:

`skills:` and `imports:` both at `@v2.0.0`.

Bad:

`imports:` at `@v2.0.0` and `skills:` at `@main`, or `plugins:` at a main SHA.

## Steps for Builder

1. Document in this README and in the AW consume how-to (AW room owns that file).
2. Test in agentPacks: generated commit has the tag and every closure path exists at it.

## DoD

- Local: Cursor, Claude Code and CLI still load the same files (generated manifests and fixture tests, offline).
- gh-aw: every fixture under `tests/fixtures/gh-aw/` passes `gh aw compile --strict` with the pinned gh-aw version. No workflow run, no secrets, no Actions minutes.
- `validate` 0 errors, 0 warnings; `/pack-check` the same; tests green; main green after merge.
- Personal svennijhuis only. No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Compile output of the fixtures, test names, closure diff.
