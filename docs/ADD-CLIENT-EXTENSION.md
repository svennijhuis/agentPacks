# Add a client extension

Agent Plugins lets a client own behavior outside the portable skills and MCP components without creating an unrelated package format.

Use a reverse-domain namespace in the manifest when the client needs manifest data:

```json
{
  "extensions": {
    "com.example.client": {
      "setting": true
    }
  }
}
```

Put client-owned files in a top-level directory with exactly the same namespace:

```text
plugins/code-review/
├── plugin.json
└── com.example.client/
    └── hooks/
        └── hooks.json
```

A client can use the manifest object, the directory, or both. Agent Plugins assigns no meaning or schema to the contents. The namespace owner must document the supported fields, file layout, validation and failure behavior; other clients ignore the extension.

## How this repository uses the boundary

Rules, agents, commands and hooks are not portable components — the specification leaves all four out. This repository authors them once in a neutral form at the plugin root and generates a tree per client inside that client's namespace directory:

```text
plugins/code-review/
├── plugin.json                     # declares the namespaces under "extensions"
├── rules/  agents/  commands/      # authored, and read directly by Cursor
├── hooks.source.json               # authored, neutral
├── com.anthropic.claude-code/      # generated
├── com.openai.codex/               # generated
└── com.github.copilot/             # generated
```

Cursor keeps the plugin root because it is the one client with no documented way to be pointed elsewhere. Claude is redirected by component paths in its marketplace entry, and Codex by `.codex-plugin/plugin.json`. See [ADD-HOOK.md](ADD-HOOK.md), [ADD-AGENT.md](ADD-AGENT.md) and [ADD-RULE.md](ADD-RULE.md).

Never edit a namespace directory by hand. They are generated, and `drift.yml` fails when they stop matching the source.
