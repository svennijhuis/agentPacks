# Local access

Ask once, only after harness yes, only for what the repo does not say: `provider` (`azure`|`aws`|`gcp`|`other`) and token/API `scope` if Drive calls a protected API. No tenant or subscription questions. Save under optional `## Local access` in `docs/verify.md` (provider and scope only, never values). Answer `skip` → `provider: skip` and never ask again; human can edit later. Section present → no re-ask.

No up-front vault or cloud CLI probe. Doctor launches the app as usual, greps the startup log for access errors (table below). Match → affected scenarios
`skipped (needs: kv|auth, <redacted line>)` plus one hint line.
Non-access startup failure stays `blocked`.
Missing token → `skipped (needs: auth, missing token)`.
One try only: no retries, no login-fix attempts; same access error on another scenario → mark all scenarios needing it skipped at once. Reason once in `docs/smoke-report.md` and one retro line. Verify never blocks `/squad`.

Tokens never in any markdown, never shown to the model. App authenticates itself via the existing login. Drive API calls create the token inline inside a single curl line: `curl -sS -H "Authorization: Bearer $(az account get-access-token --scope <s> --query accessToken -o tsv)" ...`; no echo, no stored variable, no `-v`/`--trace`. AWS uses aws CLI/SDK (SigV4), never a key in curl. Model sees only the response body, cut at 120 chars and masked.

| Provider | Startup log patterns | Hint | Lint patterns |
|---|---|---|---|
| Azure | AADSTS, CredentialUnavailable, DefaultAzureCredential failed, 401/403 | az login | eyJ JWT |
| AWS | Unable to locate credentials, ExpiredToken, AccessDenied, NoCredentialProviders, The SSO session | aws sso login | AKIA/ASIA + 16 chars, aws_secret_access_key, X-Amz-Security-Token |
| GCP | invalid_grant, PERMISSION_DENIED, Could not automatically determine credentials | gcloud auth application-default login | ya29. |

## Redaction

Cut at 120 chars. Mask anything the secret lint matches. Apply to matched log lines before writing Outcome or retro:

```sh
printf '%s' "$line" | sed -E \
  -e 's/eyJ[A-Za-z0-9_-]*\.[A-Za-z0-9_-]*\.[A-Za-z0-9_-]*/[redacted]/g' \
  -e 's/(AKIA|ASIA)[A-Z0-9]{16}/[redacted]/g' \
  -e 's/ya29\.[A-Za-z0-9_-]+/[redacted]/g' \
  -e 's/(key|password|secret)=[^[:space:]]+/[redacted]/g' \
  -e 's/aws_secret_access_key[^[:space:]]*/[redacted]/g' \
  -e 's/X-Amz-Security-Token[^[:space:]]*/[redacted]/g' \
  -e 's/[A-Za-z0-9+=_-]{40,}/[redacted]/g' \
  | cut -c1-120
```

## Example ## Local access

```markdown
## Local access
provider: azure
scope: https://vault.azure.net/.default
```

`provider: skip` alone is valid and stops further asks.

## Example log → skipped

Azure log: `DefaultAzureCredential failed to retrieve a token from the included credentials. CredentialUnavailableException (Status: 401)`
→ Outcome: `skipped (needs: auth, DefaultAzureCredential failed to retrieve a token from the included credentials.)`
→ Hint: `az login`

AWS log: `Unable to locate credentials. NoCredentialProviders: no valid providers in chain`
→ Outcome: `skipped (needs: auth, Unable to locate credentials. NoCredentialProviders: no valid providers in chain)`
→ Hint: `aws sso login`

GCP log: `Could not automatically determine credentials. PERMISSION_DENIED: invalid_grant`
→ Outcome: `skipped (needs: auth, Could not automatically determine credentials. PERMISSION_DENIED: invalid_grant)`
→ Hint: `gcloud auth application-default login`

Non-access startup failure: `Unhandled exception. System.IO.FileNotFoundException: Could not load file or assembly 'App'`
→ Outcome: `blocked` (no access-pattern match; not skipped)
