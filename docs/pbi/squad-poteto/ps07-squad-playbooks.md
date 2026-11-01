# PS07: Squad playbooks as thin wrappers per work type
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS06
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "I really like the playbook way of working every pbi bug ticket or every task ... can have different playbook of way of thinking ... if he see this pbi is bug he can use other playbook then new feature where is domain logic in it".
- Squad wraps upstream playbooks and adds only its gates. No upstream text copied.

## Current state on main (`d76fa14`)

- No `playbooks/` folder. Old AP37 to AP39 planned playbook files with paraphrased steps (now replaced by this PBI).

## Target structure

```text
plugins/squad/skills/squad-md/playbooks/
  bug-fix.md        feature.md      domain.md      review.md
  plan.md           retro.md        investigation.md
```

Each file starts at H3 and has this shape:

```markdown
### Bug fix

Run `../poteto-mode/playbooks/bug-fix.md`. Squad adds:
1. Failing repro row in `docs/squad-plan.md` before the fix.
2. Matt diagnosing-bugs phase 1 loop (PS08).
3. Verifier reruns the same repro.
Principles: **Fix Root Causes**, **Prove It Works**. Standards: **standard-review-contract**.
```

Playbooks rule (from poteto): open a todo list whose first items are the matched playbook steps copied verbatim; a skipped step keeps `skip: <reason>`. Fallback when nothing fits: **figure-it-out**.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| bug | poteto bug-fix + tdd + how + why | [poteto bug-fix](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/bug-fix.md) | rides with the poteto-mode pin | architect, interrogate, tdd |
| feature | poteto feature + how + architect + arena | [poteto feature](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/feature.md) | rides with the poteto-mode pin | interrogate, model-the-domain |
| domain | poteto refactoring + principle-model-the-domain + statechart | [poteto refactoring](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/refactoring.md) | rides with the poteto-mode pin | figure-it-out, how |
| review | interrogate + blast-radius + arena | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | how, arena |
| plan | poteto multi-phase-plan + teach + technical-writing + blast-radius | [poteto multi-phase-plan](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/multi-phase-plan.md) | rides with the poteto-mode pin | swarm, deslop, check-plan.mjs (Node) |
| retro | correct + maintain pairs | [correct](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/correct/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | why |
| investigation | poteto investigation + how + why + unslop | [poteto investigation](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/investigation.md) | rides with the poteto-mode pin | none |
| fallback | figure-it-out | [figure-it-out](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/figure-it-out/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | architect, arena, show-me-your-work |

## Subagents and what they load

- planner uses plan.md; implementer uses bug-fix, feature, domain; reviewer uses review.md; retro uses retro.md.

## Steps for Builder

1. Write 7 files with write-skill.
2. Add Playbooks index lines in the hub.
3. Planner writes `Task type:` per task; hub picks the file.
4. Fixture per type; test the pick and the first todo items.

## DoD

- One fixture per type picks the right playbook.
- REF04 resolves every pointer.
- Each file at most 15 lines (thin).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- 7 fixture names.
- Pick table in PR body.

## Risks and out of scope

- check-plan.mjs needs Node.
- Out of scope: Matt weaves (PS08).
