using InventorySystem.Items;
using Loli.Spawns;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons
{
    static class FastReconnect
    {
        static readonly Dictionary<string, SafeData> UserDataArray = new();

        static FastReconnect()
        {

            CommandsSystem.RegisterConsole("rc", Command);
            static void Command(GameConsoleCommandEvent ev)
            {
                ev.Allowed = false;
                ev.Player.Client.SendConsole("Больше недоступен.", "red");
            }
        }

        [EventMethod(RoundEvents.Waiting)]
        [EventMethod(RoundEvents.Start)]
        static void Refresh()
        {
            UserDataArray.Clear();
        }

        [EventMethod(PlayerEvents.Join)]
        static void Join(JoinEvent ev)
        {
            Player pl = ev.Player;

            if (!UserDataArray.TryGetValue(pl.UserInformation.UserId, out SafeData data))
                return;

            UserDataArray.Remove(pl.UserInformation.UserId);

            if ((data.Expires - DateTime.Now).TotalSeconds < 0)
                return;

            pl.RoleInformation.SetNew(data.Role, RoleChangeReason.Respawn);

            Timing.CallDelayed(1f, () =>
            {
                if (pl.Disconnected)
                    return;

                pl.Tag = data.Tag;
                pl.UserInformation.CustomInfo = data.Info;
                pl.UserInformation.InfoToShow = data.InfoToShow;

                pl.HealthInformation.Hp = data.Hp;
                pl.HealthInformation.MaxHp = data.MaxHp;
                pl.HealthInformation.Ahp = data.Ahp;
                pl.HealthInformation.MaxAhp = data.MaxAhp;
                pl.HealthInformation.Stamina = data.Stamina;

                pl.MovementState.Position = data.Position;
                pl.MovementState.Rotation = data.Rotation;
                pl.MovementState.Scale = data.Scale;

                pl.Inventory.Clear();

                pl.Inventory.Ammo.Ammo12Gauge = data.Ammos.Ammo12Gauge;
                pl.Inventory.Ammo.Ammo556 = data.Ammos.Ammo556;
                pl.Inventory.Ammo.Ammo44Cal = data.Ammos.Ammo44Cal;
                pl.Inventory.Ammo.Ammo762 = data.Ammos.Ammo762;
                pl.Inventory.Ammo.Ammo9 = data.Ammos.Ammo9;

                while (data.Items.Count > 0)
                {
                    var item = data.Items.First();
                    data.Items.Remove(item);

#if MRP
                    /*
                    if (item.ItemSerial == data.ItemInHand)
                    {
                        pl.Inventory.Base.NetworkCurItem = new(new_item.Type, new_item.Serial);
                    }
                    */
#endif
                }
            });
        }


        static internal void Process(Player pl)
        {
            if (Round.Waiting)
                return;

            if ((DateTime.Now - pl.JoinedTime).TotalSeconds < 2)
                return;

            if (pl.GamePlay.Lift is not null)
                return;

            List<ItemBase> items = new();
            foreach (var item in pl.Inventory.Base.UserInventory.Items)
            {
                try
                {
                    items.Add(item.Value);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }

            SafeData data = new()
            {
                Tag = pl.Tag.Replace(SpawnManager.SpawnProtectTag, ""),
                Info = pl.UserInformation.CustomInfo,
                InfoToShow = pl.UserInformation.InfoToShow,

                Hp = pl.HealthInformation.Hp,
                MaxHp = pl.HealthInformation.MaxHp,
                Ahp = pl.HealthInformation.Ahp,
                MaxAhp = pl.HealthInformation.MaxAhp,
                Stamina = pl.HealthInformation.Stamina,

                Role = pl.RoleInformation.Role,

                Ammos = new()
                {
                    Ammo12Gauge = pl.Inventory.Ammo.Ammo12Gauge,
                    Ammo556 = pl.Inventory.Ammo.Ammo556,
                    Ammo44Cal = pl.Inventory.Ammo.Ammo44Cal,
                    Ammo762 = pl.Inventory.Ammo.Ammo762,
                    Ammo9 = pl.Inventory.Ammo.Ammo9,
                },
                Items = items,
                ItemInHand = 0,

                Position = pl.MovementState.Position,
                Rotation = pl.MovementState.Rotation,
                Scale = pl.MovementState.Scale,

                Expires = DateTime.Now.AddMinutes(1),
            };

            try { data.ItemInHand = pl.Inventory.Base.CurInstance.ItemSerial; } catch { }

            if (UserDataArray.ContainsKey(pl.UserInformation.UserId))
                UserDataArray.Remove(pl.UserInformation.UserId);

            UserDataArray.Add(pl.UserInformation.UserId, data);

            pl.Inventory.Clear();
            pl.MovementState.Position = Vector3.zero;
            pl.RoleInformation.Role = RoleTypeId.Spectator;

            pl.Client.Reconnect();
        }

        class SafeData
        {
            internal string Tag { get; set; }
            internal string Info { get; set; }
            internal PlayerInfoArea InfoToShow { get; set; }

            internal float Hp { get; set; }
            internal float MaxHp { get; set; }
            internal float Ahp { get; set; }
            internal float MaxAhp { get; set; }
            internal float Stamina { get; set; }

            internal RoleTypeId Role { get; set; }

            internal SafeAmmos Ammos { get; set; }
            internal List<ItemBase> Items { get; set; }
            internal ushort ItemInHand { get; set; }

            internal Vector3 Position { get; set; }
            internal Vector3 Rotation { get; set; }
            internal Vector3 Scale { get; set; }

            internal DateTime Expires { get; set; }
        }

        class SafeAmmos
        {
            internal ushort Ammo12Gauge { get; set; }
            internal ushort Ammo556 { get; set; }
            internal ushort Ammo44Cal { get; set; }
            internal ushort Ammo762 { get; set; }
            internal ushort Ammo9 { get; set; }
        }
    }
}