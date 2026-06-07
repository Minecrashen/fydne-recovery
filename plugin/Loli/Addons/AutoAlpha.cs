#if NR
using Loli.Controllers;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.World;
using Qurre.Events;
using Qurre.Events.Structs;

namespace Loli.Addons
{
    static class AutoAlpha
    {
        internal static bool InProgress { get; set; } = false;
        internal static bool Enabled { get; set; } = false;

        [EventMethod(RoundEvents.Waiting)]
        [EventMethod(RoundEvents.Start)]
        static void Refresh()
        {
            Enabled = false;
            InProgress = false;
        }

        static internal void Check()
        {
            if (22 >= Round.ElapsedTime.Minutes)
                return;
            if (InProgress || Alpha.Detonated || AlphaController.IsLocked)
                return;

            Map.Broadcast("<size=65%><color=#6f6f6f>По приказу совета О5 запущена <color=red>Альфа Боеголовка</color></color></size>", 10, true);

            if (!Alpha.Active)
                Alpha.Start();

            InProgress = true;
        }

        [EventMethod(AlphaEvents.Stop)]
        static void AntiDisable(AlphaStopEvent ev)
        {
            if (InProgress)
                ev.Allowed = false;
        }

        [EventMethod(AlphaEvents.Detonate)]
        static void Detonated()
        {
            int round = Round.CurrentRound;
            Timing.CallDelayed(30f, () =>
            {
                if (round != Round.CurrentRound) return;
                Round.End();
            });
        }
    }
}
#endif