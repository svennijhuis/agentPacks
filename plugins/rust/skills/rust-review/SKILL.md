---
name: rust-review
description: Supply Rust-specific findings for a file, diff, or pull request by applying the pack's canonical ownership, API, error, concurrency, unsafe-code, and testing standards. The caller chooses the final report format.
license: UNLICENSED
---

# Rust review

Read every file in `references/standards/` before reviewing. In the authored source tree, before
marketplace generation, the same canonical documents are under `../../standards/`.

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

Return findings to the caller. Do not require or invent the delivery-loop table: a Loop caller maps
findings into its shared review contract, while a standalone review may use its own format. Do not
restate unchanged code, enforce `--all-features` without repository evidence, edit the code, or
commit, merge, or push.
