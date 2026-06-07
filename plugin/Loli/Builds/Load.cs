using Loli.Builds.Models;
using Loli.Builds.Models.Rooms;
using MEC;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Builds
{
    internal static class Load
    {
        internal const string StaticDoorName = "StaticDoorNameInvisible";
        internal static readonly Color32 WhiteColor = new(175, 175, 175, 255);

        [EventMethod(RoundEvents.Waiting, int.MaxValue)]
        internal static void Waiting()
        {
            try { Server.Doors.Clear(); } catch { }
            try { Lift.List.Clear(); } catch { }
            Timing.KillCoroutines("DoorOpenCloseInServer");
            Timing.KillCoroutines("ServerLightBlink");
            Timing.KillCoroutines("SpawnServersInServersRoom");
            Timing.KillCoroutines("CustomLiftRunning");
            Timing.KillCoroutines("TexturesChildAndNotPrefereCoroutine");
            Timing.KillCoroutines("NeonLightModel");
            Timing.CallDelayed(0.5f, () => Initialize());
        }

        [EventMethod(RoundEvents.End)]
        internal static void End()
        {
            Timing.KillCoroutines("ServerLightBlink");
            Timing.KillCoroutines("SpawnServersInServersRoom");
        }
        internal static void Initialize()
        {
            new Radar(new(130.26f, 997.84f, 21.05f), Vector3.zero);
            Timing.CallDelayed(0.1f, () =>
            {
                try
                {
                    RadarLoc.Load();
                    Bashni.Load();
                }
                catch (System.Exception e)
                {
                    Qurre.API.Log.Error(e);
                }
            });
        }

        [EventMethod(PlayerEvents.PrePickupItem)]
        static void PrePickup(PrePickupItemEvent ev)
        {
            {
                if (Server.Doors.TryGetValue(ev.Pickup, out var _data))
                {
                    _data.InteractDoor();
                    ev.Allowed = false;
                }
            }
        }

        [EventMethod(MapEvents.DamageDoor)]
        internal static void DoorEvents(DamageDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not StaticDoorName) return;
            ev.Allowed = false;
        }

        [EventMethod(MapEvents.LockDoor)]
        internal static void DoorEvents(LockDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not StaticDoorName) return;
            ev.Allowed = false;
        }

        [EventMethod(MapEvents.OpenDoor)]
        internal static void DoorEvents(OpenDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not StaticDoorName) return;
            ev.Allowed = false;
        }
    }
}