# FYDNE Recovery — Shared Agent Log

Координационный лог между агентами/устройствами. Читай перед работой, обновляй после.
Протокол — в `AGENT_PROTOCOL.md`. Новые записи — В НАЧАЛО секции LOG ENTRIES.

### 2026-06-10 (15) WORLD-SPACE TOY ENGINE FIX - Agent: Claude Code

**Status**: CODE_PASS (not built locally — this session has no Windows/csc; build via `scripts/build-shim.ps1` + `build-plugin.ps1` before testing).
**Files Changed**: `plugin/QurreShim/src/Addons/Models.cs`, `Schematic.cs`, `SchematicJsonLoader.cs`, `plugin/Loli/Builds/Models/Rooms/AdminRoom.cs`, `plugin/Loli/DataBase/Modules/Controllers/Glow.cs`, `TODO.md`.
**Related To**: offset/dark fallback admin room; root cause of all "construction is elsewhere on the map" symptoms.

Root cause (verified against LabAPI source and EXILED wrappers, not guessed):
- `LabApi AdminToy.Create(position, rotation, scale, parent)` sets `transform.localPosition = position` — position is LOCAL when parent is passed. The previous `TransformPoint` fix (326d665) therefore double-offset the shell to ~(260,600,200): high above the map, in darkness. Matches the user report exactly.
- `AdminToyBase` syncs LOCAL transform to clients and client-side toys are always unparented, so ANY toy parented to a server-only GameObject renders at the wrong place on clients regardless of what the server passes.

