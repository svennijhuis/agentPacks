# Add an MCP server

Each plugin may declare portable MCP configuration in `plugins/<plugin>/mcp.json`. The file lives at the plugin root, and the specification forbids declaring MCP anywhere else — not inline in `plugin.json`, not under an alternative path.

An empty scaffold is valid:

```json
{
  "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
  "mcpServers": {}
}
```

Replace or extend `mcpServers` when there is a real server. The Claude generator deliberately emits no `.mcp.json` for an empty object.

## Document shape

```json
{
  "$schema": "https://agent-plugins.org/schemas/1.0.0/mcp.schema.json",
  "mcpServers": {
    "architecture-search": {
      "type": "streamable-http",
      "url": "https://mcp.example.com/architecture",
      "headers": { "X-Tenant": "platform" }
    }
  }
}
```

The `$schema` must match the specification version declared in `plugin.json`.

## Transports

| Type | Required | Notes |
|---|---|---|
| `stdio` | `type`, `command` | Optional `args`, `env`, `cwd`. |
| `streamable-http` | `type`, `url` | The current remote transport. Prefer this. |
| `sse` | `type`, `url` | Deprecated HTTP+SSE. Client support is optional, so the validator warns. |

## Rules the validator enforces

**stdio**

- `command` is one executable token: a bare name resolved by platform search rules, or a plugin-relative path starting with `./`. It is not a shell command — put arguments in `args`.
- No placeholder expansion applies to `command`.
- `cwd` defaults to the plugin root. An explicit `cwd` must be plugin-relative or rooted at `${PLUGIN_ROOT}` / `${PLUGIN_DATA}`, and must stay inside it. `..` is rejected.
- `${PLUGIN_ROOT}` and `${PLUGIN_DATA}` expand in `args`, `env` values and `cwd` only — never in environment keys, `command`, URLs or headers.
- `PLUGIN_ROOT` and `PLUGIN_DATA` cannot be overridden in `env`.

**Remote**

- Absolute `http` or `https` URL, with no user information and no fragment. Non-loopback endpoints must use HTTPS; `localhost`, `127.0.0.1` and `[::1]` may use plain HTTP.
- Header names must be valid HTTP field names and must not collide case-insensitively.

## No credentials

Configured headers and environment values are literal, visible package data. Agent Plugins 1.0.0 defines no portable OAuth or credential-reference fields; authentication is client-managed. The validator rejects keys that look credential-related (`token`, `secret`, `authorization`, `api-key`, and similar).

## Writing the server

Company MCP servers are .NET services, so use the official [MCP C# SDK](https://devblogs.microsoft.com/dotnet/announcing-v20-of-the-official-mcp-csharp-sdk/). Reference `ModelContextProtocol.AspNetCore` for an HTTP server or `ModelContextProtocol` for stdio with attribute-based tool discovery.

Prefer `streamable-http` here: v2.0 servers are stateless by default, with no `initialize` handshake or session header, so they scale horizontally behind ordinary HTTP infrastructure. A tool that needs input mid-execution returns `InputRequiredResult` rather than holding a session open.

## Start read-only

For the first phase, prefer lookups over writes: architecture search, coding standard search, service lookup, owner lookup. Add deployment or resource tooling later, and do not start with production write access.

## Generated Claude file

Publishing a server generates `plugins/<plugin>/.mcp.json` on the `marketplace` branch, which is what Claude loads. It never appears on `main` and is never edited manually.
