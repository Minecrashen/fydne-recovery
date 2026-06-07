using CommandSystem;
using HarmonyLib;
using Loli.DataBase.Modules;
using RemoteAdmin;
using System;
using System.Linq;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(Misc), nameof(Misc.CheckPermission), new Type[] { typeof(ICommandSender), typeof(PlayerPermissions[]) })]
    static class PatrolGetBans_TEMPLATE
    {
        static bool Prefix(ICommandSender sender, PlayerPermissions[] perms, out bool __result)
        {
            __result = false;

            CommandSender commandSender = sender as CommandSender;

            if (commandSender is not null)
            {
                if (perms.Any(x => x is PlayerPermissions.LongTermBanning or PlayerPermissions.BanningUpToDay or PlayerPermissions.KickingAndShortTermBanning) && Patrol.Verified.Contains(commandSender.SenderId))
                    __result = true;
                else
                    __result = PermissionsHandler.IsPermitted(commandSender.Permissions, perms);
            }

            return false;
        }
    }


    [HarmonyPatch(typeof(Misc), nameof(Misc.CheckPermission), new Type[] { typeof(ICommandSender), typeof(PlayerPermissions) })]
    static class PatrolGetBans_TEMPLATE2
    {
        static bool Prefix(ICommandSender sender, PlayerPermissions perm, out bool __result)
        {
            __result = false;

            CommandSender commandSender = sender as CommandSender;

            if (commandSender is not null)
            {
                if (perm is PlayerPermissions.LongTermBanning or PlayerPermissions.BanningUpToDay or PlayerPermissions.KickingAndShortTermBanning && Patrol.Verified.Contains(commandSender.SenderId))
                    __result = true;
                else
                    __result = PermissionsHandler.IsPermitted(commandSender.Permissions, perm);
            }

            return false;
        }
    }


    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.CheckPermissions), new Type[] { typeof(CommandSender), typeof(PlayerPermissions) })]
    static class PatrolGetBans_TEMPLATE3
    {
        static bool Prefix(CommandSender sender, PlayerPermissions perm, out bool __result)
        {
            if (perm is PlayerPermissions.PlayerSensitiveDataAccess && Patrol.Verified.Contains(sender.SenderId))
                __result = true;
            else
                __result = PermissionsHandler.IsPermitted(sender.Permissions, perm);

            return false;
        }
    }
}