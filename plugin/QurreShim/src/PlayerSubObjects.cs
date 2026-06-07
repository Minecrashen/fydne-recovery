// Под-объекты Qurre.API.Player → плоский LabApi.Player.
using System;
using System.Collections.Generic;
using UnityEngine;
using Lab = LabApi.Features.Wrappers;
using Qurre.API.Controllers;

namespace Qurre.API
{
    public readonly struct UserInformationW
    {
        readonly Lab.Player p;
        public UserInformationW(Lab.Player b) { p = b; }
        public string UserId => p.UserId;
        public string Nickname => p.Nickname; // LabApi: read-only (смена ника — через nicknameSync, TODO)
        public string DisplayName { get => p.DisplayName; set => p.DisplayName = value; }
        public string CustomInfo { get => p.CustomInfo; set => p.CustomInfo = value; }
        public int Id => p.PlayerId;
        public string Ip => p.IpAddress;
        public bool DoNotTrack => p.DoNotTrack;
        public uint NetId => p.NetworkId;
    }

    public readonly struct RoleInformationW
    {
        readonly Lab.Player p;
        public RoleInformationW(Lab.Player b) { p = b; }
        public PlayerRoles.RoleTypeId Role => p.Role;
        public PlayerRoles.RoleTypeId CachedRole => p.Role;
        public PlayerRoles.Team Team => p.Team;
        public PlayerRoles.Faction Faction => p.Faction;
        public bool IsAlive => p.IsAlive;
        public bool IsScp => p.IsSCP;
        public bool IsHuman => p.IsHuman;
        public void SetNew(PlayerRoles.RoleTypeId role,
                           PlayerRoles.RoleChangeReason reason = PlayerRoles.RoleChangeReason.RemoteAdmin)
            => p.SetRole(role, reason, PlayerRoles.RoleSpawnFlags.All);
    }

    public readonly struct HealthInformationW
    {
        readonly Lab.Player p;
        public HealthInformationW(Lab.Player b) { p = b; }
        public float Hp { get => p.Health; set => p.Health = value; }
        public float MaxHp { get => p.MaxHealth; set => p.MaxHealth = value; }
        public float Stamina { get => p.StaminaRemaining; set => p.StaminaRemaining = value; }
        public float Ahp { get => p.ArtificialHealth; set => p.ArtificialHealth = value; }
        public float MaxAhp => p.MaxArtificialHealth;
        public void Heal(float amount) => p.Heal(amount);
        public void Kill(string reason) => p.Kill(reason, null);
        public void Kill(PlayerStatsSystem.DeathTranslation translation)
            => p.Kill(translation.LogLabel, null); // TODO: точный DamageHandler
    }

    public readonly struct InventoryW
    {
        readonly Lab.Player p;
        public InventoryW(Lab.Player b) { p = b; }
        public InventorySystem.Inventory Base => p.ReferenceHub.inventory;
        public Dictionary<ItemType, ushort> Ammo => p.Ammo;
        public void Clear() => p.ClearInventory(true, true);
        public Lab.Item AddItem(ItemType item)
            => p.AddItem(item, InventorySystem.Items.ItemAddReason.AdminCommand);
    }

    public readonly struct MovementStateW
    {
        readonly Lab.Player p;
        public MovementStateW(Lab.Player b) { p = b; }
        public Vector3 Position { get => p.Position; set => p.Position = value; }
        public Vector3 Scale { get => p.Scale; set => p.Scale = value; }
        public Quaternion Rotation { get => p.Rotation; set => p.Rotation = value; }
        public Transform Transform => p.GameObject.transform;
    }

    public readonly struct GamePlayW
    {
        readonly Lab.Player p;
        public GamePlayW(Lab.Player b) { p = b; }
        public Room Room => Room.Get(p.Room);
        public MapGeneration.FacilityZone CurrentZone => p.Zone;
        public bool Overwatch { get => p.IsOverwatchEnabled; set => p.IsOverwatchEnabled = value; }
        public bool GodMode { get => p.IsGodModeEnabled; set => p.IsGodModeEnabled = value; }
        public bool Cuffed => p.IsDisarmed;
        public Player Cuffer => Player.Get(p.DisarmedBy);
        public Lift Lift => null; // TODO: текущий лифт игрока
    }

    public readonly struct ClientW
    {
        readonly Lab.Player p;
        public ClientW(Lab.Player b) { p = b; }
        public void Broadcast(string message, ushort duration, bool clearPrevious = false)
            => p.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
        public void Broadcast(ushort duration, string message, bool clearPrevious = false)
            => p.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
        public void SendConsole(string text, string color = "white") => p.SendConsoleMessage(text, color);
        public void ShowHint(string text, float duration = 5f) => p.SendHint(text, duration);
        public void Reconnect() => p.Reconnect(0f, false);
    }

    /// <summary>Qurre-хранилище переменных на игрока (нет аналога в LabAPI).</summary>
    public readonly struct VariablesW
    {
        readonly Player owner;
        public VariablesW(Player o) { owner = o; }
        public object this[string key]
        {
            get => owner.Vars.TryGetValue(key, out var v) ? v : null;
            set => owner.Vars[key] = value;
        }
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
}
