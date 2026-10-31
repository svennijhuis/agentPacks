# PBI-50 (AP50): Open decisions for Sven (L1 to L5)
- Status: besluit Sven open
- Area: squad
- Phase: G review loop (gate before AP46 to AP48)
- Lane: Sven (decision), then Product Owner updates the PBIs
- Depends on: none. Blocks [AP43](43-verdict-labels.md), [AP46](46-review-loop-budget.md), [AP47](47-review-ledger.md), [AP48](48-review-pr-line.md) (L1, L2), the Source lines of AP37 to AP63 (L3), [AP57](57-paved-path-size-guard.md) (L4) and [AP56](56-trust-level.md) (L5)
- proof: Sven's answer per question (L1 A or B or C, L2 A or B or C, L3 A or B, L4 A or B or C, L5 A or B) is written into this file under `## Decision`, and the PBIs it names are updated in the same PR
- door: two-way (docs); L1 option B is one-way for the room charter until changed back
- triage: decide (Sven asked 2026-10-26 for one PBI holding the three open questions)
- feedback: Product Owner checks that every PBI this file names matches the chosen option (grep in Definition of Done)
- harness: none (decision)
- review-input: this PBI + the linked PBIs + RULES.md, never builder session logs (PA6)
- kind: skill
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/references/bugbot-triage.md#L7-L12 and D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L15-L25

## Problem now

Five choices block the review-loop PBIs, the Source lines, the size guard (AP57) and the trust list (AP56). Without an answer, Builder would guess, and a wrong guess changes `SquadContractTests`, the room charter, or every PBI header.

## Why (plain words)

Each question has a cheap answer and an expensive one. Pick now, before AP46 to AP48 are built, so the build matches your rule and no check undoes it later.

## L1. Review-round budget vs "max 2 fix rounds"

### Context

