# Lint one Squad retro: date-slug file name, no em dash (U+2014).
# Twin of check-retro.sh. Local files only. Exit 0 clean, 1 findings, 2 usage.
param([string]$Retro)
if (-not $Retro -or -not (Test-Path -LiteralPath $Retro -PathType Leaf)) {
    [Console]::Error.WriteLine('usage: check-retro.ps1 docs/retros/<yyyy-mm-dd>-<slug>.md'); exit 2
}
$script:errors = 0
function Fail([string]$message) { Write-Output "${Retro}: $message"; $script:errors++ }

$name = Split-Path -Leaf $Retro
if ($name -cnotmatch '^[0-9]{4}-[0-9]{2}-[0-9]{2}-[a-z0-9]+(-[a-z0-9]+)*\.md$') { Fail 'file name must be <yyyy-mm-dd>-<slug>.md' }

$lines = @(Get-Content -LiteralPath $Retro -Encoding UTF8)
for ($i = 0; $i -lt $lines.Count; $i++) {
    if ($lines[$i].Contains([string][char]0x2014)) { Fail "line $($i + 1) contains an em dash" }
}

if ($script:errors -gt 0) { exit 1 }
exit 0
