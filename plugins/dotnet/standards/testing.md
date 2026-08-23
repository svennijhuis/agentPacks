# .NET testing standards

Use the test framework and repeated test patterns already evidenced by the project for choices this standard leaves open.

- Put branching logic under focused unit tests and composition, routing, migrations, database
  behavior, and dependency wiring under integration tests.
- Match the installed framework and its major version. In particular, xUnit v2 `IAsyncLifetime`
  methods return `Task`, while xUnit v3 uses `ValueTask`; inspect package references before copying a
  fixture signature.
- Share expensive hosts, databases, and containers at the narrowest fixture lifetime that preserves
  isolation. Do not start one container per test.
- Prefer a real provider in integration tests when an in-memory substitute changes constraints,
  migrations, transactions, or query semantics.
- Test observable behavior. A behavior change includes a test that fails without the change.
- Inject time and randomness. Replace sleeps and wall-clock assertions with deterministic control.
