# Claude Code: generated marketplace branch

Claude clones a generated marketplace branch. The authored `main` branch contains no Claude marketplace file.

The repository is public today. Nothing about the setup changes when it becomes private, except that Claude then needs credentials for it: see [Authentication](#authentication).

## What is generated

A push to `main` publishes the complete installable tree to `marketplace-beta`. Testers install from that branch. When it looks right, a maintainer runs **Promote beta marketplace to stable**, which copies that exact commit onto `marketplace`. The catalog is always generated; an MCP adapter exists only for a plugin that declares real servers:

```
.claude-plugin/marketplace.json                 the plugin catalog (thin name/source list)
plugins/<plugin>/.claude-plugin/plugin.json     Claude component routing
plugins/<plugin>/.mcp.json                      only when that plugin has MCP servers
```

The catalog root uses a top-level `description`. `metadata.description` still parses as a Claude compatibility fallback, but `claude plugin validate` warns when the top-level field is missing. Cursor and Copilot keep description under `metadata` because that is their schema.

The catalog entry points at the completed plugin directory on that same branch. Component paths stay off the catalog: a marketplace `hooks` path or array never runs, and `commands`/`agents` on an entry would append to `.claude-plugin/plugin.json` instead of replacing the default scan.

```json
{
  "name": "squad",
  "displayName": "Squad",
  "source": "./plugins/squad"
}
```

`displayName` is the Claude Code v2.1.143+ picker title. Install still uses kebab-case `name` (`squad@agentpacks`). Copilot's catalog has no display-name field, so it stays kebab-case. `.claude-plugin/plugin.json` inside the plugin points Claude at `com.anthropic.claude-code/` so root `commands/` (Cursor's dialect) is not a second copy.

## Why `.mcp.json` exists

Claude reads MCP configuration from `.mcp.json` in the plugin root. Agent Plugins fixes the portable location at `mcp.json`. Rather than ask Claude to understand the portable file, the tooling derives `.mcp.json` from it, converting `${PLUGIN_ROOT}` to `${CLAUDE_PLUGIN_ROOT}`, `${PLUGIN_DATA}` to `${CLAUDE_PLUGIN_DATA}`, and the `streamable-http` transport to `http`.

`mcp.json` stays the only file anyone authors. Drift between the two is what `generate-claude --check` and the scheduled drift job exist to catch.

## Why `version` is omitted

Claude resolves plugin updates from an explicit `version` first, and only falls back to the Git commit SHA. A version copied onto the catalog or onto `.claude-plugin/plugin.json` and never bumped would leave everyone on cached content after a skill changed. Both generated files omit it. Testers see each merge to `main` on `marketplace-beta`, and stable installs move when that commit is promoted.

## Why the catalog omits component paths

Claude Code's plugin manifest is `.claude-plugin/plugin.json` inside the plugin. `commands` and `agents` there replace the default root scan; a hooks path there is valid. The same fields on a marketplace entry are the wrong layer: a hooks path or array never runs, and with `plugin.json` present the entry's component fields append rather than replace.

Skills still load from default `skills/`. `.mcp.json` still loads from the plugin root. Neither is restated on the catalog or the Claude plugin manifest. Cursor still reads root `commands/`.

## Install

Stable catalog:

```bash
/plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
```

Testers use `#marketplace-beta` instead (remove the stable marketplace first; both catalogs are named `agentpacks`).

```bash
/plugin install squad@agentpacks
```

Update the catalog:

```bash
/plugin marketplace update agentpacks
```

Reload after an update when MCP configuration changed: edits to a `SKILL.md` apply immediately, but MCP needs a reload:

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
