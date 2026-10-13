# Smoke report

Write tracked `docs/smoke-report.md`, fully rewritten each run (never appended), so it always shows the last run only.

Per scenario, one heading named like the smoke Case, then:
- Manual steps: what a human can follow without the harness.
- Expected: the result that counts as pass.
- Outcome: `pass`, `fail`, `skipped` with reason, or `blocked`.
- Evidence: the path as plain text, marked local to the run machine. No clickable link; it would be dead on GitHub.

Evidence files stay under `$(git rev-parse --git-dir)/squad/verify/<slug>/` and are never tracked.
