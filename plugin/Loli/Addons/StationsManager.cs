using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;

namespace Loli.Addons
{
    static class StationsManager
    {
        static readonly Dictionary<WorkStation, Action<InteractWorkStationEvent>> Events = new();
        static readonly List<WorkStation> BlockUpdates = new();

        static internal void AddBlock(WorkStation workStation)
        {
            BlockUpdates.Add(workStation);
        }
        static internal bool RemoveBlock(WorkStation workStation)
        {
            return BlockUpdates.Remove(workStation);
        }

        static internal void Register(WorkStation workStation, Action<InteractWorkStationEvent> action, bool addToBlock = true)
        {
            if (Events.ContainsKey(workStation))
            {
                Log.Error($"WorkStation \"${workStation}\" already exist");
                return;
            }

            Events.Add(workStation, action);

            if (addToBlock)
            {
                AddBlock(workStation);
            }
        }


        [EventMethod(RoundEvents.Restart)]
        [EventMethod(RoundEvents.Waiting, int.MaxValue)]
        static void Refresh()
        {
            Events.Clear();
            BlockUpdates.Clear();
        }

        [EventMethod(MapEvents.WorkStationUpdate)]
        internal static void Event(WorkStationUpdateEvent ev)
        {
            if (!BlockUpdates.Contains(ev.Station))
                return;

            ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.InteractWorkStation)]
        internal static void Event(InteractWorkStationEvent ev)
        {
            if (!Events.TryGetValue(ev.Station, out var action))
                return;

            action(ev);
        }
    }
}