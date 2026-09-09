---
name: typescript-review
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. Supply TypeScript-specific findings from the pack's canonical type, module, and testing standards.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# TypeScript review

When loaded by exact Skill tool name `typescript-review` during review or build:
1. Read every file in `references/standards/`.
2. Standards in force: `typescript.md`, `testing.md`.
3. Cite the document filename on each finding (`typescript.md`, not "the TS standard").

Inspect `tsconfig*.json`, `package.json` module kind, the package manager lockfile, and nearby typed patterns. Review every changed `.ts` / `.tsx` / config path.

Process findings in this order:

1. reachable type holes (`any`, unchecked assertions, missing null) and broken discriminants;
2. floating promises, wrong module kind, and public-export type regressions;
3. `tsconfig` weakened for the change;
4. missing behaviour coverage and runner mismatch;
5. framework-only nits only when the repo already uses that framework — do not invent Next/React rules.

For each finding: location, severity, cause, actionable fix, and the standard or repo evidence. Return findings. Do not edit or commit.

Good: cite `typescript.md` on an unchecked `as`.
Bad: "looks fine" with no location or standard.
