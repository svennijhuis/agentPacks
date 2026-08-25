# delivery-loop

A capability pack for a main-agent-controlled delivery workflow:

```text
plan -> implement -> verify -> parallel review -> merge -> (fix -> verify -> parallel review -> merge) x 2 max -> hand off
```

The main agent owns user interaction, invokes `loop-planner` once per question round, launches applicable reviewers in parallel, and routes the merged verdict. `loop-orchestrator` only merges completed reports and verifier evidence. No phase commits, merges, or pushes.

Obvious typos, renames, and one-line fixes take the small-change route: the main agent implements and verifies directly, with no plan and without invoking planner, implementer, verifier, or review agents.

## Components

| Component | Name | Responsibility |
|---|---|---|
| Skill | `delivery-loop` | Routing, phase ownership, security gate, fix-round cap, worktree lifecycle, and hand-off |
| Contract | `planning-contract` | Turn-based planner input, one-round output, confirmation gate, and plan shape |
| Contract | `review-contract` | Severity, report formats, merge identity, evidence gate, and verdict rules |
| Rule | `review-checklist` | Source-review checklist scoped by glob; Cursor-only by design |
| Agent | `loop-planner` | Returns one numbered planning round, or writes the one confirmed plan |
| Agent | `loop-implementer` | Implements a confirmed plan or merged fix list |
| Agent | `loop-verifier` | Reports independent evidence per plan criterion |
| Agent | `loop-reviewer` | Reviews correctness and plan compliance |
| Agent | `loop-security-reviewer` | Reviews trust-boundary changes against [OWASP Top 10:2025](https://owasp.org/Top10/) |
| Agent | `loop-simplifier` | Finds unnecessary implementation complexity |
| Agent | `loop-orchestrator` | Deduplicates completed reports, assigns the verdict, and appends the fix list |
| Command | `deliver` | Runs a new change through the proportional workflow |
| Command | `review-diff` | Reviews an existing diff without a plan, verdict, or fix round |

All seven agents remain portable across supported generated clients.

## Planning

Planning is mediated by the main agent. Each `loop-planner` invocation receives the request, repository evidence, settled decisions, prior answers, and open frontier. It returns one numbered round and stops. The main agent presents that round and supplies the answers on the next invocation.

After every decision is settled, the planner returns a shared-understanding confirmation question. Only after the user confirms may a write-mode invocation create `docs/plans/<slug>.md`. Repository evidence, citations, decisions, and rejected alternatives stay in that plan; no separate research artifact is created.

## Review and verdicts

For a planned change, the main agent runs correctness and simplification reviewers together, adding security when a trust boundary changed. It then sends completed reports, verifier evidence, round, plan path, and the recorded security decision to the orchestrator.

`pass` requires adequate evidence for every criterion and no blocking merged finding. `high` or `medium` findings produce `fix`; a plan defect produces `replan`. At most two fix rounds are allowed.

`/review-diff` uses the same conditional reviewers for an existing diff, but has no plan, verifier evidence, verdict, or fix round.

## Stack and workspace

[`pack-check`](../pack-check/README.md) detects every registered stack, then selects the stacks
applicable to the target paths, diff, and acceptance criteria. A mixed change loads both .NET and
Rust slots; a single-stack change does not load or request the unrelated pack. A full loop records
the applicable plugin standards and concrete repository conventions in the plan. The
[language-pack contract](../../docs/ADD-LANGUAGE-PACK.md) defines the required skill names.

The hand-off records whether work ran in the primary checkout, an existing worktree, or a loop-created worktree. Externally owned and dirty worktrees are preserved. A clean loop-created worktree may be removed without force.

## Provider support

| | Skill | Glob-scoped rule | Agents | Command |
|---|---|---|---|---|
| Claude | yes | no | yes | yes |
| Cursor | yes | yes | yes | yes |
| GitHub Copilot | yes | no | yes | yes |
| Codex | yes | no | manual copy | — |

The scoped-rule limitation is intentional. Cursor is the only target that can carry the rule's glob contract through plugin packaging; generation emits the documented portability warning for the other clients instead of making the checklist always-on.

Codex agent files are generated for manual copy:

```shell
cp plugins/delivery-loop/com.openai.codex/agents/*.toml .codex/agents/
```

## Editing

Authored: `plugin.json`, `skills/`, `rules/`, `agents/`, and `commands/`.

Generated only in validation output or on the marketplace branch: client manifests and `com.*` provider trees.

See [ADD-SKILL.md](../../docs/ADD-SKILL.md), [ADD-HOOK.md](../../docs/ADD-HOOK.md), [ADD-AGENT.md](../../docs/ADD-AGENT.md), and [ADD-RULE.md](../../docs/ADD-RULE.md).
