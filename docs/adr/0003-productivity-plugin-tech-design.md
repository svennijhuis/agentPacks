# ADR 0003: Productivity plugin and /tech-design

Status: accepted

## Context

A business architect needs a technical design that developers can build, with almost all gaps closed, so wayfinder can turn it into tickets that can be storypointed. Squad already builds code. This loop needs its own plugin, because squad work is in flight.

The user slash list is closed. The root README (the table under "Using the plugins") and [`docs/CONTRIBUTE.md`](../CONTRIBUTE.md) lock the commands: `/squad`, `/squad-review`, `/scenarios`, `/pack-check`, `/security-audit`, and `/pack-author`. The README and [`docs/PACK-AUTHOR.md`](../PACK-AUTHOR.md) call `/pack-author` the intentional fifth user slash: Lane A for marketplace authors, not a Squad phase. [`docs/pbi/64-squad-session-split.md`](../pbi/64-squad-session-split.md) says "No 5th slash": `/squad split <request>` stays a mode of `/squad`. [`plugins/squad/skills/squad/references/session-split.md`](../../plugins/squad/skills/squad/references/session-split.md) says a later `/squad-split` only runs that section. The squad skill says "No new command." [`advisor-lite`](../../plugins/squad/skills/squad/references/advisor-lite.md) says not to add an advisor command.

[`docs/PLAN.md`](../PLAN.md) already reserves the name `productivity` for a role pack (`grill-me`, `grilling`, `writing-for-agents`, `caveman`, `caveman-compress`). It also says a phase of an existing workflow is not a new pack, a command is what makes a capability pack, and two packs may share an event but not a command. [ADR 0001](0001-source-of-truth-and-generated-files.md) says contributors do not copy upstream Markdown by hand. External skills are URL pins in `external-skills.json` ([`docs/EXTERNAL-SKILLS.md`](../EXTERNAL-SKILLS.md)).

This record is 0003 because open pull request 95 adds ADR 0002. Plan v3.1 was approved by the product owner on 2026-11-11, after Sven's direction.

## Decision

**The plugin is `plugins/productivity/`.** The name is Matt Pocock's productivity folder, the same name the catalog plan reserves. The first version does not import that role-pack list. It lives outside squad so squad work in flight stays untouched. Tech design is not a Squad phase. Squad builds code. This command prepares a design for developers. A user command is a component a skill cannot express, so under the catalog plan this is a capability pack. Later productivity commands can live here. This decision ships one command.

**`/tech-design` is the one new slash.** Sven grants it as the explicit exception to the no-5th-slash rule above. It is not a command on squad. Usage:

- `/tech-design new <name> [types]`
- `/tech-design <name>` resumes that design
- `/tech-design <name> grill [gap]`
- `/tech-design <name> ready`

Grill and ready are modes of this one command, the same way `/squad split` is not its own slash. The person types the command. The model does not start a design on its own. On Copilot the command is namespaced under the plugin, as the root README already does for other packs. Command files are still generated as ADR 0001 describes.

**The purpose is a technical design a business architect prepares for developers.** Almost all gaps are closed before hand-off. Wayfinder then turns the design into tickets that can be storypointed. Skills and playbooks stay generic. They contain no examples of real systems.

**The wayfinder way is the core loop.** Sessions are small. One session takes one gap or one topic: grill, research, write, update `gaps.md`, append one `log.md` line, and stop. A session never writes the whole design. Grill me asks one question at a time along the decision tree and never fills a gap silently. No answer sets that gap to `waiting-on-input`, with an owner and a due date.

**Each design is a folder in the product repo at `docs/design/<name>/`.** It holds `README.md` (how to start, how to resume), `design.md`, `gaps.md`, `research/<topic>.md`, `input/<date>.md`, and `log.md`. It also holds `openapi.yaml` when the api type is in use. Resume starts from that README.

**A gap status is `open`, `research`, `waiting-on-input`, `decided`, or `accepted-risk`.** A gap row has id, question, status, owner, due, waiting-on note, source or answer, and blocks. Owner, due, and the waiting-on note are required while the status is `waiting-on-input`. Every waiting gap has a deadline. A gap starts `open`. A research session sets `research`. A written answer sets `decided`. An accepted risk sets `accepted-risk`. After the input file is in the folder, a later session moves a waiting gap to `research`, or to `decided` when that input is the answer.

```mermaid
stateDiagram-v2
  [*] --> open
  open --> research: session researches
  open --> waiting-on-input: no answer
  research --> waiting-on-input: no answer
  waiting-on-input --> research: input file in
  waiting-on-input --> decided: input is the answer
  open --> decided: answer written
  research --> decided: answer written
  open --> accepted-risk: risk accepted
  research --> accepted-risk: risk accepted
```

**Document status is `draft`, `discussion`, `ready`, `committed`, or `abandoned`.** That lifecycle lives in Git, in the Oxide RFD style. `ready` is allowed only when zero gaps are `open`, `research`, or `waiting-on-input`. `decided` and `accepted-risk` do not block `ready`.

