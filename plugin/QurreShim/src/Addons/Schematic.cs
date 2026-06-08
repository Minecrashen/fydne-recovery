// SchematicUnity.API — НАШ загрузчик схематик (по открытому формату), плюс базовый SObject/Scheme.
// Реализация LoadSchematic (парсинг .json + спавн тоев) — следующий этап; сейчас каркас компилируется.
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VoiceChat;

namespace SchematicUnity.API.Objects
{
    /// <summary>Базовый объект сцены (узел дерева построек).</summary>
    public class SObject
    {
        public string Name = "";
        public GameObject GameObject { get; protected set; }
        public SObject Parent;
        public List<SObject> Childrens = new List<SObject>();
        public byte MovementSmoothing = 60;
        public bool IsStatic { get; set; }
        public bool Collider { get; set; } = true;

        protected Transform Tr => GameObject != null ? GameObject.transform : null;
        public Transform Transform => Tr;
        public virtual dynamic Base => null;
        public virtual dynamic Primitive => this;
        public virtual dynamic Light => null;

        public SObject()
        {
            GameObject = new GameObject("SObject");
            Qurre.API.ShimState.SceneObjects.Add(this);
        }

        public SObject(Qurre.API.Addons.Models.Model parent, PrimitiveType type, Color color, Vector3 position,
                       Vector3 rotation, Vector3 scale, bool collidable = false) : this()
        {
            Name = "Primitive";
            Parent = parent;
            Color = color;
            Position = position;
            Rotation = rotation;
            Scale = scale;
            Collider = collidable;
            parent?.AddPart<SObject>(this);
        }

        public virtual Vector3 Position
        {
            get => Tr != null ? Tr.position : Vector3.zero;
            set { if (Tr != null) Tr.position = value; }
        }
        public virtual Vector3 Rotation
        {
            get => Tr != null ? Tr.eulerAngles : Vector3.zero;
            set { if (Tr != null) Tr.eulerAngles = value; }
        }
        public virtual Vector3 Scale
        {
            get => Tr != null ? Tr.localScale : Vector3.one;
            set { if (Tr != null) Tr.localScale = value; }
        }
        public virtual Color Color { get; set; } = Color.white;

        public virtual void Destroy()
        {
            foreach (var c in Childrens.ToArray()) c.Destroy();
            Childrens.Clear();
            Qurre.API.ShimState.SceneObjects.Remove(this);
            if (GameObject != null) Object.Destroy(GameObject);
        }
    }

    /// <summary>Загруженная схематика — дерево объектов.</summary>
    public class Scheme : SObject
    {
        public List<SObject> Objects => Childrens;
        public void Unload() => Destroy();
    }

    /// <summary>Параметры примитива.</summary>
    public struct PrimitiveParams
    {
        public dynamic Base => this;
        public PrimitiveType Type;
        public Color Color;
        public Vector3 Position;
        public Quaternion Rotation;
        public Vector3 Scale;
        public bool Collidable;
    }

    public struct LightParams
    {
        public dynamic Base => this;
        public Color Color;
        public Vector3 Position;
        public float Intensity;
        public float Range;
        public float ShadowStrength;
        public bool Shadows;
    }
}

namespace SchematicUnity.API
{
    using SchematicUnity.API.Objects;

    public static class SchematicManager
    {
        /// <summary>
        /// Загрузить схематику из .json и заспавнить. TODO: парсинг формата MapEditorReborn + спавн
        /// PrimitiveObjectToy. Сейчас — каркас (возвращает пустую Scheme), чтобы плагин собирался.
        /// </summary>
        public static Scheme LoadSchematic(string path, Vector3 position, Quaternion rotation = default)
        {
            var scheme = new Scheme();
            // TODO: if (File.Exists(path)) { parse json -> spawn toys -> scheme.Childrens }
            scheme.Position = position;
            return scheme;
        }
    }
}

namespace Qurre.API.Addons.Audio
{
    public class StreamAudio
    {
        public Stream Stream { get; }
        public StreamAudio(Stream stream) { Stream = stream; }
    }

    /// <summary>Аудио-плеер (NVorbis/Opus поверх SpeakerToy). TODO: реализация воспроизведения.</summary>
    public class AudioPlayerBot
    {
        public string Name { get; set; } = "AudioBot";
        public float Volume { get; set; } = 100f;
        public bool IsPlaying { get; private set; }
        public ReferenceHub ReferenceHub { get; set; }

        public static AudioPlayerBot Create(string name = "AudioBot") => new AudioPlayerBot { Name = name };
        public void Play(string path) { IsPlaying = true; }      // TODO
        public void Play(int id) { IsPlaying = true; }           // TODO
        public AudioTask Play(StreamAudio audio, VoiceChatChannel channel = VoiceChatChannel.Intercom)
        {
            IsPlaying = true;
            return new AudioTask();
        }
        public void RunCoroutine() { }
        public IEnumerator<float> CheckPlayingAndDestroy()
        {
            yield return 0f;
            Destroy();
        }
        public void Stop() { IsPlaying = false; }                // TODO
        public void Destroy() { IsPlaying = false; }
        public void DestroySelf() => Destroy();
    }

    public class AudioTask
    {
        public AudioBlacklist Blacklist { get; } = new AudioBlacklist();
    }

    public class AudioBlacklist
    {
        public List<object> AccessConditions { get; } = new List<object>();
    }
}

namespace Qurre.API
{
    using PlayerRoles;
    using UnityEngine;
    using Qurre.API.Addons.Audio;

    public static class Audio
    {
        public static AudioPlayerBot CreateNewAudioPlayer(string name, RoleTypeId role, Vector3 position, Vector3 rotation)
            => AudioPlayerBot.Create(name);
    }
}

namespace Qurre.API.Addons.Audio.Objects
{
    /// <summary>Условия доступа (Qurre). Используется кастомными зонами/постройками.</summary>
    public interface IAccessConditions { }
}

namespace Qurre.API.Classification.Player
{
    using Lab = LabApi.Features.Wrappers;

    /// <summary>Per-player клиент (Qurre): бродкасты, хинты, консоль, дисконнект. Патчится плагином (ShowHint).</summary>
    public class Client
    {
        readonly Qurre.API.Controllers.Player _player;
        public Client(Qurre.API.Controllers.Player player) { _player = player; }
        Lab.Player P => _player.Base;

        public void Broadcast(string message, ushort duration, bool clearPrevious = false)
            => P.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
        public void Broadcast(ushort duration, string message, bool clearPrevious = false)
            => P.SendBroadcast(message, duration, global::Broadcast.BroadcastFlags.Normal, clearPrevious);
        public void SendConsole(string text, string color = "white") => P.SendConsoleMessage(text, color);
        public void ShowHint(string text, float duration = 5f) => P.SendHint(text, duration);
        public HintDisplayProxy HintDisplay => new HintDisplayProxy(P);
        public void Reconnect() => P.Reconnect(0f, false);
        public void Disconnect(string reason = "") => P.Disconnect(reason);
        public void DimScreen() { /* TODO: затемнение экрана */ }
    }

    public class HintDisplayProxy
    {
        readonly Lab.Player _player;
        public HintDisplayProxy(Lab.Player player) { _player = player; }
        public void Show(object hint) => _player.SendHint(hint?.ToString() ?? string.Empty, 1f);
    }
}
