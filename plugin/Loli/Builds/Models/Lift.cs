using MEC;
using Mirror;
using PlayerRoles.FirstPersonControl;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using Qurre.API.World;
using UnityEngine;
using Door = Qurre.API.Controllers.Door;
using Map = Qurre.API.World.Map;
using Player = Qurre.API.Controllers.Player;
using Round = Qurre.API.World.Round;

namespace Loli.Builds.Models
{
    internal class Lift
    {
        static internal bool Locked { get; set; } = false;

        private static readonly Color _red = new(3, 0, 0);
        private static readonly Color _cyan = new(0, 4, 4);
        private static readonly Color _yellow = new(4, 4, 0);
        private static readonly Color _lime = new(0, 4, 0);
        private const string LiftDoorName = "CustomLift";
        internal static List<Lift> List = new();
        internal int Speed = 5;
        private bool Using = false;
        private bool InFirst = true;
        private readonly bool StopAlpha = false;
        internal readonly LiftObjects Objects;
        private readonly List<DoSpawnDoor> DSD = new();
        internal Lift(LiftCreator first, LiftCreator second, Color color, bool stopAlpha = false)
        {
            StopAlpha = stopAlpha;
            List<LiftDoor> _doors = new();
            var _first = Create(first, true);
            var _second = Create(second, false);
            Objects = new(first, second, _first.Positions, _second.Positions, _doors, _first.Sensor, _second.Sensor, _first.CustomDoor, _second.CustomDoor);
            List.Add(this);

            LocalCreator Create(LiftCreator cre, bool first)
            {
                Model Model = new("Lift", cre.Position, cre.Rotation);
                NetworkServer.UnSpawn(Model.GameObject);
                Model.GameObject.transform.parent = cre.Parent;
                Model.GameObject.transform.localPosition = cre.Position;
                Model.GameObject.transform.localRotation = Quaternion.Euler(cre.Rotation);
                NetworkServer.Spawn(Model.GameObject);

                GameObject zeroDoor = new();
                zeroDoor.transform.parent = Model.GameObject.transform;
                zeroDoor.transform.localPosition = Vector3.down * 1.7835f;
                zeroDoor.transform.rotation = Model.GameObject.transform.rotation;

                if (Round.Waiting)
                {
                    DSD.Add(new(zeroDoor.transform.position, zeroDoor.transform.rotation, first));
                }
                else
                {
                    Door door = new(zeroDoor.transform.position, DoorPrefabs.DoorHCZ, zeroDoor.transform.rotation)
                    {
                        Name = LiftDoorName,
                        Scale = new(1, 1, 1.5f)
                    };
                    if (first) door.Open = true;
                    _doors.Add(new(door, first));
                }

                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(-1.35575f, 0), Vector3.zero, new(1.305f, 3.5525f, 0.29f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(1.35575f, 0), Vector3.zero, new(1.305f, 3.5525f, 0.29f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(-0.012f, 1.18175f), Vector3.zero, new(1.45f, 1.189f, 0.29f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(-1.971991f, 0, -2.171383f), Vector3.zero, new(0.0725f, 3.5525f, 4.06f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(1.972f, 0, -2.171367f), Vector3.zero, new(0.0725f, 3.5525f, 4.06f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(0, 0, -4.165125f), new(0, 90), new(0.0725f, 3.5525f, 3.9875f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(0, -1.801626f, -1.993749f), new(0, 0, 90), new(0.0725f, 3.9875f, 4.2775f)));
                Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cube, color, new(0, 1.74f, -2.131501f), new(0, 0, 90), new(0.0725f, 3.9875f, 4.06f)));
                Model.AddPart(new ModelLight(Model, new Color32(120, 120, 120, 255), new(0, 1.199874f, -1.759576f), lightRange: 7));
                ModelPrimitive sensor = new(Model, PrimitiveType.Quad, _lime, new(-0.9062505f, 0.2175004f, 0.1493461f), new(0, 180), new(0.145f, 0.29f, 1));
                Model.AddPart(sensor, false);

                foreach (var prim in Model.Primitives)
                {
                    prim.Primitive.IsStatic = true;
                }

                if (cre.NoShowDoor)
                {
                    Model CustomDoor = new("CustomHCZDoor", Vector3.zero, Vector3.zero, Model);
                    CreateButton(new(-0.9f, -0.516925f, 0.151525f), Vector3.zero);
                    CreateButton(new(0.9f, -0.516925f, -0.15225f), new(0, 180));

                    void CreateButton(Vector3 pos, Vector3 rot)
                    {
                        Model Button = new("Button", pos, rot, CustomDoor);
                        Button.AddPart(new ModelPrimitive(Button, PrimitiveType.Cube, new Color32(60, 60, 60, 255), Vector3.zero, Vector3.zero, new(0.18125f, 0.18125f, 0.0725f)));
                        Button.AddPart(new ModelPrimitive(Button, PrimitiveType.Cylinder, Color.red, new(0, 0, 0.03625f), new(90, 0), new(0.10875f, 0.00725f, 0.10875f)));
                    }

                    Model DoorLeft = new("DoorLeft", new(first ? -1.069f : -0.3625f, -0.590875f), Vector3.zero, CustomDoor);
                    DoorLeft.AddPart(new ModelPrimitive(DoorLeft, PrimitiveType.Cube, new Color32(48, 48, 48, 255), Vector3.zero, Vector3.zero, new(0.725f, 2.35625f, 0.145f)));
                    DoorLeft.AddPart(new ModelPrimitive(DoorLeft, PrimitiveType.Quad, _red, new(0.12325f, 0, -0.07322501f), Vector3.zero, new(0.2175f, 1.0875f, 0.1595f)));
                    DoorLeft.AddPart(new ModelPrimitive(DoorLeft, PrimitiveType.Quad, _red, new(0.2523f, 0.47125f, -0.07322501f), Vector3.zero, new(0.2175f, 0.145f, 0.1595f)));
                    DoorLeft.AddPart(new ModelPrimitive(DoorLeft, PrimitiveType.Quad, _red, new(0.2523f, 0.05437499f, -0.07322501f), Vector3.zero, new(0.2175f, 0.145f, 0.1595f)));
                    DoorLeft.AddPart(new ModelPrimitive(DoorLeft, PrimitiveType.Quad, _red, new(0.21895f, 0.05437499f, 0.07322501f), new(0, 180), new(0.29f, 0.145f, 0.1595f)));
                    DoorLeft.AddPart(new ModelPrimitive(DoorLeft, PrimitiveType.Quad, _red, new(0.21895f, 0.47125f, 0.07322501f), new(0, 180), new(0.29f, 0.145f, 0.1595f)));

                    Model DoorRight = new("DoorRight", new(first ? 1.069f : 0.3625f, -0.590875f), Vector3.zero, CustomDoor);
                    DoorRight.AddPart(new ModelPrimitive(DoorRight, PrimitiveType.Cube, new Color32(48, 48, 48, 255), Vector3.zero, Vector3.zero, new(0.725f, 2.35625f, 0.145f)));
                    DoorRight.AddPart(new ModelPrimitive(DoorRight, PrimitiveType.Quad, _red, new(-0.21895f, 0.47125f, -0.07322501f), Vector3.zero, new(0.29f, 0.145f, 0.1595f)));
                    DoorRight.AddPart(new ModelPrimitive(DoorRight, PrimitiveType.Quad, _red, new(-0.21895f, 0.05437499f, -0.07322501f), Vector3.zero, new(0.29f, 0.145f, 0.1595f)));
                    DoorRight.AddPart(new ModelPrimitive(DoorRight, PrimitiveType.Quad, _red, new(-0.12325f, 0, 0.07322501f), new(0, 180), new(0.2175f, 1.0875f, 0.1595f)));
                    DoorRight.AddPart(new ModelPrimitive(DoorRight, PrimitiveType.Quad, _red, new(-0.2523f, 0.47125f, 0.07322501f), new(0, 180), new(0.2175f, 0.145f, 0.1595f)));
                    DoorRight.AddPart(new ModelPrimitive(DoorRight, PrimitiveType.Quad, _red, new(-0.2523f, 0.05437499f, 0.07322501f), new(0, 180), new(0.2175f, 0.145f, 0.1595f)));

                    try
                    {
                        DoorLeft.Primitives.ForEach(prim => prim.Primitive.MovementSmoothing = 64);
                        DoorLeft.GameObject.AddComponent<FixPrimitiveSmoothing>().Model = DoorLeft;
                    }
                    catch { }
                    try
                    {
                        DoorRight.Primitives.ForEach(prim => prim.Primitive.MovementSmoothing = 64);
                        DoorRight.GameObject.AddComponent<FixPrimitiveSmoothing>().Model = DoorRight;
                    }
                    catch { }

                    return new(sensor, new(Model.GameObject.transform), new(DoorLeft.GameObject.transform, DoorRight.GameObject.transform));
                }

                return new(sensor, new(Model.GameObject.transform), null);
            }
        }


        [EventMethod(AlphaEvents.Detonate)]
        internal static void Detonated()
        {
            foreach (var lift in List)
            {
                if (!lift.StopAlpha) continue;
                try { lift.Objects.FirstPanel.Primitive.Color = _red; } catch { }
                try { lift.Objects.SecondPanel.Primitive.Color = _red; } catch { }
            }
        }

        [EventMethod(RoundEvents.Start)]
        internal static void SpawnDoors()
        {
            Locked = false;
            Timing.CallDelayed(5f, () =>
            {
                foreach (Lift lift in List)
                {
                    List<LiftDoor> _doors = new();
                    foreach (var dsd in lift.DSD)
                    {
                        Door door = new(dsd.Position, DoorPrefabs.DoorHCZ, dsd.Rotation)
                        {
                            Name = LiftDoorName,
                            Scale = new(1, 1, 1.5f)
                        };
                        if (dsd.First) door.Open = true;
                        _doors.Add(new(door, dsd.First));
                    }
                    lift.Objects.Doors.AddRange(_doors);
                    lift.DSD.Clear();
                }
            });
        }

        [EventMethod(MapEvents.DamageDoor)]
        internal static void DoorEvents(DamageDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not LiftDoorName) return;
            ev.Allowed = false;
        }

        [EventMethod(MapEvents.LockDoor)]
        internal static void DoorEvents(LockDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not LiftDoorName) return;
            ev.Allowed = false;
        }

