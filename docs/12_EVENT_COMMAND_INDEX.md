# FYDNE event, command, socket and patch index

Дата среза: 2026-06-09.

Индекс построен статическим проходом по `plugin/Loli/**/*.cs`.

## 1. EventMethod inventory

Всего найдено: 398 `[EventMethod(...)]`.

| Event type | Handlers |
|---|---:|
| `RoundEvents.Waiting` | 78 |
| `PlayerEvents.Spawn` | 43 |
| `RoundEvents.Start` | 29 |
| `PlayerEvents.ChangeRole` | 25 |
| `PlayerEvents.Join` | 18 |
| `RoundEvents.End` | 14 |
| `PlayerEvents.Dead` | 13 |
| `PlayerEvents.InteractDoor` | 12 |
| `RoundEvents.Restart` | 12 |
| `PlayerEvents.Damage` | 12 |
| `MapEvents.DamageDoor` | 10 |
| `PlayerEvents.Attack` | 9 |
| `ServerEvents.RemoteAdminCommand` | 8 |
| `PlayerEvents.Leave` | 7 |
| `MapEvents.LockDoor` | 6 |
| `MapEvents.OpenDoor` | 6 |
| `PlayerEvents.Escape` | 5 |
| `PlayerEvents.PickupItem` | 5 |
| `PlayerEvents.Dies` | 4 |
| `PlayerEvents.InteractGenerator` | 4 |
| `PlayerEvents.UsedItem` | 3 |
| `EffectEvents.Enabled` | 3 |
| `ScpEvents.Scp173AddObserver` | 3 |
| `MapEvents.CreatePickup` | 3 |
| `PlayerEvents.ChangeItem` | 3 |
| `PlayerEvents.Kick` | 3 |
| `ServerEvents.GameConsoleCommand` | 3 |
| `AlphaEvents.Detonate` | 3 |
| `ScpEvents.Scp914UpgradePlayer` | 3 |
| `ScpEvents.Scp106Attack` | 2 |
| `EffectEvents.Disabled` | 2 |
| `ScpEvents.Scp096AddTarget` | 2 |
| `AlphaEvents.Stop` | 2 |
| `PlayerEvents.Cuff` | 2 |
| `PlayerEvents.Ban` | 2 |
| `PlayerEvents.InteractLift` | 2 |
| `PlayerEvents.DroppedItem` | 2 |
| `PlayerEvents.CheckReserveSlot` | 1 |
| `PlayerEvents.CheckWhiteList` | 1 |
| `ServerEvents.RequestPlayerListCommand` | 1 |
| `PlayerEvents.Banned` | 1 |
| `ServerEvents.CheaterReport` | 1 |
| `ScpEvents.Scp079Recontain` | 1 |
| `RoundEvents.ForceStart` | 1 |
| `ScpEvents.GeneratorStatus` | 1 |
| `PlayerEvents.UseItem` | 1 |
| `ScpEvents.Scp914UpgradePickup` | 1 |
| `MapEvents.TriggerTesla` | 1 |
| `ServerEvents.LocalReport` | 1 |
| `PlayerEvents.DropAmmo` | 1 |
| `RoundEvents.Check` | 1 |
| `MapEvents.CorpseSpawned` | 1 |
| `PlayerEvents.UnCuff` | 1 |
| `ScpEvents.Scp049RaisingStart` | 1 |
| `ScpEvents.ActivateGenerator` | 1 |
| `PlayerEvents.JailbirdTrigger` | 1 |
| `PlayerEvents.UsingRadio` | 1 |
| `MapEvents.WorkStationUpdate` | 1 |
| `PlayerEvents.InteractLocker` | 1 |
| `PlayerEvents.ChangeSpectate` | 1 |
| `PlayerEvents.InteractWorkStation` | 1 |
| `AlphaEvents.Start` | 1 |
| `MapEvents.LczDecontamination` | 1 |
| `ScpEvents.Scp049RaisingEnd` | 1 |
| `ScpEvents.Scp079NewLvl` | 1 |
| `PlayerEvents.PrePickupItem` | 1 |
| `ScpEvents.Scp096SetState` | 1 |
| `PlayerEvents.DropItem` | 1 |
| `ScpEvents.Scp173EnableSpeed` | 1 |
| `ScpEvents.Scp173RemovedObserver` | 1 |

