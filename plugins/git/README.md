# git

A capability pack with one job: block selected Git commands that can destroy local or remote work.
It is independent of the delivery loop and protects shell calls in any task.

## Blocked operations

| Rule | Operation | Risk |
|---|---|---|
| `GIT001` | `git reset --hard` | Discards uncommitted working-tree changes |
| `GIT002` | forced `git clean` (`-f`, `-fd`, `-xdf`, `--force`) | Permanently deletes untracked files |
| `GIT003` | forced push (`-f`, bundled `-uf`, `--force`, `--force-with-lease`) | Rewrites remote history |
| `GIT004` | forced branch deletion (`-D`, bundled `-df`, or `--delete --force`) | Can make unmerged commits unreachable |
| `GIT005` | forced checkout (`-f`, `--force`) and checkout over paths (`checkout .`, `checkout -- <path>`) | Overwrites working-tree changes |
| `GIT006` | restore of the working tree | Overwrites working-tree changes |

Staged-only restore (`git restore --staged <path>`) is allowed. Ordinary status, commit, merge, push,
branch deletion with `-d`, dry-run clean, and branch checkout are also allowed.

The guard recognizes Git after shell separators, inside command groups and substitutions
(`(git ...)`, `$(git ...)`, `{ git ...; }`), and after common global options such as `git -C <path>`
and `git -c name=value`. It classifies the Git subcommand and flags rather than relying on one
fragile whole-command regex, and it reads bundled short options, so `-uf` is the same force as `-f`.

A bare `git checkout <path>` with no `--` separator is **not** blocked: it is indistinguishable from
switching to a branch of that name without asking Git, and blocking branch checkout would make the
pack unusable.

## Four-provider hook contract

The authored event is `beforeShellExecution`. Generation translates it to:

| Provider | Generated event | Matcher | Command field read by the guard |
|---|---|---|---|
| Claude Code | `PreToolUse` | `Bash` | `tool_input.command` |
| Cursor | `beforeShellExecution` | command matcher | `command` |
| Codex | `PreToolUse` | `Bash` | `tool_input.command` |
| GitHub Copilot | `PreToolUse` | `bash\|powershell` | `tool_input.command` |

Exit code `2` is the denial signal. Claude, Cursor, and Copilot document that behavior for their
pre-execution hook; the repository tests the generated Codex shape and script contract, but the
runtime guarantee still depends on the installed Codex plugin host.

The Bash and PowerShell implementations have the same six stable rule ids, and both are executed by
the unit tests against the same commands so a divergence fails the build rather than waiting for a
Windows developer to hit it. The tests exercise both provider payload shapes, dangerous and safe
variants, bundled short options, grouped invocations, global Git options, compound shell commands,
and the no-secret-output rule. Where `pwsh` is unavailable the parity theory is skipped; CI has it.

## Privacy and failure behavior

The guard never echoes the command or raw hook payload; a shell command can contain credentials. A
block reports only a stable rule id and a generic reason. Unknown or malformed payload shapes are
allowed silently instead of scanning unrelated JSON fields and producing false positives.

The POSIX guard uses `jq` to parse hook JSON. If `jq` is unavailable, it allows the command. This is a
visible deployment prerequisite, not a claim of fail-closed security. Windows uses PowerShell's JSON
parser.

Disable the guard for an intentional operation with:

```shell
AGENTPACKS_GIT_GUARD=off
```

Anything else, including an unset value, keeps it enabled.

## Editing this pack

Authored: `plugin.json`, `hooks.source.json`, `scripts/git-guard.sh`, and
`scripts/git-guard.ps1`.

Generated on the `marketplace` branch or in temporary validation output, never on `main`: `hooks/`,
script dispatchers, client manifests, and `com.*` provider trees.

Keep both script implementations aligned. See [ADD-HOOK.md](../../docs/ADD-HOOK.md).
