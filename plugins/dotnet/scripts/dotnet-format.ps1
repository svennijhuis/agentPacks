# Formats the C# file an agent just wrote, using the repository's .editorconfig.
#
# Windows half of the pair. Same contract as dotnet-format.sh: read the client's hook event JSON on
# stdin, apply the -Matcher regex, format the file, never block, exit 0.
#
# AGENTPACKS_DOTNET_FORMAT: whitespace (default), full, off.
[CmdletBinding()]
param(
    [string] $Matcher = ''
)

$ErrorActionPreference = 'Stop'

$mode = $env:AGENTPACKS_DOTNET_FORMAT

if ([string]::IsNullOrWhiteSpace($mode)) {
    $mode = 'whitespace'
}

if ($mode -eq 'off') {
    exit 0
}

$payload = [Console]::In.ReadToEnd()

if ([string]::IsNullOrWhiteSpace($payload)) {
    exit 0
}

# The edited path sits under a different key in each dialect, and Claude nests it inside tool_input.
# Walk the object rather than guessing the shape.
function Find-FilePath {
    param($Node, [int] $Depth = 0)

    if ($null -eq $Node -or $Depth -gt 6) {
        return $null
    }

    foreach ($key in @('file_path', 'filePath', 'path', 'notebook_path')) {
        if ($Node.PSObject.Properties.Name -contains $key) {
            $value = $Node.$key

            if ($value -is [string] -and -not [string]::IsNullOrWhiteSpace($value)) {
                return [string] $value
            }
        }
    }

    foreach ($property in $Node.PSObject.Properties) {
        if ($property.Value -is [psobject] -and $property.Value -isnot [string]) {
            $found = Find-FilePath -Node $property.Value -Depth ($Depth + 1)

            if ($found) {
                return $found
            }
        }
    }

    return $null
}

try {
    $event = $payload | ConvertFrom-Json
}
catch {
    exit 0
}

$filePath = Find-FilePath -Node $event

if ([string]::IsNullOrWhiteSpace($filePath)) {
    exit 0
}

if ($Matcher -and $filePath -notmatch $Matcher) {
    exit 0
}

# The authored matcher cannot be anchored — the manifest forbids $ — so "[.]cs" also matches
# "Index.cshtml". Settle the extension exactly here.
if ([System.IO.Path]::GetExtension($filePath) -notin @('.cs', '.csx', '.vb')) {
    exit 0
}

if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
    exit 0
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    exit 0
}

$fileDir = Split-Path -Path $filePath -Parent

if ([string]::IsNullOrWhiteSpace($fileDir)) {
    $fileDir = '.'
}

# Walks up from the edited file for the nearest thing dotnet format can take as a workspace.
function Find-Project {
    param([string] $StartDirectory)

    $dir = Resolve-Path -LiteralPath $StartDirectory -ErrorAction SilentlyContinue

    if (-not $dir) {
        return $null
    }

    $current = [System.IO.DirectoryInfo] $dir.Path

    while ($current) {
        foreach ($pattern in @('*.slnx', '*.sln', '*.csproj')) {
            $match = Get-ChildItem -LiteralPath $current.FullName -Filter $pattern -File -ErrorAction SilentlyContinue |
                Sort-Object -Property Name |
                Select-Object -First 1

            if ($match) {
                return $match.FullName
            }
        }

        $current = $current.Parent
    }

    return $null
}

$ErrorActionPreference = 'Continue'

$fileAbsolute = (Resolve-Path -LiteralPath $filePath).Path

try {
    if ($mode -eq 'full') {
        $project = Find-Project -StartDirectory $fileDir

        if (-not $project) {
            exit 0
        }

        # --include is matched against the workspace, not the filesystem: an absolute path silently
        # formats nothing. Run from the project directory and pass the path relative to it.
        $base = Split-Path -Path $project -Parent
        $include = [System.IO.Path]::GetRelativePath($base, $fileAbsolute)

        Push-Location -LiteralPath $base

        try {
            $output = & dotnet format (Split-Path -Path $project -Leaf) --include $include 2>&1
        }
        finally {
            Pop-Location
        }
    }
    else {
        Push-Location -LiteralPath $fileDir

        try {
            $output = & dotnet format whitespace . --folder --include (Split-Path -Path $fileAbsolute -Leaf) 2>&1
        }
        finally {
            Pop-Location
        }
    }
}
catch {
    exit 0
}

if ($LASTEXITCODE -ne 0) {
    $detail = ($output | Where-Object { $_ -and "$_".Trim() } | Select-Object -First 1)
    Write-Output "dotnet: dotnet format failed on $filePath`: $detail"
    exit 0
}

Write-Output "dotnet: formatted $filePath"
exit 0
