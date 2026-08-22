# Add a subagent

A subagent is a focused reviewer or worker the main agent delegates to, with its own context. Every client except Codex loads them from a plugin; the Agent Plugins specification defines no portable format, so they are authored once and generated per client.

## Steps

1. Create `plugins/<plugin>/agents/<name>.md`.
2. Write the frontmatter and the system prompt.
3. Run `dotnet run --project tools/AgentPacks.Cli -- validate`.
4. Open a pull request.

## Frontmatter

```markdown
---
name: security-reviewer
description: Reviews a change for security defects only. Use when a change touches authentication, user input, file paths or credentials.
model: inherit
readonly: true
tools:
  - read
  - grep
---
```

| Field | Required | Rule |
|---|---|---|
| `name` | yes | Kebab-case, and equal to the filename. Clients disagree on which one wins, so they must match. |
| `description` | yes | What it does *and* when to delegate to it. This is the only thing the main agent uses to decide. |
| `model` | no | `inherit`, `opus`, `sonnet` or `haiku`. Defaults to `inherit`. |
| `tools` | no | List of lowercase tool names. Translated to each client's spelling. |
| `readonly` | no | `true` or `false`. Cursor honours it directly; elsewhere it is expressed by the tools you grant. |

Anything else is rejected: a key three of the four clients ignore looks like a working restriction and is not one.

## Writing the prompt

The body is the system prompt. Say what the agent does, in what order, and what it must not do. A reviewer that can edit files will eventually edit files, so state the boundary and grant only the tools it needs.

## What gets generated

| Path | For |
|---|---|
| `agents/<name>.md` | Cursor — reads the authored file directly |
| `com.anthropic.claude-code/agents/<name>.md` | Claude — tool names in PascalCase |
| `com.github.copilot/agents/<name>.agent.md` | Copilot — note the extension |
| `com.openai.codex/agents/<name>.toml` | Codex — body becomes `developer_instructions` |

## The Codex gap

Codex loads subagents from `~/.codex/agents/` or `<repo>/.codex/agents/` only. Its plugin format has no agents component, so installing the plugin does **not** register them. The TOML is generated correctly and ready to copy:

```shell
cp plugins/delivery-loop/com.openai.codex/agents/*.toml .codex/agents/
```

If Codex gains plugin-shipped agents, only the generated manifest needs a field.
