# frontend

A role pack for frontend craft: visual direction, motion, React and React Native implementation,
component composition, and browser verification. Every skill in it is a reviewed external import
pinned to an exact upstream commit; the pack authors no skills of its own.

Install it because of the surface you build, not the language you compile. It is not a layer split
of a language pack — see [Why this is a role pack](#why-this-is-a-role-pack).

## What is in it

| Skill | Upstream | Answers |
|---|---|---|
| `frontend-design` | anthropics/skills | What should this look like, and which choices read as generated defaults |
| `emil-design-eng` | emilkowalski/skills | Which details make a component feel finished |
| `apple-design` | emilkowalski/skills | Springs, gestures, materials, optical typography |
| `animate` | emilkowalski/skills | Build one web animation, in decision order |
| `animate-expo` | emilkowalski/skills | The same, in React Native and Expo |
| `choreograph` | emilkowalski/skills | Several elements moving as one intentional motion |
| `find-animation-opportunities` | emilkowalski/skills | What should animate but does not |
| `remove-animations` | emilkowalski/skills | What animates but should not |
| `improve-animations` | emilkowalski/skills | A whole codebase's motion, as a prioritized plan |
| `review-animations` | emilkowalski/skills | Whether a motion diff clears the bar |
| `pick-ui-library` | emilkowalski/skills | Which library for a given frontend task |
| `vercel-react-best-practices` | vercel-labs/agent-skills | React and Next.js performance |
| `vercel-composition-patterns` | vercel-labs/agent-skills | Component APIs that scale |
| `vercel-react-view-transitions` | vercel-labs/agent-skills | Route and shared-element transitions |
| `vercel-react-native-skills` | vercel-labs/agent-skills | React Native and Expo implementation |
| `shadcn` | shadcn-ui/ui | shadcn/ui components, registries, presets, design systems |
| `webapp-testing` | anthropics/skills | Verifying the result in a real browser |

Authored here: `plugin.json`, `external-skills.json`, this README. Nothing else — generation
materializes each import under `skills/<name>/` on the `marketplace` branch, never on `main`.

## Precedence between overlapping skills

Seventeen skills with opinions about the same screen will pull against each other unless the order
is written down. It is:

1. **Visual direction is `frontend-design`'s.** It decides palette, type, layout concept and how
   much boldness the brief earns. `emil-design-eng`, `apple-design` and `shadcn` execute inside that
   direction; they do not reopen it.
2. **Whether to animate comes before how.** `find-animation-opportunities` and `remove-animations`
   answer whether; `animate` (web) or `animate-expo` (native) answer how for one animation;
   `choreograph` takes over once two or more things move on the same event. `frontend-design`'s
   limit on non-user-triggered motion is the ceiling all four work under: one orchestrated moment
   beats scattered entrances, and per-section fade-and-slide-up stays out.
3. **Implementation rules lose to design intent only on measurement.** `vercel-react-best-practices`
   wins where it has a number — a dropped frame, a bundle cost, a hydration flicker. Where it only
   expresses a preference, the design skills decide.
4. **`shadcn` owns the component layer, not the brand.** It applies a DESIGN.md or brand spec; the
   spec itself comes from `frontend-design`.
5. **`webapp-testing` is the verification step, not a reviewer.** `review-animations` judges motion;
   `webapp-testing` establishes what the browser actually did.

## Interaction with the other packs

| Overlap | Resolution |
|---|---|
| `emil-design-eng` mandates a Before/After markdown table for UI code review; [`delivery-loop`](../delivery-loop/README.md)'s [review contract](../delivery-loop/skills/delivery-loop/references/review-contract.md) specifies a severity table | Presentational only. The contract's input normalization accepts a noncanonical layout when every semantic field is present, so the orchestrator merges it as-is. The loop's severity vocabulary still decides the verdict |
| `improve-animations` dispatches an executor subagent in an isolated worktree; `delivery-loop` owns the worktree lifecycle and its hand-off | Inside a loop, use `improve-animations` for the audit and plan only, and let the loop implement. Its `execute <plan>` path is for standalone use |
| `webapp-testing` runs a local server and a browser; no other pack does | No overlap |
| Hooks | This pack installs none. [`git`](../git/README.md) owns the only shell guard and `delivery-loop` owns the stop-time hand-off; a frontend pack has no business on either event |

The pack contributes no [language-pack slots](../../docs/ADD-LANGUAGE-PACK.md) and carries no
`language-pack` keyword, so it has no row in `pack-check`'s registry and the delivery loop never
resolves it by name. It is loaded the ordinary way, by an agent reading a description.

## Why this is a role pack

[The catalog plan](../../docs/PLAN.md) puts `frontend` on the *layer* axis and allows a layer split
only after a language pack gets fat. That rule is about splitting `typescript` into halves, and this
is not that pack: nothing in it is TypeScript-specific, nothing fills a loop slot, and the design,
motion and verification skills apply to a Swift screen or a static HTML page as readily as to Next.js.
It installs because you build interfaces, which is the role-pack test.

The React, React Native and shadcn skills are the framework skills the plan already expects to live
inside a pack rather than become plugins of their own. They sit here instead of in `typescript`
because they travel with the design and motion skills that decide what they implement, and because a
`typescript` pack does not exist yet to hold them. When it does and the frontend set outgrows this
pack, the split is the plan's ordinary one.

## Imports that were reviewed and rejected

Checked against the [external-skills review checklist](../../docs/EXTERNAL-SKILLS.md) and left out:

| Candidate | Why not |
|---|---|
| `impeccable` (pbakaus/impeccable) | Its launcher runs "a self-contained binary that ships next to it or is downloaded once on first run" — unpinned code fetched at runtime, which the checklist's point 4 exists to prevent. It also hardcodes `.claude/skills/impeccable/...` paths, self-installs a post-edit detector hook, and pins its own `/<command>` shortcuts |
| `design-taste-frontend` (Leonxlnx/taste-skill) | Declares a `Stop` hook with a 600-second timeout in skill frontmatter, running `node gate.mjs --hook claude`. Hooks belong in `hooks.source.json` where they are reviewed and generated per client; an imported stop-time gate is both Claude-only and a second claim on the event `delivery-loop` uses for its hand-off |
| `ui-ux-pro-max` (nextlevelbuilder/ui-ux-pro-max-skill) | Every documented command is `python "${CLAUDE_PLUGIN_ROOT}/.claude/skills/ui-ux-pro-max/scripts/search.py"`, a path that does not exist once the skill is materialized at `skills/ui-ux-pro-max/`, and we do not rewrite upstream Markdown. Its `--design-system` step is also marked REQUIRED for new pages, which puts a 51-style, 192-palette catalog in charge of exactly the decision `frontend-design` requires you to revise when it reads as a default |
| `web-design-guidelines` (vercel-labs/agent-skills) | No license: the repository has no LICENSE file and this skill, unlike its siblings, declares none in frontmatter. Its body also WebFetches the rules from a `main` URL at review time, so pinning a commit would pin nothing |
| `canvas-design`, `skill-creator` (anthropics/skills) | Not frontend. `canvas-design` produces static `.png` and `.pdf` art; `skill-creator` authors skills. One home per subject |

`critique`, `polish`, `bolder`, `distill`, `quieter` and `delight` are not separate skills upstream —
they are sub-commands inside `impeccable`, and would have arrived with it.

## Upstream quirks worth knowing

- `review-animations` and `pick-ui-library` set `disable-model-invocation: true` and no Codex
  equivalent, so upstream makes them user-invoked on Claude and model-invoked everywhere else. The
  plan asks for both dialects or neither; this is upstream's inconsistency and is left as authored.
- `shadcn` sets `user-invocable: false`, which is not a field in the Agent Skills frontmatter table.
  Clients that do not know it ignore it.
- `vercel-*` skills carry their license in frontmatter rather than a repository LICENSE file, which
  is why the manifest records MIT per entry.

## Editing this pack

Add or move a pin in `external-skills.json` and follow
[EXTERNAL-SKILLS.md](../../docs/EXTERNAL-SKILLS.md). Reference closure over imports is a review
duty, not a validator check: nine of the eleven motion skills are here because another one names
them.

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
```
