using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Addons
{
    static class Caches
    {
        static internal Dictionary<string, VecPos> Positions { get; } = new();
        static internal Dictionary<int, RoleTypeId> Role { get; } = new();

        static internal bool IsAlive(string userid)
        {
            if (!Positions.TryGetValue(userid, out var _data))
                return true;

            return _data.Alive;
        }

        static internal void PosCheck()
        {
            try
            {
                foreach (Player pl in Player.List)
                {
                    if (!Positions.ContainsKey(pl.UserInformation.UserId))
                        Positions.Add(pl.UserInformation.UserId, new VecPos());

                    if (pl.RoleInformation.Role is not RoleTypeId.Spectator &&
                        Vector3.Distance(Positions[pl.UserInformation.UserId].Pos, pl.MovementState.Position) < 0.1)
                    {
                        if (Positions[pl.UserInformation.UserId].sec > 30)
                        {
                            Positions[pl.UserInformation.UserId].Alive = false;
                        }
                        else
                        {
                            Positions[pl.UserInformation.UserId].sec += 5;
                            Positions[pl.UserInformation.UserId].Pos = pl.MovementState.Position;
                        }
                    }
                    else
                    {
                        Positions[pl.UserInformation.UserId].Alive = true;
                        Positions[pl.UserInformation.UserId].sec = 0;
                        Positions[pl.UserInformation.UserId].Pos = pl.MovementState.Position;
                    }
                }
            }
            catch { }
        }

        [EventMethod(PlayerEvents.Join)]
        static void Join(JoinEvent ev)
        {
            if (Role.ContainsKey(ev.Player.UserInformation.Id))
                Role.Remove(ev.Player.UserInformation.Id);

            Role.Add(ev.Player.UserInformation.Id, RoleTypeId.Spectator);
        }

        [EventMethod(PlayerEvents.Dies, int.MinValue)]
        static void Dies(DiesEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (!Role.ContainsKey(ev.Target.UserInformation.Id))
                Role.Add(ev.Target.UserInformation.Id, ev.Target.RoleInformation.Role);
            else
                Role[ev.Target.UserInformation.Id] = ev.Target.RoleInformation.Role;
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            Positions.Clear();
            Role.Clear();
        }

        [Serializable]
        internal class VecPos
        {
            internal int sec = 0;
            internal Vector3 Pos = new();
            internal bool Alive { get; set; } = true;
        }
    }
}