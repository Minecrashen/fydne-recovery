using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mirror;
using Respawning;
using Respawning.Waves;
using Respawning.Waves.Generic;
using UnityEngine;
using Qurre.API.Addons.Audio;
using BaseIntercom = PlayerRoles.Voice.Intercom;
using BaseIntercomDisplay = PlayerRoles.Voice.IntercomDisplay;

public static class Paths
{
    public static string Plugins => Path.Combine(Environment.CurrentDirectory, "Plugins");
    public static string Logs => Path.Combine(Environment.CurrentDirectory, "Logs");
}

public static class Alpha
{
    // Флаги выставляются обвязкой EventMap из LabAPI Warhead-событий (Started/Stopped/Detonated),
    // а не этими методами — иначе при отмене старта боеголовки флаг оставался бы рассинхронен.
    public static bool Detonated { get; set; }
    public static bool Active { get; set; }

    // Раньше методы только меняли флаги, не трогая реальную боеголовку: AutoAlpha/CO2/OmegaWarhead/
    // NuclearAttack/Scp008 «запускали/останавливали» воображаемый отсчёт. Теперь дёргаем настоящий
    // AlphaWarheadController через рефлексию (тип точно есть в Assembly-CSharp; рефлексия защищает
    // от смены сигнатур между версиями SCP:SL и шумит в лог при промахе — как требует recovery-режим).
    public static void Start() { Invoke("InstantPrepare"); Invoke("StartDetonation"); }
    public static void Stop() => Invoke("CancelDetonation");
    public static void Detonate() => Invoke("Detonate");
    public static void Shake() { } // косметика, не в объёме правок

    static readonly HashSet<string> _misses = new HashSet<string>();

    static void Invoke(string methodName)
    {
        object controller = WarheadSingleton();
        if (controller == null)
        {
            LogMiss("singleton", "Qurre-shim: AlphaWarheadController.Singleton == null — Alpha." + methodName + " проигнорирован.");
            return;
        }

        var methods = controller.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.Name == methodName)
            .OrderBy(m => m.GetParameters().Length)
            .ToArray();

        var method = methods.FirstOrDefault();
        if (method == null)
        {
            LogMiss(methodName, $"Qurre-shim: метод AlphaWarheadController.{methodName} не найден — проверьте против Assembly-CSharp текущей версии.");
            return;
        }

        var pars = method.GetParameters();
        object[] args = pars.Select(p => p.HasDefaultValue
            ? p.DefaultValue
            : (p.ParameterType.IsValueType ? Activator.CreateInstance(p.ParameterType) : null)).ToArray();

        try { method.Invoke(controller, args); }
        catch (Exception ex) { LogMiss(methodName + ":ex", $"Qurre-shim: ошибка вызова AlphaWarheadController.{methodName}: {ex.InnerException?.Message ?? ex.Message}"); }
    }

    static object WarheadSingleton()
    {
        try
        {
            var type = typeof(AlphaWarheadController);
            var prop = type.GetProperty("Singleton", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null) return prop.GetValue(null);
            var field = type.GetField("Singleton", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            return field?.GetValue(null);
        }
        catch { return null; }
    }

    static void LogMiss(string key, string message)
    {
        lock (_misses)
            if (!_misses.Add(key)) return;
        Qurre.API.Log.Warn(message);
    }
}

public static class GlobalLights
{
    public static void TurnOff(float duration = 0f)
    {
        if (duration > 0f) LabApi.Features.Wrappers.Map.TurnOffLights(duration);
        else LabApi.Features.Wrappers.Map.TurnOffLights();
    }

    public static void ChangeColor(Color color, bool onlyRooms = true)
        => LabApi.Features.Wrappers.Map.SetColorOfLights(color);

    public static void ChangeColor(Color color, bool onlyRooms, bool lockChange, bool force)
        => LabApi.Features.Wrappers.Map.SetColorOfLights(color);

    public static void SetToDefault()
        => LabApi.Features.Wrappers.Map.ResetColorOfLights();

    public static void SetToDefault(bool onlyRooms, bool force)
        => LabApi.Features.Wrappers.Map.ResetColorOfLights();
}

public static class Respawn
{
    public static List<SpawnableWaveBase> SpawnableWaveBases => WaveManager.Waves;
    public static TimeBasedWave[] TimeBasedWaves => SpawnableWaveBases.OfType<TimeBasedWave>().ToArray();
    public static NtfSpawnWave NtfSpawnWave => SpawnableWaveBases.OfType<NtfSpawnWave>().FirstOrDefault();
    public static ChaosSpawnWave ChaosSpawnWave => SpawnableWaveBases.OfType<ChaosSpawnWave>().FirstOrDefault();
    public static NtfMiniWave NtfMiniWave => SpawnableWaveBases.OfType<NtfMiniWave>().FirstOrDefault();
    public static ChaosMiniWave ChaosMiniWave => SpawnableWaveBases.OfType<ChaosMiniWave>().FirstOrDefault();

