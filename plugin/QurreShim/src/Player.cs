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
    public partial class Player
    {
        /// <summary>Базовый объект LabAPI.</summary>
        public Lab.Player Base { get; }

        /// <summary>Qurre-фича: произвольное хранилище данных на игрока.</summary>
        internal readonly Dictionary<string, object> Vars = new Dictionary<string, object>();

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
        public Mirror.NetworkConnectionToClient ConnectionToClient => Base.ConnectionToClient;

        // Под-объекты Qurre (см. PlayerSubObjects.cs)
        public UserInformationW UserInformation => new UserInformationW(Base);
        public RoleInformationW RoleInformation => new RoleInformationW(Base);
        public HealthInformationW HealthInformation => new HealthInformationW(Base);
        public InventoryW Inventory => new InventoryW(Base);
        public MovementStateW MovementState => new MovementStateW(Base);
        public GamePlayW GamePlay => new GamePlayW(Base);
        public ClientW Client => new ClientW(Base);
        public VariablesW Variables => new VariablesW(this);

        /// <summary>RA-бейдж (текст тега). Qurre Player.Tag.</summary>
        public string Tag
        {
            get => Base.ReferenceHub.serverRoles.Network_myText ?? string.Empty;
            set => Base.ReferenceHub.serverRoles.Network_myText = value;
        }

        public override bool Equals(object obj) => obj is Player p && ReferenceEquals(p.Base, Base);
        public override int GetHashCode() => Base?.GetHashCode() ?? 0;
    }
}
