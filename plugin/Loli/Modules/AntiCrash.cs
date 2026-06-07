using Loli.DataBase.Modules;
using Loli.Webhooks;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Loli.Modules
{
    static class AntiCrash
    {
        [EventMethod(ServerEvents.RemoteAdminCommand)]
        static void AntiBan(RemoteAdminCommandEvent ev)
        {
            if (ev.Name != "ban")
                return;

            string[] str = ev.Args[0].Split('.');
            if (str.Count() > 3)
            {
                ev.Allowed = false;
                if (!Data.Users.TryGetValue(ev.Player.UserInformation.UserId, out var _data) ||
                    (_data.donater && !(_data.trainee || _data.helper || _data.mainhelper || _data.admin || _data.mainadmin)))
                    return;
                ev.Player.Client.Disconnect("ай, ай, ай");
                Core.Socket.Emit("database.remove.admin", new object[] { _data.id });
                new Dishook(Core.WebHooks.Protect).Send("Замечена попытка краша. В целях безопасности, админка у данного администратора снята.", Core.ServerName, null, false,
                    embeds: new List<Embed>()
                    {
                        new()
                        {
                            Title = "Попытка краша",
                            Color = 1,
                            Description = $"Попытался крашнуть:\n{ev.Player.UserInformation.Nickname} - {ev.Player.UserInformation.UserId}",
                            TimeStamp = DateTimeOffset.Now
                        }
                    });
            }
        }
    }
}