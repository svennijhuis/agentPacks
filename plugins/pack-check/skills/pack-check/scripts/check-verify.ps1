# Lint one Squad verify harness: required five H2 sections, no em dash, secret-looking values.
# Optional args: docs/smoke-report.md and evidence files.
# Twin of check-verify.sh. Local files only. Exit 0 clean, 1 findings, 2 usage.
param(
    [Parameter(Mandatory = $false, Position = 0)][string]$Verify,
    [Parameter(Mandatory = $false, ValueFromRemainingArguments = $true)][string[]]$Rest
)
$usage = 'usage: check-verify.ps1 docs/verify.md [docs/smoke-report.md] [evidence files...]'
if (-not $Verify -or -not (Test-Path -LiteralPath $Verify -PathType Leaf)) { [Console]::Error.WriteLine($usage); exit 2 }
if ($null -eq $Rest) { $Rest = @() }
foreach ($f in $Rest) {
    if (-not (Test-Path -LiteralPath $f -PathType Leaf)) { [Console]::Error.WriteLine($usage); exit 2 }
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
$dash = [string][char]0x2014
function NoDash([string]$file) {
    $n = 0
    foreach ($line in @(Get-Content -LiteralPath $file -Encoding UTF8)) {
        $n++
        if ($line.Contains($dash)) { Write-Output "${file}:${n}: em dash"; $script:errors++ }
    }
}
NoDash $Verify
foreach ($f in $Rest) { NoDash $f }

function Test-SecretLine([string]$line) {
    if ($line -match '(^|[^A-Za-z0-9_])(key|password|secret)=(\S+)') { return $true }
    if ($line -match '[A-Za-z0-9+/=_-]{40,}') { return $true }
    if ($line.Contains('eyJ')) { return $true }
    if ($line -match '(AKIA|ASIA)[A-Z0-9]{16}') { return $true }
    if ($line.Contains('aws_secret_access_key')) { return $true }
    if ($line.Contains('X-Amz-Security-Token')) { return $true }
    if ($line.Contains('ya29.')) { return $true }
    return $false
}

function Scan-Secrets([string]$file, [string]$sectionOnly = '') {
    $n = 0
    $inSection = $false
    foreach ($line in @(Get-Content -LiteralPath $file -Encoding UTF8)) {
        $n++
        if ($sectionOnly) {
            if ($line -ceq "## $sectionOnly") { $inSection = $true; continue }
            if ($line.StartsWith('## ')) { $inSection = $false; continue }
            if (-not $inSection) { continue }
        }
        if (Test-SecretLine $line) {
            Write-Output "${file}:${n}: secret-looking value"
            $script:errors++
        }
    }
}

function Scan-GetAccessToken([string]$file) {
    $n = 0
    foreach ($line in @(Get-Content -LiteralPath $file -Encoding UTF8)) {
        $n++
        $stripped = [regex]::Replace($line, '\$\([^)]*\)', '')
        if ($stripped.Contains('get-access-token')) {
            Write-Output "${file}:${n}: get-access-token outside `$("
            $script:errors++
        }
    }
}

Scan-Secrets $Verify 'Local access'
Scan-GetAccessToken $Verify
foreach ($f in $Rest) { Scan-Secrets $f }

if ($script:errors -eq 0) { exit 0 } else { exit 1 }
