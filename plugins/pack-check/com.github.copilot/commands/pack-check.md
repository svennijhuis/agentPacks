---
name: "pack-check"
description: "Detect the repository stack, verify its required language-pack skills, and request approval to install the missing pack."
---

# Check the language pack

Load the `pack-check` skill with the Skill tool by exact name `pack-check`. Never write
`/pack-check` as prose to load it. Then run its complete detect, resolve, approval, and
installation flow.
When the pack is already installed, print its explicit installed status. When installation succeeds,
stop and ask for a reload instead of continuing in the current session.
