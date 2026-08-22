# code-review

A capability pack: the knowledge for reviewing a change, plus the machinery around it.

## What is in it

| Component | Name | What it does |
|---|---|---|
| Skill | `code-review` | Reviews a change for correctness, security and maintainability, ranked by severity |
| Rule | `review-standards` | Standards that apply to every change, always on |
| Rule | `review-checklist` | Checklist applied to source files, scoped by glob |
| Agent | `security-reviewer` | Security defects only: injection, secrets, authorisation, unsafe defaults |
| Agent | `diff-reviewer` | Correctness and maintainability of the change in progress |
| Command | `review-diff` | Reviews the current uncommitted diff |
| Hook | `beforeShellExecution` | Flags commands that skip the review loop, such as `git push` or `--no-verify` |

## What each client receives

| | Skill | Rules | Agents | Command | Hook |
|---|---|---|---|---|---|
| **Claude** | yes | always-on only, injected at session start | yes | yes | yes |
| **Cursor** | yes | yes, including glob scope | yes | yes | yes |
| **Copilot** | yes | yes, as instruction files | yes | yes | yes |
| **Codex** | yes | manual copy | manual copy | — | yes |

Two gaps are the clients', not the pack's. Codex loads subagents only from `.codex/agents/`, and reads `AGENTS.md` from the workspace rather than from a plugin. Both files are generated and ready to copy:

```shell
cp plugins/code-review/com.openai.codex/agents/*.toml .codex/agents/
```

```shell
cp plugins/code-review/com.openai.codex/AGENTS.md ./AGENTS.md
```

Claude has no rules concept at all, so always-on rules arrive as a `SessionStart` hook that prints them as context. Glob-scoped rules cannot be expressed there and are not generated for Claude — generation says so rather than dropping them quietly.

## The hook

`review-guard` is advisory. It writes one line and exits 0; it never blocks a command. Clients disagree on how a hook blocks, and a guard that blocks on one client and waves things through on three is worse than one that only ever advises.

## Editing this pack

Authored: `plugin.json`, `skills/`, `rules/`, `agents/`, `commands/`, `hooks.source.json`, `scripts/*.sh`, `scripts/*.ps1`.

Generated, never edit: `hooks/`, `scripts/*.cmd`, `.cursor-plugin/`, `.codex-plugin/`, `com.anthropic.claude-code/`, `com.openai.codex/`, `com.github.copilot/`.

See [ADD-HOOK.md](../../docs/ADD-HOOK.md), [ADD-AGENT.md](../../docs/ADD-AGENT.md) and [ADD-RULE.md](../../docs/ADD-RULE.md).
