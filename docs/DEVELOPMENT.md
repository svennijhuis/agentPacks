# Development

This document is for contributors maintaining agentPacks. The root README stays focused on choosing and installing plugins.

## Source layout

```text
plugins/<plugin>/plugin.json                manifest
plugins/<plugin>/skills/                    authored Markdown skills
plugins/<plugin>/external-skills.json       optional URL imports owned by this plugin
plugins/<plugin>/mcp.json                   optional portable MCP configuration
plugins/<plugin>/<reverse-domain>/          optional client-owned extension files
```

Agent Plugins v1 standardizes two portable component types: skills and MCP. Commands, hooks, agents, rules and LSP servers remain client-specific. Client-specific manifest data belongs under `extensions`; client-owned files belong in a matching reverse-domain directory.

Use these guides when changing content:

- [Add a skill](ADD-SKILL.md)
- [Add an MCP server](ADD-MCP.md)
- [Add a client extension](ADD-CLIENT-EXTENSION.md)
- [Import external skills](EXTERNAL-SKILLS.md)

## Generated distribution

GitHub Actions publishes these paths to the generated `distribution` branch:

```text
.claude-plugin/marketplace.json
plugins/*/.mcp.json
plugins/*/skills/<external-name>/
```

`main` contains no generated Claude marketplace and no copied external Markdown. Update the owning plugin's `external-skills.json`; publication materializes each exact commit into the installable branch.

## Validate locally

Install the .NET SDK pinned in `global.json`, then run:

```bash
dotnet test tools/Company.AI.Tooling.slnx -c Release
dotnet run --project tools/Company.AI.Tooling -- validate
dotnet run --project tools/Company.AI.Tooling -- validate-all
```

`validate` checks portable source. `validate-all` also builds and validates Claude compatibility in memory without writing generated output into `main`. Deliberate generation requires `generate-claude --out <directory>`.

The validator reports all findings in one run:

- `spec`: a client rejects the content.
- `spec(tolerated)`: clients can continue, but the repository does not accept it.
- `policy`: a repository rule.
- `warning`: non-blocking advice.

Validation fetches the canonical schemas from `agent-plugins.org`; this repository does not keep schema copies.

## Continuous integration

Pull requests test and validate the source without modifying the working tree. After merge, publication materializes external URL sources, generates Claude compatibility, validates the completed packages, and pushes the result to `distribution`.

The generated branch must not be edited manually. The scheduled drift workflow verifies both generated Claude files and materialized external skills.

See [the source-of-truth ADR](adr/0001-source-of-truth-and-generated-files.md), [GitHub Agentic Workflows](GITHUB-AGENTIC-WORKFLOWS.md), and [references](REFERENCES.md) for design context.
