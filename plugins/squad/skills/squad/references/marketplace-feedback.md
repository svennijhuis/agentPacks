# Marketplace feedback

An append-only, sendable local report when a **plugin** skill, agent, pack, or
command failed to load or produced an unusable report. It lives at
`docs/agentpacks-feedback.md` in the repository being changed.

It is not a second brain. It does not rewrite skills. It is not product-test
noise. There is no server and no prompt. People send the file when they want.

Create the file with the title and send-blurb below if it does not exist, then
append the entry.

```markdown
# AgentPacks marketplace feedback

Local and gitignored. Open a GitHub issue at
https://github.com/svennijhuis/agentPacks/issues/new?template=marketplace-feedback.md
and paste one or more entries. Do not include source code, secrets, or
repository-private paths.
```

## When to write

The main agent appends exactly one entry after hand-off, with no ask, when any
of these happened this run:

| Failure class | When |
|---|---|
| `skill-miss` | Exact Skill tool name did not load |
| `agent-unusable` | An agent was missing, empty, or still malformed after the one re-ask |
| `pack-missing` | A required language pack or contracted slot was missing and blocked the loop |
| `contract-error` | Orchestrator input-error after the accept marker was already supplied |
| `command-miss` | `/squad`, `/squad-review`, or `/scenarios` did not run as documented |

Never write for a clean `pass`. Never write because product tests failed.
Never write because the user stopped the run. Never edit or delete an earlier
entry. Never auto-generate a skill from this file.

## Entry shape

```markdown
## 2026-09-09 — /squad

- Entrypoint: squad
- Provider: cursor
- Model tier: inherit
- Plugin: squad
- Component: squad-reviewer (agent)
- Failure class: agent-unusable
- What happened: reviewer returned no severity after one re-ask
- Marketplace tip: require severity in the reviewer Good/Bad pair
```

| Field | Rule |
|---|---|
| Heading | Date (`YYYY-MM-DD`) and the entrypoint (`/squad` or `/squad-review`) |
| Entrypoint | `squad` or `squad-review` |
| Provider | The client that ran: `claude`, `cursor`, `copilot`, or `codex` |
| Model tier | `inherit`, `fast`, `standard`, or `frontier` |
| Plugin | Exact plugin name (`squad`, `pack-check`, `git`, `dotnet`, `rust`, `typescript`) |
| Component | Exact skill or agent name plus kind in parentheses, or `None` |
| Failure class | Exactly one of the table values above |
| What happened | One line, no source, no secrets, no repo-private paths |
| Marketplace tip | One concrete catalog or prompt fix, or `None` |

Same local-gitignore rule as `docs/learnings.md`: list this path in
`.git/info/exclude`. Do not edit the committed `.gitignore`. Do not ask.
