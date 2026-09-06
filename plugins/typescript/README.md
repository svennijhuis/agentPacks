# typescript

The TypeScript language pack for [Squad](../squad/README.md). It supplies the
build, test, and review skills that Squad discovers by exact name.

Slot skills are Squad internals. Do not run them directly — Squad loads each by exact Skill name.

## What is in it

| Skill | Used by | Purpose |
|---|---|---|
| `typescript-build` | implementer, simplifier | Internal loop skill. Package manager, `tsconfig`, typecheck and build commands |
| `typescript-test-patterns` | implementer, verifier | Internal loop skill. Vitest / Jest / `node:test` and the exact test command |
| `typescript-review` | correctness reviewer | Internal loop skill. Types, modules, promises, and test-boundary mistakes |

No `<lang>-security-review`. OWASP stays the floor. No framework slot skills (`react-*`, `next-*`).

Canonical standards live once under `standards/`:

- `typescript.md` — strictness, `unknown` vs `any`, modules, exhaustiveness, promises.
- `testing.md` — runner match, behaviour coverage, isolation.

`standards.source.json` maps each document to the skills that need it.

## Editing this pack

Authored: `plugin.json`, `standards.source.json`, `standards/`, and `skills/`.
No shipped MCP server.

See [ADD-LANGUAGE-PACK.md](../../docs/ADD-LANGUAGE-PACK.md).
