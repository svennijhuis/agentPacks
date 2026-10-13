# Lint one Squad plan: date-slug file name, required sections for its Status, no em dash.
# Twin of check-plan.sh. Local files only. Exit 0 clean, 1 findings, 2 usage.
param([string]$Plan)
if (-not $Plan -or -not (Test-Path -LiteralPath $Plan -PathType Leaf)) {
    [Console]::Error.WriteLine('usage: check-plan.ps1 docs/plans/<yyyy-mm-dd>-<slug>.md'); exit 2
}
$script:errors = 0
function Fail([string]$message) { Write-Output "${Plan}: $message"; $script:errors++ }
function Warn([string]$message) { Write-Output "${Plan}: warning: $message" }

$lines = @(Get-Content -LiteralPath $Plan -Encoding UTF8)
$text = $lines -join "`n"
$name = Split-Path -Leaf $Plan
if ($name -cnotmatch '^[0-9]{4}-[0-9]{2}-[0-9]{2}-[a-z0-9]+(-[a-z0-9]+)*\.md$') { Fail 'file name must be <yyyy-mm-dd>-<slug>.md' }
if ($text.Contains([string][char]0x2014)) { Fail 'contains an em dash' }

$status = ''
$index = [Array]::IndexOf($lines, ($lines | Where-Object { $_ -like '## Status*' } | Select-Object -First 1))
if ($index -ge 0 -and $index + 1 -lt $lines.Count) {
    $value = $lines[($index + 1)..($lines.Count - 1)] | Where-Object { $_.Trim() } | Select-Object -First 1
    if ($value -and -not $value.StartsWith('## ')) { $status = ($value.Trim() -split '\s+')[0] }
}
if (-not $status) { Fail 'missing ## Status value' }

function Need([string]$section) { if (-not ($lines -ccontains "## $section")) { Fail "missing ## $section" } }
Need 'Request'
if ($lines -ccontains '## Goal') {
    foreach ($s in 'Goal', 'Decision', 'Verify') { Need $s }
} else {
    Need 'Grill log'
    if ($status -ne 'draft') {
        foreach ($s in 'Decisions', 'Acceptance criteria', 'Test plan matrix', 'Open questions', 'Verification') { Need $s }
        if ($lines -ccontains 'Design: skipped (small)') { }
        elseif ($lines -ccontains '## Design') { Need 'Map' }
        else { Fail "missing ## Design or 'Design: skipped (small)'" }
    }
}
$hardIndex = [Array]::IndexOf($lines, '### Hard decisions')
if ($hardIndex -ge 0) {
    $hard = @()
    for ($i = $hardIndex + 1; $i -lt $lines.Count -and -not $lines[$i].StartsWith('#'); $i++) { if ($lines[$i].Trim()) { $hard += $lines[$i].Trim() } }
    $joined = $hard -join "`n"
    $slug = ($name -replace '^[0-9]{4}-[0-9]{2}-[0-9]{2}-', '') -replace '\.md$', ''
    if ($hard.Count -gt 0 -and $joined -ne 'None') {
        if (-not $joined.Contains('docs/adr/')) { Fail 'hard decision without a docs/adr/ link' }
        elseif ($joined -cnotmatch "docs/adr/[0-9]{4}-$([regex]::Escape($slug))(-[a-z0-9-]+)?\.md") { Fail "ADR link must be docs/adr/NNNN-$slug.md" }
    }
}
$slicesIndex = [Array]::IndexOf($lines, '## Slices')
if ($slicesIndex -ge 0) {
    for ($i = $slicesIndex + 1; $i -lt $lines.Count -and -not $lines[$i].StartsWith('## '); $i++) {
        $slice = $lines[$i]
        if ($slice -notmatch '^[0-9]+\. ') { continue }
        if ($slice -notmatch 'files') { Warn "slice has no files line: $slice" }
        if ($slice -notmatch 'done') { Warn "slice has no done check: $slice" }
    }
}
if ($slicesIndex -ge 0) {
    $sliceCount = 0; $reason = $false
    for ($i = $slicesIndex + 1; $i -lt $lines.Count -and -not $lines[$i].StartsWith('## '); $i++) {
        if ($lines[$i] -match '^[0-9]+\. ') { $sliceCount++ }
        if ($lines[$i].StartsWith('Single slice because:')) { $reason = $true }
    }
    if ($sliceCount -eq 1 -and -not $reason) { Warn "one slice and no 'Single slice because:' line" }
}
$mapIndex = [Array]::IndexOf($lines, '## Map')
if ($mapIndex -ge 0) {
    $hasMermaid = $false
    for ($i = $mapIndex + 1; $i -lt $lines.Count -and -not $lines[$i].StartsWith('## '); $i++) { if ($lines[$i].StartsWith('```mermaid')) { $hasMermaid = $true } }
    if (-not $hasMermaid) { Warn '## Map has no fenced mermaid block' }
}
if ($script:errors -eq 0) { exit 0 } else { exit 1 }
