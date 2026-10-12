# Testing (shared)

Cross-language. Language packs add runner, framework, and layout rules on top.

## Rules

- Use the repository's runner, framework, and repeated test patterns for choices a standard leaves open.
- A behavior change includes a test that fails without the change. Happy-path-only coverage is rejected.
- Test observable behavior and error contracts, not private implementation details.
- Arrange, act, assert: set up, make one call to the subject, then assert. Keep the three parts visible and in that order.
- One reason to fail: each test proves one behavior, so a red test names the broken rule.
- Do not unit-test that a markdown, skill, README, or docs file contains a phrase. Wording is reviewed. Parsers that consume markdown as structured input (tables, frontmatter, logs) get fixture tests of parse and evaluate behavior. Generators and validators are tested as code.
- Inject time, randomness, and process boundaries. Replace sleeps, wall-clock assertions, and timing tolerances with deterministic control.

## Test behavior, not implementation

- A test calls the code as its users do and asserts the exact result they see. Delete or rewrite a test that would still pass if the subject returned a wrong value.
- Banned shapes: loose assertions (`toBeDefined`, `toBeTruthy`, `not.toThrow`, `Assert.NotNull` alone, `> 0`), call-only or absence-only checks (`toHaveBeenCalled`, `Received()` without arguments, empty-list equality), expected values computed by the subject, restated constants or config defaults, and assertions on data the test built without calling the subject.
- Fix: one concrete input, the literal output or effect. For a mock, assert the payload it received. For an absence, also assert the presence on another input.
- No test beats a bad test. When a failing test first is not cheap, use the closest executable check and say so.
