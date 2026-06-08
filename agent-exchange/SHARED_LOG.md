# FYDNE Recovery — Shared Agent Log

Координационный лог между агентами/устройствами. Читай перед работой, обновляй после.
Протокол — в `AGENT_PROTOCOL.md`. Новые записи — В НАЧАЛО секции LOG ENTRIES.

---

## 📋 CURRENT STATUS

| Agent | Status | Working On |
|-------|--------|------------|
| Codex | 🟢 ACTIVE | Qurre→LabAPI compile-pass закрыт; offline event-bridge pass #1 и bootstrap загрузки Loli.dll сделаны |
| (другие) | — | — |

## 🔴 ACTIVE BLOCKERS

- **Стратегический (не-техн.):** возврат старой аудитории невозможен без **дампа MongoDB** —
  он только у основателя. «Без баз» решено стартовать с нуля (см. DECISION 2026-06-07).
- **Runtime не проверен:** `Qurre.dll` и `Loli.dll` собираются, но ещё не загружались на живой SCP:SL/LabAPI сервер.
- **Event bridge частичный:** workstation and exact generator/locker/corpse/Tesla semantics
  и часть map/SCP payload ещё требуют отдельного bridge-pass.
- **Железо:** французская `vm.nano` 2 vCore / 2 GB RAM годится только для лёгкого теста/ностальгического mini-сервера;
  полный публичный стек с несколькими SCP:SL инстансами и Mongo туда не помещается.

---

## 0. TL;DR текущего состояния

