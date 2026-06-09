param([switch]$Census)

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
$loli = Join-Path $root "plugin\Loli"
$deps = Join-Path $root "dependencies"
$shim = Join-Path $root "plugin\QurreShim\bin\Qurre.dll"
$out = Join-Path $root "plugin\Loli\bin"
$csc = Find-Csc

New-Item -ItemType Directory -Force -Path $out | Out-Null

if (-not (Test-Path $shim)) {
    throw "Missing QurreShim artifact: $shim. Run scripts\build-shim.ps1 first."
}
if (-not (Test-Path $deps)) {
    throw "Missing dependencies directory: $deps. Copy game/LabAPI assemblies into dependencies first."
}

# Prefer Assembly-CSharp_public.dll when present. Referencing both public and original
# Assembly-CSharp creates duplicate assembly identity errors.
$hasAsmPublic = Test-Path (Join-Path $deps "Assembly-CSharp_public.dll")
$refs = Get-ChildItem $deps -Filter *.dll | Where-Object {
    $_.Name -ne "Qurre.dll" -and
    -not ($hasAsmPublic -and $_.Name -eq "Assembly-CSharp.dll")
} | ForEach-Object { "/r:`"$($_.FullName)`"" }
$refs += "/r:`"$shim`""

$src = Get-ChildItem $loli -Recurse -Filter *.cs | ForEach-Object { "`"$($_.FullName)`"" }

# Build-time aliases for legacy Qurre-style unqualified types used by the old plugin.
$globals = Join-Path $root "plugin\_PluginGlobals.cs"
if (Test-Path $globals) { $src += "`"$globals`"" }

$args = @(
    "/nologo",
    "/target:library",
    "/unsafe",
    "/define:NR;TRACE;FYDNE_SKIP_LEGACY_PATCHES",
    "/langversion:12",
    "/out:`"$out\Loli.dll`""
) + $refs + $src

$log = Join-Path $out "build.log"
Write-Host "Compiling Loli.dll ($($src.Count) source files, $($refs.Count) references)..." -ForegroundColor Cyan
Write-Host "Compiler: $csc" -ForegroundColor DarkGray
& $csc $args 2>&1 | Out-File $log -Encoding utf8
$ok = $LASTEXITCODE -eq 0

$errs = Select-String -Path $log -Pattern ": error CS" | ForEach-Object { $_.Line }
Write-Host ("Loli build: {0}; errors: {1}; log: {2}" -f ($(if ($ok) { "OK" } else { "FAIL" })), $errs.Count, $log) -ForegroundColor $(if ($ok) { "Green" } else { "Yellow" })

if ($Census) {
    Write-Host "`n=== TOP error codes ===" -ForegroundColor Cyan
    $errs | ForEach-Object { ([regex]::Match($_, 'error (CS\d+)')).Groups[1].Value } |
        Group-Object | Sort-Object Count -Descending | Select-Object Count,Name -First 12 | Format-Table -AutoSize

    Write-Host "=== TOP missing identifiers ===" -ForegroundColor Cyan
    $errs | ForEach-Object { ([regex]::Match($_, [char]0x27 + '([^' + [char]0x27 + ']+)' + [char]0x27)).Groups[1].Value } |
        Where-Object { $_ } | Group-Object | Sort-Object Count -Descending | Select-Object Count,Name -First 30 | Format-Table -AutoSize
}

if (-not $ok) { exit 1 }
exit 0
