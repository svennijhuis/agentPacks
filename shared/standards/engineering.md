# Engineering principles (shared)

Cross-language. Seven lines. Reviewer and simplifier findings cite the line by name.

- **Fix root causes.** Reproduce, name the cause, fix it, and fix the same pattern elsewhere. A guard, catch, or retry with no named cause is a symptom patch.
- **Prove it works.** Run the real artifact and assert the exact user-visible result. A proxy check or a call-only assertion is not proof.
- **Guard at boundaries.** Parse and validate where data enters, into a named domain type. Trust the types inside.
- **Make operations idempotent.** Scripts, handlers, and migrations converge when run twice.
- **Separate before serializing.** Give shared state one owner before adding locks or queues around it.
- **Subtract before you add.** Smallest diff, one source of truth. Delete a layer or copy before adding another.
- **Follow the repo.** Use established configuration and local patterns for choices a standard leaves open, keep public surface small, and never land a shape you know is worse just to finish faster.
