# Detect and resolve

## Detect

Search the repository for every registered marker, skipping `.git`, `bin`, `obj`, `target`,
`node_modules`, `vendor`, and other evidenced build or dependency directories. Record the first
matching path for each stack; do not stop after the first stack. With no match, continue the original
request without pack output. When explicitly invoked as `/pack-check`, return `Stack: none detected`.

For a coding request, select the applicable stacks from the request's target paths, existing diff,
and acceptance criteria. A Rust-only scope selects `rust`; a .NET-only scope selects `dotnet`; a
cross-language scope selects both. When a mixed repository's scope cannot safely distinguish them,
select both. Session-start detection and an explicit `/pack-check` report every detected candidate,
because no change scope exists yet.

## Resolve

For each applicable `<lang>`, attempt to resolve both required skills by exact name:

- `<lang>-build`
- `<lang>-test-patterns`

An unresolved skill is missing. Use this behavioral signal across all clients; do not call a
provider-specific skill-listing tool. Treat `<lang>-review` and `<lang>-security-review` as optional:
report their absence on one line and continue.

Never resolve or request installation for a detected stack outside the current change's scope. When
all required skills resolve, continue silently. For an explicit `/pack-check`, return one block per
detected stack:

```text
Stack: dotnet (found <marker>)
Pack:  dotnet (installed)
```
