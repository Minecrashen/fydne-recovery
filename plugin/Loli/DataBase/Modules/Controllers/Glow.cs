#if NR
using Mirror;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Loli.DataBase.Modules.Controllers
{
    internal class Glow
    {
        internal static readonly List<Glow> Glows = new();
        static readonly Vector3 Position = new(0, 1.425f, 0);
        //static readonly Vector3 Position = new(-0.572f, 1.425f, -0.155f);

        internal readonly Player Player;
        internal readonly LightPoint Light;

        internal Glow(Player pl, Color32 color)
        {
            var lg = new LightPoint(pl.MovementState.Position, color, 1, 5, shadowStrength: 0);
            lg.Base.transform.parent = pl.MovementState.Transform;
            lg.Base.transform.localPosition = Position;
            lg.ShadowStrength = 0;
            Player = pl;
            Light = lg;
            Glows.Add(this);

            foreach (var cat in Player.List)
            {
                try
                {
                    if (cat.RoleInformation.Role is not RoleTypeId.Scp939 and not RoleTypeId.Scp106 and not RoleTypeId.Scp096)
                        continue;

                    if (!Light.Base.gameObject.TryGetComponent(out NetworkIdentity identity))
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
                catch (Exception e) { Log.Error(e); }
            }
        }

        internal void Destroy()
        {
            try { Glows.Remove(this); } catch { }
            try { Light.Destroy(); } catch { }
        }


        [EventMethod(EffectEvents.Enabled)]
        internal static void Update(EffectEnabledEvent ev)
        {
            if (ev.Type != EffectType.Invisible)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Light.Base.transform.parent = null; } catch { }
            try { obj.Light.Position = Vector3.zero; } catch { }
        }

        [EventMethod(EffectEvents.Disabled)]
        internal static void Update(EffectDisabledEvent ev)
        {
            if (ev.Type != EffectType.Invisible)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Light.Base.transform.parent = ev.Player.Transform; } catch { }
            try { obj.Light.Base.transform.localPosition = Position; } catch { }
        }

        [EventMethod(PlayerEvents.Leave)]
        static void Leave(LeaveEvent ev)
        {
            if (!TryGet(ev.Player, out var obj))
                return;

            Glows.Remove(obj);
            try { obj.Light.Destroy(); } catch { }
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void RoleChange(ChangeRoleEvent ev)
        {
            if (ev.Role != RoleTypeId.Spectator)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Light.Base.transform.parent = null; } catch { }
            try { obj.Light.Position = Vector3.zero; } catch { }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void RoleChange(SpawnEvent ev)
        {
            if (ev.Role == RoleTypeId.Spectator || ev.Role == RoleTypeId.None)
                return;

            if (!TryGet(ev.Player, out var obj))
                return;

            try { obj.Light.Base.transform.parent = ev.Player.MovementState.Transform; } catch { }
            try { obj.Light.Base.transform.localPosition = Position; } catch { }
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
                foreach (var obj in Glows)
                {
                    if (!obj.Light.Base.gameObject.TryGetComponent(out NetworkIdentity identity))
                    {
                        continue;
                    }

                    identity.AddObserver(ev.Player.ConnectionToClient);
                }
            }
            else if (boolean2)
            {
                foreach (var obj in Glows)
                {
                    try
                    {
                        if (!obj.Light.Base.gameObject.TryGetComponent(out NetworkIdentity identity))
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
                    catch (Exception e) { Log.Error(e); }
                }
            }
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting() => Glows.Clear();

        static internal bool TryGet(Player pl, out Glow glow)
        {
            var list = Glows.ToArray();
            foreach (var item in list)
                if (item.Player == pl)
                {
                    glow = item;
                    return true;
                }
            glow = default;
            return false;
        }
    }
}
#endif