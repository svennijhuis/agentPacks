# Workflow

Parent coordinates. Specialists own their context. Profiles change breadth, never the evidence bar.

## Profiles

| Profile | Hunt |
|---|---|
| `quick` | One hunter wave, then validate. Partial coverage; say so. |
| `standard` | Default. One class per hunter, then validate every unique candidate. |

A scoped run (named paths, one subsystem, or a diff) seeds only in-scope surfaces. Everything else is out of scope, never "covered".

## 1. Recon

Launch `security-recon` once against the target and scope. It returns the architecture summary: app type, stack, actors, authn/z, input surfaces with `path:line`, and in-scope attack classes.

Do not hunt before that summary exists. If recon cannot name a surface, stop and say so — do not invent one.

## 2. Hunt

Launch `security-hunter` in **one** parallel batch. One spawn per in-scope class from recon. Each prompt includes:

1. The recon summary, verbatim.
2. The assigned class and its starting paths.
3. The instruction to return candidates or `No exploitable vulnerabilities found`.

Hunters are isolated. Do not paste one hunter's transcript into another. A hunter that needs a rabbit hole uses its own tools; it does not spawn `security-validator`.

## 3. Validate

Merge candidates that share location and cause. Launch `security-validator` in **one** parallel batch — one spawn per unique candidate. Each prompt includes only that candidate and the recon summary.

The validator tries to **disprove** the claim. Parent never validates. Hunter never validates.

Drop `rejected`. Keep `confirmed` and `needs_validation`.

## 4. Report

Write `docs/security-audit/<slug>.md` from the surviving rows using [the report](report.md). Show the same content in the IDE/CLI. Do not commit. Do not edit product code.

Stop only when the file is written, or when recon could not produce a surface and the parent said so.
