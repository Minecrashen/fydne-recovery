using Loli.Addons;
using Loli.Builds.Models.Rooms;
using Loli.DataBase.Modules;
using Loli.HintsCore;
using Loli.HintsCore.Utils;
using MEC;
using Newtonsoft.Json;
using PlayerRoles;
using PlayerStatsSystem;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.DataBase;

static class Levels
{
    static DateTime LastPay = DateTime.Now;
    static readonly Regex regexProject = new(@"#fydne");
    static readonly Regex regexLoliProject = new(@"#loli");
    const string hintBlockTag = "Levels_HintsDisplayBlock";
    const string tagScp106Attacker = "Levels_Scp106AttackerCache";


    [EventMethod(PlayerEvents.Join)]
    static void Join(JoinEvent ev)
    {
        ev.Player.SetRank("ERR уровень", "carmine");
        Timing.CallDelayed(100f, () => ev.Player.Client.Broadcast(15,
            "<size=90%><color=#ff007f>❤️ Если вы напишите в нике</color> <color=#ff7300>#fydne</color>,\n" +
            "<color=#e1ff73>то будете получать в 2 раза больше опыта и монет</color></size>"));

        Timing.CallDelayed(130f, () => ev.Player.Client.Broadcast(15,
            "<size=90%><color=#ff007f>❤️ Если вы напишите в нике</color> <color=#ff7300>#loli</color>,\n" +
            "<color=#e1ff73>то будете получать в 3!! раза больше опыта и монет</color></size>"));

        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        DisplayBlock block = new(new(3000, 100), new(350, 500), align: Align.Right);

        display.AddBlock(block);
        ev.Player.Variables[hintBlockTag] = block;
    }

    #region Get XP
    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (ev.Role is RoleTypeId.None)
            return;

