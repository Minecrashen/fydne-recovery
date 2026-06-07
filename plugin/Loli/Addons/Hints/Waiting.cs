using HarmonyLib;
using Loli.HintsCore;
using MEC;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons.Hints;

static class Waiting
{
    const string CoroutineTag = "Waiting_CoroutineHintTag";
    static readonly DisplayBlock Block;
    static readonly MethodInfo JoinEvent;

    static Waiting()
    {
        Block = new(new(0, -200), new(1000, 300));
        JoinEvent = AccessTools.Method(typeof(Waiting), nameof(Waiting.Join));
    }

    [EventMethod(RoundEvents.Waiting)]
    static void Wait()
    {
        Qurre.API.Core.InjectEventMethod(JoinEvent);
        Timing.RunCoroutine(Coroutine(), CoroutineTag);
    }

    [EventMethod(RoundEvents.Start)]
    static void Start()
    {
        Qurre.API.Core.ExtractEventMethod(JoinEvent);
        Timing.KillCoroutines(CoroutineTag);

        Block.Contents.Clear();

        foreach (Player pl in Player.List)
        {
            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                continue;

            display.RemoveBlock(Block);
        }
    }

    [EventsIgnore]
    [EventMethod(PlayerEvents.Join, -1)]
    static void Join(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.AddBlock(Block);
    }

    static IEnumerator<float> Coroutine()
    {
        while (Round.Waiting)
        {
            Block.Contents.Clear();

            if (Round.WaitTime > 0)
                Block.Contents.Add(new($"<b>Раунд начнется через {Round.WaitTime} секунд</b>", Color.yellow, "150%"));

            Block.Contents.Add(new($"{Player.List.Count()} игроков", Color.magenta, "120%"));

            yield return Timing.WaitForSeconds(1f);
        }

        yield break;
    }
}