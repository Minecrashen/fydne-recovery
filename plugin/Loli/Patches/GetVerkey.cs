using HarmonyLib;

namespace Loli.Patches;

[HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.RefreshToken))]
static class GetVerkey
{
    [HarmonyPrefix]
    static bool Call()
    {
        string token = Core.ConfigsCore.SafeGetValue("verkey", "default");

        if (token == "default")
            return true;

        ServerConsole.ScheduleTokenRefresh = false;
        if (ServerConsole.Password != token)
        {
            ServerConsole.AddLog("Verification token reloaded.");
            ServerConsole.Update = true;
        }
        ServerConsole.Password = token;
        CustomNetworkManager.IsVerified = true;
        return false;
    }
}