# Dependency contract

A builder who has to connect to something must not find out on the day that nobody knows how. Every outside system gets a contract row under `## Dependencies` in `design.md`. An empty field is a gap that blocks ready.

An outside system is anything this codebase does not own: another team's service, a vendor API, a data source, a queue, an identity provider, a file drop.

## Fields

One row per outside system. Columns, in this order:

| Field | Write |
| --- | --- |
| System | Its name, as in `glossary.md` |
| Owner | The team or person who answers questions, and how fast they answer |
| Access | How a caller authenticates: the method, the scopes or roles, and where the secret lives |
| Credentials | Who issues them, how long that takes, and whether they are already issued for each environment |
| Environments | Which exist (test, staging, production), their addresses, and how the test one differs |
| Contract | The spec or document that defines requests, responses, and errors, as a link or a file in `input/` |
| Limits | Rate limits, quotas, payload size, timeouts, cost per call |
| Change notice | How this team learns that the system changes or goes down |
| Proof | The file in `input/` or `research/` that backs the row |

## Rules

- Add the row the moment the system is named. Do not wait for the design to reach it.
- For each empty field, add one gap. The question names the system and the field: "How does the billing service authenticate callers?"
- Pick the gap type by where the answer lives. Public docs answer it: `research`. Only the owner answers it: `grilling` with the human, or `waiting-on-input` when the human must ask the owner. Someone must request access: `task`.
- Proof is a spec, a document, or a written answer saved in `input/`. A line in chat is not proof.
- Make no live call to the system to find an answer.
- "Not needed" is an answer only with a reason: `none: public read-only endpoint`.

The table is done when every row has every field filled and a proof file.
