# Rust build commands

## Find the shape before building

```bash
rg --files -g 'Cargo.toml' -g 'Cargo.lock' -g 'rust-toolchain*' -g '.cargo/config*'
rg -n '^\[(workspace|package|workspace.dependencies|workspace.lints|features)' -g 'Cargo.toml'
rg -n 'cargo (check|build|test|nextest|clippy|fmt)' .github .gitlab-ci.yml azure-pipelines.yml Makefile justfile 2>/dev/null
```

| File | What it means |
|---|---|
| Root `Cargo.toml` with `[workspace]` | Commands must respect members, default members, inherited dependencies, features, and lints |
| `[workspace.dependencies]` | **Crate versions live here.** Members use `{ workspace = true }` |
| `[workspace.package]` | Shared `edition`, `rust-version` (MSRV), license, and similar metadata members inherit |
| `rust-toolchain.toml` / `rust-toolchain` | The compiler this repo builds with (channel, targets, rustfmt, Clippy); use it rather than the host default |
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

### Targeted verify first

Prefer the changed package before a full workspace check:

```bash
cargo check -p <package> --all-targets
cargo check --workspace --all-targets
```

During implement, check the touched crate first. Reserve `--workspace` for verifier / CI-shaped
checks. Do not `cargo clean` mid-loop unless diagnosing a corrupted target directory — cleaning
under contention forces full rebuilds and disk thrash.

### Contention constraints

- Cargo holds a package lock under `target/`; avoid parallel full workspace builds on the same tree.
- Do not delete `target/` while another agent may be compiling.
- Do not invent a shared coordination lock file; sequential targeted `-p` checks are enough.

## Workspace crate versions

When the root `Cargo.toml` has `[workspace.dependencies]`:

1. Add or bump the version and shared features there.
2. The member lists `foo = { workspace = true }` — **no version of its own.**
3. Inspect the `Cargo.lock` diff and reject unrelated upgrades.

A version in the member when the workspace already inherits is a second source of truth, not an
override you want. Preserve registry, git, path, default-feature, and target-specific choices
already made nearby.

Prefer the repository's existing edit path. When `cargo add` writes the workspace form the repo
already uses:

```bash
cargo add -p <package> <dependency>
```

When adding a second crate to a repo that is not yet a workspace, introduce `[workspace]`,
`[workspace.dependencies]`, and `[workspace.package]` at the root rather than copying versions
into both manifests. Do not convert a single-crate repo that has never used a workspace.

See [workspace-crates](examples/workspace-crates.md).

## Pinning the Rust version

Two pins, both at the workspace root:

| File | What it pins |
|---|---|
| `rust-toolchain.toml` | The compiler this repo builds with (channel, rustfmt, Clippy, targets) |
| `[workspace.package] rust-version` | MSRV members inherit |

Use that toolchain rather than the host default. Members set `rust-version.workspace = true` and
`edition.workspace = true`. Do not `rustup override` or restate `rust-version` in a member.

When adding a workspace that has no toolchain file yet, add `rust-toolchain.toml` at the root.
MSRV and the toolchain channel may differ (MSRV older than the pin); match both files the repo
already has.

See [toolchain-version](examples/toolchain-version.md).

## Adding a workspace member

Add a new member through the root `[workspace].members` shape. Inherit edition, `rust-version`,
crate versions, lints, and directory naming rather than restating root configuration.

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
| Member crate carries its own version | Workspace inherits; the version belongs in `[workspace.dependencies]` |
| Lock file needs update under `--locked` | Manifest changed without the intended lock-file update |
| Toolchain/component unavailable | The pinned channel, target, rustfmt, or Clippy component is not installed |
| Linker error after `cargo check` passes | Native dependency, target, linker, or feature configuration differs at build time |
| Failure only with `--all-features` | The repository's features are not intended to be enabled together |
