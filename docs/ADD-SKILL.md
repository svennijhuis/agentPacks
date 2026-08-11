# Add a skill

Skills are the portable, reusable part of agentPacks. Every compatible client loads them, so this is where shared knowledge belongs.

## Steps

1. Create `plugins/<plugin>/skills/<name>/SKILL.md` in the plugin that owns the capability.
2. Write the frontmatter and the instructions.
3. Run `dotnet run --project tools/AgentPacks.Cli -- validate`.
4. Open a pull request.

## Frontmatter

Agent Skills publishes no JSON Schema, so the validator implements the specification's normative table directly:

| Field | Required | Rule |
|---|---|---|
| `name` | yes | 1–64 characters, lowercase letters, digits and single hyphens. No leading or trailing hyphen, no `--`, **no periods**. Must equal the directory name. |
| `description` | yes | Non-empty, at most 1024 characters. Say what it does *and* when to use it. |
| `license` | no | License name or a reference to a bundled license file. |
| `compatibility` | no | At most 500 characters. Only when the skill has real environment requirements. |
| `metadata` | no | A mapping of string keys to string values. |
| `allowed-tools` | no | Space-separated string. Experimental; support varies between clients. |

Plugin names may contain periods (`acme.tools` is valid); skill names may not. This trips people up.

## Writing the description

The description is loaded for every skill at startup, and it is the only thing an agent uses to decide whether to open the skill. Write it for that decision.

Good: `Extracts text and tables from PDFs, fills forms, merges files. Use when working with PDF documents.`

Poor: `Helps with PDFs.`

## Writing the body

- Keep `SKILL.md` under roughly 500 lines. Move detail into `references/` and link to it — agents load those files only when needed.
- Put runnable code in `scripts/`, static resources in `assets/`.
- Reference other files with paths relative to the skill root, one level deep.

## YAML support

Frontmatter is parsed with a real YAML parser, so quoted values containing colons, folded and multiline scalars, comments and nested `metadata` maps all work as expected.

## A note on strictness

The validator is deliberately stricter than the minimum client conformance bar. The specification lets clients skip an invalid skill and carry on; we fail the build instead, because a skill that silently fails to load is worse than a red pull request.
