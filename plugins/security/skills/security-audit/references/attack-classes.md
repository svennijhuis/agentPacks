# Attack classes

Recon picks in-scope rows. Core classes run on every audit. Companion classes run only when the target matches the trigger.

## Core — always

| Class | Hunt |
|---|---|
| Access control | Missing, inverted, or skippable authz. IDOR. Path traversal. SSRF. Default-allow. |
| Injection | Untrusted input reaches SQL, shell, template, LDAP, or eval. Trace every transform. |
| Authentication | Bypass, session fixation, credential stuffing surfaces, reset/token leaks. |
| Cryptography | Homegrown crypto, nonce reuse, weak KDF, secrets in source, HTTP for secrets. |
| Business logic | State skips, price/quantity tampering, race on a security decision. |
| Integrity | Unsigned or unverified artifacts, unsafe deserialization, prototype pollution on the server. |
| Feature abuse | Intended features chained into a boundary cross (export, webhook, impersonate, debug). |
| Wildcard | What recon named that no other class owns. One unusual trust assumption. |

## Companions — only when triggered

| Trigger | Class | Hunt |
|---|---|---|
| Browser/DOM, postMessage, webview | Client-side | XSS, UI redress, origin checks on messages. |
| Native, unsafe, kernel, parser | Memory safety | Bounds, lifetime, unsafe blocks, parser confusion. |
| LLM, tools, prompts, agents | AI/LLM | Prompt injection, tool-result trust, secret exfil via output. |
| HTTP cache, CDN, request framing | Web protocol | Cache poisoning, host/header smuggling, method override. |
| lockfile, CI, release, plugin | Supply chain | Unpinned actions, review-bypass paths, install scripts. |
| IAM, IaC, containers, ingress | Cloud | Public buckets, wildcard roles, privileged containers. |
| Multi-tenant, export, delete, backup | Isolation | Cross-tenant read/write, incomplete deletion, backup leak. |

Good: recon lists `Access control` and `Injection` with the files that handle authz and queries.
Bad: launching every companion "just in case"; a hunter that covers two classes in one spawn.
