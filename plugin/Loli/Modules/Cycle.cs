using Loli.Addons;
using Loli.Addons.Hints;
using MEC;
using Qurre.API.Attributes;
using Qurre.Events;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;

#if MRP
using Loli.Addons.RolePlay;
using Loli.Modules.Voices;
#elif NR
using Loli.Concepts.Ragnarok;
#endif

namespace Loli.Modules
{
    static class Cycle
    {
        [EventMethod(RoundEvents.Waiting)]
        static void NullCall() { }

        static Cycle()
        {
            ServerConsole.ReloadServerName();
            Timing.RunCoroutine(Сycle());
            Timing.RunCoroutine(SlowСycle());
            Timing.RunCoroutine(BroadCasts.Send());
            Timing.RunCoroutine(CountTicks());
            Timing.RunCoroutine(CountTicksUpdate());
        }

        static IEnumerator<float> Сycle()
        {
            for (; ; )
            {
                try { ScpHeal.Heal(); } catch { }
                try { Icom.Update(); } catch { }
                try { Fixes.FixRoundFreeze(); } catch { }
                try { Fixes.CheckBcAlive(); } catch { }
                try { Fixes.FixFreezeEnd(); } catch { }

#if NR
                try { VoteRestart.CycleCheck(); } catch { }
#endif

                try { Fixes.CheckPlayersPing(); } catch { }
                try { Fixes.AntiAllFreeze(); } catch { }

#if MRP
				try { DontPlayInPodval.CheckPlayers(); } catch { }
				try { Scp173Rework.Cycle1s(); } catch { }
				try { Scp096Rework.Cycle1s(); } catch { }
#endif

                try { ScpHint.UpdateScps(); } catch { }
                try { VoteRestart.CycleCheck(); } catch { }
                try { Logo.TimeUpdate(); } catch { }

#if NR
                try { Priest.Update1s(); } catch { }
#endif

                yield return Timing.WaitForSeconds(1f);
            }
        }
        static IEnumerator<float> SlowСycle()
        {
            for (; ; )
            {
#if NR
                try { AutoAlpha.Check(); } catch { }
#endif

                try { Caches.PosCheck(); } catch { }

#if MRP
				try { Scp173Rework.Cycle5s(); } catch { }
				try { Scp096Rework.Cycle5s(); } catch { }
#endif

                yield return Timing.WaitForSeconds(5f);
            }
        }

        static IEnumerator<float> CountTicks()
        {
            for (; ; )
            {
                Core.Ticks++;
                yield return Timing.WaitForOneFrame;
            }
        }
        static IEnumerator<float> CountTicksUpdate()
        {
            for (; ; )
            {
                Core.TicksMinutes = Core.Ticks / 5;
                Core.Ticks = 0;

                if (Core.ServerID != 0 && Player.List.Any())
                    try
                    {
                        Core.Socket.Emit("server.tps", new object[] {
                            Core.ServerID,
                            Core.ServerName,
                            Core.TicksMinutes, Player.List.Count()
                        });
                    }
                    catch { }

                yield return Timing.WaitForSeconds(5);
            }
        }
    }
}