- **Цель:** воскресить FYDNE (SCP:SL). Бэкенд (Node) — восстанавливать; плагин (C#) — мигрировать
  с фреймворка **Qurre** (доступен только у основателя) на официальный **LabAPI**.
- **Подход к миграции:** не переписывать 196 файлов плагина, а собрать **shim-сборку с именем
  `Qurre.dll`** поверх LabAPI — плагин подхватит её без правок в себе.
- **Окружение развёрнуто** на машине разработчика: сервер SCP:SL (SteamCMD), **147 DLL**,
  **LabApi 1.1.7**, компилятор VS2022/csc. Полная карта API LabApi выгружена в
  `docs/labapi_1.1.7_api_reference.txt`.
- **Shim компилируется** (`plugin/QurreShim` → `Qurre.dll`, ~95 КБ): Log, Server, Round,
  Player + под-объекты, контроллеры, диспетчер событий, LabAPI event bridge #1,
  sibling DLL loader, точка входа LabAPI `QurreBootstrap`.
- **Текущее состояние 2026-06-08:** `Qurre.dll` и `Loli.dll` собираются с **0 compile errors**.
  `QurreBootstrap` грузит соседние DLL, чтобы `Loli.dll` попадал в `AppDomain`; `EventMap` имеет первый
  реальный LabAPI bridge для round/player/combat/map/warhead/SCP/effect событий.
- **Перепись плагина:** `scripts/build-plugin.ps1` → **0 compile errors**. Это не runtime-pass:
  часть shim API ещё является заглушками/эвристикой, а реальная проверка начнётся с загрузки DLL на сервер.

---

## 1. Что уже в репозитории

```
docs/01..09 + labapi_1.1.7_api_reference.txt   — аудит, дорожная карта, зависимости, патчи,
                                                  бэкенд, версия, миграция, статус окружения
scripts/   clone-sources, gather-dependencies, build-shim, build-plugin
backend/   docker-compose (Mongo) + config-templates
patches/   transpiler-to-prefix паттерн
plugin/Loli       — исходник плагина (196 .cs, MIT)
plugin/QurreShim  — shim Qurre→LabAPI (компилируется)
```

## 2. Окружение на машине разработчика (НЕ в git)

| Что | Путь |
|---|---|
| Сервер SCP:SL | `C:\Users\Admin\fydne_build\scpsl-server` |
| Игровые DLL | `…\SCPSL_Data\Managed` (144 шт) |
| SteamCMD | `C:\Users\Admin\fydne_build\steamcmd` |
| Инспектор API (MetadataLoadContext) | `C:\Users\Admin\fydne_build\tools\Inspect.exe` |
| dependencies/ | в репо-папке, gitignored, 147 DLL |

## 3. Что осталось (кратко — детально в TODO.md)

1. Runtime smoke-test: загрузить `Qurre.dll` + `Loli.dll` на локальный SCP:SL/LabAPI сервер и собрать первый лог.
2. Довести event bridge gaps: `UsedItem/UseItem/UsingRadio`, `GameConsoleCommand`, generators/workstation/lockers/corpse/full Tesla/OpenDoor/effects.
3. Проверить Harmony patches под `FYDNE_SKIP_LEGACY_PATCHES`; включать legacy patches только после отдельной ревизии.
4. Довести `Map`/`Room`/`Door`/`Lift`/`Tesla`/`Cassie`/`Generator` и `Player.Effects` из scaffolding до реальной семантики.
5. Бэкенд: стартовать без старой MongoDB, новые секреты через env/config templates.

---

## 📝 LOG ENTRIES

### 2026-06-08 (7) ✅ EVENT BRIDGE PASS #1 — Agent: Codex

**Status**: COMPILE_PASS + OFFLINE_EVENT_BRIDGE_PASS — `Qurre.dll` and `Loli.dll` still build with **0 compile errors**.
**Related To**: Qurre→LabAPI runtime readiness before first SCP:SL smoke test.

Verified commands:
- `scripts/build-shim.ps1` → OK, warnings only.
- `scripts/build-plugin.ps1` → OK, `0` errors.

Changed:
- `plugin/QurreShim/src/Bootstrap.cs`: `QurreBootstrap.Enable()` now loads sibling DLLs from the same folder before `Core.BootstrapAll()`.
  This is required so a legacy `Loli.dll` placed next to `Qurre.dll` can enter `AppDomain` even though it is not a LabAPI plugin class.
- `plugin/QurreShim/src/Core.cs`: `[EventMethod]` scanner now supports paramless legacy handlers and single-argument handlers separately,
  rejects unsupported signatures, and skips non-static event handlers instead of invoking them through a null instance.
- `plugin/QurreShim/src/Structs/Structs.cs`: added distinct shim event types for round lifecycle, alpha detonation,
  LCZ decontamination, and SCP-079 recontainment.
- `plugin/QurreShim/src/EventMap.cs`: filled legacy enum→event type map and added LabAPI subscriptions for:
  round lifecycle/check/force-start; player join/leave/spawn/role/death/damage; doors; pickup/drop/change/use/used item;
  radio drain; escape;
  cuffs; ban/kick/reports/RA-list; RA/client/server-console command split with reply propagation;
  pickup creation/decon/door damage/door lock/cancellable open-door; reflection-safe generator/locker/corpse bridge;
  warhead start/stop/detonate;
  Scp914/173/096/079/049/106 partial events; effect updating.

Known deferred bridge gaps:
- `UsedItem`, `UseItem`, and `UsingRadio` are now wired after raw metadata probing exposed `UsableItem`, `RadioItem`, and `Drain`.
- Command reply/permission semantics still need runtime verification on RA, client console, and server console.
- Workstation, exact generator/locker/corpse semantics, full Tesla payload, and full effect disabled/type mapping are still pending.

Next runtime step:
1. Put `Qurre.dll` and `Loli.dll` in the same LabAPI plugin directory.
2. Start a local SCP:SL server with `FYDNE_SKIP_LEGACY_PATCHES` enabled.
3. Collect first-load logs and fix failures in order: assembly load → `[PluginInit]` lifecycle → event dispatch → Harmony patch errors → join/spawn flow.

---

### 2026-06-08 (5) ✅ COMPILE PASS — Agent: Codex

**Status**: COMPILE_PASS — `Qurre.dll` and `Loli.dll` build with **0 compile errors**.
**Related To**: Qurre→LabAPI shim recovery, first plugin DLL artifact.

Verified commands:
- `scripts/build-shim.ps1` → OK, warnings only.
- `scripts/build-plugin.ps1` → OK, `0` errors.

Artifacts:
- `plugin/QurreShim/bin/Qurre.dll` — 68,096 bytes.
- `plugin/Loli/bin/Loli.dll` — 574,976 bytes.

Final compile fixes after the 25-error checkpoint:
- Replaced missing `ItemType.GetItemCategory()` with explicit `ItemType.GetCategory()` mapping.
- Fixed `Model(...)` overload ambiguity in `AirDrop`.
- Typed event fields that were blocking compile: `CreatePickupEvent.Inventory`, `EventBase.Inventory`, `EventBase.Corpse`.
- Added compat wrappers/helpers: `EffectControllerW`, `InventoryItemCompat`, `Corpse.Scale/Owner`, `ItemSerial()`, `RemoveWhere`, `RpcShowRoundSummary`.
- Replaced private/removed API usage in compile path: `ItemBase.IsLocalPlayer` guards under `FYDNE_SKIP_LEGACY_PATCHES`, `light._light` → `light.gameObject`, `ev.Inventory._hub` → `ev.Player` position fallback.

Important warning:
This is only a **compile-pass**, not a runtime-pass. A large part of the shim is still compatibility scaffolding/stubs. Next work must be runtime smoke testing:
1. Copy `Qurre.dll` + `Loli.dll` into the LabAPI plugin folder of the local SCP:SL server.
2. Start server and collect logs.
3. Fix first-load exceptions in this order: plugin bootstrap, dependency load, event wiring, Harmony patch failures, map/model spawn, player join/spawn loop.
4. Keep `FYDNE_SKIP_LEGACY_PATCHES` enabled until runtime is stable.

Do not commit `dependencies/`, game DLLs, generated server folders, IPs, tokens, DB dumps, or webhook URLs.

---

### 2026-06-08 (4) 🔄 IN_PROGRESS — Agent: Codex

**Status**: IN_PROGRESS — shim compiles; plugin was reduced to **25 compile errors on the last confirmed build**. After that, a few more targeted fixes were made but not re-measured because the user asked to stop, update logs, and commit.
**Related To**: Qurre→LabAPI compile recovery, public-repo hygiene.

Important correction: the previous “12 errors left” state was not a real finish line. Those 12 were the visible legacy Harmony patch layer. After adding `FYDNE_SKIP_LEGACY_PATCHES` and skipping the obsolete/private v14 patch targets, the actual next compatibility layer appeared at ~1188 errors. This session reduced that layer roughly:

`1188 → 964 → 596 → 380 → 193 → 123 → 89 → 57 → 25`

Main work done:
- Added `FYDNE_SKIP_LEGACY_PATCHES` in `scripts/build-plugin.ps1`.
- Wrapped/disabled obsolete Harmony patches that target removed/private SCP:SL v14 methods.
- Expanded `plugin/QurreShim` heavily: Player subobjects, effects/admin/stats wrappers, Models/Schematic compatibility, Room/Door/WorkStation/Map/Round/Server helpers, MethodInfo event injection, audio stubs, global Qurre-like services.
- Added compatibility wrappers for old Qurre patterns: `TryFind(out result, predicate)`, `RoomType.GetRoom`, `DoorType.GetDoor`, ammo shortcuts, `Map.Broadcast` returning `MapBroadcast`, `Round.CurrentRound` numeric token, old `Door.Lock` property, etc.
- Replaced real Discord webhook URLs found during the session with env variables:
  `FYDNE_WEBHOOK_BANS_PUBLIC`, `FYDNE_WEBHOOK_BANS_PATROL`, `FYDNE_WEBHOOK_BANS_ADMIN`, `FYDNE_WEBHOOK_BANS_OWNERS`,
  `FYDNE_WEBHOOK_ANTICHEAT`, `FYDNE_WEBHOOK_MODERATION`, `FYDNE_WEBHOOK_BUG_REPORT`.

Last confirmed build state:
- `scripts/build-shim.ps1`: OK (`Qurre.dll` builds; warnings only).
- `scripts/build-plugin.ps1`: FAIL with 25 errors.

First next step:
1. Run `scripts/build-shim.ps1`.
2. Run `scripts/build-plugin.ps1`.
3. Re-read `plugin/Loli/bin/build.log`; some of the 25 may already be fixed by the last unmeasured edits.

Known remaining categories from the last measured 25:
- `CS1061`: old helpers/properties such as `ItemType.GetCategory`, `Item.ItemSerial`, `RoundSummary.RpcShowRoundSummary`, `LightSourceToy._light`, `Item.Value`.
- `CS1977`: lambdas passed through dynamic dispatch, mostly `AhpActiveProcesses.ForEach(...)`.
- `CS8197`: `out var` with dynamic sources (`TryGetEffect`, `TryGetComponent`, `TryGetDrink`, donate status).
- `CS0122`: `ItemBase.IsLocalPlayer` in `RealisticArmory`.
- `CS8752/CS8754`: target-typed `new(...)` with dynamic targets.
- `CS1503`: a few narrow API signature mismatches.

Do not commit `dependencies/`, game DLLs, dumps, IPs, tokens, or webhook URLs.

---

### 2026-06-08 (3) 🤝 HANDOFF → следующему агенту (напр. Codex/GPT-5-codex)

**Status**: IN_PROGRESS — плагин компилируется до **12 ошибок** (с 887). Подхвати отсюда.
**Related To**: финиш миграции Qurre→LabAPI (shim)

Контекст этой сессии не наследуется — всё нужное здесь и в репо. Стартуй так:

1. Прочитай: `AGENT_PROTOCOL.md`, эту запись, `TODO.md` (P0), `docs/08`/`docs/09`.
2. Окружение (сервер SCP:SL, LabApi, компилятор, публичайзер, .NET SDK) уже развёрнуто —
   пути и состояние в `docs/09_BUILD_ENV_STATUS.md`. Зависимости — в `dependencies/` (gitignored;
   если их нет — пересобери по `scripts/gather-dependencies.ps1` + `docs/03`).
3. Цикл: `scripts/build-shim.ps1` → `scripts/build-plugin.ps1 -Census`. Цель: 0 ошибок → `Loli.dll`.

**Оставшиеся 12 ошибок = Harmony-патчи на v14-методы игры** (полный список — `TODO.md` P0).

**ПЕРВЫЙ ШАГ (путь A — публичайзинг):** гипотеза — ссылка на `Assembly-CSharp_public.dll` ломает
резолв из-за identity-коллизии с обычной `Assembly-CSharp.dll` в той же папке `dependencies/`.
Тест: убрать/переименовать обычную `Assembly-CSharp.dll` из `dependencies/` так, чтобы осталась
ТОЛЬКО `_public`, прогнать census. Если гипотеза верна — CS0122 и приватные CS0117 уйдут сами.
(Публичайзер уже установлен; как генерить `_public` — см. `docs/09` / BepInEx.AssemblyPublicizer.)

**Если A не сработал (путь B):** перевести проблемные патчи на строковые имена
`[HarmonyPatch(typeof(X), "Method")]` + доступ через AccessTools/Traverse (string не требует
compile-доступа). Реально удалённые в v14 типы/методы — отключить (пример: `_DisableCassie.cs`,
патч на `NineTailedFoxAnnouncer` уже закомментирован — тип удалён игрой).

**Архитектура shim'а** (чтобы не ломать): выходная сборка называется `Qurre.dll` (drop-in замена
Qurre). `Player` живёт в `Qurre.API.Controllers`, `Client` — в `Qurre.API.Classification.Player`,
движок построек `Qurre.API.Addons.Models` реализован на родных LabAPI AdminToys (PrimitiveObjectToy/
LightSourceToy). Карта событий Qurre→LabAPI — в `EventMap.cs` (наполняется), полный API LabApi —
`docs/labapi_1.1.7_api_reference.txt`. **Не коммить секреты/DLL/дампы (см. `.gitignore`).**

