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

found_any=0

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
      \( -type d \( -name .git -o -name bin -o -name obj -o -name target -o -name node_modules -o -name vendor \) -prune \) -o \
      \( -type f -name "$marker" -print -quit \) 2>/dev/null)"

    if [ -n "$found" ]; then
      printf 'pack-check detected stack %s with pack %s.\n' "$stack" "$pack"
      found_any=1
      break
    fi
  done
done < "$registry"

if [ "$found_any" -eq 1 ]; then
  printf 'Before handling the first coding request, use the pack-check skill to select the stacks applicable to the change, resolve their required slot skills, and request installation approval when they are missing.\n'
fi

exit 0
