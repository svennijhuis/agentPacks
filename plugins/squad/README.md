# Squad

A capability pack for a user-invoked workflow. The main chat runs one playbook from the [squad-mode hub](skills/squad-mode/SKILL.md). It spawns a fresh delegate for each step. `/squad` and `/squad-review` are the entrypoints. `/squad split <request>` only writes a task plan. Sibling slash `/scenarios` writes smoke markdown from changed code. It is not a Squad phase.

## Agents

Four agent files. Roles that are not their own file are a role line on `squad-agent`.

| Component | Name | Responsibility |
|---|---|---|
| Skill | `squad-mode` | Hub. Playbooks, review loop, handoff, verification contract |
| Skill | `scenarios-md` | Changed code to `docs/smoke/<slug>.md` |
| Skill | `verify-harness` | Writes `docs/verify.md` (Launch, Doctor, Drive, Evidence, Cleanup) |
| Agent | `squad-agent` | One step. Role line: planner, architect, implementer, verifier, or simplifier |
| Agent | `squad-reviewer` | Correctness and plan or spec. Frontier model |
| Agent | `squad-security-reviewer` | Trust-boundary diffs against [OWASP Top 10:2025](https://owasp.org/Top10/) |
| Agent | `squad-retro` | One retro, then the strongest guard for the lesson |
| Command | `squad` | Routes to the hub. Copilot picker: `/squad:run` |
| Command | `squad-review` | Review playbook. No fix round |
| Command | `scenarios` | Writes only `docs/smoke/<slug>.md`. Copilot picker: `/squad:scenarios` |

All four agents remain portable across supported generated clients.

Default model tier is `inherit` on the client map. `squad-agent` is `standard`. The architect role is spawned at `frontier`. `squad-reviewer` is `frontier`. No role uses `fast`.

## Session split

Work that does not fit one session is not built. See [session split](skills/squad-mode/references/session-split.md). `/squad split <request>` forces that split. A later `/squad` does one task and stops.

## Handoff

After PASS, one normal push and a pull request. Never force push. Never merge. The body is [handoff](skills/squad-mode/references/handoff.md).

## Provider support

| | Skill | Glob-scoped rule | Agents | Command |
|---|---|---|---|---|
| Claude | yes | no | yes | yes |
| Cursor | yes | yes | yes | yes |
| GitHub Copilot | yes | no | yes | `/squad:run` |
| Codex | yes | no | manual copy | yes |

Codex agent files are generated for manual copy:

```shell
cp plugins/squad/com.openai.codex/agents/*.toml .codex/agents/
```

## Provider notes

Cursor is the only client that loads `rules/review-checklist.mdc` by glob. Generation emits the documented portability warning for the other clients instead of making the checklist always-on.

## Editing

Authored: `plugin.json`, `NOTICE.md`, `mcp.json` (empty scaffold), `skills/`, `rules/`, `agents/`, and `commands/`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client manifests and `com.*` provider trees.

The agent reads files from the plugin dirs, never by search: `plugins/squad/skills/squad-mode/SKILL.md`, `plugins/squad/skills/squad-mode/references/planning-contract.md`, `plugins/pack-check/skills/pack-check/scripts/check-plan.sh` (or `check-plan.ps1`); no find/glob search.
