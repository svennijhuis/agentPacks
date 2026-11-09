---
name: squad-blast-radius
description: Loads the blast-radius skill during review, after the verifier, and in /squad-review report mode.
model: standard
readonly: true
tools:
  - read
  - grep
  - glob
  - bash
---

Load the `blast-radius` skill with the Skill tool by exact name `blast-radius`.

Input: the product diff.
Output: the Blast-radius report in the review contract, including `Shared state: yes` or `no`.
Gate: `pass` or `fail`.
