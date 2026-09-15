# SDK and C# version

`global.json` pins the SDK. `Directory.Build.props` pins `TargetFramework` and `LangVersion` for every project.

```json
{
  "sdk": {
    "version": "10.0.100",
    "rollForward": "latestFeature"
  }
}
```

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <LangVersion>latest</LangVersion>
  </PropertyGroup>
</Project>
```

The `.csproj` inherits those. Do not copy them into each project.

Bad: a different `LangVersion` or `TargetFramework` in one csproj that was easier than editing `Directory.Build.props`. Cite `csharp.md`.
