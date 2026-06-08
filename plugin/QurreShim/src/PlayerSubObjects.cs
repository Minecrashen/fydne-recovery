using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Lab = LabApi.Features.Wrappers;
using Qurre.API.Controllers;

namespace Qurre.API
{
    public class UserInformationW
    {
        readonly Lab.Player p;
        global::PlayerInfoArea _infoToShow = global::PlayerInfoArea.Nickname | global::PlayerInfoArea.Badge | global::PlayerInfoArea.CustomInfo;
        public UserInformationW(Lab.Player b) { p = b; }
        public string UserId => p.UserId;
        public string Nickname { get => p.Nickname; set { } }
        public string DisplayName { get => p.DisplayName; set => p.DisplayName = value; }
        public string CustomInfo { get => p.CustomInfo; set => p.CustomInfo = value; }
        public global::PlayerInfoArea InfoToShow { get => _infoToShow; set => _infoToShow = value; }
        public int Id => p.PlayerId;
        public string Ip => p.IpAddress;
        public bool DoNotTrack => p.DoNotTrack;
        public uint NetId => p.NetworkId;
    }

    public class RoleInformationW
    {
        readonly Lab.Player p;
        public RoleInformationW(Lab.Player b) { p = b; }
        public PlayerRoles.PlayerRoleBase Base => p.RoleBase;
        public PlayerRoles.RoleTypeId Role { get => p.Role; set => p.SetRole(value, PlayerRoles.RoleChangeReason.RemoteAdmin, PlayerRoles.RoleSpawnFlags.All); }
        public PlayerRoles.RoleTypeId CachedRole => p.Role;
        public PlayerRoles.Team Team => p.Team;
        public PlayerRoles.Faction Faction => p.Faction;
        public bool IsAlive => p.IsAlive;
        public bool IsScp => p.IsSCP;
        public bool IsHuman => p.IsHuman;
        public void SetNew(PlayerRoles.RoleTypeId role, PlayerRoles.RoleChangeReason reason = PlayerRoles.RoleChangeReason.RemoteAdmin)
            => p.SetRole(role, reason, PlayerRoles.RoleSpawnFlags.All);
        public void SetNew(PlayerRoles.RoleTypeId role, PlayerRoles.RoleChangeReason reason, PlayerRoles.RoleSpawnFlags flags)
            => p.SetRole(role, reason, flags);
        public dynamic Scp079 => null;
        public dynamic Scp106 => null;
    }

    public class HealthInformationW
    {
        readonly Lab.Player p;
        public HealthInformationW(Lab.Player b) { p = b; }
        public float Hp { get => p.Health; set => p.Health = value; }
        public float MaxHp { get => p.MaxHealth; set => p.MaxHealth = value; }
        public float Stamina { get => p.StaminaRemaining; set => p.StaminaRemaining = value; }
        public float Ahp { get => p.ArtificialHealth; set => p.ArtificialHealth = value; }
        public float MaxAhp { get => p.MaxArtificialHealth; set { } }
        public List<AhpProcess> AhpActiveProcesses { get; } = new List<AhpProcess>();
        public void Heal(float amount) => p.Heal(amount);
        public void Heal(float amount, bool _) => p.Heal(amount);
        public bool Damage(float amount, string reason = "") => p.Damage(amount, reason, null);
        public bool Damage(float amount, string reason, Player attacker) => p.Damage(amount, reason, null);
        public bool Damage(float amount, PlayerStatsSystem.DeathTranslation translation, Player attacker = null)
            => p.Damage(amount, translation.LogLabel, null);
        public void Kill(string reason) => p.Kill(reason, null);
        public void Kill(PlayerStatsSystem.DeathTranslation translation) => p.Kill(translation.LogLabel, null);
    }

    public class InventoryW
    {
        readonly Lab.Player p;
        public InventoryW(Lab.Player b) { p = b; }
        public InventorySystem.Inventory Base => p.ReferenceHub.inventory;
        public AmmoW Ammo => new AmmoW(p.Ammo);
        public Dictionary<ushort, Lab.Item> Items => p.Items?.ToDictionary(x => x.Serial, x => x) ?? new Dictionary<ushort, Lab.Item>();
        public int ItemsCount => p.Items?.Count() ?? 0;
        public void Clear() => p.ClearInventory(true, true);
        public void Reset() => Clear();
        public void Reset(bool clearAmmo) => Clear();
        public void Reset(IEnumerable<ItemType> items)
        {
            Clear();
            if (items == null) return;
            foreach (var item in items) AddItem(item);
        }
        public Lab.Item AddItem(ItemType item) => p.AddItem(item, InventorySystem.Items.ItemAddReason.AdminCommand);
        public Lab.Item AddItem(ItemType item, ushort amount) => p.AddItem(item, InventorySystem.Items.ItemAddReason.AdminCommand);
        public void RemoveItem(ushort serial) { try { p.DropItem(serial); } catch { } }
        public void RemoveItem(ushort serial, bool destroy) { try { p.DropItem(serial); } catch { } }
        public void RemoveItem(Lab.Item item) { if (item != null) RemoveItem(item.Serial); }
        public void SelectItem(ushort serial) { try { p.CurrentItem = p.Items.FirstOrDefault(x => x.Serial == serial); } catch { } }
    }

