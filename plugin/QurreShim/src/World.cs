// Qurre.API.World.Round → LabApi.Features.Wrappers.Round
// (Map требует обёрток Room/Door — добавляется отдельным этапом)
using System;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API.World
{
    public static class Round
    {
        /// <summary>Qurre: раунд идёт прямо сейчас (используется голым как bool).</summary>
        public static bool CurrentRound => Lab.Round.IsRoundInProgress;
        public static bool Started => Lab.Round.IsRoundStarted;
        public static bool Ended => Lab.Round.IsRoundEnded;
        /// <summary>Лобби (раунд ещё не стартовал и не закончился).</summary>
        public static bool Waiting => !Lab.Round.IsRoundStarted && !Lab.Round.IsRoundEnded;

        public static TimeSpan ElapsedTime => Lab.Round.Duration;

        // TODO: уточнить семантику при переносе зависимых модулей
        public static int ActiveGenerators => 0;

        public static void End() => Lab.Round.End(true);
        public static void Start() => Lab.Round.Start();
        public static void Restart() => Lab.Round.Restart(false, true, global::ServerStatic.NextRoundAction.Restart);
    }
}
