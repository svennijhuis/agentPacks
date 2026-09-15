---
name: tokio-tune-runtime
description: When tuning Tokio schedule latency, long polls, spawn_blocking, mutex stalls, or P99 fairness.
license: UNLICENSED
---

# Tokio runtime tuning

Performance overlay for Tokio. The correctness floor stays in `errors-concurrency.md`. Summarized from [Principles for fast Tokio applications](https://dial9-rs.github.io/blog/principles-for-fast-tokio-applications/).

When loaded by exact Skill tool name `tokio-tune-runtime`:

1. Name the metric you are moving: P99 versus P50, throughput, or the Tokio schedule-latency histogram. Done when that metric is named, or when you stop because there is none and no stall evidence.
2. Hold this model:
   - A poll is the work between `.await` points.
   - A future sits idle until the executor polls it again because it is ready.
   - Tokio runs N workers, usually one per core; each has a local queue. Overflow or off-worker spawn goes to the global queue.
   - Another worker steals only when it has capacity and the runtime sees the imbalance.
3. Classify the symptom, then read the matching reference. Done when the case is one of: fairness, batching overhead, global-resource depth, mutex stall, OS unpark delay, or a justified advanced exception.
   - Default path: [principles](references/principles.md).
   - Known-better exceptions only after isolation and a named metric justify them: [advanced](references/advanced.md).
4. Apply the smallest change that can move that metric. Cite `tokio-tune-runtime`. Also cite `errors-concurrency.md` when the finding is a correctness rule (blocking the executor, a sync guard across `.await`).

Good: yield after several immediately-ready pipeline reads because P99 is 10× P50.
Bad: shortening every poll past 100µs with no user-facing metric.
