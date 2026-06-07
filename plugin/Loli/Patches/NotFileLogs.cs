using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(ServerLogs), nameof(ServerLogs.StartLogging))]
    internal static class NotFileLogs1
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }

    [HarmonyPatch(typeof(ServerLogs), "AddLog")]
    internal static class NotFileLogs2
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }

    [HarmonyPatch(typeof(ServerLogs), "AppendLog")]
    internal static class NotFileLogs3
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}