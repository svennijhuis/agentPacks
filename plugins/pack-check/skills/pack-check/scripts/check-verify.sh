#!/usr/bin/env sh
# Lint one Squad verify harness: required five H2 sections.
# Local files only. Exit 0 clean, 1 findings, 2 usage.
set -u
verify=${1:-}
[ -n "$verify" ] && [ -f "$verify" ] || { echo "usage: check-verify.sh docs/verify.md" >&2; exit 2; }
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

[ "$errors" -eq 0 ]
