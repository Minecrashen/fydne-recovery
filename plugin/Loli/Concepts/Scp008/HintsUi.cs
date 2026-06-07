using Loli.HintsCore;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Concepts.Scp008;

static class HintsUi
{
    const string ProgressText = "<size=110%>[</size><cspace=-0.2em>" +
        "<color={color1}>■</color>" +
        "<color={color2}>■</color>" +
        "<color={color3}><size=120%>■</size></color>" +
        "<color={color4}>■</color>" +
        "<color={color5}>■</color>" +
        "</cspace><size=110%>]</size>";
    const string ActivatedColor = "#ff2c19";
    const string DisabledColor = "#19ff53";

    static readonly DisplayBlock Block;
    static readonly MessageBlock ProgressBlock;

    static HintsUi()
    {
        Block = new(new(0, -370), new(450, 300));
        ProgressBlock = new(ProgressText, Color.white);

        Block.Contents.Add(new("<b>Прогресс активации SCP-008</b>", new Color32(41, 148, 230, 255), "70%"));
        Block.Contents.Add(new("_", size: "20%"));
        Block.Contents.Add(ProgressBlock);
    }

    static internal void UpdateProgress()
    {
        ProgressBlock.Content = ProgressText
            .Replace("{color1}", RoomsData.Lcz173.Activated ? ActivatedColor : DisabledColor)
            .Replace("{color2}", RoomsData.Hcz049.Activated ? ActivatedColor : DisabledColor)
            .Replace("{color3}", ControlRoom.Activated ? ActivatedColor : DisabledColor)
            .Replace("{color4}", RoomsData.Hcz939.Activated ? ActivatedColor : DisabledColor)
            .Replace("{color5}", RoomsData.EzVent.Activated ? ActivatedColor : DisabledColor);
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        if (ev.Player.Tag.Contains(SerpentsHand.Tag))
            display.AddBlock(Block);
        else
            display.RemoveBlock(Block);
    }

    [EventMethod(RoundEvents.Waiting, int.MinValue)]
    static void Refresh()
    {
        UpdateProgress();
    }
}