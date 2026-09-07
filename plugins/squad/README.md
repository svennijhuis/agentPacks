# Squad

A capability pack for a user-invoked, main-agent-controlled Squad workflow.
`/squad` and `/squad-review` run the Squad loop. Sibling slash `/scenarios` writes smoke
markdown from changed code; it is not a Squad phase. `/pack-check` is setup, not a Squad flow.
The `squad` skill is the thin orchestrator and is not model-invoked.

The numbered flow below is locked v1 and is copied verbatim into the
[skill](skills/squad/SKILL.md) and the repository [README](../../README.md).

## Locked v1 flow

```text
/squad (user-invoked orchestrator)
1. Read and apply learnings.md (append-only): prefer passed skips/tiers; avoid what failed
2. Orient codebase (applicable stacks only)
3. Small change? → main agent only, spawn nobody → verify → append learnings → hand off uncommitted
4. Else grill/plan rounds (facts via subagent; decisions = human) → write plan
5. Gate spins: implementer → verifier → reviewers in parallel (correctness + plan/spec; security ONLY if trust boundary)
6. Orchestrator merges ≤2 fix rounds → hand off uncommitted → append learnings

/squad-review
Pin vs PR / uncommitted / main → same gated dual-axis reviewers (no plan/fix loop) → append learnings → one save-markdown ask

Always
models.source.json tiers (default inherit); load only contracted <lang>-* by Skill name; coworker docs = real dotnet test/validate on a fixture.

Not in v1
second skill pack, Matt catalog dump, eager fan-out, self-improve graphs, auto skill rewrite, redoing PR #6.
```

Step 1 applies the latest learnings entry when gating and spinning; it does not only
acknowledge the file. Prefer passed skips and tiers. A failed skip is a must-run. After a
fail, demote one model tier. Do not rewrite skills, and do not grow a graph from the log.

The main agent owns user interaction, invokes `squad-planner` once per question round, launches applicable reviewers in parallel, and routes the merged verdict. `squad-orchestrator` only merges completed reports and verifier evidence. No phase commits, merges, or pushes.

Obvious typos, renames, and one-line fixes take the small-change route: the main agent implements and verifies directly, with no plan and without invoking planner, implementer, verifier, or review agents.

## Components

| Component | Name | Responsibility |
|---|---|---|
| Skill | `squad` | User-invoked routing, exact Skill-name loading, security gate, fix-round cap, learnings, and hand-off |
| Skill | `learnings-digest` | User-invoked digest of `docs/learnings.md`. Does not rewrite skills. |
| Skill | `http-scenarios` | User-invoked changed-code → `docs/smoke/<slug>.md`. OpenAPI optional. No product-code edits. |
| Contract | `planning-contract` | Turn-based grill inside the orchestrator: frontier rounds, recommended answers, confirmation, plan shape |
| Contract | `review-contract` | Dual-axis review, severity, report formats, verify-path evaluator gates, and verdict rules |
| Contract | `learnings` | Append-only run log read on the next `/squad` or `/squad-review` |
| Rule | `review-checklist` | Source-review checklist scoped by glob; Cursor-only by design |
| Agent | `squad-planner` | Returns one numbered planning round, or writes the one confirmed plan |
| Agent | `squad-implementer` | Implements a confirmed plan or merged fix list |
| Agent | `squad-verifier` | Reports independent evidence per plan criterion |
| Agent | `squad-reviewer` | Reviews correctness and plan compliance |
| Agent | `squad-security-reviewer` | Reviews trust-boundary changes against [OWASP Top 10:2025](https://owasp.org/Top10/) |
| Agent | `squad-simplifier` | Finds unnecessary implementation complexity |
| Agent | `squad-orchestrator` | Deduplicates completed reports, assigns the verdict, and appends the fix list |
| Command | `squad` | Runs a new change through the proportional workflow. Copilot picker: `/squad:run` |
| Command | `squad-review` | Reviews a PR, uncommitted work, or a diff versus main, without a plan, verdict, or fix round |
| Command | `scenarios` | From changed code (OpenAPI optional), writes only `docs/smoke/<slug>.md`. Copilot picker: `/squad:scenarios` |

All seven agents remain portable across supported generated clients.

## Planning

Planning is mediated by the main agent. Each `squad-planner` invocation receives the request, repository evidence, settled decisions, prior answers, and open frontier. It returns one numbered round and stops. The main agent presents that round and supplies the answers on the next invocation.

After every decision is settled, the planner returns a shared-understanding confirmation question. Only after the user confirms may a write-mode invocation create `docs/plans/<slug>.md`. Repository evidence, citations, decisions, and rejected alternatives stay in that plan; no separate research artifact is created.

## Review and verdicts

For a planned change, the main agent runs correctness and simplification reviewers together, adding security when a trust boundary changed. It then sends completed reports, verifier evidence, round, plan path, and the recorded security decision to the orchestrator.

`pass` requires adequate evidence for every criterion and no blocking merged finding. `high` or `medium` findings produce `fix`; a plan defect produces `replan`. At most two fix rounds are allowed.

`/squad-review` uses the same conditional reviewers for a PR, uncommitted work, or a diff versus main, but has no plan, verifier evidence, verdict, or fix round. At the end it asks once: Save report as markdown? Yes writes `docs/reviews/<slug>.md` and still shows the findings in the IDE/CLI. No stays IDE/CLI only. Never `docs/decisions.md`.

## Stack and workspace

[`pack-check`](../pack-check/README.md) detects every registered stack, then selects the stacks
applicable to the target paths, diff, and acceptance criteria. A mixed change loads both .NET and
Rust slots; a single-stack change does not load or request the unrelated pack. A full loop records
the applicable plugin standards and concrete repository conventions in the plan. The
[language-pack contract](../../docs/ADD-LANGUAGE-PACK.md) defines the required skill names.

## Provider support

| | Skill | Glob-scoped rule | Agents | Command |
|---|---|---|---|---|
| Claude | yes | no | yes | yes |
| Cursor | yes | yes | yes | yes |
| GitHub Copilot | yes | no | yes | `/squad:run` |
| Codex | yes | no | manual copy | — |

The scoped-rule limitation is intentional. Cursor is the only target that can carry the rule's glob contract through plugin packaging; generation emits the documented portability warning for the other clients instead of making the checklist always-on.

Codex agent files are generated for manual copy:

```shell
cp plugins/squad/com.openai.codex/agents/*.toml .codex/agents/
```

## Editing

Authored: `plugin.json`, `mcp.json` (empty scaffold), `skills/`, `rules/`, `agents/`, and `commands/`.

Generated only in validation output or on the marketplace branch: client manifests and `com.*` provider trees.

Portable model tiers are authored on each agent and mapped in [`models.source.json`](../../models.source.json).
Default `inherit`. The implementer is `standard`; other squad agents are `fast`.

Test changes on a feature branch without merging to `main`:
[ADD-SKILL.md — Test a skill locally](../../docs/ADD-SKILL.md#test-a-skill-locally).

See [ADD-SKILL.md](../../docs/ADD-SKILL.md), [ADD-HOOK.md](../../docs/ADD-HOOK.md), [ADD-AGENT.md](../../docs/ADD-AGENT.md), and [ADD-RULE.md](../../docs/ADD-RULE.md).
