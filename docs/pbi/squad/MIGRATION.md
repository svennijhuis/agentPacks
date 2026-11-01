# Squad v1 to v2 migration guide

For maintainers (which PBI moves what) and for users who already run the pack locally or in GitHub Agentic Workflows (gh-aw). Where things live after v2: [STRUCTURE.md](STRUCTURE.md). How they look: [SHAPES.md](SHAPES.md). Flows: [FLOWS.md](FLOWS.md). gh-aw side: [gh-aw/README.md](gh-aw/README.md).

## Old and new side by side

```mermaid
flowchart LR
  subgraph v1["v1 (main 6c59f49)"]
    c1["commands: squad, squad-review, scenarios, pack-check, pack-author, security-audit"]
    h1["skills/squad (hub + 11 references)"]
    a1["10 squad agents, no principle or standard refs"]
    s1["shared/standards: engineering, design, testing, http-api"]
    p1["external-skills.json: caveman only"]
    c1 --> h1 --> a1
    h1 --> s1
  end
  subgraph v2["v2"]
    c2["commands: same 6 + statechart"]
    h2["skills/squad-mode hub: Non-negotiables, Principles, Standards, Playbooks"]
    pb2["squad-mode/playbooks: 9, copy + adapt"]
    st2["standard-* leafs: 9"]
    pr2["principle-* leafs: 24 copied"]
    k2["copies: tdd, statechart-mode, arena, no-comments"]
    a2["10 squad agents: hub path, named leafs"]
    s2["shared/standards: kept, gh-aw import source"]
    g2["gh-aw: aw-closure.json + fixture workflows"]
    c2 --> h2 --> pb2
    h2 --> st2 --> s2
    h2 --> pr2
    pb2 --> k2
    h2 --> a2
    h2 --> g2
  end
  v1 -.migrate callers, then delete.-> v2
```

## Path table

