# dotnet

A language pack for C# and .NET. Today it holds exactly one thing: a hook that formats a C# file the
moment an agent writes it.

```
agent writes Foo.cs -> afterFileEdit -> dotnet format -> Foo.cs matches .editorconfig
```

An agent writes C# faster than it reads `.editorconfig`. Without this, formatting drift lands in the
diff, a reviewer spends a comment on indentation, and a fix round is paid for whitespace. Formatting
at the edit means the human only ever reads a formatted diff.

The hook is advisory in the strict sense: it writes at most one line and always exits 0. A formatter
that fails on half-written code must not stop the agent that is still writing it.

## What is in it

| Component | Name | What it does |
|---|---|---|
| Hook | `afterFileEdit` | Runs `dotnet format` on the `.cs`, `.csx` or `.vb` file just written |

## Depth

| `AGENTPACKS_DOTNET_FORMAT` | What runs | Cost |
|---|---|---|
| unset or `whitespace` | `dotnet format whitespace <dir> --folder --include <file>` | No restore, no build. Works on code that does not compile yet |
| `full` | `dotnet format <nearest .slnx/.sln/.csproj> --include <file>` | Style and analyzer fixes too — usings, `var`, naming. Needs a restore, so seconds to minutes |
| `off` | nothing | — |

Whitespace is the default because a hook on every edit has to be cheap, and because an agent
mid-change usually has a project that does not build.

The hook does nothing, silently, when `dotnet` is not on `PATH`, when the edited file no longer
exists, or when the path is not C#. A non-.NET repository never hears from it.

## What each client receives

| | Hook |
|---|---|
| **Claude** | yes |
| **Cursor** | yes |
| **Copilot** | yes |
| **Codex** | yes |

`afterFileEdit` is the one component type every client supports the same way. No client's
`afterFileEdit` matcher filters on the edited path — all four spend it naming the write tool — so the
authored regex is handed to the script, and the same regex decides the outcome everywhere.

## Not here yet

[`docs/PLAN.md`](../../docs/PLAN.md) reserves this pack for C# and .NET build, review, test and
security skills. Those come when they are written; the pack exists now because a hook is real content,
not a placeholder.
