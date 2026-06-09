# FYDNE build notes

## What is built

The current recovery build produces two plugin assemblies:

- `plugin/QurreShim/bin/Qurre.dll` - LabAPI compatibility shim that exposes old Qurre namespaces/classes.
- `plugin/Loli/bin/Loli.dll` - recovered FYDNE/Loli plugin compiled against the shim.

The shim output file must be named `Qurre.dll`. The old plugin references an assembly named `Qurre`, so `QurreShim.dll` would not satisfy the legacy assembly reference at runtime.

## Requirements

- Windows.
- Visual Studio 2022 or Visual Studio Build Tools 2022 with the C# compiler.
- Current SCP:SL/LabAPI assemblies copied into `dependencies/`.

The scripts look for `csc.exe` in these places:

- Visual Studio 2022 Community.
- Visual Studio 2022 Professional.
- Visual Studio 2022 Enterprise.
- Visual Studio 2022 BuildTools.
- `vswhere.exe` latest MSBuild installation.
- `PATH`.

## Build and deploy locally

From the repository root:

```powershell
cd "C:\Users\Admin\Downloads\fydne recovery"
powershell -ExecutionPolicy Bypass -File scripts\deploy-local-plugin.ps1 -Build
```

This command runs:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1
powershell -ExecutionPolicy Bypass -File scripts\build-plugin.ps1
```

Then it copies:

- `plugin/QurreShim/bin/Qurre.dll` to `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\global\Qurre.dll`
- `plugin/Loli/bin/Loli.dll` to `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\global\Loli.dll`
- runtime dependencies to `%APPDATA%\SCP Secret Laboratory\LabAPI\dependencies\global`

## Build without deploy

```powershell
cd "C:\Users\Admin\Downloads\fydne recovery"
powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1
powershell -ExecutionPolicy Bypass -File scripts\build-plugin.ps1
```

`Loli.dll` must be built after `Qurre.dll`, because `Loli.dll` references the shim.

## Logs

`build-plugin.ps1` writes the C# compiler output here:

```text
plugin/Loli/bin/build.log
```

The current expected result is:

```text
Loli build: OK; errors: 0
```

Warnings in `QurreShim/src/Structs/Structs.cs` about hidden inherited members are currently expected. They come from legacy Qurre-compatible event classes and do not block the build.

## Common failure reasons

- Opening only `plugin/Loli.sln` does not build QurreShim. The shim is intentionally built by `scripts/build-shim.ps1`.
- Missing Visual Studio C# compiler. Install Visual Studio Build Tools 2022 if `csc.exe` is not found.
- Missing or stale `dependencies/` assemblies. The build must reference the same current SCP:SL/LabAPI assemblies used by the local server.
- Renaming the shim to `QurreShim.dll` breaks legacy assembly resolution. It must deploy as `Qurre.dll`.
