#if NR
using Loli.Addons;
using Mirror;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.DataBase.Modules.Controllers
{
    internal class Nimb
    {
        internal static readonly List<Nimb> Nimbs = new();

        internal readonly Player Player;
        internal readonly Model Model;

        internal Nimb(Player pl)
        {
            Vector3 _pos = new(0, 0.15f, 0);
            Vector3 _size = new(0.055f, 0.055f, 0.055f);
            Color col = new(4, 4, 0);
            Model = new("Nimb", _pos);
            Model.GameObject.transform.localScale = new(0.5f, 0.5f, 0.5f);
            Model.GameObject.transform.parent = pl.ReferenceHub.PlayerCameraReference;
            Model.GameObject.transform.localPosition = _pos;

            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0.237376f, 0, -0.002016022f), new(0, -90, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(-0.2373761f, 0, -0.00604802f), new(0, 90, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(-0.1188481f, 0, -0.20848f), new(0, 30, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(-0.2055041f, 0, -0.122464f), new(0, 60, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0, 0, -0.24048f), new(0, 0, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(-0.2055041f, 0, 0.11312f), new(0, 120, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(-0.1181441f, 0, 0.200544f), new(0, 150, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0.118848f, 0, 0.200416f), new(0, -150, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0.205504f, 0, 0.1144f), new(0, -120, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0, 0, 0.232416f), new(0, 180, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0.205504f, 0, -0.121184f), new(0, -60, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Cylinder, col, new(0.118144f, 0, -0.208608f), new(0, -30, 90), _size));

            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(-0.06291205f, 0, 0.232416f), new(0, 150, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(-0.1735041f, 0, 0.168544f), new(0, 120, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(-0.2373761f, 0, 0.05795198f), new(0, 0, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(-0.2373761f, 0, -0.06729602f), new(0, 60, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(-0.1744f, 0, -0.176416f), new(0, 30, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(-0.064f, 0, -0.24048f), new(0, 0, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(0.06291199f, 0, -0.24048f), new(0, -30, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(0.173504f, 0, -0.176608f), new(0, -60, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(0.237376f, 0, -0.06601603f), new(0, 180, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(0.237376f, 0, 0.05923199f), new(0, -120, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(0.1744f, 0, 0.168352f), new(0, -150, 90), _size));
            Model.AddPart(new ModelPrimitive(Model, PrimitiveType.Sphere, col, new(0.064f, 0, 0.232416f), new(0, 180, 90), _size));

            foreach (var prim in Model.Primitives)
            {
                prim.Primitive.MovementSmoothing = 64;
                prim.Primitive.Collider = false;
            }

            Model.GameObject.AddComponent<FixPrimitiveSmoothing>().Model = Model;

            Player = pl;

            Nimbs.Add(this);

            foreach (var cat in Player.List)
            {
                try
                {
                    if (cat.RoleInformation.Role is not RoleTypeId.Scp939 and not RoleTypeId.Scp106 and not RoleTypeId.Scp096)
                        continue;

                    foreach (var prim in Model.Primitives)
                    {
                        if (!prim.GameObject.TryGetComponent(out NetworkIdentity identity))
                            continue;

                        identity.RemoveObserver(cat.ConnectionToClient);

                        var message = new ObjectDestroyMessage
                        {
                            netId = identity.netId
                        };
                        using NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
                        NetworkMessages.Pack(message, networkWriterPooled);
                        ArraySegment<byte> segment = networkWriterPooled.ToArraySegment();

                        cat.ConnectionToClient.Send(segment);
                    }
                }
                catch (Exception e) { Log.Error(e); }
            }
        }

        internal void Destroy()
        {
            try { Nimbs.Remove(this); } catch { }
            try { Model.Destroy(); } catch { }
        }


        [EventMethod(EffectEvents.Enabled)]
        internal static void Update(EffectEnabledEvent ev)
        {
            if (ev.Type != EffectType.Invisible)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Model.GameObject.transform.parent = null; } catch { }
            try { obj.Model.GameObject.transform.position = Vector3.zero; } catch { }
        }

        [EventMethod(EffectEvents.Disabled)]
        internal static void Update(EffectDisabledEvent ev)
        {
            if (ev.Type != EffectType.Invisible)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Model.GameObject.transform.parent = ev.Player.ReferenceHub.PlayerCameraReference; } catch { }
            try { obj.Model.GameObject.transform.localPosition = new Vector3(0, 0.2f, 0); } catch { }
        }

        [EventMethod(PlayerEvents.Leave)]
        static void Leave(LeaveEvent ev)
        {
            if (!TryGet(ev.Player, out var obj))
                return;

            Nimbs.Remove(obj);
            try { obj.Model.Destroy(); } catch { }
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void RoleChange(ChangeRoleEvent ev)
        {
            if (ev.Role != RoleTypeId.Spectator)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Model.GameObject.transform.parent = null; } catch { }
            try { obj.Model.GameObject.transform.position = Vector3.zero; } catch { }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void RoleChange(SpawnEvent ev)
        {
            if (ev.Role is RoleTypeId.Spectator or RoleTypeId.None)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Model.GameObject.transform.parent = ev.Player.ReferenceHub.PlayerCameraReference; } catch { }
            try { obj.Model.GameObject.transform.localPosition = new Vector3(0, 0.2f, 0); } catch { }
        }

        [EventMethod(PlayerEvents.ChangeRole, int.MinValue)]
        static void UnspawnForDog(ChangeRoleEvent ev)
        {
            if (!ev.Allowed)
                return;

            bool boolean1 = ev.OldRole.RoleTypeId is RoleTypeId.Scp939 or RoleTypeId.Scp106 or RoleTypeId.Scp096;
            bool boolean2 = ev.Role is RoleTypeId.Scp939 or RoleTypeId.Scp106 or RoleTypeId.Scp096;

            if (boolean1 && boolean2)
                return;

            if (boolean1)
            {
                foreach (var obj in Nimbs)
                {
                    foreach (var prim in obj.Model?.Primitives)
                    {
                        if (!prim.GameObject.TryGetComponent(out NetworkIdentity identity))
                            continue;

                        identity.AddObserver(ev.Player.ConnectionToClient);
                    }
                }
            }
            else if (boolean2)
            {
                foreach (var obj in Nimbs)
                {
                    try
                    {
                        foreach (var prim in obj.Model?.Primitives)
                        {
                            if (!prim.GameObject.TryGetComponent(out NetworkIdentity identity))
                                continue;

                            identity.RemoveObserver(ev.Player.ConnectionToClient);

                            var message = new ObjectDestroyMessage
                            {
                                netId = identity.netId
                            };
                            using NetworkWriterPooled networkWriterPooled = NetworkWriterPool.Get();
                            NetworkMessages.Pack(message, networkWriterPooled);
                            ArraySegment<byte> segment = networkWriterPooled.ToArraySegment();

                            ev.Player.ConnectionToClient.Send(segment);
                        }
                    }
                    catch (Exception e) { Log.Error(e); }
                }
            }
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting() => Nimbs.Clear();

        static internal bool TryGet(Player pl, out Nimb nimb)
        {
            var list = Nimbs.ToArray();
            foreach (var item in list)
                if (item.Player == pl)
                {
                    nimb = item;
                    return true;
                }
            nimb = default;
            return false;
        }
    }
}
#endif