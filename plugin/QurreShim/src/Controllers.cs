// Минимальные обёртки-контроллеры Qurre.API.Controllers → LabApi.
// ЭТАП: каркас (Get + Base + базовые члены). Полные члены — следующий инкремент.
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Interactables.Interobjects.DoorUtils;
using Qurre.Events.Structs;
using Lab = LabApi.Features.Wrappers;

namespace Qurre.API.Controllers
{
    public class Camera
    {
        public Lab.Camera Base { get; }
        Camera(Lab.Camera b) { Base = b; }
        public static Camera Get(Lab.Camera b) => b == null ? null : new Camera(b);
        public static Camera Get(PlayerRoles.PlayableScps.Scp079.Cameras.Scp079Camera b)
            => b == null ? null : Get(Lab.Camera.Get(b));
        public Vector3 Position => Base?.Position ?? Vector3.zero;
        public Quaternion Rotation { get => Base?.Rotation ?? Quaternion.identity; set { if (Base != null) Base.Rotation = value; } }
        public Room Room => Room.Get(Base?.Room);
        public GameObject GameObject => Base?.GameObject;
    }

    public class Room
    {
        public Lab.Room Base { get; }
        Room(Lab.Room b) { Base = b; }
        public static Room Get(Lab.Room b) => b == null ? null : new Room(b);
        public Vector3 Position { get => Base?.Position ?? Vector3.zero; set { } }
        public Quaternion Rotation => Base?.Rotation ?? Quaternion.identity;
        public Transform Transform => Base?.GameObject?.transform;
        public GameObject GameObject => Base?.GameObject;
        public string Name => Base?.Name.ToString() ?? string.Empty;
        public dynamic Type => Base?.Name;
        public List<Door> Doors => Base?.Doors?.Select(Door.Get).ToList() ?? new List<Door>();
        public List<Camera> Cameras => Base?.Cameras?.Select(Camera.Get).ToList() ?? new List<Camera>();
        public dynamic NetworkIdentity => Base?.GameObject?.GetComponent<Mirror.NetworkIdentity>();
        public RoomLights Lights { get; } = new RoomLights();
        public void LightsOff(float duration = 10f) { Lights.Enabled = false; }
    }

    public class RoomLights
    {
        public bool Enabled { get; set; } = true;
        public bool LockChange { get; set; }
        public bool Override { get; set; }
        public Color Color { get; set; } = Color.white;
    }

    public class Door
    {
        public Lab.Door Base { get; }
        readonly GameObject _gameObject;
        string _name = "";
        Vector3 _position;
        Quaternion _rotation = Quaternion.identity;
        public Door(Lab.Door b) { Base = b; _gameObject = b?.GameObject; _name = b?.ToString() ?? ""; }
        public Door(Vector3 position, object prefab) : this(position, prefab, Quaternion.identity, null) { }
        public Door(Vector3 position, object prefab, Quaternion rotation) : this(position, prefab, rotation, null) { }
        public Door(Vector3 position, object prefab, Quaternion rotation, object permissions)
        {
            _position = position;
            _rotation = rotation;
            Permissions = permissions;
            _gameObject = new GameObject("Door");
            _gameObject.transform.position = position;
            _gameObject.transform.rotation = rotation;
        }
        public static Door Get(Lab.Door b) => b == null ? null : new Door(b);
        public Vector3 Position { get => Base?.Position ?? _position; set { _position = value; if (_gameObject != null) _gameObject.transform.position = value; } }
        public Quaternion Rotation { get => Base?.Rotation ?? _rotation; set { _rotation = value; if (_gameObject != null) _gameObject.transform.rotation = value; } }
        public Vector3 Scale { get; set; } = Vector3.one;
        public GameObject GameObject => Base?.GameObject ?? _gameObject;
        public string Name { get => Base?.ToString() ?? _name; set => _name = value; }
        public dynamic Type => Base?.GetType().Name;
        public bool Open { get => Base?.IsOpened ?? false; set { if (Base != null) Base.IsOpened = value; } }
        public bool Lock { get; set; }
        public bool Destroyed { get; set; }
        public bool IsLift => false;
        public GameObject DoorVariant => GameObject;
        public dynamic Permissions { get; set; }
        public void Unlock() { }
        public void Destroy() { Destroyed = true; try { UnityEngine.Object.Destroy(GameObject); } catch { } }
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
        public void Destroy() { }
    }

