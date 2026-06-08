using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Qurre.API.Addons.Audio;

public static class Paths
{
    public static string Plugins => Path.Combine(Environment.CurrentDirectory, "Plugins");
    public static string Logs => Path.Combine(Environment.CurrentDirectory, "Logs");
}

public static class Alpha
{
    public static bool Detonated { get; set; }
    public static bool Active { get; set; }
    public static void Start() => Active = true;
    public static void Stop() => Active = false;
    public static void Detonate() { Detonated = true; Active = false; }
    public static void Shake() { }
}

public static class GlobalLights
{
    public static void TurnOff(float duration = 0f) { }
    public static void ChangeColor(Color color, bool onlyRooms = true) { }
    public static void ChangeColor(Color color, bool onlyRooms, bool lockChange, bool force) { }
    public static void SetToDefault() { }
    public static void SetToDefault(bool onlyRooms, bool force) { }
}

public static class Respawn
{
    public static void CallMtfHelicopter() { }
    public static void CallChaosCar() { }
}

public static class Decontamination
{
    public static bool InProgress { get; set; }
}

public static class Intercom
{
    public static PlayerRoles.Voice.IntercomState Status { get; set; }
    public static float RemainingCooldown { get; set; }
    public static float SpeechRemaining { get; set; }
    public static string Text { get; set; } = "";
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
