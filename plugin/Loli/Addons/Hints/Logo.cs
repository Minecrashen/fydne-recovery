using Loli.HintsCore;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using UnityEngine;

namespace Loli.Addons.Hints;

static class Logo
{
    static readonly DisplayBlock Block;
    static readonly MessageBlock TimeBlock;

    static Logo()
    {
        Block = new(new(0, -470), new(300, 200));

        Block.Contents.Add(new("<b>" +
                "<color=#ff00c0>⭐</color>" +
                "<color=#ff00a0>f</color>" +
                "<color=#ff0080>y</color>" +
                "<color=#ff0060>d</color>" +
                "<color=#ff0040>n</color>" +
                "<color=#ff0020>e</color>" +
                "<color=#ff0000>⭐</color>" +
                "</b>"
                ));

        TimeBlock = new($"{DateTime.Now:dd.MM HH:mm:ss} (МСК)", new Color32(72, 101, 252, 255), "80%"); // #4865fc
        Block.Contents.Add(TimeBlock);
    }

    static internal void TimeUpdate()
    {
        TimeBlock.Content = $"{DateTime.Now:dd.MM HH:mm:ss} (МСК)";
    }

    [EventMethod(PlayerEvents.Join, -1)]
    static void Join(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.AddBlock(Block);
    }

}