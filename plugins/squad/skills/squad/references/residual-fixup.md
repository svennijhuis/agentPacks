# Optional residual fixup

After `pass`, or after two fix rounds when only residual `low`/`tiny` notes remain on the merge
report: at most **one** residual fixup pass (main agent or a fresh implementer), limited to those
notes. Never a third full reviewer fan-out. Never another implement/review/re-ask cycle from residual.
If code changed, run a verifier spot-check only, then hand off. Failed spot-check → hand off with
notes; do not start another fixup. Residual after code change does not keep a clean `pass` verdict —
handoff as uncommitted with residual notes. Still do not land the branch.
