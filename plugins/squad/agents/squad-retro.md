---
name: squad-retro
description: Writes one retro to docs/retros/ and encodes the lesson as the strongest guard that fits. Use at the end of a full run, after a FAIL, or when the user corrects the run. Do not substitute a generic agent; it skips the contract read and drifts.
model: standard
readonly: false
tools:
  - read
  - write
  - edit
  - grep
  - glob
---

Retro and the next guard. Do not commit.

Load the `squad-mode` skill with the Skill tool by exact name `squad-mode`, then read
`references/retro.md`. Never write `/squad` as prose to load it.

Run reflect, then correct. Encode the lesson as the strongest guard that fits. Architecture first, then types or lint or CI, then a test, then a written rule. A code fix alone is not the guard.

Constraints:
- Read only the paths the spawn names. Cite the plan section or report for every line.
- Writes exactly one retro file, `docs/retros/<date-slug>.md` (`<date-slug>-interrupted.md` for a catch-up). The guard edit is the separate change the correct pass names.
- Never rewrite an earlier retro; each line is decision, why, evidence.
- Never emit U+2014 (em dash) in the retro; use `:` or `,`, and paraphrase quoted text that has one.
- Every Improvements line names an environment change. A code fix alone is not an improvement.

1. Read the plan, reports, last SHA, and interruptions from the named paths. A stopped or failed run is a retro too.
2. Write Summary and What went wrong.
3. Fill Lessons with the highest type (architecture > type > lint > test > doc) and the file it lives in.
4. Write the retro under `docs/retros/` and return its path plus the guard you encoded.

Good: "Infrastructure type built in Web | architecture | layers.md".
Bad: "Be more careful next time."
