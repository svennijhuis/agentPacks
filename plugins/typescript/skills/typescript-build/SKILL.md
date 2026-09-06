---
name: typescript-build
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. How a TypeScript repository is laid out and built — package manager, tsconfig, and the exact tsc / script commands.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# TypeScript build

Facts before editing. Read the repository files first.

When loaded by exact Skill tool name `typescript-build` during implement or review:
1. Read every file in `references/standards/`.
2. Standards in force: `typescript.md`.
3. Cite `typescript.md` on each edit.

## Find the shape

```bash
rg --files -g 'package.json' -g 'tsconfig*.json' -g 'pnpm-lock.yaml' -g 'package-lock.json' -g 'yarn.lock' -g 'bun.lockb'
rg -n '"packageManager"|"(scripts|type)"' -g 'package.json'
```

| File | Means |
|---|---|
| `pnpm-lock.yaml` | Use `pnpm` |
| `yarn.lock` | Use `yarn` |
| `bun.lockb` | Use `bun` |
| `package-lock.json` | Use `npm` |
| `package.json` `"packageManager"` | That pin wins |

Do not add a lockfile the repo does not have.

## Commands

From the package that owns the change (workspace root unless a `packages/` member is in scope):

```bash
<pm> install
<pm> exec tsc --noEmit -p tsconfig.json
```

If `package.json` already has `build` / `typecheck`, run that script instead:

```bash
<pm> run typecheck
<pm> run build
```

Target the repo's `tsconfig*.json`. Do not pass `--strict` flags the config already owns. Do not introduce Vite/Next/React as a slot — those are frameworks, reached through this skill when the repo already uses them.

Good: `pnpm run typecheck` when that script exists.
Bad: pass `--strict` flags the tsconfig already owns.