**`design.md` uses this section order.** The set is modeled on Google design docs, plus ideas from Amazon, Uber, Shopify, and Oxide. The words are ours.

1. User outcome.
2. Context and scope.
3. Goals.
4. Non-goals.
5. Glossary.
6. Design: overview, diagram, boundaries and contracts, data model, flows as mermaid.
7. Dependencies: one dependency-contract block each.
8. Alternatives, including do nothing.
9. Cross-cutting: security, privacy, observability, rollout, testing.
10. Type checklists.
11. Advice: the recommended choice, why, and what changes if a gap flips.
12. Risks.
13. Acceptance criteria.
14. Sign-off.

**Type playbooks are separate files: web, api, data, and integration.** Types combine (`types: [web, api]`). The gaps are the union of those checklists, deduped by checklist key, so each gap appears once and carries every type that named it. The api playbook writes or updates `openapi.yaml` (OpenAPI 3.1) as the contract first, with auth level, error codes, and version on each endpoint. An empty field becomes a gap. Playbook text is our own words.

**The dependency-contract playbook runs for every external dependency, and it can be used on its own.** An empty field is a blocking gap. The owner's OpenAPI spec is requested. A missing spec is a blocking gap.

| Field | What it records |
| --- | --- |
| Owner | Who owns it, and their response time |
| Access | Auth method, level or scopes, how to request, lead time |
| Environments | Test and prod URLs, test accounts, allowlist or VPN |
| Contract | Request, response, version, errors, timeout |
| Limits | Rate, SLA, cost |
| Change notice | How a change is announced |
| Proof | A real working call in test before start |

Proof is checked with the verify-this skill. Only VERIFIED closes that gap.

**Imports come from three sources only, all MIT.** Matt Pocock `mattpocock/skills` at `4ae2317ddb22b046e26455e4f9c9b3aaaf62212d`, pstack, and the Cursor team kit in `cursor/plugins` at `ae5650febf4320793844c12c4adaa937f417f8da`. The pstack plugin manifest sets its repository to `https://github.com/cursor/plugins` (tree path `pstack`). pstack and the Cursor team kit are that one repository. No other third-party repository, not even as a source of ideas.

Model-invocable skills are pinned in `external-skills.json` by exact name and the full 40-hex SHA of their source commit, and they are never copied. From Matt: `domain-modeling`, `codebase-design`, `research`, `grilling`. From the Cursor team kit: `verify-this`.

Skills with `disable-model-invocation: true` are copied and adapted. That is a narrow exception to ADR 0001. Each copy has `NOTICE.md`, a Source line, and a What we changed note. From Matt: `wayfinder`, `to-spec`, `to-questionnaire`, `to-tickets`. From pstack: `architect`, `principle-exhaust-the-design-space`, `statechart`, `blast-radius`, `interrogate`. The statechart copy keeps mermaid flows only and takes no scripts. `docs-canvas`, from the Cursor team kit, is optional and is used to show `design.md` at a meeting.

**`/tech-design <name> ready` is the gate.** It runs the gate, runs a final attack pass, sets document status to `ready`, writes tickets whose blockers are sized for storypoints, and writes a wayfinder brief. Everywhere in the loop, including this gate: check the real code first, and invent nothing.

**Delivery is one pull request at a time, in this order.** TD0 is this ADR. TD1 is the plugin skeleton. TD2 is the loop core and grill. TD3 is dependency-contract. TD4 is web and api. TD5 is data and integration. TD6 is research, input, and options. TD7 is readiness. TD8 is smoke.

## Consequences

- A product repo holds one working set per design, under `docs/design/<name>/`. The next session resumes from that folder.
- Tickets and the wayfinder brief exist only after `ready`. `ready` waits until no gap is `open`, `research`, or `waiting-on-input`.
- The catalog gains one command, `/tech-design`, on a new plugin. Squad's commands stay as they are. The README, `docs/CONTRIBUTE.md`, and `docs/PLAN.md` still describe today's catalog until the plugin pull request updates them.
- Model-invocable skills stay pins. The named `disable-model-invocation` skills are the only copied upstream Markdown, and each copy shows its source and what we changed.
- An external dependency with an empty field, a missing owner spec, or no VERIFIED test call stays a blocking gap.
- A later productivity command has a plugin to land in. A new slash still needs its own decision.

## Alternatives rejected

**Put `/tech-design` inside squad.** Squad work is in flight, and the squad charter says no new command and no 5th slash.

**Give grill, research, and ready their own slashes.** That would be several exceptions. One command with modes keeps the exception to a single slash.

**Ship the catalog plan's productivity skill list in this plugin.** Those names are a different job, and some of them come from repositories this decision does not allow.

**Pin the `disable-model-invocation` skills and never adapt them.** The model-invocable skills work as pins. The copied set has to run under this loop, so it is adapted and labeled.

**Let one session write the whole `design.md`.** The wayfinder way is one gap or one topic, then stop. A full draft hides open gaps.

**Treat an empty dependency field as a note.** An empty field is a blocking gap. A missing owner spec is a blocking gap. Only VERIFIED proof closes the proof gap.
