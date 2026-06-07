using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using Qurre.API.Controllers;

#if NR
using PlayerRoles;
#endif

namespace Loli.DataBase.Modules
{
    static class Updater
    {
#if NR
        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev) => ChangeRole(ev.Player, ev.Role.GetTeam());

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev) => ChangeRole(ev.Player, ev.Role.GetTeam());

        static void ChangeRole(Player pl, Team tm)
        {
            if (tm is not Team.SCPs) return;
            Data.scp_play[pl.UserInformation.UserId] = true;
        }
#endif

        static bool _endSaving = false;

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting() => _endSaving = false;

        [EventMethod(RoundEvents.End)]
        static void End()
        {
#if NR
            if (Core.BlockStats)
                return;
#endif

            _endSaving = true;
            double cf = 1;
            try
            {
                int hour = DateTime.UtcNow.Hour + 3;
                if (hour >= 3 && hour < 7)
                    cf = Core.PreMorningStatsCf;
                else if (hour >= 7 && hour < 13)
                    cf = Core.MorningStatsCf;
                else if (hour >= 13 && hour < 17)
                    cf = Core.DayStatsCf;

#if NR
                else if (hour >= 17 && hour < 21)
                    cf = Core.AverageCf;
#endif

                else if (hour >= 21 && hour < 24)
                    cf = Core.PreNightCf;
                else if (hour >= 0 && hour < 2)
                    cf = Core.NightCf;
            }
            catch { }
            foreach (var pl in Player.List) try
                {
                    if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var data) && data.found)
                    {
                        int played = (int)(DateTime.Now - data.entered).TotalSeconds;
                        int total = (int)Math.Round(played * cf);
                        Core.Socket.Emit("database.add.time", new object[] { data.id, 1, total });
                    }
                }
                catch { }
        }

        [EventMethod(PlayerEvents.Leave)]
        static void Leave(LeaveEvent ev)
        {
#if NR
            if (Core.BlockStats)
                return;

            Module.Prefixs.Remove(ev.Player.UserInformation.UserId);
#endif

            if (Data.Users.TryGetValue(ev.Player.UserInformation.UserId, out var data))
            {
                Data.Users.Remove(ev.Player.UserInformation.UserId);
                if (_endSaving) return;
                if (data.found)
                {
                    double cf = 1;
                    try
                    {
                        int hour = DateTime.UtcNow.Hour + 3;
                        if (hour >= 3 && hour < 7)
                            cf = Core.PreMorningStatsCf;
                        else if (hour >= 7 && hour < 13)
                            cf = Core.MorningStatsCf;
                        else if (hour >= 13 && hour < 17)
                            cf = Core.DayStatsCf;

#if NR
                        else if (hour >= 17 && hour < 21)
                            cf = Core.AverageCf;
#endif

                        else if (hour >= 21 && hour < 24)
                            cf = Core.PreNightCf;
                        else if (hour >= 0 && hour < 2)
                            cf = Core.NightCf;
                    }
                    catch { }
                    int played = (int)(DateTime.Now - data.entered).TotalSeconds;
                    int total = (int)Math.Round(played * cf);
                    Core.Socket.Emit("database.add.time", new object[] { data.id, 1, total });
                }
            }
        }
    }
}