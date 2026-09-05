<!-- Generated from standards/async-errors.md via standards.source.json. Edit the canonical document, not this copy. -->

# Async and error-handling standards

Use established project configuration and repeated local patterns for choices this standard leaves open.

## Async and cancellation

- Return `Task` or `Task<T>` from asynchronous operations. Reserve `async void` for event-handler
  interfaces that require it.
- Accept a `CancellationToken` at public asynchronous boundaries and operations whose underlying
  work is cancellable. Pass accepted tokens through every cancellable call.
- Do not add a token mechanically to synchronous, atomic, or non-cancellable work merely because it
  touches I/O somewhere below the abstraction.
- Avoid blocking on tasks with `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()` on request,
  UI, or concurrency-sensitive paths.
- Materialize deferred work before reusing it; repeated enumeration of async-producing LINQ can
  start work more than once.

## Resources and errors

- Dispose owned `IDisposable` and `IAsyncDisposable` values with `using` and `await using`.
- Preserve stack traces with `throw;` when rethrowing the current exception.
- Catch only when the boundary can add context, translate to its own error contract, recover, or
  perform required cleanup. Logging and continuing is valid only when continuing is explicitly safe.
- Use exceptions for exceptional failures. Use result or `Try*` shapes when failure is an expected
  branch callers routinely handle.
- Assign review severity from reachability and impact. The same construct can be high on a request
  path, medium behind a rare failure, or no finding when the surrounding contract makes it safe.
