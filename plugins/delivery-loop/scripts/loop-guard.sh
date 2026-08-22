#!/usr/bin/env bash
# Advisory guard for the commands that land work: committing, pushing, merging, opening or merging a PR.
#
# The delivery loop stops at a hand-off, so none of its phases should reach here. Reads the client's
# hook event JSON on stdin. Clients disagree on how a hook blocks an action, so this deliberately
# never blocks: it writes one line of advice and exits 0, which every client treats as "allow, and
# show this". The regex arrives as -Matcher because only Cursor can filter on command text itself;
# the other three spend their matcher naming the shell tool. The flag is spelled the one way both
# parsers accept: PowerShell binds -Matcher to param($Matcher) and has no double-dash parameters.
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

payload="$(cat || true)"

# The command sits under a different key in each dialect. Take the first plausible one rather than
# depending on a JSON parser being installed.
command_text="$(printf '%s' "$payload" \
  | tr ',' '\n' \
  | grep -Eo '"(command|command_line|shell_command)"[[:space:]]*:[[:space:]]*"[^"]*"' \
  | head -n 1 \
  | sed -E 's/^"[^"]*"[[:space:]]*:[[:space:]]*"//; s/"$//')"

if [ -z "$command_text" ]; then
  exit 0
fi

if [ -n "$matcher" ] && ! printf '%s' "$command_text" | grep -Eq "$matcher"; then
  exit 0
fi

printf 'delivery-loop: "%s" lands work, which is the human step. Hand over the working tree and the loop summary instead.\n' "$command_text"
exit 0
