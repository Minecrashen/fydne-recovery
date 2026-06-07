# build-plugin.ps1 — компиляция плагина Loli против Qurre-shim'а (флавор NR).
# Это инструмент «переписи ошибок»: показывает, чего ещё не хватает в shim'е.
# Запуск:  powershell -ExecutionPolicy Bypass -File scripts\build-plugin.ps1
param([switch]$Census)

$ErrorActionPreference = "Stop"
$root  = Split-Path -Parent $PSScriptRoot
$loli  = Join-Path $root "plugin\Loli"
$deps  = Join-Path $root "dependencies"
$shim  = Join-Path $root "plugin\QurreShim\bin\Qurre.dll"
$out   = Join-Path $root "plugin\Loli\bin"
$csc   = "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe"
New-Item -ItemType Directory -Force -Path $out | Out-Null

if (-not (Test-Path $shim)) { throw "Сначала собери shim: scripts\build-shim.ps1" }

$hasAsmPublic = Test-Path (Join-Path $deps "Assembly-CSharp_public.dll")
$refs = Get-ChildItem $deps -Filter *.dll | Where-Object {
    $_.Name -ne "Qurre.dll" -and
    -not ($hasAsmPublic -and $_.Name -eq "Assembly-CSharp.dll")
} | ForEach-Object { "/r:`"$($_.FullName)`"" }
$refs += "/r:`"$shim`""

$src = Get-ChildItem $loli -Recurse -Filter *.cs | ForEach-Object { "`"$($_.FullName)`"" }

$args = @("/nologo","/target:library","/unsafe","/define:NR;TRACE","/langversion:12",
          "/out:`"$out\Loli.dll`"") + $refs + $src
$log = Join-Path $out "build.log"
& $csc $args 2>&1 | Out-File $log -Encoding utf8
$ok = $LASTEXITCODE -eq 0

$errs = Select-String -Path $log -Pattern ": error CS" | ForEach-Object { $_.Line }
Write-Host ("Сборка плагина: {0}; ошибок: {1}" -f ($(if($ok){"OK"}else{"FAIL"})), $errs.Count) -ForegroundColor $(if($ok){"Green"}else{"Yellow"})

if ($Census) {
    Write-Host "`n=== TOP error codes ===" -ForegroundColor Cyan
    $errs | ForEach-Object { ([regex]::Match($_, 'error (CS\d+)')).Groups[1].Value } |
        Group-Object | Sort-Object Count -Descending | Select-Object Count,Name -First 12 | Format-Table -AutoSize
    Write-Host "=== TOP missing identifiers ===" -ForegroundColor Cyan
    $errs | ForEach-Object { ([regex]::Match($_, [char]0x27 + '([^' + [char]0x27 + ']+)' + [char]0x27)).Groups[1].Value } |
        Where-Object { $_ } | Group-Object | Sort-Object Count -Descending | Select-Object Count,Name -First 30 | Format-Table -AutoSize
}