После работы — обнови этот лог и `TODO.md`, затем commit + push.

---

### 2026-06-08 (2) 🔄 IN_PROGRESS — Agent: Claude Code (Opus)

**Status**: IN_PROGRESS — плагин компилируется до **12 ошибок** (с 887, −98.6%)
**Files Changed**: `plugin/QurreShim/src/*` (Models/Schematic/Client/MapBroadcast/PrimitiveParams),
`plugin/_PluginGlobals.cs`, `plugin/Loli/Modules/Voices/_DisableCassie.cs` (откл. устаревший патч),
`scripts/build-*.ps1`, `dependencies/` (-SchematicUnity.dll, +Newtonsoft 13, +Harmony 2.3.3)

- ⚙️ **Движок построек написан САМ** на родных LabAPI AdminToys (`PrimitiveObjectToy`/`LightSourceToy`):
  `Qurre.API.Addons.Models` (Model/ModelPrimitive/Primitive/LightPoint/CustomRoom). **Без SchematicUnity
  и основателя.** Загрузчик `.json` (SchematicManager) — каркас написан, парсинг формата MapEditorReborn — TODO.
- 🔧 FIX: Newtonsoft.Json 13 + Harmony 2.x (namespace HarmonyLib) подцеплены в deps (−~490 ошибок).
- 📌 INFO: в Qurre `Player` лежит в `Qurre.API.Controllers`; `Client` — в `Qurre.API.Classification.Player`.
- 🔴 BLOCKER (мелкий, build-config): публичайзинг. SDK 8.0.421 + BepInEx-публичайзер стоят в `fydne_build`,
  но ссылка на `Assembly-CSharp_public.dll` ломает резолв (identity-коллизия с обычной в deps; даёт 1174
  ложных ошибки). Следующий тест: убрать обычную Assembly-CSharp.dll из deps, оставить только `_public`.
