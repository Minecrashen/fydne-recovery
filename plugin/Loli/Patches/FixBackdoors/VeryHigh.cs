using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.ServerEvent;
using CommandSystem.Commands.Shared;
using HarmonyLib;
using RemoteAdmin;

namespace Loli.Patches.FixBackdoors
{
    #region Spy Mail
    [HarmonyPatch(typeof(ContactCommand), nameof(ContactCommand.Execute))]
    static class AntiSpyMyMail
    {
        [HarmonyPrefix]
        static bool Call(ICommandSender sender, out string response)
        {
            if (sender is PlayerCommandSender playerCommandSender
                && playerCommandSender.ReferenceHub.authManager.NorthwoodStaff)
            {
                response = string.Empty;
                return true;
            }

            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti RCON
    [HarmonyPatch(typeof(RconCommand), nameof(RconCommand.Execute))]
    static class AntiSpyRcon
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Spy Configs
    [HarmonyPatch(typeof(ValueCommand), nameof(ValueCommand.Execute))]
    static class AntiSpyConfigValue
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }

    [HarmonyPatch(typeof(PathCommand), nameof(PathCommand.Execute))]
    static class AntiSpyConfigPath
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Stop Next Round
    [HarmonyPatch(typeof(StopNextRoundCommand), nameof(StopNextRoundCommand.Execute))]
    static class AntiStopNextRound
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti Crush Players
    [HarmonyPatch(typeof(TerminateUnconnectedCommand), nameof(TerminateUnconnectedCommand.Execute))]
    static class AntiCrushPlayers
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion
}