# Integration

Lives in the repo's `tests/` (or existing folder). Touches real I/O or a composed process.

```ts
it("returns 400 when the field is missing", async () => {
  const res = await fetch(app + "/items", { method: "POST", body: "{}" });
  expect(res.status).toBe(400);
});
```

```bash
<pm> run test
```

A green happy-path-only file is not coverage.