        try { SetPrefix(ev.Player); } catch { }
    }

    [EventMethod(RoundEvents.End)]
    static void XpRoundEnd()
    {
        foreach (Player pl in Player.List)
        {
            if (regexLoliProject.IsMatch(pl.UserInformation.Nickname))
            {
                pl.AddStats(400, 0, "#loli в нике", false);
                pl.Client.Broadcast(10, "<size=80%><color=#e1ff73>Вы получили <color=#ff007f>400xp</color>, так как в вашем нике есть <color=#ff7300>#loli</color>!</color></size>");
            }
            else if (regexProject.IsMatch(pl.UserInformation.Nickname))
            {
                pl.AddStats(200, 0, "#fydne в нике", false);
                pl.Client.Broadcast(10, "<size=80%><color=#e1ff73>Вы получили <color=#ff007f>200xp</color>, так как в вашем нике есть <color=#ff7300>#fydne</color>!</color></size>");
            }
        }
    }

    [EventMethod(PlayerEvents.Escape, int.MinValue)]
    static void XpEscape(EscapeEvent ev)
    {
        if (!ev.Allowed)
            return;

        if (ev.Player.RoleInformation.Team is Team.ChaosInsurgency or Team.FoundationForces)
            ev.Player.AddStats(10, 10, $"побег");
        else
            ev.Player.AddStats(500, 50, $"побег");

        if (ev.Player.GamePlay.Cuffed)
        {
            Player cuffer = ev.Player.GamePlay.Cuffer;

            if (ev.Player.Variables["RealCuffer"] is Player cuffer2)
                cuffer = cuffer2;

            if (cuffer == null || cuffer == ev.Player)
                return;

            if (cuffer.Disconnected)
                return;

            ev.Player.AddStats(250, 50, $"побег");
        }
    }

    [EventMethod(ScpEvents.Scp106Attack)]
    static void XpScp106Damaged(Scp106AttackEvent ev)
    {
        ev.Target.Variables[tagScp106Attacker] = ev.Attacker;
    }

    [EventMethod(ScpEvents.Scp079NewLvl, int.MinValue)]
    static void XpScp079Level(Scp079NewLvlEvent ev)
    {
        if (!ev.Allowed)
            return;

        ev.Player.AddStats(ev.Level * 10, ev.Level * 10, $"получение {ev.Level} уровня за SCP-079");
    }

    [EventMethod(ScpEvents.Scp049RaisingEnd, int.MinValue)]
    static void XpScp049Raising(Scp049RaisingEndEvent ev)
    {
        if (!ev.Allowed)
            return;

        ev.Player.AddStats(100, 10, $"воскрешение игрока \"{ev.Target.UserInformation.Nickname}\"");
    }

    [EventMethod(PlayerEvents.Dead, int.MinValue)]
    static void XpKill(DeadEvent ev)
    {
        if (ev.Target.InPocket())
        {
            if (!ev.Target.Variables.TryGetAndParse(tagScp106Attacker, out Player attacker))
                return;

            (string, string) hasRole106 = ev.Target.RoleInformation.CachedRole.GetInfoRole();
            attacker.AddStats(25, 5, $"убийство {ev.Target.UserInformation.Nickname}, который был \"<color={hasRole106.Item2}>{hasRole106.Item1}</color>\"");
            return;
        }

        if (ev.Attacker == ev.Target || ev.Attacker == Server.Host)
            return;

        if (ev.DamageInfo is not AttackerDamageHandler attack)
            return;

        float money, xp;
        switch (attack.Attacker.Role.GetTeam())
        {
            case Team.ChaosInsurgency:
                {
                    money = 5;
                    xp = ev.Target.RoleInformation.CachedRole.GetTeam() switch
                    {
                        Team.FoundationForces or Team.OtherAlive => 50,
                        Team.Scientists => 25,
                        Team.SCPs => 250,
                        _ => (float)0,
                    };
                    break;
                }
            case Team.FoundationForces:
                {
                    money = 5;
                    xp = ev.Target.RoleInformation.CachedRole.GetTeam() switch
                    {
                        Team.ChaosInsurgency or Team.OtherAlive => 50,
                        Team.ClassD => 25,
                        Team.SCPs => 250,
                        _ => (float)0,
                    };
                    break;
                }
            case Team.OtherAlive:
                {
                    money = 3;
                    xp = ev.Target.RoleInformation.CachedRole.GetTeam() switch
                    {
                        Team.ClassD or Team.Scientists => 25,
                        Team.FoundationForces or Team.ChaosInsurgency => 50,
                        Team.SCPs => 10,
                        _ => (float)0,
                    };
                    break;
                }
            case Team.Scientists:
                {
                    money = 7;
                    xp = ev.Target.RoleInformation.CachedRole.GetTeam() switch
                    {
                        Team.ClassD => 25,
                        Team.ChaosInsurgency or Team.OtherAlive => 100,
                        Team.SCPs => 500,
                        _ => (float)0,
                    };
                    break;
                }
            case Team.ClassD:
                {
                    money = 7;
                    xp = ev.Target.RoleInformation.CachedRole.GetTeam() switch
                    {
                        Team.Scientists => 25,
                        Team.FoundationForces or Team.OtherAlive => 100,
                        Team.SCPs => 500,
                        _ => (float)0,
                    };
                    break;
                }
            case Team.SCPs:
                {
                    money = 3;
                    xp = 25;
                    break;
                }
            default:
                {
                    money = 0;
                    xp = 0;
                    break;
                }
        }

        (string, string) hasRole = ev.Target.RoleInformation.CachedRole.GetInfoRole();
        ev.Attacker.AddStats(xp, money, $"убийство {ev.Target.UserInformation.Nickname}, который был \"<color={hasRole.Item2}>{hasRole.Item1}</color>\"");
    }
    #endregion

    #region API
    internal static void SetPrefix(Player player)
    {
        if (player.Disconnected)
            return;

        if (player.UserInformation.UserId == "76561199054401641@steam")
        {
            player.SetRank("Комитет по Этике", "nickel");
            return;
        }

        string prefix = "";
        string clan = "";
        string prime = "";
        string role = "";
        string srole = "";

        Data.Users.TryGetValue(player.UserInformation.UserId, out var imain);
        Data.Roles.TryGetValue(player.UserInformation.UserId, out var roles);

        int lvl = imain.lvl;

        string color = lvl switch
        {
            0 or 1 => "green",
            10 or (>= 10 and < 10) or (>= 100 and < 100) => "tomato",
            2 or (>= 20 and < 30) or (>= 200 and < 300) => "crimson",
            3 or (>= 30 and < 40) or (>= 300 and < 400) => "cyan",
            4 or (>= 40 and < 50) or (>= 400 and < 500) => "deep_pink",
            5 or (>= 50 and < 60) or (>= 500 and < 600) => "yellow",
            6 or (>= 60 and < 70) or (>= 600 and < 700) => "orange",
            7 or (>= 70 and < 80) or (>= 700 and < 800) => "lime",
            8 or (>= 80 and < 90) or (>= 800 and < 900) => "pumpkin",
            _ => "red",
        };

        string lvlString = $"{lvl}";

        if (player.UserInformation.DoNotTrack)
        {
            lvlString = "ERR";
            color = "carmine";
        }

        if (imain.clan != "" && !imain.anonym)
        {
            if ((!roles.Gradients && !roles.Benefactor) || string.IsNullOrEmpty(imain.clanColor))
                clan = $"{imain.clan} | ";
            else
                clan = $"<color={imain.clanColor}>{imain.clan}</color> | "
                    .Replace("<", "\\u003C")
                    .Replace(">", "\\u003E");
        }

        if (roles.Prime)
        {
            prime += " | Апостол";
            color = "pumpkin";
        }

#if NR
        if (roles.Mage)
        {
            prime += " | Маг";
            color = "mint";
        }
        if (roles.Sage)
        {
            prime += " | Пророк";
            color = "crimson";
        }
        if (roles.Star)
        {
            prime += " | Звездочка";
            color = "magenta";
        }
        if (roles.Hand)
        {
            prime += " | Патриарх";
            color = "cyan";
        }
        if (roles.Priest)
        {
            prime += " | Священник";
            color = "pink";
        }
#endif

        if (roles.Benefactor)
        {
            prime += " | Благодетель";
            color = "emerald";
        }

        if (imain.isNitro)
        {
            prime += " | !Nitro Booster!";
            color = "magenta";
        }

        if ((roles.Rainbow || roles.Gradients || roles.Benefactor) && imain.prefix != "")
        {
            prefix = $" | {Constants.ReplaceUnAllowedRegex.Replace(imain.prefix, "")}";
        }

        if (!imain.anonym)
        {
            if (imain.trainee)
            {
                color = "lime";
                role = " | Практикант";
            }
            if (imain.helper)
            {
                color = "aqua";
                role = " | Смотритель";
            }
            if (imain.mainhelper)
            {
                color = "cyan";
                role = " | Дружинник";
            }
            if (imain.admin)
            {
                color = "yellow";
                role = " | Боярин";
            }
            if (imain.control)
            {
                color = "magenta";
                role = " | Дьяк";
            }
            if (imain.mainadmin)
            {
                color = "red";
                role = " | Сенат";
            }
            if (imain.maincontrol)
            {
                color = "pumpkin";
                role = " | Император";
            }
            if (imain.it)
            {
                color = "deep_pink";
                role = " | Тех. отдел";
            }
        }

        if (imain.found && !imain.anonym)
            imain.cached_role = $"{clan}{srole}{lvlString} уровень{prime}{role}{prefix}".Trim();
        else
            imain.cached_role = $"{clan}{srole}{lvlString} уровень{prime}{prefix}".Trim();

        player.SetRank(imain.cached_role, color);
    }

    static internal void AddStats(this Player pl, float xp, float money, string desc = "???", bool combine = true)
    {
        if (pl.UserInformation.DoNotTrack)
            return;

        if (combine)
        {
            GetTotalMoney(pl, ref money);
            GetTotalXp(pl, ref xp);
        }

        ShowHint(pl, $"+{xp}xp & {money} 💰");

        Timing.CallDelayed(0.5f, () => pl.Client.SendConsole($"Вы получили {xp}xp & {money} монет за {desc}", "white"));

        Stats.Add(pl, (int)Math.Round(xp), (int)Math.Round(money));
    }

    static internal void ShowHint(Player pl, string text)
    {
        if (!pl.Variables.TryGetAndParse(hintBlockTag, out DisplayBlock block))
            return;

        MessageBlock message = new(text, new Color32(253, 255, 187, 255), "80%");
        block.Contents.Add(message);

        Timing.CallDelayed(10f, () => block.Contents.Remove(message));
    }

    static internal void GetTotalXp(Player pl, ref float xp)
    {
        float UpDegree = 1;

        if (regexLoliProject.IsMatch(pl.UserInformation.Nickname.ToLower()))
            UpDegree += 2;

        if (regexProject.IsMatch(pl.UserInformation.Nickname.ToLower()))
            UpDegree++;

        if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var user))
        {
            if (pl.IsPrime())
                UpDegree++;

            if (Data.Clans.TryGetValue(user.clan.ToUpper(), out var clan))
            {
                foreach (int boost in clan.Boosts)
                {
                    if (boost == 1)
                        UpDegree++;
                    else if (boost == 2)
                        UpDegree += 2;
                }
            }
        }

        xp *= UpDegree;
    }

    static internal void GetTotalMoney(Player pl, ref float money)
    {
        float UpDegree = 1;

        if (regexLoliProject.IsMatch(pl.UserInformation.Nickname.ToLower()))
            UpDegree += 2;

        if (regexProject.IsMatch(pl.UserInformation.Nickname.ToLower()))
            UpDegree++;

        money *= UpDegree;

        int upped = (int)money;
        if (pl.Variables.TryGetAndParse("Levels_MoneyUpped", out int value))
            upped += value;

        if (upped > 1000)
        {
            money -= Math.Max(upped - 1000, 0);
            pl.Variables["Levels_MoneyUpped"] = 1000;
            return;
        }

        pl.Variables["Levels_MoneyUpped"] = upped;
    }
    #endregion

    #region Commands
    static Levels()
    {
        CommandsSystem.RegisterConsole("xp", ConsoleXP);
        CommandsSystem.RegisterConsole("lvl", ConsoleXP);
        CommandsSystem.RegisterConsole("money", ConsoleXP);
        CommandsSystem.RegisterConsole("stats", ConsoleXP);

        CommandsSystem.RegisterConsole("pay", ConsolePay);
        CommandsSystem.RegisterConsole("пей", ConsolePay);
        CommandsSystem.RegisterConsole("пэй", ConsolePay);
    }

    static void ConsoleXP(GameConsoleCommandEvent ev)
    {
        ev.Allowed = false;

        if (!Data.Users.TryGetValue(ev.Player.UserInformation.UserId, out var data))
        {
            ev.Reply = "<color=#ff007f>Вы не найдены в массиве игроков </color>";
            ev.Color = "white";
            return;
        }

        ev.Reply = "\n";
        ev.Reply += DecoradeString(" ─── ⋆⋅☆⋅⋆ ─── 〔 Уровень 〕 ─── ⋆⋅☆⋅⋆ ───");
        ev.Reply += DecoradeString($" Уровень: {data.lvl}");
        ev.Reply += DecoradeString($" Опыт: {data.xp}/{data.to}");
        ev.Reply += DecoradeString($" Баланс: {data.money}");
        ev.Reply += DecoradeString(" ─── ⋆⋅☆⋅⋆ ─── 〔 Уровень 〕 ─── ⋆⋅☆⋅⋆ ───").Trim();

        ev.Color = "white";

        static string DecoradeString(string str)
            => $"<color=#ff7073>{str}</color>\n";
    }

    static void ConsolePay(GameConsoleCommandEvent ev)
    {
        ev.Allowed = false;

        if (ev.Args.Count() < 2)
        {
            ev.Reply = $"Команда введена неверно.\nПример: {ev.Name} 10 Ник игрока";
            ev.Color = "red";
            return;
        }

        if (!int.TryParse(ev.Args[0], out int result))
        {
            ev.Reply = "Введите корректное кол-во монет";
            ev.Color = "red";
            return;
        }

        string search = string.Join(" ", ev.Args.Skip(1));
        Player pl = search.GetPlayer();

        if (pl == null)
        {
            ev.Reply = $"Игрок \"{search}\" не найден";
            ev.Color = "red";
            return;
        }

        if (pl.UserInformation.UserId == ev.Player.UserInformation.UserId)
        {
            ev.Reply = "Нельзя передать монетки самому себе";
            ev.Color = "red";
            return;
        }

        if ((DateTime.Now - LastPay).TotalSeconds < 0.5)
        {
            ev.Reply = "Рейт-лимит, попробуйте снова";
            ev.Color = "red";
            return;
        }

        LastPay = DateTime.Now;

        string uid = "";
        uid = Core.Socket.On("database.get.stats", obj => DoUpdate(obj));

        Core.Socket.Emit("database.get.stats", new object[] {
            ev.Player.UserInformation.UserId.Replace("@steam", "").Replace("@discord", ""),
            ev.Player.UserInformation.UserId.Contains("discord"),
            ev.Player.UserInformation.UserId+"+updating"}
        );

        ev.Reply = "Запрос на перевод создан";

        void DoUpdate(object[] obj)
        {
            string userid = obj[1].ToString();

            if (userid != ev.Player.UserInformation.UserId + "+updating")
                return;

            var sender = userid.Replace("+updating", "").GetPlayer();
            if (sender is null)
                return;

            Core.Socket.Off(uid);

            SocketStatsData json = JsonConvert.DeserializeObject<SocketStatsData>(obj[0].ToString());

            if (result < 0)
            {
                ev.Player.Client.SendConsole("Ай, Ай, Ай, Накручивать плохо", "red");
                return;
            }

            if (result == 0)
            {
                ev.Player.Client.SendConsole("Нельзя передать 0 монет", "red");
                return;
            }

            if (json.money >= result)
            {
                Stats.AddMoney(ev.Player, 0 - result);
                Stats.AddMoney(pl, result);

                ShowHint(ev.Player, $"-{result} 💰");
                ShowHint(pl, $"+{result} 💰");

                ev.Player.Client.SendConsole($"Вы успешно передали {result} монет игроку {pl.UserInformation.Nickname}", "white");
                pl.Client.SendConsole($"{ev.Player.UserInformation.Nickname} передал вам {result} монет", "white");

                return;
            }

            ev.Player.Client.SendConsole($"Не хватает монет({json.money}/{result})", "red");
        }

    } // command
    #endregion
}