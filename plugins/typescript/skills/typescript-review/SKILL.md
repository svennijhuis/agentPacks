---
name: typescript-review
description: When reviewing TypeScript files, diffs, or pull requests.
license: UNLICENSED
user-invocable: false
metadata:
  audience: loop
---

Internal. Do not run directly — Squad loads by exact Skill name.

# TypeScript review

Loop-only; not as a user entrypoint.

When loaded by exact Skill tool name `typescript-review` during review or build:
1. Read every file in `references/standards/`.
2. Standards in force: `typescript.md`, `testing.md`.
3. Cite the document filename on each finding (`typescript.md`, not "the TS standard").

Process findings per [the checklist](references/checklist.md).

Good: cite `typescript.md` on an unchecked `as`.
Bad: "looks fine" with no location or standard.
