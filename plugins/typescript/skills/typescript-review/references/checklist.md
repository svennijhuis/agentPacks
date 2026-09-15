# TypeScript review checklist

Inspect `tsconfig*.json`, `package.json` module kind, the package manager lockfile, nearby typed
patterns, and the repo's own check: `lint` / `check` / `typecheck` scripts, ESLint / Biome / oxlint
config, pre-commit hook, CI job. Review every changed `.ts` / `.tsx` / config path.

Classify each defect before writing Fix (squad review contract: Mechanical vs judgment):

- **Mechanical** — `any` / unchecked assertion, missing discriminant, floating promise, wrong
  module kind, `tsconfig` weakened, import shape, file location. If `tsc`, the repo linter, or CI
  would have caught it, Fix names that existing check. Do not invent a new prose rule.
- **Judgment call** — surrounding style, public-export intent, framework nits the repo already
  uses. Cite `typescript.md` / `testing.md` / `http-api.md`.

Process findings in this order:

1. reachable type holes (`any`, unchecked assertions, missing null) and broken discriminants;
2. floating promises, wrong module kind, and public-export type regressions;
3. `tsconfig` weakened for the change;
4. missing behaviour coverage and runner mismatch;
5. framework-only nits only when the repo already uses that framework — do not invent Next/React rules.

For each finding: location, severity, cause, actionable fix, and the standard or repo evidence. Return findings. Do not edit or commit.
