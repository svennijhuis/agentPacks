# Worktree

Preserve the primary checkout and externally created worktrees. Remove a squad-created worktree
only when `git status --porcelain` is empty, using `git worktree remove <exact-path>` without `--force`.
Preserve dirty worktrees. Hand off uncommitted. Append one `docs/learnings.md` entry.
Zero or one `docs/suggestions.md` entry when this run evidenced a pack/skill/agent/command/contract change.
