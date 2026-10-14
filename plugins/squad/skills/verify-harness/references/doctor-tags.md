# Doctor tags

Optional checks carry `needs: kv|secret|auth|local-resource`.

- Hard check (app up, port or health endpoint answers): fails means `blocked`, never `pass`.
- Tagged check passes only on a 2xx. A 403, any other non-2xx status, or any error means the check failed (a stub that answers 403 is not "up"). Result: tag-only `skipped (needs: <tag>)`; the rest keeps running. Reason is not in the Outcome or Result cell.
- A Drive scenario that depends on a failed tagged check: `skipped (needs: <tag>)` too, not `pass`, not `blocked`. A `skipped` without a `needs:` tag is not a pass.
- Startup access errors (patterns in [local access](local-access.md)): match → Outcome `skipped (needs: kv|auth)` (or the single tag). `/scenarios` harness writer: write the redacted reason once as a shared `Local access:` note plus one `Hint:` line in `docs/smoke-report.md`. On `/squad` verify, `squad-verifier` is read-only on those files: write its own one-line `Local access: <redacted reason>; <login hint>` note in its verifier report (whole line <=120 chars: truncate the redacted reason to fit after the prefix and hint; do not cut the reason to 120 first), never in `docs/smoke-report.md`. Non-access startup failure stays `blocked`. One try only, see [local access](local-access.md).
- A scenario not driven for any other reason: `not run`, never `skipped`.
- Every later run drives every touched scenario; a touched scenario not driven is `not run`, which blocks. `skipped` does not block.
