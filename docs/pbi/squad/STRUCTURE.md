# Structure of the squad pack (v2 target)

Where every kind of file lives after PS01 to PS20 and GA01 to GA07, what it is, one example, and how to add or maintain it. How each kind looks: [SHAPES.md](SHAPES.md). Moving from v1: [MIGRATION.md](MIGRATION.md). gh-aw use: [gh-aw/README.md](gh-aw/README.md). Flows: [FLOWS.md](FLOWS.md). Reuse rule: [README](README.md#reuse-mode-per-skill).

## Target tree

```text
plugins/squad/
  commands/
    squad.md  squad-review.md  scenarios.md  statechart.md      # thin: load a hub, run its flow
  agents/
    squad-planner.md  squad-architect.md  squad-indexer.md
    squad-implementer.md  squad-verifier.md  squad-reviewer.md
    squad-simplifier.md  squad-security-reviewer.md
    squad-orchestrator.md  squad-retro.md                       # each: hub path, named leafs, return shape
  skills/
    squad-md/
      SKILL.md                     # hub: Non-negotiables, Principles, Autonomy, Subagents,
                                   #      Writing the reply, Comments, Playbooks, Standards
      playbooks/                   # copy + adapt of upstream playbooks, plus our own
        bug-fix.md feature.md domain.md perf.md investigation.md
        plan.md review.md pr.md retro.md
      references/                  # contracts we own (planning, review, session split, adr, ...)
        templates/CODING_STANDARDS.md
    standard-no-em-dash/SKILL.md   # one leaf per standard, same shape as a principle leaf
    standard-review-contract/SKILL.md
    standard-...                   # 9 leafs (PS07)
    tdd/  statechart-md/  arena/  no-comments/   # copy + adapt of pstack skills
    scenarios-md/  verify-harness/                # ours
  external-skills.json             # pins at 40-hex SHA: 24 principle-* leafs, architect,
                                   # blast-radius, interrogate, ... , Matt codebase-design
  NOTICE                           # MIT pstack, MIT Matt, Apache-2.0 humanlayer credit
  aw-closure.json                  # generated: hub + every skill it reads, for gh-aw skills: (GA01)
plugins/pack-author/skills/pack-author-md/
  SKILL.md
  playbooks/ write-skill.md check-skill.md write-subagent.md check-subagent.md change.md
shared/standards/*.md              # stays the gh-aw and language-pack import source
tests/fixtures/gh-aw/*.md          # workflows that must pass gh aw compile --strict (GA04)
docs/squad-plan.md                 # run state in the target repo (#85), unchanged
```

## Per kind

| Kind | What it is | Lives in | Example | Add or maintain with |
|---|---|---|---|---|
| Command | Thin entry the user types. Loads a hub, nothing else. | `plugins/squad/commands/<name>.md` | `statechart.md` loads `statechart-md` | write-skill (command section), then `validate` |
| Hub | One skill per command family that routes: rules first, then indexes. | `skills/squad-md/SKILL.md` | `## Playbooks` lists Task type to playbook | write-skill, check-skill |
| Playbook | Ordered steps for one task type. Copy + adapt by default. | `squad-md/playbooks/<type>.md` | `bug-fix.md`: repro row, failing test, fix, verify | write-skill (playbook mode); Source line and `What we changed or added` kept current |
| Principle | How to think. Pinned upstream leaf, never copied. | `external-skills.json` | `principle-fix-root-causes` | bump the SHA in one PR; REF02 checks the index |
| Standard | Our rule that must hold. Same shape as a principle leaf plus how to check. | `skills/standard-<x>/SKILL.md` | `standard-no-em-dash` | write-skill (standard kind), index line in hub; REF03 |
| Copied skill | Upstream skill we must change (gates, client parts). | `skills/<name>/` | `tdd` (runs on every behavior change) | edit the copy, update `What we changed`; REF06 |
| Pinned skill | Upstream skill that fits unchanged. | `external-skills.json` | `blast-radius` | bump SHA; check upstream diff first |
| Woven part | A few steps from a Matt skill inside our own file. | the file that uses it | two review axes in `review-contract.md` | edit our file; NOTICE line stays |
| Subagent | Fresh context with one job. | `plugins/squad/agents/<name>.md` | `squad-verifier` returns PASS or FAIL with numbers | write-subagent, check-subagent |
| Contract | Shared format between agents. | `squad-md/references/` | `planning-contract.md` | write-skill (change mode); tests in `SquadContractTests` |
| Template | File the squad writes into a target repo. | `references/templates/` | `CODING_STANDARDS.md` | write-skill; fixture shows planner proposes it |

## Next to poteto and Matt

| Part | poteto (pstack) | Matt skills | Ours | What we take, where we differ |
|---|---|---|---|---|
| Hub | `poteto-mode/SKILL.md` | none (flat skills) | `squad-md/SKILL.md` | Take the H2 order. Differ: add `## Standards`, no Cursor model slugs or `poteto-agent`. |
| Rules first | Non-negotiables | none | Non-negotiables | Same. Ours route to tdd, unslop, no-comments and our gates. |
| Principles | `principle-*` leafs, index in hub | none | same leafs, pinned | Same, unchanged. |
| Standards | none | none | `standard-*` leafs, index in hub | New, built in the principle shape plus a "how to check" line. |
| Playbooks | `poteto-mode/playbooks/*.md` | none | `squad-md/playbooks/*.md` | Copy + adapt. Differ: our plan file, one PR per PBI, no `/loop` or swarm. |
| Skills | one folder per skill | one folder per skill, grouped by area | one folder per skill | Same. Matt parts are woven, not copied whole. |
| Subagents | `poteto-agent` (Cursor) | none | 10 `squad-*` agents | Ours, with hub path and named leafs in every prompt. |
| Commands | Cursor slash and built-ins | none | 7 thin commands | Ours. Planning stays ours (`docs/squad-plan.md`). |
| Licence | MIT | MIT | NOTICE lists both | Source line with SHA on every copy. |
