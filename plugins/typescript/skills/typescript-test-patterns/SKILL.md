---
name: typescript-test-patterns
description: When writing or running tests in a TypeScript repository.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# TypeScript test patterns

Loop-only; not as a user entrypoint. How a test is written *in this stack*. What deserves a test is not a TypeScript question.

When loaded by exact Skill tool name `typescript-test-patterns` during implement or verify:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `testing.md`.
4. Cite `testing.md` when choosing a runner or command.

Read [shape, boundary, and commands](references/commands.md).

Concrete cases: [unit](references/examples/unit.md), [integration](references/examples/integration.md), [deployed-smoke](references/examples/deployed-smoke.md).

Good: the repo's `test` script plus an edge case; filtered file while implementing.
Bad: a happy-path-only file marked as coverage; full suite every TDD cycle.
