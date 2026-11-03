# Feature

You own this task. The main chat runs this playbook and delegates each step to a fresh squad-agent with the named role.

1. **planner**: Name the outcome, the scope, and the acceptance checks. Use how, why, and blast-radius. **Evidence:** the plan path and the open decisions.
2. **architect**: Record Design and Map when the change crosses a layer, adds a project or package, or changes a public API. Otherwise write `Design: skipped (small)`. Use architect. **Evidence:** the Design and Map sections, or the skipped line.
3. **implementer**: Build from the plan, failing test first. Use tdd and the language standards. **Evidence:** the files touched and the failing-then-passing test output.

Run the Code review loop.
