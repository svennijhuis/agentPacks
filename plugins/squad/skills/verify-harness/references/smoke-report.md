# Smoke report

Write tracked `docs/smoke-report.md`, fully rewritten each run (never appended), so it always shows the last run only.

Per scenario, one heading named like the smoke Case, then:
- Manual steps: what a human can follow without the harness.
- Expected: the result that counts as pass.
- Outcome: `pass`, `fail`, `skipped (needs: <tag>, <status>)`, e.g. `skipped (needs: kv, 403)` (failed `needs:` check only; the verifier copies this form into its Result cell), `not run`, or `blocked`.
- Evidence: the path as plain text, marked local to the run machine. No clickable link; it would be dead on GitHub.

Evidence files stay under `$(git rev-parse --git-dir)/squad/verify/<slug>/<run-id>/` and are never tracked.

Example:

### unknown id
- Manual steps: `curl -s -w '%{http_code}' -X GET http://localhost:8087/pets/no-such` (no body).
- Expected: status 404, `application/problem+json` with `title`.
- Outcome: `pass`
- Evidence: .git/squad/verify/pets-api/20261013-2300/unknown-id.txt (local to the run machine)

No em dashes in `docs/verify.md` or `docs/smoke-report.md`; step 4 lint fails on them.
