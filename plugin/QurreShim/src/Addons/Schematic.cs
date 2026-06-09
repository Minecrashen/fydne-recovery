// SchematicUnity.API — НАШ загрузчик схематик (по открытому формату), плюс базовый SObject/Scheme.
// Реализация LoadSchematic (парсинг .json + спавн тоев) — следующий этап; сейчас каркас компилируется.
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CentralAuth;
using MEC;
using Mirror;
using Newtonsoft.Json.Linq;
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

        public SObject() : this(true)
        {
        }

        protected SObject(bool createObject)
        {
            if (!createObject) return;
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
            return SchematicJsonLoader.Load(path, position, rotation);
        }
    }
}

namespace Qurre.API.Addons.Audio
{
    public class StreamAudio
    {
        public Stream Stream { get; }
        public StreamAudio(Stream stream) { Stream = stream; }
        public void ResetReadPosition()
        {
            if (Stream.CanSeek) Stream.Position = 0;
        }
        public bool IsReadEnded()
            => !Stream.CanRead || (Stream.CanSeek && Stream.Position >= Stream.Length);
        public float GetReadPercent()
            => Stream.CanSeek && Stream.Length > 0 ? Mathf.Clamp01((float)Stream.Position / Stream.Length) : 1f;
        public float EstimatedDurationSeconds()
            => Stream.CanSeek ? Mathf.Max(0.1f, Stream.Length / (48000f * sizeof(float))) : 0.1f;
        public void Close()
        {
            try { Stream.Dispose(); } catch { }
        }
    }

    /// <summary>Аудио-плеер (NVorbis/Opus поверх SpeakerToy). TODO: реализация воспроизведения.</summary>
    public class AudioPlayerBot
    {
        CoroutineHandle _coroutine;
        public string Name { get; set; } = "AudioBot";
        public float Volume { get; set; } = 100f;
        public bool IsPlaying => CurrentAudioTask != null || AudioTasks.Any(x => !x.IsDone);
        public ReferenceHub ReferenceHub { get; set; }
        public Queue<AudioTask> AudioTasks { get; } = new Queue<AudioTask>();
        public AudioTask CurrentAudioTask { get; private set; }

        public static AudioPlayerBot Create(string name = "AudioBot") => Qurre.API.Audio.CreateNewAudioPlayer(name, PlayerRoles.RoleTypeId.Spectator, Vector3.zero, Vector3.zero);
        public void Play(string path)
        {
            if (!File.Exists(path)) return;
            Play(new StreamAudio(new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)), VoiceChatChannel.Intercom);
        }
        public void Play(int id) { }
        public AudioTask Play(StreamAudio audio, VoiceChatChannel channel = VoiceChatChannel.Intercom)
        {
            var task = new AudioTask(audio, channel);
            AudioTasks.Enqueue(task);
            RunCoroutine();
            return task;
        }
        public void RunCoroutine()
        {
            if (!_coroutine.IsRunning)
                _coroutine = Timing.RunCoroutine(MainCoroutine(), Segment.FixedUpdate);
        }
        public IEnumerator<float> CheckPlayingAndDestroy()
        {
            while (IsPlaying)
                yield return Timing.WaitForSeconds(0.2f);
            Destroy();
        }
        public void Stop()
        {
            foreach (var task in AudioTasks) task.Skip();
            CurrentAudioTask?.Skip();
        }
        public void Destroy()
        {
            Stop();
            if (_coroutine.IsRunning) Timing.KillCoroutines(_coroutine);
            try
            {
                if (ReferenceHub != null && !ReferenceHub.isLocalPlayer)
                    NetworkServer.Destroy(ReferenceHub.gameObject);
            }
            catch { }
        }
        public void DestroySelf() => Destroy();

        IEnumerator<float> MainCoroutine()
        {
            while (AudioTasks.Count > 0)
            {
                CurrentAudioTask = AudioTasks.Dequeue();
                CurrentAudioTask.IsRunning = true;
                CurrentAudioTask.RunAt = System.DateTime.Now;
                yield return Timing.WaitForSeconds(CurrentAudioTask.Audio.EstimatedDurationSeconds());
                CurrentAudioTask.IsRunning = false;
                CurrentAudioTask.IsDone = true;
                CurrentAudioTask.Audio.Close();
                CurrentAudioTask = null;
            }
        }
    }

    public class AudioTask
    {
        static int _nextId;
        public int Id { get; } = _nextId++;
        public StreamAudio Audio { get; }
        public VoiceChatChannel VoiceChannel { get; set; }
        public AudioBlacklist Blacklist { get; } = new AudioBlacklist();
        public AudioBlacklist Whitelist { get; } = new AudioBlacklist();
        public bool IsRunning { get; internal set; }
        public bool IsDone { get; internal set; }
        public System.DateTime RunAt { get; internal set; }

        public AudioTask() { }
        public AudioTask(StreamAudio audio, VoiceChatChannel channel)
        {
            Audio = audio;
            VoiceChannel = channel;
        }

        public void Skip()
        {
            IsRunning = false;
            IsDone = true;
            Audio?.Close();
        }
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
        {
            try
            {
                if (NetworkManager.singleton?.playerPrefab == null)
                    return new AudioPlayerBot { Name = name, ReferenceHub = HostHub() };

                var botObject = UnityEngine.Object.Instantiate(NetworkManager.singleton.playerPrefab);
                var connection = new ZeroConnectionToClient();
                var hub = botObject.GetComponent<ReferenceHub>();
                NetworkServer.AddPlayerForConnection(connection, botObject);
                hub.nicknameSync.Network_myNickSync = name;
                hub.nicknameSync.Network_displayName = name;
                hub.roleManager.ServerSetRole(role, RoleChangeReason.None);

                Timing.CallDelayed(0.2f, () =>
                {
                    try { hub.characterClassManager.GodMode = true; } catch { }
                    try { hub.transform.position = position; } catch { }
                    try { hub.transform.eulerAngles = rotation; } catch { }
                });

                return new AudioPlayerBot { Name = name, ReferenceHub = hub };
            }
            catch
            {
                return new AudioPlayerBot { Name = name, ReferenceHub = HostHub() };
            }
        }

        static ReferenceHub HostHub()
        {
            ReferenceHub.TryGetHostHub(out var hub);
            return hub;
        }

        sealed class ZeroConnectionToClient : NetworkConnectionToClient
        {
            public ZeroConnectionToClient() : base(0) { }
            public override string address => "127.0.0.1";
            protected override void UpdatePing() { }
            public override void Send(System.ArraySegment<byte> segment, int channelId = 0) { }
            public override void Disconnect() { }
        }
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
