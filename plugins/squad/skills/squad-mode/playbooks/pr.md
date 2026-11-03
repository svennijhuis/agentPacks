# PR

You own this task. The main chat runs this playbook and delegates each step to a fresh squad-agent with the named role.

1. **verifier**: Read CI and the unresolved review threads. Use the repo verifier skill. **Evidence:** red checks, thread links, and PASS, FAIL, or BLOCKED.
2. **implementer**: Fix the failures the verifier named. Use tdd and the language standards. **Evidence:** the files touched and the commands run.

Run the Code review loop.
