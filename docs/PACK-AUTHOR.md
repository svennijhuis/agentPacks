# Pack author

The fifth user slash is intentional. `/pack-author` is Lane A for marketplace authors: make, review, or change a skill or an agent. It is not a Squad phase and not a pack-check mode.

| Identity | Name |
|---|---|
| Plugin | `pack-author` |
| Command | `pack-author` (Copilot file: `author`) |
| Skill | `pack-author-md` |

| Client | Type |
|---|---|
| Claude Code, Cursor | `/pack-author` |
| Codex | `$pack-author` |
| GitHub Copilot | `/pack-author:author` |

v1 scaffolds a skill or an agent. Commands, hooks, and rules stay on [ADD-CLIENT-EXTENSION.md](ADD-CLIENT-EXTENSION.md), [ADD-HOOK.md](ADD-HOOK.md), and [ADD-RULE.md](ADD-RULE.md). Review returns PASS/FAIL. Change applies at most two rounds, and only the FAIL items a human approved. Pack: [`plugins/pack-author/README.md`](../plugins/pack-author/README.md).

## Lane A

[`docs/CONTRIBUTE.md`](CONTRIBUTE.md) lists `/pack-author` with the locked user commands. Claude and Cursor type `/pack-author`. Copilot types `/pack-author:author`. Suggestions stay human-apply only. This pack does not auto-rewrite skills.
