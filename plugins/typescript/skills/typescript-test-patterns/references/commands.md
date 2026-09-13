# TypeScript test commands

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

### Targeted verify first

Prefer file or name filters before the full suite:

```bash
<pm> exec vitest run path/to/file.test.ts
<pm> exec jest path/to/file.test.ts
<pm> run test
```

Implement iterates on the narrow command. Verifier runs the criterion command, then one wider suite.

### Contention constraints

- Avoid overlapping full-suite runs on the same tree from concurrent agents.
- Do not invent a shared coordination lock file; sequential targeted runs are enough.
