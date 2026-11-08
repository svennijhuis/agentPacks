# typescript

The TypeScript language pack for [Squad](../squad/README.md). It supplies the
build, test, and review skills that Squad discovers by exact name.

Slot skills are Squad internals. Do not run them directly: Squad loads each by exact Skill name.

## What is in it

| Skill | Used by | Purpose |
|---|---|---|
| `typescript-build` | implementer, simplifier | Internal loop skill. Package manager, `tsconfig`, typecheck and build commands |
| `typescript-test-patterns` | implementer, verifier | Internal loop skill. Vitest / Jest / `node:test` and the exact test command |
| `typescript-review` | correctness reviewer | Internal loop skill. Types, modules, promises, and test-boundary mistakes |

No `<lang>-security-review`. OWASP stays the floor. No framework *slot* skills (`react-*`, `next-*`):
the contracted names are the three above and nothing else.

Framework knowledge arrives as reviewed external imports, reached *through* the slot skills rather
than discovered by Squad:

| Skill | Upstream | License | Purpose |
|---|---|---|---|
| `vercel-react-best-practices` | vercel-labs/agent-skills | MIT | React and Next.js performance: rendering, re-renders, bundles, server data fetching |
| `vercel-composition-patterns` | vercel-labs/agent-skills | MIT | Component APIs that scale: compound components, children over render props, explicit variants, React 19 |
| `vercel-react-view-transitions` | vercel-labs/agent-skills | MIT | Route changes, shared elements, enter/exit and list reorder, with the Next.js integration |
| `vercel-react-native-skills` | vercel-labs/agent-skills | MIT | React Native and Expo list performance, native UI, navigation, styling, monorepo deps |
| `shadcn` | shadcn-ui/ui | MIT | shadcn/ui components, registries, presets, and applying a DESIGN.md or brand spec |

They are pinned in `external-skills.json` and materialized by publication, so `main` holds only the
URL records. Their names keep the `[<framework>-]<action>-<object>` shape from
[PLAN.md](https://github.com/svennijhuis/agentPacks/blob/main/docs/PLAN.md), which is why none of
them starts with `typescript-`: a `typescript-*` name that is not a slot is the near-miss the
validator rejects.

Design direction, motion decisions and browser verification are language-agnostic and live in the
[`frontend`](../frontend/README.md) role pack. Where `vercel-react-best-practices` has a measured
cost, it outranks that pack's design preferences; where it only states a preference, design decides.

Canonical standards live once under `standards/`:

- `typescript.md`: strictness, `unknown` vs `any`, modules, exhaustiveness, promises.
- `testing.md`: runner match, behaviour coverage, isolation.
- [`http-api.md`](https://github.com/svennijhuis/agentPacks/blob/main/shared/standards/http-api.md): named collection envelope for list APIs, not a root JSON array (shared across language packs).

`standards.source.json` maps each document to the skills that need it.

## Editing this pack

Authored: `plugin.json`, `standards.source.json`, `standards/`, `skills/`, and `external-skills.json`.
No shipped MCP server. Imported skills are never edited here; move or repin them at their URL record
and let publication materialize the change.

See [ADD-LANGUAGE-PACK.md](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-LANGUAGE-PACK.md).