- Squad today: "At most two fix rounds" inside one `/squad` run ([`plugins/squad/skills/squad/SKILL.md:108`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L108), [`plugins/squad/commands/squad.md:35`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md#L35)). The review contract numbers rounds 1 to 3 ([`plugins/squad/skills/squad/references/review-contract.md:245`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L245)).
- `/squad-review` is report-only: no fix round ([`plugins/squad/commands/squad-review.md:27-28`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md#L27-L28)).
- Room: Reviewer bot posts one PASS/FAIL per PR SHA. Builder fixes, Reviewer checks again. No count, no stop rule.
- tstack sol-loop: 3 rounds, 5 for security, auth, data loss or concurrency. Stop when only nits remain. Budget end is not approval ([`sol-loop/SKILL.md:15-25`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L15-L25), [`:103-108`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L103-L108)).
- Affected: [AP43](43-verdict-labels.md) (verdict table), [AP46](46-review-loop-budget.md) (REV01 to REV04), [AP48](48-review-pr-line.md) (REV10). Rules: [RULES.md rule table](RULES.md#rule-table), [ID scheme](RULES.md#id-scheme).

### Options

| Option | What | Consequences |
|---|---|---|
| A (advice) | Two kinds of round with separate names. **Fix round**: inside `/squad`, max 2, unchanged. **Review round**: on a PR, Reviewer bot vs Builder, budget 3 (5 for risk). Round 1 = Reviewer's first PASS/FAIL. Verdict words on disk and in replies: `PASS | FAIL | BLOCKED` only (AP43 table), never `APPROVE`/`CHANGES`. Out of budget = `BLOCKED (out of budget)`, Sven decides. | No change to `SquadContractTests` or the orchestrator. Room charter gets one line. AP43 and AP46 to AP48 already use these words. The Squad verifier keeps its own Result words (`pass | not verified | blocked | skipped`). The number of passes on a PR can be 3 review rounds, each with up to 2 internal fix rounds. |
| B | One shared budget: max 2 rounds total, PR review included. | Simplest rule to remember. sol-loop budget drops to 2, so a risky PR stops after 2 Reviewer passes and goes to Sven more often. AP46: REV01 budget becomes 2, REV02 (budget 5) is dropped. Room "max 2 fix rounds" stays literally true. |
| C | No budget on PR review; only the stop rules (stop on nits, out-of-scope declined, dispute after 2 replies to Sven). | No new numbers. Loops can run long on PRs that keep finding real bugs. REV01 to REV03 are dropped; REV04 and AP47 stay. |

### Advice

A. It keeps today's tested rule as is and adds a cap where there is none now (PR review). Two names stop the "is this round 3 or fix round 1?" confusion. B makes risky PRs land on you sooner. C has no hard stop.

## L2. Severity words

### Context

- Main: `high | medium | low | tiny` with effects ([`plugins/squad/skills/squad/references/review-contract.md:5-14`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L5-L14)).
- sol-loop: `blocker | should | nit` ([`sol-loop/SKILL.md:74`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L74)).
- AP25 (planned, not in the repo yet): `severe | important | nit`.
- pstack bugbot triage uses actions, not severities: `fix | dismiss | ask` ([`poteto-mode/references/bugbot-triage.md:7-12`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/references/bugbot-triage.md#L7-L12)).
- Affected: [AP47](47-review-ledger.md) (REV05, finding line), [AP48](48-review-pr-line.md) (reply shape), AP25. Rules: [RULES.md rule table](RULES.md#rule-table), [old to new](RULES.md#old-to-new).

### Options

| Option | What | Consequences |
|---|---|---|
| A (advice) | Keep the 4 words in files. One alias table in `review-contract.md ## Severity`: blocker = high, should = medium, nit = low or tiny. AP25 uses it too. | No churn in review-contract, tests or old reviews. The finding line and ledger (AP47) store only the 4 words; check-review (REV05) accepts only those. Alias words are for chat; the table maps them. One set on disk. |
| B | Switch to blocker, should, nit everywhere. | Shorter and matches sol-loop. Rewrites `review-contract.md`, its tests and the effect rules ("any high or medium means fix"). Old review files keep old words. AP25 must change. |
| C | Keep two vocabularies (linters vs reviewers) with no alias. | No work now. Reviewer and Builder argue about what "should" means. REV05 cannot check severity. Not recommended. |

### Advice

A. Same meaning, no rewrite, one spelling on disk for the linter. The alias costs 4 lines.

## L3. pstack and tstack in the URL learnings source list

### Context

- The PBI template check needs `- Source:` with a D or PA id plus a URL (TPL01, from AP36). Every PBI in this folder uses D39 (pstack) or D40 (tstack), marked "(proposed)".
- The source list (D1 to D38) is Strategy's URL learnings, outside this repo.
- Sources: [pstack poteto-mode](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md), [tstack sol-loop](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md). Licenses: [NOTICE.md](NOTICE.md).
- Affected: the `- Source:` header line of all 26 PBIs [AP37](37-task-type.md) to [AP63](63-rules-catalog.md) (AP42 dropped on 2026-11-01), this file included. D41 (talk) and D42 (podcast) follow the same option. Rules: [RULES.md rule table](RULES.md#rule-table) (TPL01).

### Options

| Option | What | Consequences |
|---|---|---|
| A (advice) | Strategy adds D39 = cursor/plugins pstack + cursor-team-kit @ 9e88dad, and D40 = t3dotgg/tstack @ 7de017b, each with the license line. Strategy adds D41 and D42 the same way. Product Owner drops "(proposed)" from the 27 `- Source:` lines (AP37 to AP63). | One edit outside the repo, one small docs commit. TPL01 stays meaningful. |
| B | Use PA ids (Product Owner approved sources) instead of D ids. | Works for TPL01 too. Mixes upstream code sources into the PO list, so the D list no longer has every external source. |

### Advice

A. D ids are external sources by design, and these are external repos.

## L4. File size limit for DIFF01 and DIFF02 (AP57)

### Context

- [AP57](57-paved-path-size-guard.md) warns on a new file over N lines and on a file that grows past N. poteto: early Grok Bot had "eight god files ... at least 10,000 lines long" ([podcast, after 27:09](https://www.youtube.com/watch?v=MN9dGgmLyso)).
- The number is one constant in `check-diff.sh`. It is a warning, not a block.
- Affected: [AP57](57-paved-path-size-guard.md) (DIFF01, DIFF02 constant).

### Options

| Option | What | Consequences |
|---|---|---|
| A (advice) | 400 lines | Catches real god files, few false alarms in test files. |
| B | 300 lines | More warnings; pushes small files early. |
| C | 600 lines | Fewer warnings; god files grow longer before a warning. |

### Advice

A. A warning at 400 is cheap to ignore once and loud when it repeats in the weed log (AP59).

## L5. Which repos may reach trust level T3 (AP56)

### Context

- [AP56](56-trust-level.md) T3 = agents merge after full gates, only on `door: two-way` PRs. Today the room rules allow bot merges only for Chatmarkt and agentic-workflows after full gates and PO GO.
- T3 is a decision, never computed from files.
- Affected: [AP56](56-trust-level.md) (T3 list, VER14).

### Options

| Option | What | Consequences |
|---|---|---|
| A (advice) | T3 list = today's two repos, written in the room charter; agentPacks stays T2 max | No change to merge rights; the file only makes today's rule visible. |
| B | No T3 anywhere until AP52 to AP58 are built | Stricter than today; Chatmarkt and agentic-workflows lose bot merges. |

### Advice

A. It writes down what already holds and keeps agentPacks merges with Sven.


## Example decision block (Sven fills it in)

```markdown
## Decision
- L1: A (2026-10-27, Sven)
- L2: A
- L3: A
- L4: A
- L5: A
```

## Before / after

| | Before | After |
|---|---|---|
| Review-round rule | not written down | one chosen option, referenced by AP46 |
| Severity words | 3 sets | one rule (alias or switch) |
| Source ids | "proposed" | real D39 to D42 rows |
| Size limit (AP57) | open | one number |
| T3 repos (AP56) | room rule only | written list |

## Definition of Done

- [ ] `## Decision` block filled with one option per question and the date.
- [ ] Every PBI named under "Affected" matches the choice. One positive check per answer (run from the repo root, `D=docs/pbi/pstack-tstack`):
  - L1 A: `grep -n 'Budget: 3' $D/46-*.md` hits, and `grep -n -E 'APPROVE|CHANGES' $D/4[6-8]-*.md` prints nothing. L1 B: `grep -n 'Budget: 2' $D/46-*.md` hits and `grep -n REV02 $D/46-*.md` prints nothing. L1 C: `grep -n -E 'REV0[1-3]' $D/46-*.md` prints nothing.
  - L2 A: `grep -n -E '\[(blocker|should|nit)\]|\| (blocker|should|nit) \|' $D/4[6-8]-*.md $D/51-*.md` prints nothing (the alias words appear only as "chat alias"). L2 B: `grep -n -E '\[(high|medium|low|tiny)\]' $D/4[6-8]-*.md $D/51-*.md` prints nothing.
  - L3 A or B: `grep -n '^- Source:.*(proposed)' $D/3[7-9]-*.md $D/4[0-9]-*.md $D/5[0-9]-*.md $D/6[0-3]-*.md` prints nothing, and every `- Source:` line names a D id (A) or a PA id (B).
  - L4: `grep -n -E '(300|400|600) lines' $D/57-*.md` shows only the chosen number.
  - L5 A: the T3 rows in `$D/56-*.md` name exactly the repos in the decision and not agentPacks. L5 B: AP56 has no T3 repo.
- [ ] `bash scripts/check-pbi-template.sh docs/pbi/pstack-tstack/*.md` exits 0 once AP36 lands (local copy used until then).
- [ ] No em or en dashes.

## Technical design

- Module boundary: this file holds the decision; PBIs link here, they do not repeat the options.
- Rules: none new. L1 can change REV01 to REV03, L2 changes REV05, L3 touches TPL01 inputs only.

## Sources

- tstack [`sol-loop/SKILL.md:15-25`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L15-L25) (budget), [`:74`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L74) (severities), [`:103-108`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md#L103-L108) (stuck, budget end is not approval).
- pstack [`poteto-mode/references/bugbot-triage.md:7-12`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/references/bugbot-triage.md#L7-L12) (fix, dismiss, ask).
- agentPacks [`plugins/squad/skills/squad/SKILL.md:108`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L108), [`plugins/squad/commands/squad.md:35`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md#L35), [`plugins/squad/commands/squad-review.md:27-28`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md#L27-L28), [`plugins/squad/skills/squad/references/review-contract.md:5-14`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L5-L14), [`:245`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md#L245).

## Files this PBI changes

Before the answer: this file only. After `## Decision` is filled, in the same PR:

- L1: [`43-verdict-labels.md`](43-verdict-labels.md), [`46-review-loop-budget.md`](46-review-loop-budget.md), [`48-review-pr-line.md`](48-review-pr-line.md), [`RULES.md`](RULES.md) (REV01 to REV03 rows).
- L2: [`46-review-loop-budget.md`](46-review-loop-budget.md), [`47-review-ledger.md`](47-review-ledger.md), [`48-review-pr-line.md`](48-review-pr-line.md), [`51-finding-guard-layer.md`](51-finding-guard-layer.md), [`RULES.md`](RULES.md) (Old to new).
- L3: the `- Source:` line of all 27 files `37-*.md` to `63-*.md`, plus [`README.md`](README.md).
- L4: [`57-paved-path-size-guard.md`](57-paved-path-size-guard.md).
- L5: [`56-trust-level.md`](56-trust-level.md).

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/references/bugbot-triage.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/references/bugbot-triage.md)
- t3dotgg/tstack: [`skills/sol-loop/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/sol-loop/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/commands/squad-review.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad-review.md)
- agentPacks main (existing files): [`plugins/squad/commands/squad.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/commands/squad.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/references/review-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/review-contract.md)
