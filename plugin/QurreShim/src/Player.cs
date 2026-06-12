// Qurre.API.Player → обёртка над LabApi.Features.Wrappers.Player.
// ЭТАП 1 (этот файл): ядро + статика. Под-объекты (UserInformation, RoleInformation,
// Inventory, HealthInformation, Tag, Client, MovementState, GamePlay, Effects, Variables)
// добавляются поэтапно — спецификация в docs/08_LABAPI_MIGRATION.md.
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using CentralAuth;
using Qurre.API;
using Qurre.Events.Structs;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API.Controllers
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
        public UnityEngine.Transform Transform => Base.GameObject.transform;
        public UnityEngine.Transform CameraTransform => Base.ReferenceHub.PlayerCameraReference;
        public Mirror.NetworkConnectionToClient ConnectionToClient => Base.ConnectionToClient;

        // Под-объекты Qurre (см. PlayerSubObjects.cs).
        // Кэшируются лениво НА ОБЁРТКУ: раньше каждый доступ создавал новый объект, из-за чего
        // stateful-поля (UserInformation.InfoToShow, HealthInformation.AhpActiveProcesses,
        // StatsInformation.KillsCount/DeathsCount/Kills) жили лишь до конца выражения («амнезия»).
        UserInformationW _userInformation;
        RoleInformationW _roleInformation;
        HealthInformationW _healthInformation;
        InventoryW _inventory;
        MovementStateW _movementState;
        GamePlayW _gamePlay;
        EffectsW _effects;
        AdministrativeW _administrative;
        StatsInformationW _statsInformation;
        Qurre.API.Classification.Player.Client _client;
        VariablesW _variables;
        public UserInformationW UserInformation => _userInformation ??= new UserInformationW(Base);
        public RoleInformationW RoleInformation => _roleInformation ??= new RoleInformationW(Base);
        public HealthInformationW HealthInformation => _healthInformation ??= new HealthInformationW(Base);
        public InventoryW Inventory => _inventory ??= new InventoryW(Base);
        public MovementStateW MovementState => _movementState ??= new MovementStateW(Base);
        public GamePlayW GamePlay => _gamePlay ??= new GamePlayW(Base);
        public EffectsW Effects => _effects ??= new EffectsW(Base);
        public AdministrativeW Administrative => _administrative ??= new AdministrativeW(Base);
        public StatsInformationW StatsInformation => _statsInformation ??= new StatsInformationW(Base);
        public Qurre.API.Classification.Player.Client Client => _client ??= new Qurre.API.Classification.Player.Client(this);
        public VariablesW Variables => _variables ??= new VariablesW(this);
        public bool Disconnected => Base.IsDestroyed;
        public bool IsHost => Base.IsHost;
        public int Ping => Base.ReferenceHub.connectionToClient?.rtt is double rtt ? (int)(rtt * 1000) : 0;
        public System.DateTime JoinedTime { get; set; } = System.DateTime.UtcNow;
        public System.DateTime SpawnedTime { get; set; } = System.DateTime.UtcNow;
        public float LastSynced { get; set; }
        public Mirror.NetworkConnectionToClient Connection => ConnectionToClient;
        public PlayerAuthenticationManager AuthManager => ReferenceHub.authManager;
        public void InvokeEscape(bool cuffed = false)
            => Core.Dispatch(new EscapeEvent { Player = this, OldRole = RoleInformation.Role, Role = RoleInformation.Role });
        public void InvokeEscape(PlayerRoles.RoleTypeId newRole)
            => Core.Dispatch(new EscapeEvent { Player = this, OldRole = RoleInformation.Role, Role = newRole });
        public InventorySystem.Items.ItemBase CreateItemInstance(InventorySystem.Items.ItemIdentifier item, bool addToInventory = false)
        {
            try { return Base?.ReferenceHub?.inventory?.CreateItemInstance(item, false); }
            catch { return null; }
        }

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
