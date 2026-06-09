# Сборка FYDNE

## Что собирается

Текущая recovery-сборка создает две сборки плагинов:

- `plugin/QurreShim/bin/Qurre.dll` - совместимый слой поверх LabAPI, который имитирует старые пространства имен и классы Qurre.
- `plugin/Loli/bin/Loli.dll` - восстановленный основной плагин FYDNE/Loli, который компилируется против shim-версии Qurre.

Файл shim должен называться именно `Qurre.dll`. Старый код `Loli.dll` ссылается на assembly с именем `Qurre`, поэтому файл `QurreShim.dll` не подойдет для runtime-загрузки.

## Требования

- Windows.
- Visual Studio 2022 или Visual Studio Build Tools 2022 с C# compiler.
- Актуальные assembly игры SCP:SL и LabAPI в папке `dependencies/`.

Скрипты автоматически ищут `csc.exe` в следующих местах:

- Visual Studio 2022 Community.
- Visual Studio 2022 Professional.
- Visual Studio 2022 Enterprise.
- Visual Studio 2022 BuildTools.
- Последняя MSBuild-установка через `vswhere.exe`.
- `PATH`.

Если компилятор не найден, нужно поставить Visual Studio Build Tools 2022 или полноценную Visual Studio 2022 с компонентом C#/.NET build tools.

## Быстрая сборка и установка локально

Запустить из корня репозитория:

```powershell
cd "C:\Users\Admin\Downloads\fydne recovery"
powershell -ExecutionPolicy Bypass -File scripts\deploy-local-plugin.ps1 -Build
```

Эта команда делает полный цикл:

1. Собирает `QurreShim` в `Qurre.dll`.
2. Собирает `Loli.dll` против свежего `Qurre.dll`.
3. Копирует оба DLL в папку LabAPI plugins.
4. Копирует runtime-зависимости в папку LabAPI dependencies.

Файлы копируются сюда:

```text
%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\global\Qurre.dll
%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\global\Loli.dll
```

Зависимости копируются сюда:

```text
%APPDATA%\SCP Secret Laboratory\LabAPI\dependencies\global
```

## Сборка без установки в LabAPI

Если нужно только скомпилировать DLL, но не копировать их в папку сервера:

```powershell
cd "C:\Users\Admin\Downloads\fydne recovery"
powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1
powershell -ExecutionPolicy Bypass -File scripts\build-plugin.ps1
```

Порядок важен: сначала собирается `Qurre.dll`, потом `Loli.dll`.

`Loli.dll` зависит от `Qurre.dll`, поэтому если сначала запустить `build-plugin.ps1`, сборка должна упасть с ошибкой о отсутствующем shim-артефакте.

## Почему QurreShim не собирается через Loli.sln

`plugin/Loli.sln` содержит только старый проект `Loli.csproj`.

`QurreShim` сейчас не оформлен как отдельный `.csproj`. Он собирается напрямую через `csc.exe` скриптом:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\build-shim.ps1
```

Это сделано специально для recovery-этапа: shim быстро расширяется под ошибки старого кода, и отдельный скрипт проще поддерживать, чем постоянно чинить старую структуру solution.

## Где смотреть лог сборки

Основной лог компиляции `Loli.dll` пишется сюда:

```text
plugin/Loli/bin/build.log
```

Ожидаемый успешный результат:

```text
Loli build: OK; errors: 0
```

Предупреждения в `plugin/QurreShim/src/Structs/Structs.cs` вида `CS0108` сейчас ожидаемые. Они связаны с тем, что legacy-compatible event-классы повторяют поля базового `EventBase` ради совместимости со старым Qurre-кодом. Эти предупреждения сборку не ломают.

## Частые причины ошибок

### Открыли только `Loli.sln`

Через solution собирается только старый `Loli.csproj`. Shim там не лежит.

Правильная команда:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\deploy-local-plugin.ps1 -Build
```

### Не найден `csc.exe`

Нужно установить Visual Studio 2022 или Visual Studio Build Tools 2022.

Минимально нужен C# compiler/MSBuild.

### Нет папки `dependencies/`

Сборка использует DLL игры, LabAPI и runtime-зависимости из:

```text
dependencies/
```

Если папки нет или в ней старые DLL, сборка может не пройти или плагин может сломаться на runtime.

### Переименовали `Qurre.dll`

Нельзя переименовывать shim в `QurreShim.dll` для установки на сервер.

На диске исходники лежат в папке `QurreShim`, но результат сборки обязан называться:

```text
Qurre.dll
```

Именно это имя ожидает старый FYDNE/Loli-код.

### Собрали `Loli.dll` против старого Qurre

В `dependencies/` может быть старый `Qurre.dll`, но скрипты специально исключают его из reference list и добавляют свежий:

```text
plugin/QurreShim/bin/Qurre.dll
```

Если собирать руками в IDE, нужно проследить, чтобы reference указывал именно на shim-версию, а не на старую оригинальную Qurre DLL.

## Проверка после сборки

После успешного запуска:

```powershell
powershell -ExecutionPolicy Bypass -File scripts\deploy-local-plugin.ps1 -Build
```

в конце должно быть примерно так:

```text
[OK] Qurre.dll: ...
Loli build: OK; errors: 0
[OK] Deployed Qurre.dll and Loli.dll to ...\LabAPI\plugins\global
[OK] Deployed runtime dependencies to ...\LabAPI\dependencies\global
```

После этого нужно полностью перезапустить SCP:SL dedicated server/LocalAdmin. Уже запущенный сервер новые DLL не подхватит.
