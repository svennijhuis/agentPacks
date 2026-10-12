# .NET testing standards

Shared rules live in `testing-principles` (behavior tests, docs-phrase rule, injected time). This file adds .NET specifics.

- Put branching logic under focused unit tests and composition, routing, migrations, database
  behavior, and dependency wiring under integration tests.
- Match the installed framework and its major version. In particular, xUnit v2 `IAsyncLifetime`
  methods return `Task`, while xUnit v3 uses `ValueTask`; inspect package references before copying a
  fixture signature.
- Share expensive hosts, databases, and containers at the narrowest fixture lifetime that preserves
  isolation. Do not start one container per test.
- Prefer a real provider in integration tests when an in-memory substitute changes constraints,
  migrations, transactions, or query semantics.
