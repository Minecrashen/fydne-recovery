using HarmonyLib;
using PlayerRoles.PlayableScps.Scp079;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Loli.Modules.Voices
{
    // [v14 DRIFT] Тип NineTailedFoxAnnouncer удалён из игры (миграция Cassie -> Announcer).
    // Отключение Cassie о смерти SCP переписать на событие LabAPI
    // ServerEvents.CassieQueuingScpTermination (ev.IsAllowed = false). Патч отключён.
    /*
    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.Update))]
    static class PATCH_DisableCassie_SCPDeath
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
    */

    [HarmonyPatch(typeof(Scp079Recontainer), nameof(Scp079Recontainer.PlayAnnouncement))]
    static class PATCH_DisableCassie_Overcharge
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}