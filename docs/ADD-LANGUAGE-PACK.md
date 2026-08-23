# Add a language pack

A language pack is how the [delivery loop](../plugins/delivery-loop/README.md) learns what it is looking at. The loop's agents are language-agnostic on purpose; the code they edit never is. This document is the contract between the two.

Read [the catalog plan](PLAN.md) first — it decides *whether* something earns a plugin. This decides what a language pack must contain once it has.

## The contract is a set of skill names

A language pack contributes by shipping skills at **contracted names**. Nothing else carries it:

| Component | Why not |
|---|---|
| Rules | Glob-scoped rules are Cursor-only and generate a portability warning; use skills for conditional language guidance that must reach every client |
| Agents | Codex does not load agents from a plugin at all. And an agent is a role — the loop already has seven; it does not need a C#-flavoured copy of each |
| Hooks | A hook acts, it does not inform. `dotnet format` on write is a hook; how to write an integration test is not |

Skills are the one component every client loads identically, which makes them the only thing the loop can rely on finding.

## The slots

| Slot | Skill name | Answers | Read by | Required |
|---|---|---|---|---|
| Build | `<lang>-build` | Toolchain, project layout, the build and run commands | `loop-implementer`, `loop-simplifier` | yes |
| Test | `<lang>-test-patterns` | How a test is written here; unit vs integration; fixtures; the test command | `loop-implementer`, `loop-verifier` | yes |
| Review | `<lang>-review` | Language-specific review checklist | `loop-reviewer` | no |
| Security | `<lang>-security-review` | Ecosystem footguns, layered on top of OWASP | `loop-security-reviewer` | no |

`<lang>` is the pack name, for example `dotnet`.

The names *are* the interface. A skill called `dotnet-testing` instead of `dotnet-test-patterns` is a skill the loop silently never loads — which is why `LanguagePackValidator` fails the build on a near-miss rather than letting it ship.

## Framework skills are not slots

Framework knowledge keeps the `[<framework>-]<action>-<object>` shape from [PLAN.md](PLAN.md) — `aspnet-api-design`, `react-component-scaffold`, `axum-routing` — and is reached *through* the slot skills, never discovered by the loop directly.

That is deliberate. Frameworks churn faster than languages; if the loop's contract named them, every Next.js major would be a change to the delivery loop.

## Steps

1. Confirm the pack earns a plugin at all: [PLAN.md](PLAN.md). A framework never does.
2. Create both `plugins/<lang>/skills/<lang>-build/SKILL.md` and `plugins/<lang>/skills/<lang>-test-patterns/SKILL.md`. A delivery loop must be able to build and verify the stack.
3. Add `"language-pack"` to `keywords` in `plugins/<lang>/plugin.json`. That is what turns the validator's checks on.
4. Add the marker, stack and pack row to `plugins/pack-check/skills/pack-check/references/packs.md`. That row is what makes the new pack discoverable at session start.
5. Fill the optional review slots as you have real content for them. A thin `<lang>-security-review` is worse than none — OWASP is already the floor.
6. Write the pack `README.md` with the slot table, so a reader can see what is filled and what is not.
7. Validate and open a pull request:

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
```

Frontmatter and body rules are the ordinary skill rules: [ADD-SKILL.md](ADD-SKILL.md).

## Writing a slot skill

The loop reads these under time pressure, in the middle of another task. Write for that.

- **Facts and commands, not philosophy.** *What deserves a test* is cross-language craft and belongs in a role pack. *How you write an integration test in this stack* is the slot.
- **Say how to find the shape.** Repositories differ. Open with the `ls`/`grep` that establishes which variant this one is, then give the defaults for when it answers nothing.
- **Give exact commands.** The verifier runs what `<lang>-test-patterns` says. `dotnet test <solution>` is usable; "run the tests" is not.
- **Name the failure.** A slot skill earns its place by covering what an agent gets wrong unprompted — a container started per test instead of per collection, a `Version` attribute under Central Package Management.
- **Inspect the repository.** State which project files, dependencies, directory layout, and repeated
  local patterns select among the variants the pack supports. One isolated example is not a convention.

## Canonical pack standards

Do not repeat the same rule across three skills. Put canonical Markdown documents in
`plugins/<lang>/standards/`, then map them to consumers with `standards.source.json`:

```json
{
  "$schema": "../../schema/standards.schema.json",
  "version": 1,
  "documents": {
    "testing": "standards/testing.md"
  },
  "consumers": {
    "dotnet-test-patterns": ["testing"],
    "dotnet-review": ["testing"]
  }
}
```

Generation places the selected documents under each skill's `references/standards/` directory on the
`marketplace` branch or in temporary output. Source `main` stays authored-only. Every consuming skill
must tell the agent to read those references before acting.

The validator rejects unknown keys, paths outside the plugin, missing Markdown files, unknown skills,
duplicate references, and unused documents.

## Repository evidence

The plugin is the complete standards package. Repository-specific choices come from the repository
itself: project and formatter configuration, dependencies, directory layout, tests, and repeated
nearby code patterns. The planner records those under `## Repository conventions observed`, with a
path or configuration entry for each claim. Later phases consume that evidence from the plan so they
do not rediscover a different style independently.

Repository evidence specializes choices a canonical standard leaves open. A direct conflict with a
plugin requirement becomes a planning decision; it is never silently converted into a new standard.

## What the build enforces

| Check | Enforced by | Why |
|---|---|---|
| Both `<lang>-build` and `<lang>-test-patterns` | `LanguagePackValidator` | A full loop must know how to build and how to verify the stack |
| Exactly one row in `pack-check`'s bundled registry | `PackCheckContractTests` | An installed pack that cannot be discovered is unreachable onboarding knowledge |
| Every `<lang>-*` skill is a known slot or matches `[<framework>-]<action>-<object>` | `LanguagePackValidator` | A misspelled slot is a skill the loop never finds, and nothing else would notice |
| Standards manifest references only existing skills and canonical Markdown files | `StandardsValidator` | Generated references cannot silently disappear or drift from their source |
