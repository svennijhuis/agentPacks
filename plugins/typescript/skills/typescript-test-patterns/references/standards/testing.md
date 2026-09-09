<!-- Generated from standards/testing.md via standards.source.json. Edit the canonical document, not this copy. -->

# TypeScript testing standards

Match the installed runner. A Vitest repo does not want its first Jest test.

- A behaviour change includes a test that fails without the change. Happy-path-only coverage is rejected.
- Prefer the runner already in `package.json` / `vitest.config.*` / `jest.config.*`.
- Isolate time, network, and filesystem. Do not rely on wall-clock sleeps.
- Colocate unit tests next to the module when the repo already does (`*.test.ts` / `*.spec.ts`). Put integration tests where the repo already puts them.
- Assert observable behaviour and error contracts, not private implementation details.
- Use the repo's assertion library (`expect`, `node:assert`). Do not add a second one for one test.
