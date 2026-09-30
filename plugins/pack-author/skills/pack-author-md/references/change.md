# Change

Apply FAIL items from [review](review.md). The user approves each round. At most two rounds. Then stop, even when FAIL lines remain.

1. Show the PASS/FAIL list. Do not edit yet.
2. Ask which FAIL lines to apply. Wait. "All of them" approves every FAIL line for this round. PASS lines stay.
3. Edit only the approved lines. Keep the file's job.
4. Spawn `pack-author-reviewer` again on the same path.
5. When FAIL lines remain, run one more round: ask, wait, edit, review. That is round two.
6. Stop. Report the latest list and the round count.

Do not auto-rewrite. Do not apply `docs/suggestions.md`. Do not start a third round.
