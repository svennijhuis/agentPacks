# External skills

External skills are authored as pinned URL records. Contributors do not copy upstream Markdown and do not add Claude marketplace entries.

Add an entry to the plugin that owns the import, for example `plugins/engineering/external-skills.json`:

```json
{
  "name": "code-review",
  "description": "Two-axis review of a diff.",
  "repository": "https://github.com/mattpocock/skills",
  "path": "skills/engineering/code-review",
  "commit": "84fdeffd12f2ee307994d1eb6feb48173b6e0502",
  "license": "MIT"
}
```

The commit must be an exact 40-character SHA. `path` must identify a skill directory containing `SKILL.md`. The containing plugin directory is the destination; there is no separate `plugin` field.

## Publication

After a merge to `main`, GitHub Actions performs both generated steps:

1. `materialize-external` fetches each URL at its pinned commit and writes the real files beneath `plugins/<plugin>/skills/<name>/`.
2. `generate-claude` builds `.claude-plugin/marketplace.json` and any Claude MCP adapter from the completed portable plugin.

The workflow publishes these outputs to the generated `distribution` branch. A generated skill contains `.external-source.json`; `main` retains only the URL record beside its owning plugin. Removing a source entry removes its generated directory from the next distribution.

This staging step is necessary because Agent Plugins v1 discovers real immediate child directories beneath `skills/` and has no manifest field for URL imports. Once materialized, the external skill is part of the same portable package for Codex, Cursor, Copilot, Kiro, VS Code, and Claude.

## Review checklist

1. Confirm the repository and exact directory.
2. Review `SKILL.md`, scripts, references, and assets because they run or load on developer machines.
3. Confirm the license permits redistribution and internal use.
4. Pin an exact commit rather than a branch.
5. Merge the URL record; let publication generate client-facing files.
