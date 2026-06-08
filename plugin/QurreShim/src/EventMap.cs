using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using PArgs = LabApi.Events.Arguments.PlayerEvents;
using SArgs = LabApi.Events.Arguments.ServerEvents;
using WArgs = LabApi.Events.Arguments.WarheadEvents;
using Scp049Args = LabApi.Events.Arguments.Scp049Events;
using Scp079Args = LabApi.Events.Arguments.Scp079Events;
using Scp096Args = LabApi.Events.Arguments.Scp096Events;
using Scp106Args = LabApi.Events.Arguments.Scp106Events;
using Scp173Args = LabApi.Events.Arguments.Scp173Events;
using Scp914Args = LabApi.Events.Arguments.Scp914Events;
using PlayerHandlers = LabApi.Events.Handlers.PlayerEvents;
using ServerHandlers = LabApi.Events.Handlers.ServerEvents;
using WarheadHandlers = LabApi.Events.Handlers.WarheadEvents;
using Scp049Handlers = LabApi.Events.Handlers.Scp049Events;
using Scp079Handlers = LabApi.Events.Handlers.Scp079Events;
using Scp096Handlers = LabApi.Events.Handlers.Scp096Events;
using Scp106Handlers = LabApi.Events.Handlers.Scp106Events;
using Scp173Handlers = LabApi.Events.Handlers.Scp173Events;
using Scp914Handlers = LabApi.Events.Handlers.Scp914Events;
using LabPlayer = LabApi.Features.Wrappers.Player;

namespace Qurre.API
{
    internal static class EventMap
    {
        static readonly Dictionary<object, Type> _enumToStruct = new Dictionary<object, Type>();
        static bool _wired;

        internal static void PopulateEnumMap()
        {
            _enumToStruct.Clear();

            Map(RoundEvents.Waiting, typeof(RoundWaitingEvent));
            Map(RoundEvents.Start, typeof(RoundStartEvent));
            Map(RoundEvents.End, typeof(RoundEndEvent));
            Map(RoundEvents.Restart, typeof(RoundRestartEvent));
            Map(RoundEvents.Check, typeof(RoundCheckEvent));
            Map(RoundEvents.ForceStart, typeof(RoundForceStartEvent));

            Map(PlayerEvents.Spawn, typeof(SpawnEvent));
            Map(PlayerEvents.ChangeRole, typeof(ChangeRoleEvent));
            Map(PlayerEvents.Join, typeof(JoinEvent));
            Map(PlayerEvents.Dead, typeof(DeadEvent));
            Map(PlayerEvents.Dies, typeof(DiesEvent));
            Map(PlayerEvents.InteractDoor, typeof(InteractDoorEvent));
            Map(PlayerEvents.Damage, typeof(DamageEvent));
            Map(PlayerEvents.Attack, typeof(AttackEvent));
            Map(PlayerEvents.Leave, typeof(LeaveEvent));
            Map(PlayerEvents.PickupItem, typeof(PickupItemEvent));
            Map(PlayerEvents.PrePickupItem, typeof(PrePickupItemEvent));
            Map(PlayerEvents.Escape, typeof(EscapeEvent));
            Map(PlayerEvents.InteractGenerator, typeof(InteractGeneratorEvent));
            Map(PlayerEvents.UsedItem, typeof(UsedItemEvent));
            Map(PlayerEvents.UseItem, typeof(UseItemEvent));
            Map(PlayerEvents.Kick, typeof(KickEvent));
            Map(PlayerEvents.ChangeItem, typeof(ChangeItemEvent));
            Map(PlayerEvents.InteractLift, typeof(InteractLiftEvent));
            Map(PlayerEvents.DroppedItem, typeof(DroppedItemEvent));
            Map(PlayerEvents.DropItem, typeof(DropItemEvent));
            Map(PlayerEvents.DropAmmo, typeof(DropAmmoEvent));
            Map(PlayerEvents.Cuff, typeof(CuffEvent));
            Map(PlayerEvents.UnCuff, typeof(UnCuffEvent));
            Map(PlayerEvents.Ban, typeof(BanEvent));
            Map(PlayerEvents.Banned, typeof(BannedEvent));
            Map(PlayerEvents.UsingRadio, typeof(UsingRadioEvent));
            Map(PlayerEvents.JailbirdTrigger, typeof(JailbirdTriggerEvent));
            Map(PlayerEvents.InteractWorkStation, typeof(InteractWorkStationEvent));
            Map(PlayerEvents.InteractLocker, typeof(InteractLockerEvent));
            Map(PlayerEvents.CheckWhiteList, typeof(CheckWhiteListEvent));
            Map(PlayerEvents.CheckReserveSlot, typeof(CheckReserveSlotEvent));
            Map(PlayerEvents.ChangeSpectate, typeof(ChangeSpectateEvent));

            Map(ScpEvents.Scp914UpgradePlayer, typeof(Scp914UpgradePlayerEvent));
            Map(ScpEvents.Scp914UpgradePickup, typeof(Scp914UpgradePickupEvent));
            Map(ScpEvents.Scp173AddObserver, typeof(Scp173AddObserverEvent));
            Map(ScpEvents.Scp173RemovedObserver, typeof(Scp173RemovedObserverEvent));
            Map(ScpEvents.Scp173EnableSpeed, typeof(Scp173EnableSpeedEvent));
            Map(ScpEvents.Scp106Attack, typeof(Scp106AttackEvent));
            Map(ScpEvents.Scp096AddTarget, typeof(Scp096AddTargetEvent));
            Map(ScpEvents.Scp096SetState, typeof(Scp096SetStateEvent));
            Map(ScpEvents.Scp079Recontain, typeof(Scp079RecontainEvent));
            Map(ScpEvents.Scp079NewLvl, typeof(Scp079NewLvlEvent));
            Map(ScpEvents.Scp079LockDoor, typeof(Scp079LockDoorEvent));
            Map(ScpEvents.Scp079InteractDoor, typeof(Scp079InteractDoorEvent));
            Map(ScpEvents.Scp049RaisingStart, typeof(Scp049RaisingStartEvent));
            Map(ScpEvents.Scp049RaisingEnd, typeof(Scp049RaisingEndEvent));
            Map(ScpEvents.GeneratorStatus, typeof(GeneratorStatusEvent));
            Map(ScpEvents.ActivateGenerator, typeof(ActivateGeneratorEvent));

            Map(ServerEvents.RemoteAdminCommand, typeof(RemoteAdminCommandEvent));
            Map(ServerEvents.GameConsoleCommand, typeof(GameConsoleCommandEvent));
            Map(ServerEvents.RequestPlayerListCommand, typeof(RequestPlayerListCommandEvent));
            Map(ServerEvents.LocalReport, typeof(LocalReportEvent));
            Map(ServerEvents.CheaterReport, typeof(CheaterReportEvent));

            Map(MapEvents.OpenDoor, typeof(OpenDoorEvent));
            Map(MapEvents.DamageDoor, typeof(DamageDoorEvent));
            Map(MapEvents.LockDoor, typeof(LockDoorEvent));
            Map(MapEvents.CreatePickup, typeof(CreatePickupEvent));
            Map(MapEvents.WorkStationUpdate, typeof(WorkStationUpdateEvent));
            Map(MapEvents.TriggerTesla, typeof(TriggerTeslaEvent));
            Map(MapEvents.LczDecontamination, typeof(LczDecontaminationEvent));
            Map(MapEvents.CorpseSpawned, typeof(CorpseSpawnedEvent));

            Map(EffectEvents.Enabled, typeof(EffectEnabledEvent));
            Map(EffectEvents.Disabled, typeof(EffectDisabledEvent));

            Map(AlphaEvents.Detonate, typeof(AlphaDetonateEvent));
            Map(AlphaEvents.Stop, typeof(AlphaStopEvent));
            Map(AlphaEvents.Start, typeof(AlphaStartEvent));
        }

