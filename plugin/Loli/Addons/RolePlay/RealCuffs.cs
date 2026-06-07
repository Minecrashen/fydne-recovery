#if MRP
using InventorySystem.Disarming;
using InventorySystem.Items.Jailbird;
using Loli.Addons.Hints;
using Loli.HintsCore;
using Loli.HintsCore.Utils;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using InventorySystem.Items;
using LabApi.Features.Wrappers;
using UnityEngine;
using Utils.Networking;
using Map = Qurre.API.World.Map;
using Object = UnityEngine.Object;
using Player = Qurre.API.Controllers.Player;
using Random = UnityEngine.Random;
using Room = Qurre.API.Controllers.Room;

namespace Loli.Addons.RolePlay
{
    static class RealCuffs
    {
        const string BlockTag = "Loli.Addons.RolePlay.RealCuffs.BlockTag";
        static readonly HashSet<ushort> CuffSerials = new();

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            CuffSerials.Clear();
        }

        [EventMethod(RoundEvents.Start)]
        static void SpawnCuffs()
        {
            Room room1 = Map.Rooms.Find(x => x.Type == RoomType.HczArmory);
            Room room2 = Map.Rooms.Find(x => x.Type == RoomType.LczArmory);

            GameObject go = new("CuffSpawner");

            go.transform.parent = room1.Transform;
            go.transform.localPosition = new(0.32f, 1.2f, -1.66f);
            go.transform.localRotation = Quaternion.Euler(0, 146, 0);
            SpawnCuff(go.transform.position, go.transform.rotation);

            go.transform.localPosition = new(1.52f, 1.2f, -1.6f);
            go.transform.localRotation = Quaternion.Euler(0, 225, 0);
            SpawnCuff(go.transform.position, go.transform.rotation);

            go.transform.parent = room2.Transform;
            go.transform.localPosition = new(4, 1, 1.5f);
            go.transform.localRotation = Quaternion.Euler(0, 180, 0);
            SpawnCuff(go.transform.position, go.transform.rotation);

            go.transform.localPosition = new(4, 1, -1.5f);
            go.transform.localRotation = Quaternion.Euler(0, 0, 0);
            SpawnCuff(go.transform.position, go.transform.rotation);

            Object.Destroy(go);
        }

        [EventMethod(PlayerEvents.Damage)]
        static void UncuffSpawn(DamageEvent ev)
        {
            if (ev.LiteType is not LiteDamageTypes.Jailbird)
                return;

            if (!CuffSerials.Contains(ev.Attacker.Inventory.Hand.Serial))
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void UncuffSpawn(SpawnEvent ev)
        {
            ev.Player.Variables.Remove("RealCuffer");
            MainInfo.UpdateCuff(ev.Player, null);
        }

        [EventMethod(PlayerEvents.UnCuff)]
        static void Uncuff(UnCuffEvent ev)
        {
            ev.Target.Variables.Remove("RealCuffer");
            MainInfo.UpdateCuff(ev.Target, null);
        }

        [EventMethod(PlayerEvents.Cuff)]
        static void Cuff(CuffEvent ev)
        {
            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.JailbirdTrigger, 1)]
        static void CuffItem(JailbirdTriggerEvent ev)
        {
            if (!CuffSerials.Contains(ev.JailbirdBase.ItemSerial))
                return;

            if (ev.Message is JailbirdMessageType.ChargeLoadTriggered)
                return;

            if (ev.Message is JailbirdMessageType.ChargeFailed)
            {
                ev.Player.Client.ShowHint("<color=#ff3e0d>Вы отменили процесс связывания</color>", 10);
                return;
            }

            ev.Allowed = false;

            if (ev.Message is JailbirdMessageType.AttackTriggered)
            {
                Player target = ev.Player.GetLookingAtPlayer(3f);

                if (target is null)
                {
                    ev.Player.Client.ShowHint("<color=#ff3e0d>Человек для обыска не найден</color>", 10);
                    return;
                }

                if (!target.RoleInformation.IsHuman)
                {
                    ev.Player.Client.ShowHint("<color=#ff3e0d>Нельзя обыскать не человека</color>", 10);
                    return;
                }

                string items = string.Empty;

                foreach (var item in target.Inventory.Base.UserInventory.Items)
                    items += "- " + item.Value.ItemTypeId + "\n";

                if (string.IsNullOrEmpty(items))
                    items = "Пусто";

                ev.Player.Client.ShowHint($"<color=#4f60c9>Предметы человека '<color={target.RoleInformation.Role.GetInfoRole().Item2}>{target.UserInformation.Nickname}</color>':\n{items.Trim()}</color>", 10);

                return;
            }

            if (ev.Message is JailbirdMessageType.ChargeStarted)
            {
                Player target = ev.Player.GetLookingAtPlayer(3f);

                if (target is null)
                {
                    ev.Player.Client.ShowHint("<color=#ff3e0d>Жертва для связывания не найдена</color>", 10);
                    return;
                }

                if ((DateTime.Now - ev.Player.SpawnedTime).TotalMinutes < 1f &&
                    target.RoleInformation.Team == ev.Player.RoleInformation.Team)
                {
                    ev.Player.Client.ShowHint("<color=#ca4574>Нельзя использовать в первую минуту после спавна</color>", 10);
                    return;
                }

                if (!target.RoleInformation.IsAlive)
                {
                    ev.Player.Client.ShowHint("<color=#ff3e0d>Нельзя связать мертвого (как это произошло?)</color>", 10);
                    return;
                }

                if (target.RoleInformation.IsScp)
                {
                    switch (target.RoleInformation.Role)
                    {
                        case RoleTypeId.Scp173:
                            Scp173Rework.Process(ev.Player, target, ev.JailbirdBase);
                            return;
                        case RoleTypeId.Scp096:
                            Scp096Rework.Process(ev.Player, target, ev.JailbirdBase);
                            return;
                        default:
                            ev.Player.Client.ShowHint("<color=#ff3e0d>Нельзя связать данного SCP</color>", 10);
                            return;
                    }
                }

                if (!target.Inventory.Hand.IsEmpty)
                {
                    ev.Player.Client.ShowHint($"<color=#c9964f>У жертвы для связывания предмет в руке: {target.Inventory.Hand.Type}</color>", 10);
                    return;
                }

                target.Variables["RealCuffer"] = ev.Player;
                MainInfo.UpdateCuff(target, ev.Player);

                target.Inventory.Base.SetDisarmedStatus(null);
                target.Inventory.DropAll();

                DisarmedPlayers.Entries.Add(new DisarmedPlayers.DisarmedEntry(target.UserInformation.NetId, 0U));
                new DisarmedPlayersListMessage(DisarmedPlayers.Entries).SendToAuthenticated(0);

                return;
            }
        }

