// Qurre→LabAPI shim: enum'ы событий.
// Значения извлечены из фактического использования в плагине Loli ([EventMethod(...)]).
// Диспетчер (EventDispatcher) маппит эти значения на события LabAPI.
namespace Qurre.Events
{
    public enum RoundEvents
    {
        Waiting, Start, End, Restart, Check, ForceStart
    }

    public enum PlayerEvents
    {
        Spawn, ChangeRole, Join, Dead, Dies, InteractDoor, Damage, Attack, Leave,
        PickupItem, PrePickupItem, Escape, InteractGenerator, UsedItem, UseItem, Kick,
        ChangeItem, InteractLift, DroppedItem, DropItem, DropAmmo, Cuff, UnCuff, Ban, Banned,
        UsingRadio, JailbirdTrigger, InteractWorkStation, InteractLocker,
        CheckWhiteList, CheckReserveSlot, ChangeSpectate
    }

    public enum ScpEvents
    {
        Scp914UpgradePlayer, Scp914UpgradePickup,
        Scp173AddObserver, Scp173RemovedObserver, Scp173EnableSpeed,
        Scp106Attack,
        Scp096AddTarget, Scp096SetState,
        Scp079Recontain, Scp079NewLvl, Scp079LockDoor, Scp079InteractDoor,
        Scp049RaisingStart, Scp049RaisingEnd,
        GeneratorStatus, ActivateGenerator
    }

    public enum ServerEvents
    {
        RemoteAdminCommand, GameConsoleCommand, RequestPlayerListCommand,
        LocalReport, CheaterReport
    }

    public enum MapEvents
    {
        OpenDoor, DamageDoor, LockDoor, CreatePickup, WorkStationUpdate,
        TriggerTesla, LczDecontamination, CorpseSpawned
    }

    public enum EffectEvents
    {
        Enabled, Disabled
    }

    public enum AlphaEvents
    {
        Detonate, Stop, Start
    }
}
