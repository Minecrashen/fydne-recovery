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
using UnityEngine;

namespace Qurre.API
{
    public static class Core
    {
        sealed class Entry
        {
            public int Priority;
            public string Name;
            public object Key;                 // MethodInfo или Delegate — для удаления
            public Action<IBaseEvent> Invoke;  // унифицированный вызов
        }

        static readonly Dictionary<Type, List<Entry>> _byType = new Dictionary<Type, List<Entry>>();
        static readonly HashSet<string> _highRiskEvents = new HashSet<string>
        {
            nameof(RoundWaitingEvent), nameof(RoundStartEvent), nameof(RoundEndEvent),
            nameof(RoundRestartEvent), nameof(RoundForceStartEvent), nameof(RoundCheckEvent),
            nameof(JoinEvent), nameof(LeaveEvent), nameof(SpawnEvent), nameof(ChangeRoleEvent),
            nameof(RemoteAdminCommandEvent), nameof(GameConsoleCommandEvent),
            nameof(InteractDoorEvent), nameof(OpenDoorEvent), nameof(InteractLiftEvent),
            nameof(InteractGeneratorEvent), nameof(Scp106AttackEvent),
            nameof(AlphaStartEvent), nameof(AlphaStopEvent), nameof(AlphaDetonateEvent)
        };

        static readonly HashSet<string> _traceFields = new HashSet<string>
        {
            "Player", "Target", "Attacker", "Issuer", "Cuffer", "Scp", "New",
            "Allowed", "Success", "FriendlyFire", "Active", "End",
            "Reply", "Color", "Prefix", "Details", "Reason", "Name", "Message", "UserId", "Args",
            "Sender", "Role", "OldRole", "Damage", "Position", "Level", "EnragedCount", "TotalCount",
            "Door", "Winner", "Station", "Pickup", "Item", "NewItem", "OldItem", "Info", "Inventory",
            "Corpse", "DamageInfo", "Generator", "Locker", "Lift", "Tesla", "JailbirdBase", "Setting",
            "Chamber", "Consumption", "Status", "State", "LiteType", "DamageType", "Type", "Duration",
            "Intensity"
        };

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
                    if (!m.IsStatic)
                    {
                        Log.Error($"Qurre-shim: skipped non-static event handler {t.FullName}.{m.Name}");
                        continue;
                    }

                    var pars = m.GetParameters();
                    if (pars.Length > 1)
                    {
                        Log.Error($"Qurre-shim: skipped event handler with unsupported signature {t.FullName}.{m.Name}");
                        continue;
                    }

                    bool withoutArgs = pars.Length == 0;
                    Type structType = withoutArgs ? EventMap.EnumToStruct(attr.EventType) : pars[0].ParameterType;
                    if (structType == null || structType == typeof(IBaseEvent))
                        structType = EventMap.EnumToStruct(attr.EventType);
                    if (structType == null || !typeof(IBaseEvent).IsAssignableFrom(structType))
                    {
                        Log.Error($"Qurre-shim: skipped event handler with unknown event type {t.FullName}.{m.Name}");
                        continue;
                    }

