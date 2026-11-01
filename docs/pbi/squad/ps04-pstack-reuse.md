# PS04: pstack reuse per skill: pin, copy + adapt, or not used
- Status: todo (docs only; build waits for Sven go per PBI)
- Batch: B1
- Depends on: PS03
- Absorbs old: old 44 (half: principles paraphrased in shared/standards/engineering.md)
- Lane: Builder (Cursor CLI for docs and markdown, Cloud Agent if multi-file C#)
- door: two-way
- Source: [skill map](sources/pstack-matt-skill-map.md), pstack `ae5650f`, Matt `4ae2317`, flows in [FLOWS.md](FLOWS.md)

## Goal and why

- Sven: "maar niet alle externe skills kunnen we zo inlaten soms delen kopie paste en verander en voeg onze dingen toe".
- Sven: "get all principles-* skills", "gwn hun skills te gebruiken maar goed te waven in onze flows".

## Current state on main (`6c59f49`)

- [`plugins/squad/external-skills.json`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/plugins/squad/external-skills.json) pins only caveman and caveman-compress.
- [`shared/standards/engineering.md`](https://github.com/svennijhuis/agentPacks/blob/6c59f49/shared/standards/engineering.md) paraphrases 11 principles (Fix root causes to Sequence verifiable units).

## Target structure

Reuse table: see [README reuse table](README.md#reuse-mode-per-skill). This PBI adds the pin records and the copies listed there. Every copy starts with `Source: <upstream link at 40-hex SHA>, MIT, see NOTICE.` under the H1 and ends with `## What we changed` (one line per change). Scripts and assets stay byte-equal unless listed there.

Changes against PO decision 3 (with reason): the full poteto closure shrinks because we do not pin the poteto-mode hub (Cursor-only text), and the copied playbooks drop swarm, control-cli, control-ui and `/loop`. Closure is what REF01 finds after the copies, listed in the PR.

Migrate then delete (principle): `shared/standards/engineering.md` sections point to the pinned leafs, then the paraphrase goes. `StandardsGhAwImportContractTests` (from #79) stays green.

## External skills per step

| Step | Skill | Reuse mode | Lives in | Reason |
|---|---|---|---|---|
| all pins | [24 principle-* leafs](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | pin | `plugins/squad/external-skills.json` | Generic, short, no client-specific text. Exactly what Sven asked ("get all principles-* skills"). Replaces our paraphrased `shared/standards/engineering.md` sections. |
| all pins | [architect (+ design-red-flags)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/architect/SKILL.md) | pin | external-skills.json | Client neutral, used as is. |
| all pins | [blast-radius](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/blast-radius/SKILL.md) | pin | external-skills.json | Used as is in plan, task and review. |
| all pins | [correct](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/correct/SKILL.md) | pin | external-skills.json | Used as is in retro; covers old lint-first and guard ladder PBIs. |
| all pins | [interrogate](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/interrogate/SKILL.md) | pin | external-skills.json | Used as is in review. |
| all pins | [how](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/how/SKILL.md) | pin | external-skills.json | Used as is. |
| all pins | [why](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/why/SKILL.md) | pin | external-skills.json | Used as is. |
| all pins | [teach](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/teach/SKILL.md) | pin | external-skills.json | Used as is in plan. |
| all pins | [figure-it-out](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/figure-it-out/SKILL.md) | pin | external-skills.json | Fallback when no playbook fits. Resolves our arena copy by name. |
| all pins | [technical-writing](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/technical-writing/SKILL.md) | pin | external-skills.json | Docs and CODING_STANDARDS.md. |
| all pins | [unslop](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/unslop/SKILL.md) | pin | external-skills.json | Every prose output. |
| all pins | [create-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-verification-skill/SKILL.md) | pin | external-skills.json | Used as is; covers old feature map PBIs. |
| all pins | [maintain-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/maintain-verification-skill/SKILL.md) | pin | external-skills.json | Used as is in retro. |
| all pins | [create-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/create-formal-verification-skill/SKILL.md) | pin | external-skills.json | Risky code only. |
| all pins | [maintain-formal-verification-skill](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/maintain-formal-verification-skill/SKILL.md) | pin | external-skills.json | Retro, risky code only. |
| all pins | [benchmark-checklist](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/benchmark-checklist/SKILL.md) | pin | external-skills.json | Perf playbook. |
| all pins | [show-me-your-work, in-your-own-words](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/show-me-your-work/SKILL.md) | pin | external-skills.json | Closure: named by figure-it-out and interrogate. Final closure list comes from REF01, listed in the PR. |
| copies | [tdd](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/tdd/SKILL.md) | copy + adapt | `plugins/squad/skills/tdd/` | Upstream has `disable-model-invocation: true` and says "use only when the user explicitly asks". Sven wants it on every behavior change ("We can use this really hard"). So we change the trigger and the flag; the loop stays. |
| copies | [arena](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/arena/SKILL.md) | copy + adapt | `plugins/squad/skills/arena/` | Picks models by setup-pstack roles (Cursor only). We map to our tiers, cap at 3 candidates and high-risk work only (token budget). |
| copies | [no-comments](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/no-comments/SKILL.md) | copy + adapt | `plugins/squad/skills/no-comments/` | Spawns the `comment-sicko` plugin agent, which we cannot pin. We point it to `squad-simplifier`. |
| not used | [poteto-mode (hub)](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | not used | structure copied into `squad-md` | Hub names `poteto-agent`, Cursor model slugs and Cursor Task. Would break on Claude, Copilot, Codex. We copy its H2 structure only, not the file. |
| not used | [setup-pstack](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/setup-pstack/SKILL.md) | not used | `docs/PSTACK-ROLES.md` | Writes `~/.cursor/rules` (Cursor only, and no long-term setup on the shared box). We document role to tier instead. |
| not used | [swarm, reflect, ketchup, deslop, control-cli, control-ui](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/swarm/SKILL.md) | not used | none | Only needed by the Cursor-heavy playbooks we copy and trim. control-cli and control-ui: Sven removed CLI and UI driving on 2026-11-01. Pinned later only if REF01 still finds a reference. |

## Example

Good (what the result looks like):

```json
{ "name": "principle-fix-root-causes",
  "repo": "cursor/plugins",
  "sha": "ae5650febf4320793844c12c4adaa937f417f8da",
  "path": "pstack/skills/principle-fix-root-causes" }
```

Bad (what we do not want):

```json
{ "name": "principle-fix-root-causes", "repo": "cursor/plugins", "ref": "main" }
```
Branch instead of 40-hex SHA: the text can change under us.

## Subagents and what they load

- none here; loads are set in PS15

## Steps for Builder

1. Pin records at `ae5650febf4320793844c12c4adaa937f417f8da`.
2. Copies tdd, arena, no-comments under `plugins/squad/skills/` with the copy rule.
3. NOTICE: MIT text, Copyright (c) 2026 Lauren Tan.
4. Migrate `shared/standards/engineering.md` to pointers.
5. Byte-compare test for pins (except injected `user-invocable: false`).

## DoD

- REF01 resolves everything with the listed closure.
- REF06 passes for 3 copies.
- gh-aw import test green.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0, 0 errors, 0 warnings.
- `/pack-check` locally gives the same result (same rules, one source).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Record count, closure list, byte-compare output.

## Risks and out of scope

- marketplace-beta is force-pushed; gh-aw needs a durable SHA for skills.
- Out: playbook copies (PS09), statechart copy (PS12).
