using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;

namespace Loli.Addons
{
    static class CommandsSystem
    {
        static readonly Dictionary<string, Action<GameConsoleCommandEvent>> _consoles = new();
        static readonly Dictionary<string, Action<RemoteAdminCommandEvent>> _ras = new();

        static internal void RegisterConsole(string command, Action<GameConsoleCommandEvent> action)
        {
            if (_consoles.ContainsKey(command)) throw new Exception($"Console command \"${command}\" already exist");
            _consoles.Add(command, action);
        }
        static internal void RegisterRemoteAdmin(string command, Action<RemoteAdminCommandEvent> action)
        {
            if (_ras.ContainsKey(command)) throw new Exception($"Console command \"${command}\" already exist");
            _ras.Add(command, action);
        }

#if MRP
        [EventMethod(ServerEvents.GameConsoleCommand)]
#elif NR
        [EventMethod(ServerEvents.GameConsoleCommand, 1)]
#endif
        static internal void ConsoleInvoke(GameConsoleCommandEvent ev)
        {
            if (!_consoles.TryGetValue(ev.Name, out var action)) return;
            action(ev);
        }

#if MRP
        [EventMethod(ServerEvents.RemoteAdminCommand)]
#elif NR
        [EventMethod(ServerEvents.RemoteAdminCommand, 1)]
#endif
        static internal void RAInvoke(RemoteAdminCommandEvent ev)
        {
            if (!_ras.TryGetValue(ev.Name, out var action)) return;
            action(ev);
        }
    }
}