# GitHub Agentic Workflows

Agentic Workflows stay in the application repositories. They do not belong here.

```
agentPacks
    └── versioned company plugin: skills, agents, MCP

application-repo
    └── .github/... agentic workflow
            └── consumes and pins a company capability
```

## Why the split

This repository is the capability source: what an agent knows how to do. An application repository decides *when* automation runs, on which events, with what permissions, against its own code.

Moving those triggers here would make one repository responsible for every team's automation policy, and would couple a skill change to unrelated production pipelines.

## How a product repository consumes agentPacks

Pin a specific commit of this repository rather than tracking `main`, for the same reason external sources are pinned: the capability a workflow runs should be the capability that was reviewed.

## Import standards in a workflow

GitHub Agentic Workflows can import plain Markdown from this repository with `imports:` entries. Pin a release tag (`vX.Y.Z`) or a full commit SHA, not a branch name.

Each language pack lists document paths in `plugins/<lang>/standards.source.json`. For review workflows, use the document paths referenced by the `<lang>-review` consumer in that file (one file per path, no index that re-imports others).

```yaml
imports:
  - svennijhuis/agentPacks/plugins/dotnet/standards/csharp.md@abc1234
  - svennijhuis/agentPacks/shared/standards/http-api.md@abc1234
```

Example consumer: [agentic-workflows `workflows/shared/standards/`](https://github.com/svennijhuis/agentic-workflows/tree/main/workflows/shared/standards).

## Later

When a team does automate, keep the agent thin there too. If a workflow needs knowledge that another team would also want, that knowledge belongs in a skill here, not in the workflow file.
