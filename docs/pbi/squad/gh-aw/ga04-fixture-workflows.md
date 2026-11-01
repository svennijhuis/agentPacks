# GA04: Fixture workflows that compile strict
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: GA01, GA02, GA03
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- Proof for every PBI that it works in gh-aw, offline.

## Target

`tests/fixtures/gh-aw/squad-review.md` (report only, staged safe outputs) and `tests/fixtures/gh-aw/scenarios.md` (staged docs output). Both `workflow_dispatch` only, caps and permissions in frontmatter, no `skills:` in any `shared/` fragment.

```yaml
on:
  workflow_dispatch:
    inputs:
      pr: { description: PR number, required: true, type: string }
max-turns: 15
timeout-minutes: 10
skills:            # copied from plugins/squad/aw-closure.json
  - svennijhuis/agentPacks/plugins/squad/skills/squad-md@<40-hex>
  - svennijhuis/agentPacks/plugins/squad/skills/tdd@<40-hex>
imports:
  - svennijhuis/agentPacks/plugins/squad/commands/squad-review.md@<40-hex>
permissions: { contents: read, pull-requests: read }
tools:
  bash: [cat, grep, head, ls, wc, "git diff:*", "git log:*", "git show:*"]
safe-outputs:
  staged: true
  create-pull-request-review-comment: { max: 10 }
```

## Example

Good:

Fixture compiles strict, `.lock.yml` committed next to it.

Bad:

A fixture with `on: schedule`, a branch ref, or `skills:` inside `shared/`.

## Steps for Builder

1. CI job runs `gh aw compile --strict` on the fixtures with the pinned gh-aw version (offline, no run).
2. Fixtures use local paths in CI and the tag form in docs.

## DoD

- Local: Cursor, Claude Code and CLI still load the same files (generated manifests and fixture tests, offline).
- gh-aw: every fixture under `tests/fixtures/gh-aw/` passes `gh aw compile --strict` with the pinned gh-aw version. No workflow run, no secrets, no Actions minutes.
- `validate` 0 errors, 0 warnings; `/pack-check` the same; tests green; main green after merge.
- Personal svennijhuis only. No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Compile output of the fixtures, test names, closure diff.
