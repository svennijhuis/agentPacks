# Advisory guard for the commands that land work: committing, pushing, merging, opening or merging a PR.
#
# Windows half of the pair. Same contract as loop-guard.sh: read the client's hook event JSON on
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

Write-Output "delivery-loop: `"$commandText`" lands work, which is the human step. Hand over the working tree and the loop summary instead."
exit 0
