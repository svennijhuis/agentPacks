---
name: scenarios
description: From changed code, write docs/smoke/<slug>.md. No product-code edits.
---

# HTTP scenarios

Load the `scenarios-md` skill with the Skill tool by exact name `scenarios-md`. Never write
`/scenarios` as prose to load it. Then run its complete changed-code-first flow and write only
`docs/smoke/<slug>.md` in the current app workspace (the repo under test).

Do not edit product source. Do not commit, merge, or push.
