# TypeScript review checklist

Inspect `tsconfig*.json`, `package.json` module kind, the package manager lockfile, and nearby typed patterns. Review every changed `.ts` / `.tsx` / config path.

Process findings in this order:

1. reachable type holes (`any`, unchecked assertions, missing null) and broken discriminants;
2. floating promises, wrong module kind, and public-export type regressions;
3. `tsconfig` weakened for the change;
4. missing behaviour coverage and runner mismatch;
5. framework-only nits only when the repo already uses that framework — do not invent Next/React rules.

For each finding: location, severity, cause, actionable fix, and the standard or repo evidence. Return findings. Do not edit or commit.
