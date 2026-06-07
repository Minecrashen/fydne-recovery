# build-shim.ps1 — быстрая сборка Qurre-shim'а через csc (без dotnet SDK).
# Компилирует plugin/QurreShim/src/*.cs против всех DLL в dependencies/ → Qurre.dll.
# Это рабочий харнесс цикла миграции: добавил обёртку → запустил → прочитал ошибки → починил.
#
# Запуск:  powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1

$ErrorActionPreference = "Stop"
$root  = Split-Path -Parent $PSScriptRoot
$shim  = Join-Path $root "plugin\QurreShim"
$deps  = Join-Path $root "dependencies"
$out   = Join-Path $shim "bin"
$csc   = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"

if (-not (Test-Path $csc)) { throw "csc.exe не найден: $csc (поставь/проверь VS2022)" }
if (-not (Test-Path $deps)) { throw "Нет папки dependencies/. Прогони gather-dependencies или установи сервер." }
New-Item -ItemType Directory -Force -Path $out | Out-Null

# Референсы — все DLL из dependencies, КРОМЕ непубличных дублей (берём _public там, где есть)
$refs = Get-ChildItem $deps -Filter *.dll | Where-Object {
    $_.Name -ne "Assembly-CSharp.dll" -and $_.Name -ne "Mirror.dll" -and $_.Name -ne "Qurre.dll"
} | ForEach-Object { "/r:`"$($_.FullName)`"" }

$src = Get-ChildItem (Join-Path $shim "src") -Recurse -Filter *.cs | ForEach-Object { "`"$($_.FullName)`"" }

$args = @("/nologo","/target:library","/unsafe","/out:`"$out\Qurre.dll`"","/nostdlib-") + $refs + $src
Write-Host "Компилирую Qurre-shim ($($src.Count) файлов, $($refs.Count) референсов)..." -ForegroundColor Cyan
& $csc $args
if ($LASTEXITCODE -eq 0 -and (Test-Path "$out\Qurre.dll")) {
    Write-Host ("[OK] Qurre.dll: {0:N0} bytes" -f (Get-Item "$out\Qurre.dll").Length) -ForegroundColor Green
} else {
    Write-Host "[FAIL] сборка с ошибками (см. выше). Чиним по списку." -ForegroundColor Red
    exit 1
}
