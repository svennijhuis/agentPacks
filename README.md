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
ln -s ~/.cursor/agentPacks/plugins/dotnet ~/.cursor/plugins/local/dotnet
ln -s ~/.cursor/agentPacks/plugins/engineering ~/.cursor/plugins/local/engineering
ln -s ~/.cursor/agentPacks/plugins/productivity ~/.cursor/plugins/local/productivity
```

Create only the links for the plugins you want, then restart Cursor or run **Developer: Reload Window**. Update later with:

```shell
git -C ~/.cursor/agentPacks pull --ff-only
```

Teams and Enterprise administrators can instead import this repository's `marketplace` branch as a team marketplace; users can then install plugins from **Customize**.

## Using the plugins

Start a new agent session after installation and ask naturally, for example:

- “Review this .NET diff.”
- “Improve these tests.”
- “Diagnose this bug.”

Installed skills are selected when relevant to your request.

Contributor and architecture documentation lives in [`docs/`](docs/).
