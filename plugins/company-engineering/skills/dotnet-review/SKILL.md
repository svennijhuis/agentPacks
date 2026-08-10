---
name: dotnet-review
description: Review .NET and C# changes for correctness, API design, async usage, error handling and testability. Use when reviewing a C# pull request, diff or file, or when asked whether .NET code follows company standards.
license: UNLICENSED
---

# .NET review

Review C# changes in this order. Stop at the first category with findings and report those before moving on — a correctness bug outranks a naming nit.

## 1. Correctness

- Nullability: does the code honour the project's nullable context? Flag `!` suppressions that hide a real null path.
- Boundary conditions in loops, ranges and slicing. `<` vs `<=` is the classic.
- Equality: reference vs value semantics, `record` vs `class`, overridden `Equals` without `GetHashCode`.
- Culture-sensitive operations. `ToUpper()`, `Parse` and `ToString()` on numbers and dates need an explicit culture.

## 2. Async

- `async void` outside event handlers is a defect.
- Missing `CancellationToken` on any method that performs I/O, and tokens accepted but never passed down.
- `.Result`, `.Wait()` and `.GetAwaiter().GetResult()` on a hot path.
- `Task.WhenAll` over a list that is enumerated twice, or a lazily-evaluated LINQ query.

## 3. Resource and error handling

- `IDisposable` without `using`, and `IAsyncDisposable` without `await using`.
- `catch` blocks that swallow, log-and-continue, or rethrow with `throw ex;` (loses the stack).
- Exceptions used for expected control flow where a result type or `TryParse` pattern reads better.

## 4. API design

- Public surface: is anything public that only tests need? Prefer `internal` plus `InternalsVisibleTo`.
- Parameter counts above four, and boolean parameters that read as `DoThing(true, false)` at the call site.
- Returning `List<T>` where `IReadOnlyList<T>` states the contract.

## 5. Testability

- New branching logic without a test. Name the specific case that is uncovered.
- Static state, `DateTime.Now` and `new HttpClient()` inline — all make the unit untestable.

## Reporting

One line per finding: `path:line — problem. Suggested fix.` Do not restate what the code does. Do not comment on formatting an analyzer already enforces.
