# Bug fix

You own this task. The main chat runs this playbook and delegates each step to a fresh squad-agent with the named role.

1. **planner**: Reproduce the failure and name the cause. Use how and why. **Evidence:** the repro command, its output, and the cause with file pointers.
2. **architect**: Run only when the fix crosses a function boundary. Otherwise return skipped. Use architect. **Evidence:** the design note, or skipped with the reason.
3. **implementer**: Write the failing repro test first, then the fix. Use tdd and the language standards. **Evidence:** the failing output, the passing output, and the files touched.

Run the Code review loop.
