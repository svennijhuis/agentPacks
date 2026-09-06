---
name: rust-test-patterns
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. How tests are written and run in a Rust repository — unit, integration and doc tests, async runtimes, feature matrices, Cargo and nextest commands.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# Rust test patterns

How a test is written *in this stack*. What deserves a test at all is a separate question, and it is
not a Rust one.

When loaded by exact Skill tool name `rust-test-patterns` during implement or verify:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `testing.md`.
4. Cite `testing.md` when choosing a runner, boundary, or command.

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

Integration tests are separate crates and cannot access private items. Concrete commands:
[unit](references/examples/unit.md), [integration](references/examples/integration.md).

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

Nextest does not replace documentation tests. Do not invent `--all-features` when the manifest
permits incompatible combinations.

Good: `cargo test -p <package> --test <integration-target>` for a real boundary.
Bad: a happy-path-only unit test marked as coverage.
