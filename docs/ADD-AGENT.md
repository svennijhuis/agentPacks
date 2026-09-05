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
name: loop-reviewer
description: Reviews a change against its plan and verifier evidence when present, then reports correctness findings. Use in every review phase, in parallel with simplification and with security when its gate applies.
model: fast
readonly: true
tools:
  - read
  - grep
  - glob
  - bash
---

One job. Load skills with the Skill tool by exact name. Never write slash-prose.
```

That is the production shape: short frontmatter, portable tier, `readonly`, closed tool list, one
job, exact Skill-tool names. Squad agents are `loop-*`. The implementer is `standard`; other loop
agents are `fast`. Bodies stay operational-but-short (28 non-empty lines after frontmatter;
security reviewer 40). Numbered steps, required outputs, one Good/Bad pair — not essay soup.
Copy from [`plugins/squad/agents/`](../plugins/squad/agents/); do not invent a public command
per specialist.

| Field | Required | Rule |
|---|---|---|
| `name` | yes | Kebab-case, and equal to the filename. Clients disagree on which one wins, so they must match. |
| `description` | yes | What it does *and* when to delegate to it. This is the only thing the main agent uses to decide. |
| `model` | no | A portable tier: `inherit` (default), `fast`, `standard`, or `frontier`. Never a Claude alias (`opus`, `sonnet`, `haiku`) and never a Cursor id. [`models.source.json`](../models.source.json) maps the tier to each client. |
| `tools` | no | List of lowercase tool names, from the closed vocabulary below. Translated to each client's spelling. |
| `readonly` | no | `true` or `false`. Cursor honours it directly; elsewhere it is expressed by the tools you grant, so `readonly: true` requires a `tools` list and rejects `write` and `edit`. |

Anything else is rejected: a key three of the four clients ignore looks like a working restriction and is not one.

### The tool vocabulary

```
read   write   edit   grep   glob   bash   webfetch   websearch
```

The list is closed, and a name outside it fails validation. That is not tidiness: it is the one authoring mistake no client reports. Claude renames these to `Read`, `WebFetch` and so on, and **drops any name it does not recognise**; the other three pass the list through untouched. So `websearchh`, or `Write` with a capital, generates cleanly, installs cleanly, and quietly leaves the agent without a capability it was granted on purpose — with nothing printed at any stage to say so.

It is also what makes `readonly` enforceable. That check works by matching declared names against `write` and `edit`, so a misspelt writing tool would pass it while the client still makes of the name whatever it makes.

The vocabulary lives in `tools/AgentPacks.Cli/Generation/NeutralTools.cs`, which the generator maps with and the validator rejects against, so a name can never be known to one and unknown to the other. Adding a tool is a line there.

`readonly` is validated rather than generated, for the same reason. No client but Cursor has a read-only flag, so what actually carries the restriction into the other three is the tool list: an agent with no `tools` inherits everything the client has, and one that lists `write` gets `write`. Declaring `readonly: true` therefore commits you to a tool list that backs it up. Note that `bash` is not counted as a writing tool — a verifier needs it to run a test suite — so an agent that grants `bash` is trusting the prompt, not the tool list, for that part.

## Writing the prompt

The body is the system prompt. One job. Numbered steps. A Good/Bad pair. Required output fields.
A reviewer that can edit files will eventually edit files, so state the boundary and grant only the
tools it needs. Load skills with the Skill tool by exact name.

## What gets generated

| Path | For |
|---|---|
| `agents/<name>.md` | Authored portable tier (source). Cursor's plugin loader still reads this root file |
| `.cursor-plugin/agents/<name>.md` | Cursor — remapped id (`inherit`, `composer-2`, `grok-4.5`, `claude-opus-5`) |
| `com.anthropic.claude-code/agents/<name>.md` | Claude — remapped id, tool names in PascalCase |
| `com.github.copilot/agents/<name>.agent.md` | Copilot — remapped id; `model` is never dropped |
| `com.openai.codex/agents/<name>.toml` | Codex — `model` is emitted and inherit-first |

## The Codex gap

Codex loads subagents from `~/.codex/agents/` or `<repo>/.codex/agents/` only. Its plugin format has no agents component, so installing the plugin does **not** register them. The TOML is generated correctly and ready to copy:

```shell
cp plugins/squad/com.openai.codex/agents/*.toml .codex/agents/
```

Codex generation always emits `model = "inherit"`. A non-inherit TOML pin is inherit-first on
purpose: pinning a specific Codex model in the generated TOML is flaky on spawn, so the catalog's
Codex column stays `inherit` even when the authored tier is `frontier`.

If Codex gains plugin-shipped agents, only the generated manifest needs a field.

Cost-first: catalog default is `inherit`. The implementer (the agent that writes code) uses
`standard`. Other loop subagents use `fast`. Generation remaps those portable tiers for every
client, including Cursor under `.cursor-plugin/agents/`. Do not author `sonnet` to mean Cursor.
