# ADR contract

Architecture decision records live in `docs/adr/NNNN-<plan-slug>.md` (next free four-digit number,
then the plan's slug) in the repository being changed. Plan, ADR, and retro share the slug, so each
links to the others by name. A second ADR for the same plan adds a suffix: `NNNN-<plan-slug>-<topic>.md`.

## When

Write one only for a hard-to-reverse decision: a new layer or boundary (including a new
`Infrastructure.<Vendor>` project), a new dependency, a storage
or API contract change, or a dropped option. `squad-architect` flags these under
`### Hard decisions` in `## Design`. Everything else stays in the plan.

## Shape (Nygard)

```markdown
# NNNN. <decision as a sentence>

## Status
proposed | accepted | superseded by NNNN-<plan-slug>

## Context
<forces and constraints; link the plan docs/plans/<yyyy-mm-dd>-<slug>.md>

## Decision
<what we will do>

## Consequences
<what becomes easier, harder, and what must now be maintained>
```

Optional: one before/after Mermaid diagram (contract rules in architect-contract.md Diagrams).

## Rules

- The planner writes the ADR with `Status: proposed`, and the plan links it under `### Hard decisions`. Each links the other.
- Confirmation of the plan sets the ADR to `accepted`. After that it is never edited.
- To change a decision, write a new ADR whose Status says it supersedes the old one. The old record changes only its Status line to `superseded by NNNN-<plan-slug>`.
- The planner reads every accepted ADR before planning and flags a plan that contradicts one as an open question.
- The retro may propose a new ADR or a supersede under Improvements. A human writes it.
