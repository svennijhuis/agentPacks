# agentPacks

Portable [Agent Plugins](https://agent-plugins.org) for engineering, code review, and testing.

## Available plugins

| Plugin | Use it for |
| --- | --- |
| `engineering` | Architecture, debugging, domain modeling, planning, and technical writing skills |
| `review` | .NET and general code-review skills |
| `testing` | Practical test-writing guidance |

Adding the marketplace makes all plugins discoverable. Install only the plugins you want. The commands below install them globally for your user.

## GitHub Copilot CLI

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#distribution
copilot plugin install engineering@agentpacks
copilot plugin install review@agentpacks
copilot plugin install testing@agentpacks
```

Update later with:

```shell
copilot plugin marketplace update agentpacks
```

## Codex

```shell
codex plugin marketplace add svennijhuis/agentPacks --ref distribution
codex plugin add engineering@agentpacks
codex plugin add review@agentpacks
codex plugin add testing@agentpacks
```

Open `/plugins` in Codex to inspect the installed plugins. Update later with:

```shell
codex plugin marketplace upgrade agentpacks
```

## Claude Code

```shell
claude plugin marketplace add https://github.com/svennijhuis/agentPacks.git#distribution --scope user
claude plugin install engineering@agentpacks --scope user
claude plugin install review@agentpacks --scope user
claude plugin install testing@agentpacks --scope user
```

Update later with:

```shell
claude plugin marketplace update agentpacks
```

## Cursor

Cursor supports the Agent Plugins standard. Until this repository is listed in a Cursor marketplace, install the plugins through Cursor's supported local plugin directory:

```shell
git clone --branch distribution --single-branch https://github.com/svennijhuis/agentPacks.git ~/.cursor/agentPacks
mkdir -p ~/.cursor/plugins/local
ln -s ~/.cursor/agentPacks/plugins/engineering ~/.cursor/plugins/local/engineering
ln -s ~/.cursor/agentPacks/plugins/review ~/.cursor/plugins/local/review
ln -s ~/.cursor/agentPacks/plugins/testing ~/.cursor/plugins/local/testing
```

Create only the links for the plugins you want, then restart Cursor or run **Developer: Reload Window**. Update later with:

```shell
git -C ~/.cursor/agentPacks pull --ff-only
```

Teams and Enterprise administrators can instead import this repository's `distribution` branch as a team marketplace; users can then install plugins from **Customize**.

## Using the plugins

Start a new agent session after installation and ask naturally, for example:

- “Review this .NET diff.”
- “Improve these tests.”
- “Diagnose this bug.”

Installed skills are selected when relevant to your request.

Contributor and architecture documentation lives in [`docs/`](docs/).
