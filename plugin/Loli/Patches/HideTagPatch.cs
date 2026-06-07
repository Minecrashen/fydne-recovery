using HarmonyLib;
using Loli.DataBase;
using Loli.DataBase.Modules;
using Qurre.API;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.TryHideTag))]
    static class HideTagPatch
    {
        [HarmonyPrefix]
        static bool Call(ServerRoles __instance)
        {
            if (__instance._hub.authManager.NorthwoodStaff)
                return true;
            try
            {
                var pl = __instance._hub.authManager.UserId.GetPlayer();
                if (pl.Administrative.ServerRoles.NetworkGlobalBadge != "")
                {
                    pl.Administrative.ServerRoles.NetworkGlobalBadge = "";
                    Levels.SetPrefix(pl);
                    pl.Client.SendConsole("Успешно", "green");
                }
                else
                {
                    if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var data) &&
                        (data.trainee || data.helper || data.mainhelper || data.admin || data.mainadmin ||
                        data.control || data.maincontrol || data.it))
                    {
                        data.anonym = true;
                        Levels.SetPrefix(pl);
                        pl.Client.SendConsole("Успешно", "green");
                    }
                    else pl.Client.SendConsole("Зачем тебе убирать префикс?", "red");
                }
            }
            catch
            {
                __instance._hub.gameConsoleTransmission.SendMessage("Зачем тебе убирать префикс?", "red");
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.RefreshLocalTag))]
    static class ShowTagPatch
    {
        [HarmonyPostfix]
        static void Call(ServerRoles __instance)
        {
            if (__instance._hub.authManager.NorthwoodStaff)
                return;
            try
            {
                var pl = __instance._hub.authManager.UserId.GetPlayer();

                if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var data) &&
                    (data.trainee || data.helper || data.mainhelper || data.admin || data.mainadmin ||
                    data.control || data.maincontrol || data.it))
                {
                    data.anonym = false;
                    pl.Client.SendConsole("Успешно", "green");
                }

                Levels.SetPrefix(pl);
            }
            catch { }
        }
    }
}