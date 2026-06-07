#if MRP
using Loli.DataBase.Modules;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons.RolePlay
{
    static class Scientists
    {
        static void SpawnMajor(Player major)
        {
            major.Tag += "MajorScientist";
            if (major.RoleInformation.Role != RoleTypeId.Scientist)
                major.RoleInformation.SetNew(RoleTypeId.Scientist, RoleChangeReason.Respawn);
            Timing.CallDelayed(0.5f, () =>
            {
                major.Inventory.Clear();
                major.GetAmmo();
                major.Inventory.AddItem(ItemType.KeycardResearchCoordinator);
                major.Inventory.AddItem(ItemType.Medkit);
                major.Inventory.AddItem(ItemType.SCP500);
                major.Inventory.AddItem(ItemType.Radio);
                major.Inventory.AddItem(ItemType.Flashlight);
                major.Inventory.AddItem(ItemType.ArmorLight);
                major.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#fdffbb>Координатор исследований</color>\n" +
                    "Научный Персонал подчиняется вашим приказам</color></size>", 10, true);
                //major.UserInformation.CustomInfo = "<color=#fdffbb>Координатор исследований</color>";
                major.UserInformation.CustomInfo = "Координатор исследований";
                major.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;
            });
        }

        static void SpawnRandom(Player pl)
        {
            var random = Random.Range(1, 100);
            if (pl.Tag.Contains(FacilityManager.Tag)) return;
            if (pl.Tag.Contains(FacilityManager.TagSpy)) return;
            if (random < 15)
            {
                pl.Inventory.AddItem(ItemType.KeycardZoneManager);
                pl.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#217879>Менеджер зон содержания</color>\n" +
                    "Подчиняйтесь приказам <color=#fdffbb>координатора исследований</color>,\nлибо эвакуируйтесь самостоятельно</color></size>", 10, true);
                //pl.NicknameSync.Network_customPlayerInfoString = "<color=#217879>Менеджер зон содержания</color>";
                pl.UserInformation.CustomInfo = "Менеджер зон содержания";
                pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;
            }
            else
            {
                pl.Inventory.AddItem(ItemType.KeycardScientist);
                pl.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#e2e26d>Научный сотрудник</color>\n" +
                    "Подчиняйтесь приказам <color=#fdffbb>координатора исследований</color>,\nлибо эвакуируйтесь самостоятельно</color></size>", 10, true);
                pl.UserInformation.CustomInfo = "Научный сотрудник";
                pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;
            }
            float scale = Random.Range(90, 110);
            pl.MovementState.Scale = new Vector3(scale / 100, scale / 100, scale / 100);
        }


        [EventMethod(RoundEvents.Start, -2)]
        static void RoundStart()
        {
            List<Player> list = Player.List.Where(x => x.RoleInformation.Role == RoleTypeId.Scientist).ToList();

            if (list.Count == 0)
                return;

            list.Shuffle();

            if (list.Count > 1)
            {
                Player major = list.First();
                SpawnMajor(major);
                list.Remove(major);
            }

            if (list.Count > 1)
            {
                if (list.TryFind(out Player fm, x => Data.Users.TryGetValue(x.UserInformation.UserId, out var data) && data.lvl > 6))
                {
                    FacilityManager.Spawn(fm);
                    list.Remove(fm);
                }
            }

            foreach (Player pl in list)
                SpawnRandom(pl);
        }

        [EventMethod(PlayerEvents.Spawn, -2)]
        static void Spawn(SpawnEvent ev)
        {
            if (ev.Role != RoleTypeId.Scientist) return;
            if (ev.Player.Tag.Contains("MajorScientist")) return;
            if (ev.Player.Tag.Contains(FacilityManager.Tag)) return;
            if (ev.Player.Tag.Contains(FacilityManager.TagSpy)) return;
            Timing.CallDelayed(0.5f, () => SpawnRandom(ev.Player));
        }

        [EventMethod(PlayerEvents.Spawn, 1)]
        static void FixTags(SpawnEvent ev)
        {
            if (ev.Role == RoleTypeId.Scientist) return;
            if (ev.Player.Tag.Contains("MajorScientist")) ev.Player.Tag = ev.Player.Tag.Replace("MajorScientist", "");
            if (ev.Player.Tag.Contains(FacilityManager.Tag)) ev.Player.Tag = ev.Player.Tag.Replace(FacilityManager.Tag, "");
            if (ev.Player.Tag.Contains(FacilityManager.TagSpy)) ev.Player.Tag = ev.Player.Tag.Replace(FacilityManager.TagSpy, "");
        }

        [EventMethod(PlayerEvents.ChangeRole, -2)]
        static void FixTags(ChangeRoleEvent ev)
        {
            if (!ev.Allowed) return;
            if (ev.Role == RoleTypeId.Scientist) return;
            if (ev.Player.Tag.Contains("MajorScientist")) ev.Player.Tag = ev.Player.Tag.Replace("MajorScientist", "");
            if (ev.Player.Tag.Contains(FacilityManager.Tag)) ev.Player.Tag = ev.Player.Tag.Replace(FacilityManager.Tag, "");
            if (ev.Player.Tag.Contains(FacilityManager.TagSpy)) ev.Player.Tag = ev.Player.Tag.Replace(FacilityManager.TagSpy, "");
        }

        [EventMethod(PlayerEvents.Dead)]
        static void FixTags(DeadEvent ev)
        {
            if (ev.Target.Tag.Contains("MajorScientist")) ev.Target.Tag = ev.Target.Tag.Replace("MajorScientist", "");
            if (ev.Target.Tag.Contains(FacilityManager.Tag)) ev.Target.Tag = ev.Target.Tag.Replace(FacilityManager.Tag, "");
            if (ev.Target.Tag.Contains(FacilityManager.TagSpy)) ev.Target.Tag = ev.Target.Tag.Replace(FacilityManager.TagSpy, "");
        }



        [EventMethod(PlayerEvents.PickupItem)]
        static void BetterHid(PickupItemEvent ev)
        {
            if (!ev.Allowed) return;
            if (ev.Pickup.Info.ItemId != ItemType.MicroHID) return;
            if (ev.Player.RoleInformation.Role != RoleTypeId.Scientist)
            {
                Timing.CallDelayed(0.5f, () => ev.Player.Inventory.SelectItem(ev.Pickup.Info.Serial));
                return;
            }
            ev.Allowed = false;
            if (ev.Player.Tag.Contains(" DontHid")) return;
            ev.Allowed = true;
            Timing.CallDelayed(0.5f, () => ev.Player.Inventory.SelectItem(ev.Pickup.Info.Serial));
            Timing.RunCoroutine(PostFix(), $"BetterHid-{ev.Player.UserInformation.UserId}");
            IEnumerator<float> PostFix()
            {
                var round = Round.CurrentRound;
                yield return Timing.WaitForSeconds(ev.Player.MovementState.Scale.y * 60);
                if (round != Round.CurrentRound || ev.Player == null || ev.Player.Inventory.Base.CurInstance == null ||
                    ev.Player.Inventory.Base.CurInstance.ItemTypeId != ItemType.MicroHID) yield break;
                ev.Player.Inventory.DropItem(ev.Player.Inventory.Base.CurInstance?.ItemSerial ?? 0);
                ev.Player.Client.ShowHint("<b><color=red>Отдыхайте...</color></b>", 10);
                ev.Player.HealthInformation.Stamina = 0;
                ev.Player.Tag += " DontHid";
                yield return Timing.WaitForSeconds(10);
                if (round != Round.CurrentRound || ev.Player == null) yield break;
                ev.Player.Tag = ev.Player.Tag.Replace(" DontHid", "");
                yield break;
            }
        }

        [EventMethod(PlayerEvents.DropItem)]
        static void BetterHid(DropItemEvent ev)
        {
            if (ev.Item.ItemTypeId != ItemType.MicroHID) return;
            if (ev.Player.RoleInformation.Role != RoleTypeId.Scientist) return;
            ev.Player.Tag += " DontHid";
            Timing.RunCoroutine(PostFix());
            Timing.KillCoroutines($"BetterHid-{ev.Player.UserInformation.UserId}");
            IEnumerator<float> PostFix()
            {
                var round = Round.CurrentRound;
                yield return Timing.WaitForSeconds(10);
                if (round != Round.CurrentRound || ev.Player == null) yield break;
                ev.Player.Tag = ev.Player.Tag.Replace(" DontHid", "");
                yield break;
            }
        }

        [EventMethod(PlayerEvents.ChangeItem, -2)]
        static void BetterHid(ChangeItemEvent ev)
        {
            if (!ev.Allowed) return;
            if (ev.OldItem == null) return;
            if (ev.OldItem.ItemTypeId != ItemType.MicroHID) return;
            //if (ev.Player.Tag.Contains("DontHid")) return;
            //if (ev.Player.RoleInformation.Role != RoleTypeId.Scientist) return;
            ev.Allowed = false;
        }
    }
}
#endif