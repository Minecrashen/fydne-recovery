// Минимальные обёртки-контроллеры Qurre.API.Controllers → LabApi.
// ЭТАП: каркас (Get + Base + базовые члены). Полные члены — следующий инкремент.
using UnityEngine;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API.Controllers
{
    public class Room
    {
        public Lab.Room Base { get; }
        Room(Lab.Room b) { Base = b; }
        public static Room Get(Lab.Room b) => b == null ? null : new Room(b);
        public Vector3 Position => Base.Position;
    }

    public class Door
    {
        public Lab.Door Base { get; }
        Door(Lab.Door b) { Base = b; }
        public static Door Get(Lab.Door b) => b == null ? null : new Door(b);
        public Vector3 Position => Base.Position;
    }

    public class Lift
    {
        public Lab.Elevator Base { get; }
        Lift(Lab.Elevator b) { Base = b; }
        public static Lift Get(Lab.Elevator b) => b == null ? null : new Lift(b);
    }

    public class Tesla
    {
        public Lab.Tesla Base { get; }
        Tesla(Lab.Tesla b) { Base = b; }
        public static Tesla Get(Lab.Tesla b) => b == null ? null : new Tesla(b);
    }

    public class WorkStation
    {
        public Component Base { get; }
        WorkStation(Component b) { Base = b; }
        public static WorkStation Get(Component b) => b == null ? null : new WorkStation(b);
        public Vector3 Position => Base != null ? Base.transform.position : Vector3.zero;
    }

    public static class Cassie
    {
        public static void Send(string words, bool makeHold = true, bool makeNoise = true, bool customAnnouncement = true)
            => Lab.Announcer.Message(words, "", makeHold);
        public static void Lock(string words, bool makeHold = true, bool makeNoise = true)
            => Lab.Announcer.Message(words, "", makeHold);
    }
}

namespace Qurre.API
{
    using System;
    using System.Collections.Generic;

    /// <summary>Qurre-расширения коллекций (List.TryFind и т.п.).</summary>
    public static class CollectionExtensions
    {
        public static bool TryFind<T>(this List<T> list, Predicate<T> match, out T result)
        {
            int i = list.FindIndex(match);
            if (i >= 0) { result = list[i]; return true; }
            result = default;
            return false;
        }
    }
}
