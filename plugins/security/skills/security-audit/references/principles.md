# Principles

Source-first audit. A candidate without a concrete lower-trust principal, crossed control, affected resource, and observed result is not a confirmed finding.

## Boundary and result

Name the actor, the accepted input or action, the intended control, the boundary crossed, and the owner-observable result. Missing best practice, self-impact, or a guessed deployment control is not a finding.

## Concrete attack

Confirmed means you can state the inputs, requests, or action sequence. "Might be injectable" is omitted.

## Impact is the severity bar

Only `confirmed` records receive severity. Likelihood × impact from the demonstrated result, not from a checklist gap. Overall severity cannot exceed demonstrated impact. `needs_validation` has no severity.

## Defense in depth

If an earlier layer already prevents the attack, a missing later layer is a hardening note, not a finding.

## Adversarial validation

The agent that checks a candidate is never the agent that found it. Hunters do not self-validate.

## Source visibility

Deployment, proxy, identity-provider, and topology controls that are absent from the repository are unresolved facts. Record `needs_validation` with the exact missing fact. Do not assume presence or absence.

## Execution

Inspect source. Do not probe live, shared, or production systems. Do not spend paid quota, alter releases, or run unbounded availability tests. Dummy principals and fixtures only. The audit describes a fix; it does not patch the target.

## Additive runs

One pass is not complete coverage. Say so. A prior `rejected` claim suppresses only that unchanged claim, not the unit.
