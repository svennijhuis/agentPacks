#!/usr/bin/env bash
# Formats the C# file an agent just wrote, using the repository's .editorconfig.
#
# Reads the client's hook event JSON on stdin. The regex arrives as -Matcher because no client's
# afterFileEdit matcher filters on the edited path — all four spend it naming the write tool — so
# the same authored regex is applied here and decides the outcome everywhere. The flag is spelled
# the one way both parsers accept: PowerShell binds -Matcher, and has no double-dash parameters.
#
# Never blocks: whatever happens, this writes at most one line and exits 0. A formatter that fails
# on half-written code must not stop the agent that is still writing it.
#
# AGENTPACKS_DOTNET_FORMAT:
#   unset|whitespace  dotnet format whitespace <dir> --folder --include <file>   (no restore, no build)
#   full              dotnet format <nearest project> --include <file>           (style and analyzers)
#   off               do nothing
set -euo pipefail

matcher=""

while [ "$#" -gt 0 ]; do
  case "$1" in
    -Matcher)
      matcher="${2-}"
      shift 2
      ;;
    *)
      shift
      ;;
  esac
done

mode="${AGENTPACKS_DOTNET_FORMAT:-whitespace}"

if [ "$mode" = "off" ]; then
  exit 0
fi

payload="$(cat || true)"

if [ -z "$payload" ]; then
  exit 0
fi

# The edited path sits under a different key in each dialect, and Claude nests it inside tool_input.
# Take the first plausible one rather than depending on a JSON parser being installed.
file_path="$(printf '%s' "$payload" \
  | tr ',' '\n' \
  | grep -Eo '"(file_path|filePath|path|notebook_path)"[[:space:]]*:[[:space:]]*"[^"]*"' \
  | head -n 1 \
  | sed -E 's/^"[^"]*"[[:space:]]*:[[:space:]]*"//; s/"$//' || true)"

if [ -z "$file_path" ]; then
  exit 0
fi

if [ -n "$matcher" ] && ! printf '%s' "$file_path" | grep -Eq "$matcher"; then
  exit 0
fi

# The authored matcher cannot be anchored — the manifest forbids $ — so "[.]cs" also matches
# "Index.cshtml". Settle the extension exactly here.
case "$file_path" in
  *.cs|*.csx|*.vb) ;;
  *) exit 0 ;;
esac

if [ ! -f "$file_path" ]; then
  exit 0
fi

if ! command -v dotnet >/dev/null 2>&1; then
  exit 0
fi

file_dir="$(dirname "$file_path")"

# Walks up from the edited file for the nearest thing dotnet format can take as a workspace.
find_project() {
  local dir
  dir="$(cd "$file_dir" 2>/dev/null && pwd || true)"

  while [ -n "$dir" ] && [ "$dir" != "/" ]; do
    local pattern
    for pattern in '*.slnx' '*.sln' '*.csproj'; do
      local match
      match="$(find "$dir" -maxdepth 1 -name "$pattern" -type f 2>/dev/null | sort | head -n 1 || true)"

      if [ -n "$match" ]; then
        printf '%s' "$match"
        return 0
      fi
    done

    dir="$(dirname "$dir")"
  done

  return 1
}

output=""
status=0

file_abs="$(cd "$file_dir" 2>/dev/null && pwd || true)/$(basename "$file_path")"

if [ "$mode" = "full" ]; then
  project="$(find_project || true)"

  if [ -z "$project" ]; then
    exit 0
  fi

  # --include is matched against the workspace, not the filesystem: an absolute path silently
  # formats nothing. Run from the project directory and pass the path relative to it.
  base="$(cd "$(dirname "$project")" 2>/dev/null && pwd || true)"
  include="${file_abs#"$base"/}"

  output="$(cd "$base" && dotnet format "$(basename "$project")" --include "$include" 2>&1)" || status=$?
else
  output="$(cd "$file_dir" && dotnet format whitespace . --folder --include "$(basename "$file_path")" 2>&1)" || status=$?
fi

if [ "$status" -ne 0 ]; then
  detail="$(printf '%s' "$output" | grep -v '^[[:space:]]*$' | head -n 1 || true)"
  printf 'dotnet: dotnet format failed on %s: %s\n' "$file_path" "$detail"
  exit 0
fi

printf 'dotnet: formatted %s\n' "$file_path"
exit 0
