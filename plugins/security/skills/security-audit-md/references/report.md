# Report

Write `docs/security-audit/<slug>.md`. Slug from the repo name and date. Same content in the IDE/CLI.

```markdown
# Security audit — <slug>

Target: <repo root>
Scope: repo | path | diff
Profile: quick | standard
Source: <git rev-parse --short HEAD>, dirty | clean

## Architecture

<recon summary>

## Findings

| Verdict | Severity | Location | Problem | Fix |
|---|---|---|---|---|
| confirmed | high | `path:line` | Boundary + attack + result | Narrowest source change |
| needs_validation | — | `path:line` | Source-grounded hypothesis | Exact unresolved fact |

No confirmed or needs-validation rows → `No findings.` plus what recon examined.

## Rejected

One line per disproved candidate: location, claimed attack, why it fails. Omit if none.

## Coverage

In-scope classes and whether a hunter ran. Out of scope named, never implied covered. One pass is partial.
```

## Field rules

| Field | Rule |
|---|---|
| Verdict | `confirmed` or `needs_validation` in Findings. `rejected` only under Rejected. |
| Severity | `high`, `medium`, or `low` on `confirmed` only. Never on `needs_validation`. |
| Location | `path:line`, or `path` for a whole-file finding. |
| Problem | Actor, control, crossed boundary, attack, result. One sentence. |
| Fix | Imperative, at the last trusted decision point. The audit does not apply it. |
| Confidence | Report only ≥ 80. Do not emit a confidence column. |

`high` — unauthenticated takeover, cross-tenant read/write, or code execution.
`medium` — a real boundary cross with limited blast radius or uncommon preconditions.
`low` — confirmed, minimal impact.

Good: `confirmed` `high` `auth/token.ts:40` — unauthenticated caller reads any tenant by swapping `tenantId`.
Bad: severity on `needs_validation`; "add auth" with no location; live exploit steps against production.