        internal static Type EnumToStruct(object enumValue)
            => enumValue != null && _enumToStruct.TryGetValue(enumValue, out var t) ? t : null;

        internal static void WireLabApi()
        {
            if (_wired) return;
            _wired = true;

            ServerHandlers.WaitingForPlayers += OnWaitingForPlayers;
            ServerHandlers.RoundStarted += OnRoundStarted;
            ServerHandlers.RoundEnded += OnRoundEnded;
            ServerHandlers.RoundRestarted += OnRoundRestarted;
            ServerHandlers.RoundEndingConditionsCheck += OnRoundCheck;
            ServerHandlers.RoundStarting += OnRoundStarting;
            ServerHandlers.CommandExecuting += OnCommandExecuting;
            ServerHandlers.PickupCreated += OnPickupCreated;
            ServerHandlers.LczDecontaminationStarting += OnLczDecontaminationStarting;
            ServerHandlers.DoorDamaging += OnDoorDamaging;
            ServerHandlers.DoorLockChanged += OnDoorLockChanged;

            PlayerHandlers.Joined += OnJoined;
            PlayerHandlers.Left += OnLeft;
            PlayerHandlers.Spawning += OnSpawning;
            PlayerHandlers.Spawned += OnSpawned;
            PlayerHandlers.ChangingRole += OnChangingRole;
            PlayerHandlers.ChangedRole += OnChangedRole;
            PlayerHandlers.Dying += OnDying;
            PlayerHandlers.Death += OnDeath;
            PlayerHandlers.Hurting += OnHurting;
            PlayerHandlers.Hurt += OnHurt;
            PlayerHandlers.InteractingDoor += OnInteractingDoor;
            PlayerHandlers.PickingUpItem += OnPickingUpItem;
            PlayerHandlers.PickedUpItem += OnPickedUpItem;
            PlayerHandlers.DroppingItem += OnDroppingItem;
            PlayerHandlers.DroppedItem += OnDroppedItem;
            PlayerHandlers.DroppingAmmo += OnDroppingAmmo;
            PlayerHandlers.ChangingItem += OnChangingItem;
            PlayerHandlers.UsingItem += OnUsingItem;
            PlayerHandlers.UsedItem += OnUsedItem;
            PlayerHandlers.UsingRadio += OnUsingRadio;
            PlayerHandlers.Escaping += OnEscaping;
            PlayerHandlers.Cuffing += OnCuffing;
            PlayerHandlers.Uncuffing += OnUncuffing;
            PlayerHandlers.InteractingElevator += OnInteractingElevator;
            PlayerHandlers.InteractingGenerator += OnInteractingGenerator;
            PlayerHandlers.ActivatingGenerator += OnActivatingGenerator;
            PlayerHandlers.ActivatedGenerator += OnActivatedGenerator;
            PlayerHandlers.DeactivatingGenerator += OnDeactivatingGenerator;
            PlayerHandlers.UnlockingGenerator += OnUnlockingGenerator;
            PlayerHandlers.InteractingLocker += OnInteractingLocker;
            PlayerHandlers.TriggeringTesla += OnTriggeringTesla;
            PlayerHandlers.SpawnedRagdoll += OnSpawnedRagdoll;
            PlayerHandlers.ChangedSpectator += OnChangedSpectator;
            PlayerHandlers.Banning += OnBanning;
            PlayerHandlers.Banned += OnBanned;
            PlayerHandlers.Kicking += OnKicking;
            PlayerHandlers.RequestingRaPlayerList += OnRequestingRaPlayerList;
            PlayerHandlers.ReportingCheater += OnReportingCheater;
            PlayerHandlers.ReportingPlayer += OnReportingPlayer;
            PlayerHandlers.UpdatingEffect += OnUpdatingEffect;
            PlayerHandlers.UpdatedEffect += OnUpdatedEffect;

            WarheadHandlers.Starting += OnWarheadStarting;
            WarheadHandlers.Started += OnWarheadStarted;
            WarheadHandlers.Stopping += OnWarheadStopping;
            WarheadHandlers.Stopped += OnWarheadStopped;
            WarheadHandlers.Detonating += OnWarheadDetonating;
            WarheadHandlers.Detonated += OnWarheadDetonated;

            Scp914Handlers.ProcessingPlayer += OnScp914ProcessingPlayer;
            Scp914Handlers.ProcessingPickup += OnScp914ProcessingPickup;
            Scp173Handlers.AddingObserver += OnScp173AddingObserver;
            Scp173Handlers.RemovingObserver += OnScp173RemovingObserver;
            Scp173Handlers.BreakneckSpeedChanging += OnScp173BreakneckSpeedChanging;
            Scp096Handlers.AddingTarget += OnScp096AddingTarget;
            Scp096Handlers.ChangingState += OnScp096ChangingState;
            Scp079Handlers.Recontaining += OnScp079Recontaining;
            Scp079Handlers.LevelingUp += OnScp079LevelingUp;
            Scp079Handlers.LockingDoor += OnScp079LockingDoor;
            Scp049Handlers.StartingResurrection += OnScp049StartingResurrection;
            Scp049Handlers.ResurrectedBody += OnScp049ResurrectedBody;
            Scp106Handlers.TeleportingPlayer += OnScp106TeleportingPlayer;
        }

