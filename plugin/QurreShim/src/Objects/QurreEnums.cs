// Qurre-энумы (Qurre.API.Objects) — извлечены из использования в плагине.
namespace Qurre.API.Objects
{
    public enum EffectType
    {
        AmnesiaItems, AmnesiaVision, Asphyxiated, BecomingFlamingo, Bleeding, Blindness,
        Burned, Corroding, Hemorrhage, Invigorated, Invisible, MovementBoost,
        PocketCorroding, SeveredHands, Sinkhole
    }

    public enum DamageTypes
    {
        Crushed, Custom, Disruptor, E11SR, Explosion, Gun, Jailbird, Pocket,
        Recontainment, Scp018, ScpDamage, Snowball, Universal, Warhead
    }

    public enum LiteDamageTypes
    {
        Custom, Disruptor, Explosion, Gun, Jailbird, Recontainment,
        Scp018, ScpDamage, Snowball, Universal, Warhead
    }

    public enum RoomType
    {
        Unknown, Surface, Pocket,
        Lcz173, LczArmory, LczClassDSpawn, LczCrossing,
        Hcz049, Hcz079, Hcz106, Hcz939, HczArmory, HczChkpA, HczChkpB,
        HczCornerDeep, HczCrossing, HczCurve, HczHid, HczJunk, HczNuke,
        HczStraight, HczTest, HczThreeWay,
        EzCrossing, EzIntercom, EzShelter, EzUpstairsPcs, EzVent
    }

    public enum DoorType
    {
        Unknown, ElevatorGateA, ElevatorGateB, ElevatorHczChkpA, ElevatorHczChkpB,
        ElevatorLczChkpA, ElevatorLczChkpB, EzCheckpointA, EzCheckpointB,
        EzCheckpointArmoryA, EzCheckpointArmoryB, Hcz049Gate, Hcz079First,
        Hcz079Second, Hcz096, Hcz106First, Hcz106Second, Hcz173Gate,
        Lcz173Connector, LczArmory, LczCafe, LczGr18Gate, LczPrison, SurfaceNuke
    }

    public enum GeneratorStatus { Activate, Deactivate, Unlock }
    public enum WorkstationStatus { Offline, PoweringUp, PoweringDown }
}