- ⏭️ ОСТАЛОСЬ: 12 ошибок = Harmony-патчи на v14-методы (5 приватных + 7 переименованных). См. TODO P0.
  Два пути: (A) починить публичайзинг, (B) строковые имена патчей / отключить сломанные.

---

### 2026-06-08 (1) 🔄 IN_PROGRESS — Agent: Claude Code (Opus)

**Status**: IN_PROGRESS
**Files Changed**: `plugin/QurreShim/src/*` (Structs, QurreEnums, World.Map, Controllers Cassie/Tesla/WorkStation, JsonConfig, _PluginGlobals), `scripts/build-plugin.ps1`, `dependencies/` (Newtonsoft.Json 13.0.3 + Harmony 2.3.3)
**Related To**: миграция плагина — event-структуры + обёртки + фикс референсов

- **Плагин: 887 → 117 ошибок (−87%)** за сессию.
- Создан весь event-слой: `EventBase` + 66 классов-событий + Qurre-энумы (EffectType/DamageTypes/LiteDamageTypes).
- 🔧 FIX: подцепил недостающие референсы — `Newtonsoft.Json` 13.0.3 (игра не везёт) и
  заменил старый `0Harmony` 1.x на **Harmony 2.3.3** (namespace `HarmonyLib`). Это сняло ~490 ошибок.
