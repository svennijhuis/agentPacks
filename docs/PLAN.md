# The plugin catalog plan

This is the rule that decides what becomes a plugin and what becomes a skill. Read it before adding either.

> **Plugins are the install axis: a role that needs no compiler, a whole language ecosystem, or a capability that needs more than skills.**
>
> **Skills are the role and the framework inside that pack: `dotnet-review`, `react-…`, `axum-…`.**
>
> **A framework never gets its own plugin.**

## The problem this solves

Two instincts pull against each other. Split, because not everyone writes Rust, C# or frontend code, and an install should match the job. Do not explode, because `engineering-backend-typescript-nestjs` style names turn a catalog into something nobody browses.

The mistake is multiplying every axis at once:

```
role × language × layer × framework
review × rust × backend × axum   →  a plugin per combination
```

| Axis | Examples | Good as a plugin? |
| --- | --- | --- |
| Role | engineering, review, testing, security, product | Yes, when it is language-agnostic |
| Language | dotnet, typescript, rust | Yes — this is the main split for code |
| Layer | frontend, backend | Only after a language pack gets fat |
| Framework | react, nestjs, axum, aspnet | Never — it is a skill |

Framework churn is high: Next.js, Axum and NestJS patterns move faster than the languages under them. Plugins should outlive frameworks. A skill can be added or dropped in one pull request; a plugin is something people have already installed.

Counting the two models makes the choice concrete:

```
role × language           (review, testing, engineering, security) × (dotnet, typescript, rust)  = 12
+ frontend/backend splits                                                                        = 18+
+ frameworks                                                                                     = 30+

role packs + language packs
productivity, product, migrations, engineering, security + dotnet, typescript, rust              = 8
```

Both give a Rust shop a way to avoid installing .NET skills. Only one stays readable.

## What ships today

The repository currently ships **one capability pack and nothing else**: `delivery-loop`.

The earlier catalog carried five role packs and three language packs, six of which held nothing but a `plugin.json`. An empty pack is not a placeholder — it is an install that appears in the marketplace, resolves, and does nothing, which is worse than not being listed. They were removed in the same branch that added `delivery-loop`; the pinned external-skill imports and the two authored skills (`engineering/testing`, `dotnet/dotnet-review`) are in history at `007f609` and can be restored when there is a pack around them worth installing.

The rest of this document is the rule for what earns a plugin, unchanged. The catalog below is the target, not an inventory.

## The target catalog

Role packs — installed because of how you work, not what you compile:

| Plugin | Who installs it | Holds |
| --- | --- | --- |
| `productivity` | anyone driving an agent | `grill-me`, `grilling`, `writing-for-agents`, `caveman`, `caveman-compress` |
| `product` | product owners and managers | user stories, requirements, notes to spec *(empty for now)* |
| `migrations` | platform teams | Azure DevOps to GitHub and similar moves *(empty for now)* |
| `engineering` | any builder | **only** cross-language craft: `domain-modeling`, `codebase-design`, `diagnosing-bugs`, `improve-codebase-architecture`, `code-review`, `to-tickets`, `triage`, `wayfinder`, `research`, `grill-with-docs`, `setup-matt-pocock-skills`, `grilling`, `testing` |
| `security` | anyone shipping software | threat modeling and general checklists *(empty for now)* |

Capability packs — installed because of a workflow you want wired into the agent loop, not just knowledge you want available:

| Plugin | Who installs it | Holds |
| --- | --- | --- |
| `delivery-loop` | anyone who wants a change planned before it is built and checked after | the `delivery-loop` skill, standards and role boundaries as rules, `loop-orchestrator`, `loop-planner`, `loop-implementer`, `loop-verifier`, `loop-reviewer`, `loop-security-reviewer` and `loop-simplifier` subagents, the `review-diff` command, and a hook that flags the commands that land work |

A capability pack is the exception to "a role is a role pack", and it earns the exception only by shipping components a skill cannot express: rules that apply without being invoked, subagents, commands, or hooks. A pack that would hold nothing but skills is a role pack, not a capability pack.

There was briefly a second one. `code-review` shipped a review skill, review standards, a security subagent, a diff subagent and a review command; `delivery-loop` then shipped a review phase with its own security gate and its own diff reviewer. Two packs, one subject, and the only real difference was whether a finding cost a fix round or was merely printed. Review folded into the loop as a phase, and the pack was removed.

The rule that falls out of it: **a capability pack is a workflow, and a phase of an existing workflow is not a new pack.** A third has to clear both bars — components a skill cannot express, and a loop that is not already someone else's phase.

Language packs — installed because of the ecosystem you live in, one per language family:

| Plugin | Holds |
| --- | --- |
| `dotnet` | An `afterFileEdit` hook that runs `dotnet format` on the C# file an agent just wrote; C# and .NET build, review, test and .NET-specific security skills come next |
| `typescript` | TypeScript frontend *and* backend; React and NestJS as skills |
| `rust` | Rust build, review and test; Axum as skills *(empty for now)* |

Typical installs:

```
.NET shop        dotnet + engineering + security + productivity
TypeScript shop  typescript + engineering + security + productivity
Rust API         rust + engineering + security
Product owner    product + productivity
Platform team    migrations + productivity
```

`engineering` stays small and strict on purpose. It is not a dumping ground: if a skill only makes sense with a compiler in hand, it belongs to a language pack.

## Naming skills inside a pack

```
[<framework>-]<action>-<object>
```

