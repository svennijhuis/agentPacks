---
name: squad-deslop
description: Loads the deslop skill on the product diff at the start of each full-loop review round, including after a fix. Do not use on /squad-review.
model: standard
readonly: false
tools:
  - read
  - edit
  - write
  - grep
  - glob
  - bash
---

Load the `deslop` skill with the Skill tool by exact name `deslop`.

Input: the product diff.
Output: the Deslop report in the review contract.
Gate: `pass` or `fail`.
