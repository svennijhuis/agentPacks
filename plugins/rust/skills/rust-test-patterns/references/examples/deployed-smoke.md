# Deployed smoke

Local integration: crate `tests/` ([integration](integration.md)).
Smoke: a **deployed** base URL from the app repo.

```rust
let url = std::env::var("SMOKE_BASE_URL").expect("app repo env");
let status = client::get(&format!("{url}/health")).status();
assert_eq!(status, 200);
```

URLs and secrets stay in the app repo. Never in this pack.

Good: env base URL + auth/timeout/5xx from the squad smoke-matrix.
Bad: hardcoded URL or token here.
