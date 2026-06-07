using PlayerRoles;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using Scp914;
using UnityEngine;

namespace Loli.Scps
{
    static class Better914
    {
#if MRP
        [EventMethod(ScpEvents.Scp914UpgradePlayer)]
        static void DamageUpgrade(Scp914UpgradePlayerEvent ev)
        {
            if (!ev.Player.RoleInformation.IsHuman)
                return;

            ev.Player.HealthInformation.Damage(Random.Range(50, 150), DeathTranslations.Crushed);
        }
#elif NR
        [EventMethod(ScpEvents.Scp914UpgradePlayer)]
        static void HealZombies(Scp914UpgradePlayerEvent ev)
        {
            if (ev.Setting != Scp914KnobSetting.VeryFine)
                return;

            if (ev.Player.RoleInformation.Role != RoleTypeId.Scp0492)
                return;

            if ((Random.Range(0, 100) % 20) != 0)
                return;

            ev.Allowed = false;

            ev.Player.RoleInformation.SetNew(RoleTypeId.ClassD, RoleChangeReason.RemoteAdmin, RoleSpawnFlags.None);
            ev.Player.HealthInformation.Hp = 20;
            ev.Player.HealthInformation.MaxHp = ev.Player.GetMaxHp();
        }
#endif

        [EventMethod(ScpEvents.Scp914UpgradePlayer)]
        static void AntiGuns(Scp914UpgradePlayerEvent ev)
        {
            ev.Inventory.RemoveWhere(x => x.Category is ItemCategory.Firearm);
        }

        [EventMethod(ScpEvents.Scp914UpgradePickup)]
        static void AntiGuns(Scp914UpgradePickupEvent ev)
        {
            if (ev.Pickup.NetworkInfo.ItemId.GetCategory() is ItemCategory.Firearm)
                ev.Allowed = false;
        }
    }
}