    public class AmmoW
    {
        readonly Dictionary<ItemType, ushort> ammo;
        public AmmoW(Dictionary<ItemType, ushort> source) { ammo = source; }
        ushort Get(ItemType type) => ammo != null && ammo.TryGetValue(type, out var value) ? value : (ushort)0;
        void Set(ItemType type, ushort value) { if (ammo != null) ammo[type] = value; }
        public ushort Ammo9 { get => Get(ItemType.Ammo9x19); set => Set(ItemType.Ammo9x19, value); }
        public ushort Ammo556 { get => Get(ItemType.Ammo556x45); set => Set(ItemType.Ammo556x45, value); }
        public ushort Ammo762 { get => Get(ItemType.Ammo762x39); set => Set(ItemType.Ammo762x39, value); }
        public ushort Ammo44Cal { get => Get(ItemType.Ammo44cal); set => Set(ItemType.Ammo44cal, value); }
        public ushort Ammo12Gauge { get => Get(ItemType.Ammo12gauge); set => Set(ItemType.Ammo12gauge, value); }
    }

    public class AhpProcess
    {
        public float DecayRate { get; set; }
    }

    public class MovementStateW
    {
        readonly Lab.Player p;
        public MovementStateW(Lab.Player b) { p = b; }
        public Vector3 Position { get => p.Position; set => p.Position = value; }
        public Vector3 Scale { get => p.Scale; set => p.Scale = value; }
        public Vector3 Rotation { get => p.Rotation.eulerAngles; set => p.Rotation = Quaternion.Euler(value); }
        public Transform Transform => p.GameObject.transform;
    }

    public class GamePlayW
    {
        readonly Lab.Player p;
        public GamePlayW(Lab.Player b) { p = b; }
        public Room Room => Room.Get(p.Room);
        public MapGeneration.FacilityZone CurrentZone => p.Zone;
        public bool Overwatch { get => p.IsOverwatchEnabled; set => p.IsOverwatchEnabled = value; }
        public bool GodMode { get => p.IsGodModeEnabled; set => p.IsGodModeEnabled = value; }
        public bool Cuffed => p.IsDisarmed;
        public Player Cuffer => Player.Get(p.DisarmedBy);
        public Lift Lift => null;
    }

    public class VariablesW
    {
        readonly Player owner;
        public VariablesW(Player o) { owner = o; }
        public object this[string key] { get => owner.Vars.TryGetValue(key, out var v) ? v : null; set => owner.Vars[key] = value; }
        public bool ContainsKey(string key) => owner.Vars.ContainsKey(key);
        public bool Remove(string key) => owner.Vars.Remove(key);
        public void Set(string key, object value) => owner.Vars[key] = value;
        public bool TryGetAndParse<T>(string key, out T value)
        {
            value = default;
            if (!owner.Vars.TryGetValue(key, out var v) || v == null) return false;
            if (v is T t) { value = t; return true; }
            try { value = (T)Convert.ChangeType(v, typeof(T)); return true; }
            catch { return false; }
        }
    }

    public class EffectsW
    {
        readonly Lab.Player p;
        public EffectsW(Lab.Player b) { p = b; }
        public dynamic Controller => p.ReferenceHub.playerEffectsController;
        public void Enable(Qurre.API.Objects.EffectType type, float duration = 0f, bool addDuration = false) { }
        public void Enable(CustomPlayerEffects.StatusEffectBase effect, float duration = 0f, bool addDuration = false) { }
        public void Enable<T>(float duration = 0f, bool addDuration = false) where T : CustomPlayerEffects.StatusEffectBase { }
        public void Disable(Qurre.API.Objects.EffectType type) { }
        public void Disable<T>() where T : CustomPlayerEffects.StatusEffectBase { }
        public void DisableAll() => p.DisableAllEffects();
        public bool TryGet(Qurre.API.Objects.EffectType type, out dynamic effect) { effect = null; return false; }
        public bool TryGet(Qurre.API.Objects.EffectType type, out CustomPlayerEffects.StatusEffectBase effect) { effect = null; return false; }
        public bool TryGet<T>(out T effect) where T : CustomPlayerEffects.StatusEffectBase { effect = null; return false; }
        public bool CheckActive(Qurre.API.Objects.EffectType type) => false;
        public bool CheckActive<T>() where T : CustomPlayerEffects.StatusEffectBase => false;
        public void SetIntensity(Qurre.API.Objects.EffectType type, byte intensity, float duration = 0f) { }
        public void SetIntensity<T>(byte intensity, float duration = 0f) where T : CustomPlayerEffects.StatusEffectBase { }
        public void SetFogType(CustomRendering.FogType type) { }
    }

    public class AdministrativeW
    {
        readonly Lab.Player p;
        public AdministrativeW(Lab.Player b) { p = b; }
        public bool RemoteAdminAccess { get => p.RemoteAdminAccess; set { } }
        public bool RemoteAdmin => p.RemoteAdminAccess;
        public string GroupName => p.PermissionsGroupName;
        public string RoleName { get => p.ReferenceHub.serverRoles.Network_myText ?? string.Empty; set => p.ReferenceHub.serverRoles.Network_myText = value; }
        public string RoleColor { get => p.ReferenceHub.serverRoles.Network_myColor ?? "default"; set => p.ReferenceHub.serverRoles.Network_myColor = value; }
        public ServerRoles ServerRoles => p.ReferenceHub.serverRoles;
        public void RaLogin() { }
        public void RaLogout() { }
        public void Ban(long duration, string reason = "", string issuer = "") { }
    }

    public class StatsInformationW
    {
        readonly Lab.Player p;
        public StatsInformationW(Lab.Player b) { p = b; }
        public int KillsCount { get; set; }
        public int DeathsCount { get; set; }
        public List<dynamic> Kills { get; } = new List<dynamic>();
    }
}
