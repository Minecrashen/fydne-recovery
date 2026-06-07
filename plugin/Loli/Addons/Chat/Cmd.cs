using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using Qurre.API.Controllers;

namespace Loli.Addons.Chat;

static class Cmd
{
    [EventMethod(RoundEvents.Waiting)]
    static void NullCall() { }

    static Cmd()
    {
        CommandsSystem.RegisterConsole("chat", Chat);
        CommandsSystem.RegisterConsole("чат", Chat);

        CommandsSystem.RegisterConsole("чай", Message);


        CommandsSystem.RegisterConsole("б", MessageRoot);
        CommandsSystem.RegisterConsole("ближний", MessageRoot);
        CommandsSystem.RegisterConsole("pos", MessageRoot);

        CommandsSystem.RegisterConsole("п", MessageRoot);
        CommandsSystem.RegisterConsole("пб", MessageRoot);
        CommandsSystem.RegisterConsole("публичный", MessageRoot);
        CommandsSystem.RegisterConsole("public", MessageRoot);

        CommandsSystem.RegisterConsole("с", MessageRoot);
        CommandsSystem.RegisterConsole("сз", MessageRoot);
        CommandsSystem.RegisterConsole("союзный", MessageRoot);
        CommandsSystem.RegisterConsole("ally", MessageRoot);

        CommandsSystem.RegisterConsole("к", MessageRoot);
        CommandsSystem.RegisterConsole("км", MessageRoot);
        CommandsSystem.RegisterConsole("командный", MessageRoot);
        CommandsSystem.RegisterConsole("team", MessageRoot);

        CommandsSystem.RegisterConsole("л", MessageRoot);
        CommandsSystem.RegisterConsole("лс", MessageRoot);
        CommandsSystem.RegisterConsole("личный", MessageRoot);
        CommandsSystem.RegisterConsole("private", MessageRoot);

        CommandsSystem.RegisterConsole("кл", MessageRoot);
        CommandsSystem.RegisterConsole("клан", MessageRoot);
        CommandsSystem.RegisterConsole("clan", MessageRoot);

        CommandsSystem.RegisterConsole("ад", MessageRoot);
        CommandsSystem.RegisterConsole("адм", MessageRoot);
        CommandsSystem.RegisterConsole("admin", MessageRoot);

        CommandsSystem.RegisterConsole("нрп", MessageRoot);
        CommandsSystem.RegisterConsole("нонрп", MessageRoot);
        CommandsSystem.RegisterConsole("nonrp", MessageRoot);
    }

    static void Chat(GameConsoleCommandEvent ev)
    {
        ev.Allowed = false;

        if (ev.Args.Length == 0)
            goto IL_001;

        switch (ev.Args[0])
        {
            case "скрыть" or "hide":
                {
                    Main.SetVisible(ev.Player, false);
                    break;
                }

            case "показать" or "show":
                {
                    Main.SetVisible(ev.Player, true);
                    break;
                }

            case "мут" or "mute":
                {
                    Main.SetMute(ev.Player, string.Join(" ", ev.Args.Skip(1)), true);
                    break;
                }

            case "размут" or "unmute":
                {
                    Main.SetMute(ev.Player, string.Join(" ", ev.Args.Skip(1)), false);
                    break;
                }

            case "очистить" or "clear":
                {
                    Main.ClearChat(ev.Player);
                    break;
                }

            case "консоль":
                {
                    Main.LogChat(ev.Player);
                    break;
                }

            default:
                goto IL_001;
        }

        return;

    IL_001:
        {
            ev.Reply = "\n";
            ev.Reply += DecoreText("•───────•°• Чат •°•───────• ");
            ev.Reply += DecoreText(SmallText(" По неволе запертые в одиночестве порой желают выбраться из него."));
            ev.Reply += "\n";
            ev.Reply += DecoreText($"Вы можете писать в чат командой 〔{SmallText(" .чай тип сообщение ")}〕, либо сразу использовать 〔{SmallText(" .тип сообщение ")}〕");
            ev.Reply += "\n";
            ev.Reply += DecoreText("☆ Команды управления чатом:");
            ev.Reply += DecoreText($"  Скрыть чат с экрана: 〔{SmallText(" .чат скрыть ")}〕");
            ev.Reply += DecoreText($"  Вернуть отображенние чата: 〔{SmallText(" .чат показать ")}〕");
            ev.Reply += DecoreText($"  Замутить локально игрока: 〔{SmallText(" .чат мут ник игрока ")}〕");
            ev.Reply += DecoreText($"  Размутить локально игрока: 〔{SmallText(" .чат размут ник игрока ")}〕");
            ev.Reply += DecoreText($"  Очистить сообщения в чате: 〔{SmallText(" .чат очистить ")}〕");
            ev.Reply += DecoreText($"  Вывести в консоль текущие сообщения: 〔{SmallText(" .чат консоль ")}〕");
            ev.Reply += "\n";
            ev.Reply += DecoreText("☆ Команды для отправки сообщений:");
            ev.Reply += DecoreText($"  Написать в ближний чат: 〔{SmallText(" .чай Привет, я пишу в ближний!! ")}〕 или 〔{SmallText(" .б Привет, я пишу в ближний!! ")}〕");
            ev.Reply += DecoreText($"  Написать в публичный чат: 〔{SmallText(" .чай пб Привет, я пишу в публичный чат!! ")}〕 или 〔{SmallText(" .п Привет, я пишу в публичный чат!! ")}〕");
            ev.Reply += DecoreText($"  Написать в союзный чат: 〔{SmallText(" .чай сз Привет, товарищи!! ")}〕 или 〔{SmallText(" .сз Привет, товарищи!! ")}〕");
            ev.Reply += DecoreText($"  Написать в командный чат: 〔{SmallText(" .чай км Привет, товарищи!! ")}〕 или 〔{SmallText(" .км Привет, товарищи!! ")}〕");
            ev.Reply += DecoreText($"  Написать в личный чат: 〔{SmallText(" .чай лс Ник_игрока qq!! ")}〕 или 〔{SmallText(" .лс Ник_игрока qq!! ")}〕");
            ev.Reply += DecoreText($"  Написать в клановый чат: 〔{SmallText(" .чай кл Внимание клану, все устраиваем мтк! ")}〕 или 〔{SmallText(" .кл не убивайте друг друга ")}〕");
            ev.Reply += DecoreText($"  Написать в чат админам: 〔{SmallText(" .чай ад Але, админы, тут мтк ")}〕 или 〔{SmallText(" .ад Але, админы, тут мтк ")}〕");
            ev.Reply += DecoreText($"  Написать в нрп чат: 〔{SmallText(" .чай нрп какое еще нонрп, я на ноурулесе играю ")}〕 или 〔{SmallText(" .нрп ерпе ")}〕");
            ev.Reply += DecoreText("•───────•°• Чат •°•───────• ").Trim();
        }

        static string DecoreText(string str)
            => $"<color=#ff7073><size=25%>{str}</size></color>\n";

        static string SmallText(string text)
            => $"<size=20%><color=#ffc600>{text}</color></size>";
    }

