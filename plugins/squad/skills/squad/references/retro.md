# Retro

One retro per full `/squad` run or `/squad-review`, including a run that stopped or failed, at `docs/retros/<yyyy-mm-dd>-<slug>.md` (the
plan's date and slug). A small change gets no retro. `squad-retro` writes the file and may
write nothing outside `docs/retros/`. A human applies it. Nothing rewrites a skill, standard, or contract.

## Catch-up

At step 1 of the next run, if the newest plan in `docs/plans/` is still `Status: draft` and has no
retro with the same date-slug, the main agent spawns `squad-retro` on it first with reason
`interrupted`. The retro's Summary records that reason.

## Inputs

The spawn passes paths, never pasted text: the plan `.md` (with `## Grill log`, `## Design`,
`## Map`, `## Fix list`, `## Handoff notes`), the verifier report, the merged review, the last SHA,
and a short list of user interruptions. Read only those. Do not scrape older retros or transcripts.

## Shape

```markdown
# Retro: <plan title>

## Summary
Entrypoint, result, phases run, agents spawned and skipped, fix rounds, last SHA.

## What went wrong
Each miss with its evidence: failed gates, fix rounds, user interruptions, and design misses
(where the built change left the chosen Design option or Map), and each `Plan gap:` with its question and patch path (a second gap on one slice is the `Replan:`). `None` when clean.

## Lessons
| Lesson | Type | File |
|---|---|---|
| <what to prevent> | architecture, type, lint, test, or doc | <file the enforcer lives in> |

## Improvements
<one line per lesson: the environment change a human should make>
```

A retro has no diagrams.

## Enforcer ladder

Pick the highest type that can stop the lesson from recurring:
architecture > type > lint > test > doc. A `doc` row needs a reason no higher type fits.
A `lint` row names the lint to add, not prose.

Every Improvements line names an environment change: a lint rule, a standard line, a contract line,
a check, or a new or superseding ADR in `docs/adr/` ([ADR contract](adr.md)). A code fix alone is not an improvement.

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
