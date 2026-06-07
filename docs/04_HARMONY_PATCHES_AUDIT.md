# Аудит Harmony-патчей `Loli-Merged`

Полный список из **51 патча** (извлечён из кода). `PatchAll()` применяет их пачкой —
**любой упавший патч роняет загрузку всего плагина**. Чинить по приоритету.

Хрупкость:
- 🔴 **HIGH** — транспайлер ИЛИ патч на часто меняющиеся игровые internals (SCP-механики, боевка, голос, сеть). Сломается почти наверняка.
- 🟡 **MED** — патч на относительно стабильный API (команды, роли, эффекты).
- 🟢 **LOW** — патч на стабильные служебные методы (логи, консоль).

`T` = использует Transpiler (особо хрупко — рассинхрон стека → `InvalidProgramException`).

## Таблица патчей

| Файл | Цель (тип.метод) | T | Хрупк. | Назначение / действие |
|---|---|---|---|---|
| `Addons/RealisticArmory.cs` | `ItemBase.OnAdded/OnRemoved`, `FirearmDamageHandler.ProcessDamage` | T | 🔴 | Реалистичная броня/урон → мигрировать на LabAPI `Hurting`/инвентарь-события |
| `Addons/RolePlay/Patches/Anti079Recontain.cs` | `Scp079Recontainer.OnServerRoleChanged` | T | 🔴 | Блок реконтейна 079 → LabAPI Scp079-событие |
| `Addons/RolePlay/Patches/Scp173Teleport.cs` | `Scp173BlinkTimer.AbilityReady` (getter) | | 🔴 | Рероворк 173 (телепорт) |
| `Addons/RolePlay/Patches/ScpSpeaks.cs` | `VoiceTransceiver.ServerReceiveMessage` | T | 🔴 | Голос SCP → ⚠️ конфликт с `FixSpoiled.cs` (та же цель!) |
| `Patches/FixSpoiled.cs` | `VoiceTransceiver.ServerReceiveMessage` | | 🔴 | ⚠️ **двойной патч одного метода** — проверить порядок/приоритет |
| `Patches/AntiCringeUpdates.cs` | `Scp3114Strangle.ServerProcessCmd/ServerUpdateTarget`, `Scp3114Spawner.OnPlayersSpawned`, `CandyPink.ServerApplyEffects` | T | 🔴 | Отключение/правка SCP-3114 (скелет) |
| `Patches/AntiNewYear.cs` | `Scp956Pinata.Update`, `Scp2536GiftController.TryGrantWeapon`, `Scp2536Controller.Awake` | T | 🔴 | Отключение НГ-ивентов (**маркер версии v14**) |
| `Patches/DeleteDeadman.cs` | `DeadmanSwitch.OnUpdate` | T | 🔴 | Отключение dead-man switch |
| `Patches/FixPockerEscape.cs` | `Scp106PocketExitFinder.GetBestExitPosition` | T | 🔴 | Фикс выхода из кармана 106 |
| `Patches/FixNetCrash.cs` | `NetPacket.Verify` | | 🔴 | Анти-краш сети (Mirror internals) |
| `Modules/Voices/_DisableCassie.cs` | `NineTailedFoxAnnouncer.Update`, `Scp079Recontainer.PlayAnnouncement` | T | 🔴 | Отключение/замена Cassie |
| `Scps/Scp294/Patch.cs` | `Scp207.OnEffectsActivated` | | 🟡 | SCP-294 / эффект 207 |
| `DataBase/Customize.cs` | `Adrenaline.OnEffectsActivated` | | 🟡 | Кастомизация эффекта адреналина |
| `Patches/DisableGunAudio.cs` | `FirearmExtensions.ServerSendAudioMessage` | | 🟡 | Отключение звука оружия |
| `HintsCore/Fixer/Patch.cs` | `Client.ShowHint` | | 🟡 | Кастомная система хинтов (ядро) |
| `Patches/HideRaAuth.cs` | (string-target) | T | 🟡 | Скрытие RA-аутентификации |
| `Patches/GetVerkey.cs` | `ServerConsole.RefreshToken` | | 🟡 | Перехват verkey сервера |
| `Patches/HideTagPatch.cs` | `ServerRoles.TryHideTag/RefreshLocalTag` | | 🟡 | Скрытие тегов |
| `Patches/TEMP_FIX_RICH.cs` | `Misc.SanitizeRichText` | | 🟡 | ⚠️ помечен `TEMP_FIX` — известный техдолг |
| `Patches/FixBackdoors/VeryHigh.cs` | `ContactCommand`, `RconCommand`, `ValueCommand`, `PathCommand`, `StopNextRoundCommand`, `TerminateUnconnectedCommand` `.Execute` | | 🟡 | **Анти-бэкдор**: блок опасных RA-команд |
| `Patches/FixBackdoors/High.cs` | `VersionCommand`, `BuildInfoCommand`, `PingCommand` `.Execute` | | 🟡 | Анти-бэкдор / анти-лик инфо |
| `Patches/FixBackdoors/Low.cs` | `RAConfigCommand`, `PermCommand`, `KeyCommand`, `DisableCoverCommand`, `EnableCoverCommand`, `GrantCommand`, `RevokeCommand`, `SetTagCommand`, `SetColorCommand`, `RefreshCommandsCommand`, `SrvCfgCommand`, `IpCommand` `.Execute` | | 🟡 | Анти-бэкдор (12 команд) |
| `Patches/NotFileLogs.cs` | `ServerLogs.StartLogging/AddLog/AppendLog` | T | 🟢 | Отключение файловых логов |
| `Patches/FixDelayed.cs` | `ServerConsole.AddLog` | | 🟢 | Фикс отложенного лога |

