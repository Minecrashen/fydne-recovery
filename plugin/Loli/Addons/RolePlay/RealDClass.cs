#if MRP
using Loli.Addons.Hints;
using MEC;
using PlayerRoles;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons.RolePlay
{
    static class RealDClass
    {
        static int janitors = 0;

        [EventMethod(RoundEvents.Waiting)]
        static void ClearJanitors() => janitors = 0;

        [EventMethod(PlayerEvents.PickupItem, -1)]
        static void PickReal(PickupItemEvent ev)
        {
            if (ev.Player.RoleInformation.Role != RoleTypeId.ClassD)
                return;

            if (ev.Pickup.Info.ItemId == ItemType.MicroHID)
            {
                ev.Allowed = false;
                ev.Player.Client.ShowHint("<color=red><b>25 кг 🌚</b></color>", 10);
                return;
            }
            if (ev.Player.Tag.Contains("DSkinny") && (ev.Pickup.Info.ItemId == ItemType.GunAK || ev.Pickup.Info.ItemId == ItemType.GunE11SR ||
                ev.Pickup.Info.ItemId == ItemType.GunLogicer || ev.Pickup.Info.ItemId == ItemType.GunShotgun || ev.Pickup.Info.ItemId == ItemType.MicroHID))
            {
                ev.Allowed = false;
                ev.Player.Client.ShowHint("<color=red><b>Данное оружие слишком тяжелое для вас</b></color>", 10);
            }
        }

        [EventMethod(PlayerEvents.Dead)]
        static void FixTagsEvent(DeadEvent ev)
        {
            ev.Target.UserInformation.DisplayName = "";
            FixTags(ev.Target);
        }

        [EventMethod(PlayerEvents.Escape, -1)]
        static void FixTagsEvent(EscapeEvent ev)
        {
            if (!ev.Allowed)
                return;

            FixTags(ev.Player);
        }

        [EventMethod(PlayerEvents.ChangeRole, -1)]
        static void FixTagsEvent(ChangeRoleEvent ev)
        {
            if (!ev.Allowed)
                return;

            FixTags(ev.Player);
        }

        [EventMethod(PlayerEvents.Spawn, -1)]
        static void RealName(SpawnEvent ev)
        {
            if (ev.Role == RoleTypeId.ClassD)
            {
                int number = Random.Range(1000, 9999);
                ev.Player.UserInformation.DisplayName = $"D-class {number}";
                ev.Player.Variables["RPName"] = $"#{number}";
                ev.Player.UserInformation.CustomInfo = $"Прозвище: {ev.Player.UserInformation.Nickname.Replace("<", "").Replace(">", "")}";
                ev.Player.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo |
                    PlayerInfoArea.Role | PlayerInfoArea.PowerStatus;
                RealСharacter(ev);
                MainInfo.UpdateName(ev.Player, $"#{number}");
            }
            else
            {
                ev.Player.UserInformation.DisplayName = "";
                FixTags(ev.Player);
            }
        }

        static void RealСharacter(SpawnEvent ev)
        {
            var random = Random.Range(1, 100);
            if (random < 20 && Round.ElapsedTime.TotalMinutes < 1 && janitors < 5)
            {
                janitors++;
                //ev.Position = Respawn.GetPosition(RoleTypeId.Scientist) + Vector3.up;
                Timing.CallDelayed(0.5f, () =>
                {
                    if (!ev.Player.Inventory.Base.UserInventory.Items.Any(x => x.Value.ItemTypeId == ItemType.KeycardJanitor))
                        ev.Player.Inventory.AddItem(ItemType.KeycardJanitor);
                    ev.Player.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#aebaf1>Уборщик</color> <color=#ff9900>D-класса</color></color></size>", 10, true);
                    ev.Player.UserInformation.CustomInfo += "\nРоль: Уборщик";
                });
            }
            else if (random < 20)
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    ev.Player.MovementState.Scale = new Vector3(0.85f, 1, 0.85f);
                    ev.Player.HealthInformation.Hp = 90;
                    ev.Player.HealthInformation.MaxHp = 90;
                    ev.Player.Tag += "DSkinny";
                    ev.Player.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#ff5000>Слабый</color> <color=#ff9900>D-класс</color></color></size>", 10, true);
                });
            }
            else if (random < 40)
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    ev.Player.MovementState.Scale = new Vector3(1.1f, 0.9f, 1.1f);
                    ev.Player.HealthInformation.Hp = 100;
                    ev.Player.HealthInformation.MaxHp = 100;
                    ev.Player.Tag += "DThick";
                    ev.Player.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#ffbf00>Толстый</color> <color=#ff9900>D-класс</color></color></size>", 10, true);
                });
            }
            else if (random < 60)
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    float scale = Random.Range(82, 90);
                    ev.Player.MovementState.Scale = new Vector3(scale / 100, scale / 100, scale / 100);
                    ev.Player.HealthInformation.Hp = 90;
                    ev.Player.HealthInformation.MaxHp = 90;
                    ev.Player.Tag += "DSkinny";
                    ev.Player.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#60ff00>Маленький</color> <color=#ff9900>D-класс</color></color></size>", 10, true);
                });
            }
            else
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    float scale = Random.Range(93, 115);
                    ev.Player.MovementState.Scale = new Vector3(scale / 100, scale / 100, scale / 100);
                });
            }
        }

        static void FixTags(Player pl)
        {
            if (pl.Tag.Contains("DSkinny")) pl.Tag = pl.Tag.Replace("DSkinny", "");
            if (pl.Tag.Contains("DThick")) pl.Tag = pl.Tag.Replace("DThick", "");
        }
    }
}
#endif