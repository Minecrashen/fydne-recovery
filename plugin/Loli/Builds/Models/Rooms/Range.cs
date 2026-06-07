using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events.Structs;
using Qurre.Events;
using System.IO;
using UnityEngine;
using Qurre.API.Controllers;
using Qurre.API.World;
using SchematicUnity.API.Objects;

namespace Loli.Builds.Models.Rooms
{
    static class Range
    {
        static float YVent = 0;
        static Room VentRoom;

        static internal Vector3 ChaosSpawnPoint { get; private set; }
        static internal Vector3 DonateSpawnPoint { get; private set; }

        static internal bool InvisibleInCheck(Player pl)
        {
            Vector3 pos = pl.MovementState.Position;
            // -402 984 155
            // -353 1000 124
            if ((pos.x < -353 && pos.x > -402) &&
                (pos.y > 284 && pos.y < 3000) &&
                (pos.z > 124 && pos.z < 155))
                return true;
            if (pos.y > YVent && pos.y < YVent + 20 && pl.GamePlay.Room == VentRoom)
                return true;
            if (pos.y > 294f && pos.x.Difference(-130) < 10 && pos.z.Difference(-20) < 10)
                return true;

            return false;
        }

#if MRP
        // TODO: [EventMethod(PlayerEvents.Spawn, 666)]
#elif NR
        [EventMethod(PlayerEvents.Spawn, 666)]
#endif
        static void SpawnChangePos(SpawnEvent ev)
        {
            if (DonateSpawnPoint != default && ev.Player.Tag.Contains("DonateSpawnPoint"))
                ev.Position = DonateSpawnPoint;
            else if (ChaosSpawnPoint != default && ev.Role is RoleTypeId.ChaosConscript
                or RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosRifleman)
                ev.Position = ChaosSpawnPoint;
        }

