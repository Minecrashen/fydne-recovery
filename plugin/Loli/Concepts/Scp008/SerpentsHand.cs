using Loli.Controllers;
using Loli.Spawns;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Qurre.API.World;
using UnityEngine;

#if NR
using Qurre.API.Objects;
#endif

namespace Loli.Concepts.Scp008
{
    static class SerpentsHand
    {
        internal const string Tag = " SerpentsHandPlayer";
        internal const string CoroutineTag = "Loli.Concepts.Scp008.SerpentsHand_Coroutine";

        static int _spawnedPlayers = 0;

        static internal bool Spawned => _spawnedPlayers >= 8;

        static internal bool Spawn()
        {
            if (ConceptsController.IsActivated || Alpha.Detonated)
                return false;

            if (Spawned)
                return false;

            List<Player> list = Player.List.Where(x => x.RoleInformation.Role is RoleTypeId.Spectator).ToList();

#if MRP
            if (list.Count < 5)
#elif NR
            if (list.Count < 3)
#endif
                return false;

            list.Shuffle();

            int i = 0;
#if MRP
            for (; i < list.Count && _spawnedPlayers < 8; i++)
#elif NR
            for (; i < list.Count && _spawnedPlayers < 7; i++)
#endif
            {
                int mode = 0;
                if (_spawnedPlayers is 0 or 1)
                    mode = 1;
                else if (_spawnedPlayers == 2)
                    mode = 2;

                try
                {
                    SpawnOne(list[i], mode);
                    _spawnedPlayers++;
                }
                catch
                {
                }
            }

#if MRP
            return i >= 8;
#elif NR
            return i >= 4;
#endif
        }

        static internal void SpawnOne(Player pl, int mode = 0)
        {
            SpawnManager.SpawnProtect(pl);
            pl.Tag += Tag;

            pl.RoleInformation.SetNew(RoleTypeId.Tutorial, RoleChangeReason.Respawn);

            pl.Inventory.Clear();
            pl.GetAmmo();
            pl.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
            pl.Inventory.AddItem(ItemType.GunCrossvec);
            pl.Inventory.AddItem(ItemType.ArmorCombat);
            pl.Inventory.AddItem(ItemType.SCP207);
            pl.Inventory.AddItem(ItemType.SCP500);
            pl.Inventory.AddItem(ItemType.Medkit);

            if (mode == 1)
                pl.Inventory.AddItem(ItemType.SCP268);
            else if (mode == 2)
                pl.Inventory.AddItem(ItemType.SCP018);
            else
                pl.Inventory.AddItem(ItemType.SCP1853);

            pl.Inventory.AddItem(ItemType.Lantern);

            pl.HealthInformation.Hp = 125;
            pl.HealthInformation.MaxHp = 125;

            pl.Client.Broadcast(10, $"<size=80%><color=#1eb323>Вы - Длань змея</color>\n" +
                                    $"<color=#0084c2>Ваша задача - активировать</color> <color=red>SCP-008</color></size>",
                true);
            pl.UserInformation.CustomInfo = "Длань змея";
            pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo |
                                            PlayerInfoArea.PowerStatus;
        }


        [EventMethod(MapEvents.CreatePickup)]
        static void AntiScpAttack(CreatePickupEvent ev)
        {
            if (ev.Info.ItemId != ItemType.SCP1853)
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.Dead)]
        static void Dead(DeadEvent ev)
        {
            if (!ev.Target.Tag.Contains(Tag))
                return;

            ev.Target.Tag = ev.Target.Tag.Replace(Tag, "");
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev)
        {
            if (ev.Role is RoleTypeId.Tutorial)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            if (!ev.Player.Tag.Contains(Tag))
                return;

            if (ev.Role is RoleTypeId.Tutorial)
            {
                ev.Position = new Vector3(0, 302, 5);
            }
            else
            {
                ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
            }
        }

#if NR
        [EventMethod(PlayerEvents.InteractGenerator, 1)]
        static void Generator(InteractGeneratorEvent ev)
        {
            if (ev.Player is null)
                return;

            if (ev.Status is not GeneratorStatus.Activate and not GeneratorStatus.Unlock)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(ScpEvents.Scp173AddObserver, 1)]
        static void Anti173Stop(Scp173AddObserverEvent ev)
        {
            if (!ev.Target.Tag.Contains(Tag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(ScpEvents.Scp096AddTarget, 1)]
        static void Anti096Enrage(Scp096AddTargetEvent ev)
        {
            if (!ev.Target.Tag.Contains(Tag))
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.Damage, 1)]
        static void Attack(DamageEvent ev)
        {
            if (ev.Target.Tag.Contains(Tag) && ev.Attacker.Tag.Contains(Tag) &&
                ev.Target.UserInformation.Id != ev.Attacker.UserInformation.Id)
            {
                ev.Allowed = false;
                ev.Damage = 0f;
            }
            else if (ev.Target.Tag.Contains(Tag) && (ev.Attacker.GetTeam() is Team.SCPs || ev.DamageType is DamageTypes.Pocket))
            {
                ev.Allowed = false;
                ev.Damage = 0f;
            }
            else if (ev.Attacker.Tag.Contains(Tag) && ev.Target.GetTeam() is Team.SCPs)
            {
                ev.Allowed = false;
                ev.Damage = 0f;
            }
        }
#endif

        [EventMethod(RoundEvents.Waiting)]
        [EventMethod(RoundEvents.Restart)]
        static void Refresh()
        {
            _spawnedPlayers = 0;

            Timing.KillCoroutines(CoroutineTag);
        }

        [EventMethod(RoundEvents.Start)]
        static void Started()
        {
            Timing.RunCoroutine(Coroutine(), CoroutineTag);
            return;

            static IEnumerator<float> Coroutine()
            {
                yield return Timing.WaitForSeconds(Random.Range(300, 600));

                while (!Spawn())
                    yield return Timing.WaitForSeconds(Random.Range(30, 60));

                yield break;
            }

        }
    }
}