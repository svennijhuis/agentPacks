# HTTP collection envelope

Prefer a named collection key over a root JSON array for list APIs. An envelope can add
`page` / `total` later. A root array becoming an object is a breaking change — C# and Rust
DTOs take that hit hardest.

Good:

```json
{ "users": [{ "id": 1 }] }
```

Bad:

```json
[{ "id": 1 }]
```
