param()

$ErrorActionPreference = "Stop"

function Find-Csc {
    $known = @(
        "C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\Roslyn\csc.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\Roslyn\csc.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\Roslyn\csc.exe",
        "C:\Program Files\Microsoft Visual Studio\2022\BuildTools\MSBuild\Current\Bin\Roslyn\csc.exe"
    )

    foreach ($path in $known) {
        if (Test-Path $path) { return $path }
    }

    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    if (Test-Path $vswhere) {
        $install = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
        if ($install) {
            $path = Join-Path $install "MSBuild\Current\Bin\Roslyn\csc.exe"
            if (Test-Path $path) { return $path }
        }
    }

    $cmd = Get-Command csc.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }

    throw "csc.exe was not found. Install Visual Studio 2022, Visual Studio Build Tools 2022, or add csc.exe to PATH."
}

$root = Split-Path -Parent $PSScriptRoot
$shim = Join-Path $root "plugin\QurreShim"
$deps = Join-Path $root "dependencies"
$out = Join-Path $shim "bin"
$csc = Find-Csc

if (-not (Test-Path $deps)) {
    throw "Missing dependencies directory: $deps. Copy game/LabAPI assemblies into dependencies first."
}

New-Item -ItemType Directory -Force -Path $out | Out-Null

# Prefer Assembly-CSharp_public.dll when present. Referencing both public and original
# Assembly-CSharp creates duplicate assembly identity errors.
$hasAsmPublic = Test-Path (Join-Path $deps "Assembly-CSharp_public.dll")
$refs = Get-ChildItem $deps -Filter *.dll | Where-Object {
    $_.Name -ne "Qurre.dll" -and
    -not ($hasAsmPublic -and $_.Name -eq "Assembly-CSharp.dll")
} | ForEach-Object { "/r:`"$($_.FullName)`"" }

$src = Get-ChildItem (Join-Path $shim "src") -Recurse -Filter *.cs | ForEach-Object { "`"$($_.FullName)`"" }

$args = @(
    "/nologo",
    "/target:library",
    "/unsafe",
    "/out:`"$out\Qurre.dll`"",
    "/nostdlib-"
) + $refs + $src

Write-Host "Compiling QurreShim -> Qurre.dll ($($src.Count) source files, $($refs.Count) references)..." -ForegroundColor Cyan
Write-Host "Compiler: $csc" -ForegroundColor DarkGray
& $csc $args

if ($LASTEXITCODE -eq 0 -and (Test-Path "$out\Qurre.dll")) {
    Write-Host ("[OK] Qurre.dll: {0:N0} bytes" -f (Get-Item "$out\Qurre.dll").Length) -ForegroundColor Green
    exit 0
}

Write-Host "[FAIL] QurreShim build failed. Check compiler errors above." -ForegroundColor Red
exit 1