    public class Corpse
    {
        readonly Lab.Ragdoll _base;
        Vector3 _position;
        Quaternion _rotation = Quaternion.identity;
        Vector3 _scale = Vector3.one;
        public Vector3 Position
        {
            get => _base?.Position ?? _position;
            set { _position = value; if (_base != null) _base.Position = value; else if (GameObject != null) GameObject.transform.position = value; }
        }
        public Quaternion Rotation
        {
            get => _base?.Rotation ?? _rotation;
            set { _rotation = value; if (_base != null) _base.Rotation = value; else if (GameObject != null) GameObject.transform.rotation = value; }
        }
        public Vector3 Scale
        {
            get => _base?.Scale ?? _scale;
            set { _scale = value; if (_base != null) _base.Scale = value; else if (GameObject != null) GameObject.transform.localScale = value; }
        }
        public Player Owner { get; set; }
        public GameObject GameObject { get; set; }
        public Corpse() { }
        public Corpse(Lab.Ragdoll ragdoll, Player owner = null)
        {
            _base = ragdoll;
            Owner = owner;
            _position = ragdoll?.Position ?? Vector3.zero;
            _rotation = ragdoll?.Rotation ?? Quaternion.identity;
            _scale = ragdoll?.Scale ?? Vector3.one;
            GameObject = ragdoll?.Base?.gameObject;
        }
        public Corpse(PlayerRoles.RoleTypeId role, Vector3 position, Quaternion rotation, PlayerStatsSystem.DamageHandlerBase damageHandler, string nickname)
        {
            Position = position;
            Rotation = rotation;
            try
            {
                var ragdoll = Lab.Ragdoll.SpawnRagdoll(role, position, rotation, damageHandler, nickname, null, null, null);
                _base = ragdoll;
                GameObject = ragdoll?.Base?.gameObject;
            }
            catch
            {
                GameObject = new GameObject("Corpse");
                GameObject.transform.position = position;
                GameObject.transform.rotation = rotation;
            }
        }
        public void Destroy()
        {
            try
            {
                if (_base != null)
                {
                    Qurre.API.ShimState.UntrackCorpse(_base);
                    _base.Destroy();
                    return;
                }
            }
            catch { }
            try { UnityEngine.Object.Destroy(GameObject); } catch { }
        }
    }

    public class Locker
    {
        public Vector3 Position { get; set; }
        public GameObject GameObject { get; set; }
        public Locker() { }
        public Locker(object prefab, Vector3 position, Quaternion rotation)
        {
            Position = position;
            GameObject = new GameObject("Locker");
            GameObject.transform.position = position;
            GameObject.transform.rotation = rotation;
        }
        public Locker(Vector3 position, object prefab, Quaternion rotation) : this(prefab, position, rotation) { }
        public void Destroy() { try { UnityEngine.Object.Destroy(GameObject); } catch { } }
    }

    public class WorkStation
    {
        public Component Base { get; }
        readonly GameObject _gameObject;
        Lab.InteractableToy _toy;
        public WorkStation(Component b)
        {
            Base = b;
            _gameObject = b?.gameObject;
            Qurre.API.ShimState.WorkStations.Add(this);
            EnsureInteractable(b?.transform, Vector3.one);
        }
        public WorkStation(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            _gameObject = new GameObject("WorkStation");
            _gameObject.transform.position = position;
            _gameObject.transform.eulerAngles = rotation;
            _gameObject.transform.localScale = scale;
            Qurre.API.ShimState.WorkStations.Add(this);
            EnsureInteractable(_gameObject.transform, scale);
        }
        public static WorkStation Get(Component b) => b == null ? null : new WorkStation(b);
        public Vector3 Position => Base != null ? Base.transform.position : _gameObject != null ? _gameObject.transform.position : Vector3.zero;
        public dynamic Status { get; set; }
        public GameObject GameObject => _gameObject;