| Kind | v1 path (main `6c59f49`) | v2 path | Action | PBI |
|---|---|---|---|---|
| command | `plugins/squad/commands/squad.md` | same | edit: load `squad-mode` | PS06 |
| command | `plugins/squad/commands/squad-review.md` | same | edit: load `squad-mode` | PS06 |
| command | `plugins/squad/commands/scenarios.md` | same | edit: restyled skill | PS16 |
| command | (none) | `plugins/squad/commands/statechart.md` | add | PS12 |
| command | `plugins/pack-check/commands/pack-check.md` | same | edit: load `pack-check-mode` | PS16 |
| command | `plugins/pack-author/commands/pack-author.md` | same | edit: playbook routing | PS01 |
| command | `plugins/security/commands/security-audit.md` | same | edit | PS18 |
| hub | `plugins/squad/skills/squad/SKILL.md` | `plugins/squad/skills/squad-mode/SKILL.md` | rename and restyle; old dir kept as a one-line pointer until GA07 | PS06, GA07 |
| contract | `plugins/squad/skills/squad/references/*.md (11 files)` | `plugins/squad/skills/squad-mode/references/*.md` | move | PS06 |
| contract | `plugins/squad/skills/squad/references/review-contract.md` | `plugins/squad/skills/squad-mode/references/review-contract.md + standard-review-contract` | split: words and verdicts to the leaf, flow stays | PS07 |
| contract | `plugins/squad/skills/squad/references/planning-contract.md` | `plugins/squad/skills/squad-mode/references/planning-contract.md` | edit: `Task type:` per task | PS09 |
| contract | `plugins/squad/skills/squad/references/adr.md, retro.md` | same names under squad-mode | edit: Matt weave | PS10 |
| playbook | (none) | `plugins/squad/skills/squad-mode/playbooks/*.md (9)` | add (copy + adapt) | PS09 |
| standard | (none, rules in chat and memory) | `plugins/squad/skills/standard-*/SKILL.md (9)` | add | PS07 |
| standard | `shared/standards/engineering.md` | same | edit: links to principle leafs; text kept in v2; trim later | PS04, GA07 |
| standard | `shared/standards/design.md, testing-principles.md, http-api.md` | same | keep; leafs link to them | PS07 |
| standard | `plugins/*/standards.source.json` | same | keep | none |
| skill | (none) | `plugins/squad/skills/tdd, statechart-mode, arena, no-comments` | add (copy + adapt) | PS04, PS12 |
| skill | `plugins/squad/skills/scenarios-mode/SKILL.md` | same | restyle; steps to a playbook | PS16 |
| skill | `plugins/squad/skills/verify-harness/SKILL.md` | same | restyle | PS16 |
| skill | `plugins/pack-check/skills/pack-check/SKILL.md` | `plugins/pack-check/skills/pack-check-mode/SKILL.md` | rename (name equals command today); pointer until GA07 | PS16, GA07 |
| skill | `plugins/pack-author/skills/pack-author-mode/SKILL.md` | same | restyle hub | PS01 |
| skill | `pack-author-mode/references/make.md` | `pack-author-mode/playbooks/write-skill.md + write-subagent.md` | split | PS01, PS02 |
| skill | `pack-author-mode/references/review.md` | `pack-author-mode/playbooks/check-skill.md + check-subagent.md` | split | PS01, PS02 |
| skill | `pack-author-mode/references/change.md` | `pack-author-mode/playbooks/change.md` | move | PS01 |
| skill | `pack-author-mode/references/style.md` | `pack-author-mode/references/skill-bars.md + subagent-bars.md` | rename and split; the "do not copy an external skill set" line becomes the copy rule | PS01, PS02 |
| skill | `plugins/security/skills/security-audit-mode/SKILL.md` | same | restyle | PS18 |
| skill | `plugins/{dotnet,rust,typescript}/skills/*` | same | restyle, drop boilerplate | PS17 |
| agent | `plugins/squad/agents/squad-*.md (10)` | same names | rewrite with write-subagent | PS15 |
| agent | `plugins/pack-author/agents/pack-author-reviewer.md` | same | rewrite | PS02 |
| agent | `plugins/security/agents/security-{recon,hunter,validator}.md` | same | restyle | PS18 |
| pins | `plugins/squad/external-skills.json (caveman, caveman-compress)` | same + pins for skills we use unchanged (blast-radius, correct, interrogate, teach, unslop, verification pairs, benchmark-checklist, in-your-own-words; Matt codebase-design, domain-modeling, research and chief-of-staff dependencies) | add pins | PS04, PS05 |
| skill | (none) | `plugins/squad/skills/principle-*/ (24) and pstack skills we change (architect, how, why, figure-it-out, technical-writing, show-me-your-work, arena, no-comments)` | add (copy + adapt) | PS04, PS13, PS14 |
| skill | (none) | `plugins/squad/skills/{code-review,improve-codebase-architecture,chief-of-staff}` | add (copy + adapt of Matt); diagnosing-bugs, pr, retro woven into playbooks | PS05, PS10, PS11 |
| pins | `plugins/typescript/external-skills.json` | same + typescript-best-practices | add pin (unchanged) | PS17 |
| fork | (fork files) | `fork leafs not in migrations/v2.json (standard-<topic>, principle-<title>, playbooks/<title>.md), references/custom-rules.md` | keep (fork-owned, never edited) | PS03, PS06, PS07 |
| licence | (none) | `plugins/squad/NOTICE, plugins/pack-author/NOTICE` | add | PS05 |
| validator | `SkillReferenceValidator (backticked `/name` only)` | + REF01 to REF06, HUB01 | add | PS03 |
| validator | `"six user commands" FAIL bar` | `seven (adds statechart)` | edit | PS12 |
| gh-aw | (none) | `tests/fixtures/gh-aw/*.md, plugins/*/aw-closure.json` | add | GA01 to GA04 |
| version | `plugin.json 0.x / 1.0.0` | `2.0.0 for moved plugins, tag v2.0.0` | bump | PS20 |

## What breaks from v1

