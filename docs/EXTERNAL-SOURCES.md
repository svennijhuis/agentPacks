# External skills

External skills are **referenced, never copied**. This repository stores a URL, a pinned commit and a little metadata; the skill's own content stays in its own repository, where its author maintains it.

Approved sources live in `external/sources.json`. The file must exist even when nothing is approved:

```json
{ "sources": [] }
```

## Adding one

```json
{
  "name": "code-review",
  "description": "Two-axis review of a diff: repository standards and the originating spec.",
  "repository": "https://github.com/mattpocock/skills",
  "path": "skills/engineering/code-review",
  "commit": "84fdeffd12f2ee307994d1eb6feb48173b6e0502",
  "license": "MIT"
}
```

| Field | Why |
|---|---|
| `name` | What the skill installs and is invoked as. |
| `description` | One line for the catalog. Optional. |
| `repository` | Where it lives. |
| `path` | Which directory inside that repository is the skill. |
| `commit` | Exact 40-character commit SHA. |
| `license` | Reviewed before adoption. |

Run `validate-all`, then open a pull request. Generation turns each entry into a catalog entry:

```json
{
  "name": "code-review",
  "source": {
    "source": "git-subdir",
    "url": "https://github.com/mattpocock/skills",
    "path": "skills/engineering/code-review",
    "sha": "84fdeffd12f2ee307994d1eb6feb48173b6e0502"
  }
}
```

The client does a sparse clone of just that subdirectory at that commit. A directory whose `SKILL.md` sits at its root loads as a single skill, and its own frontmatter supplies the name and description that the agent sees.

## Never track a branch

`main` is not a version. The validator rejects anything that is not a full commit SHA, in `sources.json` and again in the generated entry, because a moving reference means the content someone runs today is not the content that was reviewed.

Updating is deliberate: bump the SHA in a pull request, re-review the diff upstream, merge.

## Which clients get them

Catalog entries are read by Claude Code and by Copilot CLI, which also looks for `.claude-plugin/marketplace.json`. Cursor, Codex and Kiro install the portable plugin only; Agent Plugins v1 has no import mechanism, since `skills/` is a fixed local location and symlinks may not escape the package. Those clients can install the upstream skill directly instead — it is somebody else's repository, not ours to re-publish.

## Watch for wrapper skills

Some skills are thin wrappers that delegate to a sibling. `grill-me` is little more than "run a `/grilling` session", and referencing it without `grilling` gives you a skill that loads and then dead-ends.

The validator catches this: any `` `/name` `` a skill invokes must resolve to a skill in this plugin or to an entry in `sources.json`. When it fires, either add the sibling or drop the wrapper.

## Review checklist

1. Identify the source and confirm it is a project worth depending on.
2. Read the skill and any `scripts/` it ships. Scripts run on developer machines.
3. Check the license permits internal use. **No LICENSE file means no permission** — default copyright is all rights reserved, so an unlicensed repository is not adoptable however useful it looks.
4. Pin the exact commit.
5. Check what the skill invokes, and bring its siblings along.
6. Open a pull request.
