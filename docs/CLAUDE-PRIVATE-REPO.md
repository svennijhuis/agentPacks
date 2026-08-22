# Claude Code — generated marketplace branch

Claude clones the repository's generated `marketplace` branch. The authored `main` branch contains no Claude marketplace file.

The repository is public today. Nothing about the setup changes when it becomes private, except that Claude then needs credentials for it — see [Authentication](#authentication).

## What is generated

GitHub Actions creates the complete installable tree on `marketplace`. The catalog is always generated; an MCP adapter exists only for a plugin that declares real servers:

```
.claude-plugin/marketplace.json          the plugin catalog
plugins/<plugin>/.mcp.json               only when that plugin has MCP servers
```

The catalog entry points at the completed plugin directory on that same branch:

```json
{
  "name": "delivery-loop",
  "source": "./plugins/delivery-loop",
  "skills": "./skills/",
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

If that ever changes, the fallback is `strict: true` and letting Claude discover `skills/` and `.mcp.json` by their default locations.

## Install

```bash
/plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
```

```bash
/plugin install code-review@agentpacks
/plugin install delivery-loop@agentpacks
```

Update the catalog:

```bash
/plugin marketplace update agentpacks
```

Reload after an update when MCP configuration changed — edits to a `SKILL.md` apply immediately, but MCP needs a reload:

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
      "source": {
        "source": "url",
        "url": "https://github.com/svennijhuis/agentPacks.git",
        "ref": "marketplace"
      }
    }
  },
  "enabledPlugins": {
    "delivery-loop@agentpacks": true
  }
}
```

Add this only when a team wants it.