    public static int? NtfTokens
    {
        get => NtfSpawnWave?.RespawnTokens;
        set { if (NtfSpawnWave != null) NtfSpawnWave.RespawnTokens = value ?? 0; }
    }

    public static int? ChaosTokens
    {
        get => ChaosSpawnWave?.RespawnTokens;
        set { if (ChaosSpawnWave != null) ChaosSpawnWave.RespawnTokens = value ?? 0; }
    }

    public static void Spawn(SpawnableWaveBase wave, bool forceSpawn = false)
    {
        if (wave == null) return;
        if (forceSpawn) WaveManager.Spawn(wave);
        else WaveManager.InitiateRespawn(wave);
    }

    public static void CallMtfHelicopter() => InvokeAnimation(NtfSpawnWave);
    public static void CallChaosCar() => InvokeAnimation(ChaosSpawnWave);

    static void InvokeAnimation(SpawnableWaveBase wave)
    {
        if (wave != null) WaveUpdateMessage.ServerSendUpdate(wave, UpdateMessageFlags.Trigger);
    }
}

public static class Decontamination
{
    public static bool InProgress { get; set; }
}

public static class Intercom
{
    static readonly FieldInfo BaseSingleton = typeof(BaseIntercom).GetField("_singleton", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    static readonly FieldInfo DisplaySingleton = typeof(BaseIntercomDisplay).GetField("_singleton", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
    static readonly FieldInfo NextTime = typeof(BaseIntercom).GetField("_nextTime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    static readonly FieldInfo CooldownTime = typeof(BaseIntercom).GetField("_cooldownTime", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
    static readonly FieldInfo DisplayText = typeof(BaseIntercomDisplay).GetField("_overrideText", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

    static BaseIntercom Base => BaseSingleton?.GetValue(null) as BaseIntercom;
    static BaseIntercomDisplay Display => DisplaySingleton?.GetValue(null) as BaseIntercomDisplay;

    public static PlayerRoles.Voice.IntercomState Status
    {
        get => BaseIntercom.State;
        set => BaseIntercom.State = value;
    }

    public static double RemainingCooldown
    {
        get => Status == PlayerRoles.Voice.IntercomState.Cooldown ? Math.Max(GetNextTime() - NetworkTime.time, 0d) : 0d;
        set => SetNextTime(NetworkTime.time + value);
    }

    public static float RechargeCooldown
    {
        get => GetFloat(CooldownTime);
        set => SetField(CooldownTime, value);
    }

    public static float SpeechRemaining
    {
        get => Base?.RemainingTime ?? 0f;
        set => SetNextTime(NetworkTime.time + value);
    }

    public static string Text
    {
        get => DisplayText?.GetValue(Display) as string ?? "";
        set
        {
            var display = Display;
            if (display != null) display.Network_overrideText = value ?? "";
        }
    }

    static double GetNextTime()
    {
        var value = NextTime?.GetValue(Base);
        return value is double number ? number : 0d;
    }

    static float GetFloat(FieldInfo field)
    {
        var value = field?.GetValue(Base);
        return value is float number ? number : 0f;
    }

    static void SetNextTime(double value) => SetField(NextTime, value);

    static void SetField(FieldInfo field, object value)
    {
        var instance = Base;
        if (field != null && instance != null) field.SetValue(instance, value);
    }
}

public static class DoorPrefabs
{
    public static readonly object DoorHCZ = new object();
    public static readonly object DoorEZ = new object();
}

public static class TargetPrefabs
{
    public static readonly object Sport = new object();
    public static readonly object Binary = new object();
    public static readonly object Dboy = new object();
}

public static class LockerPrefabs
{
    public static readonly object RifleRack = new object();
}

public static class AudioExtensions
{
    public static AudioPlayerBot PlayInIntercom(string path, string name = "AudioBot", IEnumerable<object> blacklist = null)
    {
        var bot = AudioPlayerBot.Create(name);
        bot.Play(path);
        return bot;
    }

    public static AudioPlayerBot PlayFromPlayer(string path, Qurre.API.Controllers.Player player, string name = "AudioBot")
    {
        var bot = AudioPlayerBot.Create(name);
        bot.Play(path);
        return bot;
    }

    public static AudioPlayerBot PlayFromPosition(string path, Vector3 position, string name = "AudioBot")
    {
        var bot = AudioPlayerBot.Create(name);
        bot.Play(path);
        return bot;
    }
}

public static class SCPLogs
{
    public static class Lua
    {
        public static class Globals
        {
            public static void SetGlobalVariable(string name, object value) { }
        }
    }
}

public static class CompatVoiceMutes
{
    public static HashSet<string> Mutes { get; } = new HashSet<string>();
}

namespace Loli.Patches
{
    using System.Collections.Generic;

    public static class FixSpoiled
    {
        public static HashSet<ReferenceHub> Whitelist { get; } = new HashSet<ReferenceHub>();
    }
}