        void EnsureInteractable(Transform parent, Vector3 scale)
        {
            if (_toy != null || parent == null) return;
            try
            {
                _toy = Lab.InteractableToy.Create(Vector3.zero, Quaternion.identity, scale == default ? Vector3.one : scale, parent, true);
                _toy.Shape = AdminToys.InvisibleInteractableToy.ColliderShape.Box;
                _toy.InteractionDuration = 0f;
                _toy.OnSearching += DispatchUpdate;
                _toy.OnInteracted += DispatchInteract;
            }
            catch { }
        }

        void DispatchUpdate(Lab.Player player)
        {
            Core.Dispatch(new WorkStationUpdateEvent { Player = Player.Get(player), Station = this, Allowed = true });
        }

        void DispatchInteract(Lab.Player player)
            => Core.Dispatch(new InteractWorkStationEvent { Player = Player.Get(player), Station = this, Allowed = true });
    }

    /// <summary>Управляемый бродкаст карты (Qurre MapBroadcast) — можно менять текст на лету.</summary>
    public class MapBroadcast
    {
        public string Text { get; set; } = "";
        public string Message { get => Text; set => Text = value; }
        public ushort Duration { get; set; } = 10;
        public MapBroadcast() { }
        public MapBroadcast(string text, ushort duration = 10) { Text = text; Duration = duration; }
        public void Update(string text) => Text = text;
    }

    public static class Cassie
    {
        public static bool Lock { get; set; }
        public static void Send(string words, bool makeHold = true, bool makeNoise = true, bool customAnnouncement = true)
            => Lab.Announcer.Message(words, "", makeHold);
        public static void SendLocked(string words, bool makeHold = true, bool makeNoise = true)
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
        public static bool TryFind<T>(this List<T> list, out T result, Predicate<T> match)
            => TryFind(list, match, out result);
        public static bool TryFind<T>(this IEnumerable<T> source, Predicate<T> match, out T result)
        {
            result = source.FirstOrDefault(x => match(x));
            return result != null;
        }
        public static bool TryFind<T>(this IEnumerable<T> source, out T result, Predicate<T> match)
            => TryFind(source, match, out result);
        public static List<T> Shuffle<T>(this IEnumerable<T> source) => source.OrderBy(_ => Guid.NewGuid()).ToList();
    }

    public static class PlayerLookupExtensions
    {
        public static Qurre.API.Controllers.Player GetPlayer(this string userId) => Qurre.API.Controllers.Player.List.FirstOrDefault(x => x.UserInformation.UserId == userId);
        public static Qurre.API.Controllers.Player GetPlayer(this int playerId) => Qurre.API.Controllers.Player.List.FirstOrDefault(x => x.UserInformation.Id == playerId);
        public static Qurre.API.Controllers.Player GetPlayer(this ReferenceHub hub) => Qurre.API.Controllers.Player.Get(hub);
        public static Qurre.API.Controllers.Player GetPlayer(this GameObject go) => Qurre.API.Controllers.Player.Get(go.GetComponent<ReferenceHub>());
    }

    public static class WorldLookupExtensions
    {
        public static Qurre.API.Controllers.Room GetRoom(this Qurre.API.Objects.RoomType type)
            => Qurre.API.World.Map.Rooms.FirstOrDefault(x => Equals(x.Type, type));

        public static Qurre.API.Controllers.Door GetDoor(this Qurre.API.Objects.DoorType type)
            => Qurre.API.World.Map.Doors.FirstOrDefault(x => Equals(x.Type, type));
    }

    public static class MirrorNetworkExtensions
    {
        public static void AddObserver(this Mirror.NetworkIdentity identity, Qurre.API.Controllers.Player player)
        {
            if (identity == null || player?.Connection == null) return;
        }

        public static void AddObserver(this Mirror.NetworkIdentity identity, Mirror.NetworkConnectionToClient connection)
        {
            if (identity == null || connection == null) return;
        }

        public static void RemoveObserver(this Mirror.NetworkIdentity identity, Qurre.API.Controllers.Player player)
        {
            if (identity == null || player?.Connection == null) return;
        }

