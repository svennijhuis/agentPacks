# HTTP integration

`WebApplicationFactory<TEntryPoint>` boots the real host in-process.

```csharp
public sealed class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureTestServices(services =>
        {
            // Replace only what must not be real.
        });
}
```

```csharp
public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder().Build();
    public string ConnectionString => container.GetConnectionString();
    public Task InitializeAsync() => container.StartAsync();
    public Task DisposeAsync() => container.DisposeAsync().AsTask();
}
```

Use `IClassFixture` / `ICollectionFixture`, not the constructor, for containers.

```bash
dotnet test <solution> --filter "FullyQualifiedName~Integration"
```
