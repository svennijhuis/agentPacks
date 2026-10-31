# PBI-38 (AP38): Playbook files per task type
- Status: todo
- Area: squad
- Phase: F playbooks
- Lane: Builder (Cursor CLI, easy: docs)
- Depends on: [PBI-37](37-task-type.md)
- proof: `dotnet run --project tools/AgentPacks.Cli -- validate && dotnet test tools/AgentPacks.Cli.Tests --filter SquadContractTests` exits 0, and the fixture with a missing `perf.md` fails with SQD01
- door: two-way
- triage: build (Sven priority 1, 2026-10-26)
- feedback: `SquadContractTests` fails with SQD01 when a task type has no file, and with SQD02 when a file is not linked or is over 20 lines
- harness: none (script)
- review-input: diff + this PBI + DoD (+ linked spec) + PR description, never builder session logs (PA6)
- kind: skill
- Source: D39 (proposed) https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md and D40 (proposed) https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/full-send/SKILL.md#L65-L80

## Problem now

Squad has one generic loop.
The steps that make a bug fix, a refactor or a perf change trustworthy are not written anywhere the agent reads.

## Why (plain words)

Each kind of work has two or three steps that matter most.
A bug fix: reproduce it yourself first.
A refactor: pin the current behavior first.
A perf change: measure first, with the same command you measure with after.
One short file per kind, loaded only for that kind, keeps the main skill thin.

## Files and content (paraphrased, max 15 lines each)

`plugins/squad/skills/squad/references/playbooks/`:

