---
name: dotnet-test-patterns
description: When writing or running tests in a .NET repository.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# .NET test patterns

Loop-only; not as a user entrypoint. How a test is written *in this stack*. What deserves a test at all is a separate question, and it is not a .NET one.

When loaded by exact Skill tool name `dotnet-test-patterns` during implement or verify:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `testing.md`.
4. Cite `testing.md` when choosing a fixture, boundary, or command.

Read [shape, boundary, and commands](references/commands.md).

Concrete fixtures and commands: [xunit-unit](references/examples/xunit-unit.md),
[http-integration](references/examples/http-integration.md),
[deployed-smoke](references/examples/deployed-smoke.md).

Good: `IClassFixture` for a shared factory; filtered test while implementing.
Bad: start Testcontainers in the constructor; full-suite every TDD cycle.
