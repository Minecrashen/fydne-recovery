# FYDNE module catalog

Дата среза: 2026-06-09.

Этот документ разбирает `plugin/Loli` как набор внутренних подсистем. В коде это один assembly `Loli.dll`, но архитектурно каждый статический класс/папка работает как отдельный plugin-module.

## 1. Сводка по размеру

Всего в `plugin/Loli`: 198 `.cs` файлов.

| Подсистема | Файлов | Назначение |
|---|---:|---|
| `Addons` | 54 | команды, чат, донатные фичи, anti-cheat, roleplay helpers, random buffs |
| `Scps` | 37 | SCP reworks, SCP-294 и напитки |
| `Concepts` | 19 | большие игровые сценарии: CO2, Hackers, NuclearAttack, Ragnarok, Scp008 |
| `Patches` | 17 | Harmony patches на внутренности SCP:SL |
| `Builds` | 13 | custom geometry, rooms, lifts, doors, props |
| `DataBase` | 13 | user data, levels, donate, admins, patrol, stats |
| `Modules` | 13 | core cycle, waiting, fixes, cleanup, round rules, voices |
| `HintsCore` | 12 | custom hint UI framework |
| `Logs` | 5 | reports, bans, log rewriting |
| `Spawns` | 3 | MTF/CI/custom waves |
| `Webhooks` | 3 | Discord/Telegram helpers |
| `Controllers` | 2 | small controllers for alpha/concepts |

## 2. Core

### `Core.cs`

Назначение:

- main `[PluginInit]` entrypoint.
- config/env bootstrap.
- socket wrapper.
- command base registration.
- patrol/admin/scp294 initialization.
- Harmony patching.

Key state:

- `ServerName`
- `ServerID`
- `DonateID`
- `BlockStats`
- `Ticks`, `TicksMinutes`
- `RecoveryMode`
- `SocketEnabled`
- `APIUrl`, `CDNUrl`, `ApiToken`, `SteamToken`

External dependencies:

- `QurreSocket.Client`
- `JsonConfig`
- `HarmonyLib`
- LabAPI/Qurre wrappers.

Recovery status:

- Works enough for startup.
- Socket guarded by `SafeSocket`.
- Harmony guarded per type.
- `PluginDisable => Server.Restart()` remains dangerous for plugin reload scenarios.

## 3. QurreShim-dependent event model

All modules below depend on these shim guarantees:

- legacy event enum maps to correct event struct;
- cancellable fields write back to LabAPI:
  - `Allowed`
  - `Position`
  - `Role`
  - `Damage`
  - command `Reply`
- wrappers do not return null where legacy code expects real objects;
- command bridge supplies `Player`, `Sender`, `Args`, `Name`.

If a module "looks correct" but still breaks at runtime, first check whether the shim event payload has all fields the legacy handler expects.

## 4. Modules

### `Modules/Waiting.cs`

Events:

- `PlayerEvents.Join`
- `RoundEvents.ForceStart`
- `RoundEvents.Waiting`

Responsibilities:

- welcome hint.
- delayed Tutorial role assignment in waiting.
- socket `server.join` and `server.addip`.
- auto-start coroutine after countdown.
- `StartedClear()` converts Tutorial waiters into Spectator before start.

Critical branches:

- If round remains waiting and timer reaches start, calls `Round.Start()`.
- If force-start happens, only `StartedClear()` runs.
- If AdminRoom spawn point is invalid, Tutorial waiters spawn in air.

Recovery status:

- double-start risk already reduced.
- still tightly coupled to old waiting/admin zone.

### `Modules/Cycle.cs`

Events:

- `RoundEvents.Waiting` only as null-call to force static registration.

Responsibilities:

- starts global coroutines:
  - 1s cycle.
  - 5s slow cycle.
  - broadcast sender.
  - TPS counter.
  - socket TPS sender.

Critical branches:

- watchdog calls into `Fixes`.
- `CheckPlayersPing()` is now recovery-disabled.

Recovery status:

- necessary core loop.
- watchdog parts must remain gated until runtime semantics are verified.

### `Modules/Fixes.cs`

Events: 16.

Main responsibilities:

- fast-end watchdog.
- round timer freeze watchdog.
- broadcast/game script watchdog.
- no-player freeze watchdog.
- zombie spawn fallback.
- anti-lift/escape/cuff/drop-ammo edge cases.
- invisible item fix.
- ragdoll scale fix.
- pickup freeze fix.
- blocks dangerous LocalAdmin `forceclass`/`give`.
- `FixNotSpawn()` watchdog after round start.
- player ping auto-reconnect check.
- all-player freeze detector.

