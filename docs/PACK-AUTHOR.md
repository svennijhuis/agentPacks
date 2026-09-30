# Pack author

The fifth user slash is intentional. `/pack-author` is Lane A for marketplace authors: make, review, or change a skill or an agent. It is not a Squad phase and not a pack-check mode.

| Identity | Name |
|---|---|
| Plugin | `pack-author` |
| Command | `author` |
| Skill | `pack-author-md` |

| Client | Type |
|---|---|
| Claude Code, Cursor | `/author` |
| GitHub Copilot | `/pack-author:author` |
| Codex | `$author` |

v1 scaffolds a skill or an agent. Commands, hooks, and rules stay on [ADD-CLIENT-EXTENSION.md](ADD-CLIENT-EXTENSION.md), [ADD-HOOK.md](ADD-HOOK.md), and [ADD-RULE.md](ADD-RULE.md). Review returns PASS/FAIL. Change applies at most two rounds, and only the FAIL items a human approved. Pack: [`plugins/pack-author/README.md`](../plugins/pack-author/README.md).

## Reconcile `docs/CONTRIBUTE.md`

`docs/CONTRIBUTE.md` is not on `main` (open PR #63). When that guide lands, update it in the same change:

- Retire the absolute "No new slash" line. The locked user commands include `/pack-author`.
- Lane A links this note. Authors make, review, and change skills and agents from `/pack-author` (Copilot: `/pack-author:author`).
- Suggestions stay human-apply only. This pack does not auto-rewrite skills.
