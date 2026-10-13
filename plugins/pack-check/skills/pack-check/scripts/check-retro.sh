#!/usr/bin/env sh
# Lint one Squad retro: date-slug file name, no em dash (U+2014).
# Local files only. Exit 0 clean, 1 findings, 2 usage.
set -u
retro=${1:-}
[ -n "$retro" ] && [ -f "$retro" ] || { echo "usage: check-retro.sh docs/retros/<yyyy-mm-dd>-<slug>.md" >&2; exit 2; }
errors=0
fail() { echo "$retro: $1"; errors=$((errors + 1)); }

basename "$retro" | grep -Eq '^[0-9]{4}-[0-9]{2}-[0-9]{2}-[a-z0-9]+(-[a-z0-9]+)*\.md$' \
  || fail "file name must be <yyyy-mm-dd>-<slug>.md"

for n in $(tr -d '\r' < "$retro" | grep -n "$(printf '\342\200\224')" | cut -d: -f1); do
  fail "line $n contains an em dash"
done

[ "$errors" -eq 0 ] || exit 1
exit 0
