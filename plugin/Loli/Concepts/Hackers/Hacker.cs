using Loli.Spawns;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Concepts.Hackers;

static class Hacker
{
    internal const string Tag = " Hacker";
    internal const string GuardTag = " HackGuard";

    const RoleTypeId RoleId = RoleTypeId.ChaosRepressor;

    static internal void Spawn(Player pl, Player guard = null)
    {
        SpawnManager.SpawnProtect(pl);

        pl.Tag += Tag;
        pl.RoleInformation.SetNew(RoleId, RoleChangeReason.Respawn);
        pl.UserInformation.CustomInfo = "Хакер";
        pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;

        Timing.CallDelayed(0.3f, () =>
        {
            HintsUi.AddUi(pl);
            pl.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=red>Хакер</color> <color=green>Повстанцев Хаоса</color>\n" +
                "Ваша задача - взломать комплекс, и выкачать информацию.</color></size>", 10, true);
            // pl.MovementState.Position = new Vector3(128, 990, 28); TODO
            pl.Inventory.Clear();

#if MRP
            pl.Inventory.Ammo.Ammo556 = 200;
#elif NR
            pl.GetAmmo();
#endif

            pl.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
            pl.Inventory.AddItem(ItemType.GunE11SR);
            pl.Inventory.AddItem(ItemType.GunFRMG0);
            pl.Inventory.AddItem(ItemType.SCP500);
            pl.Inventory.AddItem(ItemType.ArmorHeavy);
            pl.Inventory.AddItem(ItemType.Radio);
            pl.Inventory.AddItem(ItemType.Lantern);
            pl.Inventory.AddItem(ItemType.GrenadeFlash);
        });

        if (guard is null)
        {
            var list = Player.List.Where(x => x.RoleInformation.Role is RoleTypeId.Spectator && !x.GamePlay.Overwatch);
            if (list.Count() > 0)
                SpawnGuard(list.ElementAt(Random.Range(0, list.Count())));
        }
        else
            SpawnGuard(guard);
    }

    internal static void SpawnGuard(Player pl)
    {
        SpawnManager.SpawnProtect(pl);

        pl.Tag += GuardTag;
        pl.RoleInformation.SetNew(RoleId, RoleChangeReason.Respawn);
        pl.UserInformation.CustomInfo = "Охранник Хакера";
        pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;

        Timing.CallDelayed(0.3f, () =>
        {
            pl.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=red>Охранник Хакера</color> <color=green>Повстанцев Хаоса</color>\n" +
                "Ваша задача - защитить <color=red>хакера</color>.</color></size>", 10, true);
            // pl.MovementState.Position = new Vector3(128, 990, 28); TODO
            pl.Inventory.Clear();

#if MRP
            pl.Inventory.Ammo.Ammo556 = 200;
            pl.Inventory.Ammo.Ammo12Gauge = 74;
#elif NR
            pl.GetAmmo();
#endif

            pl.Inventory.AddItem(ItemType.KeycardChaosInsurgency);
            pl.Inventory.AddItem(ItemType.GunE11SR);
            pl.Inventory.AddItem(ItemType.GunShotgun);
            pl.Inventory.AddItem(ItemType.SCP500);
            pl.Inventory.AddItem(ItemType.GrenadeHE);
            pl.Inventory.AddItem(ItemType.GrenadeFlash);
            pl.Inventory.AddItem(ItemType.Lantern);
            pl.Inventory.AddItem(ItemType.ArmorHeavy);
        });
    }

    // [EventMethod(PlayerEvents.Spawn)]
    static void FixPos(SpawnEvent ev)
    {
        if (ev.Role is not RoleId)
            return;

        if (ev.Player.Tag.Contains(Tag))
            ev.Position = new(128, 990, 28);
        else if (ev.Player.Tag.Contains(GuardTag))
            ev.Position = new(128, 990, 28);
    }

    [EventMethod(PlayerEvents.ChangeRole)]
    static void HackerZero(ChangeRoleEvent ev)
    {
        if (ev.Role is RoleId)
            return;

        if (ev.Player.Tag.Contains(Tag))
        {
            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
            ev.Player.HealthInformation.AhpActiveProcesses.ForEach(x => x.DecayRate = 1);
            return;
        }

        if (ev.Player.Tag.Contains(GuardTag))
        {
            ev.Player.Tag = ev.Player.Tag.Replace(GuardTag, "");
            return;
        }
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void HackerZero(SpawnEvent ev)
    {
        if (ev.Role is RoleId)
            return;

        if (ev.Player.Tag.Contains(Tag))
        {
            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
            ev.Player.HealthInformation.AhpActiveProcesses.ForEach(x => x.DecayRate = 1);
            return;
        }

        if (ev.Player.Tag.Contains(GuardTag))
        {
            ev.Player.Tag = ev.Player.Tag.Replace(GuardTag, "");
            return;
        }
    }

    [EventMethod(PlayerEvents.Dead)]
    static void HackerZero(DeadEvent ev)
    {
        if (ev.Target.Tag.Contains(Tag))
        {
            ev.Target.Tag = ev.Target.Tag.Replace(Tag, "");
            ev.Target.HealthInformation.AhpActiveProcesses.ForEach(x => x.DecayRate = 1);
            return;
        }

        if (ev.Target.Tag.Contains(GuardTag))
        {
            ev.Target.Tag = ev.Target.Tag.Replace(GuardTag, "");
            return;
        }
    }

}