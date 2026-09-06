# Deployed smoke

Local integration: in-process host ([http-integration](http-integration.md)).
Smoke: a **deployed** base URL from the app repo.

```csharp
var url = Environment.GetEnvironmentVariable("SMOKE_BASE_URL");
using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
var res = await client.GetAsync(url + "/health");
Assert.Equal(HttpStatusCode.OK, res.StatusCode);
```

URLs and secrets stay in the app repo. Never in this pack.

Good: env base URL + auth/timeout/5xx from the squad smoke-matrix.
Bad: hardcoded URL or token here.
