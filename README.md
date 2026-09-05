# agentPacks

Portable [Agent Plugins](https://agent-plugins.org) that wire a workflow into the agent loop.

## Available plugins

Three capability packs, installed because you want a behaviour wired into the agent loop rather than knowledge sitting on a shelf, and two language packs, installed because of the ecosystem you compile in:

| Plugin | Use it for |
| --- | --- |
| `delivery-loop` | User-invoked `/build` and `/review`: the main agent mediates grill-style planning, runs plan-bound implementation and verification, fans dual-axis reviewers, and merges at most two fix rounds before an uncommitted hand-off |
| `pack-check` | Detecting the repository stack at session start and asking before installing the language pack that supplies its required build and test skills |
| `git` | Blocking the git commands that destroy work an agent cannot get back — `reset --hard`, `clean -fd`, `push --force`, `branch -D`, `checkout .` — before the client runs them |
| `dotnet` | Teaching the loop how C# is built, tested and reviewed, backed by one canonical set of standards distributed only to the skills that need each document |
| `rust` | Teaching the loop how Cargo workspaces are built, tested and reviewed, backed by canonical Rust, error/concurrency, and testing standards |

That is the whole catalog today, deliberately. The remaining role packs (`engineering`, `productivity`, `security`) and language pack (`typescript`) are planned and have their own rules for what earns one — a pack that ships nothing but an empty `plugin.json` advertises an install that does nothing, so they are added when there is real content to add. The reasoning is in [`docs/PLAN.md`](docs/PLAN.md).

Adding the marketplace makes all plugins discoverable. Install only the plugins you want. The commands install globally for your user.

## GitHub Copilot CLI

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
copilot plugin install delivery-loop@agentpacks
copilot plugin install pack-check@agentpacks
copilot plugin install git@agentpacks
copilot plugin install dotnet@agentpacks
copilot plugin install rust@agentpacks
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
codex plugin add rust@agentpacks
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
claude plugin install rust@agentpacks --scope user
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
ln -s ~/.cursor/agentPacks/plugins/rust ~/.cursor/plugins/local/rust
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

Two user-invoked entrypoints. The model does not pick the orchestrator. `/squad` and `/build` are
the same command.

### `/squad` or `/build`

```
/squad add an integration test for the orders endpoint
/build add an integration test for the orders endpoint
```

1. Read `docs/learnings.md` and **apply** the latest `/build`/`/squad` entry: keep a passed skip and
   tier; a failed skip becomes a must-run. Do not rewrite skills.
2. Orient the codebase (applicable stacks only).
3. Small change? Main agent only — spawn nobody — verify — append learnings — hand off uncommitted.
4. Else grill/plan rounds (facts via the planner; decisions = human) → write the confirmed plan.
5. Gate spins: implementer → verifier → dual-axis reviewers in parallel (correctness + plan/spec;
   security ONLY if a trust boundary, or learnings mark it must-run).
6. Merge; at most two fix rounds on a fresh implementer; hand off uncommitted; append learnings.

### `/review`

```
/review
/review --uncommitted
/review --pr
/review --base main
```

1. Apply the latest `/review` learnings entry.
2. Pin the diff: PR, uncommitted, or versus main.
3. Same gated dual-axis reviewers. No plan, no verdict, no fix loop.
4. Append learnings.

Language-pack slots (`dotnet-build`, `rust-review`, …) are loop internals, loaded by exact Skill
tool name. They are not a second public skill surface.

`git` needs no invocation either, and has nothing to ask: its hook blocks `git reset --hard`, `git clean -fd`, `git push --force`, `git branch -D`, `git checkout .` and `git restore .` before the client runs them, with the reason on stderr. [Its README](plugins/git/README.md) covers the `AGENTPACKS_GIT_GUARD=off` switch and which clients the blocking contract is actually verified on.

`pack-check` runs at session start. It maps .NET markers to `dotnet` and `Cargo.toml` to `rust`, then
verifies the required build and test slots for the stacks applicable to the change. A mixed change
loads both; an unrelated stack in the same repository does not trigger an install. CLI clients run
their own installer after one grouped approval, while Cursor uses **Customize**. Reload after
installation so the new skills enter the next session. [Its README](plugins/pack-check/README.md)
has the provider-specific behavior.

`dotnet` needs no invocation. Its skills are loaded when the Loop detects a `.slnx`, `.sln`, or
`.csproj`. [Its README](plugins/dotnet/README.md) explains how its canonical standards stay inside
the plugin while agents match the repository they are working in.

`rust` needs no invocation. Its skills are loaded for an applicable Cargo workspace. In a mixed
repository, the Loop uses Rust, .NET, or both from the target paths, diff, and acceptance criteria.
[Its README](plugins/rust/README.md) covers Cargo commands, standards, and repository evidence.

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
