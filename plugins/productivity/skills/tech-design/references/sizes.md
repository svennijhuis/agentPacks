# Sizes and types

Size sets the depth of a design. Types add required content. Neither changes the structure in [structure.md](structure.md).

## Pick the size

At chart, propose a size with the reasons, and let the human confirm. Take the largest size for which any line holds.

| Size | Any of these holds |
| --- | --- |
| `small` | One team. No new data store. No new external dependency. Easy to reverse. A pattern this codebase already uses. |
| `medium` | Two or more services or teams change. A new API or schema. Data crosses a trust boundary. A new external dependency. Options with real trade-offs. |
| `big` | Many teams or the whole organisation. A new platform or data store. Hard or impossible to reverse. Regulated or sensitive data. |

Size only goes up. When a session finds a `medium` or `big` line holds, raise `Size` in the header, log it, and add the sections and diagrams the new size requires as gaps.

## Depth per size

| | `small` | `medium` | `big` |
| --- | --- | --- | --- |
| `design.md` budget | 150 lines | 500 lines | 1200 lines |
| `n/a: <reason>` allowed in | Actors, Failure modes, Dependencies, Cross-cutting, Design subsections | Only Design subsections whose diagram trigger did not fire | Same as medium |
| Diagrams | Only those whose trigger fires | Context, plus those whose trigger fires | Medium, plus deployment and one failure sequence per main flow |
| Alternatives | "do nothing" and one other | "do nothing" and two others | Same, each with a cost |
| Rollout and rollback | One line | Steps to enable and to roll back | The six rollout questions below |
| Approvers | One | One per team that changes | One per team, plus security when data is sensitive |

A `design.md` over its budget means detail leaked in. Move discussion to gap rows and facts to `research/`. A `big` design still over budget is two designs. Stop, tell the human, and split the destination.

## Rollout questions for big

Answer each under `### Rollout and rollback`:

1. How is it enabled, and for whom first?
2. How is it disabled without a deploy?
3. What happens to data written while it was enabled, after a rollback?
4. What happens when it is enabled again after a rollback?
5. Which metric says roll back, and at what value?
6. What does each dependency outage do to this system?

## Types

The person may name types at chart. Each type adds requirements on top of the size.

| Type | Adds |
| --- | --- |
| `api` | Under `### Boundaries and contracts`: a table of every endpoint with method, path, auth level, request, response, and each error with its status. One sequence diagram per main flow, with the auth level on each arrow. Include an endpoint only when a source lists it or a gap decided it. |
| `integration` | A dependency contract row per outside system. One sequence diagram per main flow, with an error path. A failure mode row per outside call: timeout, bad response, duplicate, out of order. |
| `data` | An ER diagram. Under `### Data model`: owner, retention, and classification per entity. A migration plan with a way back under `### Rollout and rollback`. |
| `web` | A screen flow diagram. Under `### Flows`: each screen, its states (empty, loading, error, full), and who may see it. |

A design with no type still follows its size.
