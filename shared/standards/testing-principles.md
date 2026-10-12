# Testing (shared)

Cross-language. Language packs add runner, framework, and layout rules on top.

- Use the repository's runner, framework, and repeated test patterns for choices a standard leaves open.
- A behavior change includes a test that fails without the change. Happy-path-only coverage is rejected.
- Test observable behavior and error contracts, not private implementation details.
- Do not unit-test that a markdown, skill, README, or docs file contains a phrase. Wording is reviewed. Parsers that consume markdown as structured input (tables, frontmatter, logs) get fixture tests of parse and evaluate behavior. Generators and validators are tested as code.
- Inject time, randomness, and process boundaries. Replace sleeps, wall-clock assertions, and timing tolerances with deterministic control.