        internal static void UnwireLabApi()
        {
            if (!_wired) return;
            _wired = false;

            ServerHandlers.WaitingForPlayers -= OnWaitingForPlayers;
            ServerHandlers.RoundStarted -= OnRoundStarted;
            ServerHandlers.RoundEnded -= OnRoundEnded;
            ServerHandlers.RoundRestarted -= OnRoundRestarted;
            ServerHandlers.RoundEndingConditionsCheck -= OnRoundCheck;
            ServerHandlers.RoundStarting -= OnRoundStarting;
            ServerHandlers.CommandExecuting -= OnCommandExecuting;
            ServerHandlers.PickupCreated -= OnPickupCreated;
            ServerHandlers.LczDecontaminationStarting -= OnLczDecontaminationStarting;
            ServerHandlers.DoorDamaging -= OnDoorDamaging;
            ServerHandlers.DoorLockChanged -= OnDoorLockChanged;

            PlayerHandlers.Joined -= OnJoined;
            PlayerHandlers.Left -= OnLeft;
            PlayerHandlers.Spawning -= OnSpawning;
            PlayerHandlers.Spawned -= OnSpawned;
            PlayerHandlers.ChangingRole -= OnChangingRole;
            PlayerHandlers.ChangedRole -= OnChangedRole;
            PlayerHandlers.Dying -= OnDying;
            PlayerHandlers.Death -= OnDeath;
            PlayerHandlers.Hurting -= OnHurting;
            PlayerHandlers.Hurt -= OnHurt;
            PlayerHandlers.InteractingDoor -= OnInteractingDoor;
            PlayerHandlers.PickingUpItem -= OnPickingUpItem;
            PlayerHandlers.PickedUpItem -= OnPickedUpItem;
            PlayerHandlers.DroppingItem -= OnDroppingItem;
            PlayerHandlers.DroppedItem -= OnDroppedItem;
            PlayerHandlers.DroppingAmmo -= OnDroppingAmmo;
            PlayerHandlers.ChangingItem -= OnChangingItem;
            PlayerHandlers.UsingItem -= OnUsingItem;
            PlayerHandlers.UsedItem -= OnUsedItem;
            PlayerHandlers.UsingRadio -= OnUsingRadio;
            PlayerHandlers.Escaping -= OnEscaping;
            PlayerHandlers.Cuffing -= OnCuffing;
            PlayerHandlers.Uncuffing -= OnUncuffing;
            PlayerHandlers.InteractingElevator -= OnInteractingElevator;
            PlayerHandlers.InteractingGenerator -= OnInteractingGenerator;
            PlayerHandlers.ActivatingGenerator -= OnActivatingGenerator;
            PlayerHandlers.ActivatedGenerator -= OnActivatedGenerator;
            PlayerHandlers.DeactivatingGenerator -= OnDeactivatingGenerator;
            PlayerHandlers.UnlockingGenerator -= OnUnlockingGenerator;
            PlayerHandlers.InteractingLocker -= OnInteractingLocker;
            PlayerHandlers.TriggeringTesla -= OnTriggeringTesla;
            PlayerHandlers.SpawnedRagdoll -= OnSpawnedRagdoll;
            PlayerHandlers.ChangedSpectator -= OnChangedSpectator;
            PlayerHandlers.Banning -= OnBanning;
            PlayerHandlers.Banned -= OnBanned;
            PlayerHandlers.Kicking -= OnKicking;
            PlayerHandlers.RequestingRaPlayerList -= OnRequestingRaPlayerList;
            PlayerHandlers.ReportingCheater -= OnReportingCheater;
            PlayerHandlers.ReportingPlayer -= OnReportingPlayer;
            PlayerHandlers.UpdatingEffect -= OnUpdatingEffect;
            PlayerHandlers.UpdatedEffect -= OnUpdatedEffect;

            WarheadHandlers.Starting -= OnWarheadStarting;
            WarheadHandlers.Started -= OnWarheadStarted;
            WarheadHandlers.Stopping -= OnWarheadStopping;
            WarheadHandlers.Stopped -= OnWarheadStopped;
            WarheadHandlers.Detonating -= OnWarheadDetonating;
            WarheadHandlers.Detonated -= OnWarheadDetonated;

            Scp914Handlers.ProcessingPlayer -= OnScp914ProcessingPlayer;
            Scp914Handlers.ProcessingPickup -= OnScp914ProcessingPickup;
            Scp173Handlers.AddingObserver -= OnScp173AddingObserver;
            Scp173Handlers.RemovingObserver -= OnScp173RemovingObserver;
            Scp173Handlers.BreakneckSpeedChanging -= OnScp173BreakneckSpeedChanging;
            Scp096Handlers.AddingTarget -= OnScp096AddingTarget;
            Scp096Handlers.ChangingState -= OnScp096ChangingState;
            Scp079Handlers.Recontaining -= OnScp079Recontaining;
            Scp079Handlers.LevelingUp -= OnScp079LevelingUp;
            Scp079Handlers.LockingDoor -= OnScp079LockingDoor;
            Scp049Handlers.StartingResurrection -= OnScp049StartingResurrection;
            Scp049Handlers.ResurrectedBody -= OnScp049ResurrectedBody;
            Scp106Handlers.TeleportingPlayer -= OnScp106TeleportingPlayer;
        }

