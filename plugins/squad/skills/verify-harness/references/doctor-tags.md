# Doctor tags

Optional checks carry `needs: kv|secret|auth|local-resource`.

- Hard check (app up, port or health endpoint answers): fails means `blocked`, never `pass`.
- Tagged check passes only on a 2xx. A 403, any other non-2xx status, or any error means the check failed (a stub that answers 403 is not "up"). Result: `skipped (needs: <tag>, <status>)`, e.g. `skipped (needs: kv, 403)`; the rest keeps running.
- A Drive scenario that depends on a failed tagged check: `skipped (needs: <tag>, <status>)` too, not `pass`, not `blocked`. A `skipped` without a `needs:` reason is not a pass.
- A scenario not driven for any other reason: `not run`, never `skipped`.
- Every later run drives every touched scenario; a touched scenario not driven is `not run`, which blocks. `skipped` does not block.
