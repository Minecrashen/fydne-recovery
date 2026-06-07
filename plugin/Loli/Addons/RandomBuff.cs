#if NR
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Addons
{
    static class RandomBuff
    {
        static int id = 0;

        [EventMethod(RoundEvents.Waiting)]
        static internal void Enable()
        {
            id = Random.Range(0, 15);
        }

        [EventMethod(PlayerEvents.Join)]
        [EventMethod(PlayerEvents.Spawn)]
        static internal void SendJoin(IBaseEvent ev)
        {
            Player pl = null;
            if (ev is JoinEvent ev1)
                pl = ev1.Player;
            else if (ev is SpawnEvent ev2)
            {
                if (ev2.Role == PlayerRoles.RoleTypeId.Spectator)
                    return;
                pl = ev2.Player;
            }
            else
                return;

            if (id == 0)
                return;

            switch (id)
            {
                case 1:
                    {
                        Broadcast("Исцеляющий <color=#ff9900>D-class</color>", "💉 D-class получает дополнительные предметы для исцеления при спавне 💉");
                        break;
                    }
                case 2:
                    {
                        Broadcast("Разрушающий <color=#006aff>МОГ</color>", "⚡ Капитан МОГ может появиться с моллекулятором частиц ⚡");
                        break;
                    }
                case 3:
                    {
                        Broadcast("Опасная <color=#727c8a>Охрана</color>", "⬆️ Охрана комплекса обладает улучшенным инвентарем ⬆️");
                        break;
                    }
                case 4:
                    {
                        Broadcast("Не сегодня, SCP", "❤️ Урон от и по SCP снижен в 1.5 раза ❤️");
                        break;
                    }/*
                case 5:
                    {
                        Broadcast("В активном поиске", "🔎 ПХ, МОГ и Охрана спавнятся только с карточкой, аптечкой и пистолетом");
                        break;
                    }*/
            }
            void Broadcast(string name, string message)
            {
                MEC.Timing.CallDelayed(5f, () =>
                {
                    pl.Client.Broadcast("<b><color=#d138ff>🚀 Случайный бафф 🚀</color></b>\n" +
                        $"<size=70%><color=#00edd5>❓В этом раунде действует бафф - <color=red>{name}</color>❓</color>\n" +
                        $"<color=#00e07f>{message}</color></size>", 30);
                });
            }
        }

        static internal void Spawn(SpawnEvent ev)
        {
            switch (id)
            {
                case 1:
                    {
                        if (ev.Role != PlayerRoles.RoleTypeId.ClassD)
                            break;

                        ev.Player.Inventory.AddItem(ItemType.Painkillers);
                        if (Random.Range(0, 100) < 30)
                            ev.Player.Inventory.AddItem(ItemType.Medkit);
                        if (Random.Range(0, 100) < 25)
                            ev.Player.Inventory.AddItem(ItemType.Adrenaline);

                        break;
                    }
                case 2:
                    {
                        if (ev.Role != PlayerRoles.RoleTypeId.NtfCaptain)
                            break;

                        if (Random.Range(0, 100) < 25)
                        {
                            try
                            {
                                if (ev.Player.Inventory.ItemsCount >= 8)
                                {
                                    if (ev.Player.Inventory.Items.TryFind(out var item, x => x.Value.ItemTypeId == ItemType.GunRevolver))
                                    {
                                        ev.Player.Inventory.RemoveItem(item.Value);
                                    }
                                    else
                                    {
                                        if (ev.Player.Inventory.Items.TryFind(out var item2, x => x.Value.Category != ItemCategory.Keycard))
                                        {
                                            ev.Player.Inventory.RemoveItem(item2.Value);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }
                            }
                            catch { }
                            ev.Player.Inventory.AddItem(ItemType.ParticleDisruptor);
                        }

                        break;
                    }
                case 3:
                    {
                        if (ev.Role != PlayerRoles.RoleTypeId.FacilityGuard)
                            break;

                        ev.Player.Inventory.Reset(new ItemType[]
                        {
                            ItemType.KeycardMTFPrivate,
                            (Random.Range(0, 100) < 50) ? ItemType.GunE11SR :
                            (Random.Range(0, 100) < 50) ? ItemType.GunAK :
                            ((Random.Range(0, 100) < 50) ? ItemType.GunShotgun : ItemType.GunLogicer),
                            ItemType.GunCOM18,
                            ItemType.SCP500,
                            ItemType.Medkit,
                            ItemType.Flashlight,
                            ItemType.Radio,
                            ItemType.ArmorHeavy
                        });
                        ev.Player.GetAmmo();

                        break;
                    }/*
                case 5:
                    {
                        MEC.Timing.CallDelayed(0.5f, () =>
                        {
                            if (ev.Role != ev.Player.RoleInformation.Role)
                                return;

                            switch (ev.Role)
                            {
                                case PlayerRoles.RoleTypeId.ChaosConscript or PlayerRoles.RoleTypeId.ChaosMarauder or
                                PlayerRoles.RoleTypeId.ChaosRepressor or PlayerRoles.RoleTypeId.ChaosRifleman:
                                    {
                                        AddItems(ev.Player);
                                        ev.Player.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
                                        break;
                                    }
                                case PlayerRoles.RoleTypeId.NtfCaptain:
                                    {
                                        AddItems(ev.Player);
                                        ev.Player.Inventory.AddItem(ItemType.KeycardNTFCommander);
                                        break;
                                    }
                                case PlayerRoles.RoleTypeId.NtfSergeant:
                                    {
                                        AddItems(ev.Player);
                                        ev.Player.Inventory.AddItem(ItemType.KeycardNTFLieutenant);
                                        break;
                                    }
                                case PlayerRoles.RoleTypeId.NtfPrivate:
                                    {
                                        AddItems(ev.Player);
                                        ev.Player.Inventory.AddItem(ItemType.KeycardNTFOfficer);
                                        break;
                                    }
                                case PlayerRoles.RoleTypeId.FacilityGuard:
                                    {
                                        AddItems(ev.Player);
                                        ev.Player.Inventory.AddItem(ItemType.KeycardGuard);
                                        break;
                                    }
                            }
                        });

                        static void AddItems(Player pl)
                        {
                            pl.Inventory.Reset(new ItemType[]
                            {
                                ItemType.Medkit,
                                ItemType.Painkillers,
                                (Random.Range(0, 100) < 33) ? ItemType.GunCOM15 :
                                (Random.Range(0, 100) < 33) ? ItemType.GunCOM18 : ItemType.GunRevolver,
                                ItemType.Radio,
                                ItemType.ArmorLight,
                            });

                            pl.GetAmmo();

                            if (Random.Range(0, 100) < 30)
                                pl.Inventory.AddItem(ItemType.Medkit);
                            else
                                pl.Inventory.AddItem(ItemType.Flashlight);

                            if (Random.Range(0, 100) < 40)
                                pl.Inventory.AddItem(ItemType.Adrenaline);
                            else
                                pl.Inventory.AddItem(ItemType.Coin);
                        }

                        if (ev.Role is PlayerRoles.RoleTypeId.ChaosConscript or PlayerRoles.RoleTypeId.ChaosMarauder or
                            PlayerRoles.RoleTypeId.ChaosRepressor or PlayerRoles.RoleTypeId.ChaosRifleman or
                            PlayerRoles.RoleTypeId.NtfCaptain or PlayerRoles.RoleTypeId.NtfSergeant or
                            PlayerRoles.RoleTypeId.NtfSpecialist or PlayerRoles.RoleTypeId.NtfPrivate or
                            PlayerRoles.RoleTypeId.FacilityGuard)
                            break;

                        ev.Player.Inventory.Reset(new ItemType[]
                        {
                            ItemType.KeycardNTFOfficer,
                            (Random.Range(0, 100) < 50) ? ItemType.GunE11SR :
                            (Random.Range(0, 100) < 50) ? ItemType.GunAK :
                            ((Random.Range(0, 100) < 50) ? ItemType.GunShotgun : ItemType.GunLogicer),
                            ItemType.GunCOM18,
                            ItemType.SCP500,
                            ItemType.Medkit,
                            ItemType.Flashlight,
                            ItemType.Radio,
                            ItemType.ArmorHeavy
                        });
                        ev.Player.GetAmmo();

                        break;
                    }*/
            }
        }

        [EventMethod(PlayerEvents.Attack, -3)]
        static internal void Attack(AttackEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (id != 4)
                return;

            if (0 >= ev.Damage)
                return;

            if (ev.Target.RoleInformation.Team != PlayerRoles.Team.SCPs &&
                ev.Attacker.RoleInformation.Team != PlayerRoles.Team.SCPs)
                return;

            ev.Damage /= 1.5f;
        }
    }
}
#endif