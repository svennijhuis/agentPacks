# Session split

One `/squad` run does at most one session of work. Work that does not fit is split into tasks
in `docs/squad-plan.md`, and each later run does exactly one task in a fresh session.

## Which mode

Check before step 1, in this order:

1. `docs/squad-plan.md` has a `Status: open` task and `/squad` has no request text, or the request
   names the plan (`docs/squad-plan.md`, `squad-plan`, `next task`, or `task <n>`) → task mode.
2. Open plan and any other request → ask once: `Open plan: task <n> is next. Run task <n>, or run
   the new request and leave the plan as is?` Task → task mode. New request → normal run; the plan
   is not read or written. If that new request does not fit one session, stop and tell the user to
   finish or delete the open plan first.
3. No plan, or no open task left → normal run with the fits-one-session check.

A request is never dropped: it runs, or the user chose task mode instead.

## Fits one session?

Normal run only. Run the check after step 2 (orient), before step 3. The work does not fit when
any limit is hit:

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

`Status` is `open` or `done`. Only a normal run that does not fit writes a new plan file, and only
when no plan with an open task exists. After that the file is edited in place, never rewritten:
a status flip, or a split of one open task (below). Every other task stays byte for byte.

## Task mode

1. Take the lowest open task in file order. Its Goal is the request for this run.
2. Task still too big (it hits a limit above, or the planner returns `Session split: <n> steps`):
   replace only that task block with subtasks `<n>.1`, `<n>.2`, ... at the same place, each with
   Goal, Done-check and `Status: open`. Build no code, then stop. The next run takes `<n>.1`.
3. Otherwise run it through the normal route (small change or full loop).
4. Run its Done-check. Exit 0 → set that task to `Status: done`. Otherwise leave it `open`.
5. Stop. Do not start the next task in this session. Tell the user the next task number.

Squad still never commits; each task is handed off uncommitted like any run.
