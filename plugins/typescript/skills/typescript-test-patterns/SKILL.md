---
name: typescript-test-patterns
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. How tests are written and run in a TypeScript repository — Vitest, Jest, node:test, and the exact test command.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# TypeScript test patterns

How a test is written *in this stack*. What deserves a test is not a TypeScript question.

When loaded by exact Skill tool name `typescript-test-patterns` during implement or verify:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `testing.md`.
4. Cite `testing.md` when choosing a runner or command.

## Find the shape

```bash
rg --files -g 'package.json' -g 'vitest.config.*' -g 'jest.config.*' -g 'node:test'
rg -n '"vitest"|"jest"|"node:test"' -g 'package.json'
```

| Signal | Runner | Command |
|---|---|---|
| `vitest` in deps or `vitest.config.*` | Vitest | `<pm> exec vitest run` |
| `jest` in deps or `jest.config.*` | Jest | `<pm> exec jest` |
| `node:test` imports | Node test | `node --test` |
| `"test"` script | That script | `<pm> run test` |

A `"test"` script wins when it already encodes the runner. Do not add Vitest to a Jest repo.

Concrete cases: [unit](references/examples/unit.md), [integration](references/examples/integration.md), [deployed-smoke](references/examples/deployed-smoke.md).

## Boundary

| | Unit | Integration |
|---|---|---|
| Lives | `*.test.ts` / `*.spec.ts` beside the module, if the repo does | `tests/` or the repo's existing folder |
| Touches | One module, collaborators faked | Real I/O, HTTP, or a composed process |

## Running

```bash
<pm> run test
```

If there is no script, use the runner table above. Report the command and its output. A green happy-path-only file is not coverage.

Good: the repo's `test` script plus an edge case.
Bad: a happy-path-only file marked as coverage.
