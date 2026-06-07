using Loli.HintsCore;
using Loli.HintsCore.Utils;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons.Hints;

static class EndStats
{
    static readonly Dictionary<string, (TimeSpan, string)> Escapes = new();
    static readonly Dictionary<string, int> Kills = new();
    static readonly Dictionary<string, int> ScpKills = new();
    static readonly Dictionary<string, int> Damages = new();

    static readonly DisplayBlock Block;
    static readonly Color MainColor;

    static EndStats()
    {
        Block = new(new(-Constants.CanvasSafeWidth + 300, 505), new(600, 800), align: Align.Left);
        MainColor = new Color32(199, 255, 147, 255);
    }

    [EventMethod(RoundEvents.End)]
    static void End()
    {
        Block.Contents.Clear();

        if (Escapes.Any())
        {
            Block.Contents.Add(new("Самые быстрые побеги:", MainColor, "90%"));
            for (int i = 0; i < Escapes.Count && i < 3; i++)
            {
                var data = Escapes.ElementAt(i);
                Block.Contents.Add(new($"[<color=#ff7fa4>{i}</color>] {data.Key} сбежал за <color={data.Value.Item2}><b>{data.Value.Item1:mm\\:ss}</b></color>", Color.white, "60%"));
            }
        }

        if (Kills.Any())
        {
            Block.Contents.Add(new("_", Color.clear, "90%"));
            Block.Contents.Add(new("Больше всего убийств:", MainColor, "90%"));
            var kills = Kills.OrderByDescending(x => x.Value);
            for (int i = 0; i < kills.Count() && i < 5; i++)
            {
                var data = kills.ElementAt(i);
                Block.Contents.Add(new($"[<color=#ff7fa4>{i}</color>] {data.Key} - <b>{data.Value}</b>", Color.white, "60%"));
            }
        }

        if (ScpKills.Any())
        {
            Block.Contents.Add(new("_", Color.clear, "90%"));
            Block.Contents.Add(new("Больше всего убийств за SCP:", MainColor, "90%"));
            var scpKills = ScpKills.OrderByDescending(x => x.Value);
            for (int i = 0; i < scpKills.Count() && i < 5; i++)
            {
                var data = scpKills.ElementAt(i);
                Block.Contents.Add(new($"[<color=#ff7fa4>{i}</color>] {data.Key} - <b>{data.Value}</b>", Color.white, "60%"));
            }
        }

        if (Damages.Any())
        {
            Block.Contents.Add(new("_", Color.clear, "90%"));
            Block.Contents.Add(new("Больше всего нанесенного урона:", MainColor, "90%"));
            var damages = Damages.OrderByDescending(x => x.Value);
            for (int i = 0; i < damages.Count() && i < 5; i++)
            {
                var data = damages.ElementAt(i);
                Block.Contents.Add(new($"[<color=#ff7fa4>{i}</color>] {data.Key} - <b>{data.Value}</b>", Color.white, "60%"));
            }
        }

        foreach (Player pl in Player.List)
        {
            if (!pl.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                continue;

            display.AddBlock(Block);
        }
    }

    [EventMethod(RoundEvents.Waiting)]
    [EventMethod(RoundEvents.Start)]
    static void Refresh()
    {
        Block.Contents.Clear();
        Escapes.Clear();
        Kills.Clear();
        ScpKills.Clear();
        Damages.Clear();
    }

    [EventMethod(PlayerEvents.Dead)]
    static void Dead(DeadEvent ev)
    {
        if (ev.Attacker == ev.Target)
            return;

        string nick = ev.Attacker.UserInformation.Nickname;

        if (ev.Attacker.RoleInformation.Team == Team.SCPs)
        {
            if (!ScpKills.ContainsKey(nick))
                ScpKills.Add(nick, 1);
            else
                ScpKills[nick]++;
        }
        else
        {
            if (!Kills.ContainsKey(nick))
                Kills.Add(nick, 1);
            else
                Kills[nick]++;
        }
    }

    [EventMethod(PlayerEvents.Attack)]
    static void Damage(AttackEvent ev)
    {
        if (!ev.Allowed)
            return;

        if (ev.Damage < 1)
            return;

        if (ev.FriendlyFire && !Server.FriendlyFire)
            return;

        if (ev.Attacker == ev.Target || ev.Attacker == Server.Host)
            return;

        if (ev.Attacker.RoleInformation.Team == Team.SCPs)
            return;

        if (!Damages.ContainsKey(ev.Attacker.UserInformation.Nickname))
            Damages.Add(ev.Attacker.UserInformation.Nickname, (int)ev.Damage);
        else
            Damages[ev.Attacker.UserInformation.Nickname] += (int)ev.Damage;
    }

    [EventMethod(PlayerEvents.Escape)]
    static void Escape(EscapeEvent ev)
    {
        if (!ev.Allowed)
            return;

        if (Escapes.ContainsKey(ev.Player.UserInformation.Nickname))
            return;

        Escapes.Add(
            ev.Player.UserInformation.Nickname,
            (Round.ElapsedTime, ev.Player.RoleInformation.Role.GetInfoRole().Item2)
            );
    }
}