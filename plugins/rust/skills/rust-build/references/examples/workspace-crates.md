# Workspace crates

Root `Cargo.toml` `[workspace.dependencies]` is the version home. Every member can use the crate.

```toml
# root Cargo.toml
[workspace]
members = ["crates/api", "crates/domain"]

[workspace.dependencies]
serde = { version = "1", features = ["derive"] }
tokio = { version = "1", features = ["macros", "rt-multi-thread"] }
```

```toml
# crates/api/Cargo.toml
[dependencies]
serde = { workspace = true }
tokio = { workspace = true }
```

Bad: `serde = "1"` in a member when the workspace already inherits. Cite `rust.md`.