- Обёртки: `World.Map`, `Cassie`(→Announcer), `Tesla`, `WorkStation`, `JsonConfig`, `List.TryFind`.
- 📌 INFO: в Qurre класс `Player` лежит в `Qurre.API.Controllers` — решено через build-time
  `global using Player` (`plugin/_PluginGlobals.cs`), без правок плагина.
- 🔴 BLOCKER: оставшиеся ~100 ошибок = подсистема **схематик/постройки** (`Qurre.API.Addons.Models`
  + `SchematicUnity.API`). Нужна оригинальная `SchematicUnity.dll` ИЛИ временно исключить `Builds/`.
  См. TODO P0.

---

### 2026-06-07 ✅ DONE — Agent: Claude Code (Opus), session 2026-06-07

**Status**: IN_PROGRESS (миграция продолжается)
**Files Changed**: весь репозиторий `fydne-recovery` (init → текущее состояние)
**Related To**: аудит + развёртывание стенда + старт миграции Qurre→LabAPI

Сделано за сессию:
- **Аудит** трёх репозиториев FYDNE (Loli-Merged, plugins-scpsl-2020-2024, fydne-prod-web).
  Документы `docs/01..09`. Версия игры зафиксирована — **v14.x** (маркер `Scp956Pinata`).
- **Развернул стенд** автономно: SteamCMD → сервер SCP:SL → 147 DLL + LabApi 1.1.7.
  Собрал инспектор на MetadataLoadContext, выгрузил **полную карту API LabApi**.
- **Старт миграции** через shim (`AssemblyName=Qurre`): Log/Server/Round/Player(+8 под-объектов)/
  контроллеры(мин)/диспетчер(loader+registry+Inject)/Bootstrap — **всё компилируется** против
  реального LabApi.
- **Перепись плагина**: 887 ошибок (карта оставшегося, см. TODO.md).

⚠️ DECISION: мигрируем на LabAPI (не ждём Qurre у основателя). Старт «без баз данных».
📌 INFO: `Qurre.dll`-shim — drop-in замена, плагин править почти не нужно.
🐛 BUG (в самом плагине, на будущее): `VoiceTransceiver.ServerReceiveMessage` патчится дважды
   (`ScpSpeaks.cs` + `FixSpoiled.cs`) — проверить конфликт при сборке.

Следующий шаг: наполнять `EventMap.cs` + создавать event-структуры пачками, гоняя
`build-plugin.ps1 -Census` до нуля.

---
