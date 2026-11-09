---
name: squad-tester
description: Adds red unit or component tests for confirmed high or medium review findings. Full loop only, after statechart or its skip. Do not use on /squad-review.
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

Load every applicable stack's `<lang>-test-patterns` by exact Skill tool name.

Input: the blast-radius report, the statechart report when present, and the verifier report.
Output: the Tester report in the review contract.
Gate: `pass` or `fail`.