        static void Map(object enumValue, Type structType) => _enumToStruct[enumValue] = structType;
        static Qurre.API.Controllers.Player Q(LabPlayer player) => Qurre.API.Controllers.Player.Get(player);
        static Qurre.API.Controllers.Player Q(CommandSender sender)
        {
            try { return Q(LabPlayer.Get(sender)); }
            catch { return Server.Host; }
        }
        static void PushAllowed(dynamic args, EventBase ev) { try { args.IsAllowed = ev.Allowed; } catch { } }
        static float ReadDamage(object handler) { try { return (float)((dynamic)handler).Damage; } catch { return 0f; } }
        static T Prop<T>(object source, string name, T fallback = default)
        {
            if (source == null) return fallback;
            try
            {
                object value = source.GetType().GetProperty(name)?.GetValue(source);
                if (value is T typed) return typed;
            }
            catch { }
            return fallback;
        }
        static void SetProp(object target, string name, object value)
        {
            if (target == null) return;
            try
            {
                var prop = target.GetType().GetProperty(name);
                if (prop?.CanWrite == true) prop.SetValue(target, value);
            }
            catch { }
        }
        static Qurre.API.Objects.EffectType EffectTypeFrom(object effect)
        {
            var name = effect?.GetType().Name;
            if (name != null && Enum.TryParse(name, out Qurre.API.Objects.EffectType parsed)) return parsed;
            return default;
        }

