using HarmonyLib;
using Loli.HintsCore;
using Loli.HintsCore.Utils;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons;

static class VoteRestart
{
    static readonly DisplayBlock Block;
    static readonly MessageBlock VoteBlock;
    static readonly MethodInfo JoinEvent;
    static readonly List<string> _voted = new();

    static internal int VotedCount
        => _voted.Count;
    static internal int NeedCount
        => Math.Max(Math.Max(Player.List.Count(), 1) / 6 * 4, 10);

    static VoteRestart()
    {
        JoinEvent = AccessTools.Method(typeof(VoteRestart), nameof(VoteRestart.Join));

        Block = new(new(500, 300), new(400, 300), align: Align.Center);
        Block.Contents.Add(new("Сервер умер?", Color.red, "90%"));
        Block.Contents.Add(new("Проголосуйте за рестарт командой", new Color32(255, 253, 129, 255), "70%"));
        Block.Contents.Add(new(".res в консоли на [ё]", new Color32(255, 253, 129, 255), "70%"));

        VoteBlock = new($"{VotedCount}/{NeedCount}", new Color32(130, 255, 135, 255), "60%");
        Block.Contents.Add(VoteBlock);

        CommandsSystem.RegisterConsole("res", Command);
        static void Command(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;

            if (_voted.Contains(ev.Player.UserInformation.UserId))
            {
                ev.Reply = "Вы уже голосовали." + getVoteMessage();
                ev.Color = "red";
                return;
            }

            _voted.Add(ev.Player.UserInformation.UserId);
            ev.Reply = "Вы проголосовали." + getVoteMessage();
            ev.Color = "lime";

            VoteBlock.Content = $"{VotedCount}/{NeedCount}";

            static string getVoteMessage()
                => $"\nХод голосования: {VotedCount}/{NeedCount}";
        }
    }


    static internal void CycleCheck()
    {
        if (!Round.Waiting)
            return;

        VoteBlock.Content = $"{VotedCount}/{NeedCount}";

        if (VotedCount < NeedCount)
            return;

        Extensions.RestartCrush("Голосование за рестарт от игроков");
    }


    [EventMethod(RoundEvents.Waiting)]
    static void Waiting()
    {
        _voted.Clear();
        VoteBlock.Content = $"{VotedCount}/{NeedCount}";

        Qurre.API.Core.InjectEventMethod(JoinEvent);
    }

    [EventMethod(RoundEvents.Start)]
    static void RoundStart()
    {
        _voted.Clear();
        VoteBlock.Content = $"{VotedCount}/{NeedCount}";

        Qurre.API.Core.ExtractEventMethod(JoinEvent);

        foreach (Player pl in Player.List)
        {
            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                continue;

            display.RemoveBlock(Block);
        }
    }

    [EventMethod(PlayerEvents.Leave)]
    static void CacheClear(LeaveEvent ev)
    {
        if (!_voted.Contains(ev.Player.UserInformation.UserId))
            return;

        _voted.Remove(ev.Player.UserInformation.UserId);

        VoteBlock.Content = $"{VotedCount}/{NeedCount}";
    }

    [EventsIgnore]
    [EventMethod(PlayerEvents.Join, -1)]
    static void Join(JoinEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        display.AddBlock(Block);
    }
}