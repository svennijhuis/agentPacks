# Rust version

`rust-toolchain.toml` pins the compiler this workspace builds with. `[workspace.package]` pins edition and MSRV members inherit.

```toml
# rust-toolchain.toml
[toolchain]
channel = "1.85.0"
components = ["rustfmt", "clippy"]
```

```toml
# root Cargo.toml
[workspace.package]
edition = "2021"
rust-version = "1.85"
```

```toml
# crates/api/Cargo.toml
[package]
edition.workspace = true
rust-version.workspace = true
```

Use that toolchain rather than the host default.

Bad: `rustup override` or a member `rust-version` that drifts from the workspace pin. Cite `rust.md`.
