// Qurre→LabAPI shim: атрибуты.
// Сигнатуры выведены из использования в плагине:
//   [PluginInit("Loli","fydne","6.6.6")]  [PluginEnable]  [PluginDisable]
//   [EventMethod(PlayerEvents.Spawn)]  [EventMethod(PlayerEvents.Spawn, 666)]  [EventsIgnore]
using System;

namespace Qurre.API.Attributes
{
    /// <summary>Помечает класс плагина. Аналог Qurre PluginInit.</summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginInitAttribute : Attribute
    {
        public string Name { get; }
        public string Author { get; }
        public string Version { get; }

        public PluginInitAttribute(string name, string author, string version)
        {
            Name = name;
            Author = author;
            Version = version;
        }
    }

    /// <summary>Метод вызывается при включении плагина.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PluginEnableAttribute : Attribute { }

    /// <summary>Метод вызывается при выключении плагина.</summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class PluginDisableAttribute : Attribute { }

    /// <summary>
    /// Подписка метода на событие. eventType — boxed-значение одного из enum'ов Qurre.Events
    /// (object, т.к. используется с 7 разными enum-типами). priority — порядок вызова.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class EventMethodAttribute : Attribute
    {
        public object EventType { get; }
        public int Priority { get; }

        public EventMethodAttribute(object eventType, int priority = 0)
        {
            EventType = eventType;
            Priority = priority;
        }
    }

    /// <summary>Исключить класс/метод из сканирования событий.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public sealed class EventsIgnoreAttribute : Attribute { }
}
