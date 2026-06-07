#if MRP
using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;

namespace Loli.Logs.Patch;

[HarmonyPatch]
internal static class Invisible
{
    private static MethodBase TargetMethod()
    {
        Type type = AccessTools.TypeByName("SCPLogs.Extensions.EventsExtensions");
        return AccessTools.Method(type, "SendLog");
    }

    [HarmonyPrefix]
    static bool Call(string message)
    {
        return Extensions.GetInvisibleLogs().All(userid => !message.Contains(userid));
    }
}
#endif