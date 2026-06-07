using Loli.Addons;
using Loli.Concepts.Hackers;
using Loli.Controllers;
using Loli.DataBase.Modules;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.World;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using UnityEngine;
using DateTime = System.DateTime;

namespace Loli.Spawns
{
    static class ChaosInsurgency
    {
        static ChaosInsurgency()
        {
            CommandsSystem.RegisterRemoteAdmin("hacker", HackerConsole);
        }

        static internal bool First => ChaosSquads == 0;
        static internal bool HackerSpawn => HackersSpawn < 7;
        static int HackersSpawn = 0;
        static int ChaosSquads = 0;


        [EventMethod(RoundEvents.Waiting)]
        [EventMethod(RoundEvents.Start)]
        static void Refresh()
        {
            HackersSpawn = 0;
            ChaosSquads = 0;
        }

        static public void SpawnCI()
        {
#if NR
            if (Commands.EventMode)
                return;
#endif

            if (ConceptsController.IsActivated)
                return;

            if (Alpha.Detonated)
                return;

            if ((DateTime.Now - SpawnManager.LastEnter).TotalSeconds < 30)
                return;

            var pls = Player.List.Where(x => x.RoleInformation.Role is RoleTypeId.Spectator).ToList();

            if (!pls.Any())
                return;

            SpawnManager.LastEnter = DateTime.Now;

            if (First)
            {
                BackupPower.StartBackup(120f);
                ChaosSquads++;
            }

            pls.Shuffle();
            foreach (var pl in pls)
                SpawnOne(pl);
        }
        static public void SpawnOne(Player pl)
        {
            RoleTypeId _role = RoleTypeId.ChaosRifleman;
            var rand = Random.Range(0, 100);
            if (rand > 66) _role = RoleTypeId.ChaosRepressor;
            else if (rand > 33) _role = RoleTypeId.ChaosMarauder;
            SpawnManager.SpawnProtect(pl);
            if (HackerSpawn || rand < 7)
            {
                Hacker.Spawn(pl);
                HackersSpawn++;
            }
            else if (pl.RoleInformation.Role is RoleTypeId.Spectator)
                pl.RoleInformation.SetNew(_role, RoleChangeReason.Respawn);
        }
        static void HackerConsole(RemoteAdminCommandEvent ev)
        {
            ev.Allowed = false;
            ev.Prefix = "SERVER_EVENT";
#if MRP
            if (!(ev.Sender.SenderId == "SERVER CONSOLE" || (Data.Users.TryGetValue(ev.Player.UserInformation.UserId, out var _d) && _d.id == 1)))
#elif NR
            if (!(ev.Sender.SenderId == "SERVER CONSOLE" || ev.Player.UserInformation.UserId == "76561199298395982@steam" || (Data.Users.TryGetValue(ev.Player.UserInformation.UserId, out var _d) && _d.id == 1)))
#endif
            {
                ev.Reply = "Отказано в доступе";
                return;
            }
            string name = string.Join(" ", ev.Args);
            if (name.Contains("&"))
            {
                var names = name.Split('&');
                Player hacker = names[0].GetPlayer();
                Player guard = names[1].GetPlayer();
                if (hacker is null)
                {
                    ev.Reply = "Хакер не найден";
                    return;
                }
                if (guard is null)
                {
                    ev.Reply = "Охранник не найден";
                    return;
                }
                ev.Reply = "Успешно";
                Hacker.Spawn(hacker, guard);
            }
            else
            {
                Player player = name.GetPlayer();
                if (player is null)
                {
                    ev.Reply = "Игрок не найден";
                    return;
                }
                ev.Reply = "Успешно";
                Hacker.Spawn(player);
            }
        }
    }
}