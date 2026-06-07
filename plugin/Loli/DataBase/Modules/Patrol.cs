using Loli.Addons;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;

#if NR
using Loli.Webhooks;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Threading;
#endif

namespace Loli.DataBase.Modules
{
    internal static class Patrol
    {
        internal static List<string> Verified = new();
        internal static void Init()
        {
            Core.Socket.On("database.get.patrol", obj =>
            {
                string userid = obj[1].ToString();
                var pl = userid.GetPlayer();

                if (pl is null)
                    return;

                if (!(bool)obj[2])
                {
                    if (Verified.Contains(pl.UserInformation.UserId))
                        Verified.Remove(pl.UserInformation.UserId);
                }
                else
                {
                    if (!Verified.Contains(pl.UserInformation.UserId))
                        Verified.Add(pl.UserInformation.UserId);

                    pl.Administrative.RaLogin();
                }
            });

            CommandsSystem.RegisterRemoteAdmin("bring", Bring);
            CommandsSystem.RegisterRemoteAdmin("ban", Ban);

            static void Ban(RemoteAdminCommandEvent ev)
            {
                if (!Verified.Contains(ev.Sender.SenderId))
                    return;

                ev.Allowed = false;

                if (!int.TryParse(ev.Args[1], out int dur))
                {
                    ev.Reply = "Неверно указано время";
                    return;
                }

                var ids = ev.Args[0].Split('.');
                List<Player> pls = new();
                foreach (var id in ids)
                {
                    try
                    {
                        var pl = int.Parse(id).GetPlayer();
                        if (pl is not null) pls.Add(pl);
                    }
                    catch { }
                }

                if (pls.Count == 0)
                {
                    ev.Reply = "Игроки не выбраны";
                    return;
                }

                string reason = string.Join(" ", ev.Args.Skip(2));
                if (reason.Length == 0)
                {
                    ev.Reply = "Отсутствует причина бана";
                    return;
                }

                reason += " - Бан от патруля";

                if (pls.Count > 3)
                {
                    ev.Reply = "ай, ай, ай";
                    return;
                }

                foreach (var pl in pls)
                {
                    pl.Administrative.Ban(dur * 60, reason, $"Патруль ({ev.Sender.SenderId})");
                }

                ev.Success = true;
            }

            static void Bring(RemoteAdminCommandEvent ev)
            {
                if (!ev.Allowed)
                    return;

                if (!Verified.Contains(ev.Sender.SenderId))
                    return;

                string Arg0 = ev.Args.Length > 0 ? ev.Args[0].ToLower() : string.Empty;

                var pls = Arg0.Split('.');
                foreach (var id in pls)
                {
                    try
                    {
                        Player pl = id.GetPlayer();
                        pl.MovementState.Position = ev.Player.MovementState.Position;
                    }
                    catch (Exception err)
                    {
                        ev.Player.Client.SendConsole($"Произошла ошибка при bring {id}: {err}", "red");
                    }
                }

                ev.Allowed = false;
                ev.Reply = "Успешно";
                ev.Success = true;
            }
        }


        [EventMethod(ServerEvents.RemoteAdminCommand, 3)]
        static void Force(RemoteAdminCommandEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Name != "forceclass")
                return;

            bool list2 = Verified.Contains(ev.Sender.SenderId);

            if (!list2)
                return;

            string Arg0 = ev.Args.Length > 0 ? ev.Args[0].ToLower() : string.Empty;
            string Arg1 = ev.Args.Length > 1 ? ev.Args[1].ToLower() : string.Empty;

            if (Arg0 == "spectator")
            {
                ev.Player.RoleInformation.SetNew(RoleTypeId.Spectator, RoleChangeReason.RemoteAdmin);
                goto IL_1;
            }

            if (Arg0 == "overwatch" || Arg1 == "overwatch")
            {
                ev.Player.RoleInformation.SetNew(RoleTypeId.Overwatch, RoleChangeReason.RemoteAdmin);
                goto IL_1;
            }

            if (Arg0 == "tutorial")
            {
                ev.Player.RoleInformation.SetNew(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
                goto IL_1;
            }

            if (Arg1 == "tutorial")
            {
                var pls = Arg0.Split('.');
                foreach (var id in pls)
                {
                    try
                    {
                        if (!int.TryParse(id.Replace(".", ""), out int parsed_id))
                        {
                            ev.Player.Client.SendConsole($"Произошла ошибка при спавне {{парсинг int}} {id}", "red");
                            continue;
                        }

                        Player pl = parsed_id.GetPlayer();
                        pl.RoleInformation.SetNew(RoleTypeId.Tutorial, RoleChangeReason.RemoteAdmin);
                    }
                    catch (Exception err)
                    {
                        ev.Player.Client.SendConsole($"Произошла ошибка при спавне {id}: {err}", "red");
                    }
                }
                goto IL_1;
            }

#if MRP
            if (Arg0 == "scp3114" || Arg1 == "scp3114")
            {
                ev.Player.RoleInformation.SetNew(RoleTypeId.Scp3114, RoleChangeReason.RemoteAdmin);
                goto IL_1;
            }
#elif NR
            if (Arg0 == "scp0492" || Arg1 == "scp0492")
            {
                ev.Player.RoleInformation.SetNew(RoleTypeId.Scp0492, RoleChangeReason.RemoteAdmin);
                goto IL_1;
            }
#endif


            if (Arg1 == "spectator")
            {
                var pls = Arg0.Split('.');
                foreach (var id in pls)
                {
                    try
                    {
                        if (!int.TryParse(id.Replace(".", ""), out int parsed_id))
                        {
                            ev.Player.Client.SendConsole($"Произошла ошибка при спавне {{парсинг int}} {id}", "red");
                            continue;
                        }

                        Player pl = parsed_id.GetPlayer();
                        if (pl.RoleInformation.Role is RoleTypeId.Tutorial or RoleTypeId.Scp0492)
                        {
                            pl.RoleInformation.SetNew(RoleTypeId.Spectator, RoleChangeReason.RemoteAdmin);
                        }
                    }
                    catch (Exception err)
                    {
                        ev.Player.Client.SendConsole($"Произошла ошибка при спавне {id}: {err}", "red");
                    }
                }
                goto IL_1;
            }

            return;

        IL_1:
            {
                ev.Allowed = false;
                ev.Reply = "Успешно";
                ev.Success = true;
            }
        }

#pragma warning disable CS0649
        [Serializable]
        class SteamMainInfoApi
        {
            public string personaname;
            public string avatar;
        }
#pragma warning restore CS0649
    }
}