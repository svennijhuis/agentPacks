---
description: "From changed code, write docs/smoke/<slug>.md. No product-code edits."
---

# HTTP scenarios

Load the `scenarios-md` skill with the Skill tool by exact name `scenarios-md`. If that does not resolve, read `<Squad plugin root>/skills/scenarios-md/SKILL.md` directly; never run `find` for it. Never write
`/scenarios` as prose to load it. Then run its complete changed-code-first flow and write
`docs/smoke/<slug>.md` in the current app workspace (the repo under test). When `docs/verify.md` is missing it asks
yes/no first; when it exists it extends it without asking. On yes or an existing file, the `verify-harness` skill also writes `docs/verify.md` and `docs/smoke-report.md`.

Do not edit product source. Do not commit, merge, or push.
