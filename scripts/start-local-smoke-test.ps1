param(
    [string]$ServerPath = "C:\Users\Admin\fydne_build\scpsl-server",
    [switch]$Build
)

$ErrorActionPreference = "Stop"

if ($Build) {
    & (Join-Path $PSScriptRoot "deploy-local-plugin.ps1") -Build
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$localAdmin = Join-Path $ServerPath "LocalAdmin.exe"
if (!(Test-Path $localAdmin)) {
    throw "LocalAdmin.exe not found: $localAdmin"
}

Write-Host "Starting SCP:SL LocalAdmin smoke-test..." -ForegroundColor Cyan
Write-Host "Server path: $ServerPath"
Write-Host "Watch the new console for LabAPI, Qurre.dll, Loli.dll, TypeLoadException, MissingMethodException, NullReferenceException."

Start-Process -FilePath $localAdmin -WorkingDirectory $ServerPath
