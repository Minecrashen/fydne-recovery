using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace Loli.Patches;

#if !FYDNE_SKIP_LEGACY_PATCHES
[HarmonyPatch(typeof(DeadmanSwitch), nameof(DeadmanSwitch.OnUpdate))]
static class DeleteDeadman
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
    {
        yield return new CodeInstruction(OpCodes.Ret);
    }
}
#endif
