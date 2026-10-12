#!/usr/bin/env sh
# Lint one Squad plan: date-slug file name, required sections for its Status, no em dash.
# Local files only. Exit 0 clean, 1 findings, 2 usage.
set -u
plan=${1:-}
[ -n "$plan" ] && [ -f "$plan" ] || { echo "usage: check-plan.sh docs/plans/<yyyy-mm-dd>-<slug>.md" >&2; exit 2; }
errors=0
text=$(tr -d '\r' < "$plan")
lines() { printf '%s\n' "$text"; }
fail() { echo "$plan: $1"; errors=$((errors + 1)); }

name=$(basename "$plan")
echo "$name" | grep -Eq '^[0-9]{4}-[0-9]{2}-[0-9]{2}-[a-z0-9]+(-[a-z0-9]+)*\.md$' \
  || fail "file name must be <yyyy-mm-dd>-<slug>.md"

lines | grep -q "$(printf '\342\200\224')" && fail "contains an em dash"

status=$(lines | awk '/^## Status/{while ((getline) > 0 && $0 ~ /^[[:space:]]*$/) {} ; if ($0 !~ /^[[:space:]]*$/ && $0 !~ /^## Status/) print $1; exit}')
[ -n "$status" ] || fail "missing ## Status value"

need() { lines | grep -q "^## $1\$" || fail "missing ## $1"; }
need Request
if lines | grep -q '^## Goal$'; then
  for s in Goal Decision Verify; do need "$s"; done
else
  need "Grill log"
  if [ "$status" != draft ]; then
    for s in Decisions "Acceptance criteria" "Test plan matrix" "Open questions" Verification; do need "$s"; done
    if lines | grep -q '^## Design$'; then need Map
    else lines | grep -q '^Design: skipped (small)$' || fail "missing ## Design or 'Design: skipped (small)'"; fi
  fi
fi

hard=$(lines | awk '/^### Hard decisions/{f=1; next} /^#/{f=0} f && NF' )
if [ -n "$hard" ] && [ "$hard" != None ]; then
  slug=$(echo "$name" | sed -E 's/^[0-9]{4}-[0-9]{2}-[0-9]{2}-//; s/\.md$//')
  if ! printf '%s\n' "$hard" | grep -q 'docs/adr/'; then
    fail "hard decision without a docs/adr/ link"
  elif ! printf '%s\n' "$hard" | grep -Eq "docs/adr/[0-9]{4}-$slug(-[a-z0-9-]+)?\.md"; then
    fail "ADR link must be docs/adr/NNNN-$slug.md"
  fi
fi

[ "$errors" -eq 0 ]
