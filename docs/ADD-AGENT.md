# Add an agent

## What an agent is here

Agents are **not** an Agent Plugins v1 component. The specification standardizes skills and MCP only, and states that commands, hooks, agents, rules and LSP servers "remain too client-specific for a stable portable contract".

So `agents/` in this repository is a company convention that rides alongside the portable content:

- **Claude Code** loads it through the generated catalog.
- **GitHub Copilot** loads it — `agents/` is its documented default agent path.
- **Cursor** and **Codex** ignore the directory. Their native agent formats (`.cursor-plugin/`, `.codex-plugin/`) are formats we do not author.

That asymmetry is accepted. We do not fork the tree or duplicate content to satisfy one client.

## Steps

1. Create `plugins/company-engineering/agents/<name>.agent.md`.
2. Add frontmatter with `name` and `description`.
3. Run `dotnet run --project tools/Company.AI.Tooling -- validate`.
4. Open a pull request.

```markdown
---
name: my-agent
description: What it reviews or does, and when to invoke it.
---

# My agent

Scope, boundaries, output format.
```

## Keep agents thin

Reusable knowledge belongs in `skills/`, which every client loads. An agent should reference the relevant skills rather than restating standards, so the standard lives in one place and cannot drift between copies. That is the whole reason agents stay thin: the portable half is the valuable half.

Do not give a proof-of-concept agent authority to write to production systems.

## Duplicate names

Agent names must be unique within a plugin. Findings on agents are reported as company policy rather than specification conformance, because the specification has nothing to say about them.
