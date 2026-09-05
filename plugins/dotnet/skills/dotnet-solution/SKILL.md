---
name: dotnet-solution
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. Read-only local view of a .NET solution and package graph. Use local CLI or local stdio MCP tools list_projects, list_packages, describe_project. Never a remote service.
license: UNLICENSED
metadata:
  audience: loop
---

# .NET solution (local, read-only)

Local machine only. No hosted Roslyn. No remote MCP. No credentials.

1. `dotnet sln <solution> list`
2. `dotnet list <csproj> package`
3. Or the local stdio process in `mcp/DotnetSolutionMcp.csproj`: `--list-tools`, `--list-projects`, `--list-packages`
4. Or read `*.sln`, `*.slnx`, `*.csproj`, `Directory.Packages.props`, `packages.lock.json`

MCP tools when the local stdio server is attached: `list_projects`, `list_packages`, `describe_project`.

No write tools. No restore via MCP. No codegen. Do not invent a fourth tool. Never write `/dotnet-solution` as prose to load this skill.
