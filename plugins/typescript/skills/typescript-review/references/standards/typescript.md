<!-- Generated from standards/typescript.md via standards.source.json. Edit the canonical document, not this copy. -->

# TypeScript standards

Use the repository `tsconfig`, package manager, and module kind. Do not invent a second compiler policy.

- Keep `strict` on when the repo already has it. Do not weaken `strict`, `noImplicitAny`, or `exactOptionalPropertyTypes` to land a change.
- Prefer `unknown` over `as any`. A cast needs a type guard or a comment that names the checked boundary.
- Model absence with `undefined` or a discriminated union. Do not use `null` and `undefined` interchangeably without repo evidence.
- Public exports stay typed. Do not add an implicit `any` parameter to dodge a generic.
- Match the repo module system: ESM (`type: module`, `.js` specifiers in relative imports) or the existing CJS shape. Do not mix `require` into an ESM package.
- Exhaust `switch` on discriminated unions. A missing variant is a correctness defect, not a style note.
- Do not fire-and-forget a `Promise`. Await it, return it, or void it at a documented boundary.
- Keep `tsconfig` paths, `jsx`, and `lib` as the repo set them. A local override that only the new file needs is a finding.
- Cast with `as` only after validation. Prefer `satisfies` to check a literal without widening it.
- A type guard must verify every property it claims. Name it `isX` or `hasX`. When the repo has a runtime schema library, infer the type from the schema instead of hand-writing the guard.
- Brand primitives that callers can mix up (ids, units) and validate them once at the boundary.
- Prefer one object argument over several positional ones, except on hot paths.
- Shipped code logs through the repo's structured logger, not `console.log`.
