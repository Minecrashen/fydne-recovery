// Qurre.API.Player → обёртка над LabApi.Features.Wrappers.Player.
// ЭТАП 1 (этот файл): ядро + статика. Под-объекты (UserInformation, RoleInformation,
// Inventory, HealthInformation, Tag, Client, MovementState, GamePlay, Effects, Variables)
// добавляются поэтапно — спецификация в docs/08_LABAPI_MIGRATION.md.
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API
{
    public class Player
    {
        /// <summary>Базовый объект LabAPI.</summary>
        public Lab.Player Base { get; }

        Player(Lab.Player labPlayer) { Base = labPlayer; }

        // Кэш обёрток: одна Qurre-обёртка на один LabApi-Player.
        static readonly ConditionalWeakTable<Lab.Player, Player> _cache =
            new ConditionalWeakTable<Lab.Player, Player>();

        public static Player Get(Lab.Player labPlayer)
        {
            if (labPlayer == null) return null;
            return _cache.GetValue(labPlayer, p => new Player(p));
        }

        public static Player Get(ReferenceHub hub) => hub == null ? null : Get(Lab.Player.Get(hub));

        public static IEnumerable<Player> List => Lab.Player.List.Select(Get);

        public ReferenceHub ReferenceHub => Base.ReferenceHub;
        public UnityEngine.GameObject GameObject => Base.GameObject;

        public override bool Equals(object obj) => obj is Player p && ReferenceEquals(p.Base, Base);
        public override int GetHashCode() => Base?.GetHashCode() ?? 0;
    }
}
