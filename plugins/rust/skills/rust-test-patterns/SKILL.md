---
name: rust-test-patterns
description: How tests are written and run in a Rust repository — unit, integration and documentation tests, async runtimes, parallel isolation, feature matrices, Cargo and nextest commands. Use when adding or changing a Rust test, choosing its boundary, or verifying a Cargo workspace.
license: UNLICENSED
---

# Rust test patterns

How a test is written *in this stack*. What deserves a test at all is a separate question, and it is
not a Rust one.

Before writing or reviewing tests, read every file in `references/standards/`. In the authored source
tree, before marketplace generation, the same canonical documents are under `../../standards/`.

## Find the shape before writing

```bash
rg --files -g 'Cargo.toml' -g 'tests/**' -g '*.rs' -g '.config/nextest.toml' -g 'nextest.toml'
rg -n '#\[(test|tokio::test|async_std::test)|cfg\(test\)|dev-dependencies|nextest' -g '*.rs' -g 'Cargo.toml' -g '*.toml'
rg -n 'cargo (test|nextest)' .github .gitlab-ci.yml azure-pipelines.yml Makefile justfile 2>/dev/null
```

Match the installed runtime and runner. A workspace using ordinary libtest does not need nextest,
and a crate using async-std does not want its first Tokio test because an example used it.

## Choose the boundary

| Test | Lives in | Use for |
|---|---|---|
| Unit | `#[cfg(test)] mod tests` beside the module | Private logic, invariants, focused branches |
| Integration | `<crate>/tests/*.rs` | The crate's public API and composed dependencies |
| Documentation | Rustdoc code blocks on public items | A public example that must keep compiling and behaving as documented |
| System/end-to-end | Repository-specific harness | Processes, services, real protocols, migrations, or native boundaries |

Integration tests are separate crates and cannot access private items. Do not make an implementation
item public only to test it; test through the public behavior or keep a focused unit test beside it.

## Failures, async work, and isolation

A fallible setup can return `Result`:

```rust
#[test]
fn parses_valid_input() -> Result<(), Box<dyn std::error::Error>> {
    let value = parse("42")?;
    assert_eq!(value, 42);
    Ok(())
}
```

When failure is the behavior under test, inspect the error instead of propagating it with `?`.
Use `#[should_panic(expected = "...")]` only for an intentional panic contract.

Use the async test attribute already supplied by the crate's runtime. Avoid real sleeps: control time
with the runtime's test facilities or inject a clock. Tests in one binary run in parallel by default,
so give files, ports, databases, environment, and global state unique ownership; serialize only the
smallest unavoidable shared group.

## Running them

Use repository CI commands when present. Otherwise, from the workspace root:

```bash
cargo fmt --all -- --check
cargo test --workspace --no-fail-fast
```

`cargo test` covers unit, integration, and library documentation tests by default. Use narrow commands
while iterating, then run the workspace command:

```bash
cargo test -p <package> <test-name>
cargo test -p <package> --test <integration-target>
cargo test -p <package> --doc
```

When the repository configures nextest, use its checked-in profile and the equivalent workspace scope:

```bash
cargo nextest run --workspace
cargo test --workspace --doc
```

Nextest does not replace documentation tests. Run the feature and target combinations established by
CI; do not invent `--all-features` when the manifest permits incompatible combinations.