Critical mutations:

- `ev.Position = GetZombiePoint()`.
- many `ev.Allowed = false`.
- `Extensions.RestartCrush(...)`.
- old `CheckPlayersPing()` can call `FastReconnect.Process()`.

Recovery status:

- `CheckPlayersPing()` disabled in recovery.
- `RestartCrush()` recovery-safe.
- `FixNotSpawn()` still diagnoses spawn failure but should not restart in recovery.

### `Modules/RoundCheck.cs`

Events:

- `RoundEvents.Check`

Responsibilities:

- custom win-condition logic.
- ignores players in admin/range invisible zones.
- checks tutorial/custom/faction state.

Recovery status:

- QurreShim also blocks solo round end in recovery for <=1 human.
- Production rules need retest with real players.

### `Modules/Another.cs`

Events: 11.

Responsibilities:

- leave cleanup/socket leave.
- clear IPs on round end.
- waiting UI object tweak.
- Tesla/FF/end-of-round behavior.
- join-time spawn/change nick/block checks.
- damage hint/feedback logic.

Risks:

- broad utility module with many unrelated behaviors.
- contains player role and FF mutations.

### `Modules/Clear.cs`

Events:

- `RoundEvents.Start`
- `RoundEvents.End`

Responsibilities:

- cleanup warhead/items/ragdolls/pickups.
- tracks protected pickups.

Risks:

- can delete pickups created by custom build modules if serial tracking is wrong.

### `Modules/AntiCrash.cs`

Events:

- server/report/admin-related guard.

Responsibilities:

- blocks/handles abusive actions.
- emits `database.remove.admin`.

Recovery status:

- backend event currently accepted as no-op/audit.

### `Modules/BugReport.cs`

Commands:

- `bug`
- `bag`
- `баг`

Responsibilities:

- player bug report command.

Recovery status:

- usable if webhook/config exists; otherwise degraded.

### `Modules/Voices/*`

Files:

- `_DisableCassie.cs`
- `Core.cs`
- `DontPlayInPodval.cs`
- `Generators.cs`
- `ScpDead.cs`

Responsibilities:

- custom voice/audio bot layer.
- disable/replace selected CASSIE behavior.
- generator/recontain/death announcements.
- avoid audio in admin basement/podval.

Risks:

- audio shim is partial.
- real Opus/Mirror voice output is not fully restored.
- Harmony targets for old CASSIE/SCP internals are version-sensitive.

## 5. Builds

### `Builds/Load.cs`

Events:

- `RoundEvents.Waiting` priority `int.MaxValue`
- `RoundEvents.End`
- `PlayerEvents.PrePickupItem`
- `MapEvents.DamageDoor`
- `MapEvents.LockDoor`
- `MapEvents.OpenDoor`

Responsibilities:

- global build reset.
- custom door/lift coroutine cleanup.
- initializes `RadarLoc` and `Bashni`.
- blocks fake static door interactions.
- maps custom door pickups to `Server.Doors`.

Recovery risk:

- high. This is foundational for custom geometry.

### `Builds/Models/*`

Files:

- `Lamp.cs`
- `Lift.cs`
- `Radar.cs`
- `SafeNetwork.cs`
- `Server.cs`

Responsibilities:

- custom model primitives/lights/doors/lifts/radar/server room pieces.
- custom lift interaction and teleport/movement.
- `SafeNetwork` prevents fake GameObjects without `NetworkIdentity` from being spawned through Mirror.

Critical events:

- `Lift` has 8 event hooks around alpha, map door events, and player door interaction.

Recovery risk:

- high. Any missing `NetworkIdentity`, wrong transform, or invalid primitive state affects live clients.

### `Builds/Models/Rooms/AdminRoom.cs`

Events:

- `PlayerEvents.Spawn` priority `666`
- `PlayerEvents.DroppedItem`
- `MapEvents.CreatePickup`
- `PlayerEvents.PickupItem`
- `RoundEvents.Waiting`

Responsibilities:

- builds FYDNE admin/waiting room.
- sets `TutorialSpawnPoint`.
- sets `WaitingSpawnPoint`.
- redirects Tutorial/Scp3114 spawn.
- removes dropped items/ammo near waiting room.
- spawns shooting range weapons.

Critical branch:

- During round waiting, Tutorial spawn goes to `WaitingSpawnPoint`.
- Outside waiting, Tutorial spawn goes to `TutorialSpawnPoint`.

