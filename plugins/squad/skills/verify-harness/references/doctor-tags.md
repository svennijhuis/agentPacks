# Doctor tags

Optional checks carry `needs: kv|secret|auth|local-resource`.

- Hard check (app up, port or health endpoint answers): fails means `blocked`, never `pass`.
- Tagged check passes only on a 2xx. A 403, any other non-2xx status, or any error means the check failed (a stub that answers 403 is not "up"). Result: `skipped` with the reason; the rest keeps running.
- A Drive scenario that depends on a failed tagged check: `skipped`, not `pass`, not `blocked`.
- A scenario not driven for any other reason: `not run`, never `skipped`.
