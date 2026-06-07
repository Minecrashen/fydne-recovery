using Loli.HintsCore;
using Loli.HintsCore.Utils;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Addons.Hints;

static class MainInfo
{
    const string BlockTag = "Loli.Addons.Hints.MainInfo.Block";
    internal const string NameBlockTag = "Loli.Addons.Hints.MainInfo.NameBlock";
    internal const string CuffBlockTag = "Loli.Addons.Hints.MainInfo.CuffBlock";

    static internal void UpdateCuff(Player pl, Player cuffer)
    {
        if (!pl.Variables.TryGetAndParse(CuffBlockTag, out MessageBlock message))
            return;

        if (cuffer is null)
        {
            message.Content = string.Empty;
            return;
        }

        (string, string) roleInfo = cuffer.RoleInformation.Role.GetInfoRole();
        message.Content = $"🔗 <b>Вас связал: <color={roleInfo.Item2}>{cuffer.UserInformation.Nickname.OptimizeNick(10)}</color></b>";
    }

    static internal void UpdateName(Player pl, string name)
    {
        if (!pl.Variables.TryGetAndParse(NameBlockTag, out MessageBlock message))
            return;

        if (name == string.Empty)
        {
            message.Content = string.Empty;
            return;
        }

        (string, string) roleInfo = pl.RoleInformation.Role.GetInfoRole();// 🔪 🔑 ⚔️
        message.Content = $"👤 <b>Имя: <color={roleInfo.Item2}>{name}</color></b>";
    }

    static internal void UpdateMessage(Player pl, MessageBlock message, bool value, string tag)
    {
        if (!pl.Variables.TryGetAndParse(BlockTag, out DisplayBlock block))
            return;

        if (pl.Variables.TryGetAndParse(tag, out MessageBlock oldmsg))
        {
            block.Contents.Remove(oldmsg);
            pl.Variables[tag] = null;
        }

        if (message is null)
            return;

        if (value)
        {
            block.Contents.Add(message);
            pl.Variables[tag] = message;
        }
        else
        {
            block.Contents.Remove(message);
            pl.Variables[tag] = null;
        }
    }

    [EventMethod(PlayerEvents.Join, -1)]
    static void Join(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        DisplayBlock block = new(new(-3000, -510), new(400 + 420, 600), padding: new(0, 0, 0, 420), align: Align.Left, newFromTop: false);
        display.AddBlock(block);
        ev.Player.Variables[BlockTag] = block;

        MessageBlock nameBlock = new(string.Empty, new Color32(255, 112, 115, 255), "60%");
        block.Contents.Add(nameBlock);
        ev.Player.Variables[NameBlockTag] = nameBlock;

        MessageBlock cuffBlock = new(string.Empty, new Color32(255, 112, 115, 255), "60%");
        block.Contents.Add(cuffBlock);
        ev.Player.Variables[CuffBlockTag] = cuffBlock;

    }
}