        static void OnWaitingForPlayers() { Qurre.API.ShimState.ClearRoundState(); Core.Dispatch(new RoundWaitingEvent()); }
        static void OnRoundStarted() { Qurre.API.ShimState.ClearRoundState(); Core.Dispatch(new RoundStartEvent()); }
        static void OnRoundEnded(SArgs.RoundEndedEventArgs args) => Core.Dispatch(new RoundEndEvent { Winner = args.LeadingTeam });
        static void OnRoundRestarted() => Core.Dispatch(new RoundRestartEvent());
        static void OnRoundStarting(SArgs.RoundStartingEventArgs args) { var ev = Core.Dispatch(new RoundForceStartEvent { Allowed = args.IsAllowed }); PushAllowed(args, ev); }
        static void OnRoundCheck(SArgs.RoundEndingConditionsCheckEventArgs args) { var ev = Core.Dispatch(new RoundCheckEvent { End = args.CanEnd }); args.CanEnd = ev.End; }

        static void OnCommandExecuting(SArgs.CommandExecutingEventArgs args)
        {
            string[] argv = args.Arguments.ToArray();
            if (args.CommandType == LabApi.Features.Enums.CommandType.RemoteAdmin)
            {
                var ev = Core.Dispatch(new RemoteAdminCommandEvent { Player = Q(args.Sender), Allowed = args.IsAllowed, Sender = args.Sender, Name = args.CommandName, Args = argv });
                PushCommandResult(args, ev, true);
                return;
            }

            if (args.CommandType == LabApi.Features.Enums.CommandType.Client || args.CommandType == LabApi.Features.Enums.CommandType.Console)
            {
                var ev = Core.Dispatch(new GameConsoleCommandEvent { Player = Q(args.Sender), Allowed = args.IsAllowed, Sender = args.Sender, Name = args.CommandName, Args = argv });
                PushCommandResult(args, ev, false);
            }
        }

        static void PushCommandResult(SArgs.CommandExecutingEventArgs args, EventBase ev, bool remoteAdmin)
        {
            args.IsAllowed = ev.Allowed;
            if (string.IsNullOrEmpty(ev.Reply)) return;

            try
            {
                if (remoteAdmin) args.Sender.RaReply(ev.Reply, true, false, ev.Color ?? string.Empty);
                else args.Sender.Respond(ev.Reply, true);
            }
            catch
            {
                try { args.Sender.Print(ev.Reply); } catch { }
            }
        }

