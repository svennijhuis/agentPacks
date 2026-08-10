# agentPacks

Repository for sharing Agent Plugins: skills, MCP integrations and specialized agents.

Currently a public repository. Everything here works the same way once it moves into a private company org — the only difference is that clients then authenticate with normal Git credentials.

The portable content follows the open [Agent Plugins v1](https://agent-plugins.org/) specification, so one plugin works across ChatGPT/Codex, Cursor, GitHub Copilot, Kiro and VS Code without any per-client packaging. Claude Code is the single exception, and it is handled by a small generated compatibility layer that points at the same directory.

## What you edit

```
plugins/company-engineering/plugin.json     manifest
plugins/company-engineering/skills/         reusable skills
plugins/company-engineering/agents/         thin shared agents
plugins/company-engineering/mcp.json        MCP servers
external/sources.json                       approved, pinned external sources
```

## What you never edit

```
.claude-plugin/marketplace.json             GENERATED
plugins/*/.mcp.json                         GENERATED
```

Those two are produced by the .NET tooling from the files above and committed by GitHub Actions. Editing them by hand is undone on the next merge, and the scheduled drift job will flag it.

## Who reads what

| | Codex · Cursor · Copilot · Kiro · VS Code | Claude Code |
|---|---|---|
| Skills | portable tree, no packaging | generated catalog |
| MCP | portable tree, no packaging | generated catalog |
| `agents/` | Copilot reads `agents/`; the others ignore it | generated catalog |

Agent Plugins v1 standardizes exactly two component types, skills and MCP. Commands, hooks, agents, rules and LSP servers are left to individual clients, so `agents/` is a company convention rather than a portable component. We do not author client-specific plugin formats — no `.cursor-plugin/`, no `.codex-plugin/`, no vendor extension directories. A client feature that would require its own packaging is a feature we skip.

## Add a skill

Create `plugins/company-engineering/skills/<name>/SKILL.md`:

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

## Add an agent

Create `plugins/company-engineering/agents/<name>.agent.md`. Keep agents thin: reusable knowledge belongs in a skill, which is portable, while an agent is not. See [docs/ADD-AGENT.md](docs/ADD-AGENT.md).

## Add an MCP server

Edit `plugins/company-engineering/mcp.json`. Prefer read-only servers, use HTTPS for anything non-loopback, and keep credentials out: plugin files are literal, visible package data, and authentication is client-managed. See [docs/ADD-MCP.md](docs/ADD-MCP.md).

## Validate locally

Requires the .NET SDK pinned in `global.json` (.NET 10).

```bash
dotnet test tools/Company.AI.Tooling.slnx -c Release
```

```bash
dotnet run --project tools/Company.AI.Tooling -- validate
```

```bash
dotnet run --project tools/Company.AI.Tooling -- validate-all
```

`validate` checks the portable source. `validate-all` also regenerates the Claude layer and validates the result. Use `generate-claude --check` to confirm the committed generated files still match the source without writing anything.

External skills are referenced, never copied. Record the URL, the directory and an exact commit in `external/sources.json`, and generation adds a pinned catalog entry pointing at the upstream repository. No third-party markdown lands in this repository. See [docs/EXTERNAL-SOURCES.md](docs/EXTERNAL-SOURCES.md).

The validator reports every finding in one run, tagged by severity: `spec` for something a client would reject, `spec(tolerated)` for a violation clients ignore but we do not, `policy` for our own rules, and `warning` for advice that does not fail the build.

Validation is offline. The `$schema` URL in a manifest is an identifier, not a fetch: the official schemas are vendored under `schemas/` and resolved locally.

## Install

### Copilot, Cursor, Codex, Kiro, VS Code

These clients support Agent Plugins directly and read `plugins/company-engineering/` with no adapter. For Copilot CLI:

```bash
copilot plugin marketplace add svennijhuis/agentPacks
```

```bash
copilot plugin install company-engineering@agentpacks
```

### Claude Code

```bash
/plugin marketplace add svennijhuis/agentPacks
```

```bash
/plugin install company-engineering@agentpacks
```

Refresh with `/plugin marketplace update agentpacks`. Claude reads the repository over Git; once it is private, your normal Git credentials apply. See [docs/CLAUDE-PRIVATE-REPO.md](docs/CLAUDE-PRIVATE-REPO.md).

## Versioning and updates

The generated marketplace entry deliberately omits `version`. Claude resolves updates from an explicit version before falling back to the Git commit SHA, so a version that is never bumped would keep everyone pinned to cached content even after a skill changes. Leaving it out means every merge to `main` is picked up. The same expectation applies to any other marketplace we publish to: either omit the version, or bump it on every change to installable content.

## Continuous integration

Pull requests run the tests, validate the source, and prove that generation succeeds — into a temporary directory, so the working tree stays clean. A stale committed catalog does not fail a pull request.

After a merge to `main`, a separate job re-runs the same gate and commits the regenerated files. If branch protection blocks the bot from pushing, switch that job to open a pull request with `peter-evans/create-pull-request` instead — and drop the push. The source-of-truth rule does not change: developers own the plugin source, automation owns the generated files.

## Scope

First proof of concept: one repository, one `company-engineering` plugin, shared skills, thin agents, read-only MCP first, external skills only when reviewed and pinned, C# validation, GitHub Actions, and a generated Claude catalog. GitHub Agentic Workflows stay in the application repositories that consume these capabilities — see [docs/GITHUB-AGENTIC-WORKFLOWS.md](docs/GITHUB-AGENTIC-WORKFLOWS.md).

Avoid building an internal framework until the standards settle and teams have proven they need more.

Reference links: [docs/REFERENCES.md](docs/REFERENCES.md).
