# pack-check

A small capability pack that answers one question at session start: does this repository have the
language pack its stack needs?

The bundled registry maps `.slnx`, `.sln`, and `.csproj` files to `dotnet`, and `Cargo.toml` to
`rust`. Detection records every stack, then the coding request's target paths, diff, and acceptance
criteria select which packs apply. Rust-only work does not load .NET; .NET-only work does not load
Rust; a cross-language or unresolved mixed scope loads both.

When a required `<lang>-build` or `<lang>-test-patterns` skill cannot be resolved, the agent groups
every applicable missing pack into one approval round. It never installs silently, asks for an
unrelated detected stack, or prints a marketplace URL.

## Provider behavior

| Provider | After approval |
|---|---|
| Claude Code | Runs `claude plugin install <pack>@agentpacks --scope user` |
| Codex | Runs `codex plugin add <pack>@agentpacks` |
| GitHub Copilot CLI | Runs `copilot plugin install <pack>@agentpacks` |
| Cursor | Waits while the user installs the selected pack from **Customize → agentpacks** |

Installed plugins become available after the client reloads or starts a new session. Declining the
install lets an ordinary coding request continue from repository evidence without another prompt in
that session. A full delivery loop stops unless `--no-pack` or its small-change gate applies.

Run `/pack-check` where commands are supported. In Codex, invoke `$pack-check` or ask naturally to
check the repository's language-pack setup.

## Editing this pack

Authored: `plugin.json`, `hooks.source.json`, `skills/`, `commands/`, and the paired scripts.

Generated on the marketplace branch or in temporary validation output: `hooks/`, script shims,
client manifests, and the `com.*` provider trees.
