# Rust error and concurrency standards

Use established workspace dependencies and repeated local patterns for choices this standard leaves open.

## Errors and panic boundaries

- Return `Result` for anticipated runtime failure and `Option` when absence is a normal state. Reserve panic for
  violated internal invariants, process-startup conditions that cannot be recovered, tests, and unreachable states
  that are justified by construction.
- Do not use `unwrap` or `expect` on a reachable fallible production path unless the surrounding invariant makes
  failure impossible and the message states that invariant.
- Preserve the underlying error as a source when adding context or translating at a boundary. Do not flatten a
  diagnostic chain into an unstructured string before the reporting boundary.
- Keep library errors typed and useful to callers. Application boundaries may add operational context and choose
  a reporting format without leaking secrets or implementation details.
- Handle partial progress explicitly. A failure after mutation, I/O, or a multi-step write either rolls back,
  records resumable state, or documents the committed prefix.

## Async and concurrency

- Do not call blocking filesystem, network, process, or synchronization APIs on an async executor thread when the
  runtime provides a blocking boundary. Match the runtime already used by the workspace.
- Do not hold a synchronous mutex or read/write guard across `.await`. Narrow the guard scope, move owned data out,
  or use the runtime's async primitive when the lock must span suspension.
- Treat cancellation as a drop at any await point. Do not leave shared state half-updated when a future is cancelled.
- Make task ownership explicit. Await spawned work, return its handle, or attach it to a supervised lifetime; detached
  work must have an intentional error-reporting and shutdown path.
- Add `Send`, `Sync`, and `'static` bounds only where the execution boundary requires them. Do not spread bounds
  through APIs to repair one spawning site.
- Avoid blocking channels, sleeps, and wall-clock coordination in concurrent code when a runtime-aware primitive or
  deterministic signal expresses the dependency.
