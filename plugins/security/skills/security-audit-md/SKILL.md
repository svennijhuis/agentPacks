---
name: security-audit-md
description: When the user types /security-audit or asks to audit a codebase for vulnerabilities.
license: MIT
disable-model-invocation: true
user-invocable: false
---

# Security audit

User-invoked. The command is the slash. Report only. Do not edit product code or commit.

Load with the Skill tool by exact name `security-audit-md`. Never write `/security-audit` as prose to load it.

Read [principles](references/principles.md), then [the workflow](references/workflow.md). Hunt from [attack classes](references/attack-classes.md). Emit [the report](references/report.md).

1. Resolve target (repo root) and scope (repo, path, or diff). Default profile `standard`; `quick` is one hunter wave.
2. Launch `security-recon` once. Hunters receive its architecture summary verbatim.
3. Launch `security-hunter` in parallel — one spawn per in-scope class from recon. Isolated; do not share hunter transcripts.
4. Deduplicate candidates by location + cause. Launch `security-validator` in parallel — one spawn per unique candidate. Hunter ≠ validator.
5. Write `docs/security-audit/<slug>.md`. Show it in the IDE/CLI. Do not commit.

Good: confirmed finding with a concrete attack, crossed boundary, and observed result.
Bad: "add more validation" with no reachable attack; live probing; editing the target.

`security-recon`, `security-hunter`, and `security-validator` are `fast` and readonly. Do not spawn squad reviewers.
