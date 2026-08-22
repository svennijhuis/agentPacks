# agentPacks

Portable [Agent Plugins](https://agent-plugins.org) for engineering, product work, and the language you actually ship in.

## Available plugins

Role packs, installed because of how you work:

| Plugin | Use it for |
| --- | --- |
| `productivity` | Sharpening plans, writing documents agents consume, compact responses |
| `product` | User stories, requirements, notes to spec *(empty for now)* |
| `migrations` | Platform moves such as Azure DevOps to GitHub *(empty for now)* |
| `engineering` | Cross-language craft: domain modeling, codebase design, debugging, review, triage, tickets, testing |
| `security` | Threat modeling and general secure-design checklists *(empty for now)* |

Capability packs, installed because you want the workflow wired into the agent loop:

| Plugin | Use it for |
| --- | --- |
| `code-review` | Reviewing changes: a review skill, always-on standards, security and diff subagents, a `/review-diff` command, and a guard hook |
| `delivery-loop` | Running a change through plan, implement, verify and review as separate roles, with a bounded fix round and a hand-off that stops short of committing |

Language packs, installed because of the ecosystem you live in:

| Plugin | Use it for |
| --- | --- |
| `dotnet` | C# and .NET: building, reviewing, testing, .NET-specific security |
| `typescript` | TypeScript frontend and backend, including React and NestJS *(empty for now)* |
| `rust` | Rust building, reviewing and testing, including Axum *(empty for now)* |

A .NET shop installs `dotnet + engineering + security + productivity` and never sees Rust skills. Frameworks are skills inside a language pack, never plugins of their own — the reasoning is in [`docs/PLAN.md`](docs/PLAN.md).

Adding the marketplace makes all plugins discoverable. Install only the plugins you want; the examples below use a .NET setup, so substitute your own names. The commands install globally for your user.

## GitHub Copilot CLI

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
copilot plugin install code-review@agentpacks
copilot plugin install delivery-loop@agentpacks
copilot plugin install dotnet@agentpacks
copilot plugin install engineering@agentpacks
copilot plugin install productivity@agentpacks
```

Update later with:

```shell
copilot plugin marketplace update agentpacks
```

## Codex

```shell
codex plugin marketplace add svennijhuis/agentPacks --ref marketplace
codex plugin add code-review@agentpacks
codex plugin add delivery-loop@agentpacks
codex plugin add dotnet@agentpacks
codex plugin add engineering@agentpacks
codex plugin add productivity@agentpacks
```

Open `/plugins` in Codex to inspect the installed plugins. Update later with:

```shell
codex plugin marketplace upgrade agentpacks
```

## Claude Code

```shell
claude plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace --scope user
claude plugin install code-review@agentpacks --scope user
claude plugin install delivery-loop@agentpacks --scope user
claude plugin install dotnet@agentpacks --scope user
claude plugin install engineering@agentpacks --scope user
claude plugin install productivity@agentpacks --scope user
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
ln -s ~/.cursor/agentPacks/plugins/code-review ~/.cursor/plugins/local/code-review
ln -s ~/.cursor/agentPacks/plugins/delivery-loop ~/.cursor/plugins/local/delivery-loop
ln -s ~/.cursor/agentPacks/plugins/dotnet ~/.cursor/plugins/local/dotnet
ln -s ~/.cursor/agentPacks/plugins/engineering ~/.cursor/plugins/local/engineering
ln -s ~/.cursor/agentPacks/plugins/productivity ~/.cursor/plugins/local/productivity
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

Codex loads subagents only from `.codex/agents/` and reads `AGENTS.md` from the workspace rather than from a plugin, so those two arrive as generated files you copy once. The [`code-review`](plugins/code-review/README.md) and [`delivery-loop`](plugins/delivery-loop/README.md) READMEs have the commands.

## Using the plugins

Start a new agent session after installation and ask naturally, for example:

- “Review this diff.” — or `/review-diff` with the `code-review` pack installed.
- “Plan this change, then build it.” — the `delivery-loop` pack splits it into plan, implement, verify and review, and hands the result back uncommitted.
- “Review this .NET diff.”
- “Improve these tests.”
- “Diagnose this bug.”

Installed skills are selected when relevant to your request.

Contributor and architecture documentation lives in [`docs/`](docs/).
