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
- Do not unit-test that a markdown, skill, README, or docs file contains a phrase. Wording is reviewed. Parsers that consume markdown as structured input (tables, frontmatter, logs) get fixture tests of parse and evaluate behavior. Generators and validators are tested as code.
- Inject time and randomness. Replace sleeps and wall-clock assertions with deterministic control.
