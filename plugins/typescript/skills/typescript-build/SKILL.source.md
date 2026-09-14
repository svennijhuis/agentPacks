---
name: typescript-build
description: When typechecking or building a TypeScript repository.
title: TypeScript build
language: TypeScript
intro: Facts before editing. Read the repository files first.
commands: shape and commands
---

Good: `pnpm run typecheck` when that script exists; package-scoped tsc while implementing.
Bad: pass `--strict` flags the tsconfig already owns; clean install every TDD cycle.