Recovery risk:

- very high. This directly matches current "spawn in air" symptom if room geometry/point is invalid.

### `Builds/Models/Rooms/Range.cs`

Events:

- `PlayerEvents.Spawn` priority `666`
- `PlayerEvents.Damage` priority `-2`
- `RoundEvents.Waiting`
- `PlayerEvents.Spawn` normal priority for guard fallback.

Responsibilities:

- builds range/tir zone.
- computes `ChaosSpawnPoint`, `DonateSpawnPoint`.
- detects invisible/check areas.
- redirects donate/chaos spawns.
- blocks damage in protected range.

Recovery risk:

- high. Hardcoded positions and VentRoom dependency.

### Other room modules

| File | Responsibility | Runtime risk |
|---|---|---|
| `Gate3.cs` | custom Gate3, door teleport, anti-machine death | high: hardcoded teleport positions |
| `Bashni.cs` | tower/surface custom props | medium-high |
| `RadarLoc.cs` | radar location props | medium |
| `SurfaceObjects.cs` | custom surface replacement objects | medium |
| `VertPlace.cs` | evacuation helicopter/route/workstation | high: moves players and sets escaped spectator |

## 6. Spawns

### `Spawns/SpawnManager.cs`

Events:

- `RoundEvents.Waiting`
- `RoundEvents.Start`
- `RoundEvents.End`
- `PlayerEvents.Damage`

Commands:

- RA `server_event`
- RA `ci`
- RA `mtf`
- console `co2group` under MRP.

Responsibilities:

- periodic custom spawn wave scheduler.
- admin manual MTF/CI spawn.
- spawn protection tag.
- access checks based on `Data.Users`.

Risk:

- depends on database roles for admin access.
- custom spawn waves mutate roles heavily.

### `Spawns/MobileTaskForces.cs`

Events:

- refresh on waiting/start/restart.
- dead/attack hooks.

Responsibilities:

- custom MTF squad composition.
- role assignment.
- inventory/custom info setup.
- custom unit behavior.

Risk:

- high, many delayed role/inventory operations.

### `Spawns/ChaosInsurgency.cs`

Commands:

- RA `hacker`

Responsibilities:

- custom CI spawn wave.
- can spawn hacker concept.

Risk:

- medium-high, depends on concept modules and spawn points.

## 7. DataBase

### `DataBase/Modules/Loader.cs`

Events:

- `RoundEvents.Waiting`
- `PlayerEvents.Join`

Socket:

- emits:
  - `server.database.clans`
  - `database.get.data`
  - `database.get.donate.roles`
  - `database.get.donate.customize`
  - `database.get.nitro`
- consumes:
  - `socket.database.clans`
  - `database.get.data`
  - `ChangeFreezeSCPServer`
  - `database.get.nitro`
  - `database.get.donate.roles`
  - `database.get.donate.ra`

Responsibilities:

- player data load.
- donor roles load.
- nitro/custom cosmetic load.
- clan loading.
- live freeze/unfreeze of donate roles.

Risk:

- high for full restoration.
- replacement backend currently returns minimal/default data.
- clan branch still contains old HTTP API calls, but new backend returns empty clan list.

### `DataBase/Levels.cs`

Events:

- join/spawn/end/escape/scp/dead XP hooks.

Commands:

- `xp`, `lvl`, `money`, `stats`
- `pay`, `пей`, `пэй`

Socket:

- `database.get.stats`
- `database.add.stats`
- `database.internal.unsafe.set_level`

Responsibilities:

- XP/money/level display.
- reward calculation.
- pay/transfer flow.

Risk:

- medium-high. Gameplay can run with defaults, but economy needs real persistent backend.

### `DataBase/Customize.cs`

Events:

- waiting, leave, spawn, change role, attack.

Socket:

- `database.get.donate.customize`

Harmony:

- patches `Adrenaline.OnEffectsActivated`.

Responsibilities:

- player scale.
- donate cosmetic effects.
- damage modifiers.

Risk:

- medium. Scale changes can cause collision/weirdness if role state is wrong.

### `DataBase/Modules/Admins.cs`

Events:

- reserve slot.
- whitelist.
- request player list.
- ban.
- kick.

Socket:

- `SCPServerInit`
- `database.get.adm.steams`
- `database.admin.ban`
- `database.admin.kick`

Responsibilities:

- admin steam list.
- reserve/whitelist.
- custom RA player list coloring/prefixes.
- public ban/kick messages.

Risk:

