---
name: dotnet-solution
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. Read-only local view of a .NET solution, package graph, and on-machine Roslyn symbols, refs, and diagnostics. Never a remote service.
license: UNLICENSED
metadata:
  audience: loop
---

# .NET solution (local, read-only)

Local machine only. No hosted Roslyn. No remote MCP. No credentials.

1. `dotnet sln <solution> list`
2. `dotnet list <csproj> package`
3. Or the local stdio process in `mcp/DotnetSolutionMcp.csproj`: `--list-tools`, `--list-projects`, `--list-packages`, `--list-symbols`, `--find-references`, `--list-diagnostics`
4. Or read `*.sln`, `*.slnx`, `*.csproj`, `Directory.Packages.props`, `packages.lock.json`

MCP tools when the local stdio server is attached: `list_projects`, `list_packages`, `describe_project`, `list_symbols`, `find_references`, `list_diagnostics`.

No write tools. No restore via MCP. No codegen. No rename or refactor. Do not wrap the rest of Roslyn. Never write `/dotnet-solution` as prose to load this skill.
