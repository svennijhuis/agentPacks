# Rust test commands

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

Integration tests are separate crates and cannot access private items.

## Failures, async work, and isolation

Use the async test attribute already supplied by the crate's runtime. Avoid real sleeps. Tests in
one binary run in parallel by default, so give files, ports, databases, environment, and global
state unique ownership; serialize only the smallest unavoidable shared group.

## Running them

Use repository CI commands when present. Otherwise, from the workspace root:

```bash
cargo fmt --all -- --check
cargo test --workspace --no-fail-fast
```

### Targeted verify first

Prefer package or test-target scope before the full workspace suite:

```bash
cargo test -p <package> --lib
cargo test -p <package> --test <integration-target>
cargo test --workspace --no-fail-fast
```

Implement iterates on the narrow command. Verifier runs the criterion command, then one wider suite.
Do not `cargo clean` mid-loop unless diagnosing a corrupted target.

### Contention constraints

- Avoid overlapping full-workspace `cargo test` on the same tree.
- Prefer sequential `-p` runs over inventing a coordination lock file.

Nextest does not replace documentation tests. Do not invent `--all-features` when the manifest
permits incompatible combinations.
