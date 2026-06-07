using CommandSystem;
using CommandSystem.Commands.RemoteAdmin;
using CommandSystem.Commands.RemoteAdmin.PermissionsManagement.Group;
using CommandSystem.Commands.Shared;
using HarmonyLib;
using RemoteAdmin;

namespace Loli.Patches.FixBackdoors
{
    #region Anti Set Configs
    [HarmonyPatch(typeof(RAConfigCommand), nameof(RAConfigCommand.Execute))]
    static class AntiSetConfigs
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti Set Configs
    [HarmonyPatch(typeof(PermCommand), nameof(PermCommand.Execute))]
    static class AntiSpyMyRights
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti Public Key Players
    [HarmonyPatch(typeof(KeyCommand), nameof(KeyCommand.Execute))]
    static class AntiPublicKeyPlayers
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti Change Roles Cfg
    [HarmonyPatch(typeof(DisableCoverCommand), nameof(DisableCoverCommand.Execute))]
    static class AntiChangeRolesCfgCoverOff
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }

    [HarmonyPatch(typeof(EnableCoverCommand), nameof(EnableCoverCommand.Execute))]
    static class AntiChangeRolesCfgCoverOn
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }

    [HarmonyPatch(typeof(GrantCommand), nameof(GrantCommand.Execute))]
    static class AntiChangeRolesCfgGrant
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }

    [HarmonyPatch(typeof(RevokeCommand), nameof(RevokeCommand.Execute))]
    static class AntiChangeRolesCfgRevoke
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }

    [HarmonyPatch(typeof(SetTagCommand), nameof(SetTagCommand.Execute))]
    static class AntiChangeRolesCfgSetTag
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }

    [HarmonyPatch(typeof(SetColorCommand), nameof(SetColorCommand.Execute))]
    static class AntiChangeRolesCfgSetColor
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti Reload Commands
    [HarmonyPatch(typeof(RefreshCommandsCommand), nameof(RefreshCommandsCommand.Execute))]
    static class AntiReloadCommands
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Отказано";
            return false;
        }
    }
    #endregion

    #region Anti Check GamePlay Cfg
    [HarmonyPatch(typeof(SrvCfgCommand), nameof(SrvCfgCommand.Execute))]
    static class AntiCheckGamePlayCfg
    {
        [HarmonyPrefix]
        static bool Call(ICommandSender sender, out string response)
        {
            response = "Отказано";

            if (sender is PlayerCommandSender playerCommandSender
                && playerCommandSender.ReferenceHub.authManager.NorthwoodStaff)
                return true;

            return false;
        }
    }
    #endregion

    #region Anti Get External IP
    [HarmonyPatch(typeof(CommandSystem.Commands.Console.IpCommand), nameof(CommandSystem.Commands.Console.IpCommand.Execute))]
    static class AntiGetExternalIP
    {
        [HarmonyPrefix]
        static bool Call(out string response)
        {
            response = "Данная команда отключена";
            return false;
        }
    }
    #endregion
}