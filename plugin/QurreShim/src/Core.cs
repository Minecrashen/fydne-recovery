// Qurre.API.Core — загрузчик плагина + диспетчер событий поверх LabAPI.
// Реестр ключуется по ТИПУ event-структуры. Хендлеры приходят двумя путями:
//   1) [EventMethod(enum)] на методах (сканирование) — enum маппится на тип структуры через EventMap;
//   2) Core.InjectEventMethod<T>(Action<T>) — ручная подписка по типу T.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qurre.API.Attributes;
using Qurre.Events.Structs;

namespace Qurre.API
{
    public static class Core
    {
        sealed class Entry
        {
            public int Priority;
            public object Key;                 // MethodInfo или Delegate — для удаления
            public Action<IBaseEvent> Invoke;  // унифицированный вызов
        }

        static readonly Dictionary<Type, List<Entry>> _byType = new Dictionary<Type, List<Entry>>();

        // ---- Жизненный цикл (вызывается из QurreBootstrap) ----

        internal static void BootstrapAll()
        {
            EventMap.PopulateEnumMap();

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
                catch { continue; }

                foreach (var t in types)
                {
                    ScanType(t);
                    if (t.GetCustomAttribute<PluginInitAttribute>() != null)
                        InvokeMarked(t, typeof(PluginEnableAttribute));
                }
            }

            EventMap.WireLabApi();   // подписка на события LabAPI (наполняется инкрементами)
            Log.Custom($"Qurre-shim: реестр событий — {_byType.Count} типов, {_byType.Values.Sum(l => l.Count)} хендлеров", "Qurre", ConsoleColor.Green);
        }

        internal static void ShutdownAll()
        {
            EventMap.UnwireLabApi();
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] types;
                try { types = asm.GetTypes(); } catch { continue; }
                foreach (var t in types)
                    if (t.GetCustomAttribute<PluginInitAttribute>() != null)
                        InvokeMarked(t, typeof(PluginDisableAttribute));
            }
            _byType.Clear();
        }

        // ---- Сканирование ----

        static void ScanType(Type t)
        {
            const BindingFlags F = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            if (t.GetCustomAttribute<EventsIgnoreAttribute>() != null) return;

            foreach (var m in t.GetMethods(F))
            {
                if (m.GetCustomAttribute<EventsIgnoreAttribute>() != null) continue;
                foreach (var attr in m.GetCustomAttributes<EventMethodAttribute>())
                {
                    var pars = m.GetParameters();
                    Type structType = pars.Length == 1 ? pars[0].ParameterType : null;
                    if (structType == null || structType == typeof(IBaseEvent))
                        structType = EventMap.EnumToStruct(attr.EventType);
                    if (structType == null) continue;

                    var method = m;
                    Add(structType, new Entry
                    {
                        Priority = attr.Priority,
                        Key = m,
                        Invoke = ev => method.Invoke(method.IsStatic ? null : null, new object[] { ev })
                    });
                }
            }
        }

        static void Add(Type structType, Entry e)
        {
            if (!_byType.TryGetValue(structType, out var list))
                _byType[structType] = list = new List<Entry>();
            list.Add(e);
            list.Sort((a, b) => b.Priority.CompareTo(a.Priority)); // больший приоритет — раньше
        }

        static void InvokeMarked(Type t, Type attrType)
        {
            const BindingFlags F = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (var m in t.GetMethods(F))
                if (m.GetCustomAttributes(attrType, false).Length > 0 && m.GetParameters().Length == 0)
                    try { m.Invoke(null, null); }
                    catch (Exception ex) { Log.Error($"Qurre-shim: ошибка в {t.Name}.{m.Name}: {ex.InnerException?.Message ?? ex.Message}"); }
        }

        // ---- Публичное API для плагина ----

        public static void InjectEventMethod<T>(Action<T> method, int priority = 0) where T : IBaseEvent
            => Add(typeof(T), new Entry { Priority = priority, Key = method, Invoke = ev => method((T)ev) });

        public static void ExtractEventMethod<T>(Action<T> method) where T : IBaseEvent
        {
            if (_byType.TryGetValue(typeof(T), out var list))
                list.RemoveAll(e => Equals(e.Key, (Delegate)method));
        }

        public static void InjectEventMethod(MethodInfo method, int priority = 0)
        {
            var eventType = method?.GetParameters().FirstOrDefault()?.ParameterType;
            if (eventType == null || !typeof(IBaseEvent).IsAssignableFrom(eventType)) return;
            Add(eventType, new Entry { Priority = priority, Key = method, Invoke = ev => method.Invoke(null, new object[] { ev }) });
        }

        public static void ExtractEventMethod(MethodInfo method)
        {
            if (method == null) return;
            foreach (var list in _byType.Values)
                list.RemoveAll(e => Equals(e.Key, method));
        }

        // ---- Вызов из обвязки LabAPI ----

        public static T Dispatch<T>(T ev) where T : IBaseEvent
        {
            if (_byType.TryGetValue(typeof(T), out var list))
                foreach (var e in list.ToArray())
                    try { e.Invoke(ev); }
                    catch (Exception ex) { Log.Error($"Qurre-shim: хендлер {typeof(T).Name}: {ex.InnerException?.Message ?? ex.Message}"); }
            return ev;
        }
    }
}
