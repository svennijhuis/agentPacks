---
name: http-scenarios
description: From an OpenAPI/Swagger URL or file, write only docs/smoke/<slug>.md in the current app workspace. No product-code edits. Type /http-scenarios; do not model-invoke.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# HTTP scenarios

One job. Current app workspace (the repo under test). Scenarios md for a real tester on TST.
`/squad-review` is code/diff. `/squad` may read this file later; it does not write smoke for push.

1. Read [the smoke matrix](../../references/smoke-matrix.md). Seed kinds from that shape.
2. Take OpenAPI/Swagger from a URL or file path. If missing, ask once.
3. Write only `docs/smoke/<slug>.md`. Slug from `info.title` or the spec filename.
4. Table columns: case · kind · request · status · expected · why.

| case | kind | request | status | expected | why |
|---|---|---|---|---|---|

   Kind is one of `happy` / `edge` / `fail` / `auth` / `biz` / `nothing-breaks`.
   Fill rows from each path/operation. Map matrix Happy/Edge/Fail/Auth/Timeout/5xx into those kinds; add `biz` and `nothing-breaks` from documented rules and no-op cases.
5. Placeholders only: `BASE_URL`, `TOKEN_VALID`, `TOKEN_INVALID`. Never a real token.
6. Do not edit product source. Do not commit, merge, or push.
7. Bruno/Postman: optional mention of an existing app-repo collection. Do not write one.

Stop when the file is written.

Good: rows from OpenAPI operations; env placeholders only.
Bad: patching a controller or committing a collection.
