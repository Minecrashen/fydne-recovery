using Loli.Webhooks;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Loli.Logs;
using Qurre.API.Controllers;
using Qurre.API.World;

namespace Loli.Addons
{
    internal static class OfflineBan
    {
        static internal Dictionary<string, BansCounts> _bans = new();

        static OfflineBan()
        {
            CommandsSystem.RegisterRemoteAdmin("ob", OfflineBan.Send);
            CommandsSystem.RegisterRemoteAdmin("oban", OfflineBan.SendOld);
        }

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            _bans.Clear();
        }

        internal static void SendOld(RemoteAdminCommandEvent ev)
        {
            if (!(ev.Sender.SenderId == "76561198840787587@steam" || (ev.Sender.Nickname == "Dedicated Server" &&
                                                                      ev.Sender.SenderId == "")
                                                                  || ev.Sender.SenderId == "SERVER CONSOLE"))
            {
                return;
            }

            Send(ev);
        }

        internal static void Send(RemoteAdminCommandEvent ev)
        {
            ev.Allowed = false;
            ev.Prefix = "ob";
            if (!(ev.Sender.SenderId == "76561198840787587@steam" || (ev.Sender.Nickname == "Dedicated Server" && ev.Sender.SenderId == "")
                || ev.Sender.SenderId == "SERVER CONSOLE"))
            {
                ev.Reply = "Вам не разрешено использовать данную команду.";
                return;
            }
            if (ev.Args.Length < 3)
            {
                ev.Reply = "oban <userid> <длительность> <причина>\nДлительность в часах";
                return;
            }
            if (!uint.TryParse(ev.Args[1], out uint num))
            {
                ev.Reply = "Аргумент 2 должен быть действительным временем в часах: " + ev.Args[1];
                return;
            }
            try
            {
                if (!_bans.TryGetValue(ev.Sender.Nickname, out BansCounts cl))
                {
                    cl = new();
                    _bans.Add(ev.Sender.Nickname, cl);
                }
                if (cl.Counts > 10)
                {
                    new Dishook(Core.WebHooks.Protect)
                        .Send("", embeds: new List<Embed>()
                        {
                            new()
                            {
                                Color = 16711680,
                                Author = new() { Name = "Попытка краша | Превышен лимит Offline-Банов" },
                                Footer = new() { Text = Server.Ip + ":" + Server.Port },
                                TimeStamp = DateTimeOffset.Now,
                                Description = $"Нарушил: {ev.Sender.Nickname} | {ev.Sender.SenderId}"
                            }
                        });
                    Telegram.Send($"Попытка краша | Превышен лимит Offline-Банов\n" +
                        $"{Server.Ip}:{Server.Port}\n" +
                        $"{DateTimeOffset.Now}\n" +
                        $"Нарушил: {ev.Sender.Nickname} | {ev.Sender.SenderId}");
                    return;
                }
                cl.Add();
            }
            catch { }

            if (num > 999999)
                num = 999999;

            Player player = ev.Args[0].GetPlayer();
            string Reason = string.Join(" ", ev.Args.Skip(2));
            uint SecondsBan = num * 60 * 60;
            long BanExpieryTime = TimeBehaviour.GetBanExpirationTime(SecondsBan);
            long IssuanceTime = TimeBehaviour.CurrentTimestamp();
            if (player != null)
            {
                DateTime ExpireDate = DateTime.Now.AddHours(num);
                Map.Broadcast($"<size=70%><color=#6f6f6f><color=#ff0000>{player.UserInformation.Nickname}</color> был забанен " +
                    $"до <color=#ff0000> {ExpireDate:dd.MM.yyyy HH:mm}</color>. <color=#ff0000>Причина</color>: {Reason}</color></size>", 15);
                player.Administrative.Ban(SecondsBan, Reason, ev.Sender.Nickname);
                ev.Reply = $"{player.UserInformation.Nickname} успешно забанен на {ev.Args[1]} час(а/ов), причина: {Reason}";
            }
            else
            {
                IEnumerable<string> source = ev.Args[0].Split('@');
                if (source.Count() != 2)
                    ev.Reply = $"Кривой userID: {ev.Args[0]}";
                else if (!long.TryParse(source.First(), out _))
                    ev.Reply = $"Кривой userID: {source.First()}";
                else
                {
                    BanHandler.IssueBan(new BanDetails
                    {
                        Expires = BanExpieryTime,
                        Id = ev.Args[0],
                        IssuanceTime = IssuanceTime,
                        Issuer = ev.Sender.Nickname,
                        OriginalName = "Offline Ban",
                        Reason = Reason
                    }, BanHandler.BanType.UserId);

                    ev.Reply = $"{ev.Args[0]} успешно забанен на {ev.Args[1]} час(а/ов), причина: {Reason}";

                    DateTime ExpireDate = DateTime.Now.AddHours(num);
                    Map.Broadcast($"<size=70%><color=#6f6f6f><color=#ff0000>[HIDDEN]</color> был забанен " +
                                  $"до <color=#ff0000>{ExpireDate:dd.MM.yyyy HH:mm}</color>. <color=#ff0000>Причина</color>: {Reason}\noffline ban</color></size>",
                        15);
                    string time = $"<t:{new DateTimeOffset(ExpireDate).ToUnixTimeSeconds()}:f>";
                    Bans.SendHook(Bans.LogType.Admin, true, ev.Args[0], ev.Sender.Nickname, Reason, time);
                }
            }
        }
    }
}