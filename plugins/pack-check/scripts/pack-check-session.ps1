# Detects registered stack markers and emits only trusted registry identifiers as agent context.
# Keep behavior aligned with pack-check-session.sh.
$ErrorActionPreference = 'Stop'

$registry = Join-Path $PSScriptRoot '../skills/pack-check/references/packs.md'
if (-not (Test-Path -LiteralPath $registry -PathType Leaf)) { exit 0 }

# Walks the tree pruning the skipped directories before descending, the way the POSIX half's
# `find -prune` does. Recursing into node_modules and filtering afterwards would put the whole
# dependency tree between session start and the first prompt. Reparse points are skipped for the
# same reason `find` does not follow symlinks: a link back up the tree would never terminate.
function Find-Marker([string] $Pattern) {
    $pruned = @('.git', 'bin', 'obj', 'node_modules')
    $pending = [System.Collections.Generic.Stack[string]]::new()
    $pending.Push((Get-Location).Path)

    while ($pending.Count -gt 0) {
        $directory = $pending.Pop()

        $hit = Get-ChildItem -LiteralPath $directory -File -Filter $Pattern -ErrorAction SilentlyContinue |
            Select-Object -First 1

        if ($hit) { return $true }

        foreach ($child in (Get-ChildItem -LiteralPath $directory -Directory -ErrorAction SilentlyContinue)) {
            if ($pruned -contains $child.Name) { continue }
            if ($child.Attributes -band [System.IO.FileAttributes]::ReparsePoint) { continue }
            $pending.Push($child.FullName)
        }
    }

    return $false
}

foreach ($line in Get-Content -LiteralPath $registry) {
    $cells = $line.Split('|')
    if ($cells.Count -lt 5) { continue }

    $markerCell = $cells[1].Trim().Replace('`', '')
    $stack = $cells[2].Trim().Replace('`', '')
    $pack = $cells[3].Trim().Replace('`', '')

    if (-not $markerCell -or $markerCell -eq 'Marker' -or $markerCell.StartsWith('---')) { continue }
    if ($stack -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') { continue }
    if ($pack -notmatch '^[a-z0-9]+(?:-[a-z0-9]+)*$') { continue }

    foreach ($marker in ($markerCell.Split(',') | ForEach-Object { $_.Trim() } | Where-Object { $_ })) {
        if (Find-Marker $marker) {
            Write-Output "pack-check detected stack $stack with pack $pack."
            Write-Output 'Before handling the first coding request, use the pack-check skill to resolve the required slot skills and request installation approval when they are missing.'
            exit 0
        }
    }
}

exit 0
