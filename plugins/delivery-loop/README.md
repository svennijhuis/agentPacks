# delivery-loop

A capability pack: the phases a change goes through, as separate roles with a fixed hand-off between them.

```
plan -> implement -> verify -> review (+ security gate) -> (fix -> verify -> review) x 2 max -> hand off
```

The loop ends at a hand-off summary. **No phase commits, merges or pushes** — landing the work is the human's step. `pass` means "ready to look at", not "ready to land".

## What is in it

| Component | Name | What it does |
|---|---|---|
| Skill | `delivery-loop` | The doctrine: proportional process, the plan artifact, the hand-off contract, evidence over claims, the fix-round cap |
| Rule | `delivery-loop-standards` | Role boundaries and the no-commit hand-off, always on |
| Agent | `loop-planner` | Request to numbered, testable acceptance criteria in `docs/plans/<slug>.md` |
| Agent | `loop-implementer` | Builds against the criteria, reports the diff and what it claims |
| Agent | `loop-verifier` | Runs the checks, reports pass or fail per criterion with real output |
| Agent | `loop-reviewer` | Change against plan; returns `pass`, `fix` or `replan` plus findings |
| Agent | `loop-security-reviewer` | The security gate: walks the OWASP Top 10 over the change and returns its own verdict |
| Hook | `beforeShellExecution` | Flags the commands that land work: `git commit`, `git push`, `git merge`, `gh pr create/merge` |

## How it relates to `code-review`

`code-review` is machinery around one phase: how to review any diff for correctness and security, whenever someone asks. `delivery-loop` is the sequence, and its reviewers answer to a plan — acceptance review asks whether the change did what was agreed, and the security gate asks whether it can be abused, both returning a verdict the loop acts on. The two packs overlap on security by design: install `code-review` for the standing practice, and this pack when you want the gate wired into the loop so a `fix` verdict actually costs a round.

## What each client receives

| | Skill | Rule | Agents | Hook |
|---|---|---|---|---|
| **Claude** | yes | injected at session start | yes | yes |
| **Cursor** | yes | yes | yes | yes |
| **Copilot** | yes | yes, as an instruction file | yes | yes |
| **Codex** | yes | manual copy | manual copy | yes |

Codex loads subagents only from `.codex/agents/`, and reads `AGENTS.md` from the workspace rather than from a plugin. Both are generated and ready to copy:

```shell
cp plugins/delivery-loop/com.openai.codex/agents/*.toml .codex/agents/
```

```shell
cp plugins/delivery-loop/com.openai.codex/AGENTS.md ./AGENTS.md
```

Claude has no rules concept, so the always-on rule arrives as a `SessionStart` hook that prints it as context. This pack ships no glob-scoped rule, because Claude cannot express one.

## The hook

`loop-guard` is advisory. It writes one line and exits 0; it never blocks a command. Clients disagree on how a hook blocks, and a guard that blocks on one client and waves things through on three is worse than one that only ever advises. A match usually means the human is landing the work — harmless — or that a phase is about to cross the line it was told not to.

## Editing this pack

Authored: `plugin.json`, `skills/`, `rules/`, `agents/`, `hooks.source.json`, `scripts/*.sh`, `scripts/*.ps1`.

Generated, never edit: `hooks/`, `scripts/*.cmd`, `.cursor-plugin/`, `.codex-plugin/`, `com.anthropic.claude-code/`, `com.openai.codex/`, `com.github.copilot/`.

See [ADD-HOOK.md](../../docs/ADD-HOOK.md), [ADD-AGENT.md](../../docs/ADD-AGENT.md) and [ADD-RULE.md](../../docs/ADD-RULE.md).
