# clone-sources.ps1
# Клонирует три исходных репозитория FYDNE в ./sources (gitignored).
# Запуск:  powershell -ExecutionPolicy Bypass -File scripts\clone-sources.ps1

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$sources = Join-Path $root "sources"
New-Item -ItemType Directory -Force -Path $sources | Out-Null

$repos = @(
    "https://github.com/uwu-loli/Loli-Merged.git",
    "https://github.com/uwu-loli/plugins-scpsl-2020-2024.git",
    "https://github.com/uwu-loli/fydne-prod-web-2020-2023.git"
)

foreach ($r in $repos) {
    $name = ($r -split '/')[-1] -replace '\.git$',''
    $dest = Join-Path $sources $name
    if (Test-Path $dest) {
        Write-Host "[skip] $name уже склонирован — git pull" -ForegroundColor Yellow
        git -C $dest pull
    } else {
        Write-Host "[clone] $name" -ForegroundColor Cyan
        git clone --depth 1 $r $dest
    }
}
Write-Host "Готово. Исходники в $sources (не коммитятся — папка в .gitignore)" -ForegroundColor Green
