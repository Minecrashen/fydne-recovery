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
}
