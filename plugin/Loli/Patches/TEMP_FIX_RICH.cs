using HarmonyLib;
using Loli.HintsCore;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(Misc), nameof(Misc.SanitizeRichText))]
    public class TEMP_FIX_RICH
    {
        [HarmonyPrefix]
        private static bool Call(string content, ref string __result)
        {
            __result = Constants.ReplaceUnAllowedRegex.Replace(content, "");
            return false;
        }
    }
}