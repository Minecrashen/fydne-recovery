# Манифест зависимостей плагина `Loli-Merged`

Это **главный блокер сборки**. Без этих DLL проект не компилируется.
Все пути `<HintPath>` в оригинальном `.csproj` ведут на машину разработчика
(`..\..\Qurre-sl\Qurre\depedencies\`) — их нужно заменить на локальную папку `dependencies/`.

## Что требует `.csproj` (фактические Reference)

| Сборка | Откуда брать | В репо? | Статус |
|---|---|---|---|
| `Assembly-CSharp_public.dll` | Спубличить из `Assembly-CSharp.dll` (см. ниже) | ❌ | 🔴 генерировать |
| `Assembly-CSharp-firstpass.dll` | Установка SCP:SL сервера | ✅ (v13) | 🟡 нужна v14 |
| `LabApi.dll` | Официальный релиз Northwood (NuGet/GitHub) | ❌ | 🔴 скачать |
| `Qurre.dll` | Билд под v14 (см. примечание) | ⚠️ старый 1.14.2 | 🔴 главная проблема |
| `QurreSocket.dll` | Есть в архиве плагинов | ✅ | 🟢 |
| `0Harmony.dll` | Установка сервера / NuGet `Lib.Harmony` | ✅ | 🟢 |
| `CommandSystem.Core.dll` | Установка сервера | ✅ | 🟢 |
| `Mirror.dll` / `Mirror_public.dll` | Установка сервера (Mirror — публичить) | частично | 🟡 |
| `Mirror.Components.dll` | Установка сервера | — | 🟡 |
| `NorthwoodLib.dll` | Установка сервера | ✅ | 🟢 |
| `Pooling.dll` | Установка сервера | ❌ | 🟡 из v14 |
| `Newtonsoft.Json.dll` | NuGet | ✅ | 🟢 |
| `SchematicUnity.dll` | Есть в архиве плагинов | ✅ | 🟢 |
| `SCPLogs.dll` | Внутренняя сборка FYDNE | ❌ | 🔴 нет исходника |
| `Unity.TextMeshPro.dll` | Установка сервера (Managed) | — | 🟡 |
| `UnityEngine*.dll` | Установка сервера (Managed) | — | 🟢 |
| `MEC` (`Assembly-CSharp` содержит Timing) | Установка сервера | — | 🟢 |

## Где лежат игровые DLL после установки сервера

SteamCMD: `app_update 996560` → каталог сервера →
`SCPSL_Data/Managed/*.dll` — здесь UnityEngine, Mirror, NorthwoodLib, CommandSystem.Core,
Pooling, 0Harmony, Assembly-CSharp(-firstpass), Unity.TextMeshPro и т.д.

## Как спубличить Assembly-CSharp (нужна `_public` версия)

Плагин ссылается на `Assembly-CSharp_public.dll` — это версия с публичными
внутренними членами (нужна, потому что патчатся `private`/`internal` методы игры).

Варианты:
1. **NwPluginAPI Publicizer** / `Publicizer` MSBuild-таска.
2. **AssemblyPublicizer** (CLI): `AssemblyPublicizer.exe Assembly-CSharp.dll` → `Assembly-CSharp_publicized.dll`, переименовать в `Assembly-CSharp_public.dll`.
3. EXILED/LabAPI SDK имеют встроенный publicizer — можно взять оттуда.

Аналогично нужен `Mirror_public.dll` (публичный Mirror).

## ⚠️ Проблема Qurre под v14

Старые `Qurre.dll` из архива (1.10–1.14.2, `net472`) написаны под игру эпохи EXILED/NWAPI
и **не совпадут по API** с тем, что использует `Loli-Merged` (он уже на `LabApi.Features.Wrappers`).

Три пути:
1. Найти/получить у основателя точную сборку Qurre, под которую писался `Loli-Merged`.
2. Собрать Qurre из исходников под v14 (если доступны исходники Qurre-sl).
3. **Рекомендация на дистанции:** мигрировать FYDNE с Qurre на чистый LabAPI — снимает
   single-maintainer-риск и лицензионный долг CC BY-SA. Дорого разово, дёшево в поддержке.

## SCPLogs.dll — отдельная боль

`SCPLogs.dll` — внутренняя сборка FYDNE (логирование), исходника в репозиториях нет.
Используется в `Logs/`. Варианты: получить у основателя, либо вырезать/заглушить
зависимость (логирование не критично для геймплея).

## Итоговая папка зависимостей

Создать `dependencies/` рядом с `.csproj`, сложить туда все DLL, и в `.csproj`
заменить все `<HintPath>..\..\Qurre-sl\...` на `<HintPath>dependencies\...`.
Папка `dependencies/` уже в `.gitignore` (DLL не коммитим — каждый собирает локально).
