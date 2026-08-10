# ADR 0001 — Source of truth and generated files

Status: accepted

## Context

agentPacks distributes skills, agents and MCP configuration to several AI clients. Agent Plugins v1 is an open standard for the portable half — skills and MCP — and is supported at launch by ChatGPT/Codex, Cursor, GitHub Copilot, Kiro and VS Code. Claude Code uses its own marketplace catalog and its own MCP file name.

The obvious failure mode is duplication: one copy of every skill per client, drifting apart within a quarter.

## Decision

**The portable Agent Plugin is the only authored content.** `plugins/<plugin>/plugin.json`, `skills/`, `mcp.json` and the company `agents/` convention are what developers edit.

**Claude support is generated, not authored.** The .NET tooling emits `.claude-plugin/marketplace.json` and `plugins/<plugin>/.mcp.json` from that source. The catalog entry points at the real plugin directory, so skills and agents exist once.

**We author no other client-specific format.** No `.cursor-plugin/`, no `.codex-plugin/`, no reverse-domain extension directories. If a client feature requires its own packaging, we do without the feature.

**Automation owns the generated files.** Pull requests validate the source and prove generation into a temporary directory; they do not require the committed catalog to be current. A job on `main` regenerates and commits.

**The generated marketplace entry omits `version`,** so update detection falls back to the Git commit SHA.

## Consequences

- A skill is written once and reaches every client.
- Contributors need no Claude-specific knowledge to add a skill.
- Two files in the tree must never be hand-edited. This is enforced by `generate-claude --check` and a scheduled drift job, and stated in the README.
- Cursor and Codex ignore `agents/`. Accepted: skills are the portable, valuable half.
- If Claude's marketplace schema changes, one generator changes — no content moves.

## Alternatives rejected

**Author the Claude catalog by hand.** Cheap on day one, wrong by the second plugin, and it puts client-specific knowledge in every contributor's way.

**Duplicate skills per client.** Solves loading, guarantees drift.

**Adopt each client's native plugin format.** Maximum capability, maximum maintenance, and it abandons the portability that made the standard worth adopting.
