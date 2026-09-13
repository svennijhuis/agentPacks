---
name: rust-test-patterns
description: When writing or running tests in a Rust repository.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# Rust test patterns

Loop-only; not as a user entrypoint. How a test is written *in this stack*. What deserves a test at all is a separate question, and it is
not a Rust one.

When loaded by exact Skill tool name `rust-test-patterns` during implement or verify:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `testing.md`.
4. Cite `testing.md` when choosing a runner, boundary, or command.

Read [shape, boundary, and commands](references/commands.md).

Concrete commands:
[unit](references/examples/unit.md), [integration](references/examples/integration.md), [deployed-smoke](references/examples/deployed-smoke.md).

Good: `cargo test -p <package> --test <integration-target>` for a real boundary.
Bad: a happy-path-only unit test marked as coverage; full workspace every TDD cycle.
