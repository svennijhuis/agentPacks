#!/usr/bin/env bash
# Blocks selected destructive Git operations. The command/payload is never printed because it may
# contain credentials or other secrets. Keep rule ids and behaviour aligned with git-guard.ps1.
set -uo pipefail

if [ "${AGENTPACKS_GIT_GUARD:-on}" = "off" ]; then
  exit 0
fi

matcher=""
while [ "$#" -gt 0 ]; do
  case "$1" in
    -Matcher) matcher="${2-}"; shift 2 ;;
    *) shift ;;
  esac
done

payload="$(cat || true)"
[ -z "$payload" ] && exit 0

command_text=""
if command -v jq >/dev/null 2>&1; then
  command_text="$(printf '%s' "$payload" | jq -r '
    [.tool_input.command?, .command?]
    | map(select(type == "string" and length > 0)) | first // ""
  ' 2>/dev/null)"
fi

# A malformed or unknown payload is allowed rather than scanned as raw text. Raw event fields may
# quote dangerous commands as documentation, and blocking them would be both noisy and misleading.
[ -z "$command_text" ] && exit 0

if [ -n "$matcher" ] && ! printf '%s' "$command_text" | grep -Eq "$matcher"; then
  exit 0
fi

block() {
  printf 'BLOCKED by git safety rule %s: %s\n' "$1" "$2" >&2
  printf 'The command was not run. Ask the human to perform it, or set AGENTPACKS_GIT_GUARD=off.\n' >&2
  exit 2
}

# Inspect each shell segment independently. This catches a destructive Git command after `cd &&`
# without treating harmless text elsewhere in the command as Git arguments. Grouping and
# substitution characters are separators too: `(git reset --hard)` and `$(git reset --hard)` run
# the command just as surely as a bare invocation, and leaving `(` attached would stop the Git
# detection below from ever matching.
segments="$(printf '%s' "$command_text" | tr ';|&(){}\n' '\n')"
while IFS= read -r segment; do
  if [[ ! "$segment" =~ (^|[[:space:]])git[[:space:]]+(.+) ]]; then
    continue
  fi

  args="${BASH_REMATCH[2]}"
  read -r -a words <<< "$args"
  [ "${#words[@]}" -eq 0 ] && continue

  index=0
  while [ "$index" -lt "${#words[@]}" ]; do
    word="${words[$index]}"
    case "$word" in
      -C|-c|--git-dir|--work-tree) index=$((index + 2)) ;;
      --git-dir=*|--work-tree=*|--no-pager|--bare) index=$((index + 1)) ;;
      -*) index=$((index + 1)) ;;
      *) break ;;
    esac
  done

  [ "$index" -ge "${#words[@]}" ] && continue
  verb="${words[$index]}"
  tail=("${words[@]:$((index + 1))}")

  has_exact() {
    local wanted="$1" item
    for item in "${tail[@]}"; do [ "$item" = "$wanted" ] && return 0; done
    return 1
  }

  # Short Git options bundle: `-uf` is the same force as `-f`. No safe short option of the
  # subcommands below carries an 'f', so matching the letter anywhere in a single-dash cluster
  # costs no false positive and closes the bundled spelling.
  has_force() {
    local item
    for item in "${tail[@]}"; do
      [ "$item" = "--force" ] && return 0
      [[ "$item" =~ ^-[^-]*f ]] && return 0
    done
    return 1
  }

  case "$verb" in
    reset)
      has_exact --hard && block GIT001 'reset --hard discards uncommitted working-tree changes.'
      ;;
    clean)
      has_force && block GIT002 'clean --force permanently deletes untracked files.'
      ;;
    push)
      has_force && block GIT003 'forced push rewrites remote branch history.'
      for word in "${tail[@]}"; do
        case "$word" in
          --force-with-lease|--force-with-lease=*)
            block GIT003 'forced push rewrites remote branch history.' ;;
        esac
      done
      ;;
    branch)
      # -D is the bundled spelling of --delete --force, and -df is the same again. Plain -d and
      # --delete stay allowed: Git refuses those on an unmerged branch by itself.
      has_delete() {
        local item
        for item in "${tail[@]}"; do
          [ "$item" = "--delete" ] && return 0
          [[ "$item" =~ ^-[^-]*d ]] && return 0
        done
        return 1
      }

      if has_exact -D || { has_delete && has_force; }; then
        block GIT004 'forced branch deletion can make unmerged commits unreachable.'
      fi
      ;;
    checkout)
      # `checkout --force` discards every uncommitted change in the tree, not just the paths
      # named, so it is the same loss as `checkout .` with none of the pathspec spelled out.
      has_force && block GIT005 'forced checkout discards all working-tree changes.'
      has_exact . && block GIT005 'checkout over paths discards working-tree changes.'
      for ((i = 0; i < ${#tail[@]}; i++)); do
        if [ "${tail[$i]}" = "--" ] && [ $((i + 1)) -lt "${#tail[@]}" ]; then
          block GIT005 'checkout over paths discards working-tree changes.'
        fi
      done
      ;;
    restore)
      if ! has_exact --staged || has_exact --worktree; then
        block GIT006 'restore overwrites working-tree changes.'
      fi
      ;;
  esac
done <<< "$segments"

exit 0
