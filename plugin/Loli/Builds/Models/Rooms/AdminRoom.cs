using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using SchematicUnity.API.Objects;
using System.Collections.Generic;
using System.IO;
using LabApi.Features.Wrappers;
using Qurre.API.Controllers;
using UnityEngine;
using Round = Qurre.API.World.Round;

namespace Loli.Builds.Models.Rooms;

static class AdminRoom
{
    private static readonly HashSet<uint> _serial = [];
    const string PickupTag = "AdminTirGun";
    static Vector3 TutorialSpawnPoint = Vector3.zero;
    static Vector3 WaitingSpawnPoint = Vector3.zero;

    static internal Vector3 GetSpawnPoint()
        => TutorialSpawnPoint;

    [EventMethod(PlayerEvents.Spawn, 666)]
    static void SpawnChangePos(SpawnEvent ev)
    {
#if MRP
        if (ev.Role is not RoleTypeId.Tutorial and not RoleTypeId.Scp3114)
#elif NR
        if (ev.Role is not RoleTypeId.Tutorial and not RoleTypeId.Scp3114)
#endif
            return;

        if (Round.Waiting)
        {
            ev.Position = WaitingSpawnPoint;
            ev.Player.GetAmmo();
            return;
        }

        ev.Position = TutorialSpawnPoint;
    }

    [EventMethod(PlayerEvents.DroppedItem)]
    static void DropItem(DroppedItemEvent ev)
    {
        if (ev.Player.RoleInformation.Role != RoleTypeId.Tutorial)
            return;

        if (Vector3.Distance(WaitingSpawnPoint, ev.Player.MovementState.Position) > 30f)
            return;

        Pickup.Get(ev.Pickup).Destroy();
    }

    [EventMethod(MapEvents.CreatePickup)]
    static void AntiAmmo(CreatePickupEvent ev)
    {
        if (Vector3.Distance(WaitingSpawnPoint, ev.Inventory._hub.gameObject.transform.position) > 30f)
            return;

        if (ev.Info.ItemId.GetCategory() is not ItemCategory.Ammo)
            return;

        ev.Allowed = false;
    }

    [EventMethod(PlayerEvents.PickupItem, 5)]
    static void PickupGun(PickupItemEvent ev)
    {
        if (!_serial.Contains(ev.Pickup.Info.Serial))
            return;

        if (Vector3.Distance(WaitingSpawnPoint, ev.Player.MovementState.Position) > 30f)
            return;

        ev.Allowed = false;

        if (ev.Player.RoleInformation.Role != RoleTypeId.Tutorial)
            return;

        ev.Player.Inventory.AddItem(ev.Pickup.Info.ItemId);
    }

