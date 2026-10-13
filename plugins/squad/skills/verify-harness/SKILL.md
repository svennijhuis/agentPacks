---
name: verify-harness
description: When writing a runnable docs/verify.md harness from launch clues and smoke scenarios.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly: Squad loads by exact Skill name.

# Verify harness

Write only `docs/verify.md`. If one already exists, extend it: add missing parts only; change a line only where the app now contradicts it. Never overwrite (hand edits survive a rerun). Explore with `git ls-files` and `rg`, never `find`. Discover launch from `package.json`, `*.csproj`, `Dockerfile`, `Makefile`, `README*`.

Exactly five H2 sections, in this order:

## Launch
Exact start cmd, ready signal (port or log line), timeout. No app → `none`.

## Doctor
Read-only checks. Hard (app up, port answers): fail → `blocked`. Optional tagged `needs: kv|secret|auth|local-resource`: fail → `skipped` with reason; rest keeps running. Local-secrets: [squad skill](../squad/SKILL.md). Never invent tokens. No app → `none`.

## Drive
Per scenario: steps (cmd/curl/selector) + expect. When smoke exists, one Drive entry per Case name. Scenario depending on a skipped Doctor check → `skipped`. No app → CLI or tests only. If the repo already has an e2e tool (e.g. tester-army `e2e`) or Playwright, Drive may use it for UI steps; never install or vendor tester-army/e2e or any browser tooling.

## Evidence
Dir: `$(git rev-parse --git-dir)/squad/verify/<slug>/` (outside worktree; never edit product `.gitignore`). One file per scenario.

Drive the real user path, not test-only endpoints. Capture the action and its result. Confirm side effects in files, rows, or logs. Mock only at an existing external boundary.

## Cleanup
Stop only processes/resources this run started (track PIDs); never kill by name. Never delete evidence (earlier run evidence stays). Always run, even on failure. No app → `none` (still keep evidence).

When a section has nothing: write `none` (or the no-app rule above). Out of scope: maintaining this skill, installing browser tooling, anything that needs secrets.

Dry-run once end to end for real (Launch → Doctor → Drive → Cleanup; Cleanup always). Hand over only if that run passes; quote the output.
