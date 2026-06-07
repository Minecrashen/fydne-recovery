#if NR
using Loli.Concepts.NuclearAttack;
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

namespace Loli.Addons
{
    static class Force
    {
        [EventMethod(RoundEvents.Waiting)]
        static void NullCall() { }

        static Force()
        {
            CommandsSystem.RegisterConsole("s", ConsoleS);
            CommandsSystem.RegisterConsole("force", ConsoleForce);
        }

        static internal List<RoleTypeId> SpawnedSCPs = new();

        [EventMethod(RoundEvents.Waiting)]
        static void Clear()
            => SpawnedSCPs.Clear();

        [EventMethod(PlayerEvents.Dies, int.MinValue)]
        static void AddToList(DiesEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Target.RoleInformation.Team != Team.SCPs)
                return;

            if (SpawnedSCPs.Contains(ev.Target.RoleInformation.Role))
                return;

            if (ev.Target.HealthInformation.Hp - ev.Target.HealthInformation.MaxHp > -100)
                return;

            SpawnedSCPs.Add(ev.Target.RoleInformation.Role);
        }

        [EventMethod(PlayerEvents.Join)]
        static void Join(JoinEvent ev)
        {
            if (Round.ElapsedTime.Minutes == 0 && Round.Started)
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    if (ev.Player.RoleInformation.Role is not RoleTypeId.Spectator)
                        return;

                    ev.Player.RoleInformation.Role = RoleTypeId.ClassD;
                });
            }
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            if (Round.ElapsedTime.Minutes == 0)
            {
                if (ev.Player.Tag.Contains(CPIR.Tag))
                    return;

                if (ev.Role == RoleTypeId.FacilityGuard)
                {
                    ev.Player.Client.Broadcast(10, "<color=#00ff22>Вы можете стать <color=#fdffbb>Ученым</color>, " +
                        "написав <color=#0089c7>.</color><color=#ffff00>s</color> в консоли на <color=#f47fff>[<color=red>ё</color>]</color></color>");
                }
                if (ev.Role.GetTeam() == Team.SCPs)
                {
                    ev.Player.Client.Broadcast(10, "<color=#00ff22>Вы можете изменить своего <color=red>SCP</color>, " +
                        "написав <color=#0089c7>.</color><color=#ffff00>force</color> в консоли на <color=#f47fff>[<color=red>ё</color>]</color></color>");
                }
            }
        }

        static void ConsoleS(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (ev.Player.Tag.Contains(CPIR.Tag))
            {
                ev.Reply = "Отключено для Шпионов КСИР";
                ev.Color = "red";
                return;
            }
            if (ev.Player.RoleInformation.Role is not RoleTypeId.FacilityGuard)
            {
                ev.Reply = "Данная команда доступна только для Охраны Комплекса";
                ev.Color = "red";
                return;
            }
            if (Round.ElapsedTime.TotalMinutes > 1)
            {
                ev.Reply = "Прошло более одной минуты";
                ev.Color = "red";
                return;
            }
            ev.Player.RoleInformation.Role = RoleTypeId.Scientist;
            ev.Reply = "Успешно";
            ev.Color = "green";
        }

        static void ConsoleForce(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;

            if (ev.Player.RoleInformation.Team is not Team.SCPs)
            {
                ev.Reply = "Данная команда доступна только для SCP";
                ev.Color = "red";
                return;
            }

            if (ev.Player.RoleInformation.Role == RoleTypeId.Scp0492)
            {
                ev.Reply = "\n" +
                "Увы, но данная команда недоступна для SCP 049-2.";
                ev.Color = "red";
                return;
            }

            if (Round.ElapsedTime.TotalMinutes > 2)
            {
                ev.Reply = "Прошло более 2-х минут";
                ev.Color = "red";
                return;
            }

            if (ev.Args.Length == 0)
            {
                NotFound();
                return;
            }

            switch (ev.Args[0])
            {
                case "173":
                    {
                        Force(RoleTypeId.Scp173);
                        return;
                    }
                case "106":
                    {
                        Force(RoleTypeId.Scp106);
                        return;
                    }
                case "049":
                    {
                        Force(RoleTypeId.Scp049);
                        return;
                    }
                case "079":
                    {
                        Force(RoleTypeId.Scp079);
                        return;
                    }
                case "096":
                    {
                        Force(RoleTypeId.Scp096);
                        return;
                    }
                case "939":
                    {
                        Force(RoleTypeId.Scp939);
                        return;
                    }
                case "3114":
                    {
                        Force(RoleTypeId.Scp3114);
                        return;
                    }
                default:
                    {
                        NotFound();
                        return;
                    }
            }

            void Force(RoleTypeId type)
            {
                if (SpawnedSCPs.Contains(type))
                {
                    ev.Reply = "Данный SCP уже был в раунде";
                    ev.Color = "red";
                    return;
                }

                if (Player.List.Any(x => x.RoleInformation.Role == type))
                {
                    ev.Reply = "Данный SCP уже играет";
                    ev.Color = "red";
                    return;
                }

                ev.Player.RoleInformation.Role = type;
                ev.Reply = "Успешно";
                ev.Color = "green";
            }

            void NotFound()
            {
                ev.Reply = "\n" +
                    " Не удалось найти доступную роль\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP049 -\n.force 049\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP079 -\n.force 079\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP096 -\n.force 096\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP106 -\n.force 106\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP173 -\n.force 173\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP939 -\n.force 939\n~-~-~-~-~-~-~-~-~-~-~-~-~-~\n" +
                    " SCP3114 -\n.force 3114\n~-~-~-~-~-~-~-~-~-~-~-~-~-~";
                ev.Color = "red";
            }
        }
    }
}
#endif