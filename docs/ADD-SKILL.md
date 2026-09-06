# Add a skill

Skills are the portable, reusable part of agentPacks. Every compatible client loads them, so this is where shared knowledge belongs.

## Steps

1. Pick the owning plugin with the rule in [the catalog plan](PLAN.md): a language pack for anything that needs a compiler, a role pack for anything that does not, and never a new plugin for a framework.
2. Create `plugins/<plugin>/skills/<name>/SKILL.md` there.
3. Write the frontmatter and the instructions.
4. Prove it locally with the commands in [Test a skill locally](#test-a-skill-locally).
5. Open a pull request.

## Frontmatter

Agent Skills publishes no JSON Schema, so the validator implements the specification's normative table directly:

| Field | Required | Rule |
|---|---|---|
| `name` | yes | 1–64 characters, lowercase letters, digits and single hyphens. No leading or trailing hyphen, no `--`, **no periods**. Must equal the directory name. |
| `description` | yes | Non-empty, at most 1024 characters. Say what it does *and* when to use it. |
| `license` | no | License name or a reference to a bundled license file. |
| `compatibility` | no | At most 500 characters. Only when the skill has real environment requirements. |
| `metadata` | no | A mapping of string keys to string values. Contracted `<lang>-*` slots must set `audience: loop`. |
| `allowed-tools` | no | Space-separated string. Experimental; support varies between clients. |
| `disable-model-invocation` | no | `true` for a user-invoked entrypoint. Generation writes the Codex `agents/openai.yaml` half so both dialects stay in sync. |

Plugin names may contain periods (`acme.tools` is valid); skill names may not. This trips people up.

Authored skills in this repo use that short frontmatter. A loop slot (not a user entrypoint):

```markdown
---
name: dotnet-build
description: Internal loop skill. Loaded by the Squad orchestrator by exact Skill tool name, not as a user entrypoint. How a .NET repository is laid out, restored and built.
license: UNLICENSED
metadata:
  audience: loop
---
```

A user-invoked entrypoint (`squad`, `pack-check`) adds `disable-model-invocation: true` and omits
`audience: loop`. Load either kind with the Skill tool by exact name, never slash-prose.

## Writing the description

The description is loaded for every skill at startup, and it is the only thing an agent uses to decide whether to open the skill. Write it for that decision.

Good: `Extracts text and tables from PDFs, fills forms, merges files. Use when working with PDF documents.`

Poor: `Helps with PDFs.`

## Writing the body

- Keep `SKILL.md` under roughly 500 lines. Move detail into `references/` and link to it — agents load those files only when needed.
- Put runnable code in `scripts/`, static resources in `assets/`.
- Reference other files with paths relative to the skill root, one level deep.
- A language-pack slot names its canonical docs under `Standards in force:` and tells the agent to cite the filename during review and build. Load the slot with the Skill tool by exact name.

## YAML support

Frontmatter is parsed with a real YAML parser, so quoted values containing colons, folded and multiline scalars, comments and nested `metadata` maps all work as expected.

## Test a skill locally

Do not publish to the marketplace branch or merge to `main` to test a skill. These commands
work on a feature branch without merging to `main`. Coworker verification is real `dotnet test`
/ `validate` / `validate-all --out` on a fixture, then a local install of that generated tree.

A GitHub marketplace install is generated. Symlinking the authored `plugins/<name>` directory and
trying it in a client is not enough: that tree is missing generated Codex policy, client agent
files, and skill-local standards references.

1. Clone and check out the branch (`gh pr checkout 7` on this PR).
2. Edit under `plugins/squad/` or a language pack.
3. Validate and test, including the fixture plugin suite:

```bash
dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet test tools/AgentPacks.slnx
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
```

4. Install the generated copy locally — three client paths, no merge to `main`:

```bash
claude --plugin-dir /tmp/agentpacks-marketplace/plugins/squad
ln -sfn /tmp/agentpacks-marketplace/plugins/squad ~/.cursor/plugins/local/squad
copilot plugin marketplace add /tmp/agentpacks-marketplace
copilot plugin install squad@agentpacks
```

5. Reload, then smoke `/squad` or `/review` once.

`validate-all --out` writes the marketplace-shaped tree: client namespaces, remapped agent `model`
fields, and `skills/<name>/agents/openai.yaml` for user-invoked skills. Inspect the fixture plugin
you added there:

```bash
ls /tmp/agentpacks-marketplace/plugins/<plugin>/skills/<name>
dotnet run --project tools/AgentPacks.Cli -- validate
```

To smoke-test the generated plugin in a client that already has the marketplace install, point the
client at the **generated** copy, then reload:

```bash
ln -sfn /tmp/agentpacks-marketplace/plugins/<plugin> ~/.cursor/plugins/local/<plugin>
```

Do not symlink `plugins/<plugin>` from `main` over a marketplace install and call that verification.
The authored tree is the source; the generated tree is what coworkers install.

A contracted language-pack slot is an internal loop skill. After adding one, the suite must still
resolve it by exact name: `dotnet-build`, not `dotnet-builds`. `LanguagePackContractTests` and
`dotnet test` catch a near-miss before anyone installs it.

## A note on strictness

The validator is deliberately stricter than the minimum client conformance bar. The specification lets clients skip an invalid skill and carry on; we fail the build instead, because a skill that silently fails to load is worse than a red pull request.
