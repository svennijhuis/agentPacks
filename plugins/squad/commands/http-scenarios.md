---
name: http-scenarios
description: From changed code, write docs/smoke/<slug>.md. No product-code edits.
---

# HTTP scenarios

Load the `http-scenarios` skill with the Skill tool by exact name `http-scenarios`. Never write
`/http-scenarios` as prose to load it. Then write only `docs/smoke/<slug>.md` in the current
app workspace (the repo under test). Seed **changed code first** (controllers, routes, handlers,
Azure Functions / AWS Lambda HTTP and timer/cron). OpenAPI/Swagger fills gaps only; not required.
Do not edit product source. Do not commit, merge, or push. `/squad-review` is code/diff; this slash
is scenarios md for a real tester on a deployed env. `/squad` may read the file later for test
design; it does not write smoke for push. Use `BASE_URL`. Do not require Azure, TST, or AWS.
If local Key Vault, a secrets store, or an unauthenticated protected resource is needed to fill
the matrix, mention that in the chat/output and ask the user to continue. Do not invent secrets.
Do not silently skip. Do not hardcode tokens.
