#!/usr/bin/env sh
# Lint one Squad verify harness: required five H2 sections, no em dash.
# Optional second arg: docs/smoke-report.md, checked for em dashes too.
# Local files only. Exit 0 clean, 1 findings, 2 usage.
set -u
verify=${1:-}
report=${2:-}
[ -n "$verify" ] && [ -f "$verify" ] || { echo "usage: check-verify.sh docs/verify.md [docs/smoke-report.md]" >&2; exit 2; }
[ -z "$report" ] || [ -f "$report" ] || { echo "usage: check-verify.sh docs/verify.md [docs/smoke-report.md]" >&2; exit 2; }
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
[ -z "$report" ] || nodash "$report"

[ "$errors" -eq 0 ]
