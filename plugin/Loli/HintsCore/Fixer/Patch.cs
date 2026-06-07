using HarmonyLib;
using MEC;
using Qurre.API;
using Qurre.API.Classification.Player;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.HintsCore.Fixer;

[HarmonyPatch(typeof(Client), nameof(Client.ShowHint))]
static class Patch
{
    [HarmonyPrefix]
    static bool Call(Client __instance, string text, float duration)
    {
        if (Traverse.Create(__instance).Field("_player").GetValue() is not Player pl)
            return false;

        if (!pl.Variables.TryGetAndParse(Events.Tag, out DisplayBlock block))
            return false;

        MessageBlock message = new(text, Color.white);
        block.Contents.Add(message);

        Timing.CallDelayed(duration, () => block.Contents.Remove(message));

        return false;
    }
}