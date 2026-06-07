# gather-dependencies.ps1
# Собирает игровые DLL из установки SCP: SL Dedicated Server в ./dependencies
# для сборки плагина Loli-Merged.
#
# Предварительно: установи сервер через SteamCMD (app 996560), укажи путь -ServerPath.
# Запуск:
#   powershell -ExecutionPolicy Bypass -File scripts\gather-dependencies.ps1 -ServerPath "C:\scpsl-server"

param(
    [Parameter(Mandatory=$true)]
    [string]$ServerPath
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$deps = Join-Path $root "dependencies"
New-Item -ItemType Directory -Force -Path $deps | Out-Null

# Managed-папка сервера
$managed = Join-Path $ServerPath "SCPSL_Data\Managed"
if (-not (Test-Path $managed)) {
    # альтернативные раскладки
    $alt = Get-ChildItem -Path $ServerPath -Recurse -Filter "Managed" -Directory -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($alt) { $managed = $alt.FullName } else { throw "Не нашёл папку Managed в $ServerPath" }
}
Write-Host "Managed: $managed" -ForegroundColor Cyan

# DLL, которые нужны плагину (имена как в Reference .csproj)
$need = @(
    "Assembly-CSharp.dll",
    "Assembly-CSharp-firstpass.dll",
    "0Harmony.dll",
    "CommandSystem.Core.dll",
    "Mirror.dll",
    "Mirror.Components.dll",
    "NorthwoodLib.dll",
    "Pooling.dll",
    "Newtonsoft.Json.dll",
    "Unity.TextMeshPro.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.PhysicsModule.dll",
    "UnityEngine.AudioModule.dll",
    "UnityEngine.UI.dll"
)

$found = 0; $missing = @()
foreach ($d in $need) {
    $src = Join-Path $managed $d
    if (Test-Path $src) {
        Copy-Item $src -Destination $deps -Force
        Write-Host "[ok]   $d" -ForegroundColor Green
        $found++
    } else {
        Write-Host "[MISS] $d" -ForegroundColor Red
        $missing += $d
    }
}

Write-Host ""
Write-Host "Скопировано: $found / $($need.Count)" -ForegroundColor Cyan
if ($missing) { Write-Host "Отсутствуют: $($missing -join ', ')" -ForegroundColor Yellow }

Write-Host ""
Write-Host "ДАЛЬШЕ ВРУЧНУЮ:" -ForegroundColor Magenta
Write-Host "  1. LabApi.dll      — из официального релиза Northwood (NuGet/GitHub LabAPI)"
Write-Host "  2. Qurre.dll       — сборка под v14 (см. docs/03_DEPENDENCIES.md)"
Write-Host "  3. QurreSocket.dll, SchematicUnity.dll — из sources/plugins-scpsl-2020-2024"
Write-Host "  4. SCPLogs.dll     — у основателя (внутренняя сборка)"
Write-Host "  5. Спубличить Assembly-CSharp.dll -> Assembly-CSharp_public.dll (publicizer)"
Write-Host "  6. Спубличить Mirror.dll -> Mirror_public.dll"