Note: the static scan also saw two malformed/empty attribute matches in `Builds/Models/Lift.cs`; those correspond to commented or unusual multi-event syntax and should not be treated as real event types without manual verification.

## 2. High-impact event chains

### `RoundEvents.Waiting`

Purpose:

- reset all round state;
- build custom rooms;
- initialize UI;
- reset DB/donate/cosmetic state;
- prepare special concepts.

Important handlers:

- `Builds.Load.Waiting[int.MaxValue]`
- `StationsManager.Refresh[int.MaxValue]`
- `HintsCore.Worker.Waiting[int.MaxValue]`
- `Hackers.Control.Refresh[int.MaxValue]`
- `Hackers.MainServers.Refresh[int.MaxValue]`
- `AdminRoom.Load[0]`
- `Range.Waiting[0]`
- `Gate3.Load[0]`
- `SurfaceObjects.Load[0]`
- `VertPlace.Load[0]`
- `NuclearAttack.Builds.Load[0]`
- `Scp008.RoomsData.Init[0]`
- `Waiting.WaitingMethod[0]`
- `Hackers.Control.Load[int.MinValue]`
- `Hackers.MainServers.Load[int.MinValue]`
- concept hint refreshers `[int.MinValue]`.

Main failure modes:

- invalid custom geometry;
- stale static state;
- hardcoded room positions not matching current SCP:SL map;
- missing schematics.

### `PlayerEvents.Join`

Purpose:

- create UI;
- load player data;
- set waiting Tutorial role;
- initialize chat/stats/cache;
- announce IP/user to backend.

Important handlers:

- `HintsCore.Worker.Join[int.MaxValue]`
- `HintsCore.Fixer.Events.Join[9]`
- `Caches.Join[0]`
- `Chat.Main.Join[0]`
- `Loader.Join[0]`
- `Levels.Join[0]`
- `Waiting.Join[0]`
- `Another.DoSpawn/ChangeNick/BlockKids[0]`
- `Hints.Logo/MainInfo/Waiting.Join[-1]`
- `VoteRestart.Join[-1]`
- `FastReconnect.Join[0]` - disabled in recovery.

Main failure modes:

- backend returns invalid JSON;
- waiting role assigned but AdminRoom point invalid;
- FastReconnect false-positive if recovery guard disabled.

### `PlayerEvents.Spawn`

Purpose:

- final spawn position;
- role inventory;
- donate cosmetics;
- SCP/concept role hooks;
- hints/UI;
- custom unit gating.

High-impact handlers:

- `AntiCringeUpdates.DontSpawnInLcz[int.MaxValue]`
- `AdminRoom.SpawnChangePos[666]`
- `Range.SpawnChangePos[666]`
- `Scientists.FixTags[1]`
- `Spawn.Update[0]`
- `Force.Spawn[0]`
- `Scp0492Better.Spawn[0]`
- `Scp079Better.Spawn[0]`
- `Hacker.FixPos[0]`
- `SerpentsHand.Spawn[0]`
- `CO2.Spawn[0]`
- `NuclearAttack.CPIR.Spawn[0]`
- `Customize.Spawn[0]`
- `Levels.Spawn[0]`
- `RealDClass.RealName[-1]`
- `Scientists.Spawn[-2]`
- `CustomUnits.Spawn[int.MinValue]`
- `Fixes.FixZombieSpawn[int.MinValue]`

Main failure modes:

- `ev.Position` overwritten by hardcoded custom area;
- delayed inventory/scale code runs after role already changed;
- concept roles depend on old static state.

### `PlayerEvents.ChangeRole`

Purpose:

- role tags/names;
- SCP reworks;
- concept state;
- donate cosmetics;
- cleanup of old role effects.

High-impact handlers:

- `AntiNewYear.RemoveCringe[int.MaxValue]`
- `OpenSCPs.Spawn[0]`
- `RPNames.Spawn[0]`
- `SafeSystem.FixTags[0]`
- `Scp096Rework.Spawn[0]`
- `Scp106Rework.Spawn[0]`
- `Scp173Rework.Spawn[0]`
- `CO2.Spawn[0]`
- `Hacker.HackerZero[0]`
- `SerpentsHand.Spawn[0]`
- `Customize.Spawn[0]`
- `Glow/Nimb.RoleChange[0]`
- `RealDClass.FixTagsEvent[-1]`
- `Scientists.FixTags[-2]`
- `Sweep.SweepFixTag[-2]`
- `Glow/Nimb.UnspawnForDog[int.MinValue]`

