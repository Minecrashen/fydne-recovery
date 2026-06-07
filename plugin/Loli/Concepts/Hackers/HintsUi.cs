using Loli.HintsCore;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.Concepts.Hackers;

static class HintsUi
{
    static readonly DisplayBlock Block;
    static readonly MessageBlock ProgressBlockPanels;
    static readonly MessageBlock ProgressBlockControl;
    static readonly MessageBlock ProgressBlockServers;

    static HintsUi()
    {
        Block = new(new(0, -270), new(450, 600));
        ProgressBlockPanels = new("<size=80%>[]", Color.white);
        ProgressBlockControl = new("<size=80%>[]", Color.white);
        ProgressBlockServers = new("<size=80%>[]", Color.white);

        Block.Contents.Add(new("<b>Прогресс взлома Хакеров</b>", new Color32(48, 196, 37, 255), "70%"));
        Block.Contents.Add(new("<b>Взлом Станций Безопасности</b>", new Color32(255, 174, 0, 255), "55%"));
        Block.Contents.Add(ProgressBlockPanels);
        Block.Contents.Add(new("<b>Взлом Комнаты Управления</b>", new Color32(255, 34, 34, 255), "55%"));
        Block.Contents.Add(ProgressBlockControl);
        Block.Contents.Add(new("<b>Взлом Основных Серверов</b>", new Color32(65, 105, 255, 255), "55%"));
        Block.Contents.Add(ProgressBlockServers);
    }

    static (string, string) GetColorByStatus(HackMode mode)
    {
        return mode switch
        {
            HackMode.Safe => ("#00ff19", "#009c0f"),
            HackMode.Hacking => ("#ffe200", "#998700"),
            HackMode.Hacked => ("#ff0000", "#880000"),
            _ => ("#737373", "#4a4a4a")
        };
    }

    static internal void UpdateProgressPanels()
    {
        string content = "<size=80%>[</size><cspace=-0.2em><size=70%>";

        foreach (Panel panel in Panel.ReadPanels)
            content += $"<color={GetColorByStatus(panel.Status).Item1}>■</color>";

        content += "</size></cspace><size=80%>]</size>";

        ProgressBlockPanels.Content = content;
    }

    static internal void UpdateProgressControl()
    {
        if (!Panel.AllHacked)
        {
            ProgressBlockControl.Content = "<color=#d60000><size=65%>Взломайте все Станции Безопасности</size></color>";
            return;
        }

        (string color, string back) = GetColorByStatus(Control.Status);

        string content = "<size=80%>[</size><cspace=-0.2em><size=70%>";
        content += Extensions.ProgressBar(Control.Process, 100, 10, "■", color, back);
        content += "</size></cspace><size=80%>]</size>";

        ProgressBlockControl.Content = content;
    }

    static internal void UpdateProgressServers()
    {
        if (Control.Status != HackMode.Hacked)
        {
            ProgressBlockServers.Content = "<color=#d60000><size=65%>Взломайте Комнату Управления</size></color>";
            return;
        }

        (string color, string back) = GetColorByStatus(MainServers.Status);

        string content = "<size=80%>[</size><cspace=-0.2em><size=70%>";
        content += Extensions.ProgressBar(MainServers.Process, 100, 10, "■", color, back);
        content += "</size></cspace><size=80%>]</size>";

        ProgressBlockServers.Content = content;
    }

    static internal void AddUi(Player pl)
    {
        if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.AddBlock(Block);
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.RemoveBlock(Block);
    }

    [EventMethod(RoundEvents.Waiting, int.MinValue)]
    static void Refresh()
    {
        UpdateProgressPanels();
        UpdateProgressControl();
        UpdateProgressServers();
    }
}