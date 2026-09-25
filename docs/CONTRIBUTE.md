# Contribute (org teams)

How company teams add or update plugins in **agentPacks**, and how **test teams** use Squad for coding and testing in product repos.

Audience: platform + product test teams. Maintainers still own catalog decisions in [`docs/PLAN.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/PLAN.md).

## Two lanes

| Lane | You work in | Goal |
|---|---|---|
| **A. Pack contribute** | `svennijhuis/agentPacks` | New/updated skill, standard, language pack, or capability pack |
| **B. App testing** | Your product repo | Unit / component / integration / smoke; `/scenarios` writes smoke md |

Do not invent a fifth user slash. Locked user commands: `/squad` · `/squad-review` · `/scenarios` · `/pack-check` (plus `/security-audit` on the security pack).

---

## Lane A — Contribute a plugin or update an existing one

### Decide where it belongs

1. Read [`docs/PLAN.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/PLAN.md). Frameworks do **not** earn their own plugin.
2. Pick the owner:
   - **Language pack** (`dotnet` / `rust` / `typescript`) if it needs a compiler and contracted slots.
   - **Role / capability pack** (`squad`, `git`, `security`, `pack-check`) if it is workflow, not a language.
3. Prefer **update an existing pack** over a new plugin. New plugins need catalog proof.

Deep how-tos (do not duplicate here):

