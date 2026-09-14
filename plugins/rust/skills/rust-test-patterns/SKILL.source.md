---
name: rust-test-patterns
description: When writing or running tests in a Rust repository.
title: Rust test patterns
language: Rust
commands: shape, boundary, and commands
---

Good: `cargo test -p <package> --test <integration-target>` for a real boundary.
Bad: a happy-path-only unit test marked as coverage; full workspace every TDD cycle.
