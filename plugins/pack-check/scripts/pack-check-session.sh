#!/usr/bin/env bash
# Detects registered stack markers and emits only trusted registry identifiers as agent context.
# Keep behavior aligned with pack-check-session.ps1.
set -uo pipefail

script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd -P)"
registry="$script_dir/../skills/pack-check/references/packs.md"
[ -f "$registry" ] || exit 0

trim() {
  local value="$1"
  value="${value#"${value%%[![:space:]]*}"}"
  value="${value%"${value##*[![:space:]]}"}"
  printf '%s' "$value"
}

while IFS='|' read -r _ marker_cell stack_cell pack_cell _; do
  marker_cell="$(trim "${marker_cell//\`/}")"
  stack="$(trim "${stack_cell//\`/}")"
  pack="$(trim "${pack_cell//\`/}")"

  [ -z "$marker_cell" ] && continue
  [ "$marker_cell" = "Marker" ] && continue
  [[ "$marker_cell" == ---* ]] && continue
  [[ "$stack" =~ ^[a-z0-9]+(-[a-z0-9]+)*$ ]] || continue
  [[ "$pack" =~ ^[a-z0-9]+(-[a-z0-9]+)*$ ]] || continue

  IFS=',' read -r -a markers <<< "$marker_cell"
  for marker in "${markers[@]}"; do
    marker="$(trim "$marker")"
    [ -z "$marker" ] && continue

    found="$(find "$(pwd -P)" \
      \( -type d \( -name .git -o -name bin -o -name obj -o -name node_modules \) -prune \) -o \
      \( -type f -name "$marker" -print -quit \) 2>/dev/null)"

    if [ -n "$found" ]; then
      printf 'pack-check detected stack %s with pack %s.\n' "$stack" "$pack"
      printf 'Before handling the first coding request, use the pack-check skill to resolve the required slot skills and request installation approval when they are missing.\n'
      exit 0
    fi
  done
done < "$registry"

exit 0
