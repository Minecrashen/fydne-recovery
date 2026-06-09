# FYDNE Recovery — TODO

Живой список задач. Отмечай `[x]`, коммить, пушь. Подробности — в `docs/`.
Координация между агентами — `agent-exchange/SHARED_LOG.md`.

Статус на 2026-06-08: стенд развёрнут, shim компилируется, плагин — **0 compile errors**.
Артефакты созданы: `plugin/QurreShim/bin/Qurre.dll` и `plugin/Loli/bin/Loli.dll`.
Важно: это первый compile-pass, а не runtime-pass. Следующий этап — загрузка на тестовый SCP:SL/LabAPI сервер, проверка старта, логов, event wiring, Harmony patches и отключенных legacy-заглушек.

---

## 2026-06-09 Codex pass: pre-test spawn bridge hardening

- [x] Found a static bridge bug: `SpawnEvent` and `ChangeRoleEvent` were dispatched twice, once from LabAPI pre-events (`Spawning`/`ChangingRole`) and again from post-events (`Spawned`/`ChangedRole`).
- [x] Disabled post-dispatch by default in `plugin/QurreShim/src/EventMap.cs`.
  - Legacy FYDNE handlers now run on pre-events where `Position`, `Role`, and `Allowed` can still be applied back to LabAPI.
  - Old double-dispatch behavior is still available for comparison with `FYDNE_DISPATCH_POST_ROLE_EVENTS=1`.
- [x] Added `FYDNE_DISPATCH_POST_ROLE_EVENTS=0` to `.env.example`.
- [x] Added guard/diagnostic logs for custom spawn zones:
  - `AdminRoom` logs computed waiting/tutorial spawn points and skips forced `(0,0,0)` spawn if the room was not initialized.
  - `Range` logs chaos/donate/guard spawn corrections and warns if the schematic/vent room is missing.
  - New log prefix: `FYDNE-BUILD`.
- [x] Rebuilt and deployed local plugin binaries:
  - `scripts/build-shim.ps1` -> OK, warnings only.
  - `scripts/build-plugin.ps1` -> OK, 0 errors.
  - `scripts/deploy-local-plugin.ps1` -> OK.
  - deployed `Loli.dll` SHA256: `7A916DD377CC9C3240965FDA93E368A95FB06B7C2CBF234E6B19C1CD49C1E79A`.
  - deployed `Qurre.dll` SHA256: `90BB058FDCB0D538C26CEF1E2F730A0091A7E1C2DB4FB35C9D5183072D2BE551`.

Next test additions:
- [ ] In LocalAdmin logs also inspect `FYDNE-BUILD`.
- [ ] Confirm each spawn has only one `SpawnEvent begin` and one `ChangeRoleEvent begin` unless `FYDNE_DISPATCH_POST_ROLE_EVENTS=1` is explicitly enabled.
- [ ] If waiting/tutorial spawn is still in the air, use `FYDNE-BUILD AdminRoom loaded waiting=... tutorial=...` to decide whether to keep, move, or disable the legacy admin waiting room.

## 2026-06-09 Codex pass: runtime trace instrumentation

- [x] Added QurreShim event tracing in `Qurre.API.Core.Dispatch<T>()`.
  - Recovery mode traces high-risk events by default: round lifecycle, join/leave, spawn/role, commands, doors/lifts/generators, SCP-106 attack, alpha warhead.
  - `FYDNE_TRACE_EVENTS=1` traces every bridged event.
  - `FYDNE_TRACE_SPAWN=1` forces focused `SpawnEvent`/`ChangeRoleEvent` tracing.
  - `FYDNE_TRACE_EVERY_HANDLER=1` logs every handler even when it does not mutate the event.
  - Trace records event begin/end state and per-handler diffs for fields like `Player`, `Role`, `OldRole`, `Position`, `Allowed`, `Reason`, `Message`, `Args`, `Damage`, `Door`, `Generator`, `Lift`, etc.
- [x] Added socket tracing in `Loli.Core.SafeSocket`.
  - Logs socket client creation/disabled state, subscriptions, incoming events and outgoing emits.
  - `FYDNE_TRACE_SOCKET=1` enables socket trace; recovery mode enables safe socket trace by default unless set to `0`.
  - `FYDNE_TRACE_SOCKET_PAYLOADS=1` prints bounded payloads; sensitive events such as `SCPServerInit`/token/auth/password are redacted.
  - Socket callback exceptions are now logged with event names instead of silently breaking the callback path.
