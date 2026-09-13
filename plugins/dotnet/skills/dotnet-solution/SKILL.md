---
name: dotnet-solution
description: When listing a local .NET solution or package graph.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# .NET solution (local, read-only)

Loop-only; not as a user entrypoint. Local machine only. No shipped MCP server. No credentials.

1. `dotnet sln <solution> list`
2. `dotnet list <csproj> package`
3. Or read `*.sln`, `*.slnx`, `*.csproj`, `Directory.Packages.props`, `packages.lock.json`

No write tools. No codegen. No rename or refactor. Never write `/dotnet-solution` as prose to load this skill.

Good: `dotnet sln list` on the local slnx.
Bad: call a remote package service.
