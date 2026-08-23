# C# design standards

Use established project configuration and repeated local patterns for choices this standard leaves open.

## Types and equality

- Use records for data whose identity is its scalar values; use classes for entities, mutable state,
  framework-managed objects, and objects with a lifetime.
- Record equality is only as structural as each property. Collections such as `IReadOnlyList<T>`
  normally retain reference equality, so two records holding equivalent lists are not necessarily
  equal. Add an explicit value type or comparer when collection contents belong to equality.
- Keep DTO collection properties read-only at the interface. `IReadOnlyList<T>` prevents callers
  from depending on mutation; it does not make the collection immutable or structurally comparable.
- Keep types sealed unless inheritance is part of the intended interface.

## Nullability and interfaces

- Follow the repository nullable context. A non-nullable type is a promise; guard the boundary that
  establishes it instead of spreading null-forgiving operators.
- Keep public surface area small. Prefer `internal` for implementation types and expose a type only
  when another assembly is intended to depend on it.
- Return the least-mutable interface that describes caller rights. Do not expose a mutable list when
  callers only need enumeration or indexed reading.
- Prefer parameters that name intent over boolean switches at call sites.

## Dependencies and layout

- Accept dependencies in constructors. Use `TimeProvider` for time-dependent code;
  `FakeTimeProvider` is supplied by the `Microsoft.Extensions.TimeProvider.Testing` package.
- Match existing project boundaries. A new project creates a compilation and dependency boundary;
  use a folder when that boundary buys nothing.
- Match repository naming, analyzers, `.editorconfig`, and existing patterns before introducing a
  new idiom.

## Culture-sensitive values

- Use an explicit culture for parsing, formatting, casing, and comparison when the value crosses a
  storage, protocol, identifier, or user-locale boundary. Do not let the machine's current culture
  silently define a durable contract.