        [EventMethod(PlayerEvents.Damage, -2)]
        static void AntiSpawnDamage(DamageEvent ev)
        {
            if (!InvisibleInCheck(ev.Target))
                return;

            ev.Allowed = false;
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            var Scheme = SchematicUnity.API.SchematicManager.LoadSchematic(Path.Combine(Paths.Plugins, "Schemes", "Range.json"), new(-375, 285, 140), default);
            Model range = new("Range", new(-375, 285, 140));

            var vent = Map.Rooms.FindLast(x => x.Type == RoomType.EzVent);
            {
                Model model = new("LiftModels", vent.Position, vent.Rotation.eulerAngles);
                model.AddPart(new ModelPrimitive(model, PrimitiveType.Cube, Color.white, new(0, 3.484f, -2.285f), Vector3.zero, new(1.7f, 3.74f, 0.166f)));
                model.AddPart(new ModelPrimitive(model, PrimitiveType.Cube, Color.white, new(0, 0.765f, -2.285f), Vector3.zero, new(9.65f, 1.75f, 0.17f)));

                foreach (var prim in model.Primitives)
                {
                    prim.Primitive.IsStatic = true;
                }

                spawn_coridor(true);
                spawn_coridor(false);

                YVent = vent.Transform.position.y + 2f;
                VentRoom = vent;

                void spawn_coridor(bool left)
                {
                    Color32 steklo = new(255, 255, 255, 150);

                    Model coridor = new("Coridor", new(left ? 2.768f : -2.782f, 1.65f, -1.358f), Vector3.zero, model);
                    coridor.AddPart(new ModelPrimitive(coridor, PrimitiveType.Cube, Color.white, Vector3.zero, Vector3.zero, new(1.5f, 0.1f, 1.7f)));
                    coridor.AddPart(new ModelPrimitive(coridor, PrimitiveType.Cube, Color.white, new(0, -0.83f, 0.76f), new(0, 90), new(0.17f, 1.7f, 1.5f)));

                    coridor.AddPart(new ModelPrimitive(coridor, PrimitiveType.Cube, steklo, new(0, 0.183f, 0.846f), Vector3.zero, new(1.43f, 0.3f, 0.01f)));

                    coridor.AddPart(new ModelPrimitive(coridor,
                        PrimitiveType.Cube, Color.white,
                        new(left ? 0.78f : -0.78f, 1.8f, 0.42f),
                        Vector3.zero, new(0.15f, 3.6f, 2.9f)
                        ));
                    coridor.AddPart(new ModelPrimitive(coridor,
                        PrimitiveType.Cube, steklo,
                        new(left ? -0.78f : 0.78f, 1.8f, -0.09f),
                        Vector3.zero, new(0.15f, 3.6f, 1.87f)
                        ));
                    coridor.AddPart(new ModelPrimitive(coridor,
                        PrimitiveType.Cube, steklo,
                        new(left ? -0.78f : 0.78f, 2.1f, 1.33f),
                        Vector3.zero, new(0.15f, 3f, 1f)
                        ));
                    coridor.AddPart(new ModelPrimitive(coridor,
                        PrimitiveType.Quad, steklo,
                        new(left ? -0.844f : 0.844f, -0.33f, 1.325f),
                        new(0, left ? 90 : -90), new(1, 0.1f, 1)
                        ));

                    coridor.AddPart(new ModelPrimitive(coridor,
                        PrimitiveType.Cube, Color.white,
                        new(left ? 0.78f : -0.78f, -0.8f, 1.33f),
                        Vector3.zero, new(0.15f, 1.8f, 1)
                        ));
                    coridor.AddPart(new ModelPrimitive(coridor,
                        PrimitiveType.Cube, Color.white,
                        new(left ? -0.78f : 0.78f, -0.8f, -0.01f),
                        Vector3.zero, new(0.15f, 1.8f, 1.7f)
                        ));

                    foreach (var prim in coridor.Primitives)
                    {
                        prim.Primitive.IsStatic = true;
                    }
                }
            }

            new Lift(new(range.GameObject.transform, new(14.94f, 6.165017f, -9.487508f), Vector3.zero, true),
                new(null, new(-135.45f, 296.47f, -24.04f), new(0, 180), true), Color.white);
            new Lift(new(range.GameObject.transform, new(14.94f, 6.165f, 9.5f), new(0, 180), true),
                new(vent.Transform, new(2.764f, 3.47f, -2.34f), Vector3.zero), Color.white, true);

            new Lift(new(range.GameObject.transform, new(-18.9f, 1.855f, 7.317f), new(0, 90), true),
                new(null, new(-128.359f, 296.47f, -23.96f), new(0, 180), true), Color.white);
            new Lift(new(range.GameObject.transform, new(-18.9f, 1.855f, 3.39f), new(0, 90), true),
                new(vent.Transform, new(-2.78f, 3.47f, -2.34f), Vector3.zero), Color.white, true);

            range.AddPart(new ModelTarget(range, TargetPrefabs.Sport, new(3.9f, 0.037f, 5.78f), new(0, -50f), Vector3.one));
            range.AddPart(new ModelTarget(range, TargetPrefabs.Sport, new(3.9f, 0.037f, -5.78f), new(0, 50f), Vector3.one));

            range.AddPart(new ModelTarget(range, TargetPrefabs.Sport, new(-0.98f, 0.037f, 7.77f), new(0, -50f), Vector3.one));
            range.AddPart(new ModelTarget(range, TargetPrefabs.Sport, new(-0.98f, 0.037f, -7.77f), new(0, 50f), Vector3.one));

            range.AddPart(new ModelTarget(range, TargetPrefabs.Binary, new(6.75f, 0.037f, 3.75f), new(0, -30), Vector3.one));
            range.AddPart(new ModelTarget(range, TargetPrefabs.Binary, new(6.75f, 0.037f, -3.75f), new(0, 30), Vector3.one));

            range.AddPart(new ModelTarget(range, TargetPrefabs.Dboy, new(1.62f, 0.037f, 7f), new(0, -50), Vector3.one));
            range.AddPart(new ModelTarget(range, TargetPrefabs.Dboy, new(1.62f, 0.037f, -7f), new(0, 50), Vector3.one));
            range.AddPart(new ModelTarget(range, TargetPrefabs.Dboy, new(10f, 0.037f), Vector3.zero, Vector3.one));

            range.AddPart(new ModelWorkStation(range, new(-11.93f, 0.037f, -9.333f), Vector3.zero, Vector3.one));
            range.AddPart(new ModelWorkStation(range, new(-15.75f, 0.037f, -9.333f), Vector3.zero, Vector3.one));

            {
                GameObject go = new("ChaosSpawn");
                go.transform.parent = range.GameObject.transform;
                go.transform.localPosition = new(-13f, 2f);
                ChaosSpawnPoint = go.transform.position;
                Object.Destroy(go);
            }
            {
                GameObject go = new("DonateSpawn");
                go.transform.parent = range.GameObject.transform;
                go.transform.localPosition = new(15f, 6f);
                DonateSpawnPoint = go.transform.position;
                Object.Destroy(go);
            }

            foreach (var _obj in Scheme.Objects)
                findObjects(_obj);

            static void findObjects(SObject obj)
            {
                if (obj is null)
                    return;

                if (obj.Primitive != null)
                {
                    PrimitiveParams prm = (PrimitiveParams)obj.Primitive;
                    prm.Base.IsStatic = true;
                }

                if (obj.Childrens is null)
                    return;

                foreach (var _obj in obj.Childrens)
                    findObjects(_obj);
            }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void FixGuardSpawn(SpawnEvent ev)
        {
            if (ev.Role != RoleTypeId.FacilityGuard)
                return;

            if (Vector3.Distance(VentRoom.Position, ev.Position) > 10)
                return;

            ev.Position = DonateSpawnPoint;
        }


        static internal Vector3 CheckPosition(Vector3 pos)
        {
            if (Vector3.Distance(VentRoom.Position, pos) < 11)
                return DonateSpawnPoint;

            if (pos == Vector3.zero)
                return DonateSpawnPoint;

            return pos;
        }
    }
}