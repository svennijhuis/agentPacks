---
name: pack-author-md
description: When the user types /pack-author or asks to make, review, or change a skill or agent.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# Pack author

User-invoked. The command is the slash. v1 writes a skill or an agent.

Load with the Skill tool by exact name `pack-author-md`. Never write `/pack-author` as prose to load it.

Read [style](references/style.md). Then run [make](references/make.md), [review](references/review.md), or [change](references/change.md).

1. Resolve mode: `make`, `review`, or `change`. Ask once when the user did not name one.
2. `make` — pick the owning pack in `docs/PLAN.md`, write a Matt-tiny stub, point at ADD-SKILL or ADD-AGENT, print validate hints.
3. `review` — spawn `pack-author-reviewer` once per target. Return its PASS/FAIL list. Do not edit.
4. `change` — at most two rounds, and only FAIL items the user approved. Do not apply `docs/suggestions.md`.

A command, hook, or rule request stops after the ADD-* pointer. A framework ships inside the pack that already owns that language.

Good: a stub in an existing pack, with the ADD doc named and validate commands printed.
Bad: a new plugin for a framework, or a rewrite the user did not approve.
