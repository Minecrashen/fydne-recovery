// Qurre.API.World.Round + Map → LabApi.Features.Wrappers
using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API.World
{
    public static class Map
    {
        public static List<Room> Rooms => Lab.Map.Rooms.Select(Room.Get).ToList();
        public static List<Door> Doors => Lab.Map.Doors.Select(Door.Get).ToList();
        public static List<Tesla> Teslas => Lab.Map.Teslas.Select(Tesla.Get).ToList();
        public static List<object> Corpses => new List<object>();      // TODO: ragdolls
        public static List<object> Primitives => new List<object>();   // TODO: schematic primitives

        public static void Broadcast(string message, ushort duration = 10, bool clearPrevious = false)
            => Lab.Server.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
        public static void Broadcast(ushort duration, string message, bool clearPrevious = false)
            => Lab.Server.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
    }

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
