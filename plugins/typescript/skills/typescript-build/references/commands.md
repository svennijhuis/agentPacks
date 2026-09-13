# TypeScript build commands

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

### Targeted verify first

Prefer the package that owns the change before a monorepo-wide typecheck/build:

```bash
<pm> exec tsc --noEmit -p packages/<name>/tsconfig.json
<pm> run typecheck --filter <name>   # when the monorepo tool supports it
```

During implement, typecheck the touched package first. Reserve full-workspace scripts for verifier /
CI-shaped checks. Do not delete `node_modules/` or build caches mid-loop unless diagnosing corruption.

### Contention constraints

- Avoid overlapping full-monorepo installs/builds on the same tree.
- Prefer one install, then targeted scripts. Do not invent a shared coordination lock file.
