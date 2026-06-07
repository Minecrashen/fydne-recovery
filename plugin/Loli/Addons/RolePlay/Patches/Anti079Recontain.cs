#if MRP
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Loli.Addons.RolePlay.Patches
{
    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.OnServerRoleChanged))]
    static class Anti079Recontain
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}
#endif