# Состояние сборочного окружения

Что уже развёрнуто на машине (Windows, `C:\Users\Admin`). Фиксируем, чтобы не настраивать заново.

## ✅ Готово

| Компонент | Путь / детали |
|---|---|
| Выделенный сервер SCP:SL | `C:\Users\Admin\fydne_build\scpsl-server` (SteamCMD, app 996560) |
| Игровые DLL (144 шт) | `…\scpsl-server\SCPSL_Data\Managed` |
| **LabApi.dll** | версия **1.1.7.0** (в Managed и в `dependencies/`) |
| SteamCMD | `C:\Users\Admin\fydne_build\steamcmd` |
| Компилятор | VS2022 Community: MSBuild + `csc.exe` (Roslyn) |
| Папка зависимостей | `dependencies/` — 147 DLL (игра + LabApi + 0Harmony + QurreSocket + SchematicUnity) |
| Исходник плагина | `plugin/Loli` (196 .cs, копия Loli-Merged) |
| **Qurre-shim** | `plugin/QurreShim` — enums + атрибуты, **компилируется в Qurre.dll** ✅ |
| Build-харнесс | `scripts/build-shim.ps1` (csc против dependencies/) |

## ⚠️ Отложено / нужно доделать

| Задача | Статус | Как |
|---|---|---|
| net48 targeting pack | absent (есть 4.7.1) | для csc не критично; для MSBuild — доustanovить или target net472 |
| Publicized `Assembly-CSharp_public.dll` | нет | dotnet SDK отсутствует → ставить SDK ИЛИ публичить через Mono.Cecil-скрипт |
| `Mirror_public.dll` | нет | то же |
| dotnet **SDK** | только runtime 8.0.11 | для dotnet-tools (публичайзер) нужен SDK |

## Как пересобрать shim (цикл миграции)
```powershell
powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1
```

## Как обновить игровые DLL (после апдейта игры)
```powershell
C:\Users\Admin\fydne_build\steamcmd\steamcmd.exe +force_install_dir C:\Users\Admin\fydne_build\scpsl-server +login anonymous +app_update 996560 validate +quit
powershell -ExecutionPolicy Bypass -File scripts\gather-dependencies.ps1 -ServerPath C:\Users\Admin\fydne_build\scpsl-server
```

## Важно про версии
LabApi **1.1.7** — это версия с текущего live-сервера (последняя). Плагин писался под v14.x.
Если при сборке вылезут расхождения API LabApi — это ожидаемо, чиним по ошибкам компилятора.
