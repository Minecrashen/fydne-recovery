using System;
using System.Diagnostics.CodeAnalysis;

#if MRP
using Loli.Addons.RolePlay;
#endif

using Loli.Concepts.NuclearAttack;
using Loli.Concepts.Scp008;
using Loli.DataBase;
using Loli.DataBase.Modules;
using PlayerRoles;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;

namespace Loli.Logs;

internal static class RewriteGlobals
{
    [SuppressMessage("CodeQuality", "IDE0051")]
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    [EventMethod(RoundEvents.Waiting)]
    private static void NullCall()
    {
    }

    static RewriteGlobals()
    {
        SCPLogs.Lua.Globals.SetGlobalVariable("PrintDiscord", (object)PrintDiscord);
        SCPLogs.Lua.Globals.SetGlobalVariable("IsAdmin", (object)IsAdmin);
        SCPLogs.Lua.Globals.SetGlobalVariable("IsPatrol", (object)IsPatrol);
        SCPLogs.Lua.Globals.SetGlobalVariable("IsSpy", (object)IsSpy);
        SCPLogs.Lua.Globals.SetGlobalVariable("PlayerRole", (object)PlayerRole);
        SCPLogs.Lua.Globals.SetGlobalVariable("LongTimeToSeconds", (object)LongTimeToSeconds);
    }


    private static string PrintDiscord(Player player)
    {
        if (Data.Users.TryGetValue(player.UserInformation.UserId, out UserData data))
            return $"<@!{data.discord}>";

        return "`" + player.UserInformation.Nickname + "`";
    }

    private static bool IsAdmin(Player player)
    {
        return player.ItsAdmin(false);
    }

    private static bool IsPatrol(Player player)
    {
        return Patrol.Verified.Contains(player.UserInformation.UserId);
    }

    private static bool IsSpy(Player player)
    {
        return player.ItsSpyFacilityManager() || player.Tag.Contains(CPIR.Tag);
    }

    internal static string PlayerRole(Player player)
    {
#if MRP
        if (player.ItsSpyFacilityManager())
        {
            return "Менеджер Комплекса [Шпион]";
        }
#endif

        if (player.ItsHacker())
        {
            return "Хакер";
        }

        if (player.Tag.Contains(SerpentsHand.Tag))
        {
            return "Длань Змея";
        }

#if MRP
        if (player.Tag.Contains(FacilityManager.Tag))
        {
            return "Менеджер Комплекса";
        }
#endif

        if (player.UserInformation.CustomInfo.Contains("Начальник СБ"))
        {
            return "Начальник СБ";
        }

        string unit = string.Empty;

        if (player.Variables.ContainsKey("UNIT"))
            unit = $" ({player.Variables["UNIT"]})";

        RoleTypeId roleType = player.RoleInformation.Role;

        if (roleType is RoleTypeId.Spectator or RoleTypeId.None)
            roleType = player.RoleInformation.CachedRole;

        (string role, _) = roleType.GetInfoRole();

        if (player.Tag.Contains(CPIR.Tag))
        {
            return "Шпион КСИР - " + role;
        }

        return role + unit;
    }

    internal static string LongTimeToSeconds(long expires)
    {
        return $"<t:{new DateTimeOffset(new DateTime(expires)).ToUnixTimeSeconds()}:f>";
    }
}