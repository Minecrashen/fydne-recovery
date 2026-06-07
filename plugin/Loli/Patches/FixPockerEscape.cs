using HarmonyLib;
using Loli.Builds.Models.Rooms;
using PlayerRoles.PlayableScps.Scp106;
using Qurre.API;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(Scp106PocketExitFinder), nameof(Scp106PocketExitFinder.GetBestExitPosition))]
    static class FixPockerEscape
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> list = new(instructions);

            int index = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_2);

            if (index < 1)
            {
                Log.Error($"Creating Patch error: [FixPockerEscape]: Index - {index} < 1");
                return list.AsEnumerable();
            }

            list.InsertRange(index + 1, new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldloc_2),

                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Range), nameof(Range.CheckPosition))),
                new CodeInstruction(OpCodes.Stloc_2),
            });

            return list.AsEnumerable();
        }
    }
}