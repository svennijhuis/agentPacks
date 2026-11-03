# Review

You own this task. The main chat runs this playbook and delegates each step to a fresh squad-agent with the named role.

Spawn steps 1, 2, and 3 in parallel. Do not assign a Squad verdict. Do not start a fix round.

1. **squad-reviewer**: Review the diff for correctness and plan or spec. Use interrogate and blast-radius. **Evidence:** PASS or FAIL with a concrete fix list.
2. **squad-security-reviewer**: Walk trust boundaries when the security gate says run. Otherwise return skipped with the reason. Use the review contract Security gate. **Evidence:** findings prefixed A01 to A10, or skipped with the gate reason.
3. **simplifier**: Review the diff for reuse, comments, and needless abstraction. Use no-comments. **Evidence:** each new comment by file:line with kept or not kept, and any smaller diff.
