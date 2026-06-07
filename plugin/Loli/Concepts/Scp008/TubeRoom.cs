using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using SchematicUnity.API.Objects;
using System.Collections.Generic;
using System.IO;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts.Scp008
{
    class TubeRoom
    {
        protected readonly TubeRoomType _type;
        protected readonly Door _door;
        protected readonly Scheme _scheme;
        protected bool _activated;

        static internal readonly Dictionary<Door, TubeRoom> DoorToRoom = new();

        internal TubeRoom(TubeRoomType type)
        {
            _type = type;

            string _file = string.Empty;
            RoomType _roomType = RoomType.Unknown;

            Vector3 _doorPos = Vector3.zero;
            Vector3 _doorRot = Vector3.zero;
            Vector3 _doorScl = Vector3.one;
            Vector3 _offset = Vector3.zero;

            switch (type)
            {
                case TubeRoomType.Lcz173:
                    {
                        _file = "InLcz173";
                        _roomType = RoomType.Lcz173;

                        _doorPos = new(16.036f, 12.622f, 7.555f);
                        _doorRot = Vector3.right * 90;
                        _doorScl = new(0.097f, 0.073f, 0.057f);

                        try { RoomsData.Lcz173?.Destroy(); } catch { }
                        RoomsData.Lcz173 = this;

                        break;
                    }
                case TubeRoomType.Hcz049:
                    {
                        _file = "In049";
                        _roomType = RoomType.Hcz049;
                        _offset = new(0, -4);


                        _doorPos = new(30.103f, 98.095f, 9.903f);
                        _doorRot = new(0, -90, -90);
                        _doorScl = new(0.097f, 0.073f, 0.057f);

                        try { RoomsData.Hcz049?.Destroy(); } catch { }
                        RoomsData.Hcz049 = this;

                        break;
                    }
                case TubeRoomType.Hcz939:
                    {
                        _file = "In939";
                        _roomType = RoomType.Hcz939;

                        _doorPos = new(-5.911987f, 0.968f, -5.667f);
                        _doorRot = new(0, -90, 90);
                        _doorScl = new(0.097f, 0.073f, 0.057f);

                        try { RoomsData.Hcz939?.Destroy(); } catch { }
                        RoomsData.Hcz939 = this;

                        break;
                    }
                case TubeRoomType.EzVent:
                    {
                        _file = "InEZ";
                        _roomType = RoomType.EzShelter;

                        _doorPos = new(-2.431f, 1.5451f, 5.34f);
                        _doorRot = new(0, -90, -90);
                        _doorScl = new(0.097f, 0.073f, 0.057f);

                        try { RoomsData.EzVent?.Destroy(); } catch { }
                        RoomsData.EzVent = this;

                        break;
                    }
                default:
                    {
                        Log.Error("Type is undefined");
                        return;
                    }
            }

            var room = Map.Rooms.Find(x => x.Type == _roomType);
            _scheme = SchematicUnity.API.SchematicManager.LoadSchematic(
                Path.Combine(Paths.Plugins, "Schemes", "Scp008", _file + ".json"),
                room.Position + _offset,
                room.Rotation);

            GameObject go = new();
            go.transform.parent = room.Transform;
            go.transform.localPosition = _doorPos + _offset;
            go.transform.localRotation = Quaternion.Euler(_doorRot);

            _door = new Door(go.transform.position, DoorPrefabs.DoorHCZ, go.transform.rotation) { Scale = _doorScl };
            Object.Destroy(go);

            DoorToRoom.Add(_door, this);
        }

        internal bool Activated => _activated;
        internal TubeRoomType Type => _type;

        internal void Destroy()
        {
            DoorToRoom.Remove(_door);
            _door.Destroy();
            _scheme.Unload();
        }

        [EventMethod(MapEvents.OpenDoor, int.MinValue)]
        [EventMethod(MapEvents.LockDoor, int.MinValue)]
        [EventMethod(MapEvents.DamageDoor, int.MinValue)]
        static internal void LockInteract(IBaseEvent @base)
        {
            switch (@base)
            {
                case OpenDoorEvent ev:
                    {
                        if (DoorToRoom.ContainsKey(ev.Door))
                            ev.Allowed = false;
                        break;
                    }
                case LockDoorEvent ev:
                    {
                        if (DoorToRoom.ContainsKey(ev.Door))
                            ev.Allowed = false;
                        break;
                    }
                case DamageDoorEvent ev:
                    {
                        if (DoorToRoom.ContainsKey(ev.Door))
                            ev.Allowed = false;
                        break;
                    }
            }
        }

        [EventMethod(PlayerEvents.InteractDoor, int.MinValue)]
        static internal void Interact(InteractDoorEvent ev)
        {
            if (!DoorToRoom.ContainsKey(ev.Door))
                return;

            ev.Allowed = false;

            if (ev.Player.RoleInformation.Team == PlayerRoles.Team.SCPs)
                return;

            bool isHand = ev.Player.Tag.Contains(SerpentsHand.Tag);

            if (!DoorToRoom.TryGetValue(ev.Door, out var room))
                return;

            if (room.Type == TubeRoomType.Lcz173 && Decontamination.InProgress)
                return;

            if (RoomsData.Control.StatusMonitors.TryGetValue(room.Type, out var prim))
                prim.Color = isHand ? Color.red : Color.green;

            room._activated = isHand;
            ev.Door.Open = isHand;
            HintsUi.UpdateProgress();
        }

        [EventMethod(MapEvents.LczDecontamination)]
        static internal void Decon()
        {
            if (RoomsData.Control.StatusMonitors.TryGetValue(TubeRoomType.Lcz173, out var prim))
                prim.Color = Color.grey;
        }
    }
}