---
name: pack-check
description: When checking whether required language-pack skills are present for the change.
license: UNLICENSED
---

# Pack check

Read [the bundled registry](references/packs.md) and [detect/resolve](references/detect.md), then
follow this sequence.

## Request approval

If any applicable required skill is missing and installation has not already been declined in this
session, ask exactly once before continuing. Group every applicable missing pack into that one
approval round; list each pack and only its actually missing skills. For one .NET pack the prompt is:

```text
I found a .NET project, but the dotnet language pack is not available.

Missing: dotnet-build, dotnet-test-patterns

May I install dotnet@agentpacks?
```

For Rust, substitute `Rust`, `rust`, `rust-build`, and `rust-test-patterns`. For TypeScript, substitute
`TypeScript`, `typescript`, `typescript-build`, and `typescript-test-patterns`. A refusal is session state: continue an ordinary
request from repository evidence and do not ask again in that session. For a full Squad run,
stop unless the user supplied `--no-pack`; the small-change gate may continue after reporting the
gap.

## Install after approval

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

## Plan lint

Lint a Squad plan with `sh scripts/check-plan.sh <plan>`, or `pwsh scripts/check-plan.ps1 <plan>` when
no `sh` exists: date-slug name, required sections for its `## Status`, no em dash. Local files only.
Findings are warnings: report them, never stop the run. Neither shell available: skip and say so.

Lint a Squad retro with `sh scripts/check-retro.sh <retro>`, or `pwsh scripts/check-retro.ps1 <retro>`:
date-slug name and no em dash (U+2014), reported per line. The main agent fixes every finding before Done.
