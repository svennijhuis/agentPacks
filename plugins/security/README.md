# security

A capability pack for a user-invoked full-repo security audit. `/security-audit` maps
trust boundaries, hunts in parallel, and validates every candidate with a fresh agent.

Per-change OWASP stays on Squad's `squad-security-reviewer`. This pack does not run
during `/squad` or `/squad-review`. Stack footguns stay in `<lang>-security-review`
when a language pack has real content.

The methodology is adapted from Cloudflare's
[security-audit-skill](https://github.com/cloudflare/security-audit-skill); see [NOTICE.md](NOTICE.md).
The wording, agents, and report shape are this repository's.

## Flow

```text
/security-audit
1. Recon — security-recon maps architecture, surfaces, in-scope classes
2. Hunt — security-hunter × N in parallel, one class each
3. Validate — security-validator × N in parallel, one candidate each (hunter ≠ validator)
4. Report — docs/security-audit/<slug>.md; IDE/CLI; no commit
```

## Components

| Component | Name | Responsibility |
|---|---|---|
| Skill | `security-audit-md` | User-invoked routing. Exact Skill-name loading. Not model-invoked. |
| Agent | `security-recon` | Architecture, trust model, surfaces, in-scope classes |
| Agent | `security-hunter` | One attack class; candidates with a concrete attack |
| Agent | `security-validator` | Disprove one candidate; confirmed / needs_validation / rejected |
| Command | `security-audit` | Runs the audit. Copilot picker: `/security:security-audit` |

All three agents are `fast`, `readonly`, and report-only.

## Editing

Authored: `plugin.json`, `NOTICE.md`, `skills/`, `agents/`, and `commands/`.

Generated only in validation output or on `marketplace` / `marketplace-beta`: client
manifests and `com.*` provider trees.

Test on a feature branch without merging to `main`:
[ADD-SKILL.md — Test a skill locally](../../docs/ADD-SKILL.md#test-a-skill-locally).
