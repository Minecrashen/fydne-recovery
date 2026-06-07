#if MRP
using HarmonyLib;
using PlayerRoles.PlayableScps.Scp173;

namespace Loli.Addons.RolePlay.Patches
{
    [HarmonyPatch(typeof(Scp173BlinkTimer), nameof(Scp173BlinkTimer.AbilityReady), MethodType.Getter)]
    static class Scp173Teleport
    {
        [HarmonyPrefix]
        static bool Call(Scp173BlinkTimer __instance, ref bool __result)
        {
            if (__instance.Role is not Scp173Role role)
                return true;

            if (!role.SubroutineModule.TryGetSubroutine<Scp173ObserversTracker>(out var observers))
                return true;

            if (observers.CurrentObservers < 4)
                return true;

            __result = false;
            return false;
        }
    }
}
#endif