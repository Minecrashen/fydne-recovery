/* TODO update
using HarmonyLib;
using InventorySystem.Items.Firearms;
using Qurre.API;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(FirearmExtensions), nameof(FirearmExtensions.ServerSendAudioMessage))]
    static class DisableGunAudio
    {
        [HarmonyPrefix]
        static bool Call(byte clipId)
        {
            if (clipId is not 0 and not 1)
                return true;

            return !Round.Waiting;
        }
    }
}
*/