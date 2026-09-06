# agentPacks

Portable [Agent Plugins](https://agent-plugins.org) that wire a workflow into the agent loop.

## Available plugins

Three capability packs, installed because you want a behaviour wired into the agent loop rather than knowledge sitting on a shelf, and three language packs, installed because of the ecosystem you compile in:

| Plugin | Use it for |
| --- | --- |
| `squad` | User-invoked `/squad` and `/squad-review`: grill-style planning, gated implement/verify/review, ≤2 fix rounds, uncommitted hand-off, append-only learnings |
| `pack-check` | Detecting the repository stack at session start and asking before installing the language pack that supplies its required build and test skills |
| `git` | Blocking the git commands that destroy work an agent cannot get back — `reset --hard`, `clean -fd`, `push --force`, `branch -D`, `checkout .` — before the client runs them |
| `dotnet` | Teaching the loop how C# is built, tested and reviewed, backed by one canonical set of standards distributed only to the skills that need each document |
| `rust` | Teaching the loop how Cargo workspaces are built, tested and reviewed, backed by canonical Rust, error/concurrency, and testing standards |
| `typescript` | Teaching the loop how TypeScript is typechecked, tested and reviewed, backed by canonical type and testing standards |

That is the whole catalog. [`docs/PLAN.md`](docs/PLAN.md) is the rule for what earns a later pack, not a list of packs that exist.

Adding the marketplace makes all plugins discoverable. Install only the plugins you want. The commands install globally for your user.

## GitHub Copilot CLI

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
copilot plugin install squad@agentpacks
copilot plugin install pack-check@agentpacks
copilot plugin install git@agentpacks
copilot plugin install dotnet@agentpacks
copilot plugin install rust@agentpacks
copilot plugin install typescript@agentpacks
```

Update later with:

```shell
copilot plugin marketplace update agentpacks
```

## VS Code

**Chat: Install Plugin from Source** needs the generated `marketplace` branch. Use the branch picker,
or a local clone of that branch. That is the only tree that contains
`.github/plugin/marketplace.json`.

Pasting `https://github.com/svennijhuis/agentPacks.git#marketplace` often hits `main`. The
`#marketplace` fragment is not enough: `main` has no catalog, so VS Code reports
**No plugins found** / **not a valid marketplace**.

```shell
git clone --branch marketplace --single-branch https://github.com/svennijhuis/agentPacks.git
```

Then point **Install Plugin from Source** at the clone, or add the clone as a `file:///` entry in
`chat.plugins.marketplaces`.

The Copilot CLI path still works, and VS Code discovers plugins the CLI already installed:

```shell
copilot plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace
copilot plugin install squad@agentpacks
```

## Codex

```shell
codex plugin marketplace add svennijhuis/agentPacks --ref marketplace
codex plugin add squad@agentpacks
codex plugin add pack-check@agentpacks
codex plugin add git@agentpacks
codex plugin add dotnet@agentpacks
codex plugin add rust@agentpacks
codex plugin add typescript@agentpacks
```

Open `/plugins` in Codex to inspect the installed plugins. Update later with:

```shell
codex plugin marketplace upgrade agentpacks
```

## Claude Code

```shell
claude plugin marketplace add https://github.com/svennijhuis/agentPacks.git#marketplace --scope user
claude plugin install squad@agentpacks --scope user
claude plugin install pack-check@agentpacks --scope user
claude plugin install git@agentpacks --scope user
claude plugin install dotnet@agentpacks --scope user
claude plugin install rust@agentpacks --scope user
claude plugin install typescript@agentpacks --scope user
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
ln -s ~/.cursor/agentPacks/plugins/squad ~/.cursor/plugins/local/squad
ln -s ~/.cursor/agentPacks/plugins/pack-check ~/.cursor/plugins/local/pack-check
ln -s ~/.cursor/agentPacks/plugins/git ~/.cursor/plugins/local/git
ln -s ~/.cursor/agentPacks/plugins/dotnet ~/.cursor/plugins/local/dotnet
ln -s ~/.cursor/agentPacks/plugins/rust ~/.cursor/plugins/local/rust
ln -s ~/.cursor/agentPacks/plugins/typescript ~/.cursor/plugins/local/typescript
```

Create only the links for the plugins you want, then restart Cursor or run **Developer: Reload Window**. Update later with:

```shell
git -C ~/.cursor/agentPacks pull --ff-only
```

Teams and Enterprise administrators can instead import this repository's `marketplace` branch as a team marketplace; users can then install plugins from **Customize**.

## Local development

Work on a feature branch. Do not merge to `main` or publish to `marketplace` to try a change.

```bash
git clone https://github.com/svennijhuis/agentPacks.git
cd agentPacks
gh pr checkout 7
```

Edit under `plugins/squad/` or a language pack (`plugins/dotnet/`, `plugins/rust/`, `plugins/typescript/`).

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
```

Install the **generated** tree — not the authored `plugins/` folder — without merging. All three clients:

```bash
claude --plugin-dir /tmp/agentpacks-marketplace/plugins/squad
ln -sfn /tmp/agentpacks-marketplace/plugins/squad ~/.cursor/plugins/local/squad
copilot plugin marketplace add /tmp/agentpacks-marketplace
copilot plugin install squad@agentpacks
```

Reload the client, then smoke `/squad` or `/squad-review` once. The generated tree is what coworkers install; the authored tree is the source. The same loop is in [ADD-SKILL.md — Test a skill locally](docs/ADD-SKILL.md#test-a-skill-locally).

## What each client gets

Skills and MCP servers are portable: every client loads them from the same files. Rules, subagents, commands and hooks are not — the Agent Plugins standard leaves all four out as too client-specific — so this repository authors them once and generates a tree per client.

| | Skills | MCP | Rules | Agents | Commands | Hooks |
| --- | --- | --- | --- | --- | --- | --- |
| Claude Code | yes | yes | always-on only, at session start | yes | yes | yes |
| Cursor | yes | yes | always-on and glob-scoped | yes | yes | yes |
| GitHub Copilot | yes | yes | always-on only, at session start | yes | yes | yes |
| Codex | yes | yes | always-on, manual copy | manual copy | — | yes |

Codex loads subagents only from `.codex/agents/` and reads `AGENTS.md` from the workspace rather than from a plugin, so those arrive as generated files you copy once:

```shell
cp plugins/squad/com.openai.codex/agents/*.toml .codex/agents/
```

Glob-scoped rules remain Cursor-only; other clients receive only always-on rules, and validation reports the expected portability warning. The [`squad`](plugins/squad/README.md) README has the details.

## Using the plugins

Two user-invoked entrypoints. The model does not pick the orchestrator. The numbered flow below is
the locked v1 plan; the [`squad` skill](plugins/squad/skills/squad/SKILL.md) carries the same block.

## Locked v1 flow

```text
/squad (user-invoked orchestrator)
1. Read and apply learnings.md (append-only): prefer passed skips/tiers; avoid what failed
2. Orient codebase (applicable stacks only)
3. Small change? → main agent only, spawn nobody → verify → append learnings → hand off uncommitted
4. Else grill/plan rounds (facts via subagent; decisions = human) → write plan
5. Gate spins: implementer → verifier → reviewers in parallel (correctness + plan/spec; security ONLY if trust boundary)
6. Orchestrator merges ≤2 fix rounds → hand off uncommitted → append learnings

/squad-review
Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → append learnings → one save-markdown ask

Always
models.source.json tiers (default inherit); load only contracted <lang>-* by Skill name; coworker docs = real dotnet test/validate on a fixture.

Not in v1
second skill pack, Matt catalog dump, eager fan-out, self-improve graphs, auto skill rewrite, redoing PR #6.
```

Step 1 **applies** the latest same-entrypoint entry when gating and spinning. Keep a passed
skip and tier; a failed skip becomes a must-run. After a fail, demote one model tier. Do not
rewrite skills. That is the whole v1 self-improve half: apply notes, no graphs.

Portable model tiers live in [`models.source.json`](models.source.json) (default `inherit`;
implementer `standard`; other loop agents `fast`). Test a skill on a feature branch without
merging to `main`: [docs/ADD-SKILL.md](docs/ADD-SKILL.md#test-a-skill-locally).

### `/squad`

```
/squad add an integration test for the orders endpoint
```

### `/squad-review`

```
/squad-review
/squad-review --uncommitted
/squad-review --pr
/squad-review --base main
```

After the merged list, `/squad-review` asks once: Save report as markdown? Yes writes
`docs/reviews/<slug>.md` and still shows the findings in the IDE/CLI. No stays IDE/CLI only.
No verdict, grill, or fix round. Never `docs/decisions.md`.

Language-pack slots (`dotnet-build`, `rust-review`, …) are loop internals, loaded by exact Skill
tool name. They are not a second public skill surface.

`git` needs no invocation either, and has nothing to ask: its hook blocks `git reset --hard`, `git clean -fd`, `git push --force`, `git branch -D`, `git checkout .` and `git restore .` before the client runs them, with the reason on stderr. [Its README](plugins/git/README.md) covers the `AGENTPACKS_GIT_GUARD=off` switch and which clients the blocking contract is actually verified on.

`pack-check` runs at session start. It maps .NET markers to `dotnet`, `Cargo.toml` to `rust`, and
`package.json` / `tsconfig.json` to `typescript`, then
verifies the required build and test slots for the stacks applicable to the change. A mixed change
loads both; an unrelated stack in the same repository does not trigger an install. CLI clients run
their own installer after one grouped approval, while Cursor uses **Customize**. Reload after
installation so the new skills enter the next session. [Its README](plugins/pack-check/README.md)
has the provider-specific behavior.

`dotnet` needs no invocation. Its skills are loaded when the Loop detects a `.slnx`, `.sln`, or
`.csproj`. [Its README](plugins/dotnet/README.md) explains how its canonical standards stay inside
the plugin while agents match the repository they are working in.

`rust` needs no invocation. Its skills are loaded for an applicable Cargo workspace. In a mixed
repository, the Loop uses Rust, .NET, TypeScript, or the applicable mix from the target paths, diff, and acceptance criteria.
[Its README](plugins/rust/README.md) covers Cargo commands, standards, and repository evidence.

`typescript` needs no invocation. Its skills are loaded for an applicable `package.json` / `tsconfig.json` tree.
[Its README](plugins/typescript/README.md) covers package-manager detection, `tsc`, and test runners.

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
