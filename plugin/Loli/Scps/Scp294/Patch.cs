using HarmonyLib;
using InventorySystem.Items.Usables;
using PlayerRoles;

namespace Loli.Scps.Scp294
{
    [HarmonyPatch(typeof(Scp207), nameof(Scp207.OnEffectsActivated))]
    static class Patch
    {
        [HarmonyPrefix]
        static bool Call(Scp207 __instance)
        {
            try
            {
                if (__instance.Owner.GetTeam() == Team.SCPs)
                    return false;
            }
            catch { }
            return !__instance.TryGetDrink(out _);
        }
    }
}