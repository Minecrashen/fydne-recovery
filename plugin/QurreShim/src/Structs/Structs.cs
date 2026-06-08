// Event-структуры Qurre (Qurre.Events.Structs).
// Qurre-события — КЛАССЫ (ref-типы): отмена через ev.Allowed=false работает при передаче по значению.
// EventBase несёт общие поля (по объединению ev.* в плагине). Специфика — в конкретных классах.
// Wrapper-типизированные поля (Station/Pickup/Item/Generator/Corpse) добавляются в проходе контроллеров.
using UnityEngine;
using Qurre.API;
using Qurre.API.Objects;
using Qurre.API.Controllers;
using Role = PlayerRoles.RoleTypeId;

namespace Qurre.Events.Structs
{
    public class EventBase : IBaseEvent
    {
        public Player Player;
        public Player Target;
        public Player Attacker;
        public Player Issuer;
        public Player Cuffer;
        public Player Scp;
        public Player New;

        public bool Allowed = true;
        public bool Success;
        public bool FriendlyFire;
        public bool Active;
        public bool End;

        public string Reply = "";
        public string Color = "white";
        public string Prefix = "";
        public string Details = "";
        public string Reason = "";
        public string Name = "";
        public string Message = "";
        public string UserId = "";
        public string[] Args;

        public global::CommandSender Sender;
        public Role Role;
        public Role OldRole;

        public float Damage;
        public Vector3 Position;
        public int Level;
        public int EnragedCount;
        public int TotalCount;

        public Door Door;
        public global::RoundSummary.LeadingTeam Winner;
    }

    // --- Раунд / сервер ---
    public class RoundCheckEvent : EventBase { }
    public class AlphaStartEvent : EventBase { }
    public class AlphaStopEvent : EventBase { }

    // --- Игрок: жизненный цикл ---
    public class JoinEvent : EventBase { }
    public class LeaveEvent : EventBase { }
    public class SpawnEvent : EventBase { }
    public class ChangeRoleEvent : EventBase { }
    public class ChangeSpectateEvent : EventBase { }
    public class DeadEvent : EventBase { }
    public class DiesEvent : EventBase { }
    public class EscapeEvent : EventBase { }
    public class KickEvent : EventBase { }
    public class BanEvent : EventBase { public global::BanHandler.BanType Type; public long Duration; public long Expires; }
    public class BannedEvent : EventBase { public global::BanHandler.BanType Type; }
    public class CheckWhiteListEvent : EventBase { }
    public class CheckReserveSlotEvent : EventBase { }

    // --- Бой ---
    public class DamageEvent : EventBase { public LiteDamageTypes LiteType; public DamageTypes DamageType; }
    public class AttackEvent : EventBase { public LiteDamageTypes LiteType; public DamageTypes DamageType; }

    // --- Предметы ---
    public class PickupItemEvent : EventBase { }
    public class PrePickupItemEvent : EventBase { }
    public class DropItemEvent : EventBase { }
    public class DroppedItemEvent : EventBase { }
    public class DropAmmoEvent : EventBase { }
    public class ChangeItemEvent : EventBase { }
    public class UseItemEvent : EventBase { }
    public class UsedItemEvent : EventBase { }
    public class UsingRadioEvent : EventBase { }
    public class JailbirdTriggerEvent : EventBase { }

    // --- Эффекты ---
    public class EffectEnabledEvent : EventBase { public EffectType Type; public byte Intensity; public float Duration; }
    public class EffectDisabledEvent : EventBase { public EffectType Type; }

    // --- Двери / лифты / станции / генераторы / тесла ---
    public class InteractDoorEvent : EventBase { }
    public class OpenDoorEvent : EventBase { }
    public class LockDoorEvent : EventBase { }
    public class DamageDoorEvent : EventBase { }
    public class InteractLiftEvent : EventBase { }
    public class InteractLockerEvent : EventBase { }
    public class InteractWorkStationEvent : EventBase { }
    public class WorkStationUpdateEvent : EventBase { }
    public class InteractGeneratorEvent : EventBase { }
    public class GeneratorStatusEvent : EventBase { }
    public class ActivateGeneratorEvent : EventBase { }
    public class TriggerTeslaEvent : EventBase { }
    public class CreatePickupEvent : EventBase { }
    public class CorpseSpawnedEvent : EventBase { }

    // --- Наручники ---
    public class CuffEvent : EventBase { }
    public class UnCuffEvent : EventBase { }

    // --- Команды / репорты ---
    public class RemoteAdminCommandEvent : EventBase { }
    public class GameConsoleCommandEvent : EventBase { }
    public class RequestPlayerListCommandEvent : EventBase { }
    public class LocalReportEvent : EventBase { }
    public class CheaterReportEvent : EventBase { }
    public class MessageEvent : EventBase { }

    // --- SCP ---
    public class Scp914UpgradePlayerEvent : EventBase { }
    public class Scp914UpgradePickupEvent : EventBase { }
    public class Scp173AddObserverEvent : EventBase { }
    public class Scp173RemovedObserverEvent : EventBase { }
    public class Scp173EnableSpeedEvent : EventBase { }
    public class Scp106AttackEvent : EventBase { }
    public class Scp096AddTargetEvent : EventBase { }
    public class Scp096SetStateEvent : EventBase { }
    public class Scp079NewLvlEvent : EventBase { }
    public class Scp079LockDoorEvent : EventBase { }
    public class Scp079InteractDoorEvent : EventBase { }
    public class Scp049RaisingStartEvent : EventBase { }
    public class Scp049RaisingEndEvent : EventBase { }
}
