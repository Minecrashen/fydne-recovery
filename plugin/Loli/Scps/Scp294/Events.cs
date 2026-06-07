using InventorySystem;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using Loli.DataBase.Modules;
using Loli.Scps.Scp294.API;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;
using Player = Qurre.API.Controllers.Player;

namespace Loli.Scps.Scp294
{
    static internal class Events
    {
        static internal void Init()
        {
            DrinksManager.Init();
        }

        [EventMethod(RoundEvents.Waiting)]
        static public void Waiting()
        {
            var roomTransform = RoomType.EzUpstairsPcs.GetRoom().Transform;
            var scp = new API.Scp294();

            scp.Spawn(Vector3.zero, Vector3.zero);

            scp.Transform.parent = roomTransform;
            scp.Transform.localPosition = new Vector3(9.73511f * 0.725f, 5.517578f * 0.725f, 0);
            scp.Transform.localRotation = Quaternion.Euler(Vector3.up * 180);
        }

        static public void Interact(Player pl)
        {
            if (pl.RoleInformation.IsScp)
            {
                pl.Client.ShowHint("<align=left><color=#F13D3D>Недоступно для SCP</color></align>", 3);
                return;
            }

            if (pl.Inventory.ItemsCount == 8)
            {
                pl.Client.ShowHint("<align=left><color=#F13D3D>Ваш инвентарь переполнен</color></align>", 3);
                return;
            }

            if (!Data.Users.TryGetValue(pl.UserInformation.UserId, out var data))
            {
                pl.Client.ShowHint("<align=left><color=#F13D3D>Ваша статистика не найдена</color></align>", 3);
                return;
            }

            if (data.money < 5)
            {
                pl.Client.ShowHint($"<align=left><color=#F13D3D>Недостаточно монет ({data.money}/5)</color></align>", 3);
                return;
            }

            if (!DrinksManager.TryGetRandomDrink(out var drink))
            {
                pl.Client.ShowHint("<align=left><color=#F13D3D>Напиток не найден</color></align>", 3);
                return;
            }

            var serial = ItemSerialGenerator.GenerateNext();

            pl.Inventory.Base.ServerAddItem(ItemType.SCP207, ItemAddReason.Undefined, serial);
            DrinksManager.Drinks.Add(serial, drink);
            MEC.Timing.CallDelayed(0.5f, () => pl.Inventory.SelectItem(serial));

            Stats.AddMoney(pl, -5);

            pl.Client.ShowHint($"<size=30><color=#F12E7C><b>Вы купили \"{drink.Name}\" за 5 монет</b></color></size>", 7);
            pl.Client.SendConsole($"Вы купили \"{drink.Name}\"\nОписание:\n{drink.Description}", "white");
        }

        [EventMethod(PlayerEvents.DroppedItem, int.MinValue)]
        static public void ItemDrop(DroppedItemEvent ev)
        {
            if (ev.Pickup is null)
                return;

            if (DrinksManager.Drinks.ContainsKey(ev.Pickup.Info.Serial))
            {
                DrinksManager.Drinks.Remove(ev.Pickup.Info.Serial);
                Pickup.Get(ev.Pickup).Destroy();
            }
        }

        [EventMethod(PlayerEvents.UseItem)]
        static public void ItemUsing(UseItemEvent ev)
        {
            if (!ev.Allowed) return;
            if (ev.Item.TryGetDrink(out var drink))
            {
                ev.Allowed = drink.OnStartDrinking(ev.Player);
            }
        }

        [EventMethod(PlayerEvents.UsedItem)]
        static public void ItemUsed(UsedItemEvent ev)
        {
            if (ev.Item.TryGetDrink(out var drink))
            {
                drink.OnDrank(ev.Player);
            }
        }
    }
}