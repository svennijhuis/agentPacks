# agentPacks

Portable [Agent Plugins](https://agent-plugins.org) that wire a workflow into the agent loop.

## Available plugins

One capability pack, installed because you want the workflow in the loop rather than knowledge sitting on a shelf:

| Plugin | Use it for |
| --- | --- |
| `delivery-loop` | Running a change through plan, implement, verify and review as separate roles, with an OWASP security gate, a bounded fix round, and a hand-off that stops short of committing |

That is the whole catalog today, deliberately. Role packs (`engineering`, `productivity`, `security`) and language packs (`dotnet`, `typescript`, `rust`) are planned and have their own rules for what earns one — a pack that ships nothing but an empty `plugin.json` advertises an install that does nothing, so they are added when there is real content to add. The reasoning is in [`docs/PLAN.md`](docs/PLAN.md).

Adding the marketplace makes all plugins discoverable. Install only the plugins you want. The commands install globally for your user.

## GitHub Copilot CLI

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
copilot plugin install delivery-loop@agentpacks
```

Update later with:

```shell
copilot plugin marketplace update agentpacks
```

## Codex

```shell
codex plugin marketplace add svennijhuis/agentPacks --ref marketplace
codex plugin add delivery-loop@agentpacks
```

Open `/plugins` in Codex to inspect the installed plugins. Update later with:

```shell
codex plugin marketplace upgrade agentpacks
```

## Claude Code

```shell
claude plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace --scope user
claude plugin install delivery-loop@agentpacks --scope user
```

Update later with:

```shell
claude plugin marketplace update agentpacks
```

## Cursor

Cursor supports the Agent Plugins standard. Until this repository is listed in a Cursor marketplace, install the plugins through Cursor's supported local plugin directory:

```shell
git clone --branch marketplace --single-branch https://github.com/svennijhuis/agentPacks.git ~/.cursor/agentPacks
mkdir -p ~/.cursor/plugins/local
ln -s ~/.cursor/agentPacks/plugins/delivery-loop ~/.cursor/plugins/local/delivery-loop
```

Create only the links for the plugins you want, then restart Cursor or run **Developer: Reload Window**. Update later with:

```shell
git -C ~/.cursor/agentPacks pull --ff-only
```

Teams and Enterprise administrators can instead import this repository's `marketplace` branch as a team marketplace; users can then install plugins from **Customize**.

## What each client gets

Skills and MCP servers are portable: every client loads them from the same files. Rules, subagents, commands and hooks are not — the Agent Plugins standard leaves all four out as too client-specific — so this repository authors them once and generates a tree per client.

| | Skills | MCP | Rules | Agents | Commands | Hooks |
| --- | --- | --- | --- | --- | --- | --- |
| Claude Code | yes | yes | always-on only, at session start | yes | yes | yes |
| Cursor | yes | yes | yes | yes | yes | yes |
| GitHub Copilot | yes | yes | yes | yes | yes | yes |
| Codex | yes | yes | manual copy | manual copy | — | yes |

Codex loads subagents only from `.codex/agents/` and reads `AGENTS.md` from the workspace rather than from a plugin, so those two arrive as generated files you copy once. The [`delivery-loop`](plugins/delivery-loop/README.md) README has the commands.

## Using the plugins

Start a new agent session after installation and ask naturally, for example:

- “Review this diff.” — or the `/review-diff` command, for a change that arrived with no plan.
- “Plan this change, then build it.” — `delivery-loop` splits it into plan, implement, verify and review, runs the security gate when the change touches a trust boundary, and hands the result back uncommitted.
- “Is this change safe to ship?” — the OWASP gate, as its own verdict.

Installed skills are selected when relevant to your request.

Contributor and architecture documentation lives in [`docs/`](docs/).