    [EventMethod(RoundEvents.Waiting)]
    static void Load()
    {
        _serial.Clear();
        //Room hcz = RoomType.Hcz049.GetRoom();
        //Model hczRoom = new("HCZ_Room", hcz.Position + Vector3.up * 40, hcz.Rotation.eulerAngles);
        Model hczRoom = new("HCZ_Room", new(130.26f, 300.81f, 101.3f));
        Model model = new("AdminRoom_Objects", Vector3.zero, Vector3.zero, hczRoom);
        model.AddPart(new ModelWorkStation(model, new(-5.38f, 2.586f, -3.5f), Vector3.zero, Vector3.one));

        model.AddPart(new ModelTarget(model, TargetPrefabs.Sport, new(13.964f, 2.62f, 17.223f), new(0, -50),
            Vector3.one));
        model.AddPart(
            new ModelTarget(model, TargetPrefabs.Sport, new(-5.4f, 2.62f, 17.223f), new(0, -130), Vector3.one));
        model.AddPart(new ModelTarget(model, TargetPrefabs.Sport, new(4.118f, 6.88f, 17.974f), new(180, -90),
            Vector3.one));

        model.AddPart(new ModelTarget(model, TargetPrefabs.Binary, new(10.522f, 1.632f, 18.029f), new(0, -90),
            Vector3.one));
        model.AddPart(new ModelTarget(model, TargetPrefabs.Binary, new(4.214f, 1.632f, 18.029f), new(0, -90),
            Vector3.one));
        model.AddPart(new ModelTarget(model, TargetPrefabs.Binary, new(-2.46f, 1.632f, 18.029f), new(0, -90),
            Vector3.one));

        model.AddPart(new ModelTarget(model, TargetPrefabs.Dboy, new(14.43f, 2.62f, 11.668f), new(0, -31),
            Vector3.one));
        model.AddPart(new ModelTarget(model, TargetPrefabs.Dboy, new(-6f, 2.62f, 11.668f), new(0, -149), Vector3.one));
        model.AddPart(new ModelTarget(model, TargetPrefabs.Dboy, new(1.391f, 3.97f, 17.464f), new(0, -90),
            Vector3.one));
        model.AddPart(
            new ModelTarget(model, TargetPrefabs.Dboy, new(7.912f, 3.522f, 17.461f), new(0, -70), Vector3.one));

        TutorialSpawnPoint = new Model("TutorialSpawnPoint", new(0, 1.448f, -0.121f), root: model).GameObject.transform
            .position;
        WaitingSpawnPoint = new Model("WaitingSpawnPoint", new(0, 4.614f, 1.575f), root: model).GameObject.transform
            .position;


        var scheme =
            SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes",
                "AdminRoom.json"), model.GameObject.transform.position, model.GameObject.transform.rotation);

        foreach (var _obj in scheme.Objects)
            FindObjects(_obj);


        List<ModelPickup> gunsTir = new()
        {
            new(model, ItemType.GunCOM15, new(-3.241f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunCOM18, new(-2.603f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunRevolver, new(-1.89f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunCom45, new(-1.217f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunA7, new(-0.342f, 3.893f, -2.97f), Vector3.zero, kinematic: false),

            new(model, ItemType.GunCrossvec, new(1.497f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunFSP9, new(2.292f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunE11SR, new(3.252f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunFRMG0, new(4.357f, 3.893f, -2.97f), Vector3.zero, kinematic: false),

            new(model, ItemType.GunLogicer, new(6.399f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunAK, new(7.347f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            new(model, ItemType.GunShotgun, new(8.233f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
            //new(model, ItemType.ParticleDisruptor, new(9.129f, 3.893f, -2.97f), Vector3.zero, kinematic: false),
        };

        foreach (ModelPickup gun in gunsTir)
        {
            if (gun.Pickup != null)
                _serial.Add(gun.Pickup.Serial);
            model.AddPart(gun);
        }


        static void FindObjects(SObject obj)
        {
            if (obj is null)
                return;

            if (obj.Primitive != null)
            {
                PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                prm.Base.IsStatic = true;

                if (obj.Name.EndsWith("_Uncollable"))
                    prm.Base.Collider = false;

                if (obj.Name.StartsWith("LightQuad"))
                    prm.Base.Color = new(10, 10, 8, 0.1f);
            }

            if (obj.Light != null)
            {
                LightParams lgh = (LightParams)obj.Light;
                lgh.Shadows = false;
            }

            if (obj.Name == "FallBlocks")
            {
                if (!obj.Transform.gameObject.TryGetComponent(out FallBlockMove _))
                    obj.Transform.gameObject.AddComponent<FallBlockMove>();

                return;
            }

            if (obj.Name == "StolbBlock")
            {
                if (!obj.Transform.gameObject.TryGetComponent(out StolbBlockMove _))
                    obj.Transform.gameObject.AddComponent<StolbBlockMove>();

                return;
            }

            foreach (var _obj in obj.Childrens)
                FindObjects(_obj);

        }

    }


    class FallBlockMove : MonoBehaviour
    {
        private const float Interval = 0.1f;

        private bool _toMin;
        float _nextCycle;
        Vector3 _startPos;

        void Start()
        {
            _nextCycle = Time.time;
            _startPos = transform.localPosition;
        }

        void Update()
        {
            if (Time.time < _nextCycle)
                return;

            _nextCycle = Time.time + Interval;

            _startPos.x += _toMin ? 0.5f : -0.5f;
            switch (_startPos.x)
            {
                case <= -13:
                    _startPos.x = -13;
                    _toMin = true;
                    break;
                case > 0:
                    _startPos.x = 0;
                    _toMin = false;
                    break;
            }

            transform.localPosition = _startPos;
        }
    }


    class StolbBlockMove : MonoBehaviour
    {
        private const float Interval = 0.1f;

        private bool _toMin;
        float _nextCycle;
        Vector3 _startPos;

        void Start()
        {
            _nextCycle = Time.time;
            _startPos = transform.localPosition;
        }

        void Update()
        {
            if (Time.time < _nextCycle)
                return;

            _nextCycle = Time.time + Interval;

            _startPos.z += _toMin ? -0.5f : 0.5f;
            switch (_startPos.z)
            {
                case <= -4:
                    _startPos.z = -4;
                    _toMin = false;
                    break;
                case > 2.5f:
                    _startPos.z = 2.5f;
                    _toMin = true;
                    break;
            }

            transform.localPosition = _startPos;
        }
    }
}