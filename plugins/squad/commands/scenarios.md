---
name: scenarios
description: From changed code, write docs/smoke/<slug>.md. No product-code edits.
---

# HTTP scenarios

Load the `scenarios-md` skill with the Skill tool by exact name `scenarios-md`. Never write
`/scenarios` as prose to load it. Then write only `docs/smoke/<slug>.md` in the current
app workspace (the repo under test). Seed **changed code first** (controllers, routes, handlers,
Azure Functions / AWS Lambda HTTP and timer/cron). OpenAPI/Swagger fills gaps only; not required.
Do not edit product source. Do not commit, merge, or push. `/squad-review` is code/diff; this slash
is scenarios md for a real tester on a deployed env. `/squad` may read the file later for test
design; it does not write smoke for push. Request is the method and path from the changed handler.
Do not invent a host. Do not require Azure, TST, or AWS.
Local secrets: the Squad local-secrets rule (skill `squad`).
