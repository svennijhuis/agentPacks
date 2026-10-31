# PBI-02: Release tags on generated commit + no force push

Status: todo
Phase: A contract and release
Depends on: 01
Gap(s): G2
proof: `bash -c '! grep -rnE "git push" .github/workflows --include="*.yml" -A1 | grep -qE -- "--force-with-lease|--force(=| )|-f |\\+refs"' && dotnet test tools/AgentPacks.Cli.Tests --filter ForcePushGuardTests`
door: one-way (changes release automation and creates tags; Sven's yes in Q2, Q3, Q10; rollback: revert the workflow commit, a pushed tag is never moved, cut the next patch tag instead)

## Goal

Make generated marketplace SHAs durable with annotated tags on the generated commit, and stop branch force-pushes in marketplace automation. Align with agentic-workflows POL19 spirit: normal git push only.

## Context

`git ls-remote --tags` is empty. `publish-marketplace.yml:113-114` and `promote-marketplace.yml:5-6,96-97` force-push `marketplace-beta` and `marketplace`, so pins by SHA vanish. Tags must sit on the generated tree (agents live there, not on main). Moving `v1` for `gh aw update` followers is a tag force-update (Q10).

## Steps

1. Change publish flow: append a generated commit with a normal push, or publish each release to a new generated branch (PO pick during implementation).
2. Change promote flow: promote the exact beta SHA to `marketplace` with a normal push (no `--force` branch overwrite, no exceptions for branches); document the chosen strategy in ADR 0001 or the contract doc.
3. After promote, create annotated tag `vX.Y.Z` with message `source: <main sha>` on the generated commit on `marketplace`.
4. On beta testers' commit, tag `vX.Y.Z-beta.N` on `marketplace-beta`.
5. Never tag `main`.
6. Add a `ForcePushGuardTests` (or equivalent) assertion in `tools/AgentPacks.Cli.Tests` that scans `.github/workflows/**` and named scripts for forbidden `git push` refspecs: `--force`, `--force-with-lease`, `-f`, or `+refs` (do not flag `git checkout --force`). Wire it into the existing `validate` job via `dotnet test` (or one step on that job), not a new Actions job. The only allowed exception is the `v1` tag move, and only if Sven allows it in Q10 (one named line, tag refs only).
7. Document Q10 choice: sole exception for moving `v1` tag, or drop `v1` and pin exact tags only.

## DoD

- `publish-marketplace.yml` and `promote-marketplace.yml` no longer force-push branches: no `git push` line in `.github/workflows/*.yml` may use `--force`, `--force-with-lease`, `-f`, or `+refs` on the push refspec (Q10 `v1` tag-only exception if Sven allows it).
- Same as `proof:`: workflow grep clean for branch force-pushes and `ForcePushGuardTests` green in the `validate` job `dotnet test` run.
- First annotated tag workflow documented: message format `source: <main sha>`.
- Beta tag pattern `vX.Y.Z-beta.N` documented in release doc.
- Docs updated; no em or en dashes.

## Evidence

- CI run showing force-push guard fails on a fixture bad workflow.
- `git tag -l` after dry-run or first release (Tester).
- Diff links for `.github/workflows/publish-marketplace.yml:113-114`, `promote-marketplace.yml:5-6,96-97`.

## Flow

```mermaid
sequenceDiagram
  participant Main as main push
  participant Pub as publish-marketplace
  participant Beta as marketplace-beta
  participant Prom as promote-marketplace
  participant Mkt as marketplace
  Main->>Pub: generate (normal push)
  Pub->>Beta: commit + optional beta tag
  Prom->>Mkt: promote SHA (no branch force)
  Prom->>Mkt: annotated vX.Y.Z tag
```

## Named asks

- **Q2:** First tag `v1.0.0` (contract v1) or `v0.2.0`?
- **Q3:** OK to tag the generated `marketplace` commit (one ref for imports and plugins), instead of tagging main?
- **Q10:** Moving `v1` tag needs a tag force-update: allow as the only exception, or exact tags only?

## Gist link

None.

## Sources

- Strategy PBI 02, gap G2, mirrors AW POL19.
- Packet section 3 release tag table, section 0 force-push evidence.
- `.github/workflows/publish-marketplace.yml:6-9,19,113-114`.
- `.github/workflows/promote-marketplace.yml:5-6,11-14,96-97`.
