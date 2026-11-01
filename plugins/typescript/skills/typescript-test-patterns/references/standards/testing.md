<!-- Generated from standards/testing.md via standards.source.json. Edit the canonical document, not this copy. -->

# TypeScript testing standards

Shared rules live in `testing-principles` (behavior tests, docs-phrase rule, injected time). Match the installed runner. A Vitest repo does not want its first Jest test.

- Prefer the runner already in `package.json` / `vitest.config.*` / `jest.config.*`.
- Isolate network and filesystem.
- Colocate unit tests next to the module when the repo already does (`*.test.ts` / `*.spec.ts`). Put integration tests where the repo already puts them.
- Use the repo's assertion library (`expect`, `node:assert`). Do not add a second one for one test.
