# References

## Agent Plugins

- Overview — https://agent-plugins.org/
- Specification — https://agent-plugins.org/specification
- Plugin authors — https://agent-plugins.org/plugin-authors
- Compatible clients — https://agent-plugins.org/compatible-clients
- Schemas — https://agent-plugins.org/schemas
- Specification repository — https://github.com/agentplugins/agent-plugins-spec
- Announcement — https://vercel.com/blog/introducing-agent-plugins

Validation uses the canonical published schemas directly:

- https://agent-plugins.org/schemas/1.0.0/plugin.schema.json
- https://agent-plugins.org/schemas/1.0.0/mcp.schema.json

## Agent Skills

- Overview — https://agentskills.io/
- Specification — https://agentskills.io/specification
- Reference validator — https://github.com/agentskills/agentskills/tree/main/skills-ref

There is no JSON Schema for skills. The normative frontmatter table in the specification is implemented by hand in `SkillValidator`.

## GitHub Copilot

- Creating plugins — https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-creating
- Finding and installing — https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/plugins-finding-installing
- Plugin reference — https://docs.github.com/en/copilot/reference/copilot-cli-reference/cli-plugin-reference
- Custom agents — https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/create-custom-agents-for-cli
- MCP servers — https://docs.github.com/en/copilot/how-tos/copilot-cli/customize-copilot/add-mcp-servers

## Cursor

- Plugins — https://cursor.com/docs/plugins
- Marketplace — https://cursor.com/blog/marketplace

## OpenAI Codex

- Plugin examples — https://github.com/openai/plugins
- Developer docs — https://developers.openai.com/

## Claude Code

- Plugins — https://code.claude.com/docs/en/plugins
- Plugin reference — https://code.claude.com/docs/en/plugins-reference
- Marketplaces — https://code.claude.com/docs/en/plugin-marketplaces
- Discover and install — https://code.claude.com/docs/en/discover-plugins
- MCP — https://code.claude.com/docs/en/mcp

## Building MCP servers in .NET

- MCP C# SDK v2.0 announcement — https://devblogs.microsoft.com/dotnet/announcing-v20-of-the-official-mcp-csharp-sdk/

The official C# SDK is the natural way to build the company MCP servers this repository will point at. v2.0 is production-ready, implements the 2026-07-28 protocol revision, and is backward compatible with v1 apart from the experimental Tasks extension.

| Package | Use |
|---|---|
| `ModelContextProtocol.Core` | Client and low-level server |
| `ModelContextProtocol` | Stdio server and attribute-based tool discovery |
| `ModelContextProtocol.AspNetCore` | HTTP server transport |
| `ModelContextProtocol.Extensions.Tasks` | Long-running tools |
| `ModelContextProtocol.Extensions.Apps` | Interactive UI delivery |

Two v2.0 changes matter for how we declare servers in `mcp.json`:

- **Stateless by default.** No `initialize` handshake or `Mcp-Session-Id` header is required, so a server can scale horizontally without sticky sessions. Combined with standardized `Mcp-Method` / `Mcp-Name` / `Mcp-Param-*` headers, ordinary HTTP infrastructure can route MCP traffic without inspecting payloads.
- **Multi Round-Trip Requests.** A tool can ask for user input mid-execution by returning `InputRequiredResult`, so interactivity no longer implies a persistent session.

Both point the same way as our own policy: prefer `streamable-http` over the deprecated `sse` transport, and keep authentication with the client rather than in `mcp.json`. Targets net8.0 through net10.0 and netstandard2.0.

## Examples worth reading

- Microsoft Agent Skills — https://github.com/MicrosoftDocs/Agent-Skills
- Microsoft Power Platform Skills — https://github.com/microsoft/power-platform-skills
- OpenAI Plugins — https://github.com/openai/plugins

These show the same pattern: keep reusable skills central, add only the client-specific packaging you actually need.
