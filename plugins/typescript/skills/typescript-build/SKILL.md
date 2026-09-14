---
name: typescript-build
description: When typechecking or building a TypeScript repository.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# TypeScript build

Loop-only; not as a user entrypoint. Facts before editing. Read the repository files first.

When loaded by exact Skill tool name `typescript-build` during implement or review:
1. Read every file in `references/standards/`.
2. Standards in force: `typescript.md`.
3. Cite `typescript.md` on each edit.

Read [shape and commands](references/commands.md).

Good: `pnpm run typecheck` when that script exists; package-scoped tsc while implementing.
Bad: pass `--strict` flags the tsconfig already owns; clean install every TDD cycle.