                    var method = m;
                    Add(structType, new Entry
                    {
                        Priority = attr.Priority,
                        Name = $"{m.DeclaringType?.FullName}.{m.Name}",
                        Key = m,
                        Invoke = ev => method.Invoke(null, withoutArgs ? null : new object[] { ev })
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
            => Add(typeof(T), new Entry { Priority = priority, Name = Describe(method), Key = method, Invoke = ev => method((T)ev) });

        public static void ExtractEventMethod<T>(Action<T> method) where T : IBaseEvent
        {
            if (_byType.TryGetValue(typeof(T), out var list))
                list.RemoveAll(e => Equals(e.Key, (Delegate)method));
        }

        public static void InjectEventMethod(MethodInfo method, int priority = 0)
        {
            var eventType = method?.GetParameters().FirstOrDefault()?.ParameterType;
            if (eventType == null || !typeof(IBaseEvent).IsAssignableFrom(eventType)) return;
            Add(eventType, new Entry { Priority = priority, Name = $"{method.DeclaringType?.FullName}.{method.Name}", Key = method, Invoke = ev => method.Invoke(null, new object[] { ev }) });
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
            Type eventType = typeof(T);
            bool trace = ShouldTrace(eventType);

            if (_byType.TryGetValue(eventType, out var list))
            {
                if (trace)
                    Trace($"event {eventType.Name} begin handlers={list.Count} state={SnapshotLine(ev)}");

                foreach (var e in list.ToArray())
                    try
                    {
                        Dictionary<string, string> before = trace ? Snapshot(ev) : null;
                        InvokeEntry(e, ev);

                        if (trace)
                        {
                            Dictionary<string, string> after = Snapshot(ev);
                            string diff = Diff(before, after);
                            if (!string.IsNullOrEmpty(diff) || TraceEveryHandler)
                                Trace($"event {eventType.Name} handler {e.Name} prio={e.Priority} {(string.IsNullOrEmpty(diff) ? "ok" : "changed " + diff)}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Qurre-shim: handler {eventType.Name}: {ex.InnerException?.Message ?? ex.Message}");
                    }

                if (trace)
                    Trace($"event {eventType.Name} end state={SnapshotLine(ev)}");
            }
            else if (trace)
            {
                Trace($"event {eventType.Name} begin handlers=0 state={SnapshotLine(ev)}");
            }

            return ev;
        }

        static void InvokeEntry<T>(Entry entry, T ev) where T : IBaseEvent
        {
            try
            {
                entry.Invoke(ev);
            }
            catch (Exception ex)
            {
                Log.Error($"Qurre-shim: handler {typeof(T).Name}::{entry.Name ?? "<unknown>"}: {ex.InnerException?.Message ?? ex.Message}");
            }
        }

        static string Describe(Delegate method)
        {
            var info = method?.Method;
            if (info == null) return "<delegate>";
            return $"{info.DeclaringType?.FullName}.{info.Name}";
        }

        static bool ShouldTrace(Type eventType)
        {
            if (eventType == null) return false;
            if (EnvEnabled("FYDNE_TRACE_EVENTS")) return true;
            if (EnvEnabled("FYDNE_TRACE_SPAWN") && (eventType == typeof(SpawnEvent) || eventType == typeof(ChangeRoleEvent))) return true;
            return RecoveryMode && !EnvDisabled("FYDNE_TRACE_RECOVERY") && _highRiskEvents.Contains(eventType.Name);
        }

        static bool TraceEveryHandler => EnvEnabled("FYDNE_TRACE_EVERY_HANDLER");

        static bool RecoveryMode => !EnvDisabled("FYDNE_RECOVERY_MODE");

        static bool EnvEnabled(string name)
        {
            string value = Environment.GetEnvironmentVariable(name) ?? string.Empty;
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase) || value.Equals("yes", StringComparison.OrdinalIgnoreCase) || value.Equals("on", StringComparison.OrdinalIgnoreCase);
        }

        static bool EnvDisabled(string name)
        {
            string value = Environment.GetEnvironmentVariable(name) ?? string.Empty;
            return value == "0" || value.Equals("false", StringComparison.OrdinalIgnoreCase) || value.Equals("no", StringComparison.OrdinalIgnoreCase) || value.Equals("off", StringComparison.OrdinalIgnoreCase);
        }

        static Dictionary<string, string> Snapshot(object ev)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.Ordinal);
            if (ev == null) return values;

            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public;
            Type type = ev.GetType();

            foreach (FieldInfo field in type.GetFields(flags))
            {
                if (!_traceFields.Contains(field.Name)) continue;
                try { values[field.Name] = FormatValue(field.GetValue(ev)); }
                catch (Exception ex) { values[field.Name] = $"<read-failed:{ex.GetType().Name}>"; }
            }

            foreach (PropertyInfo property in type.GetProperties(flags))
            {
                if (!_traceFields.Contains(property.Name) || property.GetIndexParameters().Length != 0) continue;
                try { values[property.Name] = FormatValue(property.GetValue(ev)); }
                catch (Exception ex) { values[property.Name] = $"<read-failed:{ex.GetType().Name}>"; }
            }

            return values;
        }

