# Add a rule

A rule is standing guidance: it applies without anyone invoking it. Always-on rules can be translated
for all clients; glob-scoped rules are portable only to Cursor and intentionally produce one warning
for the other clients.

## Steps

1. Create `plugins/<plugin>/rules/<name>.mdc`.
2. Decide the scope: always on, or limited to paths.
3. Run `dotnet run --project tools/AgentPacks.Cli -- validate`.
4. Open a pull request.

## Frontmatter

```markdown
---
description: Standards that apply to every change under review
alwaysApply: true
---
```

or

```markdown
---
description: Checklist to apply when reviewing source files
globs:
  - "**/*.cs"
  - "**/*.ts"
---
```

| Field | Required | Rule |
|---|---|---|
| `description` | yes | What the rule covers. |
| `alwaysApply` | one of the two | `true` for standing guidance. |
| `globs` | one of the two | Paths the rule is limited to. |

Declaring both is ambiguous and declaring neither produces a rule no client ever applies, so both fail validation. A rule carries no `name`: in Cursor's `.mdc` format the filename is the identity.

## Writing the body

A rule is loaded into every session it applies to, so it competes with the user's actual request for attention. Keep it to a short list of things that are true regardless of the task. Anything that only matters sometimes belongs in a skill, which is loaded on demand.

## What gets generated

| Path | For | Scope support |
|---|---|---|
| `rules/<name>.mdc` | Cursor — reads the authored file directly | `alwaysApply` and `globs` |
| `com.github.copilot/scripts/rules-context.*` | Copilot | always-on only |
| `com.anthropic.claude-code/scripts/rules-context.*` | Claude | always-on only |
| `com.openai.codex/AGENTS.md` | Codex | always-on only |

## The Claude, Copilot and Codex gaps

Claude has no rules component. Always-on rules are baked into a generated `SessionStart` hook that prints them as context — baked, so the hook reads no files at runtime. A `globs` rule cannot be expressed at all; generation emits a warning naming the rule rather than dropping it silently.

Copilot takes the same route, for a different reason. Its plugin schema declares `agents`, `skills`, `commands`, `hooks`, `mcpServers` and `lspServers` — and nothing for instructions. A `.instructions.md` file placed inside a plugin is a file no manifest key can point at, so it would ship to the marketplace and be loaded by nothing. Always-on rules become Copilot's `SessionStart` hook instead. Repository-level `.github/copilot-instructions.md` is a separate mechanism and is not something a plugin can install.

Codex reads `AGENTS.md` from the workspace, not from a plugin. The generated file is ready to copy into the repository that needs it:

```shell
cp plugins/delivery-loop/com.openai.codex/AGENTS.md ./AGENTS.md
```