| Area | What can break | How we catch it |
|---|---|---|
| gh-aw imports of `shared/standards` | A consumer workflow imports `shared/standards/*.md`; trimming or moving them breaks its prompt. | Never move or delete a `shared/standards` file in v2. Text stays until GA07, after the AW catalog pin moved. `StandardsGhAwImportContractTests` stays green in every PBI. |
| gh-aw `skills:` path | AW pins `plugins/squad/skills/squad@<sha>`. In v2 the hub is `squad-mode`. | Old pins keep working (they point at an old SHA). The bump to `@v2.0.0` changes path and the closure list in one PR (GA05, AW room). The v1 dir stays as a pointer until GA07. |
| gh-aw closure | The hub reads `../<name>/SKILL.md`; in gh-aw only skills listed in `skills:` exist. | Generator writes `aw-closure.json` per pack (GA01); the fixture workflow lists it and compiles strict (GA04). |
| Providers (Cursor, Claude Code, Copilot, Codex) | Renamed skill dirs change generated manifests and Codex TOML agents drift. | `ClientManifestTests`, `CursorCatalogContractTests`, `ComponentGenerationTests` updated in the same PR as the move; generated output diff checked per provider. |
| `external-skills.json` | New pins not closed under references; byte drift; licence missing. | `ExternalSourceTests`, REF01 closure, byte-compare test (PS04), NOTICE test (PS05). Caveman pins stay. |
| Tests and fixtures | `SquadContractTests`, `SquadSessionSplitTests`, `PackAuthorContractTests`, `PackCheckContractTests` match paths and headings. | Update in the same PR as the move; expected output changes only in paths, never behavior (PS06 DoD). |
| Links in READMEs and docs | Links to `skills/squad/` and `pack-check/skills/pack-check/` go stale. | REF01 plus a link check over `docs/` and READMEs in the PR that moves the file. |
| Pack-author bars | style.md says "do not copy an external skill set"; it would fail every copy. | PS01 replaces it with the copy rule and REF06 before any copy lands (PS04). |
| Command count | Validator fails on a seventh command. | PS12 changes the bar in the same PR that adds `/statechart`. |
| Copied pstack text | Cursor-only lines (model slugs, `/loop`, `poteto-agent`) break on other clients and in gh-aw. | Copy rule: removed or under `## Provider notes`; GA03 REF07 checks tools against the gh-aw bash allowlist. |

## Order in small steps

After every PBI main is green and all 7 commands still load (6 until PS12). No big bang: callers move first, the old path goes last.

1. **Writer and checks** (PS01, PS02, PS03). No file moves yet.
2. **Additive pins and copies** (PS04, PS05). Nothing removed; `shared/standards` keeps its text.
3. **Hub rename** (PS06). Add `squad-mode`, switch both commands, agents and tests to it; leave `skills/squad/SKILL.md` as a one-line pointer.
4. **Standards, CODING_STANDARDS, playbooks** (PS07, PS08, PS09). Additive files plus index lines.
5. **Weaves, review, statechart, verification, lessons** (PS10 to PS14). Edits inside our own files; `/statechart` added with the bar change.
6. **Agents** (PS15 a and b). Same names, so no caller changes.
7. **Helpers, language packs, security, roles** (PS16 to PS19). `pack-check-mode` with a pointer at the old dir.
8. **gh-aw** (GA01 to GA06). Closure files, provider check, fixtures, docs. AW room bumps its pin (GA05).
9. **Release** (PS20). 2.0.0 and tag `v2.0.0`.
10. **Delete old paths** (GA07). Remove the two pointer dirs and trim `engineering.md`, only after GA05 is merged.

## Every file on main

Every file under `plugins/` and `shared/standards/` at `6c59f49`, mapped. Group rows above say why.

