# Tokio runtime exceptions

Load only after isolation and a named metric justify leaving the default path. These are not the first move.

## Long polls can be fine

Batching work into a longer poll is often faster than tiny bursts. Under light load, work stealing covers one occupied worker. That coverage fails when the runtime is saturated (no spare worker) or the OS is saturated (unpark is late). Then I/O and other runtime maintenance do not run often enough for the latency target.

`tokio::join!` and `tokio::select!` are in-task concurrency: there is no steal inside one task. Blocking that task stalls every sibling on it. Timeouts and bad tail latency often come from this, not from the runtime.

How do I know: the runtime has spare workers and unpark is prompt, or the slow path is `join!` / `select!` on the same task.

## Separate runtimes by priority

The hard isolation is two (or more) runtimes, each pinned to its own cores. Put latency-sensitive work on one and background work on the other. Set OS niceness in `on_thread_start` if the host still shares cores. Many production services land on at least two runtimes.

How do I know: background work and request work share one runtime and the metric is still the request P99 after fairness and mutex work.

## Spin to keep control

For latency measured in microseconds, a short spin (about 50µs) instead of yielding avoids a scheduler or kernel round-trip. It burns a core and can delay neighbors. Wrong for most apps; a last resort under controlled isolation.

How do I know: the budget is microseconds, workers are already isolated, and yield or unpark delay is the remaining term.
