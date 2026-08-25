# rust

The Rust language pack for the [delivery loop](../delivery-loop/README.md). It supplies the build,
test, and review skills that the Loop discovers by exact name.

## What is in it

| Skill | Used by | Purpose |
|---|---|---|
| `rust-build` | implementer, simplifier | Inspect the Cargo workspace and toolchain, build, implement, lint, and format once at the end |
| `rust-test-patterns` | implementer, verifier | Choose the right unit, integration, documentation, async, and feature test boundary |
| `rust-review` | correctness reviewer | Review Rust correctness, ownership, API shape, errors, concurrency, unsafe code, and testability |

Canonical standards live once under `standards/`:

- `rust.md` — ownership, public APIs, types, dependencies, unsafe code, and formatting.
- `errors-concurrency.md` — recoverable errors, panic boundaries, async work, locks, and cancellation.
- `testing.md` — unit, integration, documentation, feature, and concurrent tests.

`standards.source.json` maps each document to the skills that need it. Marketplace generation copies
only those documents into each consumer's `references/standards/` directory. Generated copies carry
a do-not-edit header; edit the canonical source here instead.

## Standards and repository conventions

The pack contains the complete standards source. The Loop records every applied skill and canonical
document under `## Standards in force`; it does not look for another standards directory.

Repositories still choose their workspace shape, feature matrix, async runtime, test runner, lint
configuration, and lock-file policy. The planner inspects `Cargo.toml`, toolchain and Cargo config,
CI commands, directory layout, tests, and nearby code, then records supported patterns under
`## Repository conventions observed` with their evidence paths. Those conventions specialize choices
the pack leaves open while the plugin remains the source of standards.

## Formatting and linting

There is no global after-edit hook. `rust-build` runs `cargo fmt --all` once after implementation;
verification checks with `cargo fmt --all -- --check`. Clippy runs only when the repository toolchain
and CI use it, with the repository's own targets, features, and lint levels. The pack never adds
`--all-features` to a project whose features may be mutually exclusive.

## Editing this pack

Authored: `plugin.json`, `standards.source.json`, `standards/`, and `skills/`.

Generated on the `marketplace` branch or in temporary validation output, never on `main`:
`.cursor-plugin/`, `.codex-plugin/`, `com.anthropic.claude-code/`, `com.openai.codex/`,
`com.github.copilot/`, and each skill's `references/standards/` directory.

See [ADD-LANGUAGE-PACK.md](../../docs/ADD-LANGUAGE-PACK.md) and
[ADD-SKILL.md](../../docs/ADD-SKILL.md).
