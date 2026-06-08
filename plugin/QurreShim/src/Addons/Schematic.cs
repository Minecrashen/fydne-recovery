// SchematicUnity.API — НАШ загрузчик схематик (по открытому формату), плюс базовый SObject/Scheme.
// Реализация LoadSchematic (парсинг .json + спавн тоев) — следующий этап; сейчас каркас компилируется.
using System.Collections.Generic;
using UnityEngine;

namespace SchematicUnity.API.Objects
{
    /// <summary>Базовый объект сцены (узел дерева построек).</summary>
    public abstract class SObject
    {
        public string Name = "";
        public GameObject GameObject { get; protected set; }
        public SObject Parent;
        public List<SObject> Childrens = new List<SObject>();
        public byte MovementSmoothing = 60;

        protected Transform Tr => GameObject != null ? GameObject.transform : null;

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
            if (GameObject != null) Object.Destroy(GameObject);
        }
    }

    /// <summary>Загруженная схематика — дерево объектов.</summary>
    public class Scheme : SObject
    {
        public List<SObject> Objects => Childrens;
    }

    /// <summary>Параметры примитива.</summary>
    public struct PrimitiveParams
    {
        public PrimitiveType Type;
        public Color Color;
        public Vector3 Position;
        public Vector3 Rotation;
        public Vector3 Scale;
        public bool Collidable;
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
    /// <summary>Аудио-плеер (NVorbis/Opus поверх SpeakerToy). TODO: реализация воспроизведения.</summary>
    public class AudioPlayerBot
    {
        public string Name { get; set; } = "AudioBot";
        public float Volume { get; set; } = 100f;
        public bool IsPlaying { get; private set; }

        public static AudioPlayerBot Create(string name = "AudioBot") => new AudioPlayerBot { Name = name };
        public void Play(string path) { IsPlaying = true; }      // TODO
        public void Play(int id) { IsPlaying = true; }           // TODO
        public void Stop() { IsPlaying = false; }                // TODO
        public void Destroy() { IsPlaying = false; }
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
        public void HintDisplay(string text, float duration = 5f) => P.SendHint(text, duration);
        public void Reconnect() => P.Reconnect(0f, false);
        public void Disconnect(string reason = "") => P.Disconnect(reason);
        public void DimScreen() { /* TODO: затемнение экрана */ }
    }
}
