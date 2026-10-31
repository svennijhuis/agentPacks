# Session split

One `/squad` run does at most one session of work. Work that does not fit is split into tasks
in `docs/squad-plan.md`, and each later run does exactly one task in a fresh session.

## Fits one session?

Run the check after step 2 (orient), before step 3. The work does not fit when any limit is hit:

- more than one PBI or more than one independent outcome in the request
- more than 3 steps (the slice cap in the planning contract)
- more than 10 files to create or change

Not fit → write `docs/squad-plan.md`, build no code, spawn nobody, write no retro, and stop.
Tell the user: start a new session and run `/squad` again to do task 1.
A planner round that returns `Session split: <n> steps` takes the same exit.

## Plan file

`docs/squad-plan.md`, one per repository. Numbered tasks, each small enough for one session.
Every task has exactly these three fields:

```markdown
# Squad plan: <outcome>

## Request
<the ask as typed>

## Tasks

### 1. <title>
- Goal: <one outcome>
- Done-check: `<command that proves it>`
- Status: open
```

`Status` is `open` or `done`. Nothing else changes a task after it is written.

## Task mode

Before step 1: `docs/squad-plan.md` exists with at least one `Status: open` task → task mode.

1. Take the lowest-numbered open task. Its Goal is the request for this run.
2. Run it through the normal route (small change or full loop).
3. Run its Done-check. Exit 0 → set that task to `Status: done`. Otherwise leave it `open`.
4. Stop. Do not start the next task in this session. Tell the user the next task number.

No open task left → the plan is finished; say so and run the request as a normal `/squad`.
Squad still never commits; each task is handed off uncommitted like any run.
