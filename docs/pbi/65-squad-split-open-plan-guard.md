# PBI-65 (AP65): Squad split open-plan guard and write-plan branch

- Status: todo
- Area: squad
- Phase: D gist
- Lane: Builder
- Depends on: 64
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter SquadSessionSplitTests` is green with new tests that fail when the guard or the branch below is removed
door: two-way
- triage: build (Cursor review 5725466796 on #85 at 4f75322, 3 threads left open after the one fix round, Tester delta PASS 2026-11-01)
- feedback: `SquadSessionSplitTests` turns red when the over-limit write has no open-plan guard or step 5 has no `Session split:` branch
- harness: none (skill prose plus fixtures)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: PA8 (new PBI carries its source; no D row; Cursor review 5725466796) https://github.com/svennijhuis/agentPacks/pull/85

## Goal

Close the 3 open findings from Cursor review 5725466796 on #85.

1. In `plugins/squad/commands/squad.md`, the normal-run over-limit step writes `docs/squad-plan.md` without the open-plan guard. It must ask replace or append once, like `/squad split`.
2. Step 5 `write-plan` has no `Session split:` branch. Add it, so a split plan is written by the same rules as the forced split.
3. `SquadSessionSplitTests` locks only the unguarded write sentence and the step 4 marker. Add tests that lock items 1 and 2.

## Definition of done

- The 3 items above are done, and the proof line is green.
- The squad SKILL body stays at or under its 87 line cap.
- Module boundary: the squad command and skill own the guard and the write-plan branch; `docs/squad-plan.md` stays the only state between runs
