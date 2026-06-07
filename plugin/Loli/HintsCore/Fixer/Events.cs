using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.HintsCore.Fixer;

static class Events
{
    internal const string Tag = "ShowHintPublicBlock";

    [EventMethod(PlayerEvents.Join, 9)]
    static void Join(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        var block = new DisplayBlock(Vector2.zero, new(Constants.CanvasSafeWidth, Constants.CanvasSafeHeight));

        display.AddBlock(block);
        ev.Player.Variables[Tag] = block;
    }
}