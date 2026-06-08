// Qurre.API.World.Round + Map → LabApi.Features.Wrappers
using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using SchematicUnity.API.Objects;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API.World
{
    public static class Map
    {
        public static List<Room> Rooms => Lab.Map.Rooms.Select(Room.Get).ToList();
        public static List<Door> Doors => Lab.Map.Doors.Select(Door.Get).ToList();
        public static List<Tesla> Teslas => Lab.Map.Teslas.Select(Tesla.Get).ToList();
        public static List<Corpse> Corpses => new List<Corpse>();      // TODO: ragdolls
        public static List<SObject> Primitives => new List<SObject>(); // TODO: schematic primitives

        public static MapBroadcast Broadcast(string message, ushort duration = 10, bool clearPrevious = false)
        {
            Lab.Server.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
            return new MapBroadcast(message, duration);
        }
        public static MapBroadcast Broadcast(ushort duration, string message, bool clearPrevious = false)
        {
            Lab.Server.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
            return new MapBroadcast(message, duration);
        }
    }

    public static class Round
    {
        static int _currentRound;
        /// <summary>Qurre legacy used this as a numeric round token captured by coroutines.</summary>
        public static int CurrentRound => Lab.Round.IsRoundStarted ? _currentRound == 0 ? 1 : _currentRound : 0;
        public static bool Started => Lab.Round.IsRoundStarted;
        public static bool Ended => Lab.Round.IsRoundEnded;
        /// <summary>Лобби (раунд ещё не стартовал и не закончился).</summary>
        public static bool Waiting => !Lab.Round.IsRoundStarted && !Lab.Round.IsRoundEnded;

        public static TimeSpan ElapsedTime => Lab.Round.Duration;

        // TODO: уточнить семантику при переносе зависимых модулей
        public static int ActiveGenerators => 0;

        public static void End() => Lab.Round.End(true);
        public static void Start() { _currentRound++; Lab.Round.Start(); }
        public static void Restart() => Lab.Round.Restart(false, true, global::ServerStatic.NextRoundAction.Restart);
        public static float WaitTime { get; set; }
        public static DateTime StartedTime { get; set; } = DateTime.UtcNow;
        public static void DimScreen() { }
    }
}
