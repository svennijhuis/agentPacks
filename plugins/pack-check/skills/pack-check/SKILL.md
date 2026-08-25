---
name: pack-check
description: Check whether the stacks applicable to a repository change have their required language-pack skills. Use at session start, before delivery-loop planning, or when asked about pack readiness; request approval before installing and stop for a reload after success.
---

# Pack check

Read [the bundled registry](references/packs.md), then follow this sequence.

## 1. Detect

Search the repository for every registered marker, skipping `.git`, `bin`, `obj`, `target`,
`node_modules`, `vendor`, and other evidenced build or dependency directories. Record the first
matching path for each stack; do not stop after the first stack. With no match, continue the original
request without pack output. When explicitly invoked as `/pack-check`, return `Stack: none detected`.

For a coding request, select the applicable stacks from the request's target paths, existing diff,
and acceptance criteria. A Rust-only scope selects `rust`; a .NET-only scope selects `dotnet`; a
cross-language scope selects both. When a mixed repository's scope cannot safely distinguish them,
select both. Session-start detection and an explicit `/pack-check` report every detected candidate,
because no change scope exists yet.

## 2. Resolve

For each applicable `<lang>`, attempt to resolve both required skills by exact name:

- `<lang>-build`
- `<lang>-test-patterns`

An unresolved skill is missing. Use this behavioral signal across all clients; do not call a
provider-specific skill-listing tool. Treat `<lang>-review` and `<lang>-security-review` as optional:
report their absence on one line and continue.

Never resolve or request installation for a detected stack outside the current change's scope. When
all required skills resolve, continue silently. For an explicit `/pack-check`, return one block per
detected stack:

```text
Stack: dotnet (found <marker>)
Pack:  dotnet (installed)
```

## 3. Request approval

If any applicable required skill is missing and installation has not already been declined in this
session, ask exactly once before continuing. Group every applicable missing pack into that one
approval round; list each pack and only its actually missing skills. For one .NET pack the prompt is:

```text
I found a .NET project, but the dotnet language pack is not available.

Missing: dotnet-build, dotnet-test-patterns

May I install dotnet@agentpacks?
```

For Rust, substitute `Rust`, `rust`, `rust-build`, and `rust-test-patterns`. A refusal is session state: continue an ordinary
request from repository evidence and do not ask again in that session. For a full delivery loop,
stop unless the user supplied `--no-pack`; the small-change gate may continue after reporting the
gap.

## 4. Install after approval

For every approved missing pack, identify the current client from the host context and take only its action:

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
