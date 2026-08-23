# pack-check

A small capability pack that answers one question at session start: does this repository have the
language pack its stack needs?

The bundled registry currently maps `.slnx`, `.sln`, and `.csproj` files to the `dotnet` plugin.
When `dotnet-build` or `dotnet-test-patterns` cannot be resolved, the agent asks once before
installing `dotnet@agentpacks`. It never installs silently and never prints a marketplace URL.

## Provider behavior

| Provider | After approval |
|---|---|
| Claude Code | Runs `claude plugin install dotnet@agentpacks --scope user` |
| Codex | Runs `codex plugin add dotnet@agentpacks` |
| GitHub Copilot CLI | Runs `copilot plugin install dotnet@agentpacks` |
| Cursor | Waits while the user installs `dotnet` from **Customize → agentpacks** |

Installed plugins become available after the client reloads or starts a new session. Declining the
install lets an ordinary coding request continue from repository evidence without another prompt in
that session. A full delivery loop stops unless `--no-pack` or its small-change gate applies.

Run `/pack-check` where commands are supported. In Codex, invoke `$pack-check` or ask naturally to
check the repository's language-pack setup.

## Editing this pack

Authored: `plugin.json`, `hooks.source.json`, `skills/`, `commands/`, and the paired scripts.

Generated on the marketplace branch or in temporary validation output: `hooks/`, script shims,
client manifests, and the `com.*` provider trees.
