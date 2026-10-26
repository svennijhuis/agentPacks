# Fleet podcast audit (Matt Pocock x poteto)

Audited 2026-10-26 22:30 (Europe/Amsterdam). Read only. No repo edits, no messages.
Base: agentPacks main `34e1dec`, #83 head `7d8443a` (AP37-AP50), #80 head `88d3461`.

## 1. Source

| Item | Value |
|---|---|
| Video | "LIVE: Poteto (creator of pstack) on shipping 1,000's of PR's a month at SpaceX", Matt Pocock (@mattpocockuk) |
| URL | https://www.youtube.com/watch?v=MN9dGgmLyso |
| Date | Live 2026-10-02, published 2026-10-03. Length 1:06:45 |
| Format | Q&A ABOUT poteto's earlier X talk "how i shipped 2,500 PRs last month to production" (https://x.com/poteto/status/2102050467505430555). Different source from the talk the Router is transcribing now |
| Fleet's copy | Unlisted third-party gist (link kept out of this public repo; created 2026-10-24 10:03 UTC+2). File `1-LIVE-Poteto-creator-of-pstack-on-shipping-1-000-s-of-PR-s-a-month-at-SpaceX.txt` |
| Box copy | box file `gist_full.txt` (not in this repo), 67159 bytes. Byte count matches the gist raw text (verified with curl). Auto captions, no timestamps |
| Fleet notes | box files `gist-handover-agentpacks.md`, `gist-handover-agentic-workflows.md` (not in this repo) (both 2026-10-25 14:27). Point-by-point table: box file `gist-review.md` (not in this repo) (32 points + E1-E3) |
| Public summaries with timestamps | https://barnabyrobson.org/on-pstack/ , https://uiuo.nl/youtube-summaries/MN9dGgmLyso , https://prepublish.ai/youtube-transcript/MN9dGgmLyso |

Fleet error: the handovers call the source a "talk transcript ... on 2,500 PRs a month". It is Matt's interview about the talk. Fleet never gave the YouTube URL or date. The timestamps below come from barnabyrobson.org and were not rechecked against the video.

## 2. Fleet claims vs verified

| Claim | Verified |
|---|---|
| The source is the gist, and gist_full.txt is the full transcript | TRUE. The size matches the gist. The source is mislabelled (see above) |
| The 32 points have no timestamps | TRUE |
| Fold-ins went to #77, #78 and #79 through PO | NOT LANDED. All 3 PRs are OPEN. #77's last commit (`4b302e7`, 2026-10-15) predates the handover, and #79 has one commit (`ff65745`). Detail below |
| #79: contract test asserts no `imports:` key or relative import | NOT DONE. `StandardsGhAwImportContractTests.cs` only checks the file exists, has no `---` frontmatter, no `${{`, no `{{#`. Rerouted as a doc step in #80 `docs/pbi/01-import-contract-doc.md` step 7 and PBI-06 |
| #79: doc example pins `@sha` / `@vX.Y.Z` assert | NOT DONE in #79. Only in the doc prose (diff l.11). Rerouted to #80 PBI-01 step 7 |
| #77: tautological-test line in testing-principles + simplifier | NOT DONE (0 hits in the #77 diff). Note: Fleet's premise was also wrong. Main already has it in substance: `shared/standards/testing-principles.md:15-20` ("Test behavior, not implementation", delete a test that still passes on a wrong return, banned loose asserts). Fleet grepped only for the word "tautolog" |
| #77: indexer output must be `file:line` only (contract test) | PARTLY. #77 already had a `file:line` contract assert before the handover (diff l.365, l.391). Not a fold-in |
| #77: god-file line in dotnet-review checklist | NOT DONE. 0 hits in #77 and on main. No PBI in #80 or #83 |
| #78: no change | TRUE. #78 is still an OPEN draft, idle since 2026-10-19. #81 is a newer twin from the same automation (rule 41 conflict) |
| 3 verify PBIs: plugins/verify (pinned cursor/plugins), shared/standards/verification.md, repeat error to .mdc + validate check | EXIST ONLY AS BOX FILES: box folder `agentpacks-verify-pbis/32-verification-pack.md`, `33-verify-cli-standard.md`, `34-error-becomes-guard.md` (+36), 2026-10-26 08:13. No branch, no PR. #83 only links them as "planned, not in the repo yet" (`41-drive-kinds.md:6`, `43-verdict-labels.md:6`). Their Source lines cite warp.dev, t3code and the pstack guide, not the podcast |
| Its verify CLI and frontend examples were its own | TRUE for the podcast, which only says the CLI is "glue" over Playwright and CDP. The CLI properties (JSON output, `--dry-run`) come from the pstack guide `06-verify-and-ship.md`, not from Fleet |
| The 11 candidates were never ordered | FALSE. AP 1-6 became PBI-15 to PBI-20 in #80 (commit `13c5d88`, 2026-10-25), with a dependency order (15>16, 17>18, 19>20; README l.29-34, l.117-118). AW 1-5 sit on agentic-workflows main as X06 (done), F72 and F74 (B3), AO10 and F73 (B5). They are ordered. None is built |
| AW #6 / #7 | #6 MERGED 2026-10-26 16:38 UTC+1 (`6b6fd2c`): VF01 one `scripts/verify.sh` entry point, VF02 consumer verify template, POL27-POL30. Docs only. #7 OPEN: gh-aw pin bump to v0.93.5, unrelated to the podcast |

