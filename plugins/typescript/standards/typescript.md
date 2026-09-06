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
