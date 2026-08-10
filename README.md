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

## Install

Install from the generated `distribution` branch. It contains the completed portable plugins, including materialized external URL skills and Claude's generated marketplace. Choose the plugins from the table above that you need.

Adding the marketplace registers all three available plugins; it does not install them automatically. Install each wanted plugin by its `plugin@marketplace` name.

> Why the full URL? Copilot treats `OWNER/REPO` as shorthand for a GitHub.com repository's default branch. This repository keeps authored source on the default `main` branch and publishes the marketplace on `distribution`, so `https://github.com/svennijhuis/agentPacks.git#distribution` is required. Plain `svennijhuis/agentPacks` would inspect the wrong branch.

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

## Development

Want to add or maintain a plugin? See [Development](docs/DEVELOPMENT.md). Additional design and source references live under [`docs/`](docs/).