- [x] Added boot diagnostics: recovery/socket trace flags on enable, disable/restart notice, and Harmony patch pass `patched/skipped` summary.
- [x] Added diagnostic env flags to `.env.example`.
- [x] Rebuilt and deployed local plugin binaries:
  - `scripts/build-shim.ps1` -> OK, warnings only.
  - `scripts/build-plugin.ps1` -> OK, 0 errors.
  - `scripts/deploy-local-plugin.ps1` -> OK.
  - deployed `Loli.dll` SHA256: `DBE96EC43414E164B7DD2B3BB5D02F4EA9C4B3587481E150E256A5B855564747`.
  - deployed `Qurre.dll` SHA256: `6F09FDCA5341C67DD5BD54AA774D6333BE8A679B7049C95AC7A39C7D20A8EE00`.

Next test:
- [ ] Start LocalAdmin with `FYDNE_RECOVERY_MODE=1`.
- [ ] For focused crash debugging, set `FYDNE_TRACE_SPAWN=1`, `FYDNE_TRACE_SOCKET=1`, keep `FYDNE_TRACE_SOCKET_PAYLOADS=0`.
- [ ] If the crash/kick is still unclear, temporarily set `FYDNE_TRACE_EVENTS=1` and `FYDNE_TRACE_EVERY_HANDLER=1`, reproduce once, then turn them back off because logs will be huge.
- [ ] After a crash/restart, inspect LocalAdmin logs for `FYDNE-TRACE`, `FYDNE-SOCKET`, `SpawnEvent`, `ChangeRoleEvent`, `RoundStartEvent`, handler diffs and first exception after the last trace line.

## 2026-06-09 Codex pass: live spawn disconnect loop

- [x] Fresh LocalAdmin logs inspected. The newest available log still showed the old deployed build before `FixOnePrimitiveSmoothing`: more than 37k `RuntimeBinderException` entries from `Loli.FixOnePrimitiveSmoothing.Update`.
- [x] Confirmed deployed `Loli.dll` was rebuilt after the smoothing fix, then rebuilt again after the FastReconnect guard.
- [x] Found a likely cause of the player's "kick/reconnect after a few seconds": `Fixes.CheckPlayersPing()` uses `Player.LastSynced` and calls `FastReconnect.Process()`, which stores player state, sets position to `Vector3.zero`, sets spectator, then calls `pl.Client.Reconnect()`.
- [x] Disabled the old FastReconnect ping heuristic in `Core.RecoveryMode`:
  - `Loli.Modules.Fixes.CheckPlayersPing()` returns immediately in recovery mode.
  - `Loli.Addons.FastReconnect.Join()` and `FastReconnect.Process()` return immediately in recovery mode.
- [x] Rebuilt and deployed current local plugin:
  - `scripts/build-plugin.ps1` -> OK, 0 errors.
  - `scripts/deploy-local-plugin.ps1` -> OK.
  - deployed `Loli.dll` SHA256: `DA0873FFD6BF593127FF0AEEA5516F8950764AC449E80FA1F16B69BC98E0F4F6`.

Next test:
- [ ] Start backend if socket mode is used: `scripts/start-fydne-socket.ps1`.
- [ ] Start LocalAdmin with `FYDNE_RECOVERY_MODE=1`.
- [ ] Join `127.0.0.1:7777`, force-start, wait at least 30 seconds after spawn.
- [ ] If disconnect persists, inspect the new log for `FastReconnect`, `LastSynced`, `SpawnEvent`, `ChangeRoleEvent`, `AdminRoom`, `Range`, `NullReferenceException`, `MissingMethodException`, and `RuntimeBinderException`.

Next code targets if the disconnect is fixed but spawn is still wrong:
- [ ] Add recovery-mode spawn tracing for `SpawnEvent`/`ChangeRoleEvent`: original role/position, final role/position, handler source.
- [ ] Harden `AdminRoom` waiting spawn against missing/partial custom geometry.
- [ ] Audit hardcoded out-of-map rooms: `AdminRoom`, `Range`, `Gate3`, `Bashni`, `RadarLoc`, `VertPlace`, `NuclearAttack`, `Scp008`, `Hackers`.

## 2026-06-09 Codex pass: detailed architecture documents

- [x] Added `docs/10_RUNTIME_EXECUTION_MAP.md`: full runtime flow from LabAPI bootstrap to QurreShim, Loli enable, waiting, join, spawn, round start/end, socket, notifications, and current recovery branches.
- [x] Added `docs/11_MODULE_CATALOG.md`: module-by-module catalog for `Core`, `Modules`, `Builds`, `Spawns`, `DataBase`, `Addons`, `Concepts`, `Scps`, `Patches`, `HintsCore`, `Logs`, `Webhooks`, and controllers.
- [x] Added `docs/12_EVENT_COMMAND_INDEX.md`: static index for 398 `[EventMethod]` handlers, command registrations, socket events, Harmony patch groups, and high-risk mutators.
- [x] Linked the new detailed documents from `docs/FYDNE_ARCHITECTURE.md`.

