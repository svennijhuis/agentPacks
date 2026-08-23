# Blocks selected destructive Git operations. Never print the command/payload: it may contain secrets.
[CmdletBinding()]
param([string] $Matcher = '')

$ErrorActionPreference = 'Stop'
if ($env:AGENTPACKS_GIT_GUARD -eq 'off') { exit 0 }

$payload = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($payload)) { exit 0 }

$commandText = $null
try {
    $event = $payload | ConvertFrom-Json
    if ($event.tool_input -and $event.tool_input.command) {
        $commandText = [string] $event.tool_input.command
    }
    elseif ($event.PSObject.Properties.Name -contains 'command' -and $event.command) {
        $commandText = [string] $event.command
    }
}
catch { $commandText = $null }

if ([string]::IsNullOrWhiteSpace($commandText)) { exit 0 }
if ($Matcher -and $commandText -notmatch $Matcher) { exit 0 }

function Block-GitCommand([string] $Rule, [string] $Reason) {
    [Console]::Error.WriteLine("BLOCKED by git safety rule ${Rule}: $Reason")
    [Console]::Error.WriteLine('The command was not run. Ask the human to perform it, or set AGENTPACKS_GIT_GUARD=off.')
    exit 2
}

# Grouping and substitution characters are separators too: `(git reset --hard)` and
# `$(git reset --hard)` run the command just as surely as a bare invocation, and leaving `(`
# attached would stop the Git detection below from ever matching.
foreach ($segment in ($commandText -split '[;&|(){}\r\n]+')) {
    if ($segment -notmatch '(?:^|\s)git\s+(.+)') { continue }
    $words = @($Matches[1] -split '\s+' | Where-Object { $_ })
    if ($words.Count -eq 0) { continue }

    $index = 0
    while ($index -lt $words.Count) {
        $word = $words[$index]
        if ($word -in @('-C', '-c', '--git-dir', '--work-tree')) { $index += 2; continue }
        if ($word -match '^--(?:git-dir|work-tree)=' -or $word -in @('--no-pager', '--bare') -or $word.StartsWith('-')) { $index++; continue }
        break
    }
    if ($index -ge $words.Count) { continue }

    $verb = $words[$index]
    $tail = if ($index + 1 -lt $words.Count) {
        @($words[($index + 1)..($words.Count - 1)])
    }
    else { @() }
    # Case-sensitive throughout, so the two halves agree: bash's `case` and `=` are
    # case-sensitive, and PowerShell's -contains, -eq, -match and switch are not by default.
    $has = { param($value) $tail -ccontains $value }

    # Short Git options bundle: `-uf` is the same force as `-f`. No safe short option of the
    # subcommands below carries an 'f', so matching the letter anywhere in a single-dash cluster
    # costs no false positive and closes the bundled spelling.
    $hasForce = [bool] ($tail | Where-Object { $_ -ceq '--force' -or $_ -cmatch '^-[^-]*f' })

    switch -CaseSensitive ($verb) {
        'reset' {
            if (& $has '--hard') { Block-GitCommand GIT001 'reset --hard discards uncommitted working-tree changes.' }
        }
        'clean' {
            if ($hasForce) {
                Block-GitCommand GIT002 'clean --force permanently deletes untracked files.'
            }
        }
        'push' {
            if ($hasForce -or ($tail | Where-Object { $_ -ceq '--force-with-lease' -or $_ -cmatch '^--force-with-lease=' })) {
                Block-GitCommand GIT003 'forced push rewrites remote branch history.'
            }
        }
        'branch' {
            # -D is the bundled spelling of --delete --force, and -df is the same again. Plain -d
            # and --delete stay allowed: Git refuses those on an unmerged branch by itself.
            $hasDelete = [bool] ($tail | Where-Object { $_ -ceq '--delete' -or $_ -cmatch '^-[^-]*d' })

            if ((& $has '-D') -or ($hasDelete -and $hasForce)) {
                Block-GitCommand GIT004 'forced branch deletion can make unmerged commits unreachable.'
            }
        }
        'checkout' {
            # `checkout --force` discards every uncommitted change in the tree, not just the
            # paths named, so it is the same loss as `checkout .` with no pathspec spelled out.
            if ($hasForce) {
                Block-GitCommand GIT005 'forced checkout discards all working-tree changes.'
            }
            if ((& $has '.') -or (($tail -ccontains '--') -and $tail[-1] -cne '--')) {
                Block-GitCommand GIT005 'checkout over paths discards working-tree changes.'
            }
        }
        'restore' {
            if (-not (& $has '--staged') -or (& $has '--worktree')) {
                Block-GitCommand GIT006 'restore overwrites working-tree changes.'
            }
        }
    }
}

exit 0
