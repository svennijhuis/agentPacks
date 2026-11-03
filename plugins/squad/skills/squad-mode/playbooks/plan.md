# Plan

You own this task. The main chat runs this playbook and delegates each step to a fresh squad-agent with the named role.

1. **planner**: Write the plan file and stop. Use how, why, and blast-radius. **Evidence:** the plan path.

When the work does not fit one session, return `Session split: <n> steps` and write no `## Slices`. The main chat deletes this run's draft `docs/plans/` file and writes `docs/squad-plan.md` by the forced-split rules.

No product code.