## 🔥 P0 — Миграция плагина Qurre→LabAPI (почти готово)

Цель: довести `scripts\build-plugin.ps1 -Census` до **0 ошибок**.
**Прогресс: 887 → 0 compile errors; скрытый слой после отключения legacy patches: ~1188 → 0.** Compile-pass завершен.
✅ Решено: event-структуры, Player+под-объекты, Map/Round/Server, контроллеры, **движок построек
Models на родных LabAPI AdminToys** (Model/Primitive/LightPoint — БЕЗ SchematicUnity/основателя),
SchematicUnity.API-загрузчик (каркас), Audio/Classification, Client, Newtonsoft+Harmony2.x.

### ✅ Compile-pass закрыт: 0 ошибок

Codex-сессия 2026-06-08:
- [x] Синхронизация состояния после interruption по лимитам: TODO и shared log обновлены, код не менялся.
- [x] Добавлен `FYDNE_SKIP_LEGACY_PATCHES` в `scripts/build-plugin.ps1`.
- [x] Отключены/обернуты проблемные legacy Harmony patches, которые ссылались на удаленные/приватные v14 методы игры.
- [x] Сильно расширен `plugin/QurreShim`: `Player` subobjects, `Effects`, `Administrative`, `StatsInformation`, `Models/Schematic`, `Room/Door/WorkStation`, `Core.InjectEventMethod(MethodInfo)`, `Round`, `Server`, global compat helpers.
- [x] Удалены реальные Discord webhook URL из измененных/найденных файлов, заменены на env-переменные `FYDNE_WEBHOOK_*`.
- [x] Добиты последние 25 compile errors до 0:
  typed `CreatePickupEvent.Inventory`, `ItemType.GetCategory`, `Inventory.Items` compat wrapper, `EffectControllerW`,
  `RoundSummary.RpcShowRoundSummary` stub, `DoorVariant` as `GameObject`, `Corpse.Scale/Owner`, `ItemSerial()` helper,
  `RealisticArmory` private `IsLocalPlayer` guard under `FYDNE_SKIP_LEGACY_PATCHES`.
- [x] Первый offline runtime-pass по загрузке и событиям:
  `QurreBootstrap` теперь грузит соседние DLL из папки `Qurre.dll` перед сканированием, чтобы `Loli.dll`
  попадал в `AppDomain`; `Core` корректно регистрирует `[EventMethod]` без параметров и с одним параметром.
- [x] `EventMap.PopulateEnumMap()` заполнен для всех legacy enum Qurre; добавлены отдельные shim-типы
  `RoundWaitingEvent/RoundStartEvent/RoundEndEvent/RoundRestartEvent/RoundForceStartEvent`,
  `AlphaDetonateEvent`, `LczDecontaminationEvent`, `Scp079RecontainEvent`.
- [x] `EventMap.WireLabApi()` частично реализован: round lifecycle/check/force-start, player join/leave/spawn/role/death/damage,
  doors, pickup/drop/change item, escape, cuffs, ban/kick/reports/RA-list, map pickup/decon/door damage/lock,
  alpha warhead, Scp914/173/096/079/049/106 и effect updating.
- [x] Command bridge pass: LabAPI `CommandType.RemoteAdmin` → `RemoteAdminCommandEvent`,
  `CommandType.Client/Console` → `GameConsoleCommandEvent`; заполняются `Player/Sender/Name/Args`,
  `Reply` возвращается через `RaReply`/`Respond`.
- [x] Item/radio bridge pass: `UseItem/UsedItem` подключены через `UsableItem`, `UsingRadio` через `RadioItem/Drain`
  с обратной записью `Allowed/Drain`.
- [ ] Не считать runtime готовым: workstation, точная generator/locker/corpse семантика и часть SCP/map событий
  требуют следующего bridge-pass и живого smoke-test.
- [ ] Следующий шаг: runtime smoke-test на локальном SCP:SL/LabAPI сервере.

### Исторический слой: 12 Harmony-ошибок
Ровно то, что предсказал аудит (патчи на внутренности игры ломаются на апдейтах).

