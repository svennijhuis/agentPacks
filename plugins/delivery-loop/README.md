# delivery-loop

A capability pack: the phases a change goes through, as separate roles with a fixed hand-off between them.

```
plan -> implement -> verify -> review -> (fix -> verify -> review) x 2 max -> hand off

                              review = loop-reviewer        ┐
                                       loop-security-reviewer ├ in parallel -> loop-orchestrator merges
                                       loop-simplifier      ┘
```

Every phase returns a fixed shape — the reviewers all emit the same `Severity | Location | Problem | Fix` table, the verifier a criterion-by-criterion evidence table. That is what makes merging mechanical instead of interpretive; the formats are in the `delivery-loop` skill.

Three reviewers read the same diff and answer different questions, so they run at once rather than in a queue. `loop-orchestrator` merges what comes back into one list — deduplicated by location at the highest severity reported — ranks it `high` / `medium` / `low` / `tiny`, and writes it into the plan. `high` or `medium` costs a fix round; `low` and `tiny` ride along as notes. The implementer picks the list up and works it in order.

The loop ends at a hand-off summary. **No phase commits, merges or pushes** — landing the work is the human's step. `pass` means "ready to look at", not "ready to land".

## What is in it

| Component | Name | What it does |
|---|---|---|
| Skill | `delivery-loop` | The doctrine: proportional process, the plan artifact, the hand-off contract, the severity scale, the report formats, the fix-round cap |
| Rule | `delivery-loop-standards` | Standards for the change, role boundaries, and the no-commit hand-off. Always on |
| Rule | `review-checklist` | Checklist applied to source files, scoped by glob |
| Agent | `loop-planner` | Request to numbered, testable acceptance criteria in `docs/plans/<slug>.md` |
| Agent | `loop-implementer` | Builds against the criteria, reports the diff and what it claims |
| Agent | `loop-verifier` | Runs the checks, reports pass or fail per criterion with real output |
| Agent | `loop-reviewer` | Correctness and criteria: does the change do what the plan said |
| Agent | `loop-security-reviewer` | The security gate: walks the [OWASP Top 10](https://owasp.org/Top10/), linked per category |
| Agent | `loop-simplifier` | Reuse, simplification, efficiency, altitude — did it have to be this much code |
| Agent | `loop-orchestrator` | Runs the three reviewers in parallel, merges their findings into one ranked list, decides the verdict, writes the fix list into the plan |
| Command | `review-diff` | Reviews an uncommitted diff on its own, for a change that arrived without a plan |
| Hook | `beforeShellExecution` | Flags the commands that land work: `git commit`, `git push`, `git merge`, `gh pr create/merge` |

## Review lives here too

An earlier version of this repository split review into its own pack. It did not survive contact with this one: both shipped a security reviewer, both shipped a diff reviewer, and the only real difference was whether a finding cost a fix round or was just printed.

So review is a phase of the loop, not a neighbouring pack. `loop-reviewer` measures the change against a plan, `loop-security-reviewer` is the gate, `loop-simplifier` asks what the change could have skipped, and `/review-diff` runs the same three without a plan when a change turns up already written.

The pack briefly carried the old pack's `code-review` skill as well. It went the same way: its three sections had become three agents, and its instruction to *stop at the first category with findings* was actively wrong once the reviewers started running in parallel.

## What each client receives

| | Skill | Rules | Agents | Command | Hook |
|---|---|---|---|---|---|
| **Claude** | yes | always-on only, injected at session start | yes | yes | yes |
| **Cursor** | yes | yes, including glob scope | yes | yes | yes |
| **Copilot** | yes | yes, as instruction files | yes | yes | yes |
| **Codex** | yes | manual copy | manual copy | — | yes |

Codex loads subagents only from `.codex/agents/`, and reads `AGENTS.md` from the workspace rather than from a plugin. Both are generated and ready to copy:

```shell
cp plugins/delivery-loop/com.openai.codex/agents/*.toml .codex/agents/
```

```shell
cp plugins/delivery-loop/com.openai.codex/AGENTS.md ./AGENTS.md
```

Claude has no rules concept, so the always-on rule arrives as a `SessionStart` hook that prints it as context. Glob-scoped rules cannot be expressed there, so `review-checklist` is not generated for Claude — generation says so rather than dropping it quietly.

## The hook

`loop-guard` is advisory. It writes one line and exits 0; it never blocks a command. Clients disagree on how a hook blocks, and a guard that blocks on one client and waves things through on three is worse than one that only ever advises. A match usually means the human is landing the work — harmless — or that a phase is about to cross the line it was told not to.

## Editing this pack

Authored: `plugin.json`, `skills/`, `rules/`, `agents/`, `commands/`, `hooks.source.json`, `scripts/*.sh`, `scripts/*.ps1`.

Generated, never edit: `hooks/`, `scripts/*.cmd`, `.cursor-plugin/`, `.codex-plugin/`, `com.anthropic.claude-code/`, `com.openai.codex/`, `com.github.copilot/`.

See [ADD-SKILL.md](../../docs/ADD-SKILL.md), [ADD-HOOK.md](../../docs/ADD-HOOK.md), [ADD-AGENT.md](../../docs/ADD-AGENT.md) and [ADD-RULE.md](../../docs/ADD-RULE.md).
