---
name: learnings-digest
description: When summarizing docs/learnings.md without rewriting skills.
license: UNLICENSED
disable-model-invocation: true
user-invocable: false
---

# Learnings digest

Read `docs/learnings.md` when it exists. Summarize the latest same-entrypoint entries. Prefer passed skips and tiers. A failed skip is a must-run.

Do not rewrite skills. Do not grow a graph. The log is append-only. Never edit or delete an earlier entry.

Report the digest. Stop.

Good: "last `/squad` fail: skipped security is must-run; demote one tier."
Bad: rewrite a skill because the log blamed the prompt.
