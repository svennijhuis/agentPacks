# Language packs

Each row makes one language pack discoverable. Marker patterns are matched anywhere beneath the
repository root; `.git`, `bin`, `obj`, `target`, `node_modules`, and `vendor` directories are skipped.

| Marker | Stack | Pack |
|---|---|---|
| `*.slnx`, `*.sln`, `*.csproj` | `dotnet` | `dotnet` |
| `Cargo.toml` | `rust` | `rust` |
