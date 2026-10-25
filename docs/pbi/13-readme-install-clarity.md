# PBI-13: README install clarity

Status: todo
Phase: C cloud agents and local polish
Depends on: 12
Gap(s): G14
proof: `test $(grep -cE '^\*\*(Copilot CLI|Codex|Claude Code|Cursor): (marketplace add|marketplace update)\*\*$' README.md) -eq 8`
door: two-way

## Goal

Clarify that `.cursor/install.sh` bootstraps agentPacks repo .NET tooling for Cursor Cloud dev, not end-user pack install. Document eight labeled marketplace blocks in README: one **marketplace add** and one **marketplace update** per client (Copilot CLI, Codex, Claude Code, Cursor).

## Context

`.cursor/install.sh:2-4,27` and `.cursor/environment.json` serve Cursor Cloud environment for this repo's builds. `README.md:24-104` lists marketplace adds and updates but blurs bootstrap vs consumer install and lacks eight explicit labeled add/update headings (two per client). Raw `marketplace add` grep counts are already high (beta/local variants included); the target is exactly eight labeled blocks, not a loose count.

## Steps

1. Add README subsection "Install vs dev bootstrap" pointing `.cursor/install.sh` at repo contributors only.
2. Add eight labeled copy-paste blocks: `**Copilot CLI: marketplace add**`, `**Copilot CLI: marketplace update**`, and the same pair for Codex, Claude Code, and Cursor (add uses `#marketplace` or `#vX.Y.Z` as today).
3. Mention gh-aw consumers use pins in workflow yaml, not install.sh.
4. Mention `agentpacks vendor` for cloud agent repos after PBI 11.
5. Keep commands token-free for public repo.

## DoD

- README calls out `.cursor/install.sh` purpose in one short paragraph.
- Same as `proof:`: README has exactly eight lines `**<client>: marketplace add**` or `**<client>: marketplace update**` (two each for Copilot CLI, Codex, Claude Code, Cursor).
- No instruction tells pack users to run `.cursor/install.sh` for squad install.
- Docs updated; no em or en dashes.

## Evidence

- README diff with line refs `README.md:24-104`.
- Reviewer checklist against packet G14.

## Flow

```mermaid
flowchart LR
  U[Pack user] --> M[marketplace add per client]
  D[agentPacks contributor] --> I[.cursor/install.sh .NET SDK]
  C[Cloud agent repo] --> V[agentpacks vendor]
```

## Named asks

None.

## Gist link

None.

## Sources

- Strategy PBI 13, gap G14.
- Packet section 0 install.sh evidence, section 6 G14.
- `.cursor/install.sh:2-4,27`, `README.md:24-104`.