| File on main `6c59f49` | New place | Action | PBI |
|---|---|---|---|
| `plugins/dotnet/README.md` | same | keep | none |
| `plugins/dotnet/external-skills.json` | same | keep | none |
| `plugins/dotnet/mcp.json` | same | keep | none |
| `plugins/dotnet/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/dotnet/skills/dotnet-build/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/dotnet/skills/dotnet-build/references/commands.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-build/references/examples/central-packages.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-build/references/examples/toolchain-version.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-review/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/dotnet/skills/dotnet-review/references/checklist.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-review/references/examples/culture.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-review/references/examples/fire-and-forget.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-solution/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/dotnet/skills/dotnet-test-patterns/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/dotnet/skills/dotnet-test-patterns/references/commands.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-test-patterns/references/examples/deployed-smoke.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-test-patterns/references/examples/http-integration.md` | same | keep | none |
| `plugins/dotnet/skills/dotnet-test-patterns/references/examples/xunit-unit.md` | same | keep | none |
| `plugins/dotnet/standards.source.json` | same | keep | none |
| `plugins/dotnet/standards/async-errors.md` | same | keep | none |
| `plugins/dotnet/standards/csharp.md` | same | keep | none |
| `plugins/dotnet/standards/layers.md` | same | keep | none |
| `plugins/dotnet/standards/testing.md` | same | keep | none |
| `plugins/git/README.md` | same | keep | none |
| `plugins/git/external-skills.json` | same | keep | none |
| `plugins/git/hooks.source.json` | same | keep | none |
| `plugins/git/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/git/scripts/git-guard.ps1` | same | keep | none |
| `plugins/git/scripts/git-guard.sh` | same | keep | none |
| `plugins/pack-author/README.md` | same | keep | none |
| `plugins/pack-author/agents/pack-author-reviewer.md` | same | keep | none |
| `plugins/pack-author/commands/pack-author.md` | same | keep | none |
| `plugins/pack-author/external-skills.json` | same | keep | none |
| `plugins/pack-author/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/pack-author/skills/pack-author-md/SKILL.md` | `plugins/pack-author/skills/pack-author-mode/SKILL.md` | move (old dir becomes pointer until GA07) | PS01 |
| `plugins/pack-author/skills/pack-author-md/references/change.md` | `plugins/pack-author/skills/pack-author-mode/playbooks/change.md` | move | PS01 |
| `plugins/pack-author/skills/pack-author-md/references/make.md` | `plugins/pack-author/skills/pack-author-mode/playbooks/write-skill.md + write-subagent.md` | split | PS01, PS02 |
| `plugins/pack-author/skills/pack-author-md/references/review.md` | `plugins/pack-author/skills/pack-author-mode/playbooks/check-skill.md + check-subagent.md` | split | PS01, PS02 |
| `plugins/pack-author/skills/pack-author-md/references/style.md` | `references/skill-bars.md + subagent-bars.md` | split | PS01, PS02 |
| `plugins/pack-check/README.md` | same | keep | none |
| `plugins/pack-check/commands/pack-check.md` | same | keep | none |
| `plugins/pack-check/external-skills.json` | same | keep | none |
| `plugins/pack-check/hooks.source.json` | same | keep | none |
| `plugins/pack-check/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/pack-check/scripts/pack-check-session.ps1` | same | keep | none |
| `plugins/pack-check/scripts/pack-check-session.sh` | same | keep | none |
| `plugins/pack-check/skills/pack-check/SKILL.md` | `plugins/pack-check/skills/pack-check-mode/SKILL.md` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/references/detect.md` | `plugins/pack-check/skills/pack-check-mode/references/detect.md` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/references/packs.md` | `plugins/pack-check/skills/pack-check-mode/references/packs.md` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/scripts/check-plan.ps1` | `plugins/pack-check/skills/pack-check-mode/scripts/check-plan.ps1` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/scripts/check-plan.sh` | `plugins/pack-check/skills/pack-check-mode/scripts/check-plan.sh` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/scripts/check-retro.ps1` | `plugins/pack-check/skills/pack-check-mode/scripts/check-retro.ps1` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/scripts/check-retro.sh` | `plugins/pack-check/skills/pack-check-mode/scripts/check-retro.sh` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/scripts/check-verify.ps1` | `plugins/pack-check/skills/pack-check-mode/scripts/check-verify.ps1` | move (pointer until GA07) | PS16 |
| `plugins/pack-check/skills/pack-check/scripts/check-verify.sh` | `plugins/pack-check/skills/pack-check-mode/scripts/check-verify.sh` | move (pointer until GA07) | PS16 |
| `plugins/rust/README.md` | same | keep | none |
| `plugins/rust/external-skills.json` | same | keep | none |
| `plugins/rust/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/rust/skills/rust-build/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/rust/skills/rust-build/references/commands.md` | same | keep | none |
| `plugins/rust/skills/rust-build/references/examples/toolchain-version.md` | same | keep | none |
| `plugins/rust/skills/rust-build/references/examples/workspace-crates.md` | same | keep | none |
| `plugins/rust/skills/rust-review/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/rust/skills/rust-review/references/checklist.md` | same | keep | none |
| `plugins/rust/skills/rust-test-patterns/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/rust/skills/rust-test-patterns/references/commands.md` | same | keep | none |
| `plugins/rust/skills/rust-test-patterns/references/examples/deployed-smoke.md` | same | keep | none |
| `plugins/rust/skills/rust-test-patterns/references/examples/integration.md` | same | keep | none |
| `plugins/rust/skills/rust-test-patterns/references/examples/unit.md` | same | keep | none |
| `plugins/rust/skills/tokio-tune-runtime/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/rust/skills/tokio-tune-runtime/references/advanced.md` | same | keep | none |
| `plugins/rust/skills/tokio-tune-runtime/references/principles.md` | same | keep | none |
| `plugins/rust/standards.source.json` | same | keep | none |
| `plugins/rust/standards/errors-concurrency.md` | same | keep | none |
| `plugins/rust/standards/rust.md` | same | keep | none |
| `plugins/rust/standards/testing.md` | same | keep | none |
| `plugins/security/NOTICE.md` | same | keep | none |
| `plugins/security/README.md` | same | keep | none |
| `plugins/security/agents/security-hunter.md` | same | keep | none |
| `plugins/security/agents/security-recon.md` | same | keep | none |
| `plugins/security/agents/security-validator.md` | same | keep | none |
| `plugins/security/commands/security-audit.md` | same | keep | none |
| `plugins/security/external-skills.json` | same | keep | none |
| `plugins/security/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/security/skills/security-audit-md/SKILL.md` | `plugins/security/skills/security-audit-mode/SKILL.md` | move (old dir becomes pointer until GA07) | PS16 |
| `plugins/security/skills/security-audit-md/references/attack-classes.md` | `plugins/security/skills/security-audit-mode/references/attack-classes.md` | move (old dir becomes pointer until GA07) | PS16 |
| `plugins/security/skills/security-audit-md/references/principles.md` | `plugins/security/skills/security-audit-mode/references/principles.md` | move (old dir becomes pointer until GA07) | PS16 |
| `plugins/security/skills/security-audit-md/references/report.md` | `plugins/security/skills/security-audit-mode/references/report.md` | move (old dir becomes pointer until GA07) | PS16 |
| `plugins/security/skills/security-audit-md/references/workflow.md` | `plugins/security/skills/security-audit-mode/references/workflow.md` | move (old dir becomes pointer until GA07) | PS16 |
| `plugins/squad/README.md` | same | keep | none |
| `plugins/squad/agents/squad-architect.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-implementer.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-indexer.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-orchestrator.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-planner.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-retro.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-reviewer.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-security-reviewer.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-simplifier.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/agents/squad-verifier.md` | same | rewrite (write-subagent) | PS15 |
| `plugins/squad/commands/scenarios.md` | same | edit: load new hub | PS06, PS16 |
| `plugins/squad/commands/squad-review.md` | same | edit: load new hub | PS06, PS16 |
| `plugins/squad/commands/squad.md` | same | edit: load new hub | PS06, PS16 |
| `plugins/squad/external-skills.json` | same | keep caveman pins, add pins for unchanged skills | PS04, PS05 |
| `plugins/squad/mcp.json` | same | keep | none |
| `plugins/squad/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/squad/references/smoke-matrix.md` | same | keep | none |
| `plugins/squad/rules/review-checklist.mdc` | same | keep | none |
| `plugins/squad/skills/scenarios-md/SKILL.md` | `plugins/squad/skills/scenarios-mode/SKILL.md` | move (old dir becomes pointer until GA07) | PS16 |
| `plugins/squad/skills/squad/SKILL.md` | `plugins/squad/skills/squad-mode/SKILL.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/adr.md` | `plugins/squad/skills/squad-mode/references/adr.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/advisor-lite.md` | `plugins/squad/skills/squad-mode/references/advisor-lite.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/architect-contract.md` | `plugins/squad/skills/squad-mode/references/architect-contract.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/malformed-reask.md` | `plugins/squad/skills/squad-mode/references/malformed-reask.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/planning-contract.md` | `plugins/squad/skills/squad-mode/references/planning-contract.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/residual-fixup.md` | `plugins/squad/skills/squad-mode/references/residual-fixup.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/retro.md` | `plugins/squad/skills/squad-mode/references/retro.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/review-contract.md` | `plugins/squad/skills/squad-mode/references/review-contract.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/session-split.md` | `plugins/squad/skills/squad-mode/references/session-split.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/suggestions.md` | `plugins/squad/skills/squad-mode/references/suggestions.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/squad/references/worktree.md` | `plugins/squad/skills/squad-mode/references/worktree.md` | move (old dir becomes pointer) | PS06 |
| `plugins/squad/skills/verify-harness/SKILL.md` | same | restyle to SHAPES | PS16 |
| `plugins/squad/skills/verify-harness/references/doctor-tags.md` | same | keep | none |
| `plugins/squad/skills/verify-harness/references/local-access.md` | same | keep | none |
| `plugins/squad/skills/verify-harness/references/process.md` | same | keep | none |
| `plugins/squad/skills/verify-harness/references/smoke-report.md` | same | keep | none |
| `plugins/typescript/README.md` | same | keep | none |
| `plugins/typescript/external-skills.json` | same | keep | none |
| `plugins/typescript/plugin.json` | same | version bump if files moved | PS20 |
| `plugins/typescript/skills/typescript-build/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/typescript/skills/typescript-build/references/commands.md` | same | keep | none |
| `plugins/typescript/skills/typescript-review/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/typescript/skills/typescript-review/references/checklist.md` | same | keep | none |
| `plugins/typescript/skills/typescript-test-patterns/SKILL.md` | same | restyle to SHAPES | PS17 |
| `plugins/typescript/skills/typescript-test-patterns/references/commands.md` | same | keep | none |
| `plugins/typescript/skills/typescript-test-patterns/references/examples/deployed-smoke.md` | same | keep | none |
| `plugins/typescript/skills/typescript-test-patterns/references/examples/integration.md` | same | keep | none |
| `plugins/typescript/skills/typescript-test-patterns/references/examples/unit.md` | same | keep | none |
| `plugins/typescript/standards.source.json` | same | keep | none |
| `plugins/typescript/standards/testing.md` | same | keep | none |
| `plugins/typescript/standards/typescript.md` | same | keep | none |
| `shared/standards/design.md` | same | keep | none |
| `shared/standards/engineering.md` | same | links added; trimmed in GA07 | PS04, GA07 |
| `shared/standards/http-api.md` | same | keep | none |
| `shared/standards/testing-principles.md` | same | keep | none |

## Fork path

Forks with their own standards and rules follow [FORKS.md](FORKS.md): step 0 inventory into `docs/pbi/squad/fork-inventory.md`, then each PBI in order, `migrate-check` after each.

| Case | Example | What to do |
|---|---|---|
| Own change in a file we replace | fork added 12 lines to `skills/squad/SKILL.md`; PS06 renames and rewrites it | Move your lines into the `squad-mode` hub index. Inventory row `done`. Never keep two hubs. |
| Own file on a path we use | fork has `skills/tdd/SKILL.md` of its own; PS04 adds our `tdd` copy there | Do not overwrite. Rename yours to a kebab title that says what it is (for example `tdd-contract-first`); never a fork name or location in the name, update its callers, inventory row `done`; then add ours. If callers are unclear: stop, row `open`. |
| Own rule clashes with one of our standards | fork rule "merge in batches" vs our `standard-squash-merge-one-by-one` | Fork wins inside the fork: put the rule in `custom-rules.md` and add `Overrides: standard-squash-merge-one-by-one` on its line. The hub reads the hub index after our index, so the override is explicit. Inventory row `done (override)`. |
| Own standard in `shared/standards/` | `shared/standards/api-versioning.md` | Keep the file. Add `standard-api-versioning` leaf that links it, plus an index line in the hub index (PS07). |
| Own entries in `standards.source.json` | extra `documents` key `acme` | Keep. PBIs edit only keys they name (PS17). |
| Own agent with our name prefix | `squad-deployer.md` | Keep. Not in any table, so `migrate-check` says `fork-owned`. If a future v3 adds the same name, it lands in this table with rename as the fix. |

## For users of v1

**Local (Cursor, Claude Code, Copilot CLI, Codex):** update the agentPacks marketplace to `v2.0.0` and reload. Commands keep their names; `/statechart` is new. A repo with `docs/squad-plan.md` from v1 keeps working (same format).

**gh-aw:** keep your current pin until you are ready. To move: set `imports:` and `skills:` to `@v2.0.0` (or its 40-hex SHA), change `skills/squad` to `skills/squad-mode`, and add every entry from `plugins/squad/aw-closure.json` to `skills:`. Then run `gh aw compile --strict`. Details: [gh-aw/README.md](gh-aw/README.md).
