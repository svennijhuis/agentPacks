# Unified ruleset (AP37 to AP63)

AP63 moves this table to `shared/standards/rules.md` (ids unchanged). Until then this file is the home.

One home per rule, one output format, stable ids. Index: [README.md](README.md).

## ID scheme

`<PREFIX><NN>`. The prefix names the one place the rule lives. NN is two digits, never reused, never renumbered. A retired id stays in the table as `retired`.

| Prefix | Home (the only place the rule lives) | Run by |
|---|---|---|
| PLAN | [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh) + `.ps1` | `/squad` on every plan edit; `PlanLintTests` |
| VER | `.../scripts/check-verify.sh` + `.ps1` | verifier, verify-harness, `/scenarios`; `VerifyLintTests` |
| RETRO | `.../scripts/check-retro.sh` + `.ps1` | `/squad`, `/squad-review` after the retro; `PackCheckContractTests` |
| SCN | retired 2026-11-01 (AP42 dropped, no `check-scenarios` script) | none |
| REV | `.../scripts/check-review.sh` + `.ps1` (new in AP47) | `/squad-review`; `ReviewLoopTests` (new) |
| VCLI | `tools/AgentPacks.Cli/Validation/VerifyCliValidator.cs` (planned, AP33; not on main yet) | `agentpacks validate`; `VerifyCliContractTests` |
| SQD | [`tools/AgentPacks.Cli.Tests/SquadContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/SquadContractTests.cs) (rules about our own pack files) | `dotnet test` |
| DIFF | `.../scripts/check-diff.sh` + `.ps1` (new in AP57) | `/squad` verifier on the working diff; `DiffLintTests` (new) |
| BND | [`tools/AgentPacks.Cli.Tests/LanguagePackContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/LanguagePackContractTests.cs) (AP55) | `dotnet test` |
| PAT, RUL | `tools/AgentPacks.Cli.Tests/StandardsCatalogTests.cs` (new in AP62) | `dotnet test` |
| TPL, ARC, A, DONE, LINK | `scripts/check-pbi-template.sh`, `scripts/check-pbi-docs.sh` (planned, AP36; not on main yet; ported from agentic-workflows, names kept so both repos match) | one more `run:` step inside the existing `validate` job in [`.github/workflows/validate.yml`](https://github.com/svennijhuis/agentPacks/blob/7dc0435fb88be4efa4a6e2bfd63556d668a0ea6c/.github/workflows/validate.yml), no new job or workflow |

`docs/GUARDS.md` (planned, AP34; not on main yet) lists these ids. It does not make new ones.

## Output format (every script)

`<file>:<line> <ID> <message> (fix: <one action>)` on stdout. `error` rules make the script exit 1. `warn` rules print `<file>:<line> <ID> warning: <message>` and keep exit 0. Exit 2 = usage, missing tool or missing base. Same format as AP36.

Rule severity (error, warn) is not finding severity (high, medium, low, tiny). Rule severity is for linters. Finding severity is for reviewers.

## Rule table

| Id | Severity | Lives in | Message | Fix | Source PBI |
|---|---|---|---|---|---|
| PLAN01 | error | check-plan | file name must be <yyyy-mm-dd>-<slug>.md | rename the file | existing |
| PLAN02 | error | check-plan | contains an em dash | use a period or comma | existing |
| PLAN03 | error | check-plan | missing ## Status value | add `## Status` with a value | existing |
| PLAN04 | error | check-plan | missing ## <section> | add the section for this Status | existing |
| PLAN05 | error | check-plan | missing ## Design or 'Design: skipped (small)' | add one of the two | existing |
| PLAN06 | error | check-plan | hard decision without a docs/adr/ link | link the ADR | existing |
| PLAN07 | error | check-plan | ADR link must be docs/adr/NNNN-<slug>.md | rename the link | existing |
| PLAN08 | warn | check-plan | slice has no files line | add `Files:` to the slice | existing |
| PLAN09 | warn | check-plan | slice has no done check | add `Done:` to the slice | existing |
| PLAN10 | warn | check-plan | one slice and no 'Single slice because:' line | add the reason | existing |
| PLAN11 | warn | check-plan | ## Map has no fenced mermaid block | add a mermaid block | existing |
| PLAN12 | error | check-plan | missing or unknown Task type | `Task type: feature, bug-fix, refactor, perf, investigation, prototype` | AP37 |
| PLAN13 | error | check-plan | bug-fix plan has no Repro: line | add `Repro: <surface> \`<cmd>\`` | AP39 (absorbs PBI-16) |
| PLAN14 | error | check-plan | refactor plan has no Pin: line | add `Pin: \`<test cmd>\`` | AP39 |
| PLAN15 | error | check-plan | perf plan has no Baseline: line | add `Baseline: \`<cmd>\` <metric>` | AP39 |
| PLAN16 | error | check-plan | feature plan has no Data shape: line | add `Data shape: <type or table>` | AP39 |
| PLAN17 | error | check-plan | Repro:/Pin:/Baseline: needs a surface and a command in backticks | write `<api or test or trace> \`<cmd>\`` | AP39 |
| VER01 | error | check-verify | missing ## <H2> | add Launch, Doctor, Drive, Evidence, Cleanup | existing |
| VER02 | error | check-verify | em dash | use a period or comma | existing |
| VER03 | error | check-verify | secret-looking value | use a placeholder or `$(...)` | existing |
| VER04 | error | check-verify | get-access-token outside $( | wrap it in `$(...)` | existing |
| VER05 | error | check-verify | Drive entry has no Kind: api or trace | add `Kind:` (api or trace) | AP41 |
| VER06 | retired | check-verify | retired 2026-11-01: Sven removed CLI driving (was: cli Drive entry waits with sleep) | none | AP41 |
| VER07 | retired | check-verify | retired 2026-11-01: Sven removed UI driving (was: ui Drive entry installs browser tooling or has no screenshot path) | none | AP41 |
| VER08 | error | check-verify | reserved: missing or renamed harness command | owned by PBI-20 | PBI-20 |
| RETRO01 | error | check-retro | file name must be <yyyy-mm-dd>-<slug>.md | rename | existing |
| RETRO02 | error | check-retro | line N contains an em dash | use a period or comma | existing |
| RETRO03 | error | check-retro | reserved: repeated lesson without Proposed check: | owned by PBI-17 | PBI-17 |
| SCN01 | retired | check-scenarios | retired 2026-11-01: AP42 dropped (was: unknown Kind) | none | AP42 (dropped) |
| SCN02 | retired | check-scenarios | retired 2026-11-01: AP42 dropped (was: CLI row has no exit code) | none | AP42 (dropped) |
| SCN03 | retired | check-scenarios | retired 2026-11-01: AP42 dropped (was: UI row has no selector) | none | AP42 (dropped) |
| REV01 | error | check-review | round number above budget | stop the loop; escalate to Sven | AP46 |
| REV02 | error | check-review | budget 5 without a risk reason | add `Budget: 5 (security, auth, data loss or concurrency)` | AP46 |
| REV03 | error | check-review | out-of-budget review marked PASS | mark it BLOCKED | AP46 |
| REV04 | warn | check-review | later round lists a finding outside changed code and not an open id | drop it or open a new id with reason | AP46 |
| REV05 | error | check-review | finding without id, severity or file:line | write `F<nn> [<severity>] <file>:<line> <claim>` | AP47 |
| REV06 | error | check-review | finding id reused or renumbered | keep the first id | AP47 |
| REV07 | error | check-review | ledger row without outcome fixed, disagree or declined | add the outcome | AP47 |
| REV08 | error | check-review | ledger row without evidence | add a SHA, file:line, test or command output | AP47 |
| REV09 | error | check-review | item disputed after 2 replies and no `Escalate: Sven` | add the escalation | AP47 |
| REV10 | error | check-review | PR body review line has the wrong shape | use the AP48 line | AP48 |
| REV11 | warn | check-review | old PR reviewed without a disposition line | add MODERNIZE, REWORK or CLOSE | AP49 |
| SQD01 | error | SquadContractTests | task type without a playbook file | add `references/playbooks/<type>.md` | AP38 |
| SQD02 | error | SquadContractTests | playbook file not linked from squad SKILL.md, or over 20 lines | link it; cut it | AP38 |
| SQD03 | error | SquadContractTests | verifier contract lacks the before/after rule for bug-fix and perf | add the row | AP40 |
| SQD04 | error | SquadContractTests | an agent cites an engineering.md section that does not exist | fix the name | AP44 |
| SQD05 | error | SquadContractTests | handoff.md lacks one of Why, What changed, Scope, Blast radius, Verification | add the heading | AP45 |
| VCLI01 to VCLI06 | error | VerifyCliValidator | see AP33 | see AP33 | AP33 (unchanged) |
| TPL01, TPL02, ARC08, A1, DONE, LINK | error | `check-pbi-*.sh` (step in the `validate` job) | see AP36 | see AP36 | AP36 (unchanged) |
| PLAN18 | error | check-plan | perf plan with Loop: hillclimb needs Target:, Max iterations: (<= 5) and Log: | add the three lines | AP39 (update) |
| PLAN19 | warn | check-plan | slice creates a file under a concern with no Example in docs/paved-paths.md | add Example to the row | AP60 |
| VER09 | error | check-verify | trace Drive entry needs Artifact: and Metric: | add both | AP41 (update) |
| VER10 | warn, later error | check-verify | missing ## Features or a feature row without Reach, Drive, Proof or Source | add the table or fill the row | AP52 |
| VER11 | warn | check-verify | Drive entry has more than 3 lines of inline shell | move it into the verify CLI | AP52 |
| VER12 | error | check-verify | Features Source path does not exist | update the row | AP53 |
| VER13 | error | check-verify | budget row needs Metric, Limit, Command and Runs >= 3 | fill the row | AP54 |
| VER14 | error | check-verify | trust level claims evidence that is missing | lower the level or add the evidence | AP56 |
| REV12 | error | check-review | finding has no Guard: | add Guard: <layer> | AP51 |
| REV13 | warn | check-review | Guard: doc without why not lint: | add the reason or pick a stronger layer | AP51 |
| RETRO04 | error | check-retro | weeds row needs Date, PR, file:line, Smell, Guard, Seen | fill the row | AP59 |
| DIFF01 | warn | check-diff | new file over 400 lines | split by feature folder | AP57 |
| DIFF02 | warn | check-diff | file grew past 400 lines | split by feature folder | AP57 |
| DIFF03 | error | check-diff | workaround comment added | move the constraint into a test, type or lint, or add keep: <reason> <link> | AP58 |
| DIFF04 | error | check-diff | guard baseline grew | fix the new case instead | AP58 |
| DIFF05 | warn | check-diff | new file in a folder no paved-paths row covers | move the file or add a row | AP57 |
| BND01 | error | LanguagePackContractTests | pack declares a dependency rule but ships no boundary test template | add templates/boundary-test | AP55 |
| SQD06 | error | SquadContractTests | squad-repro missing, user-invocable, or verdicts not named | fix the skill file | AP61 |
| PAT01 to PAT03 | error | StandardsCatalogTests | pattern row shape, unique id, enforcement named | fix the row | AP62 |
| RUL01 to RUL03 | error | StandardsCatalogTests | rule row shape, unique id, enforcement named | fix the row | AP63 |

## Old to new

| Old (main or planned) | New id | Change |
|---|---|---|
| check-plan `file name must be ...` | PLAN01 | id added, text unchanged |
| check-plan `contains an em dash` | PLAN02 | same |
| check-plan `missing ## Status value` | PLAN03 | same |
| check-plan `missing ## <s>` (9 call sites) | PLAN04 | one id for all required sections |
| check-plan `missing ## Design or ...` | PLAN05 | same |
| check-plan ADR rules (2) | PLAN06, PLAN07 | same |
| check-plan 4 warnings | PLAN08 to PLAN11 | same, still warn |
| PBI-16 "bug-fix plan carries a failing repro step" (contract test) | PLAN13 | one home: check-plan, tested by `PlanLintTests`; PBI-16 points here |
| check-verify `missing ## <s>` | VER01 | id added |
| check-verify `em dash` | VER02 | same |
| check-verify `secret-looking value` | VER03 | stays the only secret rule; REV calls check-verify on their files instead of copying it |
| check-verify `get-access-token outside $(` | VER04 | same |
| PBI-20 drift check | VER08 | id reserved, PBI-20 owns it |
| check-retro filename, em dash | RETRO01, RETRO02 | id added |
| PBI-17 repeat lesson | RETRO03 | id reserved, PBI-17 owns it |
| review-contract `high, medium, low, tiny` | unchanged | sol-loop words become aliases (L2) |
| AP25 `severe, important, nit` (planned) | none | AP25 uses the L2 alias instead |
| AP33 VCLI01 to VCLI06 | unchanged | already in the scheme |
| AP36 TPL, ARC, A, DONE, LINK | unchanged | AW names kept |
| em dash in 3 scripts | PLAN02, VER02, RETRO02 | three homes, because each script lints its own file type. Not a duplicate. |
