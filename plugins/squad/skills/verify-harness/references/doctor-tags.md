# Doctor tags

Optional checks carry `needs: kv|secret|auth|local-resource`.

- Hard check (app up, port or health endpoint answers): fails means `blocked`, never `pass`.
- Tagged check passes only on a 2xx. A 403, any other non-2xx status, or any error means the check failed (a stub that answers 403 is not "up"). Result: `skipped (needs: <tag>, <status>)`, e.g. `skipped (needs: kv, 403)`; the rest keeps running.
- A Drive scenario that depends on a failed tagged check: `skipped (needs: <tag>, <status>)` too, not `pass`, not `blocked`. A `skipped` without a `needs:` tag is not a pass.
- Startup access errors (patterns in [local access](local-access.md)): match → Outcome `skipped (needs: kv|auth)` (or the single tag); write the redacted reason once as a shared `Local access:` note plus one `Hint:` line in the smoke report. Non-access startup failure stays `blocked`. Missing token → `skipped (needs: auth)` with reason in that shared note. One try only across the whole run: after a startup access error or the first token failure, every scenario that hits the same protected path or needs the same tag (`kv` or `auth`) is skipped up front; no second token or getToken attempt of any scope. Never probe vault or cloud login up front; never retry. Verify never blocks `/squad`.
- A scenario not driven for any other reason: `not run`, never `skipped`.
- Every later run drives every touched scenario; a touched scenario not driven is `not run`, which blocks. `skipped` does not block.
