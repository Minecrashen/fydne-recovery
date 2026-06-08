using System.Linq;
using HarmonyLib;
using LiteNetLib;

namespace Loli.Patches
{
#if !FYDNE_SKIP_LEGACY_PATCHES
    [HarmonyPatch(typeof(NetPacket), nameof(NetPacket.Verify))]
    static class FixNetCrash
    {
        [HarmonyPrefix]
        private static bool Call(NetPacket __instance, ref bool __result)
        {
            if (__instance.RawData.Any())
                return true;

            __result = false;
            return false;
        }
    }
#endif
}
