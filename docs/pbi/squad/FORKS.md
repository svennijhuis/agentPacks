# Squad v2 for forks

Many teams forked agentPacks and added their own standards, rules, agents and playbooks. These PBIs must work for them too: apply them one by one and keep everything you added. Where things live: [STRUCTURE.md](STRUCTURE.md). How they look: [SHAPES.md](SHAPES.md). What moves: [MIGRATION.md](MIGRATION.md).

## Step 0: inventory (every PBI, before any edit)

```bash
BASE=6c59f49b   # the agentPacks commit your fork started from; find it with: git merge-base HEAD upstream/main
git diff --name-status "$BASE"...HEAD -- plugins shared docs .github > /tmp/fork-diff.txt
```

Write `docs/pbi/squad/fork-inventory.md` in your fork from that list. One row per file you added or changed: own standards, rules, skills, agents, `custom-rules.md`, `.github/*instructions*`.

```markdown
| Path in fork | Added or changed | Kind | v2 place | PBI | Status |
|---|---|---|---|---|---|
| plugins/squad/skills/squad/SKILL.md (lines 80 to 92) | changed | hub lines | squad-md hub index | PS06 | todo |
| shared/standards/acme-logging.md | added | standard | standard-acme-logging + index line; file stays | PS07 | todo |
| plugins/squad/skills/acme-deploy/SKILL.md | added | skill | same path, SHAPES skill shape | none | open: owner decides |
```

Status is `todo`, `done` or `open: <reason>`. When unsure, the file stays where it is and the row is `open`. Nothing in the inventory is deleted or overwritten.

## Five rules

1. **Find by role, not by path.** If your fork renamed a file, use the MIGRATION row's *kind* and the old v1 path to find your copy, then apply the row to it.
2. **Touch only what the PBI lists.** Section 3 of each PBI is the full list. A file that is not there is not changed, moved or deleted.
3. **Your lines live in the hub index.** Every hub has one block that v2 never edits:

```markdown
## Standards
- **No Em Dash.** Any prose. Leaf: standard-no-em-dash.
- **Acme Logging.** Any service code. Leaf: standard-acme-logging.
```

4. **Your names follow the poteto pattern: kind first, then a plain kebab name.** `standard-acme-logging`, `playbooks/acme-deploy.md`, agents `<pack>-<role>`. No `local-` prefix. `fork-inventory.md` lists every name you own, and REF03 fails when one of them equals a v2 name, so nothing collides silently.
5. **Run the keep check.** Before and after each PBI:

```bash
git rev-parse HEAD > /tmp/base.sha          # before
# apply the PBI
dotnet run --project tools/AgentPacks.Cli -- migrate-check --base "$(cat /tmp/base.sha)"
```

It reads `migrations/v2.json` (PS03) and prints one line per file: `kept`, `moved` (with the new path), `fork-owned` (not ours, untouched) or `LOST` (gone and not in the moves list). Any `LOST` line fails; restore that file.

## Where your additions go in v2

| You added in v1 | Put it in v2 | PBI that makes the slot |
|---|---|---|
| Lines in `skills/squad/SKILL.md` | hub index in `skills/squad-md/SKILL.md` | PS06 |
| A standard in chat, docs or `shared/standards/` | `skills/standard-<name>/SKILL.md` + index line in the hub index; `shared/standards/<file>.md` stays and the leaf links it | PS07 |
| Entries in `standards.source.json` (`documents`, `consumers`) | same file, kept; PBIs only edit keys they name | PS17 |
| Rules in `pack-author-md/references/style.md` | `## Local bars` in `references/skill-bars.md` | PS01 |
| Your own agents | unchanged; optional `check-subagent` to restyle | PS02, PS15 |
| Edits to our agents | a `## Local` section at the end of that agent | PS15 |
| Extra task types | `squad-md/playbooks/<type>.md` + line in hub index | PS09 |
| Extra packs in `pack-check` registry | `## Local packs` in `references/packs.md` | PS16 |
| Extra third-party credits | `## Local` in `NOTICE` | PS05 |
| Extra commands beyond our 7 | `## Local commands` in skill bars; the 7-command bar counts ours only | PS12 |

## Fixture fork

`tests/fixtures/fork-acme/` (added in PS03) is a tiny fork with one own standard (`standard-acme-logging`) and one own rule (`custom-rules.md`: "Log with the Acme logger"). After each PBI:

```bash
dotnet run --project tools/AgentPacks.Cli -- migrate-check --base <fixture base> --root tests/fixtures/fork-acme   # 0 LOST
dotnet run --project tools/AgentPacks.Cli -- validate --root tests/fixtures/fork-acme                              # 0 errors
for f in tests/fixtures/fork-acme/.github/workflows/*.md; do gh aw compile --strict "$f"; done                 # from GA04
```

`MigrateCheckTests` asserts both fork items are still present and listed in the fixture's `aw-closure.json`.

## gh-aw in a fork

Your fork's marketplace commit is what your workflows pin. The closure generator (GA01) includes fork `standard-<name>` leafs and fork playbooks it finds from the hub, so your additions are listed in `aw-closure.json` without edits.

## Handing these PBIs to a junior or an agent in your fork

Give them this folder plus one PBI file. Tell them: section 3 is the full list, section 5 the steps, section 8 the checks, section 10 your fork rules. Step 0 first. They stop and ask only when `migrate-check` prints `LOST` or a section 3 path does not exist in your fork and rule 1 does not find it.
