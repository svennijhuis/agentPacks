# Lint one Squad verify harness: required five H2 sections.
# Twin of check-verify.sh. Local files only. Exit 0 clean, 1 findings, 2 usage.
param([string]$Verify)
if (-not $Verify -or -not (Test-Path -LiteralPath $Verify -PathType Leaf)) {
    [Console]::Error.WriteLine('usage: check-verify.ps1 docs/verify.md'); exit 2
}
$script:errors = 0
function Fail([string]$message) { Write-Output "${Verify}: $message"; $script:errors++ }

$lines = @(Get-Content -LiteralPath $Verify -Encoding UTF8)
function Need([string]$section) { if (-not ($lines -ccontains "## $section")) { Fail "missing ## $section" } }
Need 'Launch'
Need 'Doctor'
Need 'Drive'
Need 'Evidence'
Need 'Cleanup'
if ($script:errors -eq 0) { exit 0 } else { exit 1 }
