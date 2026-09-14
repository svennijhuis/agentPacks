---
name: typescript-test-patterns
description: When writing or running tests in a TypeScript repository.
title: TypeScript test patterns
language: TypeScript
commands: shape, boundary, and commands
---

Good: the repo's `test` script plus an edge case; filtered file while implementing.
Bad: a happy-path-only file marked as coverage; full suite every TDD cycle.