- medium. Needs backend for real admin list.

### `DataBase/Modules/Donate.cs`

Events:

- waiting.
- remote admin command intercept.

Commands:

- RA `hidetag`
- intercepts RA `give`, `forceclass`, `pfx`.

Responsibilities:

- donor item giving.
- donor forceclass.
- donor effects.
- limits/cooldowns.

Risk:

- high for public server balance/security.
- depends on `Data.Roles`, `Data.Users`, and backend correctness.

### Other DataBase modules

| File | Responsibility | Risk |
|---|---|---|
| `Module.cs` | prefix/mute/group helper logic | medium |
| `Modules/Data.cs` | in-memory static user/donate/clan state | medium |
| `Modules/Stats.cs` | stats socket responses and zero-money updates | medium |
| `Modules/Updater.cs` | playtime/session updates | medium |
| `Modules/Patrol.cs` | patrol/admin bring/ban/role RA commands | high: moderation/security |
| `Controllers/Glow.cs` | donate glow cosmetic controller | medium |
| `Controllers/Nimb.cs` | donate nimb cosmetic controller | medium |
| `Controllers/Star.cs` | star cosmetic state | low-medium |

## 8. Addons

### Commands/chat/admin

| File | Commands/events | Responsibility | Risk |
|---|---:|---|---|
| `Commands.cs` | 15 commands, 2 events | help/admin/debug commands, event mode, FF, level_set, size, TPS, internal tests | medium-high: has restart/end/debug paths |
| `CommandsSystem.cs` | command dispatch events | central console/RA command registry | high if command bridge wrong |
| `Chat/Cmd.cs` | 31 commands | aliases for public/private/team/clan/admin/nonrp chat | medium |
| `Chat/Main.cs` | join event | message routing, filtering, mute, Discord chat webhook | medium |
| `OfflineBan.cs` | RA `ob`, `oban` | offline ban helper | high: moderation |
| `VoteRestart.cs` | console `res` | player vote restart | medium; calls `RestartCrush` |
| `FastReconnect.cs` | console `rc`, join/start hooks | reconnect state restoration | high; disabled in recovery |

### Gameplay utility Addons

| File | Responsibility | Key events | Risk |
|---|---|---|---|
| `Spawn.cs` | role inventories, info flags, post-spawn loadout | Spawn | medium-high |
| `Force.cs` | player force/SCP/class spawn helpers | waiting/join/spawn/dies | high |
| `CustomUnits.cs` | blocks/marks custom units | waiting/start/spawn | medium |
| `RandomBuff.cs` | random round-wide buffs | waiting/join/spawn/attack | medium |
| `BackupPower.cs` | backup power/hack door/lift behavior | waiting/interact door/lift | medium-high |
| `AirDrop.cs` | air drop event | start/end/waiting | medium |
| `AntiCheat.cs` | exploit checks, webhook logging | waiting/door/106/pickup | high but should be audited for false positives |
| `CrashProtect.cs` | admin/crash abuse protection | RA/ban/kick/waiting | medium-high |
| `ScpHeal.cs` | heals SCPs based on movement/conditions | waiting + cycle | medium |
| `StationsManager.cs` | workstation events | restart/waiting/workstation | medium |
| `RemoteKeycard.cs` | remote keycard interactions | door/locker/generator | currently looks mostly commented/degraded |
| `RealisticArmory.cs` | weapon stats and Harmony item/damage patches | spawn + Harmony | high, version-sensitive |

### Hints Addons

| File | Responsibility |
|---|---|
| `Hints/Waiting.cs` | waiting screen hint block |
| `Hints/MainInfo.cs` | main info UI |
| `Hints/Logo.cs` | logo/timer UI |
| `Hints/ScpHint.cs` | SCP hint updates |
| `Hints/OverwatchHelp.cs` | overwatch help UI |
| `Hints/EndStats.cs` | end/death/damage/escape stats UI |
| `Hints/ClansRecs.cs` | clan recommendation/API-based info |

Risk:

- mostly UI, but depends on `HintsCore` and player state.
- `ClansRecs` can hit `APIUrl`; safe only if APIUrl empty or backend data controlled.

### RolePlay Addons

