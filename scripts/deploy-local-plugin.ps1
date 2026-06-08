param(
    [int]$Port = 0,
    [switch]$Build
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot

if ($Build) {
    & (Join-Path $PSScriptRoot "build-shim.ps1")
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & (Join-Path $PSScriptRoot "build-plugin.ps1")
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$shim = Join-Path $root "plugin\QurreShim\bin\Qurre.dll"
$loli = Join-Path $root "plugin\Loli\bin\Loli.dll"

if (!(Test-Path $shim)) { throw "Missing artifact: $shim" }
if (!(Test-Path $loli)) { throw "Missing artifact: $loli" }

$scope = if ($Port -gt 0) { "$Port" } else { "global" }
$target = Join-Path $env:APPDATA "SCP Secret Laboratory\LabAPI\plugins\$scope"
New-Item -ItemType Directory -Force -Path $target | Out-Null

Copy-Item -LiteralPath $shim -Destination (Join-Path $target "Qurre.dll") -Force
Copy-Item -LiteralPath $loli -Destination (Join-Path $target "Loli.dll") -Force

Write-Host "[OK] Deployed Qurre.dll and Loli.dll to $target" -ForegroundColor Green
