---
name: http-scenarios
description: From OpenAPI, write docs/smoke/<slug>.md. No product-code edits.
---

# HTTP scenarios

Load the `http-scenarios` skill with the Skill tool by exact name `http-scenarios`. Never write
`/http-scenarios` as prose to load it. Then write only `docs/smoke/<slug>.md` in the current
app workspace (the repo under test) from the given OpenAPI/Swagger URL or file path. Do not
edit product source. Do not commit, merge, or push. `/squad-review` is code/diff; this slash
is scenarios md for a real tester on TST. `/squad` may read the file later; it does not write
smoke for push.
