# Learnings log

An append-only, human-readable run log for the next `/squad` or `/squad-review`. It lives at
`docs/learnings.md` in the repository being changed.

It is not a second brain. It is not eager memory. It does not rewrite skills.
`docs/decisions.md` is an optional human drop-box, not this log.

## When to read

The orchestrator reads this file first, when it exists, before routing, and **applies** the latest
same-entrypoint entry. A retired `build` heading is the same entrypoint as `/squad`. A retired `review` heading is the same entrypoint as `/squad-review`. Prefer that entry's model tier after a
`pass`. After a `fail` or `stopped`, demote one tier. Prefer its skips only when the result was
`pass`. A skip from a `fail` or `stopped` run is a must-run on the next gate. Do not paste the
whole file into later phases, and do not rewrite skills.

## When to write

After hand-off, append exactly one entry. Never edit or delete an earlier entry. Never auto-generate
a skill from the log.

## Entry shape

```markdown
## 2026-09-05 — /squad

- Entrypoint: squad
- Provider: cursor
- Model tier: inherit
- Agents spun: squad-planner, squad-implementer, squad-verifier, squad-reviewer, squad-simplifier, squad-orchestrator
- Skipped: squad-security-reviewer — no trust boundary changed
- Ran: plan, implement, verify, dual-axis review, merge
- Result: pass
- Handoff themes: keep inherit; small rename skipped planner successfully
- Next tweak: keep inherit; the small rename did not need a planner
```

| Field | Rule |
|---|---|
| Heading | Date (`YYYY-MM-DD`) and the entrypoint (`/squad` or `/squad-review`) |
| Entrypoint | `squad` or `squad-review` |
| Provider | The client that ran: `claude`, `cursor`, `copilot`, or `codex` |
| Model tier | The portable tier that ran: `inherit`, `fast`, `standard`, or `frontier` |
| Agents spun | Exact agent names that were invoked |
| Skipped | Agent or phase plus the reason, or `None` |
| Ran | Phases that actually executed |
| Result | `pass`, `fail`, `stopped`, or `uncommitted hand-off` |
| Handoff themes | One line summarizing concerns/deviations that carried across phases, or `None` |
| Next tweak | One concrete adjustment that cites Result — not a vibe — or `None` |

**Handoff themes** is optional prose authored by the main agent or orchestrator from this run's
reports. It never triggers skill rewrites or a self-improve graph.

A **pass** keeps the recorded tier. A **fail** or **stopped** demotes one tier
(`frontier` → `standard` → `fast` → `inherit`) on the next same-entrypoint run.

Create the file with a one-line title `# Learnings` if it does not exist, then append the entry.
