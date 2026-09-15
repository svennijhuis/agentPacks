---
name: rust-review
description: When reviewing Rust files, diffs, or Cargo changes.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# Rust review

Loop-only; not as a user entrypoint.

When loaded by exact Skill tool name `rust-review` during review or build:
1. Read every file in `references/standards/`.
2. Standards in force: `rust.md`, `errors-concurrency.md`, `testing.md`, `http-api.md`.
3. Cite the document filename on each finding (`rust.md`, not "the Rust standard").
4. When the change uses Tokio, `spawn_blocking`, runtime workers, or a lock on an async hot path, load `tokio-tune-runtime` by exact Skill name before writing async performance findings. A performance finding needs a metric or a stall pattern; a long poll alone is not a finding.

Process findings per [the checklist](references/checklist.md).

Good: cite `errors-concurrency.md` on a dropped `Result`.
Bad: "clippy is clean" with no location or standard.
