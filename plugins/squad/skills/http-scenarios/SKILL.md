---
name: http-scenarios
description: From an OpenAPI/Swagger URL or file, write only docs/smoke/<slug>.md in the current app workspace. No product-code edits. Type /http-scenarios; do not model-invoke.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# HTTP scenarios

One job. Current app workspace (the repo under test). Scenarios md for a real tester on a deployed env.
`/squad-review` is code/diff. `/squad` may read this file later; it does not write smoke for push.
Do not require Azure, TST, or AWS. Use `BASE_URL`.

1. Read [the smoke matrix](../../references/smoke-matrix.md). Seed kinds from that shape.
2. Take OpenAPI/Swagger from a URL or file path. If missing, ask once.
3. Write only `docs/smoke/<slug>.md`. Slug from `info.title` or the spec filename.
4. Table columns: # · Case · Kind · Request · Status · Expected · Why.

| # | Case | Kind | Request | Status | Expected | Why |
|---|---|---|---|---|---|---|
| 1 | list happy | happy | GET $BASE_URL/pets | 200 | JSON array | documented list |

   Kinds: `happy` / `edge` / `fail` / `auth` / `biz` / `nothing-breaks`.
   Default fill: happy / edge / fail / biz / nothing-breaks.
   Fill rows from each path/operation. Map matrix Happy/Edge/Fail/Timeout/5xx into those kinds; add `biz` and `nothing-breaks` from documented rules and no-op cases.
5. Auth header only, by default: `Auth: Bearer TOKEN_VALID (tester supplies)`.
   Add `auth` / policy rows ONLY when the user ask or the OpenAPI change is about auth or new policies. Do not spam 401/403 rows by default.
6. Placeholders only: `BASE_URL`, `TOKEN_VALID`. Never a real token.
7. Do not edit product source. Do not commit, merge, or push.
8. Bruno/Postman: optional mention of an existing app-repo collection. Do not write one.

Stop when the file is written.

Good: rows from OpenAPI operations; env placeholders only; deployed env via `BASE_URL`.
Bad: patching a controller, requiring Azure/TST/AWS, or a default 401/403 matrix.
