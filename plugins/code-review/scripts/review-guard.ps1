# Advisory guard for shell commands that skip review: pushing, bypassing hooks, recursive deletes.
#
# Windows half of the pair. Same contract as review-guard.sh: read the client's hook event JSON on
# stdin, never block, write one line of advice and exit 0.
[CmdletBinding()]
param(
    [string] $Matcher = ''
)

$ErrorActionPreference = 'Stop'

$payload = [Console]::In.ReadToEnd()

if ([string]::IsNullOrWhiteSpace($payload)) {
    exit 0
}

# The command sits under a different key in each dialect; take the first one present.
$commandText = $null

try {
    $event = $payload | ConvertFrom-Json

    foreach ($key in @('command', 'command_line', 'shell_command')) {
        if ($event.PSObject.Properties.Name -contains $key -and $event.$key) {
            $commandText = [string] $event.$key
            break
        }
    }
}
catch {
    exit 0
}

if ([string]::IsNullOrWhiteSpace($commandText)) {
    exit 0
}

if ($Matcher -and $commandText -notmatch $Matcher) {
    exit 0
}

Write-Output "code-review: `"$commandText`" skips the review loop. Run /review-diff before this lands."
exit 0
