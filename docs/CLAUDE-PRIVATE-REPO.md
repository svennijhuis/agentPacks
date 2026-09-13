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
  "name": "squad",
  "source": "./plugins/squad",
  "skills": "./skills/",
  "strict": true
}
```

## Why `.mcp.json` exists

Claude reads MCP configuration from `.mcp.json` in the plugin root. Agent Plugins fixes the portable location at `mcp.json`. Rather than ask Claude to understand the portable file, the tooling derives `.mcp.json` from it, converting `${PLUGIN_ROOT}` to `${CLAUDE_PLUGIN_ROOT}`, `${PLUGIN_DATA}` to `${CLAUDE_PLUGIN_DATA}`, and the `streamable-http` transport to `http`.

`mcp.json` stays the only file anyone authors. Drift between the two is what `generate-claude --check` and the scheduled drift job exist to catch.

## Why `version` is omitted

Claude resolves plugin updates from an explicit `version` first, and only falls back to the Git commit SHA. A version copied from `plugin.json` and never bumped would leave everyone on cached content after a skill changed. Omitting it makes every merge to `main` visible.

## Why `strict: true`

Claude auto-discovers root `commands/` (Cursor's dialect) unless the marketplace entry is strict. The catalog already points at `com.anthropic.claude-code/commands/`, so `strict: false` loads both trees and `/squad` plus `/squad-review` appear twice.

`strict: true` keeps Claude on the declared paths. Cursor still reads root `commands/`. Skills and `.mcp.json` stay explicitly declared; a plugin.json version bump is not how Claude picks up this change (the catalog omits `version` and updates from the commit SHA).

## Install

```bash
/plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
```

```bash
/plugin install squad@agentpacks
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
        "source": "github",
        "repo": "svennijhuis/agentPacks",
        "ref": "marketplace"
      },
      "autoUpdate": true
    }
  },
  "enabledPlugins": {
    "squad@agentpacks": true
  }
}
```

`url` is a direct `marketplace.json` fetch and does not take `ref`. Use `github` + `repo` + `ref` so Claude clones the `marketplace` branch. `autoUpdate` is optional; third-party marketplaces default to off.

Add this only when a team wants it.
