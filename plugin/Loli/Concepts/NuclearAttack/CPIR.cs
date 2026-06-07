using MEC;
using PlayerRoles;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using Qurre.API.Controllers;
using UnityEngine;

#if MRP
using Loli.Addons.RolePlay;
#endif

namespace Loli.Concepts.NuclearAttack
{
    static class CPIR
    {
        internal const string Tag = " CPIRPlayer";

        internal static bool AllowAttack { get; set; } = false;

        static internal void Spawn(Player pl)
        {
            pl.Tag += Tag;
            pl.Client.Broadcast($"<size=80%><color=#6f6f6f>Вы - Шпион</color> <color=red>КСИР</color>\n" +
                $"<color=#0084c2>Ваша задача - выкачать всю информацию и вызвать подмогу</color></size>", 20, true);
            HintsUi.AddUi(pl);
        }

        [EventMethod(RoundEvents.Start)]
        static void SelectSpy()
        {
            AllowAttack = false;

            Timing.CallDelayed(1f, () =>
            {
                Select();
                if (Player.List.Count() > 20)
                    Select();

                if (Random.Range(1, 100) < 10)
                {
                    var list = Player.List.Where(x => x.RoleInformation.Role == RoleTypeId.ClassD && !x.Tag.Contains(Tag));
                    Spawn(list.ElementAt(Random.Range(0, list.Count() - 1)));
                }
            });

            void Select()
            {
                bool scientist = Random.Range(1, 100) < 70;
                RoleTypeId role = scientist ? RoleTypeId.Scientist : RoleTypeId.FacilityGuard;
                RoleTypeId roleZero = !scientist ? RoleTypeId.Scientist : RoleTypeId.FacilityGuard;

#if MRP
                var list = Player.List.Where(x => x.RoleInformation.Role == role && !x.Tag.Contains(Tag) &&
                !x.Tag.Contains(FacilityManager.TagSpy) && !x.Tag.Contains(FacilityManager.Tag));
#elif NR
                var list = Player.List.Where(x => x.RoleInformation.Role == role && !x.Tag.Contains(Tag));
#endif

                if (!list.Any())
                {
#if MRP
                    list = Player.List.Where(x => x.RoleInformation.Role == roleZero && !x.Tag.Contains(Tag) &&
                                                  !x.Tag.Contains(FacilityManager.TagSpy) &&
                                                  !x.Tag.Contains(FacilityManager.Tag));
#elif NR
                    list = Player.List.Where(x => x.RoleInformation.Role == roleZero && !x.Tag.Contains(Tag));
#endif
                }


                if (!list.Any())
                    return;

                Spawn(list.ElementAt(Random.Range(0, list.Count() - 1)));
            }
        }

        [EventMethod(PlayerEvents.Attack, -5)]
        static void Damage(AttackEvent ev)
        {
            if (ev.Attacker.Tag.Contains(Tag))
            {
                ev.FriendlyFire = false;

                if (ev.Attacker.RoleInformation.Faction == ev.Target.RoleInformation.Faction)
                    AllowAttack = true;

                return;
            }

            if (AllowAttack && ev.Target.Tag.Contains(Tag))
            {
                ev.FriendlyFire = false;
                return;
            }
        }


        [EventMethod(PlayerEvents.Dead)]
        static void Dead(DeadEvent ev)
        {
            if (ev.Target is null)
                return;

            if (!ev.Target.Tag.Contains(Tag))
                return;

            ev.Target.Tag = ev.Target.Tag.Replace(Tag, "");
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev)
        {
            if (ev.Player is null)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            if (ev.Player is null)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
        }
    }
}