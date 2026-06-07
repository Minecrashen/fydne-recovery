using HarmonyLib;
using Qurre.API.Attributes;
using Qurre.Events;
using System;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    static class FixDelayed
    {
        static DateTime Started = DateTime.MinValue;

        [HarmonyPrefix]
        static void Call(string q)
        {
            if (!q.ToLower().Contains("delayed connection incoming"))
                return;

            if (Started == DateTime.MinValue)
            {
                Started = DateTime.Now;
                return;
            }

            if ((DateTime.Now - Started).TotalSeconds > 30)
            {
                Extensions.RestartCrush("Замечена проблема с delayed connections, сервер перезапущен");
                return;
            }
        }


        [EventMethod(RoundEvents.Waiting)]
        [EventMethod(RoundEvents.Restart)]
        static void Clear()
        {
            Started = DateTime.MinValue;
        }
    }
}