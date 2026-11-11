# ADR 0003: Productivity plugin and /tech-design

Status: accepted

## Context

A business architect needs a technical design that developers can build, with almost all gaps closed, so wayfinder can turn it into tickets a team can put story points on. The catalog has no command for that work.

`docs/PLAN.md` already names a `productivity` role pack for anyone driving an agent. The name is Matt Pocock's `skills/productivity` folder. Squad work is in flight, so the command stays out of `plugins/squad/`.

User slashes stay few. `docs/pbi/64-squad-session-split.md` says "No 5th slash": `/squad split` stays inside `/squad`, and `/squad-split` is not a new command. The root README and `docs/PACK-AUTHOR.md` already record `/pack-author` as the intentional author slash for Lane A. `docs/CONTRIBUTE.md` lists the locked user commands. A phase of an existing workflow is not a new pack, and two packs do not share a command (`docs/PLAN.md`).

Open PR #95 adds `docs/adr/0002-review-sequence.md`. This record is 0003 so the numbers do not collide.

Plan v3.1 was approved by the product owner on 2026-11-11, after Sven's direction.

## Decision

**The plugin is `plugins/productivity/`.** Later productivity commands can live there. It stands beside Squad, so it does not collide with Squad work in flight. A pack enters the catalog when it has content (`docs/PLAN.md`). This record adds no plugin files.

**`/tech-design` is the one real slash, and the single explicit exception to the "no 5th slash" charter rule, granted by Sven.** It is a productivity command, not a Squad phase. This plan grants no other slash.

Usage:

- `/tech-design new <name> [types]`
- `/tech-design <name>` resumes that design
- `/tech-design <name> grill [gap]`
- `/tech-design <name> ready`

The command produces a technical design a business architect prepares for developers. Almost all gaps are closed before hand-off, so wayfinder can turn the design into tickets that can take story points. Skills stay generic. Playbook text is our own words, with no sample system in a skill.

**The core loop is the wayfinder way.** Sessions stay small. One session takes one gap or one topic: grill, research, write, update `gaps.md`, append one `log.md` line, and stop. A session never writes the whole design.

**Grill asks one question at a time**, along the decision tree, and never fills a gap in silence. No answer moves that gap to `waiting-on-input`, with an owner and a due date.

**Each design is a folder in the product repo, `docs/design/<name>/`.**

| File | Role |
| --- | --- |
| `README.md` | How to start, and how to resume |
| `design.md` | The design |
| `gaps.md` | One row per gap |
| `research/<topic>.md` | Notes for one topic |
| `input/<date>.md` | Input saved that day |
| `log.md` | One line per session |
| `openapi.yaml` | Present when the api type is in use |

**Gap statuses are `open`, `research`, `waiting-on-input`, `decided`, and `accepted-risk`.** A row holds id, question, status, owner, due, waiting-on note, source or answer, and blocks. Document status `ready` is allowed only when zero gaps are `open`, `research`, or `waiting-on-input`. `decided` and `accepted-risk` do not block that gate.

```mermaid
stateDiagram-v2
  [*] --> open
  open --> research: this session studies it
  open --> waiting-on-input: no answer
  open --> decided: answer recorded
  open --> accepted-risk: risk accepted
  research --> waiting-on-input: no answer
  research --> decided: answer recorded
  research --> accepted-risk: risk accepted
  waiting-on-input --> research: input saved
  waiting-on-input --> decided: input answers it
```

**Document status lives in the design file in git**, in the style of an Oxide RFD. The words are `draft`, `discussion`, `ready`, `committed`, and `abandoned`. `draft` is the new folder. `discussion` is work while gaps remain. `ready` means the gap gate passed. `committed` means builders follow this design. `abandoned` means the work stopped.

**`design.md` uses a fixed section order**, modeled on a Google design doc, plus ideas from Amazon, Uber, Shopify, and Oxide. Sections, in order: user outcome, context and scope, goals, non-goals, glossary, design, dependencies, alternatives, cross-cutting concerns, type checklists, advice, risks, acceptance criteria, and sign-off. User outcome comes first. The design section holds an overview, a diagram, boundaries and contracts, a data model, and flows as mermaid. Each external dependency is one dependency-contract block. Alternatives include doing nothing. Cross-cutting concerns are security, privacy, observability, rollout, and testing. Advice states the recommended choice, why, and what changes if a gap flips. Each waiting gap has a deadline.

**Type playbooks are separate files: web, api, data, and integration.** Types combine (`types: [web, api]`). Gaps are the union of those checklists, deduped by checklist key, so each gap appears once and names every type that raised it. The api playbook writes or updates `openapi.yaml` (OpenAPI 3.1) as the contract first. Each endpoint carries an auth level, error codes, and a version. An empty field becomes a gap.

