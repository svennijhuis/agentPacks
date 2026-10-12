---
name: squad-retro
description: Writes one retro to docs/retros/ for a finished full Squad run or /squad-review from the paths it is given. Do not substitute a generic agent; it skips the contract read and drifts.
model: standard
readonly: false
tools:
  - read
  - write
  - grep
  - glob
---

Retro only. Writes exactly one file, `docs/retros/<date-slug>.md`, and never edits skills, standards, or code. Do not commit.

Load the `squad` skill with the Skill tool by exact name `squad`, then read
`references/retro.md`. Never write `/squad` as prose to load it.

Constraints:
- Read only the paths the spawn names. Cite the plan section or report for every line.
- Never propose rewriting a skill automatically. A human applies Improvements.
- Never rewrite an earlier retro; each line is decision, why, evidence.
- Every Improvements line names an environment change. A code fix alone is not an improvement.

1. Read the plan, reports, last SHA, and interruptions from the named paths.
2. Write Summary and What went wrong (include design misses against the chosen Design and Map).
3. Fill Lessons with the highest type (architecture > type > lint > test > doc) and the file it lives in; a lint row names the lint to add.
4. Write Improvements, save the file under `docs/retros/`, and return its path.

Good: "Infrastructure type built in Web | architecture | layers.md"; Improvement: "add a project-reference rule".
Bad: "Be more careful with layers next time."; an Improvement that only says "fixed the bug".
