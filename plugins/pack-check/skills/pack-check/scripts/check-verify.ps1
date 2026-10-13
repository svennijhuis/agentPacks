# Lint one Squad verify harness: required five H2 sections, no em dash.
# Optional second arg: docs/smoke-report.md, checked for em dashes too.
# Twin of check-verify.sh. Local files only. Exit 0 clean, 1 findings, 2 usage.
param([string]$Verify, [string]$Report)
$usage = 'usage: check-verify.ps1 docs/verify.md [docs/smoke-report.md]'
if (-not $Verify -or -not (Test-Path -LiteralPath $Verify -PathType Leaf)) { [Console]::Error.WriteLine($usage); exit 2 }
if ($Report -and -not (Test-Path -LiteralPath $Report -PathType Leaf)) { [Console]::Error.WriteLine($usage); exit 2 }
$script:errors = 0
function Fail([string]$message) { Write-Output "${Verify}: $message"; $script:errors++ }

$lines = @(Get-Content -LiteralPath $Verify -Encoding UTF8)
function Need([string]$section) { if (-not ($lines -ccontains "## $section")) { Fail "missing ## $section" } }
Need 'Launch'
Need 'Doctor'
Need 'Drive'
Need 'Evidence'
Need 'Cleanup'
$dash = [string][char]0x2014
function NoDash([string]$file) {
    $n = 0
    foreach ($line in @(Get-Content -LiteralPath $file -Encoding UTF8)) {
        $n++
        if ($line.Contains($dash)) { Write-Output "${file}:${n}: em dash"; $script:errors++ }
    }
}
NoDash $Verify
if ($Report) { NoDash $Report }
if ($script:errors -eq 0) { exit 0 } else { exit 1 }
