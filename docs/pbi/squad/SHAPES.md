# Shapes in squad v2

How each kind of file must look. [STRUCTURE.md](STRUCTURE.md) says where it lives; this file says what is inside. Built from the analysis of pstack (`ae5650f`) and Matt (`4ae2317`) in the [skill map](sources/pstack-matt-skill-map.md) and the old [old analysis](sources/old-analysis-checklist.md), plus our bars in `pack-author-md/references/style.md` on main.

Length bars warn (WARN), shape bars fail (FAIL), same as today. Every kind must read the same in Cursor, Claude Code, Copilot, Codex and gh-aw; provider-only text goes under a last `## Provider notes` section.

## Summary

| Kind | Frontmatter | Body order | Max (non-blank lines) | Made by | Checked by |
|---|---|---|---|---|---|
| Hub skill | name, description, license, user-invocable: false, disable-model-invocation: true | H1, bundled line, Non-negotiables, Principles, Autonomy, Subagents, Writing the reply, Comments, Playbooks, Standards, Provider notes | 150 (HUB01) | write-skill | check-skill, REF01 to REF04, HUB01 |
| Skill (leaf or helper) | name, description, license, user-invocable | H1, one-line job, Non-negotiables (optional), Steps, Good and bad, links to `references/` | 40 | write-skill | check-skill, REF01 |
| Subagent | name, description, model, readonly, tools | hub path line, Non-negotiables, Load, Steps, Return, Good and bad | 60 | write-subagent | check-subagent |
| Playbook | none (H3 title) | H3, Source line (copies), owner line, numbered steps, Reply, Standards, What we changed or added | 40 | write-skill (playbook mode) | check-skill, REF04, REF06 |
| Principle leaf | upstream (pinned, not ours) | H1, rule, Why, Pattern | upstream | not written by us | REF02, byte-compare |
| Standard leaf | name, description (Apply when), user-invocable: false | H1, rule, Why, When it applies, Good and bad, How to check | 40 | write-skill (standard mode) | check-skill, REF03 |
| Command | name, description | root line, load hub by exact name, run its flow, never-do lines | 25 | write-skill (command section) | validate (7 commands) |

## Hub skill

- **In:** routing only. Rules that always hold (Non-negotiables), index lines that name a principle, standard or playbook, the Subagents table.
- **Out:** procedure steps (go in playbooks), the text of a principle or standard (lives in the leaf), model ids.
- **Bundled line** under the H1, always: "Bundled skills live at `../<name>/SKILL.md` from this file, so put this file's absolute path in every `squad-*` agent prompt."

Good:

```markdown
---
name: squad-md
description: When /squad or /squad-review runs.
license: UNLICENSED
user-invocable: false
disable-model-invocation: true
---

# Squad

Bundled skills live at `../<name>/SKILL.md` from this file, so put this file's absolute path in every `squad-*` agent prompt.

## Non-negotiables
- Any behavior change with a local test target goes to the **tdd** skill.
- `docs/squad-plan.md` is the run state. Every plan edit runs check-plan.

## Principles
Read the leaf in full for any principle you apply.
- **Fix Root Causes.** When debugging. Leaf: principle-fix-root-causes.

## Autonomy
Unattended (gh-aw, no human): take the default answer and write the open question into the report.

## Subagents
| Agent | Job | Loads |
|---|---|---|
| squad-verifier | run verify commands | Prove It Works |

## Writing the reply
## Comments
## Playbooks
- **bug** to playbooks/bug-fix.md

## Standards
- **No Em Dash.** Any prose. Leaf: standard-no-em-dash.
```

Bad:

```markdown
# Squad
When the user reports a bug, first reproduce it, then write a failing test, then ...
Use model claude-opus or grok-4.7-xhigh-fast for the fix.
```

Steps in the hub, a client model id, no bundled line, no index.

## Skill (leaf or helper)

- **In:** one job, numbered steps, one good and bad pair, links one level deep to `references/`.
- **Out:** a second job, restated principles, dates and version gossip.
- **Description** starts with `When ` and ends with `.`; over 120 characters warns.

Good:

```markdown
---
name: statechart-md
description: When a feature has states and transitions worth checking.
license: MIT
user-invocable: false
---

# Statechart

Source: <statechart at ae5650f...>, MIT, see NOTICE.

Model the states, check them, write the chart page.

1. List states and events from the code, not from memory.
2. Run the checker (scripts byte-equal to upstream).
3. Write `docs/architecture/<area>.md` with the mermaid chart.
4. Export shortest paths to bad states for `/scenarios`.

Good: chart names every state the code has. Bad: chart drawn from the PBI text.

## What we changed
- Renamed from `statechart`, added docs output and scenario export.

## Provider notes
### Cursor
- Upstream `/statechart` built-in is replaced by our command.
```

Bad: one SKILL.md with the procedure, three tables and all examples inline, name equal to the command (`statechart`), `disable-model-invocation: true` so the flow cannot load it.

## Subagent

