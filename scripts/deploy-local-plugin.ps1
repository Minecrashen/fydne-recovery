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
$deps = Join-Path $root "dependencies"

if (!(Test-Path $shim)) { throw "Missing artifact: $shim" }
if (!(Test-Path $loli)) { throw "Missing artifact: $loli" }

$scope = if ($Port -gt 0) { "$Port" } else { "global" }
$target = Join-Path $env:APPDATA "SCP Secret Laboratory\LabAPI\plugins\$scope"
$depTarget = Join-Path $env:APPDATA "SCP Secret Laboratory\LabAPI\dependencies\$scope"
New-Item -ItemType Directory -Force -Path $target | Out-Null
New-Item -ItemType Directory -Force -Path $depTarget | Out-Null

Copy-Item -LiteralPath $shim -Destination (Join-Path $target "Qurre.dll") -Force
Copy-Item -LiteralPath $loli -Destination (Join-Path $target "Loli.dll") -Force

$runtimeDependencies = @(
    "QurreSocket.dll",
    "System.Net.Http.dll",
    "Newtonsoft.Json.dll",
    "0Harmony.dll",
    "Microsoft.CSharp.dll",
    "System.Dynamic.dll"
)

foreach ($name in $runtimeDependencies) {
    $source = Join-Path $deps $name
    if (!(Test-Path $source) -and ($name -eq "Microsoft.CSharp.dll" -or $name -eq "System.Dynamic.dll")) {
        $source = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Microsoft.CSharp.dll"
        if ($name -eq "System.Dynamic.dll") {
            $source = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\System.Dynamic.dll"
        }
    }
    if (!(Test-Path $source)) {
        Write-Warning "Missing dependency: $name"
        continue
    }
    Copy-Item -LiteralPath $source -Destination (Join-Path $depTarget $name) -Force
}

Write-Host "[OK] Deployed Qurre.dll and Loli.dll to $target" -ForegroundColor Green
Write-Host "[OK] Deployed runtime dependencies to $depTarget" -ForegroundColor Green
