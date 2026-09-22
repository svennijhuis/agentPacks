---
name: rust-build
description: When checking, building, or formatting a Rust repository.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# Rust build

Loop-only; not as a user entrypoint. The facts an agent needs before it touches a `Cargo.toml`. Read the repository's own files first —
the common layout is not a guarantee.

When loaded by exact Skill tool name `rust-build` during implement or review:
1. Read every file in `references/standards/`.
2. Read every file in `references/examples/`.
3. Standards in force: `rust.md`, `errors-concurrency.md`, `http-api.md`.
4. Cite the document filename on each edit (`rust.md`, not "the Rust standard").
5. When the change uses Tokio or runtime work (`spawn_blocking`, worker threads, locks on an async hot path), load `tokio-tune-runtime` by exact Skill name.

Read [shape, commands, and failures](references/commands.md).

Concrete pins: [workspace-crates](references/examples/workspace-crates.md),
[toolchain-version](references/examples/toolchain-version.md).

Good: crate versions in `[workspace.dependencies]`; compiler in `rust-toolchain.toml`.
Bad: version a crate in a member when the workspace inherits; invent `--all-features` the CI does not use.