        static void OnPickupCreated(SArgs.PickupCreatedEventArgs args) => Core.Dispatch(new CreatePickupEvent { Pickup = args.Pickup?.Base, Position = args.Pickup?.Position ?? UnityEngine.Vector3.zero });
        static void OnLczDecontaminationStarting(SArgs.LczDecontaminationStartingEventArgs args) { Decontamination.InProgress = true; var ev = Core.Dispatch(new LczDecontaminationEvent { Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnDoorDamaging(SArgs.DoorDamagingEventArgs args) { var ev = Core.Dispatch(new DamageDoorEvent { Door = Door.Get(args.Door), Damage = args.Damage, Allowed = args.IsAllowed }); args.Damage = ev.Damage; args.IsAllowed = ev.Allowed; }
        static void OnDoorLockChanged(SArgs.DoorLockChangedEventArgs args) => Core.Dispatch(new LockDoorEvent { Door = Door.Get(args.Door) });

        static void OnJoined(PArgs.PlayerJoinedEventArgs args) => Core.Dispatch(new JoinEvent { Player = Q(args.Player) });
        static void OnLeft(PArgs.PlayerLeftEventArgs args) => Core.Dispatch(new LeaveEvent { Player = Q(args.Player) });
        static void OnSpawning(PArgs.PlayerSpawningEventArgs args) { var ev = Core.Dispatch(new SpawnEvent { Player = Q(args.Player), Role = args.Role.RoleTypeId, Position = args.SpawnLocation, Allowed = args.IsAllowed }); args.SpawnLocation = ev.Position; args.IsAllowed = ev.Allowed; }
        static void OnSpawned(PArgs.PlayerSpawnedEventArgs args) => Core.Dispatch(new SpawnEvent { Player = Q(args.Player), Role = args.Role.RoleTypeId, Position = args.SpawnLocation });
        static void OnChangingRole(PArgs.PlayerChangingRoleEventArgs args) { var ev = Core.Dispatch(new ChangeRoleEvent { Player = Q(args.Player), Role = args.NewRole, OldRole = args.OldRole?.RoleTypeId ?? default, Allowed = args.IsAllowed }); args.NewRole = ev.Role; args.IsAllowed = ev.Allowed; }
        static void OnChangedRole(PArgs.PlayerChangedRoleEventArgs args) => Core.Dispatch(new ChangeRoleEvent { Player = Q(args.Player), Role = args.NewRole.RoleTypeId, OldRole = args.OldRole });
        static void OnDying(PArgs.PlayerDyingEventArgs args) { var ev = Core.Dispatch(new DiesEvent { Player = Q(args.Player), Attacker = Q(args.Attacker), DamageInfo = args.DamageHandler, Damage = ReadDamage(args.DamageHandler), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnDeath(PArgs.PlayerDeathEventArgs args) => Core.Dispatch(new DeadEvent { Player = Q(args.Player), Attacker = Q(args.Attacker), DamageInfo = args.DamageHandler, Damage = ReadDamage(args.DamageHandler), OldRole = args.OldRole, Position = args.OldPosition });
        static void OnHurting(PArgs.PlayerHurtingEventArgs args) { var ev = Core.Dispatch(new DamageEvent { Player = Q(args.Player), Attacker = Q(args.Attacker), DamageInfo = args.DamageHandler, Damage = ReadDamage(args.DamageHandler), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnHurt(PArgs.PlayerHurtEventArgs args) => Core.Dispatch(new AttackEvent { Player = Q(args.Player), Attacker = Q(args.Attacker), DamageInfo = args.DamageHandler, Damage = ReadDamage(args.DamageHandler) });
        static void OnInteractingDoor(PArgs.PlayerInteractingDoorEventArgs args)
        {
            bool allowed = args.IsAllowed && args.CanOpen;
            var door = Door.Get(args.Door);
            var player = Q(args.Player);

            var interact = Core.Dispatch(new InteractDoorEvent { Player = player, Door = door, Allowed = allowed });
            var open = Core.Dispatch(new OpenDoorEvent { Player = player, Door = door, Allowed = interact.Allowed });

            args.CanOpen = open.Allowed;
            args.IsAllowed = open.Allowed;
        }
        static void OnPickingUpItem(PArgs.PlayerPickingUpItemEventArgs args) { var ev = Core.Dispatch(new PrePickupItemEvent { Player = Q(args.Player), Pickup = args.Pickup?.Base, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnPickedUpItem(PArgs.PlayerPickedUpItemEventArgs args) => Core.Dispatch(new PickupItemEvent { Player = Q(args.Player), Item = args.Item?.Base });
        static void OnDroppingItem(PArgs.PlayerDroppingItemEventArgs args) { var ev = Core.Dispatch(new DropItemEvent { Player = Q(args.Player), Item = args.Item?.Base, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnDroppedItem(PArgs.PlayerDroppedItemEventArgs args) => Core.Dispatch(new DroppedItemEvent { Player = Q(args.Player), Pickup = args.Pickup?.Base });
        static void OnDroppingAmmo(PArgs.PlayerDroppingAmmoEventArgs args) { var ev = Core.Dispatch(new DropAmmoEvent { Player = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnChangingItem(PArgs.PlayerChangingItemEventArgs args) { var ev = Core.Dispatch(new ChangeItemEvent { Player = Q(args.Player), OldItem = args.OldItem?.Base, NewItem = args.NewItem?.Base, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnUsingItem(PArgs.PlayerUsingItemEventArgs args) { var ev = Core.Dispatch(new UseItemEvent { Player = Q(args.Player), Item = args.UsableItem?.Base, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnUsedItem(PArgs.PlayerUsedItemEventArgs args) => Core.Dispatch(new UsedItemEvent { Player = Q(args.Player), Item = args.UsableItem?.Base });
        static void OnUsingRadio(PArgs.PlayerUsingRadioEventArgs args) { var ev = Core.Dispatch(new UsingRadioEvent { Player = Q(args.Player), Item = args.RadioItem?.Base, Consumption = args.Drain, Allowed = args.IsAllowed }); args.Drain = ev.Consumption; args.IsAllowed = ev.Allowed; }
        static void OnEscaping(PArgs.PlayerEscapingEventArgs args) { var ev = Core.Dispatch(new EscapeEvent { Player = Q(args.Player), OldRole = args.OldRole, Role = args.NewRole, Allowed = args.IsAllowed }); args.NewRole = ev.Role; args.IsAllowed = ev.Allowed; }
        static void OnCuffing(PArgs.PlayerCuffingEventArgs args) { var ev = Core.Dispatch(new CuffEvent { Player = Q(args.Target), Cuffer = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnUncuffing(PArgs.PlayerUncuffingEventArgs args) { var ev = Core.Dispatch(new UnCuffEvent { Player = Q(args.Target), Cuffer = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnInteractingElevator(PArgs.PlayerInteractingElevatorEventArgs args) { var ev = Core.Dispatch(new InteractLiftEvent { Player = Q(args.Player), Lift = Lift.Get(args.Elevator), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnInteractingGenerator(PArgs.PlayerInteractingGeneratorEventArgs args) => DispatchGenerator(args, null, false);
        static void OnActivatingGenerator(PArgs.PlayerActivatingGeneratorEventArgs args) => DispatchGenerator(args, Qurre.API.Objects.GeneratorStatus.Activate, false);
        static void OnActivatedGenerator(PArgs.PlayerActivatedGeneratorEventArgs args) => DispatchGenerator(args, Qurre.API.Objects.GeneratorStatus.Activate, true);
        static void OnDeactivatingGenerator(PArgs.PlayerDeactivatingGeneratorEventArgs args) => DispatchGenerator(args, Qurre.API.Objects.GeneratorStatus.Deactivate, false);
        static void OnUnlockingGenerator(PArgs.PlayerUnlockingGeneratorEventArgs args) => DispatchGenerator(args, Qurre.API.Objects.GeneratorStatus.Unlock, false);
        static void OnInteractingLocker(PArgs.PlayerInteractingLockerEventArgs args)
        {
            var ev = Core.Dispatch(new InteractLockerEvent
            {
                Player = Q(Prop<LabPlayer>(args, "Player")),
                Locker = Prop<object>(args, "Locker"),
                Allowed = Prop(args, "IsAllowed", true)
            });
            SetProp(args, "IsAllowed", ev.Allowed);
        }
        static void OnTriggeringTesla(PArgs.PlayerTriggeringTeslaEventArgs args) { var ev = Core.Dispatch(new TriggerTeslaEvent { Player = Q(args.Player), Tesla = Tesla.Get(args.Tesla), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnSpawnedRagdoll(PArgs.PlayerSpawnedRagdollEventArgs args)
        {
            var owner = Q(args.Player);
            Qurre.API.ShimState.TrackCorpse(args.Ragdoll, owner);
            var corpse = new Corpse(args.Ragdoll, owner);
            Core.Dispatch(new CorpseSpawnedEvent { Player = owner, Corpse = corpse, DamageInfo = args.DamageHandler });
        }
        static void OnChangedSpectator(PArgs.PlayerChangedSpectatorEventArgs args) => Core.Dispatch(new ChangeSpectateEvent { Player = Q(args.Player), Target = Q(args.NewTarget) });
        static void OnBanning(PArgs.PlayerBanningEventArgs args) { var ev = Core.Dispatch(new BanEvent { Player = Q(args.Player), Issuer = Q(args.Issuer), Reason = args.Reason, Duration = args.Duration, Allowed = args.IsAllowed }); args.Reason = ev.Reason; args.Duration = ev.Duration; args.IsAllowed = ev.Allowed; }
        static void OnBanned(PArgs.PlayerBannedEventArgs args) => Core.Dispatch(new BannedEvent { Player = Q(args.Player), Issuer = Q(args.Issuer), Reason = args.Reason, UserId = args.PlayerId });
        static void OnKicking(PArgs.PlayerKickingEventArgs args) { var ev = Core.Dispatch(new KickEvent { Player = Q(args.Player), Issuer = Q(args.Issuer), Reason = args.Reason, Allowed = args.IsAllowed }); args.Reason = ev.Reason; args.IsAllowed = ev.Allowed; }
        static void OnRequestingRaPlayerList(PArgs.PlayerRequestingRaPlayerListEventArgs args) { var ev = Core.Dispatch(new RequestPlayerListCommandEvent { Player = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnReportingCheater(PArgs.PlayerReportingCheaterEventArgs args) { var ev = Core.Dispatch(new CheaterReportEvent { Player = Q(args.Player), Target = Q(args.Target), Reason = args.Reason, Allowed = args.IsAllowed }); args.Reason = ev.Reason; args.IsAllowed = ev.Allowed; }
        static void OnReportingPlayer(PArgs.PlayerReportingPlayerEventArgs args) { var ev = Core.Dispatch(new LocalReportEvent { Player = Q(args.Player), Target = Q(args.Target), Reason = args.Reason, Allowed = args.IsAllowed }); args.Reason = ev.Reason; args.IsAllowed = ev.Allowed; }
        static void OnUpdatingEffect(PArgs.PlayerEffectUpdatingEventArgs args)
        {
            var type = EffectTypeFrom(args.Effect);
            if (args.Intensity == 0)
            {
                var disabled = Core.Dispatch(new EffectDisabledEvent { Player = Q(args.Player), Type = type, Allowed = args.IsAllowed });
                args.IsAllowed = disabled.Allowed;
                return;
            }

            var enabled = Core.Dispatch(new EffectEnabledEvent { Player = Q(args.Player), Type = type, Intensity = args.Intensity, Duration = args.Duration, Allowed = args.IsAllowed });
            args.IsAllowed = enabled.Allowed;
            args.Intensity = enabled.Intensity;
            args.Duration = enabled.Duration;
        }
        static void OnUpdatedEffect(PArgs.PlayerEffectUpdatedEventArgs args)
        {
            if (args.Intensity == 0)
                Core.Dispatch(new EffectDisabledEvent { Player = Q(args.Player), Type = EffectTypeFrom(args.Effect) });
        }

        static void DispatchGenerator(object args, Qurre.API.Objects.GeneratorStatus? status, bool activated)
        {
            var ev = Core.Dispatch(new InteractGeneratorEvent
            {
                Player = Q(Prop<LabPlayer>(args, "Player")),
                Generator = Prop<object>(args, "Generator"),
                Status = status,
                Allowed = Prop(args, "IsAllowed", true)
            });
            SetProp(args, "IsAllowed", ev.Allowed);

            if (status.HasValue)
            {
                Core.Dispatch(new GeneratorStatusEvent
                {
                    Player = ev.Player,
                    Generator = ev.Generator,
                    Status = status
                });
            }

            if (activated)
            {
                Core.Dispatch(new ActivateGeneratorEvent
                {
                    Player = ev.Player,
                    Generator = ev.Generator,
                    Status = status
                });
            }
        }

        static void OnWarheadStarting(WArgs.WarheadStartingEventArgs args) { Alpha.Active = true; var ev = Core.Dispatch(new AlphaStartEvent { Player = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnWarheadStarted(WArgs.WarheadStartedEventArgs args) { Alpha.Active = true; Core.Dispatch(new AlphaStartEvent { Player = Q(args.Player) }); }
        static void OnWarheadStopping(WArgs.WarheadStoppingEventArgs args) { var ev = Core.Dispatch(new AlphaStopEvent { Player = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; if (ev.Allowed) Alpha.Active = false; }
        static void OnWarheadStopped(WArgs.WarheadStoppedEventArgs args) { Alpha.Active = false; Core.Dispatch(new AlphaStopEvent { Player = Q(args.Player) }); }
        static void OnWarheadDetonating(WArgs.WarheadDetonatingEventArgs args) { var ev = Core.Dispatch(new AlphaDetonateEvent { Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnWarheadDetonated(WArgs.WarheadDetonatedEventArgs args) { Alpha.Detonated = true; Alpha.Active = false; Core.Dispatch(new AlphaDetonateEvent()); }

        static void OnScp914ProcessingPlayer(Scp914Args.Scp914ProcessingPlayerEventArgs args) { var ev = Core.Dispatch(new Scp914UpgradePlayerEvent { Player = Q(args.Player), Position = args.NewPosition, Setting = args.KnobSetting, Allowed = args.IsAllowed }); args.NewPosition = ev.Position; args.IsAllowed = ev.Allowed; }
        static void OnScp914ProcessingPickup(Scp914Args.Scp914ProcessingPickupEventArgs args) { var ev = Core.Dispatch(new Scp914UpgradePickupEvent { Pickup = args.Pickup?.Base, Position = args.NewPosition, Setting = args.KnobSetting, Allowed = args.IsAllowed }); args.NewPosition = ev.Position; args.IsAllowed = ev.Allowed; }
        static void OnScp173AddingObserver(Scp173Args.Scp173AddingObserverEventArgs args) { var ev = Core.Dispatch(new Scp173AddObserverEvent { Player = Q(args.Target), Scp = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp173RemovingObserver(Scp173Args.Scp173RemovingObserverEventArgs args) { var ev = Core.Dispatch(new Scp173RemovedObserverEvent { Player = Q(args.Target), Scp = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp173BreakneckSpeedChanging(Scp173Args.Scp173BreakneckSpeedChangingEventArgs args) { var ev = Core.Dispatch(new Scp173EnableSpeedEvent { Player = Q(args.Player), Active = args.Active, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp096AddingTarget(Scp096Args.Scp096AddingTargetEventArgs args) { var ev = Core.Dispatch(new Scp096AddTargetEvent { Player = Q(args.Target), Scp = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp096ChangingState(Scp096Args.Scp096ChangingStateEventArgs args) { var ev = Core.Dispatch(new Scp096SetStateEvent { Player = Q(args.Player), State = args.State, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp079Recontaining(Scp079Args.Scp079RecontainingEventArgs args) { var ev = Core.Dispatch(new Scp079RecontainEvent { Player = Q(args.Player), Target = Q(args.Activator), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp079LevelingUp(Scp079Args.Scp079LevelingUpEventArgs args) { var ev = Core.Dispatch(new Scp079NewLvlEvent { Player = Q(args.Player), Level = args.Tier, Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp079LockingDoor(Scp079Args.Scp079LockingDoorEventArgs args) { var ev = Core.Dispatch(new Scp079LockDoorEvent { Player = Q(args.Player), Door = Door.Get(args.Door), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp049StartingResurrection(Scp049Args.Scp049StartingResurrectionEventArgs args) { var ev = Core.Dispatch(new Scp049RaisingStartEvent { Player = Q(args.Player), Target = Q(args.Target), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
        static void OnScp049ResurrectedBody(Scp049Args.Scp049ResurrectedBodyEventArgs args) => Core.Dispatch(new Scp049RaisingEndEvent { Player = Q(args.Player), Target = Q(args.Target) });
        static void OnScp106TeleportingPlayer(Scp106Args.Scp106TeleportingPlayerEvent args) { var ev = Core.Dispatch(new Scp106AttackEvent { Player = Q(args.Target), Scp = Q(args.Player), Allowed = args.IsAllowed }); args.IsAllowed = ev.Allowed; }
    }
}
