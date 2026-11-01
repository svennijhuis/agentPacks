# PS09: Squad playbooks: copy + adapt is the default
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B2
- Depends on: PS07
- Absorbs old: old 37, 38, 39, 40, 54, 61
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "every pbi bug ticket or every task ... can have different playbook ... if he see this pbi is bug he can use other playbook then new feature where is domain logic in it".

## Current state on main (`6c59f49`)

- No playbooks; one generic loop. No `Task type:` on main.

## Target structure

```text
plugins/squad/skills/squad-md/playbooks/
  bug-fix.md  feature.md  domain.md  perf.md  investigation.md  plan.md  review.md  pr.md  retro.md
```

Default for playbooks (Sven 2026-11-01): copy + adapt. We copy each upstream playbook into our squad folder and fit it to what we already have, so no standard, gate or rule gets lost. Only `principle-*` leafs and stand-alone skills we can use unchanged stay pinned.

Every copied playbook:
- starts with `Source: <upstream link at 40-hex SHA>, MIT, see NOTICE.` under the H3 title,
- ends with `#### What we changed or added` (one line per change),
- keeps our rules by naming the standard leafs (PS07): No Em Dash, Squash Merge One by One, Personal Account Only, No Secrets in Markdown, Review Contract, Coding Standards File,
- keeps our gates: one PBI per PR, one PR at a time, `docs/squad-plan.md` rows and Session split (#85), max 2 fix rounds, findings fixed on the PR,
- drops client-only parts (`poteto-agent`, model slugs, `/loop`, swarm, control-cli, control-ui).

Example head of `playbooks/bug-fix.md`:

```markdown
### Bug fix

Source: https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/bug-fix.md, MIT, see NOTICE.

1. Reproduce it yourself on the surface the user hit; write the repro row in `docs/squad-plan.md`.
...
Standards: **Review Contract**, **No Secrets in Markdown**.

#### What we changed or added
- Dropped `/loop` (client built-in).
- Added Matt diagnosing-bugs phases 1 to 6.
- Added the repro row and the one-PR rule.
```

Playbooks rule: the todo list starts with the matched playbook steps verbatim; a skipped step keeps `skip: <reason>`. Planner writes `Task type:` per task. No fit: figure-it-out.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| bug | [upstream playbook bug-fix](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/bug-fix.md) | copy + adapt | `squad-md/playbooks/bug-fix.md` | 15 lines, good steps, but uses `/loop` (client built-in). We drop `/loop`, add Matt diagnosing-bugs phases and our repro row. Keeps our standards and gates (PS09). |
| bug | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |
| feature | [upstream playbook feature](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/feature.md) | copy + adapt | `squad-md/playbooks/feature.md` | 21 lines. We add statechart for docs, codebase-design, our plan file rows. Keeps our standards and gates (PS09). |
| domain | [upstream playbook refactoring](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/refactoring.md) | copy + adapt | `squad-md/playbooks/domain.md` | Base for the domain playbook. We add model-the-domain, Matt domain-modeling, GLOSSARY and ADR rules. Keeps our standards and gates (PS09). |
| perf | [upstream playbook perf-issue](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/perf-issue.md) | copy + adapt | `squad-md/playbooks/perf.md` | 25 lines. Keep baseline and benchmark-checklist, add our evidence fields. Keeps our standards and gates (PS09). |
| perf | [benchmark-checklist](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/benchmark-checklist/SKILL.md) | pin | external-skills.json | Perf playbook. |
| investigation | [upstream playbook investigation](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/investigation.md) | copy + adapt | `squad-md/playbooks/investigation.md` | 14 lines, read-only. Almost unchanged; we add our file:line answer rule. Keeps our standards and gates (PS09). |
| plan | [upstream playbook multi-phase-plan](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/multi-phase-plan.md) | copy + adapt | `squad-md/playbooks/plan.md` | 155 lines and Cursor-heavy (`poteto-agent`, swarm, control-cli, control-ui, `/loop`). We take the phase and gate idea only and keep our `docs/squad-plan.md` (#85). Keeps our standards and gates (PS09). |
| pr | [upstream playbook opening-a-pr](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/playbooks/opening-a-pr.md) | copy + adapt | `squad-md/playbooks/pr.md` | Uses deslop and swarm. We merge it with Matt pr (Evidence, Merge Danger) and our squash and personal-only standards. Keeps our standards and gates (PS09). |
| fallback | [figure-it-out](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/figure-it-out/SKILL.md) | pin | external-skills.json | Fallback when no playbook fits. Resolves our arena copy by name. |

## Subagents and what they load

- planner: plan.md. implementer: bug-fix, feature, domain, perf. reviewer: review.md. retro: retro.md.

## Steps for Builder

1. Copy and adapt each playbook.
2. Playbooks index in hub.
3. `Task type:` in the planning contract.
4. One fixture per type.

## DoD

- Each fixture picks the right playbook and its first todo items.
- REF04 and REF06 pass.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- 9 fixture names.

## Risks and out of scope

- Out: Matt weaves (PS10).
