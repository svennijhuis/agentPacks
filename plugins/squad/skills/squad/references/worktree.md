# Worktree

Preserve the primary checkout and externally created worktrees. Remove a squad-created worktree
only when `git status --porcelain` is empty, using `git worktree remove <exact-path>` without `--force`.
Preserve dirty worktrees. Hand off uncommitted. Append one `docs/learnings.md` entry.