**Every external dependency runs the dependency-contract playbook.** The same playbook can run on its own. The fields are owner and response time; access (auth method, level or scopes, how to request, lead time); environments (test and prod URLs, test accounts, allowlist or VPN); contract (request, response, version, errors, timeout); limits (rate, SLA, cost); change notice; and proof. The skill makes no real calls. Proof is the owner's spec, such as their OpenAPI file, or a written answer from the owner, saved under `input/`. Free text from a chat, or a promise, does not close the gap. Any empty field is a blocking gap. The owner's OpenAPI spec is requested, and a missing spec is a blocking gap.

**Imports come from three MIT sources, and from nowhere else, not even as ideas.** Matt Pocock: `https://github.com/mattpocock/skills` at `4ae2317ddb22b046e26455e4f9c9b3aaaf62212d`. pstack and the Cursor team kit: `https://github.com/cursor/plugins` at `ae5650febf4320793844c12c4adaa937f417f8da`. A model-invocable skill is pinned in `plugins/productivity/external-skills.json` by exact name, path, and the full 40-hex SHA (`docs/EXTERNAL-SKILLS.md`). A pin is never copied by hand.

| Skill | Path |
| --- | --- |
| `domain-modeling` | `skills/engineering/domain-modeling` |
| `codebase-design` | `skills/engineering/codebase-design` |
| `research` | `skills/engineering/research` |
| `grilling` | `skills/productivity/grilling` |
| `deslop` | `cursor-team-kit/skills/deslop` |

The first four paths are in `mattpocock/skills`. `deslop` is in `cursor/plugins`. `deslop` cleans `openapi.yaml` and any code snippets.

Skills with `disable-model-invocation: true` are copied and adapted. Each copy carries `NOTICE.md`, a Source line, and a "What we changed" note. From Matt Pocock: `wayfinder` (`skills/engineering/wayfinder`), `to-spec` (`skills/engineering/to-spec`), `to-questionnaire` (`skills/productivity/to-questionnaire`), and `to-tickets` (`skills/engineering/to-tickets`). From pstack: `architect`, `principle-exhaust-the-design-space`, `statechart`, `blast-radius`, and `interrogate` (paths under `pstack/skills/`). `statechart` contributes mermaid flows only. Its scripts are not copied.

`technical-writing` and `unslop` (pstack) are copied and adapted as well, because they are user-invocable only. `technical-writing` is the writing style for `design.md`. `unslop` is the last pass on every document.

`docs-canvas` (`docs-canvas/skills/docs-canvas` in `cursor/plugins`) is optional, for showing `design.md` at a meeting. It is outside the loop. If it is used, it is pinned the same way as `deslop`, at the same SHA.

The only command file is `tech-design`. Adapted skills are loaded by exact skill name from that command. A pointer to a skill outside this list is removed in the adapt step. The older productivity list in `docs/PLAN.md` is not imported.

**Check the real code first, and invent nothing.** That rule applies to every step.

**`/tech-design <name> ready` runs the gate.** It does a final attack pass, sets document status `ready` only when the gap gate passes, writes tickets whose blockers are sized for story points, and writes a wayfinder brief.

**Delivery is one pull request at a time.** TD0 is this record. TD1 is the plugin skeleton. TD2 is the loop core and grill. TD3 is the dependency contract. TD4 is web and api. TD5 is data and integration. TD6 is research, input, and options. TD7 is readiness. TD8 is smoke.

## Consequences

- Squad stays untouched while this work lands.
- Product repos gain one command. Resume, grill, and the gate are arguments of that command.
- A design stays below `ready` while any gap is `open`, `research`, or `waiting-on-input`.
- A waiting gap has an owner, a due date, and a deadline in `design.md`.
- An unproved dependency stays a blocking gap. Chat text is not proof.
- Pins stay Skill-tool dependencies. Publication sets them `user-invocable: false` (`docs/EXTERNAL-SKILLS.md`), so a pin does not become a slash.
- This plugin takes the `productivity` name from the catalog plan. Its command is `/tech-design`.
- The root README, `docs/CONTRIBUTE.md`, and `docs/PLAN.md` still describe today's catalog. The plugin pull request is what updates them.
- Copilot namespaces the command under the plugin, as the root README does for the other commands.

## Alternatives rejected

**Put `/tech-design` inside Squad.** Squad work is in flight, and the charter refuses another Squad slash.

**Split start, grill, and ready into their own slashes.** One command covers all four forms.

**Let one session write the whole design.** Open gaps would disappear into a long draft. The session stops after one gap or one topic.

**Fill a gap when nobody answers.** Silence is `waiting-on-input`, with an owner and a due date.

**Treat a promise or a chat line as proof.** Proof is the owner's spec, or the owner's written answer, saved under `input/`.

**Pin the user-invoked skills.** Those skills are copied and adapted, with `NOTICE.md`, a Source line, and a "What we changed" note.

**Copy the model-invoked skills by hand.** They stay pins at a full SHA.

**Import any other repository, or borrow its ideas.** The three sources above are the whole set.

**Put a sample system in a skill.** The playbooks stay generic, in our own words.
