# References

## Agent Plugins

- Overview — https://agent-plugins.org/
- Specification — https://agent-plugins.org/specification
- Plugin authors — https://agent-plugins.org/plugin-authors
- Compatible clients — https://agent-plugins.org/compatible-clients
- Schemas — https://agent-plugins.org/schemas
- Specification repository — https://github.com/agentplugins/agent-plugins-spec
- Announcement — https://vercel.com/blog/introducing-agent-plugins

Only two schemas exist, and both are vendored under `schemas/`:

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

## Examples worth reading

- Microsoft Agent Skills — https://github.com/MicrosoftDocs/Agent-Skills
- Microsoft Power Platform Skills — https://github.com/microsoft/power-platform-skills
- OpenAI Plugins — https://github.com/openai/plugins

These show the same pattern: keep reusable skills central, add only the client-specific packaging you actually need.