| Change | Doc |
|---|---|
| Skill | [`docs/ADD-SKILL.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-SKILL.md) |
| Language pack / slots | [`docs/ADD-LANGUAGE-PACK.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-LANGUAGE-PACK.md) |
| Agent | [`docs/ADD-AGENT.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-AGENT.md) |
| Hook | [`docs/ADD-HOOK.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-HOOK.md) |
| Rule | [`docs/ADD-RULE.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-RULE.md) |
| MCP | [`docs/ADD-MCP.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-MCP.md) |
| Client extension | [`docs/ADD-CLIENT-EXTENSION.md`](https://github.com/svennijhuis/agentPacks/blob/main/docs/ADD-CLIENT-EXTENSION.md) |

### Language-pack slots (test-relevant)

Contracted names are the interface. Misspell = silent miss (build fails instead):

| Slot | Skill name | For testers / verifiers |
|---|---|---|
| Build | `<lang>-build` | Toolchain + build commands |
| Test | `<lang>-test-patterns` | How to write/run unit vs integration here |
| Review | `<lang>-review` | Optional language checklist |
| Security | `<lang>-security-review` | Optional ecosystem footguns |

Cross-language HTTP shape lives once: [`shared/standards/http-api.md`](https://github.com/svennijhuis/agentPacks/blob/main/shared/standards/http-api.md) (collection envelope, methods, errors). Packs reference it via `standards.source.json`; generation emits into skill `references/standards/` on marketplace trees.

### Local prove loop (no merge to `main`)

Author on a feature branch. Merge to `main` publishes `marketplace-beta`. Stable `marketplace` only via promote action.

```bash
git clone https://github.com/svennijhuis/agentPacks.git
cd agentPacks
# edit under plugins/<pack>/

dotnet run --project tools/AgentPacks.Cli -- validate
dotnet test tools/AgentPacks.Cli.Tests
dotnet test tools/AgentPacks.slnx
dotnet run --project tools/AgentPacks.Cli -- validate-all --out /tmp/agentpacks-marketplace
```

Install the **generated** tree (not authored `plugins/`):

```bash
claude --plugin-dir /tmp/agentpacks-marketplace/plugins/squad
ln -sfn /tmp/agentpacks-marketplace/plugins/squad ~/.cursor/plugins/local/squad
copilot plugin marketplace add /tmp/agentpacks-marketplace
copilot plugin install squad@agentpacks
```

Reload client. Smoke `/squad` or `/squad-review` once. For org beta: install `#marketplace-beta` (see README).

### PR bar

- Matt-tiny skills: thin `SKILL.md`, detail in `references/`.
- Description = short when-to-use only.
- Skill `name` = directory name; no periods in skill names.
- Loop slots: `metadata.audience: loop`, first body line exact Internal line, `user-invocable: false`.
- Coworker proof = real `dotnet test` / `validate` / `validate-all --out`, not phrase-locks in markdown.
- Suggestions (`docs/suggestions.md` in app repos) are human-apply only; no auto skill rewrite; no `/suggestions` slash.

### What test teams usually contribute (pack side)

| Need | Where |
|---|---|
| Better unit/integration examples | `<lang>-test-patterns/references/examples/` |
| Sharper testing standard | `plugins/<lang>/standards/testing.md` |
| Shared API shape | `shared/standards/http-api.md` (+ consumers) |
| Smoke matrix / scenarios wording | `plugins/squad/references/smoke-matrix.md`, `scenarios-md` |
| Pack-check discoverability | `pack-check` packs registry row |

Open a PR. Maintainer squash-merge. After merge: wait for beta publish if you test via marketplace-beta.

---

## Lane B — Test teams in product repos (coding + testing)

Install packs once (team marketplace or per-user). Prefer language pack for your stack + `squad` + `pack-check`. See README install blocks.

### Who runs what

| Role | Command | Output |
|---|---|---|
| Dev | `/squad` | Code change, gated implement/verify/review, uncommitted hand-off |
| Dev | `/squad-review` | Code/diff report (optional `docs/reviews/*.md`) |
| Office tester | `/scenarios` (Copilot: `/squad:scenarios`) | Only `docs/smoke/<slug>.md` in **app** workspace |
| Setup | `/pack-check` | Detect stack; ask before language pack install |

`/scenarios` does **not** edit product source, commit, or push. `/squad` may read smoke md later for test design; it does not write smoke for push.

### Test layers (match language standards)

Use the stack’s `<lang>-test-patterns` + `standards/testing.md`. Shared idea:

| Layer | Kind | Runs where | Typical proof |
|---|---|---|---|
| **Unit** | Fast, focused branching | In-process | `dotnet test` / `cargo test` / `vitest` filtered |
| **Component** | One module + light deps | In-process or narrow host | Same runner; fakes at boundary |
| **Integration** | Composition, routing, DB, wiring | Local host / `WebApplicationFactory` / testcontainers | Shared fixture lifetime; real provider when in-memory lies |
| **Smoke** | Deployed/API probe | **Deployed env** | Manual or CI against real URL; rows from `/scenarios` |

Smoke ≠ integration. Local host / factory = integration. Deployed URL + secrets in **app repo** = smoke ([`smoke-matrix.md`](https://github.com/svennijhuis/agentPacks/blob/main/plugins/squad/references/smoke-matrix.md)).

.NET reminders (from pack standard):

- Branching → unit; composition/routing/migrations/DB/wiring → integration.
- Share expensive hosts/containers at narrowest safe fixture lifetime (not per test).
- Prefer real provider when in-memory changes constraints.
- Do **not** unit-test that a markdown/skill/README contains a phrase.
- Inject time/randomness; no wall-clock sleeps as assertions.

### Identify scenarios (tester flow)

1. Install `squad`. Open the **product** repo (not agentPacks).
2. Run `/scenarios` after handlers/routes change (optional OpenAPI path only to fill gaps).
3. Skill seeds **changed code first** (controllers, routes, HTTP APIs, timer/cron).
4. Writes `docs/smoke/<slug>.md` with columns:

| # | Case | Kind | Request | Status | Expected | Why |
|---|---|---|---|---|---|---|

Kinds: `happy` / `edge` / `fail` / `auth` / `biz` / `nothing-breaks`.

Default fill per changed path: happy, edge, fail, biz, nothing-breaks. Auth/policy rows only when the ask or OpenAPI change is about auth.

5. Timer/cron: kinds `edge`/`fail` (did not run, ran twice, poison, partial batch). Do not fake HTTP for a non-HTTP trigger.
6. Secrets: placeholders only (`TOKEN_VALID`). Tester supplies real token in the env. Never invent hosts or cloud product names.
7. Optional: mention existing Bruno/Postman collection; do not generate one.

Matrix fail rules (happy → not 2xx; edge → 2xx on invalid or 5xx; fail → wrong 4xx; auth → not 401/403; timeout/5xx folded into edge/fail kinds in the md).

### Suggested day-to-day for a test team

1. **Setup** — marketplace (stable or beta) + `squad` + language pack + `/pack-check`.
2. **New feature with API** — pair with dev: `/squad` implements + unit/integration; tester runs `/scenarios` when handlers land; execute smoke table on deployed TST/ACC.
3. **Regression** — keep `docs/smoke/*.md` as living checklist; re-run `/scenarios` when routes change.
4. **Pack feedback** — if patterns are wrong for your stack, open Lane A PR (examples/standards), or leave `docs/suggestions.md` (human-apply) from a Squad run. Do not expect auto skill rewrite.
5. **Beta** — try `#marketplace-beta` before promote to stable.

### Coding with Squad (short)

`/squad <ask>` e.g. `add an integration test for the orders endpoint`. Loop: learnings → orient → grill/plan if needed → implement → verify → ≤2 fix rounds → uncommitted hand-off. Load language slots by exact Skill name (`dotnet-test-patterns`), never slash-prose.

`/squad-review` = report only; no fix loop.

---

## Plan: what org teams can do next

Ordered backlog for platform + test chapters (pick 1–2, not all at once):

1. **Ship this doc** as `docs/CONTRIBUTE.md` on agentPacks; link from README “Contributor” line.
2. **Test-chapter onboarding sheet** (1-pager): install + `/scenarios` + smoke matrix + where secrets live (app-repo only).
3. **Per-stack example PRs** in a sample app: unit + integration + one smoke md from `/scenarios` (dotnet first if that is the majority stack).
4. **Pack improvements from testers**: richer `*-test-patterns` examples; timer/cron smoke rows; HTTP envelope checks in review checklist.
5. **Team marketplace**: Cursor Teams Import from Repo on `marketplace` (or beta); Auto Refresh; document who promotes beta→stable.
6. **Out of scope for v1**: new slash, auto skill rewrite, Matt catalog dump, second skill pack, inventing cloud product names in scenarios.

---

## Quick links

- README install / beta: repo root
- Smoke matrix: `plugins/squad/references/smoke-matrix.md`
- Scenarios skill: `plugins/squad/skills/scenarios-md/SKILL.md`
- HTTP API standard: `shared/standards/http-api.md`
- Catalog rule: `docs/PLAN.md`
