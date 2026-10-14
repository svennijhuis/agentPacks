# Smoke report

Write tracked `docs/smoke-report.md`, fully rewritten each run (never appended), so it always shows the last run only.

Per scenario, one heading named like the smoke Case, then:
- Manual steps: what a human can follow without the harness.
- Expected: the result that counts as pass.
- Outcome: `pass`, `fail`, `not run`, or `blocked`.
- Outcome skip form: tag-only `skipped (needs: <tag>)`. Never put status or reason in the Outcome.
- That skip form is for a failed `needs:` check only; the verifier copies the tag-only form into its Result cell.
- Access-error skips: tag-only Outcome (`skipped (needs: auth)`). Harness writer shared note:
  `Local access: <redacted line>`
  `Hint: <login command>`
- Verifier (read-only): write its own one-line `Local access:` note in its report, never here.
- Evidence: the path as plain text, marked local to the run machine. No clickable link; it would be dead on GitHub.

Access errors (see [local access](local-access.md)): one try only across the whole run. After a startup access error or the first token failure, every scenario that hits the same protected path or needs the same tag (`kv` or `auth`) is skipped up front. No second token or getToken attempt of any scope (a vault-scope try at startup and then an api-scope token for a Drive call to a 401 path counts as two tries). Harness writer: write the skip reason once as the shared `Local access:` note plus one `Hint:` line, plus one retro line. Do not retry login. Verify never blocks `/squad`.

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
