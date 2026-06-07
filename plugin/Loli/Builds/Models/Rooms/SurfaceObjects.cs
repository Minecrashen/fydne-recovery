using Interactables.Interobjects.DoorUtils;
using Mirror;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.API.World;
using Qurre.Events;
using UnityEngine;

namespace Loli.Builds.Models.Rooms
{
    static class SurfaceObjects
    {
        [EventMethod(RoundEvents.Waiting)]
        static void Load()
        {
            NukeDoor();
            GateADoors();
            EscapeDoors();
        }

        static void EscapeDoors()
        {
            new Door(new(128.18f, 287.81f, 25.6277f), DoorPrefabs.DoorHCZ) { Scale = new(1, 1, 1.4f) };
            new Door(new(126.876f, 287.81f, 21.155f), DoorPrefabs.DoorHCZ, Quaternion.Euler(new(0, 90))) { Scale = new(1, 1, 1.4f) };
        }

        static void GateADoors()
        {
            new Door(new(10.473f, 296.493f, -31.66f), DoorPrefabs.DoorHCZ) { Scale = new(1, 1, 1.3f) };
            new Door(new(10.473f, 296.493f, -16.936f), DoorPrefabs.DoorHCZ) { Scale = new(1, 1, 1.6f) };
        }

        static void NukeDoor()
        {
            Door door = DoorType.SurfaceNuke.GetDoor();
            Map.Doors.Remove(door);

            Door newdoor = new(door.Position, DoorPrefabs.DoorEZ, door.Rotation, door.Permissions);

            NetworkServer.UnSpawn(newdoor.GameObject);

            newdoor.Name = door.Name;

            if (newdoor.DoorVariant.TryGetComponent<DoorNametagExtension>(out var nametag))
                nametag.UpdateName(door.Name);

            NetworkServer.Destroy(door.GameObject);
            NetworkServer.Spawn(newdoor.GameObject);
        }
    }
}