#!/usr/bin/env sh
# Lint one Squad verify harness: required five H2 sections, no em dash, secret-looking values.
# Optional args: docs/smoke-report.md and evidence files.
# Local files only. Exit 0 clean, 1 findings, 2 usage.
set -u
usage='usage: check-verify.sh docs/verify.md [docs/smoke-report.md] [evidence files...]'
verify=${1:-}
[ -n "$verify" ] && [ -f "$verify" ] || { echo "$usage" >&2; exit 2; }
shift
for f in "$@"; do
  [ -f "$f" ] || { echo "$usage" >&2; exit 2; }
done
errors=0
text=$(tr -d '\r' < "$verify")
lines() { printf '%s\n' "$text"; }
fail() { echo "$verify: $1"; errors=$((errors + 1)); }

need() { lines | grep -q "^## $1\$" || fail "missing ## $1"; }
need Launch
need Doctor
need Drive
need Evidence
need Cleanup

dash=$(printf '\342\200\224')
nodash() {
  for n in $(grep -n "$dash" "$1" | cut -d: -f1); do echo "$1:$n: em dash"; errors=$((errors + 1)); done
}
nodash "$verify"
for f in "$@"; do nodash "$f"; done

# Returns 0 if line looks secret-bearing (does not print the secret).
secret_line() {
  printf '%s\n' "$1" | grep -Eq \
    '(^|[^A-Za-z0-9_])(key|password|secret)=[^[:space:]]+' \
    || printf '%s\n' "$1" | grep -Eq '[A-Za-z0-9+/=_-]{40,}' \
    || printf '%s\n' "$1" | grep -Fq 'eyJ' \
    || printf '%s\n' "$1" | grep -Eq '(AKIA|ASIA)[A-Z0-9]{16}' \
    || printf '%s\n' "$1" | grep -Fq 'aws_secret_access_key' \
    || printf '%s\n' "$1" | grep -Fq 'X-Amz-Security-Token' \
    || printf '%s\n' "$1" | grep -Fq 'ya29.'
}

scan_secrets() {
  file=$1
  n=0
  while IFS= read -r line || [ -n "$line" ]; do
    n=$((n + 1))
    if secret_line "$line"; then
      echo "$file:$n: secret-looking value"
      errors=$((errors + 1))
    fi
  done < "$file"
}

scan_get_access_token() {
  file=$1
  n=0
  while IFS= read -r line || [ -n "$line" ]; do
    n=$((n + 1))
    stripped=$(printf '%s\n' "$line" | sed 's/\$([^)]*)//g')
    if printf '%s\n' "$stripped" | grep -Fq 'get-access-token'; then
      echo "$file:$n: get-access-token outside \$("
      errors=$((errors + 1))
    fi
  done < "$file"
}

scan_secrets "$verify"
scan_get_access_token "$verify"
for f in "$@"; do
  scan_secrets "$f"
  scan_get_access_token "$f"
done

[ "$errors" -eq 0 ]
