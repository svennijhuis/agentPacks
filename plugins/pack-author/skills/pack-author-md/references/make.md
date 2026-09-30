# Make

Write one skill or one agent into a pack that already ships. Ask once when kind, owning pack, or name is missing. Then write.

## Kind

| Kind | v1 |
|---|---|
| skill | Write it. How-to: [ADD-SKILL.md](../../../../../docs/ADD-SKILL.md) |
| agent | Write it. How-to: [ADD-AGENT.md](../../../../../docs/ADD-AGENT.md) |
| command | Stop. Point at [ADD-CLIENT-EXTENSION.md](../../../../../docs/ADD-CLIENT-EXTENSION.md). Do not create `commands/` |
| hook | Stop. Point at [ADD-HOOK.md](../../../../../docs/ADD-HOOK.md). Do not create `hooks.source.json` or `scripts/` |
| rule | Stop. Point at [ADD-RULE.md](../../../../../docs/ADD-RULE.md). Do not create `rules/` |

## Owning pack

Read [PLAN.md](../../../../../docs/PLAN.md).

1. A compiler in the picture: the language pack that already ships (`dotnet`, `rust`, `typescript`).
2. An existing capability workflow: `squad`, `pack-check`, `git`, `security`, or `pack-author`.
3. A framework (React, NestJS, Axum, ASP.NET, Next): a skill inside the language pack. Do not add a plugin.
4. No shipped pack fits: stop and name the gap. Do not write `plugin.json`.

Do not edit `plugins/squad/skills/squad/SKILL.md`.

## Names

Skill name equals the directory. Kebab-case. No periods. Skill name differs from the command name. Copilot's emitted command name differs from the plugin name. Claude, Cursor, and Codex command may equal the plugin name. The stub must pass [style](style.md) before you stop.

## Skill stub

Loop slot (`<lang>-build`, `<lang>-test-patterns`, `<lang>-review`):

```markdown
---
name: dotnet-example
description: When <trigger>.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# Title

Loop-only; not as a user entrypoint.

Read references/detail.md.
```

User-typed slash (leave the command for a later change; do not create it in v1):

```markdown
---
name: example-helper
description: When <trigger>.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# Title

One job.

Read references/detail.md.

1. Do the job.
2. Stop.

Good: one concrete case.
Bad: an essay in this file.
```

Model-invoked helper: same shape, omit `disable-model-invocation`. The description stays the when-to-use line.

Create the reference the router names. A link with no file fails validate.

Description starts with `When `, ends with `.`, and stays at or under 120 characters. The router stays at or under 40 non-blank lines.

## Agent stub

```markdown
---
name: example-reviewer
description: What it does and when to delegate.
model: fast
readonly: true
tools:
  - read
  - grep
  - glob
---

One job. Report only. Do not edit or commit.

1. Read the target.
2. Return the required fields.
3. Stop.

Good: one concrete finding with path:line.
Bad: a second job, or a tool name outside the closed list.
```

Model is `inherit`, `fast`, `standard`, or `frontier`. `readonly: true` needs a tools list and omits `write` and `edit`. Closed tool names are in ADD-AGENT.md.

## Validate hints

From the agentPacks repo:

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
```

Install the generated tree under `/tmp/agentpacks-marketplace/plugins/<pack>`. The authored `plugins/` tree is the source.