| File | Responsibility | Risk |
|---|---|---|
| `RolePlay/Modules.cs` | roleplay restrictions, D-class escape, radio, SCP door/generator rules | high gameplay impact |
| `RolePlay/RealDClass.cs` | real D-class identity/tags/inventory | medium |
| `RolePlay/Scientists.cs` | scientist variants/inventory/HID behavior | medium-high |
| `RolePlay/RPNames.cs` | role names/custom identity | low-medium |
| `RolePlay/RealCuffs.cs` | custom cuff system/display/item restrictions | high |
| `RolePlay/OpenSCPs.cs` | open/replace SCP roles | high |
| `RolePlay/SafeSystem.cs` | safety/system role tags | medium |
| `RolePlay/Safe079.cs` | generator/SCP-079 protection | medium |
| `RolePlay/FacilityManager.cs` | special scientist/spy role | medium-high |
| `RolePlay/Sweep.cs` | special MTF/O5/sweep events | high |
| `RolePlay/Cutscene.cs` | round-start cutscene corpses/props | medium |
| `RolePlay/Scp173Rework.cs` | SCP-173 rework | high |
| `RolePlay/Scp096Rework.cs` | SCP-096 rework | high |
| `RolePlay/Scp106Rework.cs` | SCP-106 room/recontain/door behavior | high |
| `RolePlay/Patches/*` | SCP voice/recontain/blink Harmony patches | high/version-sensitive |

## 9. Concepts

Concept modules are not small features; they are scenario systems with state, maps, roles, and often kill/end-round paths.

### `Concepts/CO2.cs`

Events:

- waiting.
- dead.
- change role.
- spawn.

Responsibilities:

- CO2 activation squad.
- custom MTF-like role group.
- custom inventories and spawn protection.
- late process can end round.

Risk:

- high. Heavy role/inventory mutations.

### `Concepts/Hackers/*`

Files:

- `Hacker.cs`
- `Control.cs`
- `MainServers.cs`
- `Panel.cs`
- `OmegaWarhead.cs`
- `HintsUi.cs`
- `Utils.cs`
- `HackMode.cs`

Commands:

- RA `ow`
- RA `audio`
- RA `hacker`

Responsibilities:

- Chaos hacker role.
- server room panels.
- door control.
- omega warhead flow.
- custom hints.

Critical mutations:

- `Hacker.FixPos` hardcodes spawn position `new(128, 990, 28)`.
- `OmegaWarhead` can kill players and call `Round.End()`.

Risk:

- very high. Depends on custom room geometry and hardcoded positions.

### `Concepts/NuclearAttack/*`

Files:

- `Builds.cs`
- `CPIR.cs`
- `HintsUi.cs`

Responsibilities:

- nuclear data download objectives.
- CPIR spy role.
- custom terminal/workstation interactions.
- kill everyone/surface logic.
- can call `Round.End()`.

Risk:

- high. Custom objective flow + hardcoded rooms + round-ending logic.

### `Concepts/Ragnarok/Priest.cs`

Commands:

- `priest`, `pri`, `св`
- `believe`, `bel`, `уверовать`
- `pray`, `пр`, `призыв`

Responsibilities:

- priest donor/special role.
- believer system.
- summoning ritual.
- custom broadcasts.

Risk:

- medium-high. Depends on donate roles and room checks.

### `Concepts/Scp008/*`

Files:

- `RoomsData.cs`
- `TubeRoom.cs`
- `ControlRoom.cs`
- `SerpentsHand.cs`
- `HintsUi.cs`
- `TubeRoomType.cs`

Responsibilities:

- SCP-008 custom rooms/vents/tubes.
- Serpents Hand role.
- control room activation/deactivation.
- blocks SCP attacks/observer/enrage in some cases.
- can spawn SCP-0492 and end round.

Critical mutations:

- `SerpentsHand.Spawn` can set `ev.Position = new Vector3(0, 302, 5)`.
- ControlRoom can call `Round.End()`.

Risk:

- very high. Requires custom geometry and full scenario retest.

## 10. Scps

### SCP reworks

| File | Responsibility | Risk |
|---|---|---|
| `Better914.cs` | SCP-914 player/pickup modifications | medium |
| `Scp0492Better.cs` | SCP-049-2 spawn/damage/doctor behavior | medium-high |
| `Scp079Better.cs` | custom SCP-079 console abilities | high |

`Scp079Better` command:

- `079`

Effects:

- teleport to SCP.
- blackout/door/light actions.
- poison room.
- energy/level checks.

Risk:

- depends on QurreShim `Scp079` wrapper accuracy.

### SCP-294

Files:

- `Scp294/API/*`
- `Scp294/Drinks/*`
- `Scp294/Events.cs`
- `Scp294/Patch.cs`
- `Scp294/Extensions.cs`

Responsibilities:

