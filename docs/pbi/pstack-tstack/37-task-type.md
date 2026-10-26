# PBI-37 (AP37): Task type in the plan
- Status: todo
- Area: squad
- Phase: F playbooks
- Lane: Builder (Cursor CLI, easy)
- Depends on: #80 merged (docs/pbi root and template). Edits the same Route table as AP26: AP37 lands first, AP26 rebases.
- proof: `dotnet test tools/AgentPacks.Cli.Tests --filter PlanLintTests` exits 0, and `bash plugins/pack-check/skills/pack-check/scripts/check-plan.sh tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-no-task-type/2026-10-27-login-retry.md` prints one PLAN12 line and exits 1
- door: two-way
- triage: build (Sven priority 1 on 2026-10-26: playbooks per task type first)
- feedback: `PlanLintTests` is green on the good plan and red with PLAN12 on `bad-no-task-type` and `bad-unknown-task-type`
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: script
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L121-L149

## Problem now

Squad picks a route only by size and risk: small, behavior, trust boundary, existing diff ([`plugins/squad/skills/squad/SKILL.md:45-52`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L45-L52)).
A bug fix, a refactor and a perf change all take the same "full loop".
So nothing asks a bug fix for a repro, or a refactor for a behavior pin.

## Why (plain words)

A bug fix and a refactor need different proof.
If the plan says which kind of work it is, the linter can ask for the right proof.
pstack does this by naming one playbook per task ([`poteto-mode/SKILL.md:121-123`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L121-L123)).
We do it with one line in the plan and one lint rule, not with more prose.

AP26 triage answers a different question: build, ask, park or duplicate.
AP37 answers how to build. Both lines go in the plan.

## Example

Plan head after AP37:

```markdown
# Login retry after token expiry

## Request
Users get logged out when the token expires during a save.

Task type: bug-fix
```

Allowed values: `feature`, `bug-fix`, `refactor`, `perf`, `investigation`, `prototype`.

New Route rows in [`skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md):

| Work | Route |
|---|---|
| Read-only question ("how does X work", "are we sure") | `investigation`: the main agent answers with `file:line` citations. No plan file, no agents, no retro. |
| A fork you can settle by running something | `prototype`: throwaway in `$(git rev-parse --git-dir)/squad/proto/<slug>/`, outside the worktree. The result decides. Then a `feature` plan. Uses AP31's pinned Matt `prototype` skill. |
| Work too big for one plan (many PRs, many days) | Stop. Say "too big for one /squad run" and list the PBIs Strategy should split it into (pstack `figure-it-out` and `orchestrate` adapted). |

## Before / after

| | Before | After |
|---|---|---|
| Plan without `Task type:` | passes | `docs/plans/2026-10-27-login-retry.md:1 PLAN12 missing or unknown Task type (fix: add 'Task type: feature, bug-fix, refactor, perf, investigation, prototype')`, exit 1 |
| `Task type: bugfix` | passes | same PLAN12 line with `unknown 'bugfix'` |
| Small change (`## Goal` plan) | passes | still passes. PLAN12 runs only on full-loop plans. |

## Steps

1. `planning-contract.md`: add the `Task type:` line under `## Request` for full-loop plans.
2. `check-plan.sh` + `.ps1`: add PLAN12. Switch the output to `<file>:<line> <ID> <message> (fix: ...)` for every rule (PLAN01 to PLAN11 get ids, text unchanged). See README "Old to new".
3. [`skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md): 3 new Route rows.
4. Fixtures under `tools/AgentPacks.Cli.Tests/Fixtures/plans/`: `good-bug-fix/`, `bad-no-task-type/`, `bad-unknown-task-type/`.
5. `PlanLintTests.cs`: assert the id per fixture, and assert that every existing message now carries its id.

## Definition of Done

- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter PlanLintTests` exits 0.
- [ ] Each bad fixture prints exactly one PLAN12 line and exits 1. The good fixture prints nothing and exits 0.
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0.
- [ ] No new command; `SkillInvocabilityGuardTests` stays green.
- [ ] No em or en dashes.

## Technical design

- Module boundary: the plan text is the interface. check-plan reads one line. No agent prompt changes beyond the planning contract.
- Rules: PLAN12 (README rule table). The ids PLAN01 to PLAN11 are added here because this PR touches the output format.

## Sources

- pstack [`poteto-mode/SKILL.md:121-149`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md#L121-L149) (playbook list, "Match the task to a playbook"), [`playbooks/investigation.md:5-14`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/investigation.md#L5-L14), [`playbooks/prototype.md:5-14`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/prototype.md#L5-L14), [`skills/figure-it-out/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/figure-it-out/SKILL.md) (description), [`playbooks/orchestrate.md:1-10`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/orchestrate.md#L1-L10).
- agentPacks [`plugins/squad/skills/squad/SKILL.md:45-52`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md#L45-L52), [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- edit: [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
- edit: [`plugins/squad/skills/squad/references/planning-contract.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/references/planning-contract.md)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh)
- edit: [`plugins/pack-check/skills/pack-check/scripts/check-plan.ps1`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.ps1)
- edit: [`tools/AgentPacks.Cli.Tests/PlanLintTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/PlanLintTests.cs)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-no-task-type.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-no-task-type.md)
- new: [`tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-unknown-task-type.md`](../../../tools/AgentPacks.Cli.Tests/Fixtures/plans/bad-unknown-task-type.md)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/figure-it-out/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/figure-it-out/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/investigation.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/investigation.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/orchestrate.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/orchestrate.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/prototype.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/prototype.md)
- agentPacks main (existing files): [`plugins/pack-check/skills/pack-check/scripts/check-plan.sh`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/pack-check/skills/pack-check/scripts/check-plan.sh)
- agentPacks main (existing files): [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
