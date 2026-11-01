# PS04: Pin pstack unchanged: poteto-mode, 24 leafs, linked skills, full closure
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B1
- Depends on: PS03
- Lane: Builder (json + NOTICE)
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "get all principles-* skills ... etc add also principle skills".
- Sven: "en denk na om gwn hun skills te gebruiken maar dan goed te waven in onze flows". PO rule: pstack unchanged, pinned, never rewritten.
- PO decision 3: full poteto closure plus `/loop` allowlist, because Sven wants 0 warnings.

## Current state on main (`d76fa14`)

- [`plugins/squad/external-skills.json`](https://github.com/svennijhuis/agentPacks/blob/d76fa14/plugins/squad/external-skills.json) pins only `caveman` and `caveman-compress`.
- `ExternalSourceMaterializer` already materializes pinned dirs unchanged and injects `user-invocable: false` on the generated copy.

## Target structure

Records in `plugins/squad/external-skills.json`, each `repository: https://github.com/cursor/plugins`, `commit: ae5650febf4320793844c12c4adaa937f417f8da`, `license: MIT`, `path: pstack/skills/<name>`:

- Hub: `poteto-mode` (brings `playbooks/*.md`, `references/bugbot-triage.md`, `scripts/check-plan.mjs`).
- 24 leafs: attack-the-premise, boundary-discipline, build-the-lever, encode-lessons-in-structure, exhaust-the-design-space, experience-first, explain-the-number, fix-root-causes, foundational-thinking, guard-the-context-window, laziness-protocol, make-operations-idempotent, migrate-callers-then-delete-legacy-apis, minimize-reader-load, model-the-domain, never-block-on-the-human, outcome-oriented-execution, prove-it-works, redesign-from-first-principles, separate-before-serializing-shared-state, sequence-verifiable-units, subtract-before-you-add, test-behavior-not-implementation, type-system-discipline (each `principle-<x>`).
- Linked skills: statechart, architect, blast-radius, correct, create-formal-verification-skill, maintain-formal-verification-skill, create-verification-skill, maintain-verification-skill, figure-it-out, how, arena, interrogate, no-comments, setup-pstack, tdd, teach, technical-writing, unslop, why.
- Closure: swarm, reflect, show-me-your-work, benchmark-checklist, in-your-own-words, ketchup; `cursor-team-kit/skills/` deslop, control-cli, control-ui (same SHA).
- `typescript-best-practices` goes to the typescript plugin (PS17), not squad.

`plugins/squad/NOTICE.md`: MIT text plus "Copyright (c) 2026 Lauren Tan" for cursor/plugins pstack and cursor-team-kit.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| all | every pstack skill above | [pstack/skills](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/) | pinned as-is (`external-skills.json`, 40-hex) | see map section 2a, 2b |
| closure | deslop, control-cli, control-ui | [cursor-team-kit](https://github.com/cursor/plugins/tree/ae5650febf4320793844c12c4adaa937f417f8da/cursor-team-kit/skills) | pinned as-is (`external-skills.json`, 40-hex) | none |

## Subagents and what they load

- Plugin-level `pstack/agents/poteto-agent.md` and `comment-sicko.md` are not pinned (no external-agent record). Squad agents play those roles (PS15): squad-simplifier for comment-sicko.

## Steps for Builder

1. Add all records with the 40-hex SHA.
2. Add NOTICE.
3. Run generate for all providers; confirm materialized copies.
4. Add test: each materialized file byte-equal to upstream except the injected flag.

## DoD

- Materialized copies byte-equal to upstream except `user-invocable: false`.
- `SkillReferenceValidator` resolves every backticked ref in pinned skills (closure complete, `/loop` allowlisted).
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Count of records (1 + 24 + 19 + 9 = 53).
- Byte-compare test output.

## Risks and out of scope

- marketplace-beta is force-pushed, so its SHA is not durable for gh-aw pins: gh-aw needs a tag or durable generated ref (track in PS16 notes).
- statechart needs Bun, TLC and Java via mise, network on first run: not offline in gh-aw.
- Out of scope: using them in flows (PS06 to PS13).
