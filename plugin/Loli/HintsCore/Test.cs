/*
using Loli.HintsCore.Utils;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using UnityEngine;

namespace Loli.HintsCore;

static class Test
{
    [EventMethod(RoundEvents.Waiting)]
    static void Calc()
    {
        Log.Info($"I: {Worker.CalculateContentSize("I")}; " +
            $"i: {Worker.CalculateContentSize("i")}; " +
            $"m: {Worker.CalculateContentSize("m")}; " +
            $"mi: {Worker.CalculateContentSize("mi")}; " +
            $"mi2: {Worker.CalculateContentSize("<size=50%>mi")}; " +
            $"mi2: {Worker.CalculateContentSize("<size=200%>mi")}; " +
            $"mi2: {Worker.CalculateContentSize("<size=50%><size=200%>mi")}; " +
            $"mi2: {Worker.CalculateContentSize("<size=50%><size=200%>mi</size></size>")}; " +
            $"t1: {Worker.CalculateContentSize("ы ы")}; " +
            $"");
    }

    [EventMethod(PlayerEvents.Join)]
    static void JoinTest(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        var vvv = new DisplayBlock(new(-1000, 100), new(600, 600), align: Align.Left);
        //var vv = new MessageBlock("SFHUFjesfhnu3hr#&(RPGOY*&I#GRUOY&IG#RYUG#RYUG#RYGKT#RGTY#R", Color.red, "80%");
        var vv = new MessageBlock("❤️ / 💰", Color.red, "80%");

        display.AddBlock(vvv);
        vvv.Contents.Add(vv);

        for (int i = 0; i < 20; i++)
        {
            DisplayBlock block = new(new(3000, 100 - (i * 30)), new(6000, 6000), padding: new(0, i * 100, 0, 0), align: Align.Right);
            MessageBlock msg = new($"i: {i}", Color.red, "80%");
            display.AddBlock(block);
            block.Contents.Add(msg);
        }

        Timing.CallDelayed(30f, () => vvv.Contents.Remove(vv));
    }
}
*/