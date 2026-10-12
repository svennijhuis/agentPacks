# Lint one Squad plan: date-slug file name, required sections for its Status, no em dash.
# Twin of check-plan.sh. Local files only. Exit 0 clean, 1 findings, 2 usage.
param([string]$Plan)
if (-not $Plan -or -not (Test-Path -LiteralPath $Plan -PathType Leaf)) {
    [Console]::Error.WriteLine('usage: check-plan.ps1 docs/plans/<yyyy-mm-dd>-<slug>.md'); exit 2
}
$script:errors = 0
function Fail([string]$message) { Write-Output "${Plan}: $message"; $script:errors++ }

$lines = @(Get-Content -LiteralPath $Plan -Encoding UTF8)
$text = $lines -join "`n"
$name = Split-Path -Leaf $Plan
if ($name -cnotmatch '^[0-9]{4}-[0-9]{2}-[0-9]{2}-[a-z0-9]+(-[a-z0-9]+)*\.md$') { Fail 'file name must be <yyyy-mm-dd>-<slug>.md' }
if ($text.Contains([string][char]0x2014)) { Fail 'contains an em dash' }

$status = ''
$index = [Array]::IndexOf($lines, ($lines | Where-Object { $_ -like '## Status*' } | Select-Object -First 1))
if ($index -ge 0) {
    $value = $lines[($index + 1)..($lines.Count - 1)] | Where-Object { $_.Trim() } | Select-Object -First 1
    if ($value) { $status = ($value.Trim() -split '\s+')[0] }
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
        if ($lines -ccontains '## Design') { Need 'Map' }
        elseif (-not ($lines -ccontains 'Design: skipped (small)')) { Fail "missing ## Design or 'Design: skipped (small)'" }
    }
}
if ($script:errors -eq 0) { exit 0 } else { exit 1 }