        public static void RemoveObserver(this Mirror.NetworkIdentity identity, Mirror.NetworkConnectionToClient connection)
        {
            if (identity == null || connection == null) return;
        }
    }

    public static class AmmoDictionaryExtensions
    {
        static ushort Get(Dictionary<ItemType, ushort> ammo, ItemType type)
            => ammo != null && ammo.TryGetValue(type, out var value) ? value : (ushort)0;
        static void Set(Dictionary<ItemType, ushort> ammo, ItemType type, ushort value)
        {
            if (ammo != null) ammo[type] = value;
        }

        public static ushort Ammo9(this Dictionary<ItemType, ushort> ammo) => Get(ammo, ItemType.Ammo9x19);
        public static ushort Ammo556(this Dictionary<ItemType, ushort> ammo) => Get(ammo, ItemType.Ammo556x45);
        public static ushort Ammo762(this Dictionary<ItemType, ushort> ammo) => Get(ammo, ItemType.Ammo762x39);
        public static ushort Ammo44Cal(this Dictionary<ItemType, ushort> ammo) => Get(ammo, ItemType.Ammo44cal);
        public static ushort Ammo12Gauge(this Dictionary<ItemType, ushort> ammo) => Get(ammo, ItemType.Ammo12gauge);
    }

    public static class ItemCompatExtensions
    {
        public static ItemCategory GetCategory(this ItemType type)
        {
            switch (type)
            {
                case ItemType.Ammo9x19:
                case ItemType.Ammo12gauge:
                case ItemType.Ammo44cal:
                case ItemType.Ammo556x45:
                case ItemType.Ammo762x39:
                    return ItemCategory.Ammo;

                case ItemType.GunCOM15:
                case ItemType.GunCOM18:
                case ItemType.GunCom45:
                case ItemType.GunRevolver:
                case ItemType.GunFSP9:
                case ItemType.GunCrossvec:
                case ItemType.GunE11SR:
                case ItemType.GunFRMG0:
                case ItemType.GunLogicer:
                case ItemType.GunAK:
                case ItemType.GunShotgun:
                case ItemType.GunA7:
                    return ItemCategory.Firearm;

                case ItemType.GrenadeFlash:
                case ItemType.GrenadeHE:
                    return ItemCategory.Grenade;

                case ItemType.ArmorLight:
                case ItemType.ArmorCombat:
                case ItemType.ArmorHeavy:
                    return ItemCategory.Armor;

                case ItemType.Radio:
                    return ItemCategory.Radio;

                case ItemType.Medkit:
                case ItemType.Painkillers:
                case ItemType.Adrenaline:
                    return ItemCategory.Medical;

                case ItemType.SCP018:
                case ItemType.SCP1576:
                case ItemType.SCP207:
                case ItemType.SCP244a:
                case ItemType.SCP244b:
                case ItemType.SCP268:
                case ItemType.SCP330:
                case ItemType.SCP500:
                case ItemType.SCP1853:
                case ItemType.SCP2176:
                case ItemType.AntiSCP207:
                    return ItemCategory.SCPItem;

                case ItemType.KeycardJanitor:
                case ItemType.KeycardScientist:
                case ItemType.KeycardResearchCoordinator:
                case ItemType.KeycardZoneManager:
                case ItemType.KeycardGuard:
                case ItemType.KeycardMTFPrivate:
                case ItemType.KeycardContainmentEngineer:
                case ItemType.KeycardMTFOperative:
                case ItemType.KeycardMTFCaptain:
                case ItemType.KeycardFacilityManager:
                case ItemType.KeycardChaosInsurgency:
                case ItemType.KeycardO5:
                    return ItemCategory.Keycard;

                default:
                    return ItemCategory.None;
            }
        }

        public static ushort ItemSerial(this LabApi.Features.Wrappers.Item item) => item?.Serial ?? 0;

        public static void RemoveWhere<T>(this List<T> list, Predicate<T> match)
        {
            list?.RemoveAll(match);
        }

        public static void RpcShowRoundSummary(this RoundSummary summary, object classList, object escapedClassD, RoundSummary.LeadingTeam leadingTeam, int e1, int e2, int e3, int e4, int e5)
        {
        }
    }
}