Fix:
- Toys are now ALWAYS created `parent=null` at computed world coordinates (engine-level in `ModelPrimitive`/`LightPoint`; also `SchematicJsonLoader`).
- Logical `Model` tree is kept; moving models still works: new `ToyAnchorSync` component on the model root re-applies world transforms of registered toys whenever the root matrix changes and pushes `NetworkPosition/NetworkRotation` (same dynamic pattern as Loli's `FixPrimitiveSmoothing`). This keeps lift door animation, Nimb-on-camera, and SCP cage models working.
- New `LightPoint.Follow()/StopFollow()` (`ToyWorldFollow`) replaces `toy.transform.parent = player` in `Glow.cs`.
- `ModelTarget/Door/WorkStation/Pickup` (plain server objects) were never parented at all (sat near world origin) — now attached via `SObject.AttachLocal`.
- Fallback AdminRoom: destroys previous shell on each waiting, brighter surfaces, 6 stronger lights, yellow platform-edge trim, closed front barrier, logs world base position.
- `Client.DimScreen()` implemented (black hint) instead of stub.

Next:
- Build + deploy on the Windows box, full LocalAdmin restart, join during waiting.
- Expected: bright shell at (130.26,300.81,101.3); waiting spawn lands on the upper platform; no more "room elsewhere".
- If anything still off: `FYDNE-BUILD` log now prints the world base position of the shell — compare with the spawn override coordinates in `FYDNE-TRACE`.

---
### 2026-06-09 (14) USER TEST: FALLBACK ROOM OFFSET/DARK - Agent: Codex

**Status**: TEST_RESULT_RECORDED - no code changes in this entry.
**Related To**: live retest after `326d665 Fix parented model world coordinates`.

User result:
- Player still spawns in void at the waiting/tutorial spawn point.
- The fallback construction was found elsewhere on the map.
- The fallback construction is too dark.

Current interpretation:
- `SpawnEvent` and `AdminRoom.SpawnChangePos` are working: player is moved to the configured waiting point.
- The fallback shell is also being created, but AdminToy/LabAPI parent handling still places the shell away from the target world spawn point.
- The next implementation should avoid parent transform ambiguity completely: create fallback AdminRoom primitives directly in world coordinates, not parent-local coordinates.
- Lighting/colors should be made brighter and less dependent on missing dedicated-server shaders.

Next code target:
- Rework fallback `AdminRoom` shell to spawn recovery floor/walls/lights with `parent=null` or a dedicated world-space helper.
- Keep the logical `Model` root only for compatibility with existing FYDNE code.
- Increase light intensity/range and use brighter visible primitive colors.
- Retest waiting Tutorial spawn before moving to other gameplay modules.

---
### 2026-06-09 (13) MODEL PARENT/WORLD COORDINATE FIX - Agent: Codex

**Status**: COMPILE_PASS + DEPLOYED - fixed likely cause of fallback admin shell being spawned away from the player.
**Related To**: live test after fallback AdminRoom: logs show shell creation and spawn override, but player still appears in void.

Findings from `LocalAdmin Log 2026-06-09 23.00.09.txt`:
- `AdminRoom recovery shell spawned` is present.
- `AdminRoom loaded waiting=(130.26,305.42,102.88) tutorial=(130.26,302.26,101.18)` is present.
- `SpawnEvent` changed Tutorial spawn from vanilla `(40,314.08,-32.6)` to `(130.26,305.42,102.88)`.
- Therefore `SpawnEvent` works. The likely issue is model primitive coordinate space: local positions were passed directly to LabAPI AdminToy creation while the player was moved to parent-space/world coordinates.

Changed:
- `plugin/QurreShim/src/Addons/Models.cs`: `ModelPrimitive(parent, ...)` converts local position/rotation through the parent transform before calling `PrimitiveObjectToy.Create(...)`.
- `plugin/QurreShim/src/Addons/Models.cs`: `LightPoint(parent, ...)` converts local light position through the parent transform too.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.
- `scripts/deploy-local-plugin.ps1` -> OK.
- deployed `Loli.dll` SHA256: `07EA9BCF3F0FC3E8D695F0CBF9F122CBA0B6836EA17D28307053A1080C14816A`.
- deployed `Qurre.dll` SHA256: `42995E6257F68D5F9AE3ABB6885BE134CEB6F1F6CCCE5373232BC569CB2AF0B6`.

Next live test:
- Fully restart LocalAdmin.
- Join in waiting state. Expected: waiting Tutorial spawn lands on fallback shell floor.
- If still in void, next fallback is to create recovery AdminRoom primitives without a LabAPI parent to remove ambiguity in `PrimitiveObjectToy.Create(..., parent, true)`.

---
### 2026-06-09 (12) ADMIN WAITING ROOM FALLBACK - Agent: Codex

**Status**: COMPILE_PASS + DEPLOYED - restored a minimal waiting/admin room without original schematic assets.
**Related To**: user live test: waiting spawn in air, visually broken spawns, black screen after a while, request to restore at least the start waiting/admin zone.

Findings from `LocalAdmin Log 2026-06-09 22.46.52.txt`:
- `Qurre-Shim` loaded and enabled successfully.
- `Loli.Enable()` ran: `FYDNE-BOOT Enable recovery=True socketEnabled=True traceSocket=True`.
- Event registry active: 60 event types / 282 handlers.
- `AdminRoom` was active and moved Tutorial waiting spawn to `(130.26,305.42,102.88)`.
- Old schematic assets are missing locally: no `AdminRoom.json`, `Range.json`, `Waiting_Room.json`, or `Schemes/` assets found. The loader returned empty schemes, so spawn points existed without the original floor/walls/colliders.

Changed:
- `plugin/Loli/Builds/Models/Rooms/AdminRoom.cs`: added a code-level recovery shell made from native AdminToys primitives. It creates a lower tutorial platform, upper waiting platform, collidable floor/walls, accents, glass/front panel and lights. It keeps the legacy waiting/tutorial spawn points.
- `plugin/QurreShim/src/Addons/SchematicJsonLoader.cs`: missing files now log `Schematic missing: <path>` and successfully parsed files log `Schematic loaded: <path> objects=<count>`.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.
- `scripts/deploy-local-plugin.ps1` -> OK.
- deployed `Loli.dll` SHA256: `3326CC4E306AD633CB5FBAED063EC6F9444F1A07FB0FCEED014FD8BBBC4B3D83`.
- deployed `Qurre.dll` SHA256: `FDC7535861D64131D0B66108E869D45B99D318B130BA0FE3D0772A3C74479A9F`.

Next live test:
- Fully restart LocalAdmin; DLL hot-reload is not enough.
- Join while the server is waiting for players and confirm the Tutorial waiting spawn lands on the visible recovery shell.
- Check logs for `AdminRoom recovery shell spawned` and `Schematic missing: ...AdminRoom.json`.
- If visible but ugly/dark, tune fallback primitive colors/positions before attempting a full SchematicUnity recreation.

---
### 2026-06-09 (11) PRE-TEST SPAWN BRIDGE HARDENING - Agent: Codex

**Status**: COMPILE_PASS + DEPLOYED - static hardening before the next live LocalAdmin test.
**Related To**: spawn in air / kick after force-start / suspected legacy admin waiting room and duplicated spawn handlers.

Changed:
- `plugin/QurreShim/src/EventMap.cs`: stopped dispatching legacy `SpawnEvent`/`ChangeRoleEvent` from LabAPI post events by default. They now run from pre events (`Spawning`/`ChangingRole`) where `Position`, `Role`, and `Allowed` can be applied back to LabAPI. Old double-dispatch behavior can be enabled only for comparison with `FYDNE_DISPATCH_POST_ROLE_EVENTS=1`.
- `.env.example`: added `FYDNE_DISPATCH_POST_ROLE_EVENTS=0`.
- `plugin/Loli/Builds/Models/Rooms/AdminRoom.cs`: logs computed admin waiting/tutorial spawn points, warns when schematic has no objects, and skips forced zero-position spawns if the custom room was not initialized.
- `plugin/Loli/Builds/Models/Rooms/Range.cs`: logs chaos/donate/guard spawn overrides and warns when EzVent or schematic objects are missing. New prefix: `FYDNE-BUILD`.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.
- `scripts/deploy-local-plugin.ps1` -> OK.
- deployed `Loli.dll` SHA256: `7A916DD377CC9C3240965FDA93E368A95FB06B7C2CBF234E6B19C1CD49C1E79A`.
- deployed `Qurre.dll` SHA256: `90BB058FDCB0D538C26CEF1E2F730A0091A7E1C2DB4FB35C9D5183072D2BE551`.

Next live test:
- Inspect `FYDNE-BOOT`, `FYDNE-BUILD`, `FYDNE-TRACE`, and `FYDNE-SOCKET`.
- Confirm that each spawn/role transition produces one legacy `SpawnEvent`/`ChangeRoleEvent`, not two.
- If the player still appears in the old admin waiting room, use the logged `AdminRoom loaded waiting=... tutorial=...` coordinates to decide whether to keep/move/disable that feature.

---
### 2026-06-09 (10) RUNTIME TRACE INSTRUMENTATION - Agent: Codex

**Status**: COMPILE_PASS + DEPLOYED - added broad runtime diagnostics for the current spawn/crash investigation.
**Related To**: live LocalAdmin test where player spawns in invalid position and is kicked/restarted shortly after forced round start.

Changed:
- `plugin/QurreShim/src/Core.cs`: `Qurre.API.Core.Dispatch<T>()` now traces selected runtime events. In recovery mode it traces high-risk events by default: round lifecycle, join/leave, spawn/change-role, commands, doors/lifts/generators, SCP-106 attack, and alpha warhead. It records begin/end snapshots and per-handler field diffs so we can identify the exact legacy handler that mutates `Allowed`, `Role`, `Position`, `Reason`, etc.
- `plugin/Loli/Core.cs`: boot diagnostics now log recovery/socket trace flags, disable/restart notice, Harmony patched/skipped summary; `SafeSocket` traces socket creation/disabled state, `On`, incoming events, outgoing `Emit`, and logs callback exceptions with event names. Payload logging is opt-in and token/auth/password/SCPServerInit payloads are redacted.
- `.env.example`: documented diagnostic flags: `FYDNE_TRACE_RECOVERY`, `FYDNE_TRACE_EVENTS`, `FYDNE_TRACE_EVERY_HANDLER`, `FYDNE_TRACE_SPAWN`, `FYDNE_TRACE_SOCKET`, `FYDNE_TRACE_SOCKET_PAYLOADS`.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.
- `scripts/deploy-local-plugin.ps1` -> OK.
- deployed `Loli.dll` SHA256: `DBE96EC43414E164B7DD2B3BB5D02F4EA9C4B3587481E150E256A5B855564747`.
- deployed `Qurre.dll` SHA256: `6F09FDCA5341C67DD5BD54AA774D6333BE8A679B7049C95AC7A39C7D20A8EE00`.

Next live test:
- Start LocalAdmin with `FYDNE_RECOVERY_MODE=1`, `FYDNE_TRACE_SPAWN=1`, `FYDNE_TRACE_SOCKET=1`, `FYDNE_TRACE_SOCKET_PAYLOADS=0`.
- Reproduce join -> force start -> crash/kick once.
- Inspect LocalAdmin log for `FYDNE-TRACE` and `FYDNE-SOCKET`; the last event/handler diff before the exception is the first suspect.
- If still ambiguous, run one noisy pass with `FYDNE_TRACE_EVENTS=1` and `FYDNE_TRACE_EVERY_HANDLER=1`, then disable them again.

---
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
- **Event bridge частичный:** workstation, exact effect type mapping, and exact generator/locker/corpse/Tesla semantics
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

### 2026-06-08 (8) ✅ STATUS SYNC — Agent: Codex

**Status**: SYNC_ONLY — лимиты закончились, состояние проекта зафиксировано в `TODO.md` и this log.
**Related To**: pause point after the last bridge pass.

No code changes in this mini-step. Current project status remains:
- `Qurre.dll` and `Loli.dll` compile with `0` errors.
- Event bridge is partial but materially improved: round/player/combat/command/item-radio/map/open-door/generator/locker/corpse/effects are bridged.
- Runtime smoke-test on a live SCP:SL/LabAPI server is still the next hard check.

---

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
  Scp914/173/096/079/049/106 partial events; effect enabled/disabled via intensity updates.

Known deferred bridge gaps:
- `UsedItem`, `UseItem`, and `UsingRadio` are now wired after raw metadata probing exposed `UsableItem`, `RadioItem`, and `Drain`.
- Command reply/permission semantics still need runtime verification on RA, client console, and server console.
- Workstation, exact generator/locker/corpse semantics, full Tesla payload, and exact effect type mapping are still pending.

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

---

### 2026-06-08 (9) ? QURRE-V2 REFERENCE + EXTERNAL STUBS � Agent: Codex

**Status**: OFFLINE_COMPAT_PASS � `Qurre.dll` and `Loli.dll` still build with **0 compile errors**.
**Reference Found**: https://github.com/Qurre-sl/Qurre-v2, branch `v3-use-14.1`, Apache-2.0. Cloned locally to `sources/Qurre-v2`; this path is gitignored and must not be committed because it contains dependency DLLs.

Changed:
- Fixed `.gitignore` so `sources/` and `dependencies/` are really ignored.
- Added `.env.example` with empty placeholders for FYDNE socket/API/CDN/Steam/Discord/Telegram/moderation variables.
- Hardened Discord webhooks: invalid/empty URLs are no-op; removed old Discord proxy rewrite.
- Set `FYDNE_API_URL` and `FYDNE_CDN_URL` fallback to empty, not legacy domains.
- Guarded API/audio downloads when external URLs are missing.
- Used Qurre-v2 as behavior reference for shim improvements:
  - `Scp079.MaxEnergy` get/set via LabAPI/private-field reflection fallback.
  - `Scp079.LostSignal(float)` calls `Scp079LostSignalHandler.ServerLoseSignal` when available.
  - `Scp106` wrapper exposes `Attack`, `SinkholeController`, and `StalkAbility` subroutines.
  - `Effects.SetFogType` maps to `FogControl` intensity like Qurre-v2.
- Added `scripts/deploy-local-plugin.ps1` for safe local artifact deployment into LabAPI plugin folders.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, `0` errors.

Decision:
- Do not replace the shim with Qurre-v2 directly right now. Qurre-v2 requires its own loader/install path and publicized game assemblies. Use it as the authoritative compatibility reference while keeping the current LabAPI shim path.

Next:
1. Continue Qurre-v2-guided shim parity for `WorkStation`, `Generator`, `Locker`, `Intercom`, `Respawn`, and audio.
2. Run first live smoke-test once the user is ready to start a local SCP:SL/LabAPI server.

---

### 2026-06-08 (10) RESPAWN / INTERCOM / LIFT / SCHEMATIC PASS - Agent: Codex

Status: OFFLINE_COMPAT_PASS - `Qurre.dll` and `Loli.dll` build with 0 compile errors.

Changed:
- Added current SCP:SL wave-backed `Respawn` facade: `CallMtfHelicopter`, `CallChaosCar`, `Spawn`, and MTF/Chaos token accessors.
- Added real `Intercom` facade over `PlayerRoles.Voice.Intercom` and `IntercomDisplay`.
- Expanded `Qurre.API.Controllers.Lift` and wired `Player.GamePlay.Lift` by elevator bounds.
- Implemented `Effects.Controller.UseMedicalItem(...)` through LabAPI `UsableItem.Use()`.
- Added `SchematicJsonLoader` and routed `SchematicManager.LoadSchematic(...)` through it. It supports generic JSON primitives/lights and skips unknown records safely.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.

Remaining:
- Live server smoke-test is still the hard gate.
- Audio playback is still compile-compatible only; voice output needs a separate pass.
- Actual FYDNE scheme JSON files are needed to validate schematic fidelity.

---

### 2026-06-09 (11) RUNTIME NULL GUARDS + DOOR/TESLA PARITY - Agent: Codex

Status: OFFLINE_COMPAT_PASS - `Qurre.dll` and `Loli.dll` build with 0 compile errors and were deployed locally to LabAPI global plugins.

Changed:
- `Player.AuthManager` now returns real `ReferenceHub.authManager` instead of null.
- `Player.InvokeEscape(...)` now dispatches Qurre `EscapeEvent`.
- `Administrative.RaLogin()` / `RaLogout()` now manipulate current `ServerRoles` with reflection fallbacks for private runtime members.
- `Player.Disconnected` now uses LabAPI `IsDestroyed` instead of deprecated `IsOffline`.
- Expanded `Tesla` wrapper: trigger, destroy, name, range, progress state, immunity lists.
- Expanded `Door` wrapper: real lock/unlock, permissions policy, lift detection, and Qurre `DoorType` mapping.
- Added `DoorType.Unknown` fallback.
- Ran `scripts/deploy-local-plugin.ps1 -Build`; artifacts copied to `%APPDATA%\SCP Secret Laboratory\LabAPI\plugins\global`.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.

Next:
- Start local `LocalAdmin.exe` and fix the first live server-log batch.
- Audio voice playback remains the largest known stub.

---

### 2026-06-09 (12) LOCAL SMOKE HELPER + SAFER AUDIO STATE - Agent: Codex

Status: OFFLINE_COMPAT_PASS - `Qurre.dll` and `Loli.dll` build with 0 compile errors and were deployed locally again.

Changed:
- Added `scripts/start-local-smoke-test.ps1` to launch local `LocalAdmin.exe` in a separate console.
- Improved audio shim state:
  - `AudioPlayerBot` now has queue/current-task state, stop/destroy behavior, and estimated-duration waiting.
  - `StreamAudio` can reset, report completion/read percent, estimate raw f32le duration, and close its stream.
  - `Audio.CreateNewAudioPlayer(...)` now attempts Mirror fake-player creation and falls back to host hub.
- This does not yet implement full Opus `VoiceMessage` output, but removes the worst null/resource behavior from the previous stub.

Verified:
- `scripts/build-shim.ps1` -> OK, warnings only.
- `scripts/build-plugin.ps1` -> OK, 0 errors.
- `scripts/deploy-local-plugin.ps1 -Build` -> OK.

Next:
- Live LocalAdmin plugin-load test.
- Full audio output pass after first server log batch.

---

### 2026-06-09 (13) FIRST LIVE LOCALADMIN SMOKE-PASS - Agent: Codex

Status: LIVE_START_SMOKE_PASS - SCP:SL 14.2.7 + LabAPI loads Qurre-Shim, enables it, reaches idle mode and `Waiting for players` with no Qurre event-handler runtime errors in the latest checked startup log.

Changed:
- `scripts/deploy-local-plugin.ps1` now deploys runtime dependencies to `%APPDATA%\SCP Secret Laboratory\LabAPI\dependencies\global`, including `System.Dynamic.dll` for legacy dynamic handlers.
- `scripts/start-local-smoke-test.ps1` now passes port `7777` to LocalAdmin so smoke tests do not block on the interactive port prompt.
- `QurreBootstrap.LoadSiblingAssemblies()` no longer depends only on `Assembly.Location`; it falls back to LabAPI plugin directories when LabAPI/Mono reports an empty or unstable path.
- Loli Harmony startup now patches per type with skip logging, so one broken legacy patch does not abort all later patches.
- `PrintPlayer` now skips itself when the old SCPLogs `GetRolePrint` target is unavailable.
- `Qurre.API.Core.Dispatch` now logs exact handler names for runtime event failures.
- Runtime compatibility fixes from live logs:
  - current `MapGeneration.RoomName` -> legacy `Qurre.API.Objects.RoomType` mapping, including `EzOfficeStoried -> EzUpstairsPcs`.
  - `CreatePickupEvent.Info.ItemId/Serial`, `Player`, and cancellation/destroy behavior.
  - `LegacyItemId.GetCategory()` for dynamic legacy calls.
  - `ModelPrimitive.Primitive` adapter for old `Primitive` lists.
  - `SObject(false)` adapter constructor path to avoid orphan GameObjects.

Verified:
- `scripts/deploy-local-plugin.ps1 -Build` -> OK.
- Live `LocalAdmin.exe 7777` startup -> dependencies load, Qurre-Shim loads/enables, event registry starts, server reaches `Waiting for players`.
- Latest checked log: `LocalAdmin Log 2026-06-09 11.47.16.txt`.

Remaining:
- `HideRaAuth` transpiler logs old-local-index failure; it returns original IL and does not stop startup.
- `Loli.Addons.AutoModeration.SaveLogs` Harmony target is skipped; target discovery must be ported or the feature disabled.
- Need player-join and real round-start gameplay smoke-pass next. Current pass proves startup, not full gameplay.

---

### 2026-06-09 (14) SAVED LOCALADMIN LOGS + NO-BACKEND RUNTIME HARDENING - Agent: Codex

Status: LIVE_START_SMOKE_PASS - current deployed build reaches `Waiting for players` with no startup NRE, no QurreSocket background send exception, and no Mirror `SpawnObject ... has no NetworkIdentity` spam in the latest smoke log.

User-provided live result:
- Player could join.
- Spawn/waiting behavior was wrong.
- Forced round start spawned briefly, then the server crashed.

Logs inspected:
- `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log 2026-06-09 12.47.02.txt`
- Older comparison logs around `11.53.44`, `12.38.34`, and `12.45.10`.

Root causes found from logs:
- `QurreSocket.NetSocket.Send` threw from a background thread because the old FYDNE socket backend is absent.
- `RequestPlayerListCommandEvent::Loli.DataBase.Modules.Admins.Prefixs` received a null `Sender`.
- `SpawnEvent::Loli.Scps.Scp0492Better.Spawn` assumed non-null player/tag/movement state.
- Damage/attack handlers expected legacy `Target` to be populated; the LabAPI bridge only populated `Player`/`Attacker`.
- Startup emitted Mirror warnings because legacy fake `Door`/`Lift` GameObjects were passed to `NetworkServer.Spawn` without `NetworkIdentity`.

Changed:
- `Loli.Core.Socket` is now `SafeSocket`.
  - Socket backend is disabled by default.
  - To intentionally use a restored backend, set `FYDNE_SOCKET_ENABLED=1` plus `FYDNE_SOCKET_IP`.
  - `On`, `Off`, and `Emit` are no-op guarded when disabled.
- `QurreShim.EventMap` now fills compatibility fields:
  - `Target=Player` for spawn/change-role/death/damage/attack events.
  - `Sender` for RA player-list events via guarded reflection over `ReferenceHub.queryProcessor`.
  - null spawning/spawned players are skipped before legacy handlers run.
- Added guards:
  - `Loli.DataBase.Modules.Admins.Prefixs` returns if `Sender` is still null.
  - `Loli.Scps.Scp0492Better` returns on null player/attacker and handles null tags.
  - `Loli.Builds.Models.Rooms.Range` tolerates missing/early `EzVent` and null players.
- Added `Loli.Builds.Models.SafeNetwork`.
  - Existing real network objects still use Mirror when they have `NetworkIdentity`.
  - Shim/fake model objects without `NetworkIdentity` are not passed into Mirror spawn/unspawn/destroy.
- Routed confirmed warning sources through `SafeNetwork`: custom Lift model, Server door item animation, and SurfaceObjects nuke-door replacement.

Verified:
- `scripts/deploy-local-plugin.ps1 -Build` -> OK, deployed to LabAPI global plugin/dependency folders.
- Hidden LocalAdmin smoke with port `7777` -> latest checked log:
  `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log 2026-06-09 16.53.15.txt`
- Smoke markers:
  - `Waiting for players=1`
  - `TypeLoadException=0`
  - `MissingMethodException=0`
  - `NullReferenceException=0`
  - `QurreSocket.NetSocket.Send=0`
  - `handler .*: Object reference=0`
  - `SpawnObject .* has no NetworkIdentity=0`

Remaining:
- Need user/player retest for actual forced round start; this pass verifies clean startup, not full gameplay.
- `HideRaAuth` still logs `Index - 0 < 0`; it is skipped/fallback-safe but not ported.
- `AutoModeration.SaveLogs` target method still does not resolve and is skipped.
- Missing original `Schemes/*.json` assets mean some old FYDNE builds are degraded or absent until assets are restored or rebuilt natively.

---

### 2026-06-09 (15) FOUNDER DATA LOST + REPLACEMENT SOCKET BACKEND - Agent: Codex

Status: SOCKET_BACKEND_FIRST_PASS - original FYDNE external state is treated as lost; a new QurreSocket-compatible backend now exists and passes standalone protocol and LocalAdmin startup integration smoke.

Context:
- User contacted the FYDNE founder.
- Founder reported that nothing was preserved: no database dump, no original backend state, no recoverable project data/assets.
- Recovery strategy is now replacement-first, not restoration-first.

Changed:
- Added `Core.RecoveryMode`, controlled by `FYDNE_RECOVERY_MODE` and defaulting to enabled for current recovery builds.
- Fixed force-start double-start path:
  - `Waiting.Coroutine` no longer calls `Round.Start()` after force-start or after the server has already left waiting state.
- Added one-player local test support:
  - In recovery mode, `RoundCheck` returns `End=false` for <=1 non-host players so local force-start testing is not immediately ended by FYDNE win conditions.
  - Future public deployments should set `FYDNE_RECOVERY_MODE=0`.
- Added real first-stage replacement backend under `backend/fydne-socket`:
  - Node TCP server on port `2467`.
  - Legacy QurreSocket frame format: `JSON.stringify({ ev, args }) + "⋠"`.
  - Persistent JSON store: `backend/fydne-socket/data/store.json`.
  - Handles: `SCPServerInit`, `database.get.data`, `database.get.stats`, `database.add.stats`, `database.internal.unsafe.set_level`, `database.get.adm.steams`, `database.get.donate.roles`, `database.get.donate.customize`, `database.get.donate.ra`, `database.get.nitro`, `database.get.patrol`, `server.database.clans`, `server.clearips`, `server.addip`, `server.leave`, `server.tps`, and admin audit events as accepted no-ops.
- Added `scripts/start-fydne-socket.ps1`.
- Updated `.env.example` with socket and recovery-mode variables.
- Updated `.gitignore` to exclude `backend/fydne-socket/data/`.

Verified:
- `scripts/deploy-local-plugin.ps1 -Build` -> OK and deployed.
- `node --check backend/fydne-socket/server.js` -> OK.
- Standalone protocol test -> OK: backend responded to connect/init/get data/add stats/get stats/donate roles/clans and persisted JSON store.
- LocalAdmin integration smoke with real socket enabled:
  - `FYDNE_SOCKET_ENABLED=1`
  - `FYDNE_SOCKET_IP=127.0.0.1`
  - log: `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log 2026-06-09 17.22.39.txt`
  - `Connected to Socket=2`
  - `Waiting for players=1`
  - `TypeLoadException=0`
  - `MissingMethodException=0`
  - `NullReferenceException=0`
  - `handler .*: Object reference=0`
  - `QurreSocket.NetSocket.Send=0`

Remaining:
- User needs to retest join + force-start with `backend/fydne-socket` running and socket env enabled.
- Missing original schematics must be rebuilt; no founder source remains.
- Backend is intentionally JSON-first. Move to SQLite/PostgreSQL only after gameplay loop is stable.

---

### 2026-06-09 (16) LIVE SPAWN DISCONNECT LOOP - Agent: Codex

Status: RUNTIME_HARDENING_PASS - local plugin rebuilt/deployed; likely auto-reconnect loop disabled in recovery mode.

User-provided live result:
- Server no longer restarts by itself.
- Player is thrown out a few seconds after spawn.
- Rejoining also disconnects after a few seconds.
- Sometimes spawn position is random or outside the map.

Logs inspected:
- Latest available LocalAdmin file:
  `%APPDATA%\SCP Secret Laboratory\LocalAdminLogs\7777\LocalAdmin Log 2026-06-09 18.24.49.txt`
- This log still used the old DLL from before the smoothing fix and contained a massive exception flood:
  `RuntimeBinderException: Cannot implicitly convert type 'UnityEngine.Vector3' to 'UnityEngine.Quaternion'`
  from `Loli.FixOnePrimitiveSmoothing.Update`.

New root-cause candidate:
- `Loli.Modules.Fixes.CheckPlayersPing()` checks `Player.LastSynced`.
- If it thinks the player has not synced for >1s, it schedules `FastReconnect.Process(pl)`.
- `FastReconnect.Process()` stores state, clears inventory, moves the player to `Vector3.zero`, sets spectator, and calls `pl.Client.Reconnect()`.
- On the current LabAPI/QurreShim bridge, `LastSynced` may not preserve old Qurre semantics, so this can create a false-positive reconnect loop that matches the user's symptoms.

Changed:
- `Loli.Modules.Fixes.CheckPlayersPing()` now returns immediately when `Core.RecoveryMode` is enabled.
- `Loli.Addons.FastReconnect.Join()` and `FastReconnect.Process()` now return immediately when `Core.RecoveryMode` is enabled.

Verified:
- `scripts/build-plugin.ps1` -> OK, 0 errors.
- `scripts/deploy-local-plugin.ps1` -> OK.
- Deployed `Loli.dll` SHA256:
  `DA0873FFD6BF593127FF0AEEA5516F8950764AC449E80FA1F16B69BC98E0F4F6`

Next:
- User should retest join + force-start with the newly deployed DLL.
- If disconnect persists, inspect the new log first; the previous latest log is stale relative to this patch.
- If disconnect is fixed but spawn remains bad, next target is the spawn-position chain:
  `Waiting.cs`, `AdminRoom.cs`, `Range.cs`, `Gate3.cs`, `FastReconnect.cs`, `Spawn.cs`, `SpawnManager.cs`, `Fixes.cs`.

---

### 2026-06-09 (17) DETAILED ARCHITECTURE MAP - Agent: Codex

Status: DOCUMENTATION_PASS - added detailed runtime and module architecture documents after user requested a much deeper map than the previous tactical repair notes.

Added:
- `docs/10_RUNTIME_EXECUTION_MAP.md`
  - Describes server startup from LocalAdmin/SCP:SL -> LabAPI -> QurreShim -> Loli.
  - Documents QurreShim event registry, priority order, Loli.Core.Enable, global cycles, waiting phase, join phase, round start, spawn chain, role changes, damage/death, interactions, commands, socket, notifications, end/restart, and current recovery branches.
- `docs/11_MODULE_CATALOG.md`
  - Module-by-module catalog for Core, Modules, Builds, Spawns, DataBase, Addons, Concepts, Scps, Patches, HintsCore, Logs, Webhooks, Controllers.
  - Marks responsibilities, events/commands/socket where relevant, and recovery risk.
- `docs/12_EVENT_COMMAND_INDEX.md`
  - Static index for 398 `[EventMethod]` handlers by event type/count.
  - Includes command registrations, socket contract, Harmony patch groups, high-risk position/role/round/Allowed mutators, and debug order for spawn/disconnect.
- Linked these documents from `docs/FYDNE_ARCHITECTURE.md`.
- Updated `TODO.md` with this documentation pass.

Important findings preserved in docs:
- `RoundEvents.Waiting`: 78 handlers.
- `PlayerEvents.Spawn`: 43 handlers.
- `RoundEvents.Start`: 29 handlers.
- `PlayerEvents.ChangeRole`: 25 handlers.
- High-risk spawn mutators: `AdminRoom`, `Range`, `Hacker`, `SerpentsHand`, `Fixes.FixZombieSpawn`, `FastReconnect` (recovery-disabled), `Gate3`, `AntiCringeUpdates`.

Next:
- If the next live test still has wrong spawn/disconnect, add recovery-mode spawn tracing around `EventMap.OnSpawning`, `AdminRoom.SpawnChangePos`, `Range.SpawnChangePos`, `Hacker.FixPos`, `SerpentsHand.Spawn`, and `Fixes.FixZombieSpawn`.
