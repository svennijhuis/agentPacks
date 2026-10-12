# ADR contract

Architecture decision records live in `docs/adr/NNNN-slug.md` (next free four-digit number,
kebab-case slug) in the repository being changed.

## When

Write one only for a hard-to-reverse decision: a new layer or boundary, a new dependency, a storage
or API contract change, or a dropped option. `squad-architect` flags these under
`### Hard decisions` in `## Design`. Everything else stays in the plan.

## Shape (Nygard)

```markdown
# NNNN. <decision as a sentence>

## Status
proposed | accepted | superseded by NNNN-slug

## Context
<forces and constraints; link the plan docs/plans/<yyyy-mm-dd>-<slug>.md>

## Decision
<what we will do>

## Consequences
<what becomes easier, harder, and what must now be maintained>
```

## Rules

- The planner writes the ADR with `Status: proposed`, and the plan links it under `### Hard decisions`. Each links the other.
- Confirmation of the plan sets the ADR to `accepted`. After that it is never edited.
- To change a decision, write a new ADR whose Status says it supersedes the old one. The old record changes only its Status line to `superseded by NNNN-slug`.
- The planner reads every accepted ADR before planning and flags a plan that contradicts one as an open question.
- The retro may propose a new ADR or a supersede under Improvements. A human writes it.
