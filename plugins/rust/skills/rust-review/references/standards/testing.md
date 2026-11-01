<!-- Generated from standards/testing.md via standards.source.json. Edit the canonical document, not this copy. -->

# Rust testing standards

Shared rules live in `testing-principles` (behavior tests, docs-phrase rule, injected time). This file adds Rust specifics; match the repository's async runtime and feature matrix.

- Keep focused unit tests beside private implementation when they need private access; put public cross-crate behavior
  in `tests/` integration targets and executable documentation examples in doc tests.
- Use `Result`-returning tests when setup or the exercised path is fallible; assert an expected error rather than
  using `?` when the error itself is the behavior under test.
- Match the installed async runtime and its test attribute. Do not add Tokio, async-std, or another executor solely
  because an example used it.
- Assume tests in one binary may run in parallel. Isolate files, ports, environment variables, global state, clocks,
  and external resources, or serialize only the smallest group that truly shares them.
- Exercise the feature combinations and targets the repository's CI supports. Do not infer that `--all-features` is
  valid when features may be mutually exclusive.
- Use `cargo nextest` only when the repository configures it; otherwise preserve Cargo's unit, integration, and doc-test
  coverage with `cargo test`.
