# Architect contract

`squad-architect` is read-only. It returns two sections; the planner writes them into the draft plan.

## When

A full change runs the architect when it touches more than one layer, adds a project or package, or
changes a public API. The planner states the rule that fired in the plan, or writes
`Design: skipped (small)` when none did. Small changes never run it.

## Design

```markdown
## Design
Rule fired: <layers | project or package | public API>
### Options
1. <option>: <shape, seams, cost>
2. <option>: <...>
### Decision
Chosen: <option>. Why: <reason>. Alternatives rejected: <option and reason>.
### Seams and ports
<each port, its owner, and its adapter>
### Diagram
<one mermaid diagram>
### Risks
<risk and the check that catches it>
### Hard decisions
<each hard-to-reverse decision (new layer or boundary, new dependency, storage or API contract change, dropped option) with its ADR link `docs/adr/NNNN-slug.md`, or None>
```

## Map

```markdown
## Map
| Unit | Layer or project | Rule | Source |
|---|---|---|---|
| <new or changed unit> | <where it goes> | <rule text> | <layers.md ## Where new code goes, or skill path> |
| <unit> | MISSING | no rule places it | <what was searched> |
| Accepted ADRs | `docs/adr/` | decisions this change must respect | <ADR files read> |
```

## Prompts

- Model the domain: states and transitions over boolean flags and optional-field bags.
- Redesign from first principles when an option only bolts the new need onto the old shape.
- Sketch types and signatures before code; the caller's view comes first.

## Red flags

Screen every option. A red flag rejects or revises the option.

1. Shallow module: a large interface hiding little behavior.
2. Information leakage: one internal decision appears in several modules.
3. Temporal decomposition: modules split by execution order, not by owned knowledge.
4. Pass-through method: forwards the same arguments and adds nothing.
5. Split ownership: more than one module writes the same state.
6. Two ways to do one task: keep one, move callers, delete the rest.
7. Importable internals: callers can reach internals from outside the module.
8. Hand-synced list: two lists of the same items with nothing failing when they drift.