        [EventMethod(MapEvents.OpenDoor)]
        internal static void DoorEvents(OpenDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not LiftDoorName) return;
            ev.Allowed = false;
        }

        /*[EventMethod()]
        internal static void DoorEvents(Scp079InteractDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not LiftDoorName) return;
            ev.Allowed = false;
        }

        [EventMethod()]
        internal static void DoorEvents(Scp079LockDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not LiftDoorName) return;
            ev.Allowed = false;
        }*/

        [EventMethod(PlayerEvents.InteractDoor)]
        internal static void DoorEvents(InteractDoorEvent ev)
        {
            if (ev.Door is null) return;
            if (ev.Door.Name is not LiftDoorName) return;
            if (!List.TryFind(out Lift lift, x => x.Objects.Doors.Any(x => x.Door == ev.Door))) return;
            ev.Allowed = false;
            lift.Interact();
        }
        internal void Interact()
        {
            if (Locked) return;
            if (StopAlpha && Alpha.Detonated) return;
            if (Round.Waiting) return;
            if (Using) return;
            if (InFirst) Timing.RunCoroutine(ToSecond(), "CustomLiftRunning");
            else Timing.RunCoroutine(ToFirst(), "CustomLiftRunning");
            IEnumerator<float> ToFirst()
            {
                Using = true;
                try { Objects.FirstPanel.Primitive.Color = _yellow; } catch { }
                try { Objects.SecondPanel.Primitive.Color = _yellow; } catch { }
                var _low = Objects.SecondPositions.Low;
                var _big = Objects.SecondPositions.Big;
                Vector3 LowPos = new(Mathf.Min(_low.x, _big.x), Mathf.Min(_low.y, _big.y), Mathf.Min(_low.z, _big.z));
                Vector3 BigPos = new(Mathf.Max(_low.x, _big.x), Mathf.Max(_low.y, _big.y), Mathf.Max(_low.z, _big.z));
                try { foreach (var door in Objects.Doors) door.Door.Open = false; } catch { }
                yield return Timing.WaitForSeconds(0.7f);
                if (Objects.SecondCustomDoors is not null)
                {
                    for (float i = 0; 10 >= i; i++)
                    {
                        try { Objects.SecondCustomDoors.Left.localPosition = new Vector3(-1.069f + 0.075f * i, -0.590875f); } catch { }
                        try { Objects.SecondCustomDoors.Right.localPosition = new Vector3(1.069f - 0.075f * i, -0.590875f); } catch { }
                        yield return Timing.WaitForSeconds(0.05f);
                    }
                    try { Objects.SecondCustomDoors.Left.localPosition = new Vector3(-0.3625f, -0.590875f); } catch { }
                    try { Objects.SecondCustomDoors.Right.localPosition = new Vector3(0.3625f, -0.590875f); } catch { }
                }
                if (Objects.FirstCustomDoors is not null)
                {
                    try { Objects.FirstCustomDoors.Left.localPosition = new Vector3(-0.3625f, -0.590875f); } catch { }
                    try { Objects.FirstCustomDoors.Right.localPosition = new Vector3(0.3625f, -0.590875f); } catch { }
                }
                float time = Speed / 2;
                yield return Timing.WaitForSeconds(time);
                if (StopAlpha && Alpha.Detonated)
                {
                    foreach (var pl in Player.List.Where(x => InLift(x.MovementState.Position)))
                        try { pl.HealthInformation.Kill(PlayerStatsSystem.DeathTranslations.Warhead); } catch { }
                    yield break;
                }
                foreach (var pl in Player.List.Where(x => InLift(x.MovementState.Position)))
                    try
                    {
                        GameObject par = new();
                        par.transform.parent = Objects.FirstPositions.CenterTrans;
                        par.transform.localScale = new(0.25f, 0.25f, 0.25f);
                        par.transform.localPosition = Vector3.zero;
                        par.transform.localRotation = Quaternion.identity;

                        GameObject go = new();
                        go.transform.position = pl.MovementState.Position;
                        go.transform.parent = Objects.SecondPositions.CenterTrans;

                        Vector3 pos = new(go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z);
                        Quaternion rot = new(go.transform.localRotation.x, go.transform.localRotation.y,
                            go.transform.localRotation.z, go.transform.localRotation.w);

                        go.transform.parent = par.transform;
                        go.transform.localPosition = pos;
                        go.transform.localRotation = rot;

                        Vector3 eulerRot = go.transform.rotation.eulerAngles;

                        pl.ReferenceHub.TryOverridePosition(go.transform.position);
                        pl.ReferenceHub.TryOverrideRotation(new(eulerRot.x - pl.MovementState.Rotation.x,
                            eulerRot.y - pl.MovementState.Rotation.y));

                        Object.Destroy(go);
                        Object.Destroy(par);
                    }
                    catch { }
                foreach (var pl in Pickup.List.Where(x => InLift(x.Position) && !x.Base.ItsNeededItem()))
                    try { pl.Position += Objects.FirstPositions.Center - Objects.SecondPositions.Center; } catch { }
                yield return Timing.WaitForSeconds(time);
                try { foreach (var door in Objects.Doors) if (door.ThisFirst) door.Door.Open = true; } catch { }
                if (Objects.FirstCustomDoors is not null)
                {
                    for (float i = 0; 10 >= i; i++)
                    {
                        try { Objects.FirstCustomDoors.Left.localPosition = new Vector3(-0.3625f - 0.075f * i, -0.590875f); } catch { }
                        try { Objects.FirstCustomDoors.Right.localPosition = new Vector3(0.3625f + 0.075f * i, -0.590875f); } catch { }
                        yield return Timing.WaitForSeconds(0.05f);
                    }
                    try { Objects.FirstCustomDoors.Left.localPosition = new Vector3(-1.069f, -0.590875f); } catch { }
                    try { Objects.FirstCustomDoors.Right.localPosition = new Vector3(1.069f, -0.590875f); } catch { }
                }
                try { Objects.FirstPanel.Primitive.Color = _lime; } catch { }
                try { Objects.SecondPanel.Primitive.Color = _cyan; } catch { }
                InFirst = true;
                yield return Timing.WaitForSeconds(1.5f);
                Using = false;
                bool InLift(Vector3 pos) => pos.x > LowPos.x && BigPos.x > pos.x && pos.y > LowPos.y &&
                    BigPos.y > pos.y && pos.z > LowPos.z && BigPos.z > pos.z;
            }
            IEnumerator<float> ToSecond()
            {
                Using = true;
                try { Objects.FirstPanel.Primitive.Color = _yellow; } catch { }
                try { Objects.SecondPanel.Primitive.Color = _yellow; } catch { }
                var _low = Objects.FirstPositions.Low;
                var _big = Objects.FirstPositions.Big;
                Vector3 LowPos = new(Mathf.Min(_low.x, _big.x), Mathf.Min(_low.y, _big.y), Mathf.Min(_low.z, _big.z));
                Vector3 BigPos = new(Mathf.Max(_low.x, _big.x), Mathf.Max(_low.y, _big.y), Mathf.Max(_low.z, _big.z));
                try { foreach (var door in Objects.Doors) door.Door.Open = false; } catch { }
                yield return Timing.WaitForSeconds(0.7f);
                if (Objects.FirstCustomDoors is not null)
                {
                    for (float i = 0; 10 >= i; i++)
                    {
                        try { Objects.FirstCustomDoors.Left.localPosition = new Vector3(-1.069f + 0.075f * i, -0.590875f); } catch { }
                        try { Objects.FirstCustomDoors.Right.localPosition = new Vector3(1.069f - 0.075f * i, -0.590875f); } catch { }
                        yield return Timing.WaitForSeconds(0.05f);
                    }
                    try { Objects.FirstCustomDoors.Left.localPosition = new Vector3(-0.3625f, -0.590875f); } catch { }
                    try { Objects.FirstCustomDoors.Right.localPosition = new Vector3(0.3625f, -0.590875f); } catch { }
                }
                if (Objects.SecondCustomDoors is not null)
                {
                    try { Objects.SecondCustomDoors.Left.localPosition = new Vector3(-0.3625f, -0.590875f); } catch { }
                    try { Objects.SecondCustomDoors.Right.localPosition = new Vector3(0.3625f, -0.590875f); } catch { }
                }
                float time = Speed / 2;
                yield return Timing.WaitForSeconds(time);
                foreach (var pl in Player.List.Where(x => InLift(x.MovementState.Position)))
                    try
                    {
                        GameObject par = new();
                        par.transform.parent = Objects.SecondPositions.CenterTrans;
                        par.transform.localPosition = Vector3.zero;
                        par.transform.localRotation = Quaternion.identity;
                        par.transform.localScale = new(0.25f, 0.25f, 0.25f);

                        GameObject go = new();
                        go.transform.position = pl.MovementState.Position;
                        go.transform.parent = Objects.FirstPositions.CenterTrans;

                        Vector3 pos = new(go.transform.localPosition.x, go.transform.localPosition.y, go.transform.localPosition.z);
                        Quaternion rot = new(go.transform.localRotation.x, go.transform.localRotation.y,
                            go.transform.localRotation.z, go.transform.localRotation.w);

                        go.transform.parent = par.transform;
                        go.transform.localPosition = pos;
                        go.transform.localRotation = rot;

                        Vector3 eulerRot = go.transform.rotation.eulerAngles;

                        pl.ReferenceHub.TryOverridePosition(go.transform.position);
                        pl.ReferenceHub.TryOverrideRotation(new(eulerRot.x - pl.MovementState.Rotation.x,
                            eulerRot.y - pl.MovementState.Rotation.y));

                        Object.Destroy(go);
                        Object.Destroy(par);
                    }
                    catch { }
                foreach (var pl in Pickup.List.Where(x => InLift(x.Position) && !x.Base.ItsNeededItem()))
                    try { pl.Position += Objects.SecondPositions.Center - Objects.FirstPositions.Center; } catch { }
                yield return Timing.WaitForSeconds(time);
                try { foreach (var door in Objects.Doors) if (!door.ThisFirst) door.Door.Open = true; } catch { }
                if (Objects.SecondCustomDoors is not null)
                {
                    for (float i = 0; 10 >= i; i++)
                    {
                        try { Objects.SecondCustomDoors.Left.localPosition = new Vector3(-0.3625f - 0.075f * i, -0.590875f); } catch { }
                        try { Objects.SecondCustomDoors.Right.localPosition = new Vector3(0.3625f + 0.075f * i, -0.590875f); } catch { }
                        yield return Timing.WaitForSeconds(0.05f);
                    }
                    try { Objects.SecondCustomDoors.Left.localPosition = new Vector3(-1.069f, -0.590875f); } catch { }
                    try { Objects.SecondCustomDoors.Right.localPosition = new Vector3(1.069f, -0.590875f); } catch { }
                }
                try { Objects.SecondPanel.Primitive.Color = _lime; } catch { }
                try { Objects.FirstPanel.Primitive.Color = _cyan; } catch { }
                InFirst = false;
                yield return Timing.WaitForSeconds(1.5f);
                Using = false;
                bool InLift(Vector3 pos) => pos.x > LowPos.x && BigPos.x > pos.x && pos.y > LowPos.y &&
                    BigPos.y > pos.y && pos.z > LowPos.z && BigPos.z > pos.z;
            }
        }
        private class DoSpawnDoor
        {
            internal readonly Vector3 Position;
            internal readonly Quaternion Rotation;
            internal readonly bool First;
            internal DoSpawnDoor(Vector3 position, Quaternion rotation, bool first)
            {
                Position = position;
                Rotation = rotation;
                First = first;
            }
        }
        internal class LiftDoor
        {
            internal readonly Door Door;
            internal readonly bool ThisFirst;
            internal LiftDoor(Door door, bool thisFirst)
            {
                Door = door;
                ThisFirst = thisFirst;
            }
        }
        internal class CustomDoor
        {
            internal readonly Transform Left;
            internal readonly Transform Right;
            internal CustomDoor(Transform left, Transform right)
            {
                Left = left;
                Right = right;
            }
        }
        private class LocalCreator
        {
            internal readonly ModelPrimitive Sensor;
            internal readonly InLiftPositions Positions;
            internal readonly CustomDoor CustomDoor;
            internal LocalCreator(ModelPrimitive sensor, InLiftPositions positions, CustomDoor customDoor)
            {
                Sensor = sensor;
                Positions = positions;
                CustomDoor = customDoor;
            }
        }
        internal class InLiftPositions
        {
            internal readonly Vector3 Big;
            internal readonly Vector3 Low;
            internal readonly Vector3 Center;
            internal readonly Vector3 Rotation;
            internal readonly Transform CenterTrans;
            internal InLiftPositions(Transform transform)
            {
                Rotation = transform.rotation.eulerAngles;
                {
                    GameObject obj = new();
                    var tr = obj.transform;
                    tr.parent = transform;
                    tr.localPosition = new(1.939f, 1.699f, -0.126f);
                    Big = tr.position;
                }
                {
                    GameObject obj = new();
                    var tr = obj.transform;
                    tr.parent = transform;
                    tr.localPosition = new(-1.934f, -1.764f, -4.1296f);
                    Low = tr.position;
                }
                {
                    GameObject obj = new();
                    var tr = obj.transform;
                    tr.parent = transform;
                    tr.localPosition = new(0.005f, -0.065f, -2.128f);
                    Center = tr.position;
                    CenterTrans = tr;
                }
            }
        }
        internal class LiftCreator
        {
            internal readonly Transform Parent;
            internal readonly Vector3 Position;
            internal readonly Vector3 Rotation;
            internal readonly bool NoShowDoor;
            internal LiftCreator(Transform parent, Vector3 position, Vector3 rotation, bool noShowDoor = false)
            {
                Parent = parent;
                Position = position;
                Rotation = rotation;
                NoShowDoor = noShowDoor;
            }
        }
        internal class LiftObjects
        {
            internal readonly LiftCreator FirstCreator;
            internal readonly LiftCreator SecondCreator;
            internal readonly InLiftPositions FirstPositions;
            internal readonly InLiftPositions SecondPositions;
            internal readonly List<LiftDoor> Doors;
            internal readonly ModelPrimitive FirstPanel;
            internal readonly ModelPrimitive SecondPanel;
            internal readonly CustomDoor FirstCustomDoors;
            internal readonly CustomDoor SecondCustomDoors;
            public LiftObjects(LiftCreator firstCreator, LiftCreator secondCreator, InLiftPositions firstPositions,
                InLiftPositions secondPositions, List<LiftDoor> doors, ModelPrimitive firstPanel, ModelPrimitive secondPanel,
                CustomDoor firstCustomDoors, CustomDoor secondCustomDoors)
            {
                FirstCreator = firstCreator;
                SecondCreator = secondCreator;
                FirstPositions = firstPositions;
                SecondPositions = secondPositions;
                Doors = doors;
                FirstPanel = firstPanel;
                SecondPanel = secondPanel;
                FirstCustomDoors = firstCustomDoors;
                SecondCustomDoors = secondCustomDoors;
            }
        }
    }
}