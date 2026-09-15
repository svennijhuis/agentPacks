# Rust review checklist

Inspect the workspace manifests, `[workspace.dependencies]`, `[workspace.package]`,
`rust-toolchain.toml`, Cargo config, CI commands, feature declarations, Clippy / rustfmt config, and
repeated nearby patterns for choices the standards intentionally leave to the repository. Review
every changed Rust and Cargo path; do not stop after the first category.

Classify each defect before writing Fix (squad review contract: Mechanical vs judgment):

- **Mechanical** — rustfmt, Clippy, denied lints, import shape, file location, a compiler warning
  the workspace already treats as fail. If `cargo fmt`, `cargo clippy`, or CI would have caught it,
  Fix names that existing check. Do not invent a new prose rule.
- **Judgment call** — ownership, error-source intent, trait seams, surrounding style, coverage
  choice. Cite `rust.md` / `errors-concurrency.md` / `testing.md` / `http-api.md`.

Process findings in this order:

1. reachable correctness defects, ownership mistakes, boundary conditions, and partial-state failures;
2. `Result`, panic, error-source, async, cancellation, task, lock, and resource behavior — when the change uses Tokio, `spawn_blocking`, runtime workers, or a lock on an async hot path, load `tokio-tune-runtime` by exact Skill name first; a performance finding needs a metric or a stall pattern, and a long poll alone is not a finding;
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