    static void Message(GameConsoleCommandEvent ev)
    {
        ev.Allowed = false;

        if (ev.Args.Length == 0)
            goto IL_001;

        if (!ProcessMessage(ev.Player, ev.Args[0].ToLower(), ev.Args.Skip(1).ToArray()))
            goto IL_001;

        return;

    IL_001:
        {
            ev.Reply = "\n";
            ev.Reply += DecoreText("•───────•°• Сообщения •°•───────• ");
            ev.Reply += DecoreText(SmallText(" Ты не написал алиасы, так что я сделаю это за тебя."));
            ev.Reply += DecoreText($"  Ближний чат:   〔{SmallText(" б ")}〕  〔{SmallText(" ближний ")}〕 〔{SmallText(" pos ")}〕");
            ev.Reply += DecoreText($"  Публичный чат: 〔{SmallText(" п ")}〕  〔{SmallText(" пб ")}〕  〔{SmallText(" публичный ")}〕 〔{SmallText(" public ")}〕");
            ev.Reply += DecoreText($"  Союзный чат:   〔{SmallText(" с ")}〕  〔{SmallText(" сз ")}〕  〔{SmallText(" союзный ")}〕   〔{SmallText(" ally ")}〕");
            ev.Reply += DecoreText($"  Командный чат: 〔{SmallText(" к ")}〕  〔{SmallText(" км ")}〕  〔{SmallText(" командный ")}〕 〔{SmallText(" team ")}〕");
            ev.Reply += DecoreText($"  Личный чат:    〔{SmallText(" л ")}〕  〔{SmallText(" лс ")}〕  〔{SmallText(" личный ")}〕    〔{SmallText(" private ")}〕");
            ev.Reply += DecoreText($"  Клановый чат: 〔{SmallText(" кл ")}〕  〔{SmallText(" клан ")}〕〔{SmallText(" clan ")}〕");
            ev.Reply += DecoreText($"  Админский чат: 〔{SmallText(" ад ")}〕 〔{SmallText(" адм ")}〕 〔{SmallText(" admin ")}〕");
            ev.Reply += DecoreText($"  Нрп чат:       〔{SmallText(" нрп ")}〕〔{SmallText(" нонрп ")}〕〔{SmallText(" nonrp ")}〕");
            ev.Reply += DecoreText("•───────•°• Сообщения •°•───────• ").Trim();
        }

        static string DecoreText(string str)
            => $"<color=#3bc54b><size=25%>{str}</size></color>\n";

        static string SmallText(string text)
            => $"<size=20%><color=#3c51bd>{text}</color></size>";
    }

    static void MessageRoot(GameConsoleCommandEvent ev)
    {
        ev.Allowed = false;

        if (!ProcessMessage(ev.Player, ev.Name, ev.Args))
            ev.Reply = "Тип аргумента не найден, попробуй написать 〔 .чай 〕";
    }

    static bool ProcessMessage(Player pl, string type, string[] args)
    {
        switch (type)
        {
            case "б" or "ближний" or "pos":
                {
                    Main.SendMessage(pl, MessageType.Position, args);
                    return true;
                }

            case "п" or "пб" or "публичный" or "public":
                {
                    Main.SendMessage(pl, MessageType.Public, args);
                    return true;
                }

            case "с" or "сз" or "союзный" or "ally":
                {
                    Main.SendMessage(pl, MessageType.Ally, args);
                    return true;
                }

            case "к" or "км" or "командный" or "team":
                {
                    Main.SendMessage(pl, MessageType.Team, args);
                    return true;
                }

            case "л" or "лс" or "личный" or "private":
                {
                    if (args.Length < 2)
                    {
                        pl.Client.SendConsole("Укажите более одного аргумента в личном чате.", "red");
                        return true;
                    }

                    Main.SendPrivateMessage(pl, args[0], args.Skip(1).ToArray());
                    return true;
                }

            case "кл" or "клан" or "clan":
                {
                    Main.SendMessage(pl, MessageType.Clan, args);
                    return true;
                }

            case "ад" or "адм" or "admin":
                {
                    Main.SendMessage(pl, MessageType.Admin, args);
                    return true;
                }

            case "нрп" or "нонрп" or "nonrp":
                {
                    Main.SendMessage(pl, MessageType.Nonrp, args);
                    return true;
                }

            default:
                return false;
        }

    }
}