## Ключевые находки

1. **⚠️ Двойной патч `VoiceTransceiver.ServerReceiveMessage`** в `ScpSpeaks.cs` и `FixSpoiled.cs`.
   При сборке проверить, что приоритеты/порядок не конфликтуют (иначе один перекроет другой
   или сломается голосовой конвейер).

2. **`TEMP_FIX_RICH.cs`** — уже в названии маркер временного костыля. Кандидат на переделку.

3. **Транспайлеры (12 файлов)** — самый высокий риск. Многие просто вырезают тело метода
   (`yield return Ret`). Стратегия фикса ниже.

## Стратегия починки (по приоритету)

### Приоритет 1 — заставить `PatchAll()` не падать
Обернуть применение в безопасный режим, чтобы один битый патч не ронял всё:
```csharp
var harmony = new Harmony("fydne.loli");
foreach (var type in typeof(Core).Assembly.GetTypes())
{
    try { harmony.CreateClassProcessor(type).Patch(); }
    catch (Exception e) { Log.Error($"Patch failed in {type.Name}: {e.Message}"); }
}
```
Это превращает «плагин не грузится» в «грузится, но часть фич отключена + видно что чинить».

### Приоритет 2 — транспайлеры-заглушки → префиксы
Грубый транспайлер `yield return Ret` заменить на префикс — устойчивее к смене сигнатур:
```csharp
// было: [HarmonyTranspiler] static IEnumerable<CodeInstruction> Call(...) { yield return new CodeInstruction(OpCodes.Ret); }
// стало:
[HarmonyPrefix] static bool Prefix() => false;  // false = пропустить оригинал
```
См. готовый паттерн в `patches/transpiler-to-prefix.md`.

### Приоритет 3 — критичные игровые патчи на события LabAPI
`FirearmDamageHandler`, `VoiceTransceiver`, `Scp079Recontainer`, `Scp106PocketExitFinder` —
по возможности заменить прямой патч на штатное событие LabAPI/EXILED (урон, голос, реконтейн).
Меньше IL, меньше поломок на апдейтах.

### Приоритет 4 — версионно-зависимый контент
`Scp3114*`, `Scp956Pinata`, `Scp2536*`, `DeadmanSwitch` — проверить, что типы/сигнатуры
существуют в целевой v14. Если игра ушла дальше — переписать под новые имена.
