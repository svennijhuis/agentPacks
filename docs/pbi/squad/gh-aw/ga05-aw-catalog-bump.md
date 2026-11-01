# GA05: AW catalog moves to v2 (done in the AW room)
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: PS20, GA04
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- The AW repo pins `plugins/squad/skills/squad@34e1dec`. It must move in one PR.

## Target

One PR in the AW repo: `skills:` path `squad` to `squad-md`, closure entries added, all pins to `@v2.0.0`, AW checks green (compile strict plus POL25 and POL31). agentPacks only documents it here; the AW room builds it under its own charter.

## Example

Good:

One PR: path, closure, pins, compile strict green.

Bad:

Bump the pin first and fix the path in a later PR (broken workflow in between).

## Steps for Builder

1. Hand this text to the AW room after PS20.
2. Wait for their merge before GA07.

## DoD

- Local: Cursor, Claude Code and CLI still load the same files (generated manifests and fixture tests, offline).
- gh-aw: every fixture under `tests/fixtures/gh-aw/` passes `gh aw compile --strict` with the pinned gh-aw version. No workflow run, no secrets, no Actions minutes.
- `validate` 0 errors, 0 warnings; `/pack-check` the same; tests green; main green after merge.
- Personal svennijhuis only. No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Compile output of the fixtures, test names, closure diff.
