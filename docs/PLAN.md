# The plugin catalog plan

This is the rule that decides what becomes a plugin and what becomes a skill. Read it before adding either.

> **Plugins are the install axis: a role that needs no compiler, or a whole language ecosystem.**
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

## The catalog

Role packs — installed because of how you work, not what you compile:

| Plugin | Who installs it | Holds |
| --- | --- | --- |
| `productivity` | anyone driving an agent | `grill-me`, `grilling`, `writing-for-agents`, `caveman`, `caveman-compress` |
| `product` | product owners and managers | user stories, requirements, notes to spec *(empty for now)* |
| `migrations` | platform teams | Azure DevOps to GitHub and similar moves *(empty for now)* |
| `engineering` | any builder | **only** cross-language craft: `domain-modeling`, `codebase-design`, `diagnosing-bugs`, `improve-codebase-architecture`, `code-review`, `to-tickets`, `triage`, `wayfinder`, `research`, `grill-with-docs`, `setup-matt-pocock-skills`, `grilling`, `testing` |
| `security` | anyone shipping software | threat modeling and general checklists *(empty for now)* |

Language packs — installed because of the ecosystem you live in, one per language family:

| Plugin | Holds |
| --- | --- |
| `dotnet` | C# and .NET build, review, test and .NET-specific security skills |
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

This is why `engineering` holds thirteen skills rather than the three or four "small and strict" suggests. Every one of them is cross-language craft, and they arrive as one connected workflow: architecture, diagnosis, review, and the issue-tracker loop that carries the work. Small means *no language-specific skills*, not a low count. The lean alternative — dropping `code-review` and `to-tickets`, which takes `setup-matt-pocock-skills`, `triage`, `wayfinder`, `research` and `grill-with-docs` with them — is a five-skill pack that is also closed. It is a smaller pack with less capability, not a tidier rule.

## Empty packs

`product`, `migrations`, `security`, `typescript` and `rust` currently ship a `plugin.json` and no skills. They reserve the name and the shape so the first skill is a one-file pull request rather than a catalog debate. They appear in the generated marketplace with no `skills` entry, so installing one is harmless and useless — fill them before advertising them.

## How this catalog was reached

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
