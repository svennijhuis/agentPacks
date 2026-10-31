# Session split

One `/squad` run does at most one session of work. Work that does not fit is split into tasks
in `docs/squad-plan.md`, and each later run does exactly one task in a fresh session.

## Which mode

Check before step 1, in this order:

1. The request starts with the split word `split` (`/squad split <request>`) → forced split.
2. `docs/squad-plan.md` exists → status refresh, then always ask, also when `/squad` has no request
   text. The request is empty or names the plan (`docs/squad-plan.md`, `squad-plan`, `next task`, or
   `task <n>`) → the status question. Any other request → the status line plus the new request
   question; new request → normal run, and the plan is not read or written again. If that new
   request does not fit one session, stop and tell the user to finish or close the open plan first.
3. No plan → normal run with the fits-one-session check. A request that only names the plan has
   nothing to run: tell the user there is no plan and stop.

A request is never dropped: it runs, or the user chose the plan instead.

## Fits one session?

Normal run only. Run the check right after orient (skill flow step 2, `/squad` step 1), before the
small-change route. The work does not fit when
any limit is hit:

- more than one PBI or more than one independent outcome in the request
- more than 3 steps (the slice cap in the planning contract)
- more than 10 files to create or change

Not fit → write `docs/squad-plan.md`, build no code, spawn nobody, write no retro, and stop.
Tell the user: start a new session and run `/squad` again to do task 1.
A planner round that returns `Session split: <n> steps` takes the same exit. The main agent first
deletes this run's draft `docs/plans/` file, so the next run's retro catch-up never treats it as
interrupted. A session-split stop is not an interrupted run.

## Forced split

`/squad split <request>` splits the request into tasks even when it fits one session: write
`docs/squad-plan.md`, build no code, spawn nobody, write no retro, and stop. No request text after
`split` → ask once what to split.
A plan file already exists → ask once: `Plan <outcome> exists. Replace it or append the new tasks?`
Replace → archive the old plan (as on close), then write the new one. Append → add the new task
blocks after the last task, numbered on from it; every existing line stays byte for byte.
This section is the whole forced split, so a later `/squad-split <request>` command only runs it.

## Plan file

`docs/squad-plan.md`, one per repository. Numbered tasks, each small enough for one session.
Every task has these fields, in this order:

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

Optional `- Depends: <m>` (or `<m>, <k>`) between Done-check and Status: the task waits until those
tasks are `done` (a split task `<m>` is done when all its `<m>.<i>` subtasks are).
`Status` is `open`, `started`, or `done`. Only a normal run that does not fit, or a forced split,
writes a new plan file. After that the file is edited in place, never rewritten: a status flip, a
split of one task (below), or appended tasks. Every other line stays byte for byte.

## Status refresh

Squad reads the real state, not only the Status lines. For every task that is not `done`, in file
order:

1. Run its Done-check. Exit 0 → set `Status: done`; the work is already in the code, for example
   from a merged PR or another session.
2. A task whose `Depends:` names a task that is not `done` is skipped, with the reason.

The offered task is the first one that is neither `done` nor skipped. A `started` task was half
done in an earlier session. Every task done → the close question. Tasks left but every one skipped
→ say which task waits on which, and stop.

Questions, words as written:

- Status: `Plan <outcome>: <states>. Shall I do task <n>?`
- Half done: `Plan <outcome>: <states>. Task <n> was half done, shall I finish it?`
- New request: `Plan <outcome>: <states>. Do task <n>, or run the new request?`
- Close: `Plan <outcome> done, shall I close it?`

`<states>` lists the tasks in file order up to and including the offered one, joined by `, `:
`task <n> done`, `task <n> open`, `task <n> half done`, or `task <n> skipped (waits on task <m>)`.
No → stop. Close yes → move the plan to `docs/squad-plans/<yyyy-mm-dd>-<slug>.md` (slug from the
outcome), unchanged, and stop.

## Task mode

1. Take the offered task the user said yes to. Its Goal is the request for this run. Set it to
   `Status: started` before any other work, so a stop halfway shows up as half done next run.
2. Task still too big (it hits a limit above, or the planner returns `Session split: <n> steps`):
   replace only that task block with subtasks `<n>.1`, `<n>.2`, ... at the same place, each with
   Goal, Done-check and `Status: open`. Build no code, then stop. The next run offers `<n>.1`.
3. Otherwise run it through the normal route (small change or full loop).
4. Run its Done-check. Exit 0 → set that task to `Status: done`. Otherwise it stays `started`.
5. Stop. Do not start the next task in this session. The next run asks about the next task.

Squad still never commits; each task is handed off uncommitted like any run.
