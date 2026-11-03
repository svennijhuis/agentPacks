# Verification contract

The verifier role on `squad-agent` runs the repo's own verifier skill. In this pack that skill is `verify-harness` when the change has app behavior. Load it by exact Skill name. If the target repo has no verifier skill, run create-verification-skill first and then use the skill it writes. Do not invent a private checklist in its place.

The skill must cover these five parts. Ready means usable, not merely that a process answers.

## LAUNCH

The start command and the ready signal. From an empty data dir, including migrations and seed data.

## DOCTOR

One read-only health check. Process up, the build you intend to drive, the port owned by this run.

## DRIVE

The real UI, CLI, or API a user would touch. Not an internal setter and not a test-only endpoint.

## EVIDENCE

Observed output and artifact paths. Capture the action and the resulting state. Check side effects.

## CLEANUP

Stop the processes this run started. Remove temp files. Keep the evidence.

Result is PASS, FAIL, or BLOCKED, with the real output.

- PASS. The named checks ran and the observable result matches the ask.
- FAIL. A check ran and the result does not match. Name the command and the first failing line.
- BLOCKED. A tool, access, or secret is missing. Name exactly what is missing. Do not guess. Do not push.

`not verified` is not a pass. `blocked` is unproven, not a pass. A `needs:`-tagged Doctor check that fails is `skipped (needs: <tag>)` (tag only). Reason once in a one-line `Local access: <redacted reason>; <login hint>` note in the verifier report. The whole line must be <=120 chars. Truncate the redacted reason to fit after the prefix and hint (do not cut the reason to 120 first). Never in `docs/smoke-report.md`.

Shell calls rejected → `blocked: environment (no shell)`, so no fix round is spent on it. Reading the diff is not verification.

When `docs/verify.md` exists, run Launch, Doctor, Drive for every touched scenario, then Cleanup (always). Lint with pack-check `check-verify.sh docs/verify.md` (or `.ps1`), adding `docs/smoke-report.md` only if it exists. A finding is `fail`. Read-only: never write these files. Hard Doctor fail → `blocked`, never `pass`. `skipped` and `not run`: [doctor tags](../../verify-harness/references/doctor-tags.md). Missing file → today's behavior.

tagged `needs:` → Result tag-only `skipped (needs: <tag>)`.

Quantitative criterion → re-measure and quote `<measured> <op> <bound>`. A green test name is not a measurement. Build, restore, or type-check alone → `not verified` for a behavioral criterion.

Return the verifier report including **Evidence gaps** and **Assumptions challenged** (`None` when empty).
