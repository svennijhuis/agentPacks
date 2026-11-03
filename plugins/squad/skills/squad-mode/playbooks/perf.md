# Perf

You own this task. The main chat runs this playbook and delegates each step to a fresh squad-agent with the named role.

1. **verifier**: Measure the baseline with the method you will reuse. Use the repo verifier skill. **Evidence:** the method, the baseline number, and the command output.
2. **implementer**: Change only what the baseline points at. Use tdd and the language standards. **Evidence:** the files touched.
3. **verifier**: Measure again with the same method. Use the repo verifier skill. **Evidence:** the new number beside the baseline, and the same command.

Run the Code review loop.
