using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Armor;
using Loli.Addons.Hints;
using Loli.HintsCore;
using MEC;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Addons
{
    static class RealisticArmory
    {
        const string MessageTag = "Loli.Addons.RealisticArmory.MessageBlock";
        static readonly Dictionary<string, CustomArmor> ArmorCache = new();

        static string GetColorByAlive(byte alive)
        {
            return alive switch
            {
                < 20 => "#ff3636",
                < 40 => "#ff7036",
                < 60 => "#ffb136",
                _ => "#5cff36"
            };
        }

        static CustomArmor GetArmor(Player pl)
        {
            try
            {
                foreach (var item in pl.Inventory.Base.UserInventory.Items)
                {
                    try
                    {
                        if (item.Value is BodyArmor armor)
                            return GetArmorUnsafe(item.Key, item.Value.ItemTypeId);
                    }
                    catch { }
                }
            }
            catch { }

            return null;
        }

        static CustomArmor GetArmorUnsafe(ushort serial, ItemType type)
        {
            string search = $"{serial}{type}";

            if (ArmorCache.ContainsKey(search))
                return ArmorCache[search];

            CustomArmor customArmor = new()
            {
                MaxHp = GetMaxArmorHp(type)
            };
            customArmor.Hp = customArmor.MaxHp;

            ArmorCache.Add(search, customArmor);

            return customArmor;
        }

        static float GetMaxArmorHp(ItemType type)
        {
            return type switch
            {
                ItemType.ArmorLight => 150,
                ItemType.ArmorCombat => 250,
                ItemType.ArmorHeavy => 400,
                _ => 100,
            };
        }

        static internal void DamageArmor(this Player pl, float damage)
        {
            DamageArmor(pl.ReferenceHub, damage);
        }

        static void DamageArmor(ReferenceHub hub, float damage)
        {
            Player pl = hub.GetPlayer();

            if (pl is null)
                return;

            CustomArmor armor = GetArmor(pl);

            if (armor is null)
                return;

            armor.Hp -= damage;

            armor.UpdateText();

            if (armor.Hp <= 0)
            {
                foreach (var item in pl.Inventory.Base.UserInventory.Items)
                {
                    try
                    {
                        if (item.Value is BodyArmor bodyArmor)
                        {
                            pl.Inventory.RemoveItem(item.Key, item.Value.PickupDropModel);
                            return;
                        }
                    }
                    catch { }
                }
            }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            MainInfo.UpdateMessage(ev.Player, null, true, MessageTag);

            Timing.CallDelayed(2f, () =>
            {
                CustomArmor armor = GetArmor(ev.Player);

                if (armor is null)
                    return;

                MainInfo.UpdateMessage(ev.Player, armor.MessageBlock, true, MessageTag);
            });
        }

        class CustomArmor
        {
            internal MessageBlock MessageBlock { get; }
            internal float MaxHp { get; set; }
            internal float Hp { get; set; }
            internal byte Alive => (byte)(100 - ((MaxHp - Hp) / MaxHp * 100));

            internal void UpdateText()
            {
                MessageBlock.Content = $"🎽 <b>Броня: <color={GetColorByAlive(Alive)}>{Alive}%</color></b>";
            }

            internal CustomArmor()
            {
                Hp = 100;
                MaxHp = 100;
                MessageBlock = new(string.Empty, new Color32(255, 112, 115, 255), "60%");

                UpdateText();
            }
        }


        [HarmonyPatch(typeof(ItemBase), nameof(ItemBase.OnAdded))]
        static class PatchItemAdd
        {
            [HarmonyPostfix]
            static void Call(ItemBase __instance)
            {
                if (__instance.IsLocalPlayer)
                    return;

                if (__instance.ItemTypeId is not ItemType.ArmorCombat
                    and not ItemType.ArmorHeavy
                    and not ItemType.ArmorLight)
                    return;

                CustomArmor armor = GetArmorUnsafe(__instance.ItemSerial, __instance.ItemTypeId);
                MainInfo.UpdateMessage(__instance.Owner.GetPlayer(), armor.MessageBlock, true, MessageTag);
            }
        }

        [HarmonyPatch(typeof(ItemBase), nameof(ItemBase.OnRemoved))]
        static class PatchItemRemove
        {
            [HarmonyPostfix]
            static void Call(ItemBase __instance)
            {
                if (__instance.IsLocalPlayer)
                    return;

                if (__instance.ItemTypeId is not ItemType.ArmorCombat
                    and not ItemType.ArmorHeavy
                    and not ItemType.ArmorLight)
                    return;

                CustomArmor armor = GetArmorUnsafe(__instance.ItemSerial, __instance.ItemTypeId);
                MainInfo.UpdateMessage(__instance.Owner.GetPlayer(), armor.MessageBlock, false, MessageTag);
            }
        }

        [HarmonyPatch(typeof(FirearmDamageHandler), nameof(FirearmDamageHandler.ProcessDamage))]
        static class PatchArmory
        {
            [HarmonyTranspiler]
            static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
            {
                List<CodeInstruction> list = new(instructions);
                LocalBuilder armorEfficacy = generator.DeclareLocal(typeof(int));

                list.InsertRange(0,
                [
                    new CodeInstruction(OpCodes.Nop).MoveLabelsFrom(list[0]), // move labels

                    new(OpCodes.Ldc_I4_0), // 0 [int]
                    new(OpCodes.Stloc_S, armorEfficacy.LocalIndex), // int armorEfficacy = 0;
                ]);

                int index = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_3) + 1;

                list.InsertRange(index,
                [
                    new CodeInstruction(OpCodes.Nop).MoveLabelsFrom(list[index]), // move labels

                    new(OpCodes.Ldloc_2), // armorEfficacy [int]
                    new(OpCodes.Stloc_S, armorEfficacy.LocalIndex), // int armorEfficacy = <code>.armorEfficacy;
                ]);

                list.InsertRange(list.Count - 1,
                [
                    new CodeInstruction(OpCodes.Ldarg_1).MoveLabelsFrom(list[list.Count - 2]), // ply [ReferenceHub]
                    new(OpCodes.Ldarg_0), // this [FirearmDamageHandler]
                    new(OpCodes.Ldloc_S, armorEfficacy.LocalIndex), // armorEfficacy [int]
                    new(OpCodes.Call, AccessTools.Method(typeof(PatchArmory), nameof(PatchArmory.Invoke))),
                ]);

                return list.AsEnumerable();
            }

            static void Invoke(ReferenceHub hub, FirearmDamageHandler handler, int armorEfficacy)
            {
                if (armorEfficacy == 0 ||
                    handler.Damage == 0)
                    return;

                if (handler.IsFriendlyFire && !Server.FriendlyFire)
                    return;

                float armorNegativeEffective = (float)(100 - armorEfficacy) / 100;
                float penetration = handler._penetration / armorNegativeEffective;
                float damage = handler.Damage / 10 * penetration;

                if (damage < 0)
                    return;

                DamageArmor(hub, damage);
            }
        }
    }
}