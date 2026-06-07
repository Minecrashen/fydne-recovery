using Loli.DataBase.Modules;
using Loli.Webhooks;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using Qurre.API.Controllers;

namespace Loli.Addons
{
    static internal class CrashProtect
    {
        static internal Dictionary<string, BansCounts> BansDict = new();

        [EventMethod(ServerEvents.RemoteAdminCommand)]
        static internal void SetGroup(RemoteAdminCommandEvent ev)
        {
            if (ev.Name == "setgroup" || ev.Name == "sg" || ev.Name == "sgroup")
            {
                ev.Allowed = false;
                new Dishook(Core.WebHooks.Protect)
                    .Send("", embeds: new List<Embed>()
                    {
                        new()
                        {
                            Color = 16711680,
                            Author = new() { Name = "Использование setgroup" },
                            Footer = new() { Text = Server.Ip + ":" + Server.Port },
                            TimeStamp = DateTimeOffset.Now,
                            Description = $"Использовал: {ev.Sender.Nickname} | {ev.Sender.SenderId}"
                        }
                    });
            }
        }


        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            BansDict.Clear();
        }

        [EventMethod(PlayerEvents.Ban)]
        static internal void AntiBan(BanEvent ev)
        {
            Method(ev.Issuer, out bool allowed);
            ev.Allowed = allowed;
        }

        [EventMethod(PlayerEvents.Kick)]
        static internal void AntiKick(KickEvent ev)
        {
            Method(ev.Issuer, out bool allowed);
            ev.Allowed = allowed;
        }
        static private void Method(Player issuer, out bool allowed)
        {
            allowed = true;
            if (issuer is null) return;
            if (issuer.IsHost) return;
            if (!BansDict.TryGetValue(issuer.UserInformation.UserId, out BansCounts cl))
            {
                cl = new();
                BansDict.Add(issuer.UserInformation.UserId, cl);
            }
            cl.Add();
            if (cl.Counts > 5)
            {
                allowed = false;
                if (!Data.Users.TryGetValue(issuer.UserInformation.UserId, out var _data))
                {
                    SendHook($"Нарушил: {issuer.UserInformation.Nickname} | {issuer.UserInformation.UserId}");
                    return;
                }
                Core.Socket.Emit("database.remove.admin", new object[] { _data.id });
                SendHook($"Нарушил: {issuer.UserInformation.Nickname} | {issuer.UserInformation.UserId} | {_data.name} (<@!{_data.discord}>)");
                issuer.Client.Disconnect("<color=red>Crash Protect</color>");
            }
            static void SendHook(string desc)
            {
                new Dishook(Core.WebHooks.Protect)
                    .Send("", embeds: new List<Embed>()
                    {
                        new()
                        {
                            Color = 16711680,
                            Author = new() { Name = "Попытка краша | Лимит банов" },
                            Footer = new() { Text = Server.Ip + ":" + Server.Port },
                            TimeStamp = DateTimeOffset.Now,
                            Description = desc
                        }
                    });
                Telegram.Send($"Попытка краша | Лимит банов\n{Server.Ip}:{Server.Port}\n{DateTimeOffset.Now}\n" + desc);
            }
        }
    }
}