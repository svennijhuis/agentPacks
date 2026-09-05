---
name: dotnet-solution
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. Read-only view of a .NET solution, projects, and package graph. Use MCP tools list_projects, list_packages, describe_project. Never write, restore, or generate code through MCP.
license: UNLICENSED
metadata:
  audience: loop
---

# .NET solution (read-only)

Open the repo with files first, then the `dotnet-solution` MCP server. Three tools only:

| Tool | Returns |
|---|---|
| `list_projects` | Projects in the `.sln` / `.slnx` |
| `list_packages` | Package ids (and CPM versions when present) |
| `describe_project` | One project plus its packages |

No write tools. No restore. No Roslyn codegen. No credentials.

If the MCP server is not running, read the same files yourself: `*.sln`, `*.slnx`, `*.csproj`,
`Directory.Packages.props`, `packages.lock.json`. Do not invent a fourth tool.

Never write `/dotnet-solution` as prose to load this skill.
