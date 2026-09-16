# Suggestions log

An append-only list of **human-reviewed** pack/skill/contract change proposals. It lives at
`docs/suggestions.md` in the repository being changed, next to `docs/learnings.md`.

It is not a second brain. It is not applied on the next `/squad`. It does not rewrite skills.
`docs/learnings.md` owns gate advice (tiers, skips). This file owns “change that file” proposals.

## When to read

Do not read it to gate, skip, demote, or rewrite. A human may paste one entry as the next
`/squad` request. The orchestrator never applies an entry on its own.

## When to write

After the learnings entry, at hand-off. Append **zero or one** entry. Write only when this run
produced evidence that a **pack, skill, agent, command, or contract** should change. Product-code
follow-ups stay in the plan handoff notes or residual fixup — not here.

Never edit or delete an earlier entry. Never open a skill and apply the suggestion. `None` means
do not append.

## Entry shape

```markdown
## 2026-09-15 — /squad

- Entrypoint: squad
- Target: plugins/squad/skills/squad/references/review-contract.md
- Kind: contract
- Evidence: verifier returned blocked on the secret path; merge still spent a fix round
- Suggestion: treat blocked as unproven on the verdict table; do not count it as fail
- Expected effect: a skipped secret path hands off instead of burning a fix round
- Apply: human
```

| Field | Rule |
|---|---|
| Heading | Date (`YYYY-MM-DD`) and the entrypoint (`/squad` or `/squad-review`) |
| Entrypoint | `squad` or `squad-review` |
| Target | One repo path that would change, or `None` |
| Kind | `skill` / `contract` / `agent` / `command` / `pack-doc` |
| Evidence | What this run showed — a report line, criterion, or command — not a vibe |
| Suggestion | One concrete edit, imperative, small enough for a later `/squad` |
| Expected effect | What the next run should do differently if a human applies it |
| Apply | Always `human`. Any other value is invalid |

Create the file with a one-line title `# Suggestions` if it does not exist, then append the entry.

Good: one evidenced contract edit; `Apply: human`.
Bad: rewrite a skill from this file; a product-bug dump; a graph of follow-ups.
