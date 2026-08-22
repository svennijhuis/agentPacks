---
description: "Checklist to apply when reviewing source files"
applyTo: "**/*.cs,**/*.ts,**/*.tsx,**/*.rs"
---

# Review checklist

- Does the change hold at the boundaries: empty input, one element, the maximum?
- Is every failure path either handled or deliberately propagated?
- Does untrusted input reach a shell, a query, a file path or rendered output?
- Is there a test that fails without this change?
- Do the names still describe what the code does after the change?
