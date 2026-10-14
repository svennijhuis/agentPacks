# .NET project layering

This shape is onion. Domain stays in the center. Application use cases sit around it.
Infrastructure and the ASP.NET host stay outside.

`## Folders` applies to every project, single-project apps included. The layer sections after it
apply when the repository already uses layered projects (names vary: `Web`/`Api`/`Host`,
`Application`/`Core`, `Infrastructure`/`Data`). Match the repo's names; do not invent a fourth
layer the solution does not have.

## Folders

Every new file in a user project goes in a folder by concern. The implementer places each new file
by this tree; the reviewer checks each new file against it. The folder names are one editable list.

```text
MyApp.Api/
  Program.cs                 only root .cs, plus GlobalUsings.cs
  Features/<Name>/           endpoint, service or handler, DTO records for one feature
  Extensions/                DI registration and pipeline setup
  Middleware/                only when needed
  Filters/                   only when needed
  Options/                   options classes, only when needed
MyApp.Infrastructure/        the project that owns the data-access and client dependencies
  DependencyInjection.cs     allowed at this project root
  Data/                      DbContext, Configurations/, Migrations/
  Repositories/              only when the DbContext is not enough
  Clients/                   external adapters
```

- Single-project app: the same tree, with `Infrastructure/` as a folder inside the project.
- No `.cs` file in a project root except `Program.cs` and `GlobalUsings.cs`, plus
  `DependencyInjection.cs` at the Infrastructure project root.
- No global `Services/` or `Models/` dump folders. No `Helpers/` or `Utils/`.
- A repository interface sits next to the code that uses it. Services depend on that interface.
- Create a folder only when a file needs it. No empty folders up front.
- The `.csproj` and `.sln` stay where they are. Layered multi-project solutions keep their projects.

## Dependency rule

Dependencies point inward only:

- **Domain / Core**: entities and rules; no reference to EF, ASP.NET, or HTTP clients.
- **Application**: use cases, DTOs, ports (interfaces). Depends on Domain only. Never references
  Infrastructure or Web.
- **Infrastructure**: EF Core, file/email/HTTP clients, concrete adapters. Implements Application
  ports. Depends on Application (and Domain as needed).
- **Web / Api / Host**: controllers, minimal APIs, middleware, composition root (`Program.cs`).
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

## Application has no infrastructure packages

- **Forbidden in Application:** `PackageReference` or types for persistence, storage, queues,
  email, HTTP vendor SDKs, `DbContext`, connection strings, account keys, and concrete clients.
- Ports stay in Application. Adapters and SDKs stay in Infrastructure.

Bad: Application takes an infrastructure SDK so a use case can talk to tables directly.

```xml
<!-- Application.csproj: do not -->
<PackageReference Include="Azure.Data.Tables" Version="..." />
```

Fix: port in Application, adapter in Infrastructure.

```csharp
// Application
public interface IOrderStore
{
    Task UpsertAsync(Order order, CancellationToken ct);
}
```

```csharp
// Infrastructure
sealed class AzureOrderTableStore(TableClient client) : IOrderStore
{
    public Task UpsertAsync(Order order, CancellationToken ct)
    {
        var entity = new OrderEntity
        {
            PartitionKey = order.CustomerId,
            RowKey = order.Id,
            // map remaining Order fields onto the table entity here
        };
        return client.UpsertEntityAsync(entity, cancellationToken: ct);
    }
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

## Outbound HTTP clients use RestEase

Call other HTTP APIs through a typed [RestEase](https://github.com/canton7/RestEase) interface. Do
not hand-roll `HttpClient` wrappers that build URLs, serialize bodies, and check status codes in new
code. Follow an existing client library when the repo already standardizes on one.

The RestEase interface carries RestEase attributes, so it never lives in Application. One project
per external system (not per package), so `<App>.Application` owns only the port
and domain types, and `<App>.Infrastructure.<Vendor>` owns the RestEase interface, the wire DTOs,
the adapter that maps DTOs to domain types, resilience, and one registration extension. The host only calls that
extension (package `RestEase.HttpClientFactory`).

Bad: `<App>.Application` references RestEase and `HttpClient`.

```csharp
// <App>.Application/WeatherClient.cs   (Application.csproj references RestEase)
public sealed class WeatherClient(HttpClient http)
{
    public async Task<Forecast> GetAsync(string city, CancellationToken ct)
    {
        var response = await http.GetAsync($"forecast?city={city}", ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<Forecast>(ct))!;
    }
}
```

Fix: one type per file, split by project.

```text
<App>.Application/IWeatherService.cs                    port
<App>.Application/Forecast.cs                           domain type
<App>.Infrastructure.Weather/IWeatherApi.cs             RestEase interface
<App>.Infrastructure.Weather/ForecastDto.cs             wire DTO
<App>.Infrastructure.Weather/WeatherService.cs          adapter: IWeatherService, DTO to domain
<App>.Infrastructure.Weather/WeatherServiceCollectionExtensions.cs   AddWeatherClient
<App>.Api/Program.cs                                    calls AddWeatherClient only
tests/<App>.Application.Tests                           use cases against IWeatherService
tests/<App>.Infrastructure.Weather.Tests                adapter with a stub HttpMessageHandler
```

```csharp
// <App>.Application/IWeatherService.cs
public interface IWeatherService
{
    Task<Forecast> GetForecastAsync(string city, CancellationToken ct);
}

// <App>.Application/Forecast.cs
public sealed record Forecast(string City, decimal TemperatureCelsius);

// <App>.Infrastructure.Weather/IWeatherApi.cs
public interface IWeatherApi
{
    [Get("forecast")]
    Task<ForecastDto> GetForecastAsync([Query] string city, CancellationToken ct);
}

// <App>.Infrastructure.Weather/ForecastDto.cs
public sealed record ForecastDto(string Name, double TempC);

// <App>.Infrastructure.Weather/WeatherService.cs
internal sealed class WeatherService(IWeatherApi api) : IWeatherService
{
    public async Task<Forecast> GetForecastAsync(string city, CancellationToken ct)
    {
        var dto = await api.GetForecastAsync(city, ct);
        return new Forecast(dto.Name, (decimal)dto.TempC);
    }
}

// <App>.Infrastructure.Weather/WeatherServiceCollectionExtensions.cs
public static class WeatherServiceCollectionExtensions
{
    public static IServiceCollection AddWeatherClient(this IServiceCollection services, IConfiguration config)
    {
        services.AddTransient<WeatherAuthHandler>();
        services.AddRestEaseClient<IWeatherApi>(config["Weather:BaseUrl"]!)
            .AddHttpMessageHandler<WeatherAuthHandler>()
            .AddStandardResilienceHandler();
        services.AddScoped<IWeatherService, WeatherService>();
        return services;
    }
}

// <App>.Api/Program.cs
builder.Services.AddWeatherClient(builder.Configuration);
```

`WeatherAuthHandler` is a `DelegatingHandler` in its own file in the same project; it reads the key
from configuration, never from Application.

## Simplicity

Use the layers the repo already has. Do not introduce Clean Architecture ceremony (extra projects,
generic repositories, mediator) into a simple CRUD app that does not use them. When layers exist,
respect them; when they do not, do not invent them for one feature.
