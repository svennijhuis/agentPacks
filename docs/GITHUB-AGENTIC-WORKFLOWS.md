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

## Later

When a team does automate, keep the agent thin there too. If a workflow needs knowledge that another team would also want, that knowledge belongs in a skill here, not in the workflow file.