```
plugins/typescript/skills/       plugins/rust/skills/        plugins/dotnet/skills/
  react-component-scaffold/        axum-routing/               aspnet-api-design/
  react-accessibility/             axum-extractors/            ef-core-query-audit/
  nestjs-module-design/            rust-review/                dotnet-review/
  nestjs-auth-patterns/            rust-test-patterns/         dotnet-test-patterns/
  typescript-review/                                           dotnet-security-review/
  playwright-e2e/
```

When there is no framework, prefix with the language: `typescript-review`, `rust-error-handling`, `dotnet-review`. The layer then reads off the name — `react-component-scaffold` is frontend, `nestjs-module-design` is backend — which is all an agent or a human needs to pick one. No `engineering-frontend-typescript` plugin is required to express it.

## Where review, testing and security live

| Kind of skill | Home |
| --- | --- |
| How we review a C# pull request | `dotnet` → `dotnet-review` |
| How we review a TypeScript pull request | `typescript` → `typescript-review` |
| Two-axis review of any diff | `engineering` → `code-review` |
| Threat-model any system | `security` |
| OWASP-style general checklists | `security` |
| .NET crypto and auth footguns | `dotnet` → `dotnet-security-review` |
| What deserves a test, as philosophy | `engineering` → `testing` |
| How to write a test in this stack | the language pack → `*-test-patterns` |

Stack-specific security goes in the language pack, not in `security`. One home per subject; the rule picks it.

Separate `reviewer-dotnet` and `tester-dotnet` plugins would only pay off if reviewers and testers were different install audiences who must not receive each other's skills. They usually are not: the same person wants all three, and three installs for one stack is friction with nothing behind it.

## When to split further

Rules, not vibes:

| Split | Only when |
| --- | --- |
| `typescript` → `typescript-frontend` + `typescript-backend` | roughly 12+ skills **and** most people install only one half |
| `dotnet` → two packs | only with serious Blazor/MAUI versus API audiences; most .NET work is backend-ish |
| A new language pack (`go`, `python`) | there are 2–3 real skills to put in it, not an empty shell |
| A new framework | never a plugin — it is a skill inside the language pack |

## External skills travel with their dependencies

An imported skill is part of the plugin that lists it in `external-skills.json`, and a wrapper is useless without the skill it invokes. **A pack must be closed under `/skill` references: follow every reference, including the ones in a skill's supporting Markdown, until nothing points outside the pack.**

Only the transitive closure counts. `code-review` and `to-tickets` both call `/setup-matt-pocock-skills`; its setup guides then send you to `/triage`, `/wayfinder` and `/grill-with-docs`; `wayfinder` delegates reading to `/research`. Importing the first two and stopping would ship a pack that dead-ends three steps in. The same applies to `caveman`, which invokes `/caveman-compress`.

A skill that two packs both depend on is pinned in both. `grilling` is imported into `productivity` (the skill `grill-me` wraps) and into `engineering` (what `improve-codebase-architecture`, `triage`, `wayfinder` and `grill-with-docs` all run). Duplicating one pinned URL record is cheaper than a pack whose instructions stop working.

The validator enforces closure for skills authored here, but it deliberately skips materialized external skills — upstream Markdown is not ours to reinterpret. So closure over imports is a review duty, checked when the pin is added or moved.

This is why `engineering` was sized at thirteen skills rather than the three or four "small and strict" suggests, and why it should be again. Every one of them is cross-language craft, and they arrive as one connected workflow: architecture, diagnosis, review, and the issue-tracker loop that carries the work. Small means *no language-specific skills*, not a low count. The lean alternative — dropping `code-review` and `to-tickets`, which takes `setup-matt-pocock-skills`, `triage`, `wayfinder`, `research` and `grill-with-docs` with them — is a five-skill pack that is also closed. It is a smaller pack with less capability, not a tidier rule.

## No empty packs

An earlier version of this plan reserved names: `product`, `migrations`, `security`, `typescript` and `rust` shipped a `plugin.json` and no skills, so the first skill would be a one-file pull request rather than a catalog debate.

That was the wrong trade. A reserved name still appears in the generated marketplace, still resolves, and still installs — it just does nothing afterwards, and the person who installed it has no way to tell the difference between an empty pack and a broken one. The name costs nothing to claim later; the empty install costs trust now. **A pack enters the catalog when it has content, not before.**

## How this catalog was reached

*Historical record. The packs named below are the ones removed in the branch that added `delivery-loop`; the layout they describe is the target catalog, not what is on disk.*

The previous catalog was `engineering`, `review` and `testing`: a role split with no language axis, which had already put a `.NET`-only skill inside a general `review` pack. Restructuring moved every existing skill to the home this rule gives it, without changing any skill content:

| Was | Now |
| --- | --- |
| `review/skills/dotnet-review` | `dotnet/skills/dotnet-review` |
| `testing/skills/testing` | `engineering/skills/testing` |
| `review` → external `code-review` | `engineering` → external `code-review` |
| `engineering` → external `grill-me`, `grilling`, `writing-for-agents`, `caveman` | `productivity` |
| `engineering` → the remaining external engineering skills | `engineering`, unchanged |
| — | `engineering` gains `grill-with-docs`, `triage`, `wayfinder`, `research`; `productivity` gains `caveman-compress` — all pulled in to close the reference graph |

The `testing` skill lost its xUnit-shaped assertion example when it moved, since `engineering` is language-agnostic. Nothing else in any skill changed.
