# Handoff

Use this shape as the pull request body after a PASS. The main chat fills every field from the run. Do not leave a field blank. Write `none` when it is empty.

## tested_commit

The full SHA that was pushed. Rebased work uses a new branch whose name ends in `-v2`. Never force push.

## diff summary

What changed for the person who will review it, then what the next owner of the code inherits. One short paragraph.

## commands + results

Each command that was run, and the result it printed. Quote the output. A command that was not run is listed under unverified_checks, not here.

## evidence_paths

Files, logs, or report paths the verifier and the reviewer produced. Paths, not pasted bodies.

## unverified_checks

Checks that did not run, and why. `none` when the loop passed with every named check run.

## remaining_risks

What can still break. `none` only when the run found nothing left.
