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
2. Standards in force: `rust.md`, `errors-concurrency.md`.
3. Cite the document filename on each edit (`rust.md`, not "the Rust standard").

Read [shape, commands, and failures](references/commands.md).

Good: `cargo clippy` with the repo's feature set.
Bad: invent `--all-features` the CI does not use.
