---
description: "Make, review, or change a skill or agent. Edits wait for approval."
---

# Pack author

Load the `pack-author-md` skill with the Skill tool by exact name `pack-author-md`.
Never write `/pack-author` as prose to load it. Then run the mode the user named.

Type `/pack-author`. Codex: `$pack-author`. Copilot: `/pack-author:author`.

1. `make` — owning pack from PLAN.md, Matt-tiny skill or agent stub, ADD-SKILL or ADD-AGENT, validate hints.
2. `review` — spawn `pack-author-reviewer`. Return PASS/FAIL. Do not edit.
3. `change` — at most two rounds, FAIL items the user approved. Do not auto-rewrite.

Do not scaffold a command, hook, or rule. Do not commit.
