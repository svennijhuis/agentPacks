# Central packages

`Directory.Packages.props` is the version home. Every project can reference the package.

```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>
  <ItemGroup>
    <PackageVersion Include="xunit" Version="2.9.3" />
  </ItemGroup>
</Project>
```

```xml
<!-- App.csproj -->
<ItemGroup>
  <PackageReference Include="xunit" />
</ItemGroup>
```

```bash
dotnet restore <solution>
```

Bad: `<PackageReference Include="xunit" Version="2.9.3" />` under CPM — restore error, not an override. Cite `csharp.md`.
