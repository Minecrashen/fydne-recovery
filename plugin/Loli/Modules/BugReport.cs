using Loli.Addons;
using Loli.Webhooks;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;

namespace Loli.Modules;

static class BugReport
{
    [EventMethod(RoundEvents.Waiting)]
    static void NullCall() { }

    static BugReport()
    {
        CommandsSystem.RegisterConsole("bug", Send);
        CommandsSystem.RegisterConsole("bag", Send);
        CommandsSystem.RegisterConsole("баг", Send);
    }

    static string LastBug = "";

    static void Send(GameConsoleCommandEvent ev)
    {
        if (ev.Name != "bug" && ev.Name != "bag" && ev.Name != "баг") return;
        ev.Allowed = false;
        if (ev.Args.Length == 0)
        {
            ev.Reply = "Вы не написали про баг";
            ev.Color = "red";
            return;
        }
        string desc = string.Join(" ", ev.Args).Trim();
        if (desc == "")
        {
            ev.Reply = "Вы не написали про баг";
            ev.Color = "red";
            return;
        }
        if (LastBug == desc)
        {
            ev.Reply = "Вы уже написали про этот баг";
            ev.Color = "red";
            return;
        }

        LastBug = desc;
        ev.Reply = "Успешно";
        ev.Color = "green";
        string hook = "https://discord.com/api/webhooks/854879128368578591/Lk8vrbbqGbfptOteXcZ8N_GpOJS2Loq_Mm6TYBYlo2NpkQGaLfwecyYjcS7EZPoXgIi7";
        new Dishook(hook).Send("", Core.ServerName, null, false, embeds: new List<Embed>()
        {
            new()
            {
                Title = "Новый баг",
                Color = 1,
                Author = new() { Name = $"{ev.Player.UserInformation.Nickname} - {ev.Player.UserInformation.UserId}" },
                Description = desc
            }
        });
    }
}