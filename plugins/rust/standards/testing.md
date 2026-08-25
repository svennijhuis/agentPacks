# Rust testing standards

Use the repository's runner, async runtime, feature matrix, and repeated test patterns for choices this standard leaves open.

- Keep focused unit tests beside private implementation when they need private access; put public cross-crate behavior
  in `tests/` integration targets and executable documentation examples in doc tests.
- Test observable behavior and failure contracts. A behavior change includes a test that fails without the change.
- Use `Result`-returning tests when setup or the exercised path is fallible; assert an expected error rather than
  using `?` when the error itself is the behavior under test.
- Match the installed async runtime and its test attribute. Do not add Tokio, async-std, or another executor solely
  because an example used it.
- Assume tests in one binary may run in parallel. Isolate files, ports, environment variables, global state, clocks,
  and external resources, or serialize only the smallest group that truly shares them.
- Inject time, randomness, and process boundaries. Replace sleeps and timing tolerances with deterministic control.
- Exercise the feature combinations and targets the repository's CI supports. Do not infer that `--all-features` is
  valid when features may be mutually exclusive.
- Use `cargo nextest` only when the repository configures it; otherwise preserve Cargo's unit, integration, and doc-test
  coverage with `cargo test`.
