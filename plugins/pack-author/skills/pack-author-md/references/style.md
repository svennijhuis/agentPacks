# Style

Score against these principles and the hygiene table. This file is the checklist. It is not an upstream skill catalog.

## Predictability

The same kind of file keeps the same shape: frontmatter, one job, numbered steps, one Good/Bad pair. A reader can guess the next heading before opening it.

## Leading words

Start the line with the action or the constraint (`Read`, `Return`, `Stop`). Put the condition after the action when both belong on the line.

## Prune no-ops

Delete a step that does not change what the agent does. Delete a sentence that restates the line above it. Delete sediment: dates, version gossip, and reminders a client will not run.

## Progressive disclosure

`SKILL.md` routes. Operational detail lives in `references/` and is linked from the router, one level deep. Do not paste a reference back into the router.

Good:

```text
skills/example-helper/
+-- SKILL.md              router: when, steps, one link
+-- references/
    +-- detail.md         the procedure
```

Bad:

```text
skills/example-helper/
+-- SKILL.md              router, procedure, examples, and tables in one file
```

## Hygiene

| Bar | Rule |
|---|---|
| Description | Starts with `When `, ends with `.`, at most 120 characters |
| Router | At most 40 non-blank lines. Detail moves to `references/` |
| Name | Equals the directory (skill) or filename (agent). Kebab-case. No periods in skill names |
| Disclosure | Every `references/` file outside `standards/` and `examples/` is linked from the router |
| User entry | `disable-model-invocation: true`. Command name, skill name, and plugin name are three different strings |
| Loop slot | Contracted `<lang>-*` skills: `audience: loop`, first body line `Internal. Do not run directly — Squad loads by exact Skill name.`, `user-invocable: false` |
| Agent | One job. Numbered steps. One Good/Bad pair. Tools from `read`, `write`, `edit`, `grep`, `glob`, `bash`, `webfetch`, `websearch`. Model is `inherit`, `fast`, `standard`, or `frontier` |
| Catalog | A framework ships as a skill inside the language pack. Prefer a pack that already ships. Do not add an empty pack. See [PLAN.md](../../../../../docs/PLAN.md) |

## Out

Do not copy an external skill set into the pack. Do not auto-rewrite a skill from `docs/suggestions.md`.
