# Design (shared)

Cross-language. Language packs add type, ownership, and layout rules on top.

- Use established repository configuration and repeated local patterns for choices a standard leaves open.
- Keep public surface area small. Expose an item only when another component is intended to depend on it.
- Never land a shape you know is worse just to finish faster.

## Layers

- Application never holds storage, SDK, or keys. It owns use cases, ports, and domain types.
- One project per external system (for example `<App>.Infrastructure.<Vendor>`), not per package: RestEase and Polly for the same API live in one project. It implements the Application port.
