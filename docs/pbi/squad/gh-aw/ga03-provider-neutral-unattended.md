# GA03: Provider-neutral text, unattended mode and tool allowlist (REF07)
- Status: todo (docs only; build waits for Sven go per PBI)
- Depends on: PS04, PS09
- Release: squad v2
- Docs: [STRUCTURE](../STRUCTURE.md), [SHAPES](../SHAPES.md), [FLOWS](../FLOWS.md), [MIGRATION](../MIGRATION.md), [set README](../README.md), [gh-aw README](README.md)
- AW input: Agentic Workflows room, 2026-11-01

## Goal and why

- Copied pstack skills are Cursor-only in parts. In gh-aw there is no human to answer yes or no.

## Target

- Copy rule: Cursor-only text removed or under `## Provider notes` (one H3 per provider, `gh-aw` included).
- Hub `## Autonomy`: unattended runs take the default answer and write the open question into the report.
- REF07: a skill in a pack's closure may name only tools the fixture workflow allows in `tools.bash`, or it must give a fallback (for example `rg`, else `grep -rn`). The AW `squad-review.md` allowlist today is cat, grep, head, ls, wc and git diff, log, show.

## Example

Good:

`Explore with git ls-files and rg; without rg use grep -rn.`

Bad:

`Run rg over the repo.` in a skill that the gh-aw fixture runs without rg in `tools.bash`.

## How to do it

1. Branch from main: `git switch -c pbi/ga03-provider-neutral-unattended origin/main`.
2. Read [the gh-aw README](README.md) and [SHAPES.md](../SHAPES.md); the AW repo facts are in its table "Per file".
3. Do the **Target** exactly, then the steps below. Nothing else.
4. Run **Done check**, then open one PR titled `GA03: Provider-neutral text, unattended mode and tool allowlist (REF07)`.

## Steps for Builder

1. Add REF07 with a pass and a fail fixture.
2. Add the Autonomy line in squad-md (PS06 owns the hub; this PBI adds the line).
3. Scan copies for client-only lines.

## DoD

- Local: Cursor, Claude Code and CLI still load the same files (generated manifests and fixture tests, offline).
- gh-aw: every fixture under `tests/fixtures/gh-aw/` passes `gh aw compile --strict` with the pinned gh-aw version. No workflow run, no secrets, no Actions minutes.
- `validate` 0 errors, 0 warnings; `/pack-check` the same; tests green; main green after merge.
- Personal svennijhuis only. No em dash, no tokens in markdown, FAKE fixture values only.

## Done check

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
for f in tests/fixtures/gh-aw/*.md; do gh aw compile --strict "$f"; done   # GA04 and later
```

## Evidence

- Compile output of the fixtures, test names, closure diff.
