#if MRP
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Addons.RolePlay
{
    static class FacilityManager
    {
        internal const string Tag = "FacilityManagerRole";
        internal const string TagSpy = "FacilityManagerSpy";
        internal static void Spawn(Player pl)
        {
            bool Spy = Random.Range(0, 100) > 65;
            pl.Tag += Spy ? TagSpy : Tag;
            if (pl.RoleInformation.Role != RoleTypeId.Scientist) pl.RoleInformation.SetNew(RoleTypeId.Scientist, RoleChangeReason.Respawn);
            Timing.CallDelayed(0.5f, () =>
            {
                pl.HealthInformation.Hp = 200;
                pl.HealthInformation.MaxHp = 200;
                pl.Inventory.Reset(new List<ItemType>
                {
                    ItemType.KeycardFacilityManager,
                    ItemType.ArmorHeavy,
                    ItemType.GunCOM18,
                    ItemType.SCP500,
                    ItemType.Adrenaline,
                    ItemType.Medkit,
                    ItemType.Radio,
                });
                pl.MovementState.Position = RoomType.EzIntercom.GetRoom().Position + Vector3.up;
                pl.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#ff0000>Менеджер комплекса</color>\n" +
                    "Весь персонал подчиняется вашим приказам</color></size>", 10, true);

                if (Spy)
                {
                    pl.Client.ShowHint("<b><color=red>Вы состоите в группировке <color=#1ca71a>Повстанцев Хаоса</color>.</color></b>\n" +
                    "<b><size=90%><color=#db0027>Именно Вы устроили саботаж в комплексе, но, к вашему сожалению,</color></size></b>\n" +
                    "<b><color=#db0027>из-за подозрений <color=#494949>совета О5</color>, вы не смогли покинуть комплекс</color></b>\n" +
                    "<b><color=#5eb800>Ваша задача - помочь <color=red>Хакерам</color>  <color=#1ca71a>Повстанцев Хаоса</color>,</color></b>\n" +
                    "<b><color=#5eb800>и эвакуироваться из комплекса.</color></b>\n" +
                    "<b><color=#00d30c>У вас есть доступ к комнате управления</color></b>", 30);
                    Concepts.Hackers.HintsUi.AddUi(pl);
                }

                //pl.UserInformation.CustomInfo = "<color=#ff0000>Менеджер комплекса</color>";
                pl.UserInformation.CustomInfo = "Менеджер комплекса";
                pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;
                pl.Inventory.AddItem(ItemType.Ammo9x19, 20);
                Timing.CallDelayed(1f, () => pl.Inventory.AddItem(ItemType.Coin));
            });
        }


        [EventMethod(PlayerEvents.Attack, 1)]
        static void Damage(AttackEvent ev)
        {
            if (!ev.Target.Tag.Contains(TagSpy))
                return;

            if (ev.Attacker.RoleInformation.Team != Team.ChaosInsurgency)
                return;

            if (ev.LiteType != LiteDamageTypes.Gun)
                return;

            ev.Attacker.Client.ShowHint("<b><color=red>Менеджер комплекса является <color=#1ca71a>Шпионом Повстанцев Хаоса</color></color></b>");
            ev.Damage /= 50;
        }
    }
}
#endif