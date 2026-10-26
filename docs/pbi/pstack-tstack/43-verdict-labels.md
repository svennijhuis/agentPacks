# PBI-43 (AP43): Verdict words and evidence labels
- Status: todo
- Area: verify
- Phase: E verification
- Lane: Builder (Cursor CLI, easy: docs)
- Depends on: PBI-33 (AP33, planned, not in the repo yet) (owns [`shared/standards/verification.md`](../../../shared/standards/verification.md)), AP25 (finding fields)
- proof: `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 and `grep -n '^## Verdict words' shared/standards/verification.md` prints one line
- door: two-way
- triage: build (three verdict vocabularies in play; one table stops drift)
- feedback: `validate` loads the standard; `StandardsGenerationTests` keeps the table in every provider tree
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L113 and D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/leaving-pr-comments/SKILL.md#L35-L40

## Problem now

Four sets of verdict words meet in one flow:
Squad verifier `pass | not verified | blocked | skipped` ([`agents/squad-verifier.md:19-23`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md#L19-L23)),
verify-this `VERIFIED | NOT VERIFIED | INCONCLUSIVE`,
room `PASS | FAIL | BLOCKED`,
sol-loop `APPROVE | CHANGES`.
Claims in hand-offs also carry no label saying whether they were measured.

## Why (plain words)

Same meaning, different words, leads to a "pass" that was never a pass.
One table maps them.
And every claim says how we know it: we measured it, we inferred it, or it is a guess.

## The table (goes into shared/standards/verification.md)

| Meaning | Squad verifier | verify-this | Room (Tester, Reviewer) | Review loop (AP46) |
|---|---|---|---|---|
| Proven on the real surface | `pass` | `VERIFIED` | `PASS` | `APPROVE` |
| Checked, wrong | `fail` | `NOT VERIFIED` | `FAIL` | `CHANGES` |
| Could not check | `blocked`, `not verified`, `skipped (needs: ...)` | `INCONCLUSIVE` | `BLOCKED` | out of budget |

Never a pass: `not verified`, `blocked`, `INCONCLUSIVE`, out of budget, timeout, no reply.

Evidence label on every claim in a hand-off, review finding or PR body:
`measured` (we ran it, output quoted), `inferred` (read from code or docs, cite file:line), `guess` (say so; it never closes a criterion).

Example hand-off lines:

```text
validate is 34% faster (measured: 4.1 s to 2.7 s, 5 runs each).
Old callers of LoadPins are gone (inferred: rg 'LoadPins' returns 0 hits at 3c1d9e0).
CI on Windows should pass (guess: no Windows runner locally).
```

## Before / after

| | Before | After |
|---|---|---|
| `INCONCLUSIVE` from a pinned skill | unmapped | = `blocked`, never pass |
| Hand-off claim | free text | labelled |

## Definition of Done

- [ ] `## Verdict words` and `## Evidence labels` sections in [`shared/standards/verification.md`](../../../shared/standards/verification.md).
- [ ] `squad-verifier.md` and `review-contract.md` link the table once instead of restating it.
- [ ] AP25 PBI text points here for labels (no second definition).
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0.
- [ ] No em or en dashes.

## Technical design

- Module boundary: one standards file; agents link it. No lint rule (labels need judgment); the table is the single home.
- Rules: none.

## Sources

- pstack [`poteto-mode/SKILL.md:113`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L113) (measured, inferred or guess in the same sentence), [`skills/why/references/epistemics.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/why/references/epistemics.md) (confidence tiers).
- cursor-team-kit [`verify-this/SKILL.md:26,53`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/verify-this/SKILL.md#L26).
- tstack [`leaving-pr-comments/SKILL.md:35-40`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/leaving-pr-comments/SKILL.md#L35-L40) (cite, distinguish verified facts from judgment), [`sol-loop/SKILL.md:61-62,108`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L61-L62) (timeout or no reply is not approval; budget end is not approval).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- new: [`shared/standards/verification.md`](../../../shared/standards/verification.md)
- edit: [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`cursor-team-kit/skills/verify-this/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/cursor-team-kit/skills/verify-this/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/why/references/epistemics.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/why/references/epistemics.md)
- t3dotgg/tstack: [`skills/leaving-pr-comments/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/leaving-pr-comments/SKILL.md)
- t3dotgg/tstack: [`skills/sol-loop/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/agents/squad-verifier.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/agents/squad-verifier.md)
- agentPacks new file: [`shared/standards/verification.md`](../../../shared/standards/verification.md)