Main failure modes:

- same legacy handler receives both pre-role and post-role events through shim;
- `RoleInformation.Role = ...` direct assignment bypasses some vanilla spawn flags;
- custom role state can persist after death/round restart.

### `RoundEvents.Start`

Purpose:

- start per-round coroutines;
- spawn manager;
- event systems;
- special roles and cutscenes;
- watchdogs.

High-impact handlers:

- `AirDrop.RunAirDropCoroutine[0]`
- `AutoModeration.Register[0]`
- `CustomUnits.Start[0]`
- `RealCuffs.SpawnCuffs[0]`
- `Sweep.RoundStart[0]`
- `VoteRestart.RoundStart[0]`
- `Lift.SpawnDoors[0]`
- `NuclearAttack.CPIR.SelectSpy[0]`
- `Scp008.SerpentsHand.Started[0]`
- `Clear.StartRefresh[0]`
- `Fixes.FixNotSpawn[0]`
- `ChaosInsurgency.Refresh[0]`
- `MobileTaskForces.Refresh[0]`
- `SpawnManager.SpawnCor[0]`
- `Cutscene.Init[-1]`
- `RolePlay.Modules.MainGuard[-2]`
- `Scientists.RoundStart[-2]`

Main failure modes:

- special role code assumes enough players;
- FixNotSpawn sees local solo test as broken spawn;
- concepts start coroutines that rely on rooms/geometry.

### `RoundEvents.End` / `Restart`

Purpose:

- stop coroutines;
- flush stats/time;
- clear state;
- reset concept modules.

High-impact handlers:

- `AutoModeration.UnRegister`
- `EndStats.End`
- `Builds.Load.End`
- `Levels.XpRoundEnd`
- `Updater.End`
- `Another.ClearIps/RoundEnd`
- `Clear.ClearEndRefresh`
- `Fixes.FixFastEnd`
- `SpawnManager.SpawnCor2`
- `Scp008.ControlRoom.RestartDestroy[int.MinValue]`

Main failure modes:

- end/restart clears state needed by reconnect;
- watchdog restart if round ends too fast.

## 3. Command index

### Console/client commands

| Command(s) | File | Handler/purpose |
|---|---|---|
| `help`, `хелп`, `хэлп` | `Addons/Commands.cs` | help |
| `kys`, `kill` | `Addons/Commands.cs` | suicide |
| `tps` | `Addons/Commands.cs` | TPS/debug |
| `size` | `Addons/Commands.cs` | player scale command |
| `event` | `Addons/Commands.cs` | event mode |
| `textmute` | `Addons/Commands.cs` | global text mute |
| `roundmute` | `Addons/Commands.cs` | round mute |
| `roundff` | `Addons/Commands.cs` | friendly fire toggle |
| `picks` | `Addons/Commands.cs` | pickup counts |
| `level_set` | `Addons/Commands.cs` | unsafe backend level set |
| `rc` | `Addons/FastReconnect.cs` | disabled response |
| `s`, `force` | `Addons/Force.cs` | force/class helper |
| `res` | `Addons/VoteRestart.cs` | vote restart |
| `chat`, `чат` | `Addons/Chat/Cmd.cs` | chat visibility/control |
| `чай` | `Addons/Chat/Cmd.cs` | chat message |
| `б`, `ближний`, `pos` | `Addons/Chat/Cmd.cs` | positional chat |
| `п`, `пб`, `публичный`, `public` | `Addons/Chat/Cmd.cs` | public chat |
| `с`, `сз`, `союзный`, `ally` | `Addons/Chat/Cmd.cs` | ally chat |
| `к`, `км`, `командный`, `team` | `Addons/Chat/Cmd.cs` | team chat |
| `л`, `лс`, `личный`, `private` | `Addons/Chat/Cmd.cs` | private chat |
| `кл`, `клан`, `clan` | `Addons/Chat/Cmd.cs` | clan chat |
| `ад`, `адм`, `admin` | `Addons/Chat/Cmd.cs` | admin chat |
| `нрп`, `нонрп`, `nonrp` | `Addons/Chat/Cmd.cs` | nonrp chat |
| `escd` | `RolePlay/Modules.cs` | escape D-class command |
| `ssa` | `RolePlay/SafeSystem.cs` | safety/system command |
| `priest`, `pri`, `св` | `Ragnarok/Priest.cs` | become priest |
| `believe`, `bel`, `уверовать` | `Ragnarok/Priest.cs` | believer |
| `pray`, `пр`, `призыв` | `Ragnarok/Priest.cs` | ritual |
| `xp`, `lvl`, `money`, `stats` | `DataBase/Levels.cs` | stats/economy |
| `pay`, `пей`, `пэй` | `DataBase/Levels.cs` | money transfer |
| `bug`, `bag`, `баг` | `Modules/BugReport.cs` | bug report |
| `079` | `Scps/Scp079Better.cs` | SCP-079 abilities |
| `co2group` | `Spawns/SpawnManager.cs` | MRP-only CO2 group |

