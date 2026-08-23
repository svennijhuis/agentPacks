---
name: pack-check
description: Check whether a detected repository stack has its required language-pack skills. Use at session start, before delivery-loop planning, or when asked about pack readiness; request approval before installing and stop for a reload after success.
---

# Pack check

Read [the bundled registry](references/packs.md), then follow this sequence.

## 1. Detect

Search the repository for every registered marker, skipping `.git`, `bin`, `obj`, and dependency
directories. Record the first matching path for each stack. With no match, continue the original
request without pack output. When explicitly invoked as `/pack-check`, return `Stack: none detected`.

## 2. Resolve

For each detected `<lang>`, attempt to resolve both required skills by exact name:

- `<lang>-build`
- `<lang>-test-patterns`

An unresolved skill is missing. Use this behavioral signal across all clients; do not call a
provider-specific skill-listing tool. Treat `<lang>-review` and `<lang>-security-review` as optional:
report their absence on one line and continue.

When both required skills resolve, continue silently. For an explicit `/pack-check`, return:

```text
Stack: dotnet (found <marker>)
Pack:  dotnet (installed)
```

## 3. Request approval

If either required skill is missing and installation has not already been declined in this session,
ask exactly once before continuing:

```text
I found a .NET project, but the dotnet language pack is not available.

Missing: dotnet-build, dotnet-test-patterns

May I install dotnet@agentpacks?
```

List only the skills that are actually missing. A refusal is session state: continue an ordinary
request from repository evidence and do not ask again in that session. For a full delivery loop,
stop unless the user supplied `--no-pack`; the small-change gate may continue after reporting the
gap.

## 4. Install after approval

Identify the current client from the host context and take only its action:

| Client | Approved action |
|---|---|
| Claude Code | Run `claude plugin install <pack>@agentpacks --scope user` |
| Codex | Run `codex plugin add <pack>@agentpacks` |
| GitHub Copilot CLI | Run `copilot plugin install <pack>@agentpacks` |
| Cursor | Direct the user to **Customize → agentpacks → &lt;pack&gt; → Install** and wait |

Run a CLI installer only after the user's explicit approval. Do not print or reconstruct a
marketplace URL. If the provider is unclear, ask which client is running instead of guessing a
command. Report an installer failure as-is; do not try another provider's command.

After a successful install, stop and ask the user to reload or start a new session. Plugin discovery
already happened, so do not claim the new skills are available in the current session.