        [EventMethod(PlayerEvents.ChangeItem, int.MinValue)]
        static void ChangeItem(ChangeItemEvent ev)
        {
            if (!ev.Allowed)
                return;

            DisplayBlock block = null;

            if (ev.OldItem is not null &&
                CuffSerials.Contains(ev.OldItem.ItemSerial))
            {
                block ??= GetBlock(ev.Player);
                block.Contents.Clear();
            }

            if (ev.NewItem is not null &&
                CuffSerials.Contains(ev.NewItem.ItemSerial))
            {
                ushort newSerial = ev.NewItem.ItemSerial;
                block ??= GetBlock(ev.Player);

                block.Contents.Add(new("<b>Вы держите в руках наручники</b>", new Color32(76, 168, 243, 255), "75%", (ev) => ClearDisplay(ev, newSerial, block)));
                block.Contents.Add(new("<b>[ЛКМ] - Обыскать инвентарь человека</b>", new Color32(79, 115, 223, 255), "75%"));
                block.Contents.Add(new("<b>[ПКМ] - Связать жертву 😁</b>", new Color32(153, 79, 223, 255), "75%"));

                if (Scp173Rework.CheckString(newSerial, out string s173))
                    block.Contents.Add(new($"<b>{s173}</b>", size: "75%"));

                if (Scp096Rework.CheckString(newSerial, out string s096))
                    block.Contents.Add(new($"<b>{s096}</b>", size: "75%"));
            }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void ClearDisplay(SpawnEvent ev)
        {
            DisplayBlock block = GetBlock(ev.Player);
            block.Contents.Clear();
        }

        static void ClearDisplay(MessageEvent ev, ushort serial, DisplayBlock block)
        {
            if (ev.Player.Inventory.Hand.Serial == serial)
                return;

            block.Contents.Clear();
            ev.MessageBlock.Content = string.Empty;
        }

        static DisplayBlock GetBlock(Player pl)
        {
            if (pl.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
                return block;

            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                return null;

            block = new(new(0, -100), new(600, 400));
            display.AddBlock(block);

            pl.Variables[BlockTag] = block;

            return block;
        }

        static internal Item AddCuff(this Player pl)
        {
            ItemBase item = pl.Inventory.AddItem(ItemType.Jailbird);
            CuffSerials.Add(item.ItemSerial);
            return Item.Get(item);
        }

        static internal void AddRandomCuffAbility(this Item cuff)
        {
            bool has096 = false;
            bool has173 = false;

            foreach (Player pl in Player.List)
            {
                RoleTypeId role = pl.RoleInformation.Role;

                if (role is RoleTypeId.Scp096)
                    has096 = true;
                else if (role is RoleTypeId.Scp173)
                    has173 = true;

                if (has096 && has173)
                    break;
            }

            if (has096 && has173 || (!has096 && !has173))
            {
                int rand = Random.Range(0, 100);
                if (rand % 2 == 0)
                    cuff.GetCell173();
                else
                    cuff.GetCell096();
            }
            else if (has096)
            {
                int rand = Random.Range(0, 100);
                if (rand % 7 == 0)
                    cuff.GetCell173();
                else
                    cuff.GetCell096();
            }
            else if (has173)
            {
                int rand = Random.Range(0, 100);
                if (rand % 7 == 0)
                    cuff.GetCell096();
                else
                    cuff.GetCell173();
            }
        }

        static internal void SpawnCuff(Vector3 position, Quaternion rotation)
        {
            Pickup pick = Pickup.Create(ItemType.Jailbird, position, rotation)!;
            CuffSerials.Add(pick.Serial);
        }

        static internal bool ItIsCuff(ushort serial)
        {
            return CuffSerials.Contains(serial);
        }
    }
}
#endif