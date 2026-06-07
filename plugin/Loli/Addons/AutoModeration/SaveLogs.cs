using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;

namespace Loli.Addons.AutoModeration;

[HarmonyPatch]
internal static class SaveLogs
{
    [UsedImplicitly]
    private static MethodBase TargetMethod()
    {
        Type type = AccessTools.TypeByName("SCPLogs.Extensions.EventsExtensions");
        return AccessTools.Method(type, "SendLog");
    }

    [HarmonyTranspiler, UsedImplicitly]
    private static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> list = [.. instructions];

        list.InsertRange(list.Count - 1, [
            new CodeInstruction(OpCodes.Ldloc_0),
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(SaveLogs), nameof(Invoke)))
        ]);

        return list.AsEnumerable();
    }

    private static void Invoke(string time, string message)
    {
        Executor.Messages.Add(time + message);
    }
}