### RemoteAdmin commands

| Command(s) | File | Handler/purpose |
|---|---|---|
| `bp`, `backup_power` | `Core.cs` | backup power |
| `list`, `stafflist` | `Addons/Commands.cs` | custom player/staff list |
| `ob`, `oban` | `Addons/OfflineBan.cs` | offline ban |
| `ow`, `audio` | `Hackers/OmegaWarhead.cs` | omega warhead/audio |
| `hidetag` | `DataBase/Modules/Donate.cs` | disabled anti-hide-tag |
| `bring`, `ban` | `DataBase/Modules/Patrol.cs` | patrol moderation |
| `hacker` | `Spawns/ChaosInsurgency.cs` | hacker spawn |
| `server_event` | `Spawns/SpawnManager.cs` | admin event spawn |
| `ci`, `mtf` | `Spawns/SpawnManager.cs` | force CI/MTF |

RemoteAdmin command interceptors:

- `Donate.Ra` intercepts `give`, `forceclass`, `pfx`.
- `Patrol.Force` intercepts patrol/admin commands.
- `Fixes.FixCrashes` blocks dangerous server-console `forceclass` and `give`.
- `CrashProtect.SetGroup` blocks selected group/permission abuse.

## 4. Socket contract

### Outgoing events

| Event | Source | Meaning |
|---|---|---|
| `SCPServerInit` | `Core` | server auth/init |
| `server.clearips` | `Core`, `Another` | clear online IP list |
| `server.addip` | `Core`, `Waiting` | add online player/IP |
| `server.join` | `Waiting` | player joined |
| `server.leave` | `Another` | player left |
| `server.tps` | `Cycle` | heartbeat |
| `server.database.clans` | `Loader` | request clan tags |
| `database.get.data` | `Loader` | request user data |
| `database.get.donate.roles` | `Loader` | request donor roles |
| `database.get.donate.customize` | `Loader` | request cosmetics |
| `database.get.nitro` | `Loader` | request nitro |
| `database.get.donate.ra` | `Loader` | request RA donor data |
| `database.get.adm.steams` | `Admins` | request admin steam ids |
| `database.get.stats` | `Levels` | request stats |
| `database.add.stats` | `Stats` | add stats/xp/money |
| `database.add.time` | `Updater` | add playtime |
| `database.internal.unsafe.set_level` | `Commands` | debug/admin level set |
| `database.remove.admin` | `CrashProtect`, `AntiCrash` | admin removal/audit |
| `database.admin.ban` | `Admins` | admin ban stat |
| `database.admin.kick` | `Admins` | admin kick stat |

### Incoming events

| Event | Consumer | Meaning |
|---|---|---|
| `connect` | `Core` | socket connected |
| `token.required` | `Core` | auth required |
| `SCPServerInit` | `Admins` | init ack, then ask admins |
| `socket.database.clans` | `Loader` | clan tag list |
| `database.get.data` | `Loader` | user data response |
| `database.get.stats` | `Levels`, `Stats` | stats response |
| `database.update.zero.money` | `Stats` | money depleted response |
| `database.get.donate.customize` | `Customize` | customization data |
| `database.get.nitro` | `Loader` | nitro data |
| `database.get.donate.roles` | `Loader` | donor roles |
| `database.get.donate.ra` | `Loader` | donor RA data |
| `database.get.patrol` | `Patrol` | patrol status |
| `database.get.adm.steams` | `Admins` | admin id list |
| `ChangeFreezeSCPServer` | `Loader` | live donor role freeze/unfreeze |

## 5. Harmony patch index

Patch application:

- `Core.PatchAllSafely()` creates one Harmony instance.
- It calls `CreateClassProcessor(type).Patch()` for every type.
- Failures are logged and skipped per type.

