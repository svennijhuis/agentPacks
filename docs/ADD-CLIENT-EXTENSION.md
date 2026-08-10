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
plugins/engineering/
├── plugin.json
└── com.example.client/
    └── hooks/
        └── hooks.json
```

A client can use the manifest object, the directory, or both. Agent Plugins assigns no meaning or schema to the contents. The namespace owner must document the supported fields, file layout, validation and failure behavior; other clients ignore the extension.

The `engineering` plugin's committed `com.example.client` tree is intentionally inert and demonstrates the portable extension boundary. Replace that example namespace and `{}` content only when integrating a real client whose extension contract is documented.
