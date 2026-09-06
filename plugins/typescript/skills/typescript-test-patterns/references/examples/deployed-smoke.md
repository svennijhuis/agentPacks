# Deployed smoke

Local integration: in-process or `tests/` ([integration](integration.md)).
Smoke: a **deployed** base URL from the app repo.

```ts
const url = process.env.SMOKE_BASE_URL;
const res = await fetch(url + "/health", { signal: AbortSignal.timeout(5_000) });
expect(res.status).toBe(200);
```

URLs and secrets stay in the app repo. Never in this pack.

Good: env base URL + auth/timeout/5xx from the squad smoke-matrix.
Bad: hardcoded URL or token here.
