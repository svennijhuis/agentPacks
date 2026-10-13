# Doctor tags

Optional checks carry `needs: kv|secret|auth|local-resource`.

- Hard check (app up, port answers) fails: `blocked`, never `pass`.
- Tagged check fails (for example a local Key Vault 403): `skipped` with the reason; the rest keeps running.
- A Drive scenario that depends on a skipped check: `skipped`, not `pass`, not `blocked`.
