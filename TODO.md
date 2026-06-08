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
- [ ] Не считать runtime готовым: `UsedItem/UseItem/UsingRadio` пока НЕ подключены к LabAPI, потому что в текущей
  LabAPI 1.1.7 сигнатуре не найден полезный payload для старых FYDNE-хендлеров; generator/workstation/locker/corpse
  и часть SCP/map событий требуют следующего bridge-pass.
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
- [x] Пачка 8a (эффекты partial): `EffectEnabled` через LabAPI `UpdatingEffect`.
- [ ] Пачка 4b: `UsedItem/UseItem/UsingRadio` с реальным player/item/radio payload.
- [ ] Пачка 6b: runtime-проверка reply/permission semantics для RA/client/server console.
- [ ] Пачка 7c: workstation, полный generator payload/status semantics, полноценный locker/Tesla/corpse payload.
- [ ] Пачка 8b: `EffectDisabled`/effect type mapping без эвристики по имени класса.

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
