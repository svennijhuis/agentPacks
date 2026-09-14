# TypeScript testing standards

Match the installed runner. A Vitest repo does not want its first Jest test.

- A behaviour change includes a test that fails without the change. Happy-path-only coverage is rejected.
- Prefer the runner already in `package.json` / `vitest.config.*` / `jest.config.*`.
- Isolate time, network, and filesystem. Do not rely on wall-clock sleeps.
- Colocate unit tests next to the module when the repo already does (`*.test.ts` / `*.spec.ts`). Put integration tests where the repo already puts them.
- Assert observable behaviour and error contracts, not private implementation details.
- Do not unit-test that a markdown, skill, README, or docs file contains a phrase. Wording is reviewed. Parsers that consume markdown as structured input (tables, frontmatter, logs) get fixture tests of parse and evaluate behaviour. Generators and validators are tested as code.
- Use the repo's assertion library (`expect`, `node:assert`). Do not add a second one for one test.
