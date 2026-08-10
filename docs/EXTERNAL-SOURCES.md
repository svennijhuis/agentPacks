# External sources

External skills and MCP servers are allowed only after review. Approved sources are recorded in `external/sources.json`.

The file must exist even when nothing is approved:

```json
{ "sources": [] }
```

An empty file is deliberate: it makes the policy discoverable instead of leaving newcomers to guess whether one exists.

## Import, not copy-paste

Nobody copies an external skill by hand. Record it in `external/sources.json` and run:

```bash
dotnet run --project tools/Company.AI.Tooling -- vendor
```

The tooling fetches the pinned commit with a sparse, blobless clone, writes the skill into `plugins/<plugin>/skills/<name>/`, and drops a `.vendored.json` marker recording the origin, the commit and a content hash. Bumping the pin is a one-line edit plus a re-run.

Vendored content **is** committed: that is what makes the skill portable to every client.

### Why vendoring rather than a reference

Agent Plugins v1 has no import mechanism. `skills/` is a fixed location, each skill must be a real directory whose `SKILL.md` "resolves to a regular file", and symlinks may not be used to escape the package. So a genuine reference is only possible in marketplaces that support git sources — Claude and Copilot can point a catalog entry at `{"source": "git-subdir", "url": ..., "path": ..., "sha": ...}` — while Cursor, Codex and Kiro cannot. Vendoring works everywhere, which is why it is the default here.

The two approaches are mutually exclusive per skill: vendoring it *and* importing it into the Claude catalog would load it twice.

### Drift

```bash
dotnet run --project tools/Company.AI.Tooling -- vendor --check
```

Runs offline and fails when a source was never vendored, the pinned commit no longer matches what was written, someone edited vendored content locally, or a vendored directory survives after its entry was removed. The scheduled drift workflow runs it.

Edits to vendored skills are rejected on purpose. Fix it upstream and bump the pin, or the next `vendor` silently discards the change.

## Required fields

```json
{
  "sources": [
    {
      "name": "pdf-processing",
      "repository": "https://github.com/example/skills",
      "path": "skills/pdf-processing",
      "commit": "0b8f4c2d9e1a6f3b7c5d2e8a4f6b1c3d5e7a9b0c",
      "license": "Apache-2.0",
      "owner": "platform-team"
    }
  ]
}
```

| Field | Why |
|---|---|
| `name` | What the skill is called. |
| `repository` | Where it came from. |
| `path` | Which part of that repository we use. |
| `commit` | An exact 40-character Git commit SHA. |
| `license` | Reviewed before adoption. |
| `owner` | A named internal owner, not a team-shaped shrug. |
| `plugin` | Optional. Which plugin to vendor into. Required once the repository holds more than one. |

## Never track a branch

`main` is not a version. The validator rejects anything that is not a full commit SHA, because a moving reference means the content a developer runs today is not the content that was reviewed.

Updating an external source is a deliberate act: bump the SHA in a pull request, re-review the diff, and merge.

## Review checklist

1. Identify the source and confirm it is a project we are willing to depend on.
2. Read the skill body and any `scripts/` it ships. Scripts run on developer machines.
3. Check the license is compatible with internal use.
4. Pin the exact commit.
5. Name an owner who will handle updates.
6. Open a pull request.
