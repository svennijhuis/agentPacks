# .NET project layering

This shape is onion. Domain stays in the center. Application use cases sit around it.
Infrastructure and the ASP.NET host stay outside.

Apply when the repository already uses layered projects (names vary: `Web`/`Api`/`Host`,
`Application`/`Core`, `Infrastructure`/`Data`). Match the repo's names; do not invent a fourth
layer the solution does not have. Skip this document for a single-project app or a tiny script.

## Dependency rule

Dependencies point inward only:

- **Domain / Core** — entities and rules; no reference to EF, ASP.NET, or HTTP clients.
- **Application** — use cases, DTOs, ports (interfaces). Depends on Domain only. Never references
  Infrastructure or Web.
- **Infrastructure** — EF Core, file/email/HTTP clients, concrete adapters. Implements Application
  ports. Depends on Application (and Domain as needed).
- **Web / Api / Host** — controllers, minimal APIs, middleware, composition root (`Program.cs`).
  References Application; may reference Infrastructure **only** for DI registration at the host.

## Controllers stay thin

- Controllers / minimal-API endpoints map HTTP ↔ Application use cases (handlers/services). They do
  not own business rules.
- **Forbidden in Web controllers/endpoints:** `DbContext`, EF queries, repository implementations,
  Infrastructure concrete types, direct SQL, or new Infrastructure project references added so a
  controller can touch persistence.
- Controllers call Application (injected use-case / service / mediator). Persistence and external I/O
  stay behind Application ports implemented in Infrastructure.

Fix:

```csharp
// Application
sealed class PlaceOrder(IOrderStore orders) { public Task Handle(Order order) => orders.Add(order); }
```

```csharp
// Infrastructure
sealed class EfOrders(AppDbContext db) : IOrderStore { public Task Add(Order order) => db.Orders.AddAsync(order).AsTask(); }
```

## Application has no storage SDKs

- **Forbidden in Application:** `PackageReference` to `Azure.Data.Tables` (or any other
  storage SDK), connection strings, account keys, and Table/Blob/Queue clients.
- Ports (interfaces) live in Application. Concrete adapters live in Infrastructure.

Bad: Application takes a storage SDK so a use case can talk to tables directly.

```xml
<!-- Application.csproj: do not -->
<PackageReference Include="Azure.Data.Tables" Version="..." />
```

Fix: port in Application, adapter in Infrastructure.

```csharp
// Application
public interface IOrderTableStore
{
    Task UpsertAsync(OrderRow row, CancellationToken ct);
}
```

```csharp
// Infrastructure
sealed class AzureOrderTableStore(TableClient client) : IOrderTableStore
{
    public Task UpsertAsync(OrderRow row, CancellationToken ct) =>
        client.UpsertEntityAsync(row, cancellationToken: ct);
}
```

When a layered move is large, keep this dependency rule. Do not ship a short known-worse
shape. Stop, split, or ask.

## Where new code goes

| Change | Put it in |
|---|---|
| Entity / domain rule | Domain |
| Use case, DTO, port interface | Application |
| EF mapping, DbContext, external client | Infrastructure |
| HTTP contract, auth attributes, DI wiring | Web / Host |

Do not add an Infrastructure class into the Web project to "keep it local." Do not teach a
controller to construct Infrastructure types. Prefer the existing Application seam; add a port in
Application and an adapter in Infrastructure when one is missing.

## Simplicity

Use the layers the repo already has. Do not introduce Clean Architecture ceremony (extra projects,
generic repositories, mediator) into a simple CRUD app that does not use them. When layers exist,
respect them; when they do not, do not invent them for one feature.
