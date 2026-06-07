using Loli.DataBase.Modules;
using Qurre.API;
using System;
using System.Collections.Generic;
using Qurre.API.Controllers;

namespace Loli.DataBase
{
    public static class Module
    {
        public static string CheckMuted(Player pl)
        {
            if (!VoiceChat.VoiceChatMutes.IsMuted(pl.ReferenceHub, false))
                return string.Empty;

            return "<link=RA_Muted><color=white>[</color>\ud83d\udd07<color=white>]</color></link> ";
        }

        public static string Prefix(Player pl)
        {
            if (pl.AuthManager.RemoteAdminGlobalAccess)
            {
                return "<link=RA_RaEverywhere><color=white>[<color=#EFC01A>\uf3ed</color><color=white>]</color></link> ";
            }

            if (pl.AuthManager.NorthwoodStaff)
            {
                return "<link=RA_StudioStaff><color=white>[<color=#005EBC>\uf0ad</color><color=white>]</color></link> ";
            }

            string userId = pl.UserInformation.UserId;

#if NR
            if (Prefixs.ContainsKey(userId))
            {
                if (Prefixs[userId].color == "")
                {
                    return $"[{Prefixs[userId].prefix}] ";
                }
                return $"<color={Prefixs[userId].color}>[{Prefixs[userId].prefix}]</color> ";
            }
            if (Data.Roles.TryGetValue(userId, out var roles))
            {
                if (roles.Benefactor) return "<color=#34db88>[💰]</color> ";
                if (roles.Hand) return "<color=#3498DB>[💰]</color> ";
                if (roles.Priest) return "<color=#ff9898>[💰]</color> ";
                if (roles.Star) return "<color=#f700ff>[💰]</color> ";
                if (roles.Sage) return "<color=#dc143c>[💰]</color> ";
                if (roles.Mage) return "<color=#98ff98>[💰]</color> ";
            }
#endif

            if (Data.Users.TryGetValue(userId, out var main))
            {
                if (main.id == 1 || main.anonym)
                    return string.Empty;

                string color = string.Empty;

                if (main.it) color = "#e800ff";
                else if (main.maincontrol) color = "#ffe000";
                else if (main.control) color = "#000000";
                else if (main.mainadmin) color = "#ff0000";
                else if (main.admin) color = "#fdffbb";
                else if (main.mainhelper) color = "#0089c7";
                else if (main.helper) color = "#00ffff";
                else if (main.trainee) color = "#9bff00";

                if (color != string.Empty)
                {
                    return $"<color={color}><link=Тест123>[\uf406]</link></color> ";
                }
            }
            try { if (Patrol.Verified.Contains(pl.UserInformation.UserId)) return ""; } catch { }
            if (pl.Administrative.RemoteAdmin) return "[RA] ";
            return "";
        }

        public static bool GD(CommandSender cs)
        {
            string senderId = cs.SenderId;

#if NR
            if (Prefixs.ContainsKey(senderId))
            {
                if (Prefixs[senderId].gameplay_data)
                    return true;
            }

            if (Data.Roles.TryGetValue(senderId, out var roles) && (roles.Sage || roles.Star || roles.Hand))
                return true;
#endif

            if (Patrol.Verified.Contains(senderId))
                return true;

            return false;
        }

#if NR
        public static Dictionary<string, RaPrefix> Prefixs { get; } = new();
        [Serializable]
        public class RaPrefix
        {
            public string prefix = "RA";
            public string color = "";
            public bool gameplay_data;
        }
#endif
    }

    [Serializable]
    public class UserData
    {
        public int money;
        public int xp;
        public int lvl;
        public int to;

        public bool donater = false;
        public bool trainee = false;
        public bool helper = false;
        public bool mainhelper = false;
        public bool admin = false;
        public bool mainadmin = false;
        public bool selection = false;
        public bool control = false;
        public bool maincontrol = false;
        public bool it = false;
        public int warnings = 0;

        public string prefix = "";
        public string clan = "";
        public string clanColor = "";
        public bool found = false;
        [Newtonsoft.Json.JsonIgnore]
        public bool anonym = false;
        [Newtonsoft.Json.JsonIgnore]
        public DateTime entered = DateTime.Now;
        public string name = "[data deleted]";
        public int id = 0;
        public string discord = "";
        public string login = "";

        [Newtonsoft.Json.JsonIgnore]
        public string cached_role = "";
        [Newtonsoft.Json.JsonIgnore]
        public bool isNitro = false;

        public Gradient gradient = new();
    }
    [Serializable]
    public class Gradient
    {
        public string fromA = "";
        public string toA = "";
        public string fromB = "";
        public string toB = "";
        public string prefix = "";
    }

#if NR
    public class DonateRA
    {
        public bool Force { get; internal set; } = false;
        public bool Give { get; internal set; } = false;
        public bool Effects { get; internal set; } = false;
        public bool ViewRoles { get; internal set; } = false;
    }
#endif

    [Serializable]
    public class Clan
    {
        public string Tag = "";
        public List<int> Users;
        public List<int> Boosts;
    }
#pragma warning disable IDE1006
    public class ClanUsersJson
    {
        public int[] lvl1 { get; set; }
        public int[] lvl2 { get; set; }
        public int[] lvl3 { get; set; }
        public int[] lvl4 { get; set; }
        public int[] lvl5 { get; set; }
    }
    public class ClanBoostsJson
    {
        public int[] boosts { get; set; }
        public int[] available { get; set; }
    }

#if NR
    public class BDDonateRA
    {
        public bool force { get; set; } = false;
        public bool give { get; set; } = false;
        public bool effects { get; set; } = false;
        public bool players_roles { get; set; } = false;
        public string prefix { get; set; } = "";
        public string color { get; set; } = "";
    }
#endif

    public class BDDonateRoles
    {
        public int owner { get; set; } = 0;
        public int id { get; set; } = 0;
        public int server { get; set; } = 0;
        public bool freezed { get; set; } = false;
    }
    public class SocketStatsData
    {
        public int xp { get; set; } = 0;
        public int lvl { get; set; } = 0;
        public int to { get; set; } = 0;
        public int money { get; set; } = 0;
    }
    internal class DonateRoles
    {
        internal bool Rainbow => _rainbows > 0;
        internal bool Prime => _primes > 0;
        internal bool Gradients => _gradients > 0;
        internal bool Benefactor => _benefactors > 0;

#if NR
        internal bool Priest => _priests > 0;
        internal bool Mage => _mages > 0;
        internal bool Sage => _sages > 0;
        internal bool Star => _stars > 0;
        internal bool Hand => _hand > 0;
#endif

        internal int _rainbows { get; set; } = 0;
        internal int _primes { get; set; } = 0;
        internal int _gradients { get; set; } = 0;
        internal int _benefactors { get; set; } = 0;

#if NR
        internal int _priests { get; set; } = 0;
        internal int _mages { get; set; } = 0;
        internal int _sages { get; set; } = 0;
        internal int _stars { get; set; } = 0;
        internal int _hand { get; set; } = 0;
#endif
    }
#pragma warning restore IDE1006
}