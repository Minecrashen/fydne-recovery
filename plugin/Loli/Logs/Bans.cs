using System;
using System.Collections.Generic;
using System.Linq;
using Loli.DataBase;
using Loli.DataBase.Modules;
using Loli.Webhooks;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;

namespace Loli.Logs;

internal static class Bans
{
    private static readonly Dictionary<LogType, string> Hooks = new()
    {
        {
            LogType.Public,
            Environment.GetEnvironmentVariable("FYDNE_WEBHOOK_BANS_PUBLIC") ?? string.Empty
        },
        {
            LogType.Patrol,
            Environment.GetEnvironmentVariable("FYDNE_WEBHOOK_BANS_PATROL") ?? string.Empty
        },
        {
            LogType.Admin,
            Environment.GetEnvironmentVariable("FYDNE_WEBHOOK_BANS_ADMIN") ?? string.Empty
        },
        {
            LogType.Owners,
            Environment.GetEnvironmentVariable("FYDNE_WEBHOOK_BANS_OWNERS") ?? string.Empty
        },
    };

    internal static void SendHook(LogType type, bool isBan, string user, string admin, string reason,
        string expires = "", string userFull = "")
    {
        new Dishook(Hooks[type]).Send(string.Empty, Core.ServerName, null, embeds:
        [
            new Embed
            {
                Title = isBan ? "Высшая мера наказания" : "Исключение из партии",
                Color = isBan ? 16711680 : 16776960,
                Description =
                    $"**Игрок `{(!string.IsNullOrEmpty(user) ? user : "ERR")}` был {(isBan ? "отправлен в сибирь" : "изгнан")}.**\n\n" +
                    $"### Администратор:\n {admin}\n" +
                    $"### Никнейм игрока: ```{(!string.IsNullOrEmpty(userFull) ? userFull : user)}```\n" +
                    $"### Причина: ```{reason}```\n" +
                    (isBan ? $"### Наказание истекает:\n{expires}" : string.Empty),
                Footer = new EmbedFooter
                {
                    Text = Core.ServerName
                },
                TimeStamp = DateTimeOffset.Now
            }
        ]);
    }


    [EventMethod(PlayerEvents.Kick)]
    private static void Kicked(KickEvent ev)
    {
        string publicInfo = ev.Player.UserInformation.Nickname;
        string privateInfo = $"{ev.Player.UserInformation.Nickname} - {ev.Player.UserInformation.UserId}";
        string adminNick = ev.Issuer.UserInformation.Nickname;

        if (Data.Users.TryGetValue(ev.Issuer.UserInformation.UserId, out UserData data))
            adminNick = $"<@!{data.discord}> ({data.name})";

        if (Patrol.Verified.Contains(ev.Issuer.UserInformation.UserId))
        {
            SendHook(LogType.Patrol, false, publicInfo, adminNick, ev.Reason, userFull: privateInfo);

            adminNick = "Патруль";
        }

        SendHook(LogType.Admin, false, publicInfo, adminNick, ev.Reason, userFull: privateInfo);

        if (!string.IsNullOrEmpty(publicInfo))
            SendHook(LogType.Public, false, publicInfo, adminNick, ev.Reason);
    }

    [EventMethod(PlayerEvents.Banned)]
    private static void Banned(BannedEvent ev)
    {
        string publicInfo = string.Empty;
        string privateInfo;

        if (ev.Player is not null)
        {
            publicInfo = ev.Player.UserInformation.Nickname;
            privateInfo = $"{ev.Player.UserInformation.Nickname} - {ev.Player.UserInformation.UserId}";
        }
        else if (ev.Type == BanHandler.BanType.IP)
        {
            privateInfo = ev.Details.Id;
        }
        else
        {
            if (!ev.Details.OriginalName.Contains("Offline"))
                publicInfo = ev.Details.OriginalName;
            privateInfo = ev.Details.Id;
        }

        string time =
            $"<t:{new DateTimeOffset(new DateTime(ev.Details.Expires)
                .AddHours((DateTime.Now - DateTime.UtcNow).TotalHours))
                .ToUnixTimeSeconds()}:f>";
        string adminNick = ev.Details.Issuer;

        string issuer = adminNick.Split('(').Last().Replace(")", "");

        if (Data.Users.TryGetValue(issuer, out UserData data))
            adminNick = $"<@!{data.discord}> ({data.name})";

        SendHook(LogType.Owners, true, publicInfo, adminNick, ev.Details.Reason, time, privateInfo);

        if (Patrol.Verified.Contains(issuer))
        {
            SendHook(LogType.Patrol, true, publicInfo, adminNick, ev.Details.Reason, time, privateInfo);

            adminNick = "Патруль";
        }

        SendHook(LogType.Admin, true, publicInfo, adminNick, ev.Details.Reason, time, privateInfo);

        if (!string.IsNullOrEmpty(publicInfo))
            SendHook(LogType.Public, true, publicInfo, adminNick, ev.Details.Reason, time);
    }

    internal enum LogType
    {
        Public,
        Patrol,
        Admin,
        Owners,
    }
}
