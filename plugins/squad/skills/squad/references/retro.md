# Retro

One retro file per run at `docs/retros/<yyyy-mm-dd>-<slug>.md`, the same date and slug as the plan.
`squad-retro` writes it at hand-off. A human applies it. It never rewrites a skill, standard, or contract.

## Inputs

Read only this run's files: the plan (`## Grill log`, `## Design`, `## Map`, `## Status`,
`## Fix list`, `## Handoff notes`), the verifier report, the merged review, and the hand-off summary.
List each user interruption the main agent passes in. Do not scrape older retros or chat transcripts.

## Shape

```markdown
# Retro: <plan title>

Entrypoint: /squad | /squad-review
Result: pass | fail | stopped | uncommitted hand-off
Plan: docs/plans/<yyyy-mm-dd>-<slug>.md

## End log
<phases that ran, agents spawned, skipped agents and why, fix rounds>

## Interruptions
<each time the user stopped, corrected, or redirected the run, or None>

## Design misses
<where the built change left the chosen Design option or Map, or None>

## Improvements
| Lesson | Enforcer | Change | Owner |
|---|---|---|---|
| <what went wrong> | architecture, type, lint, test, or doc | <environment change> | human |

## Backlog
<lessons a lint or check can enforce, written as the check to add>
```

## Enforcer ladder

Pick the highest enforcer that can stop the lesson from recurring:
architecture > type > lint > test > doc. A `doc` row needs a reason no higher enforcer fits.

Every Improvements row names an environment change: a lint rule, a standard line, a contract line,
or a check. A code fix alone is not an improvement. A lesson a lint can catch goes to `## Backlog`
as the lint to add, not as prose.

## Marketplace smoke entry shape

```markdown
## YYYY-MM-DD marketplace

- Client:
- Host:
- Action:
- Plugins:
- Result: ok / fail
- Next:
```

A pack/skill/agent/command/contract proposal still goes in [suggestions](suggestions.md), human-apply only.
