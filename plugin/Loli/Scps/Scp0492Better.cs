using CustomPlayerEffects;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Scps
{
    static class Scp0492Better
    {
        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            if (ev.Player == null)
                return;

            try
            {
                if (ev.Player.MovementState.Scale.x != 1 || ev.Player.MovementState.Scale.y != 1 || ev.Player.MovementState.Scale.z != 1)
                    ev.Player.MovementState.Scale = Vector3.one;
            }
            catch
            {
            }

            string tag;
            try
            {
                tag = (ev.Player.Tag ?? string.Empty).Replace("BigZombie", "").Replace("SpeedZombie", "");
                ev.Player.Tag = tag;
            }
            catch
            {
                tag = string.Empty;
            }

            if (tag.Contains("Scp008Invisible"))
                return;
            if (ev.Role is not RoleTypeId.Scp0492)
                return;

            var player = ev.Player;
            Timing.CallDelayed(0.5f, () =>
            {
                if (player == null || player.Disconnected)
                    return;

                SpawnZombieRandom(player);
            });
        }

        [EventMethod(PlayerEvents.Attack)]
        static void Damage(AttackEvent ev)
        {
            if (!ev.Allowed)
                return;
            if (ev.Attacker == null)
                return;
            try
            {
                if (ev.Attacker.RoleInformation.Role is not RoleTypeId.Scp0492)
                    return;
            }
            catch
            {
                return;
            }

            if ((ev.Attacker.Tag ?? string.Empty).Contains("BigZombie"))
                ev.Damage *= 1.5f;
        }


        internal static void SpawnZombieRandom(Player pl)
        {
            int random = Random.Range(0, 100);
            if (30 >= random) SpawnZombie(pl, "BigZombie");
            else if (60 >= random) SpawnZombie(pl, "SpeedZombie");
        }
        internal static void SpawnZombie(Player pl, string type)
        {
            if (pl == null || pl.Disconnected)
                return;

            try
            {
                pl.Tag = (pl.Tag ?? string.Empty).Replace("BigZombie", "").Replace("SpeedZombie", "");

                if (pl.RoleInformation.Role is not RoleTypeId.Scp0492)
                {
                    pl.Tag += "Scp008Invisible";
                    pl.RoleInformation.SetNew(RoleTypeId.Scp0492, RoleChangeReason.Respawn);
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (pl != null && !pl.Disconnected)
                            pl.Tag = (pl.Tag ?? string.Empty).Replace("Scp008Invisible", "");
                    });
                }

                pl.Effects.DisableAll();
            }
            catch
            {
                return;
            }

            if (type == "BigZombie")
            {
                try
                {
                    pl.HealthInformation.Hp = 1000;
                    pl.HealthInformation.MaxHp = 1000;
                    pl.MovementState.Scale = new Vector3(1.2f, 0.9f, 1.2f);
                    pl.Tag = "BigZombie";
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (pl == null || pl.Disconnected)
                            return;

                        pl.HealthInformation.Hp = 1000;
                        pl.HealthInformation.MaxHp = 1000;
                    });
                }
                catch
                {
                }
            }
            else if (type == "SpeedZombie")
            {
                try
                {
                    float scale = Random.Range(85, 90);
                    pl.HealthInformation.Hp = 350;
                    pl.HealthInformation.MaxHp = 350;
                    pl.MovementState.Scale = new Vector3(scale / 100, scale / 100, scale / 100);
                    pl.Tag = "SpeedZombie";
                    if (pl.Effects.TryGet(EffectType.MovementBoost, out StatusEffectBase playerEffect))
                    {
                        playerEffect.Intensity = 30;
                        pl.Effects.Enable(playerEffect);
                    }
                    Timing.CallDelayed(0.5f, () =>
                    {
                        if (pl == null || pl.Disconnected)
                            return;

                        pl.HealthInformation.Hp = 350;
                        pl.HealthInformation.MaxHp = 350;
                    });
                }
                catch
                {
                }
            }
        }
    }
}
