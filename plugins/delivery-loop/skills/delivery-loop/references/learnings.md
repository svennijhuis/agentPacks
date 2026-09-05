# Learnings log

An append-only, human-readable run log for the next `/build` or `/review`. It lives at
`docs/learnings.md` in the repository being changed.

It is not a second brain. It is not eager memory. It does not rewrite skills.

## When to read

The orchestrator reads this file first, when it exists, before routing. Use the latest entries to
see which agents last paid off, which were skipped and why, and the recorded next tweak. Do not
paste the whole file into later phases.

## When to write

After hand-off, append exactly one entry. Never edit or delete an earlier entry. Never auto-generate
a skill from the log.

## Entry shape

```markdown
## 2026-09-05 — /build

- Entrypoint: build
- Model tier: inherit
- Agents spun: loop-planner, loop-implementer, loop-verifier, loop-reviewer, loop-simplifier, loop-orchestrator
- Skipped: loop-security-reviewer — no trust boundary changed
- Ran: plan, implement, verify, dual-axis review, merge
- Result: pass
- Next tweak: keep inherit; the small rename did not need a planner
```

| Field | Rule |
|---|---|
| Heading | Date (`YYYY-MM-DD`) and the entrypoint (`/build` or `/review`) |
| Entrypoint | `build` or `review` |
| Model tier | The portable tier that ran: `inherit`, `fast`, `standard`, or `frontier` |
| Agents spun | Exact agent names that were invoked |
| Skipped | Agent or phase plus the reason, or `None` |
| Ran | Phases that actually executed |
| Result | `pass`, `fail`, `stopped`, or `uncommitted hand-off` |
| Next tweak | One concrete adjustment for the next run, or `None` |

Create the file with a one-line title `# Learnings` if it does not exist, then append the entry.
