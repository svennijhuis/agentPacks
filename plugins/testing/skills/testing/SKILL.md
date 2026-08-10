---
name: testing
description: Write and evaluate automated tests — what deserves a test, how to structure it, and when a test is worth deleting. Use when adding tests, reviewing test coverage, or deciding whether a change needs one.
license: UNLICENSED
---

# Testing

## What deserves a test

Test behaviour that could break and would matter if it did:

- Every branch of non-trivial conditional logic.
- Boundaries: empty, one, many, maximum, and one past the maximum.
- Error paths, not only the happy path. An untested `catch` is an untested branch.
- Every bug you fix — the regression test is the proof the fix works.

Do not test: language features, third-party libraries, or getters that only return a field. Those tests cost maintenance and catch nothing.

## Structure

- One behaviour per test. If the name needs "and", split it.
- Name the case, not the method: `Rejects_expired_token`, not `TestValidate`.
- Arrange / act / assert, in that order, with the act step a single call.
- Assert on the outcome the caller cares about. Asserting on internal calls locks the test to the implementation and it will break on every refactor.

## Test data

- Build inputs with a helper that takes only the fields the test cares about; defaults for the rest. A test that sets twelve fields hides which one matters.
- No shared mutable fixtures between tests. Order-dependent suites fail in CI and pass locally.
- Prefer real values over mocks for anything cheap and deterministic. Mock at process boundaries: network, clock, filesystem, randomness.

## Failure quality

A failing test must say what broke without a debugger. Assert on values, not booleans — `Assert.Equal(expected, actual)` prints both sides; `Assert.True(a == b)` prints nothing useful.

## When to delete a test

Delete it when it asserts on a removed requirement, duplicates another test exactly, or has been skipped for more than one release. A permanently skipped test is worse than no test: it looks like coverage.
