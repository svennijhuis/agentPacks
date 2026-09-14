# HTTP API (REST)

Cross-language. Cite `http-api.md` on API shape edits.

## Collection envelope

Prefer a named collection key over a root JSON array. Envelope can add `page` / `total` / `next` later. Root array → object is a breaking change.

Good:

```json
{ "users": [{ "id": 1 }] }
```

Bad:

```json
[{ "id": 1 }]
```

## Resources and methods

Nouns in paths (`/users`, `/users/{id}`). Verbs are HTTP methods. Do not encode actions in path (`/getUser`, `/doCreate`).

| Method | Use |
|---|---|
| GET | read, safe, idempotent |
| POST | create or non-idempotent action; return `201` + `Location` when a resource is created |
| PUT | replace; idempotent |
| PATCH | partial update; idempotent where practical |
| DELETE | remove; idempotent |

## Errors

One JSON error object. Prefer RFC 7807 `application/problem+json` (`type`, `title`, `status`, `detail`) or `{ "error": { "code", "message" } }`. Match status to class (4xx client, 5xx server). Do not return 200 with an error body.

## Pagination

Plan for pages early. Put page metadata beside the collection (`next`, `nextLink`, or `page`/`total`), not a bare array. Keep filter/sort stable across pages. Treat continuation tokens/URLs as opaque.

## Related resources

Need related records without a second client request? Add `included` (or embed on the item) on the envelope. Opt in with `?include=`. Do not invent `/getUserWithOrders`. Keep the collection key stable.

## Idempotency and concurrency

GET/PUT/DELETE safe to retry. POST create: make retry-safe (idempotency key or natural key) or document duplicate risk. Prefer `ETag` / `If-Match` for concurrent updates when resources race.

## Stability

Do not break shipped JSON shapes. Add fields; do not remove/rename without a version story. Prefer additive envelope fields over reshaping root types.
