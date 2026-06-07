#if MRP
using Interactables.Interobjects;
using Loli.Spawns;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.Addons.RolePlay
{
    static class Modules
    {
        static Modules()
        {
            CommandsSystem.RegisterConsole("escd", EscapeD);
        }
        static void EscapeD(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;

            if (!SpawnManager.ThisAccess(ev.Player.UserInformation.UserId))
            {
                ev.Reply = "Недостаточно доступа";
                return;
            }

            int count = 0;

            foreach (var pl in Player.List)
            {
                if (pl.RoleInformation.Role is not RoleTypeId.Spectator)
                    continue;

                pl.RoleInformation.SetNew(RoleTypeId.ClassD, RoleChangeReason.Respawn);
                count++;
            }

            ev.Reply = $"Вызвана новая волна Д-класса из {count} человек";
        }

        [EventMethod(RoundEvents.Start, -2)]
        static void MainGuard()
        {
            var list = Player.List.Where(x => x.RoleInformation.Role == RoleTypeId.FacilityGuard).ToArray();
            if (list.Count() == 0) return;
            list.Shuffle();
            Player guard = list.First();
            Timing.CallDelayed(0.5f, () =>
            {
                guard.HealthInformation.MaxHp = 150;
                guard.HealthInformation.Hp = 150;
                guard.Inventory.Reset(new List<ItemType>
                {
                    ItemType.KeycardMTFOperative,
                    ItemType.ArmorCombat,
                    ItemType.GunE11SR,
                    ItemType.SCP500,
                    ItemType.Medkit,
                    ItemType.Radio,
                    ItemType.Flashlight,
                });
                guard.AddCuff();
                guard.Client.Broadcast("<size=70%><color=#6f6f6f>Вы - <color=#afafa1>Начальник СБ</color>\n" +
                    "Вся охрана комплекса подчиняется вашим приказам</color></size>", 10, true);
                guard.UserInformation.CustomInfo = "Начальник СБ";
                guard.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo | PlayerInfoArea.PowerStatus;
                guard.GetAmmo();
                guard.Inventory.Ammo.Ammo556 = 120;
            });
        }

        [EventMethod(PlayerEvents.UsingRadio)]
        static void BetterRadio(UsingRadioEvent ev)
        {
            ev.Consumption *= 0.1f;
        }

        [EventMethod(PlayerEvents.Escape)]
        static void RealEscape(EscapeEvent ev)
        {
            if (ev.Role != RoleTypeId.None)
                ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.InteractDoor)]
        static void Door(InteractDoorEvent ev)
        {
            if (ev.Door.Type is not DoorType.LczCafe and not DoorType.LczGr18Gate)
                return;

            if (ev.Player.RoleInformation.Role == RoleTypeId.Scp079)
                return;

            if (ev.Player.Inventory.Base.CurInstance?.Category == ItemCategory.Keycard)
                return;

            ev.Allowed = false;
        }

        [EventMethod(MapEvents.DamageDoor)]
        static void DontDestroyDoor(DamageDoorEvent ev)
        {
            if (ev.Door.Type == DoorType.Hcz096)
            {
                ev.Allowed = false;
                return;
            }
        }

        [EventMethod(PlayerEvents.InteractLift)]
        static void Door(InteractLiftEvent ev)
        {
            if (ev.Player.RoleInformation.Role is not RoleTypeId.Scp096 and not RoleTypeId.Scp106 and not RoleTypeId.Scp173 and not RoleTypeId.Scp939)
                return;

            if (ev.Lift.Type is not ElevatorGroup.LczA01 and not ElevatorGroup.LczA02
                and not ElevatorGroup.LczB01 and not ElevatorGroup.LczB02)
                return;

            if (Round.ElapsedTime.TotalMinutes > 3)
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.InteractDoor)]
        static void DoorChck(InteractDoorEvent ev)
        {
            RoleTypeId role = ev.Player.RoleInformation.Role;

            if (role is RoleTypeId.Scp106)
            {
                ev.Allowed = false;
                return;
            }

            if (role is RoleTypeId.Scp096 or RoleTypeId.Scp173 or RoleTypeId.Scp939 && ev.Door.Open)
            {
                ev.Allowed = false;
                return;
            }

            if (ev.Door.Type is not DoorType.EzCheckpointArmoryA and not DoorType.EzCheckpointArmoryB)
                return;

            if (ev.Player.RoleInformation.Role == RoleTypeId.Scp079)
                return;

            if (ev.Player.RoleInformation.Team != Team.SCPs)
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.InteractGenerator)]
        static void GenScp(InteractGeneratorEvent ev)
        {
            if (ev.Player.RoleInformation.Team != Team.SCPs || ev.Player.RoleInformation.Role == RoleTypeId.Scp049)
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.Damage)]
        static void Damage(DamageEvent ev)
        {
            if (ev.Attacker.RoleInformation.Role is RoleTypeId.Scp096 or RoleTypeId.Scp049)
                ev.Damage = -1;
        }

        [EventMethod(ScpEvents.Scp049RaisingStart)]
        static void Anti049RaiseZombie(Scp049RaisingStartEvent ev)
        {
            if (ev.Corpse.Role is RoleTypeId.Scp0492)
                ev.Allowed = false;
        }

        static void RespawnLeave(LeaveEvent ev)
        {

        }
    }
}
#endif