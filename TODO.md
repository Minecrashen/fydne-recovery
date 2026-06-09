# FYDNE Recovery — TODO

Живой список задач. Отмечай `[x]`, коммить, пушь. Подробности — в `docs/`.
Координация между агентами — `agent-exchange/SHARED_LOG.md`.

Статус на 2026-06-08: стенд развёрнут, shim компилируется, плагин — **0 compile errors**.
Артефакты созданы: `plugin/QurreShim/bin/Qurre.dll` и `plugin/Loli/bin/Loli.dll`.
Важно: это первый compile-pass, а не runtime-pass. Следующий этап — загрузка на тестовый SCP:SL/LabAPI сервер, проверка старта, логов, event wiring, Harmony patches и отключенных legacy-заглушек.

---

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