**5× CS0122 (приватные — нужен publicized Assembly-CSharp):**
- [ ] `FirearmDamageHandler.ProcessDamage` (RealisticArmory.cs)
- [ ] `NetPacket` ×3 (FixNetCrash.cs)
- [ ] `Scp207.OnEffectsActivated` (Scps/Scp294 Patch.cs)

**7× CS0117 (метод переименован/приватен в v14):**
- [ ] `VoiceTransceiver.ServerReceiveMessage` ×2 (ScpSpeaks.cs, FixSpoiled.cs)
- [ ] `ServerLogs.StartLogging` (NotFileLogs.cs)
- [ ] `ServerConsole.RefreshToken` (GetVerkey.cs)
- [ ] `Scp3114Strangle.ServerUpdateTarget` (AntiCringeUpdates.cs)
- [ ] `Scp079Recontainer.PlayAnnouncement` (_DisableCassie.cs)
- [ ] `DeadmanSwitch.OnUpdate` (DeleteDeadman.cs)
- [ ] `CommandProcessor.CheckPermissions`

**Два пути закрыть эти 12 (выбрать):**
- [ ] (A) **Починить публичайзинг.** SDK 8.0.421 + BepInEx-публичайзер УЖЕ стоят (`fydne_build`).
      Проблема: ссылка на `Assembly-CSharp_public.dll` ломает резолв (identity-коллизия с обычной
      в той же папке deps). Следующий тест: **убрать обычную Assembly-CSharp.dll из deps**, оставить
      только `_public` → тогда CS0122 и большинство CS0117 (приватные методы) уйдут сами.
- [ ] (B) **Перевести патчи на строковые имена** `[HarmonyPatch(typeof(X), "Method")]` + доступ
      через AccessTools/Traverse (string не требует compile-доступа). Для реально переименованных
      в v14 — уточнить новые имена. Самые сломанные просто отключить (как NineTailedFoxAnnouncer).

✅ NineTailedFoxAnnouncer-патч уже отключён (тип удалён игрой в v14, Cassie→Announcer).

### Event-структуры + обвязка (compile закрыт, runtime bridge частичный)
- [x] Создать event-структуры, необходимые для compile-pass и enum bridge.
- [x] Наполнить `EventMap.PopulateEnumMap()` (legacy enum → shim-структура).
- [x] Пачка 1 (раунд): `RoundEvents.Waiting/Start/End/Restart/Check/ForceStart` → ServerEvents LabAPI.
- [x] Пачка 2 (игрок-жизнь): `Join/Leave/Spawn/ChangeRole/Dead/Dies`.
- [x] Пачка 3 (бой): `Damage/Attack` → `Hurting/Hurt/Dying/Death`.
- [x] Пачка 4a (двери/пикапы/дроп/эскейп): `InteractDoor/PickupItem/PrePickupItem/DropItem/DroppedItem/DropAmmo/ChangeItem/Escape`.
- [x] Пачка 5a (SCP partial): `Scp914/Scp173/Scp096/Scp079/Scp049/Scp106` основные события.
- [x] Пачка 6a (админ/команды partial): `RemoteAdminCommand`, `GameConsoleCommand`, RA-list, ban/kick/reports.
- [x] Пачка 7a (карта/варх partial): `OpenDoor` cancellable через `InteractingDoor`,
  `DamageDoor/LockDoor/CreatePickup/LczDecontamination/TriggerTesla/AlphaWarhead`.
- [x] Пачка 7b (generator/locker/corpse partial): reflection-safe bridge для
  `InteractGenerator/ActivateGenerator`, `InteractLocker`, `CorpseSpawned`.
- [x] Пачка 8a (эффекты): `EffectEnabled` через LabAPI `UpdatingEffect`, `EffectDisabled`
  через `UpdatingEffect/UpdatedEffect` при `Intensity == 0`.
- [x] Пачка 4b: `UsedItem/UseItem/UsingRadio` с реальным player/item/radio payload.
- [ ] Пачка 6b: runtime-проверка reply/permission semantics для RA/client/server console.
- [ ] Пачка 7c: workstation, полный generator payload/status semantics, полноценный locker/Tesla/corpse payload.
- [ ] Пачка 8b: effect type mapping без эвристики по имени класса.

### Обёртки/контроллеры
- [ ] `Map` (Rooms/Doors/… как `List<T>` + extension `TryFind`/`Find`)
- [ ] `Room` полная (Type↔RoomName маппинг, Lights, Position)
- [ ] `Door` полная (Open/Name/Type/AddPart/GameObject)
- [ ] `Lift`/`Elevator`, `Tesla`, `Cassie`, `Generator`
- [ ] `Player.Effects` (EffectType enum + Enable/TryGet/DisableAll)
- [ ] `Player.RoleInformation.Scp*` (Scp079.Lvl и пр. — SCP-роль API)

