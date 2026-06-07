using Loli.Addons;
using Loli.Concepts;
using Loli.Concepts.Scp008;
using Loli.DataBase.Modules;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using Respawning;
using System.Collections.Generic;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;
using DateTime = System.DateTime;

namespace Loli.Spawns
{
    static class SpawnManager
    {
        [EventMethod(RoundEvents.Waiting)]
        static void NullCall() { }

        internal static string SpawnProtectTag => " SpawnProtect";

        static internal DateTime LastEnter { get; set; }

        static SpawnManager()
        {
            CommandsSystem.RegisterRemoteAdmin("server_event", ServerRaEvents);
            CommandsSystem.RegisterRemoteAdmin("ci", RaCi);
            CommandsSystem.RegisterRemoteAdmin("mtf", RaMtf);

#if MRP
			CommandsSystem.RegisterConsole("co2group", SpawnCo2);
#endif
        }

#if MRP
		static void SpawnCo2(GameConsoleCommandEvent ev)
		{
			if (!ev.Allowed)
				return;

			if (!ThisAccess(ev.Player.UserInformation.UserId))
				return;

			ev.Reply = CO2.SpawnGroupFromAdmin();
			ev.Allowed = false;
		}
#endif


        static void ServerRaEvents(RemoteAdminCommandEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (!ThisAccess(ev.Player.UserInformation.UserId))
                return;

            ev.Prefix = "SERVER_EVENT";

            switch (ev.Args[0].ToLower())
            {
                case "respawn_mtf":
                    {
                        ev.Allowed = false;
                        ev.Success = true;
                        ev.Reply = "Успешно";
                        MobileTaskForces.SpawnMtf();
                        break;
                    }
                case "respawn_ci":
                    {
                        ev.Allowed = false;
                        ev.Success = true;
                        ev.Reply = "Успешно";
                        ChaosInsurgency.SpawnCI();
                        break;
                    }

            }
        }
        static void RaCi(RemoteAdminCommandEvent ev)
        {
            if (!ThisOwner(ev.Player.UserInformation.UserId)) return;
            ev.Prefix = "SERVER_EVENT";
            ev.Allowed = false;
            ev.Reply = "Успешно";
            Respawn.CallChaosCar();
        }
        static void RaMtf(RemoteAdminCommandEvent ev)
        {
            if (!ThisOwner(ev.Player.UserInformation.UserId)) return;
            ev.Prefix = "SERVER_EVENT";
            ev.Allowed = false;
            ev.Reply = "Успешно";
            Respawn.CallMtfHelicopter();
        }
        static internal bool ThisAccess(string userid)
        {
            try
            {
                if (Data.Users.TryGetValue(userid, out var data) &&
                    (data.trainee || data.helper || data.mainhelper ||
                    data.admin || data.mainadmin || data.control ||
                    data.maincontrol || data.it || data.id == 1))
                    return true;
                else return false;
            }
            catch { return false; }
        }
        static bool ThisOwner(string userid)
        {
            try
            {
                if (Data.Users.TryGetValue(userid, out var main) && main.id == 1)
                    return true;
                else return false;
            }
            catch { return false; }
        }

#if MRP
		static bool _spawned = false;
#endif

        static internal void Spawn()
        {
            if (!Round.Started)
                return;

#if MRP
			if (!_spawned)
			{
				_spawned = true;
				MobileTaskForces.SpawnMtf();
				return;
			}
#endif

            int random = Random.Range(0, 100);

            if (50 >= random) ChaosInsurgency.SpawnCI();
            else MobileTaskForces.SpawnMtf();
        }

        internal static void SpawnProtect(Player pl)
        {
            pl.Tag = pl.Tag.Replace(SpawnProtectTag, "") + SpawnProtectTag;
            Timing.CallDelayed(10f, () => pl.Tag = pl.Tag.Replace(SpawnProtectTag, ""));
        }

        [EventMethod(PlayerEvents.Damage)]
        static void SpawnProtect(DamageEvent ev)
        {
            if (ev.Target.Tag.Contains(SpawnProtectTag) && ev.DamageType is not DamageTypes.Warhead)
            {
                ev.Allowed = false;
                ev.Damage = 0f;
            }
        }


        [EventMethod(RoundEvents.Start)]
        static internal void SpawnCor()
        {
            LastEnter = DateTime.MinValue;
            Timing.RunCoroutine(SpawnTeam(), "SpawnManager_SpawnTeam");
        }

        [EventMethod(RoundEvents.End)]
        static internal void SpawnCor2()
            => Timing.KillCoroutines("SpawnManager_SpawnTeam");


        static internal IEnumerator<float> SpawnTeam()
        {
            for (; ; )
            {
#if MRP
				int random = Random.Range(200, 320);
#elif NR
                int random = Random.Range(120, 180);
#endif
                yield return Timing.WaitForSeconds(random);
                Spawn();
            }
        }
    }
}