| File | Steps (short) | From |
|---|---|---|
| `feature.md` | Name the data shape first (`Data shape:`). Slices = the throughput checkpoint: blocking first, independent next, shared writes in one slice. Verify on the surface the user touches. | pstack [`feature.md:5-17`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/feature.md#L5-L17) |
| `bug-fix.md` | Reproduce it yourself on the same surface (`Repro:`). Narrow the cause with evidence, not guesses. Failing repro lands before the fix. Smallest fix the evidence justifies; a "might help" change does not ship. Rerun the same repro after. | pstack [`bug-fix.md:5-15`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md#L5-L15), tstack `full-send:65-80` |
| `refactor.md` | Pin behavior first (`Pin:`; type check and lint are not a pin). Subtract before you add. Small steps, pin green each step. A found bug or feature splits out. | pstack [`refactoring.md:5-16`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/refactoring.md#L5-L16) |
| `perf.md` | Baseline first (`Baseline:`), same command after. Name the limiter. 5 runs per side, alternating. Count errors. Check the work actually ran. Cheapest fix first: don't do it, do it once, do it less, later, concurrently, cheaper. | pstack [`perf-issue.md:5-25`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/perf-issue.md#L5-L25), [`benchmark-checklist/SKILL.md:15-32`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/benchmark-checklist/SKILL.md#L15-L32) |
| `investigation.md` | Read only. Answer with `file:line`. Say what you could not check. | pstack [`investigation.md:5-14`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/investigation.md#L5-L14), `runtime-forensics.md`, `trace-forensics.md` (read-only part only) |
| `prototype.md` | Throwaway, outside the worktree. Name the decision it settles. Compare 2 or 3 variants. Never merged; Feature follows. | pstack [`prototype.md:5-14`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/prototype.md#L5-L14); points to AP31 Matt `prototype` |
| `README.md` | One mermaid router: Task type to file. | pstack [`docs/guide/02-poteto-mode.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/docs/guide/02-poteto-mode.md) |

[`skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md) gets one line: "Read `references/playbooks/<Task type>.md` before the first planner round."

## Example (bug-fix.md, full)

```markdown
# Bug fix

1. Reproduce it yourself on the surface the user hit. Write it as `Repro: <api|test|trace> \`<command>\``.
2. If it does not reproduce, tighten the input or add logging until it does. Ask the user only with a reason the harness cannot reach it.
3. Narrow the cause: one hypothesis at a time, each with evidence. Do not fix what you have not seen fail.
4. Slice 1 adds the failing test or repro. The fix comes after.
5. Smallest change the evidence justifies. A change that "might help" is not a fix.
6. The verifier reruns the same `Repro:` before and after (AP40).
```

## Before / after

| | Before | After |
|---|---|---|
| Agent context for a bug fix | full SKILL.md + contracts | same + one 6-line `bug-fix.md` |
| Where "repro first" lives | [`shared/standards/engineering.md:7`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md#L7) (one line, all work) | there + bug-fix.md + PLAN13 gate (AP39) |

## Definition of Done

- [ ] 6 playbook files + README exist, each under 20 lines.
- [ ] `dotnet test tools/AgentPacks.Cli.Tests --filter SquadContractTests` exits 0; fixture without `perf.md` fails with SQD01; fixture with an unlinked file fails with SQD02.
- [ ] `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 and the 4 provider trees contain the files.
- [ ] Each file has a `Source:` line to D39 or D40. Paraphrase only; no copied paragraphs (no NOTICE needed).
- [ ] No em or en dashes.

## Technical design

- Module boundary: `references/playbooks/<type>.md` loaded by path from squad SKILL.md. No new skill, no new command.
- Rules: SQD01, SQD02.

## Sources

- pstack `poteto-mode/playbooks/{feature,bug-fix,refactoring,perf-issue,investigation,prototype}.md` (line ranges in the table), [`benchmark-checklist/SKILL.md:15-32`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/benchmark-checklist/SKILL.md#L15-L32), [`docs/guide/02-poteto-mode.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/docs/guide/02-poteto-mode.md).
- tstack [`skills/full-send/SKILL.md:65-80`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/full-send/SKILL.md#L65-L80) (Proof: reproduce, fix, same repro passes; mocked green is not proof).

## Files this PBI changes

Existing files link to main at the pinned SHA. New files use a repo-relative link (they do not exist yet).

- new: [`plugins/squad/skills/squad/references/playbooks/README.md`](../../../plugins/squad/skills/squad/references/playbooks/README.md)
- new: [`plugins/squad/skills/squad/references/playbooks/feature.md`](../../../plugins/squad/skills/squad/references/playbooks/feature.md)
- new: [`plugins/squad/skills/squad/references/playbooks/bug-fix.md`](../../../plugins/squad/skills/squad/references/playbooks/bug-fix.md)
- new: [`plugins/squad/skills/squad/references/playbooks/refactor.md`](../../../plugins/squad/skills/squad/references/playbooks/refactor.md)
- new: [`plugins/squad/skills/squad/references/playbooks/perf.md`](../../../plugins/squad/skills/squad/references/playbooks/perf.md)
- new: [`plugins/squad/skills/squad/references/playbooks/investigation.md`](../../../plugins/squad/skills/squad/references/playbooks/investigation.md)
- new: [`plugins/squad/skills/squad/references/playbooks/prototype.md`](../../../plugins/squad/skills/squad/references/playbooks/prototype.md)
- edit: [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
- edit: [`tools/AgentPacks.Cli.Tests/SquadContractTests.cs`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/tools/AgentPacks.Cli.Tests/SquadContractTests.cs)

## Links

- cursor/plugins (pstack, cursor-team-kit): [`pstack/docs/guide/02-poteto-mode.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/docs/guide/02-poteto-mode.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/benchmark-checklist/SKILL.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/benchmark-checklist/SKILL.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/bug-fix.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/bug-fix.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/feature.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/feature.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/investigation.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/investigation.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/perf-issue.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/perf-issue.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/prototype.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/prototype.md)
- cursor/plugins (pstack, cursor-team-kit): [`pstack/skills/poteto-mode/playbooks/refactoring.md`](https://github.com/cursor/plugins/blob/9e88dad267e8991d26a6debc0db0d943d437104b/pstack/skills/poteto-mode/playbooks/refactoring.md)
- t3dotgg/tstack: [`skills/full-send/SKILL.md`](https://github.com/t3dotgg/tstack/blob/7de017b3fc9d50c03cf3ecb98fb71e0ca65a78bd/skills/full-send/SKILL.md)
- agentPacks main (existing files): [`plugins/squad/skills/squad/SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/plugins/squad/skills/squad/SKILL.md)
- agentPacks main (existing files): [`shared/standards/engineering.md`](https://github.com/svennijhuis/agentPacks/blob/34e1dec3535baa1d77cb441ea8c98498dc9b3684/shared/standards/engineering.md)
