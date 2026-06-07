using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using System.Collections.Generic;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons
{
    static class ScpHeal
    {
        static readonly Dictionary<string, Vector3> Positions = new();
        static internal void Heal()
        {
            if (Alpha.Detonated) return;
            foreach (Player player in Player.List)
            {
                if (player.RoleInformation.Team is not Team.SCPs) continue;
                if (player.HealthInformation.MaxHp > player.HealthInformation.Hp)
                {
                    if (player.RoleInformation.Role is RoleTypeId.Scp049)
                        Heal(player, 3);
                    else if (player.RoleInformation.Role is not RoleTypeId.Scp079)
                        Heal(player, 1);
                }
            }
        }
        static void Heal(Player player, int hp)
        {
            if (Positions.ContainsKey(player.UserInformation.UserId))
            {
                if (Vector3.Distance(player.MovementState.Position, Positions[player.UserInformation.UserId]) <= 2f)
                {
                    if (player.HealthInformation.MaxHp > player.HealthInformation.Hp + hp) player.HealthInformation.Hp += hp;
                    else player.HealthInformation.Hp = player.HealthInformation.MaxHp;
                }
                Positions[player.UserInformation.UserId] = player.MovementState.Position;
            }
            else Positions.Add(player.UserInformation.UserId, player.MovementState.Position);
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            Positions.Clear();
        }
    }
}