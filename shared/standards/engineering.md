# Engineering principles (shared)

Cross-language. Reviewer, simplifier, and verifier findings cite the section by name.

## Fix root causes

- Reproduce first. Ask why until you reach the cause, then fix it there.
- No workarounds. A guard, catch, retry, or nil check that silences a failure is a symptom patch. A workaround that needs a paragraph of comment to justify it means the code is wrong.
- Fix the pattern, not the instance: search for the same shape and fix every copy.
- When stuck, instrument. Read the actual error or add logging; do not guess.
- A failure after restart: suspect stale persistent state (config, cache, lock file, serialized state) before code.

## Attack the premise

- Two fixes that share one premise and fail the same gate are evidence against the premise, not the fixes.
- Write the premise down as one sentence before the next fix. In a Squad loop the second failed fix on the same premise is `Replan:`, not a third fix.

## Prove it works

- Check the real artifact directly: the running process, the actual value, the real response. A proxy (mtime, cached output, self-report, "it compiles") is not proof.
- When a check fails, suspect the observation method before the system.
- Prefer a rerunnable script or command over a one-time look, and keep its output as evidence a reviewer can rerun.

## Explain the number

- Name the limiter: the core, lock, disk, network, or load generator that bounds the result, from a profile or counters, not a guess.
- Rule out what else the number could measure: errors, cached or skipped work, code that never ran, an untuned side, noise.
- Keep the run count, the spread, and the limiter with every number.

## Test behavior, not implementation

- A test calls the code as its users do and asserts the exact result they see. Delete or rewrite a test that would still pass if the subject returned a wrong value.
- Banned shapes: loose assertions (`toBeDefined`, `toBeTruthy`, `not.toThrow`, `Assert.NotNull` alone, `> 0`), call-only or absence-only checks (`toHaveBeenCalled`, `Received()` without arguments, empty-list equality), expected values computed by the subject, restated constants or config defaults, and assertions on data the test built without calling the subject.
- Fix: one concrete input, the literal output or effect. For a mock, assert the payload it received. For an absence, also assert the presence on another input.
- No test beats a bad test. When a failing test first is not cheap, use the closest executable check and say so.

## Guard at boundaries

- Validate, parse, and handle errors where data enters: CLI args, config, external APIs, network protocols. Parse into named domain types there.
- Inside the system, trust the types. No redundant re-validation deep in call chains.
- Do not re-export transport, storage, framework, or wire types through the public surface.
- Keep business logic in pure functions; the shell around them stays thin and mechanical.

## Make operations idempotent

- Every state-changing operation answers: what if this runs twice, and what if the last run crashed halfway?
- Converge on startup: scan existing state, clean stale artifacts, adopt live work. Compare by content, not creation order.
- If the answer depends on what state was left behind, add a reconciliation step.

## Separate before serializing shared state

- First ask whether concurrent actors need the same mutable object. Usually they publish independent facts: give each its own file, key, or branch and merge at the read boundary.
- Only when one shared write target is a real invariant, serialize structurally (single writer, sequential phases, lock, compare-and-swap). Instructions and conventions are not concurrency control.

## Subtract before you add

- Remove complexity first, then build. Cut before you polish.
- Design for observed usage. No speculative validators, parsers, or guards beyond the spec.
- Delete a reference or stub with no novel content instead of keeping it.

## Laziness protocol

- Make the smallest change that solves the problem.
- Consolidate a decision behind one source of truth instead of repeating it.

## Minimize reader load

- Track layers to trace and state to hold. Collapse wrappers with one caller, adapters with no second implementation, and pass-through layers.
- Shrink state scope: returns over mutations, locals over fields, derive instead of sync. Name an invariant once, at the boundary.
- The 30-second test: a new reader can answer "where does X come from?" and "what can change X?" in under 30 seconds. If not, cut layers or state.

## Sequence verifiable units

- Order work as small units, each ending green, and do not advance on red. Data structures and scaffold before features.
- Deliver in the order that proves the work: failing test, then fix; subtraction before reshape.

## Encode lessons in structure

- A repeated rule becomes an architecture boundary, a type, a lint, or a test, in that order of preference. Prose is the last resort.
- If the fix is structural, use only the structural fix; the instruction was the symptom.
- A lesson that needs judgment gets a prominent instruction and an example of the failure.

