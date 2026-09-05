<!-- Generated from standards/rust.md via standards.source.json. Edit the canonical document, not this copy. -->

# Rust design standards

Use established workspace configuration and repeated local patterns for choices this standard leaves open.

## Ownership and APIs

- Borrow when the callee only needs to observe data; take ownership when it stores, transforms, or consumes it.
- Do not add `clone()` merely to satisfy the borrow checker. Establish which value owns the data and clone only
  where duplication is part of the intended behavior or is demonstrably cheaper than a more complex lifetime.
- Accept the least specific borrowed shape callers naturally have, such as `&str` instead of `&String` and
  `&[T]` instead of `&Vec<T>`.
- Represent domain invariants with enums, newtypes, and constructors that validate them. Avoid booleans and raw
  strings whose valid combinations callers must remember.
- Keep public surface area small. Use `pub(crate)` or private items until another crate is intended to depend on them.
- Treat a public API change as a compatibility decision. Preserve documented behavior and non-exhaustive evolution
  points rather than exposing representation for convenience.

## Traits, dependencies, and resources

- Introduce a trait at a real substitution boundary, not automatically for every concrete type. One production
  implementation and no meaningful test substitute is usually a concrete dependency.
- Prefer standard-library and ecosystem types over wrappers that only rename their complete interface.
- Let RAII own cleanup. Keep guards and handles in the narrowest scope that matches the resource lifetime; use
  explicit cleanup only when failure must be observed before drop.
- Match workspace dependency inheritance, feature declarations, lint configuration, edition, MSRV, and lock-file
  policy before changing a member manifest.

## Unsafe and formatting

- Keep `unsafe` blocks as small as possible. State the safety invariant at the block and validate every caller-facing
  precondition at the safe boundary.
- Do not create a safe wrapper whose internal invariant depends on undocumented caller behavior.
- Use `cargo fmt --all` once after implementation and inspect the diff. Follow repository Clippy configuration;
  do not silence a lint globally to avoid fixing one local case.
