# Smoke report

Write tracked `docs/smoke-report.md`, fully rewritten each run (never appended), so it always shows the last run only.

Per scenario, one heading named like the smoke Case, then:
- Manual steps: what a human can follow without the harness.
- Expected: the result that counts as pass.
- Outcome: `pass`, `fail`, `not run`, or `blocked`.
- Skip: tag-only `skipped (needs: <tag>)`, only for a failed `needs:` check. The verifier copies it.
- Harness writer shared note:
  `Local access: <redacted line>`
  `Hint: <login command>`
- Verifier (read-only): write its own one-line `Local access:` note in its report, never here.
- Evidence: the path as plain text, marked local to the run machine. No clickable link; it would be dead on GitHub.

Evidence files stay under `$(git rev-parse --git-dir)/squad/verify/<slug>/<run-id>/` and are never tracked.

Example:

Local access: DefaultAzureCredential failed to retrieve a token from the included credentials.
Hint: `az login`

### unknown id
- Manual steps: `curl -s -w '%{http_code}' -X GET http://localhost:8087/pets/no-such` (no body).
- Expected: status 404, `application/problem+json` with `title`.
- Outcome: `pass`
- Evidence: .git/squad/verify/pets-api/20261013-2300/unknown-id.txt (local to the run machine)

### create (needs auth)
- Manual steps: POST /pets with auth.
- Expected: status 201.
- Outcome: `skipped (needs: auth)`
- Evidence: .git/squad/verify/pets-api/20261013-2300/create.txt (local to the run machine)

### me (needs auth)
- Manual steps: GET /me with auth.
- Expected: status 401 without token, 200 with token.
- Outcome: `skipped (needs: auth)`
- Evidence: .git/squad/verify/pets-api/20261013-2300/me.txt (local to the run machine)

Five scenarios needing the same auth skip with one attempt: five `skipped` Outcomes, one shared `Local access:` note, one `Hint:` line, one retro line.

No em dashes in `docs/verify.md` or `docs/smoke-report.md`; step 4 lint fails on them.
