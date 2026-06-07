using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Loli.Logs.Patch;

[HarmonyPatch]
static class PrintPlayer
{
    private static MethodBase TargetMethod()
    {
        Type type = AccessTools.TypeByName("SCPLogs.Extensions.EventsExtensions");
        return AccessTools.Method(type, "GetRolePrint");
    }

    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> _)
    {
        yield return new CodeInstruction(OpCodes.Ldarg_0);
        yield return new CodeInstruction(OpCodes.Call,
            AccessTools.Method(typeof(RewriteGlobals), nameof(RewriteGlobals.PlayerRole)));
        yield return new CodeInstruction(OpCodes.Ret);
    }
}