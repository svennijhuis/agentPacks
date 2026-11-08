# frontend

A role pack for frontend craft: visual direction, motion, and verification in a real browser. Every
skill in it is a reviewed external import pinned to an exact upstream commit; the pack authors none
of its own.

Install it because you build interfaces, whatever you build them in. It fills no [Squad
slot](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-LANGUAGE-PACK.md), carries no `language-pack` keyword, and has no row in
`pack-check`'s registry, so Squad never resolves it by name. Its skills are reached the ordinary
way, with the Skill tool.

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
| `webapp-testing` | anthropics/skills | What the browser actually did |

Eight of the nine motion skills are pinned for reference closure rather than because they were
asked for: `animate` hands native work to `animate-expo` and components to `pick-ui-library`,
`choreograph` defers the whether-to-animate question to `find-animation-opportunities` and
`remove-animations`, and `review-animations` and `improve-animations` name each other. Nothing in
the set points outside the pack.

React, React Native and shadcn/ui framework skills are **not** here. [`PLAN.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/PLAN.md)
puts framework skills in the language pack, so they are pinned in
[`typescript`](../typescript/README.md).

## Precedence between overlapping skills

Twelve skills with opinions about the same screen pull against each other unless the order is
written down. It is:

1. **Visual direction is `frontend-design`'s.** It decides palette, type, layout concept and how
   much boldness the brief earns. `emil-design-eng` and `apple-design` execute inside that
   direction; they do not reopen it.
2. **Whether to animate comes before how.** `find-animation-opportunities` and `remove-animations`
   answer whether; `animate` (web) or `animate-expo` (native) answer how for one animation;
   `choreograph` takes over once two or more things move on the same event.
3. **`frontend-design`'s motion limit is the ceiling all five work under.** One orchestrated moment
   beats scattered effects, and per-section fade-and-slide-up entrances stay out.
4. **`webapp-testing` is the verification step, not a reviewer.** `review-animations` judges motion;
   `webapp-testing` establishes what the browser did.
5. **Measured cost outranks taste.** Where [`typescript`](../typescript/README.md)'s
   `vercel-react-best-practices` has a number (a dropped frame, a bundle cost, a hydration
   flicker), it wins. Where it only states a preference, the design skills decide.

## Interaction with the other packs

| Overlap | Resolution |
|---|---|
| `emil-design-eng` mandates a Before/After markdown table for UI code review; [Squad](../squad/README.md)'s [review contract](../squad/skills/squad/references/review-contract.md) specifies a severity table | Presentational only. The contract's input normalization merges a noncanonical layout when every semantic field is present, and Squad's severity vocabulary still decides the verdict |
| `improve-animations` dispatches an executor subagent in its own worktree; Squad owns the [worktree lifecycle](../squad/skills/squad/references/worktree.md) and the uncommitted hand-off | Inside a Squad run, use `improve-animations` for the audit and plan only and let the loop implement. Its `execute <plan>` path is for standalone use |
| `webapp-testing` starts a local server and a browser | Nothing else in the catalog does |
| Hooks | This pack installs none. [`git`](../git/README.md) owns the only shell guard and Squad owns the hand-off; a frontend pack has no business on either event |

## Why this is a role pack

[The catalog plan](https://github.com/svennijhuis/agentPacks/blob/main/docs/PLAN.md) puts `frontend` on the *layer* axis, where a split waits for
a fat language pack. That rule is about cutting `typescript` into halves, and this pack is not that
cut: after the framework skills moved to `typescript`, nothing left in it is TypeScript-specific.
Design direction, motion decisions and browser verification apply to a Swift screen or a static HTML
page as readily as to Next.js. It installs because of the kind of work you do, which is the
role-pack test.

## Imports that were reviewed and rejected

Checked against the [external-skills review checklist](https://github.com/svennijhuis/agentPacks/blob/main/docs/EXTERNAL-SKILLS.md) and left out:

| Candidate | Why not |
|---|---|
| `impeccable` (pbakaus/impeccable) | Its launcher runs "a self-contained binary that ships next to it or is downloaded once on first run": unpinned code fetched at runtime, which checklist point 4 exists to prevent. It also hardcodes `.claude/skills/impeccable/...` paths, self-installs a post-edit detector hook, and pins its own `/<command>` shortcuts |
| `design-taste-frontend` (Leonxlnx/taste-skill) | Declares a `Stop` hook with a 600-second timeout in skill frontmatter, running `node gate.mjs --hook claude`. Hooks belong in `hooks.source.json` where they are reviewed and generated per client; an imported stop-time gate is Claude-only and a second claim on the event Squad uses for its hand-off |
| `ui-ux-pro-max` (nextlevelbuilder/ui-ux-pro-max-skill) | Every documented command is `python "${CLAUDE_PLUGIN_ROOT}/.claude/skills/ui-ux-pro-max/scripts/search.py"`, a path that does not exist once the skill is materialized at `skills/ui-ux-pro-max/`, and upstream Markdown is not ours to rewrite. Its `--design-system` step is also marked REQUIRED for new pages, putting a 51-style, 192-palette catalog in charge of exactly the decision `frontend-design` requires you to revise when it reads as a default |
| `web-design-guidelines` (vercel-labs/agent-skills) | No license: the repository has no `LICENSE` file and this skill, unlike its siblings, declares none in frontmatter. Its body also fetches the rules from a `main` URL at review time, so pinning a commit would pin nothing |
| `canvas-design`, `skill-creator` (anthropics/skills) | Not frontend. `canvas-design` produces static `.png` and `.pdf` art; `skill-creator` authors skills. One home per subject |

`critique`, `polish`, `bolder`, `distill`, `quieter` and `delight` are not separate skills upstream:
they are sub-commands inside `impeccable` and would have arrived with it. `Leonxlnx/taste-skill`
likewise ships one skill, not five.

## Upstream frontmatter worth knowing

- `review-animations` and `pick-ui-library` set `disable-model-invocation: true` upstream with no
  Codex half. Generation writes the `agents/openai.yaml` half, so both dialects stay in sync, and
  publication writes `user-invocable: false` on every pin, so a pin stays a Skill-tool dependency
  rather than becoming a sixth slash command.
- Upstream descriptions are long essays, not the short when-to-use lines
  [ADD-SKILL.md](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-SKILL.md) asks of authored skills. That is upstream's text and is
  left as authored.

## Editing this pack

Authored: `plugin.json`, `external-skills.json`, and this README. Nothing else: publication
materializes each import under `skills/<name>/` on `marketplace-beta`, never on `main`.

Reference closure over imports is a review duty, not a validator check, so follow every reference in
a candidate before pinning it.

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
```

See [EXTERNAL-SKILLS.md](https://github.com/svennijhuis/agentPacks/blob/main/docs/EXTERNAL-SKILLS.md) and [PLAN.md](https://github.com/svennijhuis/agentPacks/blob/main/docs/PLAN.md).
