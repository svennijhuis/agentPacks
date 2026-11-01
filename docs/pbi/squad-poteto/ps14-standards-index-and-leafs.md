# PS14: Standards index and standard-* leafs (same shape as poteto principles)
- Status: todo (docs only, build waits for Sven go per PBI)
- Batch: B2+ (right after B2)
- Depends on: PS06, PS10
- Lane: Builder
- door: two-way
- kind: skill
- Source: [skill map](sources/pstack-matt-skill-map.md) (Repo Research 2026-11-01), pstack `ae5650f`, Matt `4ae2317`

## Goal and why

- Sven: "en ook met principle etc hoe poteto gedaan heeft hoe wij dan zelfde soort onze standarts moeten doen".
- PO: Standards index in squad-md, one leaf per standard, playbooks name standards by name, subagents load only what they need, write-skill can create a standard leaf, `/pack-check` checks index and leafs.

## Current state on main (`d76fa14`)

- Standards are inline in skills and language packs, plus room rules in chat only (no em dash, no tokens in markdown, squash merge, personal only).

## Target structure

Index in `squad-md/SKILL.md`:

```markdown
## Standards

Read the leaf skill in full for any standard you apply. Each entry names when it applies. Each leaf is the **standard-<title>** skill, title in kebab case.

- **No Em Dash.** Any prose we write. Leaf: standard-no-em-dash.
- **No Secrets in Markdown.** Any file or PR body. Leaf: standard-no-secrets-in-markdown.
- **Squash Merge One by One.** Any merge step.
- **Personal Account Only.** Any GitHub action.
- **Coding Standards File.** Any review or new repo. Leaf: standard-coding-standards.
- **Review Contract.** Any review finding.
```

Side by side:

| poteto `principle-<x>/SKILL.md` | our `standard-<x>/SKILL.md` |
|---|---|
| frontmatter: name, description "Apply when ..." | frontmatter: name, description "Apply when ...", `user-invocable: false` |
| H1 title | H1 title |
| the principle in a few lines | what the standard is |
| why it matters | why (Sven's words, date) |
| when it applies / when not | when it applies / when not |
| examples | good and bad examples |
| (implicit) | how to check it (command, test or validator rule id) |

Leafs live in `plugins/squad/skills/standard-<x>/SKILL.md` (sibling dirs, so `../<name>/SKILL.md` resolves).

## External skills per step

| Flow step | Skill | Link (pinned SHA) | Reuse mode | Depends on |
|---|---|---|---|---|
| shape | poteto principle leaf | [principle-prove-it-works](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/principle-prove-it-works/SKILL.md) | shape reference only | none |
| index rule | poteto-mode Principles intro | [poteto-mode](https://github.com/cursor/plugins/blob/ae5650febf4320793844c12c4adaa937f417f8da/pstack/skills/poteto-mode/SKILL.md) | shape reference only | none |

## Subagents and what they load

- Each agent file names only the standard leafs it needs (PS15): reviewer gets review-contract and coding-standards; implementer gets no-em-dash and no-secrets.

## Steps for Builder

1. write-skill gets a third target kind: standard leaf (small addition to PS01 playbook, or a `playbooks/write-standard.md` if over 5 lines).
2. Write the 6 leafs above.
3. Add the index to the hub.
4. Playbooks name standards by bold name.
5. REF03 (PS03) checks index and leafs.

## DoD

- Every index entry resolves to a leaf (REF03).
- Every leaf has the 6 parts.
- A standard leaf made by write-skill passes check-skill.
- `dotnet run --project tools/AgentPacks.Cli -- validate` exits 0 with 0 errors and 0 warnings.
- `/pack-check` run locally reports 0 warnings (same checks as `validate`, PO decision 1).
- `dotnet test tools/AgentPacks.Cli.Tests` green.
- No em dash, no tokens in markdown, FAKE fixture values only.

## Evidence

- Leaf list, REF03 test output.

## Risks and out of scope

- Do not duplicate CODING_STANDARDS.md content: the leaf points to the file. Out of scope: per-language standards (PS17).