## 3. The 32 points

Legend: Fleet = Fleet verdict for AP (gist-review.md). Main = agentPacks `34e1dec`. #83 = AP37-AP50. #80 = PBI-15 to PBI-20.

| # | Point | Quote (gist_full.txt) | ts | Fleet | Main (file:line) | #83 |
|---|---|---|---|---|---|---|
| 1 | Verification first | "the single most important skill ... is verification" | 16:14 | gap | `plugins/squad/agents/squad-verifier.md:34`, verify-harness (`plugins/squad/README.md:48`) | yes AP40, AP41, AP42 |
| 2 | Self-verify, hill climb | "hill climbing ... a rubric or a way to judge or score" | 18:01 | partly | verifier re-measure `squad-verifier.md:22` only | no (hillclimb playbook skipped, CHECKLIST.md:95) |
| 3 | Auto-maintained verify skill per app | "every app ... has a verification skill that is automaintained" | ~19:30 | partly | none (no upkeep) | partly: AP41 leans on AP32 (box only); #80 PBI-20 |
| 4 | Deterministic parts in a CLI, the skill is a wrapper | "skill as kind of like a wrapper" | 21:03 | partly | none | partly AP44 "Build the lever"; #80 PBI-19 |
| 5 | One shared script, no rebuild | "rebuild the world each time and then every agent did it differently" | 22:47 | covered (#78) | `engineering.md:22` rerunnable script | AP44 |
| 6 | Ask how much can be deterministic | "how much of your skills and rules could actually be deterministic" | ~23:30 | partly | none | #80 PBI-19 |
| 7 | Codemods for migrations | "code mods ... crawling the abstract syntax tree" | ~24:00 | n/a | none | AP44 mentions codemod |
| 8 | Repeated mistake becomes a lint | "how do I turn this into a lint rule" | ~31:00 | partly | `engineering.md:70-73`, `retro.md:47-53` (enforcer ladder) | no; #80 PBI-17; AP34 box only |
| 9 | One way, feature dirs, no god files | "only one way to do something ... not going to append to a god file" | ~30:00 | partly | none on main; #77 `## Folders` (open) | no |
| 10 | Strict lint, narrow types | "type narrowing ... constraining the space" | ~29:00 | covered | language standards, no shared Type-System section | partly AP44 |
| 11 | Easy to change safely | "a good codebase is ... easy to make changes in" | ~25:00 | covered | guard tests | n/a |
| 12 | Engineer's job is the environment | "the new job of the engineer is really to spend time on the environment" | 26:14 | not ours | n/a | n/a |
| 13 | Context not control | "context not control" | 41:20 | partly | n/a (AW) | n/a |
| 14 | Outer loop feeds inner loop | "creating those two loops and connecting them" | 36:12 | covered (AW) | n/a | n/a |
| 15 | Where am I the bottleneck | "where am I the bottleneck in this process" | 37:58 | covered (AW) | none | no |
| 16 | Coordinator groups related reports | "you've lost that sort of thread between them" | 44:05 | covered (AW F73) | n/a | n/a |
| 17 | Reproduce on main first | "verify that the bug actually still exists on main" | ~37:00 | gap (AP) | `engineering.md:7` (prose) | yes AP38 bug-fix `Repro:`, AP40; #80 PBI-16 |
| 18 | Findings buffer | "I don't actually tell it to fix the issue first. I tell it to append it to a document" | 47:41 | covered | `docs/retros/` per run (`squad-retro`). Fleet's "learnings.md" does not exist on main | partly AP47 ledger (review only); #80 PBI-17 |
| 19 | Gardening is most of the volume | "a lot of the work actually is spent on gardening" | 45:51 | covered (#78) | #78, #81 open drafts | no |
| 20 | Sample, don't block | "it becomes more about sampling" | 49:34 | partly | none | no (AW AO10 only) |
| 21 | Verifier swarm fuzz, count = cost knob | "spawn a bunch of verifier agents ... it will fuzz" | 52:38 | not ours | one verifier | no (swarm skipped, CHECKLIST.md:194) |
| 22 | Review after landing, revert + lint | "I'll review it in the morning by looking at my commit history" | ~54:00 | partly | n/a (Sven merges) | no |
| 23 | One-way vs two-way doors | "some PRs are kind of like two-way doors" | ~56:00 | gap | `squad/SKILL.md:51`, `planning-contract.md:114` (rollout row) | metadata only (`door:` on each PBI); plan field in #80 PBI-15 |
| 24 | Formal verification | "lean or ta plus ... a solver" | 56:28 | not ours | none | no |
| 25 | Clear intent and goals | "express your intent and your goals in a clear way" | ~9:00 | covered | PBI template | n/a |
| 26 | Kill tautological tests | "reducing or eliminating tautological tests" | ~10:00 | gap (wrong) | `testing-principles.md:15-20` already covers it | n/a |
| 27 | Grilling | "that's why sort of grilling ... works effectively" | ~9:30 | covered | `squad-planner.md:24`, `CONTRIBUTE.md:184` | n/a |
| 28 | Mine transcripts | "look at your previous transcripts ... where you correct them" | ~1:00:00 | partly | none (retro per run only) | no (automate-me skipped CHECKLIST.md:57, recall skipped :156, reflect to AP29) |
| 29 | Skills get smaller | "skills get smaller and smaller" | 1:03:47 | covered | `CONTRIBUTE.md:98` Matt-tiny | n/a |
| 30 | Own your knives, mix libraries | "everyone should have their own set of knives" | 1:01:33 | covered (#73) | #73 merged | n/a |
| 31 | Measure skill impact | "quite difficult to measure ... the impact of the skill" | 1:58 | gap | none | no; #80 PBI-18 |
| 32 | Clean context | "some benefits to ... having a clean context window" | ~20:30 | partly | `squad-verifier.md:28`, `squad-implementer.md:27` | AP44 (Guard the context window) |

Tally. On main or in substance: 1, 5, 11, 17 (prose), 23 (row), 26, 27, 29, 30, 32 = 10. Only in open PR #80: 3, 4, 6, 8, 15-18 = 7 PBIs, none built. Nowhere in agentPacks: 2, 9, 20, 21, 28, 31 (31 only as #80 PBI-18).

## 4. What Fleet missed (in gist_full.txt, not in the 32 points)

| # | Learning | Quote | ts | Relevance to agentPacks |
|---|---|---|---|---|
| M1 | Trust ladder as staged autonomy: micromanage, verification, environment, self-merge. Each rung is earned | "climbing the trust ladder"; "I sort of realized that ... verification" | 1:58, 26:14 | High. No autonomy level per repo. Sven's merge exceptions (Chatmarkt, AW) are an ad-hoc ladder |
| M2 | Make the easy thing the right thing (agents take shortcuts) | "how do I make the easy thing the right thing" | ~7:00 | Medium. Design rule for skills/defaults |
| M3 | Compressed words the agent hooks onto (tautology, TDD) as a skill-writing technique | "you compress a lot of intent and meaning into words" | ~9:30 | Medium. pack-author style bar |
| M4 | Rules live in the environment, the agent "stumbles into" them, not a longer prompt | "you're not overloading your agent ... it stumbles into the rules" | ~32:00 | High. Pushes guards over `.mdc` prose (AP34 ordering) |
| M5 | Act only on repeats across agents; a one-off needs no fix | "if it was a one-off incident, it's fine ... multiple agents ... same shortcut" | 50:33 | Medium. Threshold for PBI-17 (2+) |
| M6 | Work backwards from "how does my agent merge its own code" | "starting from the question ... how do I get to the point where my agent can merge its own code" | ~38:30 | Medium. Goal statement for the trust ladder |
| M7 | Intent snapshot goes stale; inner loop needs refresh | "the snapshot can go stale" | 35:32 | Low for the pack (AW) |
| M8 | Divide work only when the sum beats the parts | "I'm not dividing work just for the sake of it" | 12:52 | Medium. Squad fan-out sizing |
| M9 | One coordinator per area (10+ chiefs of staff: perf, bugs, exploration) | "more than 10 chiefs of staff, right? Each working on a different area" | ~51:30 | Low (fleet, not pack) |
| M10 | A good environment onboards new hires from day one | "they can be productive from day one" | 46:12 | Low |
| M11 | Recall: carry context from past chats into a new one as a skill | "recall was just a way for me to collapse ... that workflow into a skill" | ~1:02:30 | Low. Skipped on purpose (CHECKLIST.md:156) |
| M12 | Perf evidence: traces, heap snapshots, flame graphs; the CLI wraps Playwright and CDP | "flame graphs and heap snapshots"; "Playwright and the Chrome DevTools protocol" | 4:52, ~21:30 | Medium. AP41 Drive kinds lack a perf/trace kind |
| M13 | Verifiability of the domain decides how far up the ladder you go; good verification turns one-way doors into two-way doors | "the oneway doors become two-way doors in a sense" | 56:28 | Medium. Pairs with the PBI-15 door field |
| M14 | Combine skill libraries into a personal mode | "Combine Wayfinder with potato mode and make your own custom mode" | ~1:04:00 | Low (automate-me skipped) |
| M15 | Verification is critical infrastructure the whole team uses (ownership/upkeep) | "it's become critical infrastructure for our team" | ~19:30 | Medium. No owner or upkeep routine for `docs/verify.md` |

Process misses by Fleet: no URL/date, no timestamps, the gist (a third party's secret copy) was trusted without a primary-source check, fold-ins were never verified, the god-file and tautology items rest on a wrong grep, and "learnings.md" does not exist (the buffer is `docs/retros/`).

## 5. Gaps against agentPacks (for #83)

| Gap | Points | Today | Proposal |
|---|---|---|---|
| G1 Trust-ladder level per repo | M1, M6, M13, 22, 23 | ad-hoc room rules | New PBI: `autonomy:` level (0 propose, 1 PR, 2 merge after gates) tied to verify coverage + door. Docs only |
| G2 God-file / one-way-to-do-it guard | 9 | none (#77 Folders is .NET only, open) | New PBI: size and feature-dir guard per language (check script + fixture) |
| G3 Sample review of merged work | 20, 22 | none in AP (AW AO10) | New PBI: `/squad-review --sample N` on merged PRs, repeats feed PBI-17 |
| G4 Hill climb / score loop | 2 | hillclimb skipped | Sharpen AP39 perf type: metric + bound + max 5 iterations (CHECKLIST.md:95 already sketches it) |
| G5 Verify upkeep and ownership | 3, M15 | PBI-20 drift check (#80), AP32 box only | Land AP32-AP34 in a PR, or fold an upkeep step into AP41 |
| G6 Perf/trace evidence kind | M12 | AP41 has api, cli, ui | Sharpen AP41: optional `trace` kind (profile artifact path as evidence) |
| G7 Transcript mining | 28, M11 | retro per run only | Keep skipped or route to AP29 reflect. Needs Sven's call (PA2 privacy) |
| G8 Skill impact | 31 | #80 PBI-18 only | Link from AP27 eval; no new PBI |
| G9 Verifier fan-out knob | 21 | one verifier | Defer. Note in AP46 as a budget option (1 default, N for one-way doors) |
| G10 Fold-ins never landed | 8, 26 | #79, #77 open without them | Fix in #83 docs: mark 26 as covered by `testing-principles.md:15-20`; the #79 asserts are tracked by #80 PBI-01/06 |
| G11 Skill wording rule | M3, M4, M2 | none | Sharpen AP44 or the pack-author bar: one compressed term per rule, environment over prose |
| G12 Split only when it pays | M8 | Squad size/risk routing (`squad/SKILL.md:45-52`) | Sharpen AP37: a split must name the shared resource it avoids |
