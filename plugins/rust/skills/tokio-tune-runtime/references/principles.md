# Tokio runtime principles

Default path after a metric is named. Co-locate each move with the check that proves it is the problem.

## Metric first

Work backward from the metric in the skill. Long polls are common; most are benign. Schedule latency — time from a task becoming ready to Tokio polling it — is the usual Tokio-side symptom, not the cause.

How do I know: the change is a "Tokio hygiene" pass with no P99, throughput, or schedule-latency target.

## Yield for latency

Low latency across many connections needs fairness. A pipelined read loop can stay `Poll::Ready` from an in-memory buffer and never return to the runtime, so one client holds a worker while others wait. Yield after a small streak of immediately-ready reads so other tasks get a poll without giving up batching entirely.

```rust
execute_command(&self.db, &mut self.connection, frame).await?;
ready_streak += 1;
if ready_streak >= 4 {
    tokio::task::yield_now().await;
    ready_streak = 0;
}
```

How do I know: P99 is much greater than P50; a poll lasts longer than the work inside it; many spans sit inside one poll.

## Batch for throughput

Fairness costs. More useful work per runtime event — task switch, poll, steal, thread hop — raises efficiency. Batch a series of filesystem or other blocking calls into one `spawn_blocking` (or a dedicated OS thread). `tokio::fs` without `io_uring` is one `spawn_blocking` per call plus a shared blocking pool. Spawn is cheap until it is hundreds or thousands of tasks; a ~10µs unit of work as its own task is usually anti-helpful.

How do I know: `spawn_blocking` or Tokio APIs show up in flamegraphs; a tight loop issues many tiny filesystem or blocking calls; grouping the same work raises throughput.

## Global resources

Workers scale across cores; some runtime resources do not. The blocking pool is currently a global bottleneck at high enough rates — ballpark 50,000 blocking tasks per second on a 32-core host, not a law. Short bounded CPU work can be cheaper left on the worker than pushed through `spawn_blocking`. Tasks also land on the global queue when a local queue overflows or when work is scheduled from outside a worker (a channel sender on a non-Tokio thread).

How do I know: `spawn_blocking` is prominent in flamegraphs; the global queue stays deep. In a healthy app it stays near empty.

## Mutexes

A contended blocking mutex can pin every worker: a metrics registry behind a lock, then a flush that holds it while doing expensive work, then every worker blocks recording a metric. Stealing dies because nobody is running. Keep the critical section to a hashmap update, or lock, clone, release. Prefer one task that owns the data and a channel for mutations; batch those sends, because the channel is then the global resource. `tokio::sync::Mutex` is expensive, FutureLock-prone, and only fits a critical section that lasts milliseconds. `RWLock` still contends on atomics on the read path and is almost never the right primitive.

How do I know: P99 spikes on a timer (a one-minute flush); many tasks go blocked and off-CPU together.

## Bound fan-out

Tokio will spawn more tasks than the rest of the system can take. An unbounded fan-out to S3 or any client is the common accident. A `Semaphore` is usually enough.

How do I know: connection or request counts track task count with no cap.

## Isolate workers

Tokio needs workers to wake quickly. A loaded kernel can take 10–20 ms to schedule a worker after unpark — fatal if P99 is single-digit milliseconds. Other processes (or Rust threads such as a `tracing_appender` that runs 100ms+ without yielding) delay that wake. Pin Tokio workers and other threads onto separate cores with `cgroups` or the same family of APIs. Reserve cores; using every core for Tokio is rarely the latency win.

How do I know: a kernel scheduling delay sits between worker-unpark and the worker actually running.
