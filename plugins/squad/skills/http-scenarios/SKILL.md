---
name: http-scenarios
description: From changed code (OpenAPI optional), write only docs/smoke/<slug>.md in the current app workspace. No product-code edits. Type /http-scenarios; do not model-invoke.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# HTTP scenarios

One job. Current app workspace (the repo under test). Scenarios md for a real tester on a deployed env.
`/squad-review` is code/diff. `/squad` may read this file later for test design; it does not write or push smoke.
Do not require Azure, TST, or AWS. Use `BASE_URL`.

1. Read [the smoke matrix](../../references/smoke-matrix.md). Seed kinds from that shape.
2. Seed **changed code first**: controllers, routes, handlers, Azure Functions / AWS Lambda (HTTP and timer/cron triggers).
3. OpenAPI/Swagger if present — fill gaps only. Not required. Do not ask for a spec when code is enough.
4. Write only `docs/smoke/<slug>.md`. Slug from the change or `info.title`.
5. Table columns: # · Case · Kind · Request · Status · Expected · Why.

| # | Case | Kind | Request | Status | Expected | Why |
|---|---|---|---|---|---|---|
| 1 | list happy | happy | GET $BASE_URL/pets | 200 | JSON array | documented list |

   Kinds: `happy` / `edge` / `fail` / `auth` / `biz` / `nothing-breaks`.
   Default fill: happy / edge / fail / biz / nothing-breaks.
   Fill rows from each changed path/operation. Map matrix Happy/Edge/Fail/Timeout/5xx into those kinds; add `biz` and `nothing-breaks` from documented rules and no-op cases.
6. Timer/cron rows: kind `edge` / `fail` — did not run, ran twice, poison message, partial batch. Not fake HTTP when the trigger is not HTTP.
7. Auth header only, by default: `Auth: Bearer TOKEN_VALID (tester supplies)`.
   Add `auth` / policy rows ONLY when the user ask or the OpenAPI change is about auth or new policies. Do not spam 401/403 rows by default.
8. Placeholders only: `BASE_URL`, `TOKEN_VALID`. Never a real token.
9. Do not edit product source. Do not commit, merge, or push.
10. Bruno/Postman: optional mention of an existing app-repo collection. Do not write one.

Stop when the file is written.

Good: rows from changed handlers; OpenAPI fills gaps; timer rows are not GET.
Bad: requiring a swagger URL, fake HTTP for a cron trigger, or pushing the md.
