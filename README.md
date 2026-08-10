# agentPacks

Repository for sharing Agent Plugins: skills, MCP integrations and namespaced client extensions.

Currently a public repository. Everything here works the same way once it moves into a private company org — the only difference is that clients then authenticate with normal Git credentials.

The portable content follows the open [Agent Plugins v1](https://agent-plugins.org/) specification, so each plugin works across ChatGPT/Codex, Cursor, GitHub Copilot, Kiro and VS Code without per-client packaging. Claude Code is handled by a small generated compatibility layer that points at the same directories.

## Available plugins

| Plugin | Purpose | Included |
| --- | --- | --- |
| `engineering` | Architecture, debugging and productivity | 10 pinned URL skills; MCP and extension examples |
| `review` | General and .NET code review | 1 local skill; 1 pinned URL skill |
| `testing` | Automated-testing guidance | 1 local skill |

Plugins are installation boundaries. Add another one, such as `security`, only when there is real content that users should be able to install independently; do not create empty catalog entries as examples.

## What you edit

```
plugins/<plugin>/plugin.json                manifest
plugins/<plugin>/skills/                    authored Markdown skills
plugins/<plugin>/external-skills.json       URL imports owned by this plugin
plugins/<plugin>/mcp.json                   portable MCP configuration
plugins/<plugin>/<reverse-domain>/          optional client-owned extension files
```

## What GitHub generates

```
distribution:.claude-plugin/marketplace.json
distribution:plugins/*/.mcp.json
distribution:plugins/*/skills/<external-name>/
```

These exist only on the generated `distribution` branch. `main` contains no Claude marketplace and no copied external Markdown. Update the owning plugin's `external-skills.json`; GitHub Actions builds and publishes the installable branch.

## Who reads what

| Component | Standard clients | Claude Code |
| --- | --- | --- |
| Skills | Read `skills/` | Read through generated catalog |
| MCP | Read `mcp.json` | Read through generated catalog |

Agent Plugins v1 standardizes exactly two portable component types: skills and MCP. Commands, hooks, agents, rules and LSP servers remain client-specific. When a client needs extra manifest data or files, use the standard `extensions` object and a matching reverse-domain directory; clients that do not own that namespace ignore it.

## Add a skill

Create `plugins/<plugin>/skills/<name>/SKILL.md` in the plugin that owns the capability:

```markdown
---
name: my-skill
description: What this does and when an agent should use it.
---

# My skill

Instructions for the agent.
```

The frontmatter `name` must equal the directory name, use lowercase letters, digits and single hyphens, and stay under 64 characters. Skill names may not contain periods, even though plugin names may. The description is required and capped at 1024 characters.

Then validate and open a pull request. See [docs/ADD-SKILL.md](docs/ADD-SKILL.md).

## Add an MCP server

Edit `plugins/<plugin>/mcp.json` to add a server. An empty `mcpServers` object is a valid scaffold and generates no client MCP configuration until a real server is added. Prefer read-only servers, use HTTPS for anything non-loopback, and keep credentials out: plugin files are literal, visible package data, and authentication is client-managed. See [docs/ADD-MCP.md](docs/ADD-MCP.md).

## Add a client extension

Add manifest data under `plugin.json` → `extensions` and/or add a top-level directory whose name exactly matches the reverse-domain namespace, for example `com.example.client/hooks/hooks.json`. Agent Plugins defines the namespace boundary, not the contents; use the owning client's documentation for fields and validation. See [docs/ADD-CLIENT-EXTENSION.md](docs/ADD-CLIENT-EXTENSION.md).

For an external skill, record its URL, directory and exact commit in `plugins/<plugin>/external-skills.json`. Ownership is visible from the path and no separate plugin selector is needed. Nothing is copied by a contributor. After merge, GitHub Actions fetches it into that plugin's portable `skills/` tree and generates Claude's marketplace from the completed package. See [docs/EXTERNAL-SKILLS.md](docs/EXTERNAL-SKILLS.md).

The validator reports every finding in one run, tagged by severity: `spec` for something a client would reject, `spec(tolerated)` for a violation clients ignore but we do not, `policy` for our own rules, and `warning` for advice that does not fail the build.

Validation fetches the canonical `$schema` URLs from `agent-plugins.org`; schema snapshots are not copied into this repository.

## Install

Install from the generated `distribution` branch. It contains the completed portable plugins, including materialized external URL skills and Claude's generated marketplace. Choose the plugins from the table above that you need.

Adding the marketplace registers all available plugins but does not install them automatically. Install `engineering`, `review`, and/or `testing` separately.

> Copilot treats `OWNER/REPO` as shorthand for a GitHub.com repository's default branch. This repository publishes its marketplace on `distribution`, not the default `main` branch, so use the full URL ending in `#distribution`. Plain `svennijhuis/agentPacks` would inspect the wrong branch.

| Scope | GitHub Copilot | Claude Code |
| --- | --- | --- |
| User/global | CLI or `~/.copilot/settings.json` | `--scope user` |
| Repository/team | `.github/copilot/settings.json` | `--scope project` |

### GitHub Copilot CLI — global for your user

This is the normal installation: the marketplace and installed plugins are available in every repository opened with your Copilot CLI user profile.

From a terminal:

```bash
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#distribution
copilot plugin install engineering@agentpacks
copilot plugin install review@agentpacks
copilot plugin install testing@agentpacks
```

The same flow from inside an interactive Copilot CLI session:

```text
/plugin marketplace add https://github.com/svennijhuis/agentPacks.git#distribution
/plugin install engineering@agentpacks
/plugin install review@agentpacks
/plugin install testing@agentpacks
```

To configure the same global installation declaratively, add this to `~/.copilot/settings.json`:

```json
{
  "extraKnownMarketplaces": {
    "agentpacks": {
      "source": {
        "source": "github",
        "repo": "svennijhuis/agentPacks",
        "ref": "distribution"
      }
    }
  },
  "enabledPlugins": {
    "engineering@agentpacks": true,
    "review@agentpacks": true,
    "testing@agentpacks": true
  }
}
```

### GitHub Copilot — shared by one repository

Commit the same JSON to `.github/copilot/settings.json` in the consuming repository. Copilot CLI and Copilot cloud agent both read its `extraKnownMarketplaces` and `enabledPlugins` entries. This is the team/repository installation; it is not global.

### Claude Code — global for your user

Use the terminal commands with explicit user scope:

```bash
claude plugin marketplace add https://github.com/svennijhuis/agentPacks.git#distribution --scope user
claude plugin install engineering@agentpacks --scope user
claude plugin install review@agentpacks --scope user
claude plugin install testing@agentpacks --scope user
```

Inside Claude Code, the equivalent slash commands install at user scope by default:

```text
/plugin marketplace add https://github.com/svennijhuis/agentPacks.git#distribution
/plugin install engineering@agentpacks
/plugin install review@agentpacks
/plugin install testing@agentpacks
```

### Claude Code — shared by one repository

Use `--scope project` instead of `--scope user`, or commit this `.claude/settings.json` in the consuming repository:

```json
{
  "extraKnownMarketplaces": {
    "agentpacks": {
      "source": {
        "source": "url",
        "url": "https://github.com/svennijhuis/agentPacks.git",
        "ref": "distribution"
      }
    }
  },
  "enabledPlugins": {
    "engineering@agentpacks": true,
    "review@agentpacks": true,
    "testing@agentpacks": true
  }
}
```

Use `--scope local` only for an uncommitted installation in the current repository.

Refresh with `copilot plugin marketplace update agentpacks` or `claude plugin marketplace update agentpacks`. Once the repository is private, normal Git credentials apply. See [docs/CLAUDE-PRIVATE-REPO.md](docs/CLAUDE-PRIVATE-REPO.md).

### Other Agent Plugins clients

Point the client at `https://github.com/svennijhuis/agentPacks.git#distribution` using its Agent Plugins installation flow. Do not install from `main`: that branch intentionally contains URL records rather than copied external skills.

## Versioning and updates

The generated marketplace entry deliberately omits `version`. Claude resolves updates from an explicit version before falling back to the Git commit SHA, so a version that is never bumped would keep everyone pinned to cached content even after a skill changes. Leaving it out means every merge to `main` is picked up. The same expectation applies to any other marketplace we publish to: either omit the version, or bump it on every change to installable content.

## Continuous integration

Pull requests run the tests, validate the source, and prove that generation succeeds — into a temporary directory, so the working tree stays clean. A stale committed catalog does not fail a pull request.

After a merge to `main`, a separate job materializes external URL sources, generates Claude compatibility, and publishes the complete result to `distribution`. It never commits generated files back to `main`. Protect `main` normally; allow GitHub Actions to create and update `distribution`.

## Scope

First proof of concept: one repository with focused installable plugins, shared skills, read-only MCP first, external skills only when reviewed and pinned, C# validation, GitHub Actions, and a generated Claude catalog. GitHub Agentic Workflows stay in the application repositories that consume these capabilities — see [docs/GITHUB-AGENTIC-WORKFLOWS.md](docs/GITHUB-AGENTIC-WORKFLOWS.md).

Avoid building an internal framework until the standards settle and teams have proven they need more.

Reference links: [docs/REFERENCES.md](docs/REFERENCES.md).
