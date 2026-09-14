---
name: rust-build
description: When checking, building, or formatting a Rust repository.
title: Rust build
language: Rust
intro: >-
  The facts an agent needs before it touches a `Cargo.toml`. Read the repository's own files
  first; the common layout is not a guarantee.
commands: shape, commands, and failures
---

Good: `cargo clippy` with the repo's feature set.
Bad: invent `--all-features` the CI does not use.