- **In:** one role, a tier (`inherit`, `fast`, `standard`, `frontier`), a closed tool list, the skills and leafs it loads by exact name, a fixed return shape.
- **Out:** client model ids, "load whatever helps", whole skills pasted in.
- **First body line** is the hub path line. Codex has no plugin agents: the generator puts the same text in the TOML prompt.

Good:

```markdown
---
name: squad-verifier
description: Runs the plan's verify commands and reports numbers. Use after each implement or fix round.
model: standard
readonly: true
tools: [read, grep, glob, bash]
---

You receive the absolute path of `squad-md/SKILL.md`. Bundled skills live at `../<name>/SKILL.md` from it.

## Non-negotiables
- A pass needs command output, never a summary.

## Load
Principles: **Prove It Works**, **Explain the Number**. Standards: **Review Contract**.

## Steps
1. Run each verify command from the plan.
2. Run the wider unit suite once.

## Return
`PASS` or `FAIL`, command, run count, numbers.

## Good and bad
Good: "FAIL: dotnet test, 1 run, 2 of 118 failed (OrdersTests.Cancel)". Bad: "Looks good."
```

Bad:

```markdown
---
name: verifier
model: gpt-5
tools: [all]
---
You are an expert. Check everything and fix what you find.
```

Client model id, open tools, two jobs (check and fix), no return shape.

## Playbook

- **In:** steps for one task type in order, who owns the task, the reply shape, which standards apply, and for copies the Source line and What we changed or added.
- **Out:** Cursor-only commands, model slugs, steps that ignore our gates.
- **Copy + adapt by default** (PS09). Keeps: one PBI per PR, one PR at a time, `docs/squad-plan.md`, max 2 fix rounds, findings fixed on the PR.

Good:

```markdown
### Bug fix

Source: <bug-fix.md at ae5650f...>, MIT, see NOTICE.

**You own this task.** Delegate the fix to `squad-implementer`, stay in the lead.

1. Reproduce it yourself on the surface the user hit. Add the repro row to `docs/squad-plan.md`.
2. Find the cause: **how** for the subsystem, **why** for history, Matt diagnosing-bugs phases.
3. Failing test first (**tdd**), then the smallest fix.
4. Verify on the same surface. Wrong surface is not a pass.
5. Run playbooks/pr.md.

**Reply:** what broke, root cause, fix, failing then passing output.
Standards: **Review Contract**, **No Secrets in Markdown**.

#### What we changed or added
- Dropped Cursor `/loop` and the model slug.
- Added the repro row and Matt phases.
```

Bad: upstream file pasted unchanged ("Drive a long hunt with Cursor's `/loop` command", "default `grok-4.7-xhigh-fast`"), no Source line.

## Principle leaf

Pinned from pstack, never edited. Shape for reference (upstream `principle-fix-root-causes`, 21 lines): frontmatter `name`, `description` ("Apply when ..."), `disable-model-invocation: true`; H1; the rule; `**Why:**`; `**Pattern:**` bullets. Upstream sets `disable-model-invocation: true`, so the hub and agents read the leaf by its `../principle-<x>/SKILL.md` path, not with the Skill tool. A change we want is a standard leaf, not an edited principle.

## Standard leaf

- **In:** one rule of ours, why with Sven's words and date, when it applies and when not, good and bad, how to check (command, test or validator rule).
- **Out:** a copy of `shared/standards/*.md` text (link to it instead), more than one rule.

Good:

```markdown
---
name: standard-squash-merge-one-by-one
description: Apply when a step merges a PR.
user-invocable: false
---

# Squash Merge One by One

Merge with squash, one PR at a time, after every gate is green.

**Why:** Sven 2026-09-14, "one-for-one, never batch-merge".

**When:** any merge step. Not when the room stops at the GO; then nobody merges.

Good: merge #91, wait for main green, then #92. Bad: merge #91 and #92 together.

**Check:** room charter gate; squad never merges (hand-off says nothing was merged).
```

Bad: a leaf called `standard-git` with five rules (merge, branches, force push, tags, commit text) and no check.

## Command

Thin. Loads the hub by exact skill name and runs its flow. Never holds steps.

Good:

```markdown
---
name: statechart
description: Model a feature's states, check them, write the chart.
---

# Statechart

Load the `statechart-md` skill with the Skill tool by exact name. Run its flow. Do not commit, merge or push.
```

Bad: a command file with the full procedure, so Copilot, Claude and gh-aw imports each carry a different copy.

## Names

Every kind uses the poteto pattern: role plus a kebab title that says what it is. Never the origin (fork, upstream, `local`) or a location in the name.

| Kind | Name | Example |
|---|---|---|
| Hub | `<pack>-md` | `squad-md` |
| Principle leaf | `principle-<title>` | `principle-fix-root-causes` |
| Standard leaf | `standard-<topic>` | `standard-api-versioning` (fork) next to `standard-squash-merge-one-by-one` (ours) |
| Playbook | `playbooks/<title>.md` | `playbooks/bug-fix.md` |
| Subagent | `<pack>-<role>` | `squad-reviewer` |

Ownership is not in the name: ours are in the upstream manifest `migrations/v2.json`, a fork's in `fork-inventory.md`. A clash goes to the MIGRATION conflict table, fix by renaming.