Key patch groups:

| Group | Files | Purpose | Risk |
|---|---|---|---|
| Remote admin hardening | `FixBackdoors/High.cs`, `Low.cs`, `VeryHigh.cs`, `HideRaAuth.cs`, `PatrolGetBans_TEMPLATE.cs` | command permission/backdoor control | high |
| Server internals | `GetVerkey.cs`, `NotFileLogs.cs`, `FixDelayed.cs`, `FixNetCrash.cs` | token/log/network/delayed-connection internals | high |
| Voice/CASSIE | `FixSpoiled.cs`, `ScpSpeaks.cs`, `_DisableCassie.cs`, `DisableGunAudio.cs` | voice and announcement behavior | high |
| Tags/roles | `HideTagPatch.cs`, `Logs/Patch/PrintPlayer.cs`, `Logs/Patch/Invisible.cs` | tag/log display | medium |
| SCP internals | `AntiCringeUpdates.cs`, `AntiNewYear.cs`, `FixPockerEscape.cs`, `Scp294/Patch.cs`, roleplay SCP patches | SCP-specific internals | high |
| Weapons | `RealisticArmory.cs` | item add/remove and firearm damage | high |
| UI | `HintsCore/Fixer/Patch.cs` | `Client.ShowHint` behavior | medium |

Known live warnings:

- `HideRaAuth` transpiler logs `Index - 0 < 0`.
- `AutoModeration.SaveLogs` target discovery is skipped.

## 6. High-risk mutators

These files directly change role, position, `Allowed`, kill/end/restart state, or backend authority.

### Position mutators

- `AdminRoom.SpawnChangePos`
- `Range.SpawnChangePos`
- `Range.FixGuardSpawn`
- `Gate3` door teleport
- `Hacker.FixPos`
- `SerpentsHand.Spawn`
- `Fixes.FixZombieSpawn`
- `FastReconnect.Process` - disabled in recovery
- `Patrol.Bring`
- `FacilityManager`
- `AntiCringeUpdates.DontSpawnInLcz`

### Role mutators

- `Waiting.Join`
- `Waiting.StartedClear`
- `Force`
- `SpawnManager`
- `MobileTaskForces`
- `ChaosInsurgency`
- `Donate.Ra forceclass`
- `OpenSCPs`
- `Scientists`
- `RealDClass`
- `Sweep`
- `CO2`
- `Hacker`
- `SerpentsHand`
- `Scp0492Better`

### Round/end/restart mutators

- `Waiting.WaitingMethod -> Round.Start()`
- `AutoAlpha -> Round.End()`
- `OmegaWarhead -> Round.End()`
- `NuclearAttack.Builds -> Round.End()`
- `CO2 -> Round.End()`
- `Fixes.* -> RestartCrush()`
- `VoteRestart -> RestartCrush()`
- `Commands.cs` debug/admin paths -> `Round.End()` or `Server.Restart()`
- `Core.PluginDisable -> Server.Restart()`

### Allowed/cancel mutators

- `AntiCheat`
- `BackupPower`
- `Builds.Load`
- `Builds.Models.Lift`
- `Scp008.ControlRoom/TubeRoom`
- `RolePlay.Modules`
- `OpenSCPs`
- `RealCuffs`
- `Fixes`
- `Donate`
- `Patrol`
- `Reports`
- `CommandsSystem`
- `Scp294.Events`

## 7. Debugging order for the current spawn/disconnect problem

Use this order after each live test:

1. Confirm loaded DLL hash in LabAPI global plugins.
2. Inspect newest LocalAdmin log, not an old file.
3. Search for:
   - `RuntimeBinderException`
   - `NullReferenceException`
   - `MissingMethodException`
   - `handler SpawnEvent`
   - `handler ChangeRoleEvent`
   - `RestartCrush`
   - `FastReconnect`
   - `LastSynced`
4. If no exception but position wrong, instrument:
   - `EventMap.OnSpawning`
   - `AdminRoom.SpawnChangePos`
   - `Range.SpawnChangePos`
   - `Hacker.FixPos`
   - `SerpentsHand.Spawn`
   - `Fixes.FixZombieSpawn`
5. If role becomes Spectator unexpectedly, inspect:
   - `Waiting.StartedClear`
   - `FastReconnect.Process`
   - `RoundCheck`
   - `FixNotSpawn`
   - `Donate/Force/OpenSCPs/Sweep`.