- drink registry.
- item use/drop flow.
- money charge.
- per-drink effects.

Notable drink effects:

- healing/stamina/effects.
- hints.
- instant kill drinks:
  - `Carbon`
  - `Death`
  - `LiquidHidrogen`
  - `Perfume`
  - `SulfuricAcid`

Risk:

- medium. Economy/backend needed for money; effects need live test.

## 11. Patches

Harmony is applied to all types through `PatchAllSafely()`.

High-risk patches:

| File | Target class/method | Status/Risk |
|---|---|---|
| `HideRaAuth.cs` | `RaPlayer.ReceiveData` transpiler | currently logs old IL index failure |
| `AutoModeration/SaveLogs.cs` | dynamic target | currently skipped |
| `FixNetCrash.cs` | `NetPacket.Verify` | private/old internals |
| `FixSpoiled.cs` | `VoiceTransceiver.ServerReceiveMessage` | voice internals |
| `ScpSpeaks.cs` | `VoiceTransceiver.ServerReceiveMessage` | voice internals |
| `GetVerkey.cs` | `ServerConsole.RefreshToken` | old server internals |
| `NotFileLogs.cs` | `ServerLogs.*` | logging internals |
| `DeleteDeadman.cs` | `DeadmanSwitch.OnUpdate` | old gameplay internals |
| `AntiCringeUpdates.cs` | SCP-3114/new year internals | version-sensitive |
| `RealisticArmory.cs` | item add/remove/firearm damage | high gameplay impact |
| `FixBackdoors/*` | command permissions/backdoor hardening | security-sensitive |
| `HideTagPatch.cs` | `ServerRoles` tag hiding | medium |
| `Scp294/Patch.cs` | `Scp207.OnEffectsActivated` | effect internals |

Recovery rule:

- Do not treat compile success as patch success.
- Each patch must be either:
  - confirmed against v14.2.7 live logs;
  - ported to new method names/signatures;
  - disabled intentionally.

## 12. HintsCore

Files:

- `Constants.cs`
- `DisplayBlock.cs`
- `MessageBlock.cs`
- `PlayerDisplay.cs`
- `Worker.cs`
- `Fixer/*`
- utilities.

Responsibilities:

- custom layout system for hints.
- per-player display blocks.
- worker coroutine.
- patch/fixer around `Client.ShowHint`.

Risk:

- medium. UI-only, but can spam/throw if player wrappers are invalid.

## 13. Logs/Webhooks

### Logs

| File | Responsibility |
|---|---|
| `Bans.cs` | ban/kick log hooks |
| `Reports.cs` | local/cheater report hints and Discord embed |
| `RewriteGlobals.cs` | log rewrite/bootstrap |
| `Patch/Invisible.cs` | Harmony around invisible logging |
| `Patch/PrintPlayer.cs` | SCPLogs role print bridge |

### Webhooks

| File | Responsibility |
|---|---|
| `Dishook.cs` | Discord webhook sender |
| `Embed.cs` | Discord embed DTO |
| `Telegram.cs` | Telegram HTTP sender |

Recovery status:

- webhook URLs should come from env/config only.
- empty/invalid URLs should no-op.
- real Discord/Telegram bots are not required for local gameplay smoke.

## 14. Controllers

### `Controllers/AlphaController.cs`

Events:

- `AlphaEvents.Start`
- `RoundEvents.Waiting`

Responsibility:

- anti-disable/start state for alpha warhead.

### `Controllers/ConceptsController.cs`

Events:

- `RoundEvents.Waiting`

Responsibility:

- reset concept-related global state.

## 15. Practical restoration order by module risk

### Keep enabled during recovery

- `Core`
- `QurreShim`
- `Cycle` with guarded watchdogs.
- `Waiting` but with AdminRoom monitored.
- `Builds` only if no exception/network spam.
- `CommandsSystem`
- basic `Levels`/`Loader` with replacement backend defaults.
- `SpawnManager` only for local smoke after vanilla spawn works.

### Quarantine or test one-by-one

- `FastReconnect` - currently disabled in recovery.
- `AutoModeration`
- `RealisticArmory`
- `Scp079Better`
- `Scp008`
- `Hackers`
- `NuclearAttack`
- `CO2`
- `Range/AdminRoom` teleport pieces if spawn remains invalid.

### Rebuild from scratch instead of direct port

- full original backend/database.
- original web dashboard state.
- old Discord/Telegram bot integrations.
- any Harmony patch whose target no longer exists or depends on private IL layout.
- old schematics if original JSON/assets are missing.

