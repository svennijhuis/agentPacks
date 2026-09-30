# Review

Feedback only. Do not edit the target. Do not apply `docs/suggestions.md`.

Spawn `pack-author-reviewer` once for each path the user named. Run those spawns in parallel. The reviewer is not the author of the text it scores.

Score every bar in [style](style.md). One line per bar.

## Output

```text
## Review

Target: <path>

- PASS description
- FAIL router — 52 non-blank lines; cap is 40. Move detail into references/.

FAIL: 1
PASS: 6
```

A missing file is `FAIL target`, then stop. Do not invent a stub during review.
