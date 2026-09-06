---
name: rust-review
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. Supply Rust-specific findings from the pack's canonical ownership, API, error, concurrency, unsafe-code, and testing standards.
license: UNLICENSED
metadata:
  audience: loop
---

# Rust review

When loaded by exact Skill tool name `rust-review` during review or build:
1. Read every file in `references/standards/` (authored tree: `../../standards/` before generation).
2. Standards in force: `rust.md`, `errors-concurrency.md`, `testing.md`.
3. Cite the document filename on each finding (`rust.md`, not "the Rust standard").

Inspect the workspace manifests, toolchain and Cargo config, CI commands, feature declarations, and
repeated nearby patterns for choices the standards intentionally leave to the repository. Review
every changed Rust and Cargo path; do not stop after the first category.

Process findings in this order:

1. reachable correctness defects, ownership mistakes, boundary conditions, and partial-state failures;
2. `Result`, panic, error-source, async, cancellation, task, lock, and resource behavior;
3. unsafe invariants, public contracts, enum evolution, borrowing, trait seams, and compatibility;
4. unnecessary cloning, allocation, bounds, wrappers, and hand-written standard-library behavior;
5. missing unit, integration, documentation, feature, and concurrency coverage;
6. formatting or Clippy violations backed by repository configuration.

For each finding, supply a precise location, impact-based severity recommendation, defect and cause,
actionable fix, and the canonical standard or repository evidence that supports it. A compiler or
Clippy warning informs the finding; it does not determine severity without reachability and impact.

Return findings to the caller. Do not require or invent the Squad table: a Loop caller maps
findings into its shared review contract, while a standalone review may use its own format. Do not
restate unchanged code, enforce `--all-features` without repository evidence, edit the code, or
commit, merge, or push.

Good: cite `errors-concurrency.md` on a dropped `Result`.
Bad: "clippy is clean" with no location or standard.
