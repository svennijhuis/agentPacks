#!/usr/bin/env sh
# Lint one Squad plan: date-slug file name, required sections for its Status, no em dash.
# Local files only. Exit 0 clean, 1 findings, 2 usage.
set -u
plan=${1:-}
[ -n "$plan" ] && [ -f "$plan" ] || { echo "usage: check-plan.sh docs/plans/<yyyy-mm-dd>-<slug>.md" >&2; exit 2; }
errors=0
fail() { echo "$plan: $1"; errors=$((errors + 1)); }

name=$(basename "$plan")
echo "$name" | grep -Eq '^[0-9]{4}-[0-9]{2}-[0-9]{2}-[a-z0-9]+(-[a-z0-9]+)*\.md$' \
  || fail "file name must be <yyyy-mm-dd>-<slug>.md"

grep -q "$(printf '\342\200\224')" "$plan" && fail "contains an em dash"

status=$(awk '/^## Status/{while ((getline) > 0 && $0 ~ /^[[:space:]]*$/) {} ; if ($0 !~ /^[[:space:]]*$/ && $0 !~ /^## Status/) print $1; exit}' "$plan")
[ -n "$status" ] || fail "missing ## Status value"

need() { grep -q "^## $1\$" "$plan" || fail "missing ## $1"; }
need Request
if grep -q '^## Goal$' "$plan"; then
  for s in Goal Decision Verify; do need "$s"; done
else
  need "Grill log"
  if [ "$status" != draft ]; then
    for s in Decisions "Acceptance criteria" "Test plan matrix" "Open questions" Verification; do need "$s"; done
    if grep -q '^## Design$' "$plan"; then need Map
    else grep -q '^Design: skipped (small)$' "$plan" || fail "missing ## Design or 'Design: skipped (small)'"; fi
  fi
fi

[ "$errors" -eq 0 ]