### Прочее
- [ ] Публичайзер `Assembly-CSharp`→`_public` + `Mirror`→`_public` (закрыть 5× CS0122)
      — через Mono.Cecil-скрипт ИЛИ установить dotnet SDK + `BepInEx.AssemblyPublicizer.Cli`
- [ ] `SCPLogs.dll`: заглушить зависимость в `plugin/Loli/Logs/` ИЛИ получить у основателя
- [ ] `Qurre.API.Addons.Audio` (аудио-плеер) — отдельный модуль
- [ ] Закрыть 89× CS0234 (под-namespace'ы Qurre: `Objects`, `World.Map`, `Addons.Models` и т.д.)

### Финал P0
- [x] `build-plugin.ps1` → 0 ошибок, `Loli.dll` собран
- [ ] Разложить `Qurre.dll` + `Loli.dll` в plugins-папку LabAPI, проверить загрузку на сервере
- [ ] Прогнать Harmony-патчи (`PatchAll` обернуть в try/catch — см. `docs/04`)

---

## 🟧 P1 — Бэкенд (Node + MongoDB)

- [ ] Поднять Mongo (`backend/docker-compose.yml`)
- [ ] Заполнить `config.js` сервисов новыми секретами (`backend/config-templates/`)
- [ ] Убрать 3 отсутствующих сервиса из `pm2.config.js` (degraded/web-proxy/scpsl-mirror)
- [ ] Запустить `scp-socket` + `loli-api`, проверить коннект `:2467` с игрой
- [ ] Сгенерировать новый `ApiToken` (синхронно в плагине и `scp-socket`)

---

## 🟨 P2 — Инфраструктура запуска

> 📌 **Временный хостинг на первое время:** французская нода **Hostkey `секретно`**,
> `vm.nano` = **2 vCore / 2 GB RAM / 60 GB SSD** (из проекта Зоркофф). **Уже занята** —
> там REALITY-выход (Xray) для Telegram-трафика Зоркофф. ⚠️ НЕ ронять `xray-fr`/REALITY.
> Реальный потолок на 2 GB (после Xray+ОС свободно ~1.3–1.5 ГБ):
> - ✅ **лёгкий Node-бэкенд** (`scp-socket` + `loli-api`) для теста связки игра↔БД — влезет;
> - 🟡 **мини тест-сервер SCP:SL** (10–15 слотов, флавор `NR`) — влезет ВПРИТЫК, только если
>   Mongo вынести наружу и Xray под низкой нагрузкой; для теста/ностальгии с друзьями — ок,
>   для реального онлайна — нет (2 vCore burst — слабое single-thread ядро, будет лагать);
> - 🔴 **полный стек (4 сервера + Mongo + боты)** — не влезет. Нужно отдельное железо.

- [ ] (старт) Поднять на фр-ноде `секретно` лёгкий бэкенд для теста — НЕ трогая Xray
- [ ] Нормальное железо (НЕ 1 ГБ): игровой узел — быстрое ядро + 2–4 ГБ/инстанс
- [ ] Новый Steam server token + verkey → попасть в список серверов
- [ ] Домен + SSL, подключить сайт
- [ ] Один публичный NoRules-сервер (флавор `NR`)

---

## 🟦 P3 — Организация / не-техническое

- [ ] Разговор с основателем (питч — `docs/07_FOUNDER_PITCH.md`): дамп БД + бренд
- [ ] Прощупать старую аудиторию через YouTube-канал

---

## ✅ Сделано
- [x] Аудит трёх репозиториев + 9 документов
- [x] Версия игры зафиксирована (v14.x)
- [x] Стенд развёрнут (сервер SCP:SL, LabApi 1.1.7, 147 DLL, компилятор)
- [x] Полная карта API LabApi выгружена
- [x] Shim-фундамент компилируется (Log/Server/Round/Player+подобъекты/контроллеры-мин)
- [x] Каркас диспетчера + точка входа LabAPI
- [x] Перепись ошибок плагина (887 → план выше)
- [x] agent-exchange протокол + общий лог

---

## 2026-06-08 Codex pass: Qurre-v2 reference + external stubs

- [x] Cloned Qurre-v2 locally as a reference only: `sources/Qurre-v2` (gitignored). Source: https://github.com/Qurre-sl/Qurre-v2, branch `v3-use-14.1`, Apache-2.0.
- [x] Confirmed strategy: keep the LabAPI shim. Qurre-v2 is useful as behavior/spec reference, but direct replacement requires its loader/install model and publicized game assemblies.
- [x] Fixed `.gitignore`: `sources/` and `dependencies/` are now actually ignored without inline comments that break gitignore matching.
- [x] Added `.env.example` with empty stubs for socket/API/CDN/Steam/Discord/Telegram/AI moderation variables.
- [x] Hardened Discord webhook sender: empty or invalid webhook URL is now a no-op; old `discord.loli-xxx.baby` rewrite removed.
- [x] Stubbed external API/CDN defaults: `FYDNE_API_URL` and `FYDNE_CDN_URL` now default to empty strings, not old FYDNE domains.
- [x] Hardened `SendApiReq` and `DownloadAudio`: missing API/CDN URL no longer triggers real external HTTP requests.
- [x] Qurre-v2-guided shim improvements: `Scp079.MaxEnergy`, `Scp079.LostSignal`, `Scp106` wrapper, and `Effects.SetFogType` implemented closer to Qurre behavior.
- [x] Added local deploy helper: `scripts/deploy-local-plugin.ps1` copies built `Qurre.dll` + `Loli.dll` into `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\global` or a port-specific folder.
- [x] Verification: `scripts/build-shim.ps1` OK; `scripts/build-plugin.ps1` OK, `0` errors.

Remaining hard gate: live SCP:SL/LabAPI smoke-test. Compile/offline compatibility is materially better, but runtime is still unverified.

---

## 2026-06-08 Codex pass: respawn/intercom/lift/schematic parity

- [x] Implemented Qurre-compatible respawn facade over current SCP:SL wave API:
  `Respawn.CallMtfHelicopter()`, `Respawn.CallChaosCar()`, wave spawn helper, and token accessors.
- [x] Implemented `Intercom` facade over current `PlayerRoles.Voice.Intercom`:
  status, display text, cooldown, recharge cooldown, and speech remaining are now backed by game state.
- [x] Expanded `Qurre.API.Controllers.Lift`:
  type/group, bounds, position, rotation, scale, sequence status, and `Use()`.
- [x] Implemented `Player.GamePlay.Lift` detection by checking the player's position against LabAPI elevator bounds.
- [x] Implemented `Effects.Controller.UseMedicalItem(...)` through `LabApi.Features.Wrappers.UsableItem.Use()`.
- [x] Replaced empty `SchematicManager.LoadSchematic(...)` behavior with a generic JSON loader for basic primitives and lights.
- [x] Verification: `scripts/build-shim.ps1` OK; `scripts/build-plugin.ps1` OK, `0` errors.

Still runtime-sensitive:
- [ ] Verify respawn wave animation calls on a live server.
- [ ] Verify intercom display override updates on clients.
- [ ] Verify JSON schematic compatibility with the actual FYDNE `Schemes/*.json` files once they are available.
- [ ] Audio voice playback remains a stub; compile compatibility exists, real sound output is not restored yet.

---

## 2026-06-09 Codex pass: runtime-null guards and door parity

- [x] Replaced `Player.AuthManager => null` with the real `ReferenceHub.authManager`.
- [x] Implemented `Player.InvokeEscape(...)` by dispatching Qurre `EscapeEvent`.
- [x] Implemented `Administrative.RaLogin()` / `RaLogout()` using current `ServerRoles` state with reflection fallbacks for private runtime members.
- [x] Removed deprecated `Player.IsOffline` usage; `Player.Disconnected` now uses `LabAPI Player.IsDestroyed`.
- [x] Expanded `Tesla` wrapper: trigger, destroy, name, range, progress state, immunity lists.
- [x] Expanded `Door` wrapper: real `Lock`, `Unlock`, `Permissions`, `IsLift`, and Qurre `DoorType` mapping for current LabAPI door names.
- [x] Added `DoorType.Unknown` fallback to avoid misclassifying unknown doors.
- [x] Deployed current `Qurre.dll` + `Loli.dll` to local LabAPI global plugin folder via `scripts/deploy-local-plugin.ps1 -Build`.
- [x] Verification: `scripts/build-shim.ps1` OK; `scripts/build-plugin.ps1` OK, `0` errors.

Next hard gate:
- [ ] Start `C:\Users\Admin\fydne_build\scpsl-server\LocalAdmin.exe` and capture first LabAPI plugin-load log.
- [ ] Fix first live `TypeLoadException` / `MissingMethodException` / `NullReferenceException` batch from server logs.

---

## 2026-06-09 Codex pass: local smoke helper + safer audio state

- [x] Added `scripts/start-local-smoke-test.ps1` to launch local `LocalAdmin.exe` in a separate console.
- [x] Improved audio shim state:
  `AudioPlayerBot` now has a queue, current task, `ReferenceHub`, stop/destroy behavior, and waits for estimated raw-audio duration.
- [x] `StreamAudio` now supports reset/read-ended/read-percent/duration estimation and closes streams on completion/skip.
- [x] `Audio.CreateNewAudioPlayer(...)` now attempts to create a Mirror fake player and falls back to host hub if unavailable.
- [x] Re-deployed current `Qurre.dll` + `Loli.dll` to local LabAPI global plugin folder.
- [x] Verification: `scripts/build-shim.ps1` OK; `scripts/build-plugin.ps1` OK, `0` errors.

Known limitation:
- [ ] Real Opus `VoiceMessage` playback is not fully restored yet. The current pass prevents null-reference/resource issues around FYDNE voice calls, but it does not guarantee audible intercom output.

---

## 2026-06-09 Codex pass: first live LocalAdmin smoke-pass

- [x] Fixed local LabAPI deployment: `scripts/deploy-local-plugin.ps1` now copies runtime dependencies into `%APPDATA%\SCP Secret Laboratory\LabAPI\dependencies\global`.
- [x] Added `System.Dynamic.dll` deployment for legacy `dynamic` handlers.
- [x] Fixed `scripts/start-local-smoke-test.ps1` to pass port `7777` to `LocalAdmin.exe`, avoiding the interactive port prompt.
- [x] Hardened `QurreBootstrap.LoadSiblingAssemblies()` for LabAPI assemblies with empty/unstable `Assembly.Location`; it now falls back to LabAPI plugin directories.
- [x] Changed Loli Harmony startup to per-type patching so one broken legacy patch does not abort all other patches.
- [x] Added `PrintPlayer` prepare guard for missing old `SCPLogs.Extensions.EventsExtensions.GetRolePrint`.
- [x] Added named event-handler diagnostics in `Qurre.API.Core.Dispatch`.
- [x] Restored key runtime compatibility found by live logs:
  - `Room.Type` now maps current `MapGeneration.RoomName` to legacy `Qurre.API.Objects.RoomType`.
  - `CreatePickupEvent.Info.ItemId/Serial` and cancellation behavior are populated.
  - `LegacyItemId.GetCategory()` supports dynamic legacy calls.
  - `ModelPrimitive.Primitive` returns a `Primitive` adapter for old `List<Primitive>` code.
  - `SObject(false)` avoids creating orphan GameObjects for adapters.
- [x] Live verification: SCP:SL `14.2.7` + LabAPI loads `Qurre-Shim`, enables it, reaches `Idle mode is now available` and `Waiting for players`.

Remaining live warnings:
- [ ] `HideRaAuth` transpiler logs `Index - 0 < 0`; old RA IL locals changed in v14.2.7. It currently falls back to original method and does not stop startup.
- [ ] `Loli.Addons.AutoModeration.SaveLogs` Harmony target is skipped; old `TargetMethod()` no longer resolves cleanly.
- [ ] Full player-join/round-start gameplay pass is still needed; current result is a server-start smoke-pass, not full gameplay validation.

---

## 2026-06-09 Codex pass: saved LocalAdmin logs + no-backend runtime hardening

- [x] Checked saved LocalAdmin logs instead of requiring terminal copy:
  `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log 2026-06-09 12.47.02.txt`.
- [x] Identified live gameplay-start failures:
  - `QurreSocket.NetSocket.Send` background thread exception when old socket backend is absent.
  - `RequestPlayerListCommandEvent::Loli.DataBase.Modules.Admins.Prefixs` null sender.
  - `SpawnEvent::Loli.Scps.Scp0492Better.Spawn` null/player-tag assumptions.
  - many `DamageEvent` / `AttackEvent` handlers receiving no legacy `Target`.
- [x] Added `Core.SafeSocket`; socket is now disabled by default and only enabled with `FYDNE_SOCKET_ENABLED=1|true|yes`.
- [x] Filled legacy event fields in `QurreShim.EventMap`: `Target` for spawn/role/damage/death events and `Sender` for RA player-list requests.
- [x] Added local null guards in `Admins.Prefixs`, `Scp0492Better`, and `Range`.
- [x] Made missing/early `Range` vent room non-fatal.
- [x] Added `Loli.Builds.Models.SafeNetwork` and routed fake lift/door model spawns away from Mirror when no `NetworkIdentity` exists.
- [x] Verification: `scripts/deploy-local-plugin.ps1 -Build` OK and deployed.
- [x] Live startup smoke: `LocalAdmin Log 2026-06-09 16.53.15.txt` reaches `Waiting for players`.

Latest smoke markers:
- [x] `Waiting for players=1`
- [x] `TypeLoadException=0`
- [x] `MissingMethodException=0`
- [x] `NullReferenceException=0`
- [x] `QurreSocket.NetSocket.Send=0`
- [x] `handler ... Object reference=0`
- [x] `SpawnObject ... has no NetworkIdentity=0`

Next runtime gate:
- [ ] User/player live round-start retest after this build.
- [ ] If crash remains, collect the newest `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log *.txt` and fix the first post-16:53 gameplay exception batch.
- [ ] Port or disable remaining Harmony problems: `HideRaAuth` and `AutoModeration.SaveLogs`.
- [ ] Restore real FYDNE schematic JSON assets (`Schemes/*.json`) or replace missing builds with native AdminToy/LabAPI implementations.

---

## 2026-06-09 Codex pass: first player force-start log cleanup

- [x] Inspected user force-start log:
  `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log 2026-06-09 16.59.09.txt`.
- [x] Found that server reached `New round has been started`, then `Server has entered the idle mode`.
- [x] Found remaining repeated handler failure:
  `SpawnEvent::Loli.Scps.Scp0492Better.Spawn: Object reference not set`.
- [x] Made `MovementStateW` null-safe so legacy position/scale/rotation reads do not throw when LabAPI player state is not ready or already destroyed.
- [x] Hardened `Scp0492Better` spawn/damage/delayed zombie mutation paths against destroyed players and missing role/tag/movement/effects state.
- [x] Removed noisy `FIX NW #...` and ping debug spam from `Fixes.CheckPlayersPing`; reconnect logic remains.
- [x] Verification: `scripts/deploy-local-plugin.ps1 -Build` OK and deployed.
- [x] Startup smoke after fix: `LocalAdmin Log 2026-06-09 17.04.46.txt` reaches `Waiting for players`; no startup handler NRE; no `FIX NW` flood.

Next retest:
- [ ] Start server, join as player, force start again.
- [ ] If it still returns to idle, inspect whether it is vanilla solo-player round-end/idle behavior or a remaining plugin event after `RoundStart`.

---

## 2026-06-09 Codex pass: founder data lost, replacement socket backend begins

- [x] Founder confirmed no original FYDNE data/assets were preserved. Treat old MongoDB dumps, socket backend, and original `Schemes/*.json` as lost.
- [x] Strategy changed from "restore original external state" to "build compatible replacements":
  - new QurreSocket-compatible backend;
  - new local persistent player store;
  - rebuilt schematics/builds later;
  - old public progress cannot be recovered unless players manually re-seed it.
- [x] Fixed force-start lifecycle issue:
  - `Waiting.Coroutine` no longer calls `Round.Start()` after a manual/vanilla force-start already left waiting state.
  - `FYDNE_RECOVERY_MODE=1` now prevents FYDNE `RoundCheck` from ending a one-player local test round immediately.
  - For future public servers, set `FYDNE_RECOVERY_MODE=0`.
- [x] Added real first-stage socket backend:
  - `backend/fydne-socket/server.js`
  - TCP port `2467`;
  - legacy frame format `{ ev, args } + "⋠"`;
  - persistent JSON store at `backend/fydne-socket/data/store.json`;
  - users, XP, money, admins, online sessions, TPS;
  - safe empty donate/customize/clan/patrol responses.
- [x] Added `scripts/start-fydne-socket.ps1`.
- [x] Updated `.env.example` with `FYDNE_SOCKET_ENABLED`, `FYDNE_RECOVERY_MODE`, and backend socket settings.
- [x] Verification:
  - `node --check backend/fydne-socket/server.js` OK.
  - standalone TCP protocol test OK: create user, add stats, get stats, donate roles, clans.
  - integration smoke with `FYDNE_SOCKET_ENABLED=1`: `Connected to Socket=2`, `Waiting for players=1`, no TypeLoad/MissingMethod/NRE/handler errors.

Next:
- [ ] User retest: start `backend/fydne-socket`, then start SCP:SL with socket env enabled, join, force start.
- [ ] If solo round still enters idle, inspect newest log after socket-enabled retest.
- [ ] Replace missing `Schemes/*.json` with native LabAPI/AdminToy builds or new authored JSON.
- [ ] Expand backend from JSON to SQLite/PostgreSQL only after plugin gameplay loop is stable.
