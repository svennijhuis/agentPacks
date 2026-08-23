# agentPacks

Portable [Agent Plugins](https://agent-plugins.org) that wire a workflow into the agent loop.

## Available plugins

Three capability packs, installed because you want a behaviour wired into the agent loop rather than knowledge sitting on a shelf, and one language pack, installed because of the ecosystem you compile in:

| Plugin | Use it for |
| --- | --- |
| `delivery-loop` | Having the main agent mediate turn-based planning, run plan-bound implementation and verification, fan reviewers out in parallel, and merge at most two fix rounds before an uncommitted hand-off |
| `pack-check` | Detecting the repository stack at session start and asking before installing the language pack that supplies its required build and test skills |
| `git` | Blocking the git commands that destroy work an agent cannot get back — `reset --hard`, `clean -fd`, `push --force`, `branch -D`, `checkout .` — before the client runs them |
| `dotnet` | Teaching the loop how C# is built, tested and reviewed, backed by one canonical set of standards distributed only to the skills that need each document |

That is the whole catalog today, deliberately. The remaining role packs (`engineering`, `productivity`, `security`) and language packs (`typescript`, `rust`) are planned and have their own rules for what earns one — a pack that ships nothing but an empty `plugin.json` advertises an install that does nothing, so they are added when there is real content to add. The reasoning is in [`docs/PLAN.md`](docs/PLAN.md).

Adding the marketplace makes all plugins discoverable. Install only the plugins you want. The commands install globally for your user.

## GitHub Copilot CLI

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
copilot plugin install delivery-loop@agentpacks
copilot plugin install pack-check@agentpacks
copilot plugin install git@agentpacks
copilot plugin install dotnet@agentpacks
```

Update later with:

```shell
copilot plugin marketplace update agentpacks
```

## Codex

```shell
codex plugin marketplace add svennijhuis/agentPacks --ref marketplace
codex plugin add delivery-loop@agentpacks
codex plugin add pack-check@agentpacks
codex plugin add git@agentpacks
codex plugin add dotnet@agentpacks
```

Open `/plugins` in Codex to inspect the installed plugins. Update later with:

```shell
codex plugin marketplace upgrade agentpacks
```

## Claude Code

```shell
claude plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace --scope user
claude plugin install delivery-loop@agentpacks --scope user
claude plugin install pack-check@agentpacks --scope user
claude plugin install git@agentpacks --scope user
claude plugin install dotnet@agentpacks --scope user
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
ln -s ~/.cursor/agentPacks/plugins/pack-check ~/.cursor/plugins/local/pack-check
ln -s ~/.cursor/agentPacks/plugins/git ~/.cursor/plugins/local/git
ln -s ~/.cursor/agentPacks/plugins/dotnet ~/.cursor/plugins/local/dotnet
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
| Cursor | yes | yes | always-on and glob-scoped | yes | yes | yes |
| GitHub Copilot | yes | yes | always-on only, at session start | yes | yes | yes |
| Codex | yes | yes | always-on, manual copy | manual copy | — | yes |

Codex loads subagents only from `.codex/agents/` and reads `AGENTS.md` from the workspace rather than from a plugin, so those arrive as generated files you copy once. Glob-scoped rules remain Cursor-only; other clients receive only always-on rules, and validation reports the expected portability warning. The [`delivery-loop`](plugins/delivery-loop/README.md) README has the details.

## Using the plugins

Two slash commands, split by whether the code exists yet:

```
/deliver add an integration test for the orders endpoint
```

The main agent controls the whole loop. It prints the detected stack and standards, relays one
planner question round at a time, launches applicable reviewers in parallel, and ends at a hand-off
with nothing committed. Obvious small changes are implemented and verified directly without phase agents.

```
/review-diff
```

The review phase on its own, for a change that arrived already written: correctness and simplification
in parallel, with security added when needed. No plan means no verdict and no fix round.

Asking naturally works too — “plan this change, then build it”, “is this safe to ship?”, “what can this change drop?” — and selects the same agents.

`git` needs no invocation either, and has nothing to ask: its hook blocks `git reset --hard`, `git clean -fd`, `git push --force`, `git branch -D`, `git checkout .` and `git restore .` before the client runs them, with the reason on stderr. [Its README](plugins/git/README.md) covers the `AGENTPACKS_GIT_GUARD=off` switch and which clients the blocking contract is actually verified on.

`pack-check` runs at session start. In a .NET repository it verifies that `dotnet-build` and
`dotnet-test-patterns` resolve; when either is missing it asks once before installing the `dotnet`
plugin. CLI clients run their own installer after approval, while Cursor uses **Customize**. Reload
after installation so the new skills enter the next session. [Its README](plugins/pack-check/README.md)
has the provider-specific behavior.

`dotnet` needs no invocation. Its skills are loaded when the Loop detects a `.slnx`, `.sln`, or
`.csproj`. [Its README](plugins/dotnet/README.md) explains how its canonical standards stay inside
the plugin while agents match the repository they are working in.

Installed skills are selected when relevant to your request.

## Coding standards

Standards ship entirely inside language plugins. Each pack keeps canonical Markdown under its own
`standards/` directory and declares which skills consume each document in `standards.source.json`.
Generation copies those references only to the `marketplace` branch or temporary output; generated
files do not live on `main`.

The planner records those plugin sources under `## Standards in force`. It separately inspects
project configuration, directory layout, tests, and nearby code, then records concrete evidence
under `## Repository conventions observed`. That lets every phase match the repository without
creating or advertising another standards location on the repository or the developer's machine.

[`docs/ADD-LANGUAGE-PACK.md`](docs/ADD-LANGUAGE-PACK.md) has the full contract.

Contributor and architecture documentation lives in [`docs/`](docs/).
