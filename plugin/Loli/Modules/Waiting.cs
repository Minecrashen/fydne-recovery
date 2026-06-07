using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.Modules
{
    static class Waiting
    {
        static bool allowSpawn = true;

        [EventMethod(PlayerEvents.Join)]
        static void Join(JoinEvent ev)
        {
            string str = "<color=#00fffb>Добро пожаловать на сервер</color> ";
            string project = "<color=#ff0000>f</color><color=#ff004c>y</color><color=#ff007b>d</color><color=#ff00a2>n</color><color=#e600ff>e</color>";
            string str2 = "<b><color=#09ff00>Приятной игры!</color></b>";
            ev.Player.Client.ShowHint($"<b>{str}{project}</b>\n{str2}".Trim(), 10);

#if MRP
            Timing.CallDelayed(10f, () =>
            {
                if (ev.Player.Disconnected)
                    return;

                ev.Player.Client.ShowHint("<color=red><size=150%><b>Открыт набор</b></size></color>", 30);
                ev.Player.Client.ShowHint("<color=red><size=150%><b>на администратора</b></size></color>", 30);
                ev.Player.Client.ShowHint("<color=red><size=150%><b>в Discord сервере</b></size></color>", 30);
            });
#endif

            try
            {
                Timing.CallDelayed(4f, () =>
                {
                    int timer = GameCore.RoundStart.singleton.NetworkTimer;
                    if (allowSpawn && Round.Waiting && (timer > 3 || timer == -2))
                    {
                        ev.Player.RoleInformation.SetNew(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
                    }
                });
            }
            catch { }

            try
            {
                if (ev.Player.UserInformation.Id != Server.Host.UserInformation.Id)
                    Timing.CallDelayed(5f, () => Core.Socket.Emit("server.join", new object[] { ev.Player.UserInformation.UserId, ev.Player.UserInformation.Ip }));
            }
            catch { }
            try
            {
                if (!Round.Ended)
                    Core.Socket.Emit("server.addip", new object[] { Core.ServerID, ev.Player.UserInformation.Ip, ev.Player.UserInformation.UserId, ev.Player.UserInformation.Nickname });
            }
            catch { }
        }

        [EventMethod(RoundEvents.ForceStart)]
        static void ForceStart()
        {
            if (!allowSpawn)
                return;

            StartedClear();
        }

        [EventMethod(RoundEvents.Waiting)]
        static void WaitingMethod()
        {
            allowSpawn = true;
            Timing.RunCoroutine(Coroutine());

            static IEnumerator<float> Coroutine()
            {
                yield return Timing.WaitForSeconds(5f);

                while (true)
                {
                    int timer = GameCore.RoundStart.singleton.NetworkTimer;
                    if (!Round.Waiting || (timer <= 1 && timer != -2))
                        break;

                    yield return Timing.WaitForSeconds(0.5f);
                }

                StartedClear();

                yield return Timing.WaitForSeconds(0.5f);

                Round.Start();

                yield break;
            }
        }

        static void StartedClear()
        {
            allowSpawn = false;
            foreach (var pl in Player.List)
            {
                if (pl.RoleInformation.Role is not RoleTypeId.Tutorial)
                    continue;

                pl.Inventory.Clear();
                pl.RoleInformation.Role = RoleTypeId.Spectator;
            }
        }
    }
}