# Claude Code — private company repository

Nothing here is published publicly. The private GitHub repository acts as the plugin source, and Claude reads it with your normal Git credentials.

## What is generated

Two files, both produced by the .NET tooling and committed by GitHub Actions:

```
.claude-plugin/marketplace.json          the private catalog
plugins/company-engineering/.mcp.json    MCP config in Claude's own file name
```

There is no second copy of the skills or agents. The catalog entry points at the real plugin directory:

```json
{
  "name": "company-engineering",
  "source": "./plugins/company-engineering",
  "skills": "./skills/",
  "agents": "./agents/",
  "mcpServers": "./.mcp.json",
  "strict": false
}
```

## Why `.mcp.json` exists

Claude reads MCP configuration from `.mcp.json` in the plugin root. Agent Plugins fixes the portable location at `mcp.json`. Rather than ask Claude to understand the portable file, the tooling derives `.mcp.json` from it, converting `${PLUGIN_ROOT}` to `${CLAUDE_PLUGIN_ROOT}`, `${PLUGIN_DATA}` to `${CLAUDE_PLUGIN_DATA}`, and the `streamable-http` transport to `http`.

`mcp.json` stays the only file anyone authors. Drift between the two is what `generate-claude --check` and the scheduled drift job exist to catch.

## Why `version` is omitted

Claude resolves plugin updates from an explicit `version` first, and only falls back to the Git commit SHA. A version copied from `plugin.json` and never bumped would leave everyone on cached content after a skill changed. Omitting it makes every merge to `main` visible.

## Why `strict: false`

With `strict: false` the marketplace entry is the authority for component definitions. Claude reports a conflict if the plugin's own manifest *also* declares components — our root `plugin.json` is an Agent Plugins manifest with no component fields, so there is nothing to conflict with.

If that ever changes, the fallback is `strict: true` and letting Claude discover `skills/`, `agents/` and `.mcp.json` by their default locations.

## Install

```bash
/plugin marketplace add YOUR-ORG/agentPacks
```

```bash
/plugin install company-engineering@agentpacks
```

Update the catalog:

```bash
/plugin marketplace update agentpacks
```

Reload after an update when a non-skill component changed — edits to a `SKILL.md` apply immediately, but agents and MCP need a reload:

```bash
/reload-plugins
```

## Authentication

For manual install and update, Claude Code uses normal Git credential helpers:

```bash
gh auth login
```

For unattended updates, use organization-approved credential management. Do not store GitHub tokens in this repository.

## Optional: advertise the source from a product repository

A product repository can point Claude at agentPacks automatically:

```json
{
  "extraKnownMarketplaces": {
    "agentpacks": {
      "source": { "source": "github", "repo": "YOUR-ORG/agentPacks" }
    }
  },
  "enabledPlugins": {
    "company-engineering@agentpacks": true
  }
}
```

Add this only when a team wants it.
