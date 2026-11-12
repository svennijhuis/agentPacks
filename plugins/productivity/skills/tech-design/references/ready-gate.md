# Ready gate

`design.md` is the spec. `to-spec` (TD2+) checks that `design.md` meets the spec bar. `to-tickets` (TD2+) then writes `tickets.md` from `design.md`. Ready writes `tickets.md` only.

## Ticket

Every ticket in `tickets.md` has:

- a title
- an anchor link to the `design.md` section it comes from, such as `design.md#sign-off`
- acceptance criteria
- story points

Blockers name the tickets that must land first. One ticket is one slice a developer can build.
