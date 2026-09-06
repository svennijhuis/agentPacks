# Smoke matrix

Deployed/API smoke, not in-process integration. URLs and secrets stay in the **app repo**.

| Case | Probe | Fail when |
|---|---|---|
| Happy | documented ready/GET | not 2xx |
| Edge | empty / one / max allowed input | 2xx on invalid, or 5xx |
| Fail | known bad input | not the documented 4xx |
| Auth | missing or wrong credential | not 401/403 |
| Timeout | client budget from the app repo | hang or unmarked retry |
| 5xx | dependency down or forced 503 | 2xx or unhandled 500 HTML |

Kind `smoke`. Local host / `WebApplicationFactory` is integration — see `<lang>-test-patterns`.
