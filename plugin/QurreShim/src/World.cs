// Qurre.API.World.Round + Map → LabApi.Features.Wrappers
using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using SchematicUnity.API.Objects;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API
{
    internal static class ShimState
    {
        static readonly Dictionary<ushort, Player> _corpseOwners = new Dictionary<ushort, Player>();
        internal static readonly HashSet<SObject> SceneObjects = new HashSet<SObject>();
        internal static readonly HashSet<WorkStation> WorkStations = new HashSet<WorkStation>();

        internal static void TrackCorpse(Lab.Ragdoll ragdoll, Player owner)
        {
            if (ragdoll != null) _corpseOwners[ragdoll.Serial] = owner;
        }

        internal static void UntrackCorpse(Lab.Ragdoll ragdoll)
        {
            if (ragdoll != null) _corpseOwners.Remove(ragdoll.Serial);
        }

        internal static Player CorpseOwner(Lab.Ragdoll ragdoll)
            => ragdoll != null && _corpseOwners.TryGetValue(ragdoll.Serial, out var owner) ? owner : null;

        internal static void ClearRoundState()
        {
            Alpha.Active = false;
            Alpha.Detonated = false;
            Decontamination.InProgress = false;
            _corpseOwners.Clear();
            SceneObjects.RemoveWhere(x => x == null || x.GameObject == null);
            // Раньше чистились только null-записи (а C#-объект null не бывает) → набор,
            // подписки OnSearching/OnInteracted и сетевые toy'и копились каждый раунд.
            // Теперь уничтожаем toy + отписываемся и полностью очищаем набор.
            foreach (var station in WorkStations) station?.DestroyToy();
            WorkStations.Clear();
        }
    }
}

namespace Qurre.API.World
{
    public static class Map
    {
        public static List<Room> Rooms => Lab.Map.Rooms.Select(Room.Get).ToList();
        public static List<Door> Doors => Lab.Map.Doors.Select(Door.Get).ToList();
        public static List<Tesla> Teslas => Lab.Map.Teslas.Select(Tesla.Get).ToList();
        public static List<Corpse> Corpses => Lab.Map.Ragdolls.Select(x => new Corpse(x, Qurre.API.ShimState.CorpseOwner(x))).ToList();
        public static List<SObject> Primitives => Qurre.API.ShimState.SceneObjects.Where(x => x?.GameObject != null).ToList();

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
        /// <summary>Инкремент токена раунда. Зовётся из OnRoundStarted (EventMap) на КАЖДОМ старте —
        /// и ванильном (таймер лобби), и плагинном. Раньше счётчик рос только в Round.Start(),
        /// поэтому guard'ы корутин (round != CurrentRound) не срабатывали на ванильных стартах.</summary>
        internal static void MarkRoundStarted() => _currentRound++;
        public static bool Started => Lab.Round.IsRoundStarted;
        public static bool Ended => Lab.Round.IsRoundEnded;
        /// <summary>Лобби (раунд ещё не стартовал и не закончился).</summary>
        public static bool Waiting => !Lab.Round.IsRoundStarted && !Lab.Round.IsRoundEnded;

        public static TimeSpan ElapsedTime => Lab.Round.Duration;

        // TODO: уточнить семантику при переносе зависимых модулей
        public static int ActiveGenerators => Lab.Map.Generators.Count(x => x.Engaged);

        public static void End() => Lab.Round.End(true);
        // Инкремент токена делает OnRoundStarted (через MarkRoundStarted), а не этот метод —
        // иначе плагинный старт считался бы дважды (Start() + событие RoundStarted).
        public static void Start() => Lab.Round.Start();
        public static void Restart() => Lab.Round.Restart(false, true, global::ServerStatic.NextRoundAction.Restart);
        public static float WaitTime { get; set; }
        public static DateTime StartedTime { get; set; } = DateTime.UtcNow;
        public static void DimScreen() { }
    }
}
