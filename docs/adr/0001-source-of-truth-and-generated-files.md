# ADR 0001 — Source of truth and generated files

Status: accepted

## Context

agentPacks distributes skills and MCP configuration to several AI clients. Agent Plugins v1 is an open standard for the portable package and is supported at launch by ChatGPT/Codex, Cursor, GitHub Copilot, Kiro and VS Code. Claude Code uses its own marketplace catalog and its own MCP file name.

The obvious failure mode is duplication: one copy of every skill per client, drifting apart within a quarter.

## Decision

**The Agent Plugin directory is the only authored content.** `plugins/<plugin>/plugin.json`, `skills/`, `mcp.json` and namespaced client extensions are what developers edit.

**Rules, agents, commands and hooks are authored once in a neutral form and generated per client.** The specification defines none of the four, and the clients collide: Claude, Cursor and Codex all auto-discover `hooks/hooks.json` in three incompatible dialects. The authored form lives at the plugin root in Cursor's dialect — `rules/*.mdc`, `agents/*.md`, `commands/*.md`, plus a neutral `hooks.source.json` that no client discovers — and the tooling generates a tree per client. Cursor keeps the root because it is the one client with no documented path override; Claude is redirected by component paths in its marketplace entry and Codex by `.codex-plugin/plugin.json`.

**External skills are authored as plugin-local URL records and materialized by publication.** Each plugin owns `external-skills.json`, so provenance and destination are visible together. GitHub Actions fetches each pin into that plugin's `skills/` tree; contributors never copy upstream Markdown by hand.

**Claude support is generated, not authored.** The .NET tooling emits `.claude-plugin/marketplace.json` and `plugins/<plugin>/.mcp.json` from that source. The catalog entry points at the real plugin directory, so skills exist once.

**A hook names a script, never a command line.** The generator owns the invocation, so no authored file contains shell syntax, and the validator can require both a `.sh` and a `.ps1` — one generated hooks file is shared by macOS and Windows, and a script with one platform half is a hook that silently does nothing for half the team.

**Client-specific behavior uses the standard extension boundary.** Manifest data lives under `extensions`, keyed by a reverse-domain namespace, and client-owned files live in a matching top-level directory. The namespace owner defines those contents; unrelated clients ignore them.

**Automation owns a separate generated branch.** Pull requests validate `main`, which contains no Claude marketplace or materialized external skills. A job on `main` publishes the complete installable tree to `marketplace`.

**The generated marketplace entry omits `version`,** so update detection falls back to the Git commit SHA.

## Consequences

- A skill is written once and reaches every client.
- Contributors need no Claude-specific knowledge to add a skill.
- The generated marketplace must never be hand-edited. Drift checks run against the `marketplace` branch.
- Clients ignore extension namespaces they do not implement; extension contents therefore cannot be assumed portable.
- If Claude's marketplace schema changes, one generator changes — no content moves.
- Only what every client can express is authorable. The neutral hook vocabulary is an intersection, so a client-specific event is unavailable even to the clients that have it.
- Two capabilities cannot be generated at all and are documented as manual steps: Codex loads subagents only from `.codex/agents/`, and reads `AGENTS.md` from the workspace rather than from a plugin.

## Alternatives rejected

**Author the Claude catalog by hand.** Cheap on day one, wrong by the second plugin, and it puts client-specific knowledge in every contributor's way.

**Duplicate skills per client.** Solves loading, guarantees drift.

**Adopt each client's native plugin format.** Maximum capability, maximum maintenance, and it abandons the portability that made the standard worth adopting.
