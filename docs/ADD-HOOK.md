# Add a hook

Hooks run a command at a lifecycle point the user did not trigger. Every client supports them, none agree on the format, and the Agent Plugins specification leaves them out on purpose. So a hook is authored once in a neutral manifest and generated into all four dialects.

## Steps

1. Write the script as a pair: `plugins/<plugin>/scripts/<name>.sh` and `plugins/<plugin>/scripts/<name>.ps1`. Both are required.
2. Declare the hook in `plugins/<plugin>/hooks.source.json`.
3. Run `dotnet run --project tools/AgentPacks.Cli -- validate`.
4. Open a pull request.

Never create `hooks/hooks.json` by hand. Cursor owns that default path; generated marketplace and
manifest component paths route Claude, Codex and Copilot to their own dialects instead.

## The manifest

```json
{
  "$schema": "../../schema/hooks.schema.json",
  "hooks": {
    "beforeShellExecution": [
      { "script": "review-guard", "matcher": "git +push|rm +-rf" }
    ]
  }
}
```

| Field | Required | Rule |
|---|---|---|
| `script` | yes | Basename of a pair in `scripts/`. Kebab-case. Never a path, never a command line — the generator owns the invocation. |
| `matcher` | no | Regular expression narrowing when the hook fires. Not allowed on events with nothing to match against. |
| `timeout` | no | Seconds, 1 to 600. |

## Events

Only events every client can express are allowed. Anything else would work on some clients and silently do nothing on the rest.

| Neutral | Claude | Cursor | Codex | Copilot |
|---|---|---|---|---|
| `sessionStart` | `SessionStart` | `sessionStart` | `SessionStart` | `SessionStart` |
| `sessionEnd` | `SessionEnd` | `sessionEnd` | `SessionEnd` | `SessionEnd` |
| `userPromptSubmit` | `UserPromptSubmit` | `beforeSubmitPrompt` | `UserPromptSubmit` | `UserPromptSubmit` |
| `stop` | `Stop` | `stop` | `Stop` | `Stop` |
| `preToolUse` | `PreToolUse` | `preToolUse` | `PreToolUse` | `PreToolUse` |
| `postToolUse` | `PostToolUse` | `postToolUse` | `PostToolUse` | `PostToolUse` |
| `beforeShellExecution` | `PreToolUse` + `Bash` | `beforeShellExecution` | `PreToolUse` + `Bash` | `PreToolUse` + `Bash` |
| `afterFileEdit` | `PostToolUse` + `Write\|Edit` | `afterFileEdit` | `PostToolUse` + `apply_patch` | `PostToolUse` + `Write\|Edit` |

Matchers follow the vocabulary of the emitted event. Copilot's PascalCase `PreToolUse` and
`PostToolUse` events use the Claude-compatible aliases `Bash` and `Write|Edit`. Its `bash` and
`powershell` fields are command fields, not matcher names. Do not mix camelCase runtime tool names
such as `str_replace_editor` into a PascalCase event matcher.

## Document shapes

Three clients are not enough alike to share one writer:

| | Claude | Cursor | Codex | Copilot |
|---|---|---|---|---|
| Entry placement | matcher group | flat | matcher group | flat |
| POSIX command key | `command` | `command` | `command` | `bash` |
| Windows command key | — | — | `commandWindows` | `powershell` |
| Timeout key | `timeout` | `timeout` | `timeout` | `timeoutSec` |
| Format version | — | — | — | `"version": 1` |

These live in `ClientProfile`, so a client is a row rather than a branch in the generator.

## Where the matcher ends up

A client matcher filters exactly one subject, and the four clients disagree on which.

For `preToolUse` and `postToolUse` the subject is the tool name, which is what an authored matcher means there, so every client applies it natively.

For `beforeShellExecution` the subject is the command text. Only Cursor has an event whose matcher reads command text; Claude, Codex and Copilot spend their matcher naming the shell tool. There the generator passes the regex to the script as `-Matcher "<regex>"` and the script applies it. The same authored regex decides the outcome everywhere; only who evaluates it changes.

This is why a matcher may not contain `"`, `\`, `$` or `` ` ``: it is emitted verbatim inside a double-quoted shell argument, and a POSIX shell and PowerShell quote those differently. Write `[0-9]` rather than `\d`.

## Writing the script pair

Both halves receive the client's hook event JSON on **stdin** and are invoked with the same arguments. Keep them behaviourally identical — they are one hook, and a developer on the other operating system will not notice a divergence until it matters.

Clients disagree on hook payloads and failure behavior. Blocking hooks must have provider payload
fixtures and must be checked against each provider's current primary documentation. Never log the
raw command payload; shell arguments can contain credentials.

The generated hook command is the **extensionless** path `scripts/<name>`, because Claude and Cursor have no per-OS hook field and one string has to work on both platforms. Nothing authored sits at that path, so the generator writes both halves of it: a POSIX dispatcher `scripts/<name>` that execs your `.sh`, and a `scripts/<name>.cmd` shim that calls your `.ps1`. `cmd.exe` never runs an extensionless file — it appends `PATHEXT` — so each platform picks up its own half. Codex and Copilot both have a real per-OS field and skip the shim, spelled differently: Codex's `commandWindows` takes a full shell invocation, and Copilot's `powershell` is already a PowerShell context and takes `& "<script>.ps1"`.

The matcher argument is spelled `-Matcher`, with one dash and a capital. That is the only spelling both parsers accept: PowerShell binds it to `param($Matcher)` and has no double-dash parameter names, so `--matcher` would be swallowed as the parameter's value and the regex behind it would fail to bind at all.

## What gets generated

| Path | For |
|---|---|
| `hooks/hooks.json` | Cursor — flat entries, camelCase events |
| `com.anthropic.claude-code/hooks/hooks.json` | Claude — nested entries, PascalCase events |
| `com.openai.codex/hooks/hooks.json` | Codex — nested, plus `commandWindows` |
| `com.github.copilot/hooks/hooks.json` | Copilot |
| `scripts/<name>` | The POSIX dispatcher for the pair |
| `scripts/<name>.cmd` | The Windows shim for the pair |
