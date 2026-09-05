---
name: rust-build
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. How a Rust repository is laid out, checked and built — Cargo workspaces, toolchain pinning, features, lock files, and the exact check, build, run, format and Clippy commands.
license: UNLICENSED
metadata:
  audience: loop
---

# Rust build

The facts an agent needs before it touches a `Cargo.toml`. Read the repository's own files first —
the layout below is the common one, not a guarantee.

When loaded by exact Skill tool name `rust-build` during implement or review:
1. Read every file in `references/standards/` (authored tree: `../../standards/` before generation).
2. Standards in force: `rust.md`, `errors-concurrency.md`.
3. Cite the document filename on each edit (`rust.md`, not "the Rust standard").

## Find the shape before building

```bash
rg --files -g 'Cargo.toml' -g 'Cargo.lock' -g 'rust-toolchain*' -g '.cargo/config*'
rg -n '^\[(workspace|package|workspace.dependencies|workspace.lints|features)' -g 'Cargo.toml'
rg -n 'cargo (check|build|test|nextest|clippy|fmt)' .github .gitlab-ci.yml azure-pipelines.yml Makefile justfile 2>/dev/null
```

| File | What it means |
|---|---|
| Root `Cargo.toml` with `[workspace]` | Commands must respect members, default members, inherited dependencies, features, and lints |
| `rust-toolchain.toml` / `rust-toolchain` | The channel, targets, and components are pinned; use that toolchain rather than the host default |
| `.cargo/config.toml` | Aliases, target, linker, registries, and build settings can change the meaning of ordinary Cargo commands |
| `Cargo.lock` | The resolved dependency graph; follow the repository's tracked-lockfile policy and keep intentional updates narrow |
| CI / `justfile` / Makefile | The real target, feature matrix, runner, lint level, and wrapper commands |

Use `cargo metadata --no-deps --format-version 1` when the root/member relationship is unclear.
Repository evidence selects the command. When it defines none, use these defaults from the workspace root:

```bash
cargo check --workspace --all-targets
cargo build --workspace
cargo run -p <package> --bin <binary> -- <args>
```

Do not add `--all-features` by habit. Some workspaces intentionally define mutually exclusive
features; use the combinations established by CI or the manifest.

## Dependencies and workspace members

Prefer the repository's existing edit path. When `cargo add` is available:

```bash
cargo add -p <package> <dependency>
```

If dependencies are inherited from `[workspace.dependencies]`, add the version and features at the
workspace root and use `{ workspace = true }` in the member. Preserve registry, git, path, default
feature, and target-specific choices already made nearby. Inspect the `Cargo.lock` diff and reject
unrelated upgrades.

Add a new member through the root `[workspace].members` shape. Match its edition, `rust-version`,
workspace-inherited metadata, lints, and directory naming rather than restating root configuration.

## Formatting and Clippy

Format once after implementation:

```bash
cargo fmt --all
```

Inspect the diff so formatting did not expand scope. Verification checks without rewriting:

```bash
cargo fmt --all -- --check
```

Run Clippy only when the pinned toolchain and repository CI use it. Match their features and targets;
when they specify no command, the strict default is:

```bash
cargo clippy --workspace --all-targets -- -D warnings
```

## When Cargo fails

Read the first causal diagnostic and use `rustc --explain <code>` for a compiler code before changing
lifetimes or cloning data blindly.

| Symptom | Usually |
|---|---|
| Package or feature not found | Wrong registry/source, member selection, inherited dependency, or feature name |
| Lock file needs update under `--locked` | Manifest changed without the intended lock-file update |
| Toolchain/component unavailable | The pinned channel, target, rustfmt, or Clippy component is not installed |
| Linker error after `cargo check` passes | Native dependency, target, linker, or feature configuration differs at build time |
| Failure only with `--all-features` | The repository's features are not intended to be enabled together |
