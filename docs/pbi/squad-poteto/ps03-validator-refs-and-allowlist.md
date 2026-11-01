# PS03: Validator rules for paths, principle names, playbook pointers, /loop
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B0
- Depends on: PS01
- Lane: Builder (C# in tools/AgentPacks.Cli)
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "please check all this skills and check all feedback so we will accept this all and don't give warnings etc for this".
- 0 warnings must be real: a broken pointer must fail, a good one must pass. Today renamed upstream playbooks break squad silently.

## Current state on main (`d76fa14`)

- `SkillReferenceValidator` checks backticked `/name` refs only.
- Not checked: `../<name>/SKILL.md` paths, bold principle or standard names in an index, playbook file pointers.
- `/loop` is a client built-in and never resolves.
- pack-author bar: router about 40 lines, so a poteto-style hub warns forever.

## Target structure

New rules (ids for the docs, Builder may rename to fit the code):

| Rule | Check | Severity |
|---|---|---|
| REF01 | Every `../<name>/SKILL.md` resolves to a skill in the plugin or an `external-skills.json` record | error |
| REF02 | Every bold entry in a `## Principles` index resolves to a `principle-<kebab>` leaf (kebab rule from poteto) | error |
| REF03 | Every bold entry in a `## Standards` index resolves to a `standard-<kebab>` leaf (PS14) | error |
| REF04 | Every `playbooks/<x>.md` and `../poteto-mode/playbooks/<x>.md` pointer resolves | error |
| REF05 | Client built-ins allowlist: `/loop` | info |
| HUB01 | A skill with the hub H2 set may exceed the 40-line router bar | none |

Kebab rule (from poteto Principles intro): title in kebab case, hyphens kept, other punctuation dropped, unless the entry names the leaf.

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| kebab rule source | poteto-mode Principles intro | [poteto-mode](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | rides with the poteto-mode pin | none |
| principles cited | principle-encode-lessons-in-structure, principle-build-the-lever | [principle-encode-lessons-in-structure](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-encode-lessons-in-structure/SKILL.md) | pinned as-is (`external-skills.json`, 40-hex) | none |

## Subagents and what they load

- none

## Steps for Builder

1. Add REF01 to REF04 to `SkillReferenceValidator` (or a sibling validator).
2. Add the built-in allowlist with `/loop` only.
3. Add HUB01 exception keyed on the hub H2 set, not on a file name.
4. Tests: one good and one broken fixture per rule. Mutation probe: remove the rule, test fails.

## DoD

- Each rule has a failing and a passing fixture test.
- Mutation probe bites for every rule.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Test names per rule.
- Probe output in PR body.

## Risks and out of scope

- REF02 needs the upstream leaf names: works once PS04 pins them; tests use local FAKE leafs.
- Out of scope: pinning (PS04).
