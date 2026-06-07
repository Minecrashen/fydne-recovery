using Loli.HintsCore;
using Loli.HintsCore.Utils;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.Concepts.NuclearAttack;

static class HintsUi
{
    const string ProgressText = "<size=110%>[</size><cspace=-0.2em>" +
        "<color={color1}>■</color>" +
        "<color={color2}>■</color>" +
        "<color={color3}>■</color>" +
        "</cspace><size=110%>]</size>";
    const string ActivatedColor = "#ff2c19";
    const string DisabledColor = "#19ff53";

    static readonly DisplayBlock Block;
    static readonly MessageBlock ProgressBlock;
    static readonly DisplayBlock AllyBlock;

    static HintsUi()
    {
        Block = new(new(0, -370), new(450, 300));
        ProgressBlock = new(ProgressText, Color.white);
        AllyBlock = new(new(-3000, 470), new(2000, 100), padding: new(30), align: Align.Left);

        Block.Contents.Add(new("<b>Прогресс взлома для КСИР</b>", new Color32(232, 46, 46, 255), "70%"));
        Block.Contents.Add(new("_", size: "20%"));
        Block.Contents.Add(ProgressBlock);
    }

    static internal void UpdateProgress()
    {
        ProgressBlock.Content = ProgressText
            .Replace("{color1}", Builds.ActivatedHcz ? ActivatedColor : DisabledColor)
            .Replace("{color2}", Builds.ActivatedLcz ? ActivatedColor : DisabledColor)
            .Replace("{color3}", Builds.Activated ? ActivatedColor : DisabledColor);
    }

    static internal void UpdateAlly()
    {
        var list = Player.List.Where(x => x.Tag.Contains(CPIR.Tag));

        if (!list.Any())
        {
            AllyBlock.Contents.Clear();
            return;
        }

        string content = "<b>Должности бойцов КСИР: ";

        foreach (Player pl in list)
        {
            (string, string) roleInfo = pl.RoleInformation.Role.GetInfoRole();
            content += $"<color={roleInfo.Item2}>{roleInfo.Item1} ({pl.UserInformation.Nickname.OptimizeNick(7).Trim()})</color> // ";
        }

        content = content.Substring(0, content.Length - 4);
        content += "</b>";

        MessageBlock message = new(content, new Color32(255, 100, 100, 255), "70%");
        AllyBlock.Contents.Clear();
        AllyBlock.Contents.Add(message);
    }

    static internal void AddUi(Player pl)
    {
        if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.AddBlock(Block);
        display.AddBlock(AllyBlock);

        UpdateAlly();
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.RemoveBlock(Block);
        display.RemoveBlock(AllyBlock);

        UpdateAlly();
    }

    [EventMethod(RoundEvents.Waiting, int.MinValue)]
    static void Refresh()
    {
        UpdateProgress();
    }
}