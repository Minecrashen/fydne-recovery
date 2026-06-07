using MEC;
using Newtonsoft.Json;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.DataBase.Modules
{
    static class Admins
    {
        static readonly HashSet<string> UserIds = new();

        static internal void Call()
        {
            Core.Socket.On("SCPServerInit", _ =>
            {
                Core.Socket.Emit("database.get.adm.steams", new object[] { });
            });

            Core.Socket.On("database.get.adm.steams", data =>
            {
                string[] userIds = JsonConvert.DeserializeObject<string[]>(data[0].ToString());

                UserIds.Clear();

                foreach (string userId in userIds)
                {
                    UserIds.Add(userId + "@steam");
                }

            });

            Timing.RunCoroutine(UpdateInterval());
            static IEnumerator<float> UpdateInterval()
            {
                while (true)
                {
                    yield return Timing.WaitForSeconds(120);
                    Core.Socket.Emit("database.get.adm.steams", new object[] { });
                }
            }
        }

        [EventMethod(PlayerEvents.CheckReserveSlot)]
        static void ReserveSlot(CheckReserveSlotEvent ev)
        {
            if (ev.Allowed)
                return;

            if (!UserIds.Contains(ev.UserId))
                return;

            ev.Allowed = true;
        }

        [EventMethod(PlayerEvents.CheckWhiteList)]
        static void WhiteListEv(CheckWhiteListEvent ev)
        {
            if (ev.Allowed)
                return;

            if (!UserIds.Contains(ev.UserId))
                return;

            ev.Allowed = true;
        }

        [EventMethod(ServerEvents.RequestPlayerListCommand)]
        static void Prefixs(RequestPlayerListCommandEvent ev)
        {
            bool gameplayData = Module.GD(ev.Sender) || CheckPerms(ev.Sender, PlayerPermissions.GameplayData);
            string text = "\n";
            foreach (Player pl in Player.List.Where(x => x != null).OrderBy(x => x.UserInformation.Id))
            {
                string nick = pl.UserInformation.Nickname.Replace("\n", string.Empty);
                string dn = pl.UserInformation.DisplayName?.Trim();
                if (dn != null && dn != nick && dn != "") nick = $"{dn}<color=#855439>*</color> ({nick})";
                string nickname = $"({pl.UserInformation.Id}) {nick}";
                if (gameplayData)
                {
                    string color = "white";

                    /*
					if (pl.GamePlay.Overwatch)
					{
						color = "#00d7ff";
						try { if (Patrol.List.Contains(pl.UserInformation.UserId)) color = "white"; } catch { }
					} else { }
					*/

                    switch (pl.RoleInformation.Role)
                    {
                        case RoleTypeId.ClassD: color = "#ff9900"; break;
                        case RoleTypeId.Scientist: color = "#e2e26d"; break;
                        case RoleTypeId.Tutorial: color = "#e134eb"; break;
                        case RoleTypeId.ChaosConscript: color = "#58be58"; break;
                        case RoleTypeId.ChaosMarauder: color = "#23be23"; break;
                        case RoleTypeId.ChaosRepressor: color = "#38ac38"; break;
                        case RoleTypeId.ChaosRifleman: color = "#1cac1c"; break;
                        case RoleTypeId.FacilityGuard: color = "#afafa1"; break;
                        case RoleTypeId.NtfPrivate: color = "#00a5ff"; break;
                        case RoleTypeId.NtfCaptain: color = "#0200ff"; break;
                        case RoleTypeId.NtfSergeant: color = "#0074ff"; break;
                        case RoleTypeId.NtfSpecialist: color = "#1f7fff"; break;
                        default:
                            {
                                if (pl.GetTeam() == Team.SCPs) color = "#ff0000";
                                break;
                            }
                    }
                    nickname = $"<color={color}>{nickname}</color>";
                }
                try
                {
                    string prefix = Module.Prefix(pl);
                    string muted = Module.CheckMuted(pl);
                    text += $"{prefix}{muted}{nickname}\n";
                }
                catch
                {
                    text += $"{nickname}\n";
                }
            }
            ev.Sender.RaReply($"$0 {text}".Replace("RA_", string.Empty), true, false, string.Empty);
            ev.Allowed = false;
            static bool CheckPerms(CommandSender commandSender, PlayerPermissions perms)
            {
                return (ServerStatic.IsDedicated && commandSender.FullPermissions) ||
                        PermissionsHandler.IsPermitted(commandSender.Permissions, perms);
            }
        }

        [EventMethod(PlayerEvents.Ban, int.MinValue)]
        static void Ban(BanEvent ev)
        {
            try
            {
                if (Data.Users.TryGetValue(ev.Issuer.UserInformation.UserId, out var _data))
                    Core.Socket.Emit("database.admin.ban", new object[] { _data.id, 1 });
            }
            catch { }

            try
            {
                if (Patrol.Verified.Contains(ev.Issuer.UserInformation.UserId))
                    return;
            }
            catch { }

            string reason = string.Empty;

            if (ev.Reason != string.Empty)
                reason = $"Причина: <color=#ff0000>{ev.Reason}</color>";

            Map.Broadcast($"<size=70%><color=#6f6f6f><color=#ff0000>{ev.Player.UserInformation.Nickname}</color> был забанен " +
                $"до <color=#ff0000>{ev.Expires:dd.MM.yyyy HH:mm}</color>. {reason}</color></size>", 15);
        }

        [EventMethod(PlayerEvents.Kick, int.MinValue)]
        static void Kick(KickEvent ev)
        {
            try
            {
                if (Data.Users.TryGetValue(ev.Issuer.UserInformation.UserId, out var _data))
                    Core.Socket.Emit("database.admin.kick", new object[] { _data.id, 1 });
            }
            catch { }

            try
            {
                if (Patrol.Verified.Contains(ev.Issuer.UserInformation.UserId))
                    return;
            }
            catch { }

            string reason = string.Empty;

            if (ev.Reason != string.Empty)
                reason = $"Причина: <color=#ff0000>{ev.Reason}</color>";

            Map.Broadcast($"<size=70%><color=#6f6f6f><color=#ff0000>{ev.Player.UserInformation.Nickname}</color> был кикнут. {reason}</color></size>", 15);
        }
    }
}