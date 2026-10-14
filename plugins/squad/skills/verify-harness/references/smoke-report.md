# Smoke report

Write tracked `docs/smoke-report.md`, fully rewritten each run (never appended), so it always shows the last run only.

Per scenario, one heading named like the smoke Case, then:
- Manual steps: what a human can follow without the harness.
- Expected: the result that counts as pass.
- Outcome: `pass`, `fail`, `not run`, or `blocked`.
- Outcome skip form: `skipped (needs: <tag>, <status>)`, e.g. `skipped (needs: kv, 403)`.
- That skip form is for a failed `needs:` check only; the verifier copies it into its Result cell.
- Access-error skips use a redacted log line as `<status>`:
  `skipped (needs: kv|auth, <redacted line>)`.
- Evidence: the path as plain text, marked local to the run machine. No clickable link; it would be dead on GitHub.

Access errors (see [local access](local-access.md)): one try only across the whole run. When a startup or Drive access error hits, mark every scenario that needs that access `skipped` at once. Write the skip reason once in this report (shared Outcome text or one shared note), plus one retro line. Do not retry login. Verify never blocks `/squad`.

Evidence files stay under `$(git rev-parse --git-dir)/squad/verify/<slug>/<run-id>/` and are never tracked.

Example:

### unknown id
- Manual steps: `curl -s -w '%{http_code}' -X GET http://localhost:8087/pets/no-such` (no body).
- Expected: status 404, `application/problem+json` with `title`.
- Outcome: `pass`
- Evidence: .git/squad/verify/pets-api/20261013-2300/unknown-id.txt (local to the run machine)

### create (needs auth)
- Manual steps: POST /pets with auth.
- Expected: status 201.
- Outcome: `skipped (needs: auth, DefaultAzureCredential failed to retrieve a token from the included credentials.)`
- Hint: `az login`
- Evidence: .git/squad/verify/pets-api/20261013-2300/create.txt (local to the run machine)

Five scenarios needing the same auth skip with one attempt: five `skipped` Outcomes, one shared reason line in the report, one retro line.

No em dashes in `docs/verify.md` or `docs/smoke-report.md`; step 4 lint fails on them.
