# Fire-and-forget

```csharp
_ = SaveAsync(order);
```

Good: `Orders.cs:12` — fire-and-forget `Task` on the request path. Cite `async-errors.md`.

Bad: "looks fine" with no location or standard.
