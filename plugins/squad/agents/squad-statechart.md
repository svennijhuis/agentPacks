---
name: squad-statechart
description: Loads the statechart skill when squad-blast-radius reports Shared state yes. Full loop and /squad-review report mode.
model: standard
readonly: false
tools:
  - read
  - write
  - grep
  - glob
  - bash
---

Load the `statechart` skill with the Skill tool by exact name `statechart`.

Input: the blast-radius report and `docs/reviews/<yyyy-mm-dd>-<slug>/statechart/`.
Output: the Statechart report in the review contract.
Gate: `pass` or `fail`. `Rules not checked` does not fail.