        static string SnapshotLine(object ev)
        {
            Dictionary<string, string> values = Snapshot(ev);
            if (values.Count == 0) return "{}";
            return "{" + string.Join(", ", values.OrderBy(x => x.Key, StringComparer.Ordinal).Select(x => $"{x.Key}={x.Value}")) + "}";
        }

        static string Diff(Dictionary<string, string> before, Dictionary<string, string> after)
        {
            if (before == null || after == null) return string.Empty;

            List<string> changed = new List<string>();
            foreach (string key in before.Keys.Union(after.Keys).OrderBy(x => x, StringComparer.Ordinal))
            {
                before.TryGetValue(key, out string oldValue);
                after.TryGetValue(key, out string newValue);
                if (!string.Equals(oldValue, newValue, StringComparison.Ordinal))
                    changed.Add($"{key}:{oldValue}->{newValue}");
            }

            return string.Join("; ", changed);
        }

        static string FormatValue(object value)
        {
            if (value == null) return "null";

            if (value is string text) return Quote(text);
            if (value is bool boolean) return boolean ? "true" : "false";
            if (value is Vector3 vector) return $"({vector.x:0.##},{vector.y:0.##},{vector.z:0.##})";
            if (value is Quaternion rotation) return $"({rotation.eulerAngles.x:0.##},{rotation.eulerAngles.y:0.##},{rotation.eulerAngles.z:0.##})";
            if (value is Array array) return "[" + string.Join(",", array.Cast<object>().Take(8).Select(FormatValue)) + (array.Length > 8 ? ",..." : "") + "]";

            Type type = value.GetType();
            if (type.FullName != null && type.FullName.Contains("Qurre.API.Player"))
                return FormatPlayer(value);
            if (type.Name.Contains("CommandSender"))
                return FormatCommandSender(value);

            string named = TryNamed(value);
            if (!string.IsNullOrEmpty(named)) return named;

            return Truncate(value.ToString(), 96);
        }

        static string FormatPlayer(object player)
        {
            try
            {
                object user = Prop(player, "UserInformation");
                object role = Prop(player, "RoleInformation");
                object movement = Prop(player, "MovementState");

                string nickname = Prop(user, "Nickname")?.ToString() ?? "?";
                string userId = Prop(user, "UserId")?.ToString() ?? "?";
                string roleName = Prop(role, "Role")?.ToString() ?? "?";
                string position = FormatValue(Prop(movement, "Position"));

                return $"Player({Truncate(nickname, 32)}|{Truncate(userId, 48)}|{roleName}|{position})";
            }
            catch
            {
                return "Player(?)";
            }
        }

        static string FormatCommandSender(object sender)
        {
            string nickname = Prop(sender, "Nickname")?.ToString() ?? Prop(sender, "LogName")?.ToString() ?? "?";
            string senderId = Prop(sender, "SenderId")?.ToString() ?? "?";
            return $"Sender({Truncate(nickname, 32)}|{Truncate(senderId, 48)})";
        }

        static string TryNamed(object value)
        {
            object name = Prop(value, "Name") ?? Prop(value, "Type") ?? Prop(value, "Role") ?? Prop(value, "ItemTypeId");
            if (name == null) return string.Empty;
            return $"{value.GetType().Name}({Truncate(name.ToString(), 64)})";
        }

        static object Prop(object obj, string name)
        {
            if (obj == null) return null;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo property = obj.GetType().GetProperty(name, flags);
            if (property == null || property.GetIndexParameters().Length != 0) return null;
            try { return property.GetValue(obj); }
            catch { return null; }
        }

        static string Quote(string value)
        {
            return "\"" + Truncate((value ?? string.Empty).Replace("\r", "\\r").Replace("\n", "\\n"), 96) + "\"";
        }

        static string Truncate(string value, int max)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= max) return value ?? string.Empty;
            return value.Substring(0, Math.Max(0, max - 3)) + "...";
        }

        static void Trace(string message)
        {
            Log.Custom(message, "FYDNE-TRACE", ConsoleColor.DarkGray);
        }
    }
}
