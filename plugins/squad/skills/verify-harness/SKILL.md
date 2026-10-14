---
name: verify-harness
description: When writing a runnable docs/verify.md harness from launch clues and smoke scenarios.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
metadata:
  audience: loop
---

Write for the next agent: it arrives cold, mid-task, and must prove a change by running the real app.

# Verify harness

Internal. Do not run directly: Squad loads this by exact Skill name.

1. **Interview the repo.** Read `package.json`, `*.csproj`, `Dockerfile`, `Makefile` and `README*` via `git ls-files` and `rg`, never `find`, so the launch command is found, not guessed.
2. **Generate or extend `docs/verify.md`.** Five H2 sections in order: `## Launch`, `## Doctor`, `## Drive`, `## Evidence`, `## Cleanup`.
3. **Prove it.** When creating `docs/verify.md`, run Launch, Doctor, Drive for ONE feature, Cleanup, as a fast handoff proof. Hand over only if that run passes, and quote its output.
4. **Write the smoke report.** Follow [smoke report](references/smoke-report.md). Then run pack-check `check-verify.sh docs/verify.md docs/smoke-report.md` (or the `.ps1` twin) and fix every finding before handover.

Sections:
- **Launch: exact command, ready signal (port or health endpoint first, else a log line), timeout**, because a hung dev server must not stall the loop. Follow [process rules](references/process.md).
- **Doctor: read-only checks only, port or health endpoint first**, because checking must never change state. Hard check fails means `blocked`; tagged checks follow [doctor tags](references/doctor-tags.md).
- **Drive: one entry per scenario, named like the smoke Case**, so the verifier can pick the touched ones.
- **Drive defaults to API: HTTP calls with curl or the repo's existing http client on real endpoints, checking status code, key payload fields and one error case**, because most changes are APIs. UI tools only if the repo has them.
- **Evidence: `$(git rev-parse --git-dir)/squad/verify/<slug>/<run-id>/`, one file per scenario**, because it stays out of git and earlier runs are never overwritten.
- **Cleanup: stop command, keep evidence**, because proof outlives the process.

Rules:
- **Extend an existing verify.md, never overwrite it**, because hand edits must survive a rerun; change a line only where the app now contradicts it.
- **Every later run (verifier, or `/scenarios` with an existing verify.md) drives every touched scenario; `skipped` only for a failed `needs:` check, `not run` for anything else not driven**, so gaps stay visible.
- **Drive the real user path, not test-only endpoints**, because a side door proves nothing.
- **Capture action and result, and check side effects in files, rows or logs**, because a 200 alone can hide a lost write.
- **Mock only at an existing external boundary**, because a mocked core is not the app.
- **Use an e2e tool or Playwright only if the repo already has one; never install or vendor browser tooling**, because v1 adds no tools.
- **Stop only what this run started (track PIDs), never kill by name**, because other work may share the box.
- **Run Cleanup on failure too, and confirm evidence still exists after it**, because the proof is the point.
- **No app: Launch and Doctor say none, Drive uses the CLI or tests**, so libraries still get a harness.
- **Secrets: follow the local-secrets rule in [squad skill](../squad/SKILL.md)**, because invented tokens are worse than a skip.

Out of scope: maintaining this skill, browser tooling installs, anything that needs secrets.
