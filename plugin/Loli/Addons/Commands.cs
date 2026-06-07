using System.Collections.Generic;
using Loli.Concepts;
using Loli.Concepts.NuclearAttack;
using Loli.Concepts.Scp008;
using Loli.DataBase.Modules;
using Loli.HintsCore;
using Loli.Spawns;
using MEC;
using Mirror;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using RoundRestarting;
using System.Linq;
using LabApi.Features.Wrappers;
using Loli.DataBase;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using UnityEngine;
using Door = Qurre.API.Controllers.Door;
using Map = Qurre.API.World.Map;
using Player = Qurre.API.Controllers.Player;
using Room = Qurre.API.Controllers.Room;
using Round = Qurre.API.World.Round;
using Server = Qurre.API.Server;
using Tesla = Qurre.API.Controllers.Tesla;

#if MRP
using Loli.Addons.RolePlay;
#elif NR
using Loli.Controllers;
using Loli.Modules;
using VoiceChat;
#endif

namespace Loli.Addons
{
    static class Commands
    {
#if NR
        internal static bool EventMode { get; private set; } = false;
#endif

        [EventMethod(RoundEvents.Waiting)]
        private static void NullCall()
        {
#if NR
            Server.FriendlyFire = false;
            Chat.Main.GloballyMuted = false;
            EventMode = false;
            Muted = false;
#endif
        }

        static Commands()
        {
            CommandsSystem.RegisterConsole("help", Help);
            CommandsSystem.RegisterConsole("хелп", Help);
            CommandsSystem.RegisterConsole("хэлп", Help);

            CommandsSystem.RegisterConsole("kys", Suicide);
            CommandsSystem.RegisterConsole("kill", Suicide);
            CommandsSystem.RegisterConsole("tps", TPS);
            CommandsSystem.RegisterConsole("size", Size);

#if NR
            CommandsSystem.RegisterConsole("event", Event);
            CommandsSystem.RegisterConsole("textmute", TextMute);
            CommandsSystem.RegisterConsole("roundmute", RoundMute);
            CommandsSystem.RegisterConsole("roundff", RoundFF);
            CommandsSystem.RegisterConsole("picks", PicksCount);
#endif

            CommandsSystem.RegisterConsole("level_set", LevelSet);

            CommandsSystem.RegisterRemoteAdmin("list", List);
            CommandsSystem.RegisterRemoteAdmin("stafflist", StaffList);
        }

        private static void StaffList(RemoteAdminCommandEvent ev)
        {
            ev.Allowed = false;
            string names = string.Empty;

            foreach (Player player in Player.List)
            {
                if (player.UserInformation.UserId.IsPrikols())
                    continue;
                if (!Data.Users.TryGetValue(player.UserInformation.UserId, out UserData main)) continue;

                string role = string.Empty;

                if (main.trainee) role = "Практикант";
                else if (main.helper) role = "Смотритель";
                else if (main.mainhelper) role = "Дружинник";
                else if (main.admin) role = "Боярин";
                else if (main.control) role = "Дьяк";
                else if (main.mainadmin) role = "Сенат";
                else if (main.maincontrol) role = "Император";

                if (!string.IsNullOrEmpty(role))
                    names += $"{player.UserInformation.Nickname} - {role}\n";
            }

            ev.Reply = $"{Player.List.Count()}/{Core.MaxPlayers}\n";
            ev.Reply += !string.IsNullOrEmpty(names) ? names : "Нет администрации онлайн.";
        }

        private static void List(RemoteAdminCommandEvent ev)
        {
            ev.Allowed = false;
            string message = string.Empty;

            foreach (Player player in Player.List)
            {
                message += $"{player.UserInformation.Nickname} - {player.UserInformation.UserId} " +
                           $"({player.UserInformation.Id}) [{player.RoleInformation.Role}]";

                if (player.UserInformation.DoNotTrack)
                    message += " {DNT}";

                message += "\n";
            }

            ev.Reply = $"{Player.List.Count()}/{Core.MaxPlayers}\n";
            ev.Reply += !string.IsNullOrEmpty(message) ? message : "Нет игроков онлайн.";
        }

        private static void Help(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            ev.Reply = "\n";
            ev.Color = "white";

            Color Main = new Color32(255, 117, 117, 255);
            ev.Reply += ColorText("─── ⋆⋅ ✝ ⋅⋆ ─── / Команды сервера \\ ─── ⋆⋅ ✝ ⋅⋆ ───", Main);
            ev.Reply += "\n\n";


            Color Help = new Color32(255, 117, 159, 255);
            ev.Reply += ColorText("       ╭─────────────────────────.★..─╮", Help) + '\n';
            ev.Reply += ColorText("       | ⋮♯   Команда помощи: .help         |", Help) + '\n';
            ev.Reply += ColorText("       | ☆   Выводит доступные команды     |", Help) + '\n';
            ev.Reply += ColorText("       | ☣   Доступные алиасы:             |", Help) + '\n';
            ev.Reply += ColorText("       |    ⊱ .хелп ⊰    ⊱ .хэлп ⊰          |", Help) + '\n';
            ev.Reply += ColorText("       ╰─..★.─────────────────────────╯", Help) + '\n';
            ev.Reply += "\n\n";


            Color Economic = new Color32(202, 117, 255, 255);
            ev.Reply += ColorText("         •───────•°• Экономика •°•───────•", Economic);
            ev.Reply += "\n\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Economic) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда статистики: .xp               |", Economic) + '\n';
            ev.Reply += ColorText("      | ☆   Выводит вашу статистику               |", Economic) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:                     |", Economic) + '\n';
            ev.Reply += ColorText("      |   ⊱ .stats ⊰   ⊱ .lvl ⊰   ⊱ .money ⊰        |", Economic) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Economic) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Economic) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда платежа: .pay                 |", Economic) + '\n';
            ev.Reply += ColorText("      | ☆   Передает указанное количество         |", Economic) + '\n';
            ev.Reply += ColorText("      |      монеток другому игроку                |", Economic) + '\n';
            ev.Reply += ColorText("      | ♦️    Аргументы:  『 кол-во 』 『 игрок 』   |", Economic) + '\n';
            ev.Reply += ColorText("      | ➺   Пример:  「 .pay 1000 ken 」           |", Economic) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:                     |", Economic) + '\n';
            ev.Reply += ColorText("      |      ⊱ .пэй ⊰      ⊱ .пей ⊰                |", Economic) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Economic) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("         •───────•°• Экономика •°•───────•", Economic);
            ev.Reply += "\n\n";


            Color Role = new Color32(117, 163, 255, 255);
            ev.Reply += ColorText("      ─── ⋆⋅☆⋅⋆ ─── 〔 Команды ролей 〕 ─── ⋆⋅☆⋅⋆ ───", Role);
            ev.Reply += "\n\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Role) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда WinXP: .079                   |", Role) + '\n';
            ev.Reply += ColorText("      | ☆   Добавляет дополнительные               |", Role) + '\n';
            ev.Reply += ColorText("      |      функции для SCP-079                   |", Role) + '\n';
            ev.Reply += ColorText("      |      Чтобы посмотреть список способностей, |", Role) + '\n';
            ev.Reply += ColorText("      |      укажите команду без аргументов        |", Role) + '\n';
            ev.Reply += ColorText("      | ♦️    Аргументы:  『 id способности 』       |", Role) + '\n';
            ev.Reply += ColorText("      | ➺   Пример:  「 .079 1 」                  |", Role) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Role) + '\n';
            ev.Reply += "\n";

#if MRP
            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Role) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда безопасника: .ssa             |", Role) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет стать Системой              |", Role) + '\n';
            ev.Reply += ColorText("      |      Безопасности заместо SCP-079          |", Role) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Role) + '\n';
            ev.Reply += "\n";
#endif

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Role) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда пожилого: .pocket             |", Role) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет уйти в свое карманное       |", Role) + '\n';
            ev.Reply += ColorText("      |      измерение за SCP-106                  |", Role) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Role) + '\n';
            ev.Reply += "\n";

#if NR
            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Role) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда охраны: .s                    |", Role) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет сложить свои                |", Role) + '\n';
            ev.Reply += ColorText("      |      обязанности и стать ученым            |", Role) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Role) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Role) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда SCP: .force                   |", Role) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет сменить своего              |", Role) + '\n';
            ev.Reply += ColorText("      |      SCP на другого свободного             |", Role) + '\n';
            ev.Reply += ColorText("      | ♦️    Аргументы:  『 номер желаемого SCP 』  |", Role) + '\n';
            ev.Reply += ColorText("      | ➺   Пример:  「 .force 3114 」             |", Role) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Role) + '\n';
            ev.Reply += "\n";
#endif

            ev.Reply += ColorText("      ─── ⋆⋅☆⋅⋆ ─── 〔 Команды ролей 〕 ─── ⋆⋅☆⋅⋆ ───", Role);
            ev.Reply += "\n\n";

#if NR
            Color Priest = new Color32(179, 255, 117, 255);
            ev.Reply += ColorText("    ✠ ——— ✠ —- { Команды священника } —— ✠ ——— ✠", Priest);
            ev.Reply += "\n\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Priest) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда превращения: .priest          |", Priest) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет стать игровым священником,  |", Priest) + '\n';
            ev.Reply += ColorText("      |      а также собрать своих последователей |", Priest) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:                     |", Priest) + '\n';
            ev.Reply += ColorText("      |      ⊱ .pri ⊰      ⊱ .св ⊰                 |", Priest) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Priest) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Priest) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда уверования: .believe          |", Priest) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет уверовать обычным смертным  |", Priest) + '\n';
            ev.Reply += ColorText("      |      с целью призыва Отца Спасителя       |", Priest) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:                     |", Priest) + '\n';
            ev.Reply += ColorText("      |      ⊱ .bel ⊰      ⊱ .уверовать ⊰          |", Priest) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Priest) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭────────────────────────────────.★..─╮", Priest) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда призыва: .pray                |", Priest) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет призвать Отца Спасителя     |", Priest) + '\n';
            ev.Reply += ColorText("      |      в этот бренный мир. Доступно только   |", Priest) + '\n';
            ev.Reply += ColorText("      |      священнику при 3-х последователях     |", Priest) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:                     |", Priest) + '\n';
            ev.Reply += ColorText("      |      ⊱ .пр ⊰      ⊱ .призыв ⊰              |", Priest) + '\n';
            ev.Reply += ColorText("      ╰─..★.────────────────────────────────╯", Priest) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("    ✠ ——— ✠ —- { Команды священника } —— ✠ ——— ✠", Priest);
            ev.Reply += "\n\n";
#endif


            Color Other = new Color32(255, 241, 117, 255);

            ev.Reply += ColorText("      ╭─────────── · · ୨୧ · · ───────────╮", Other) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда чата: .chat             |", Other) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет написать в чат         |", Other) + '\n';
            ev.Reply += ColorText("      |      для связи с другими игроками.    |", Other) + '\n';
            ev.Reply += ColorText("      |      Напишите команду без аргументов, |", Other) + '\n';
            ev.Reply += ColorText("      |      чтобы узнать больше.             |", Other) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:                |", Other) + '\n';
            ev.Reply += ColorText("      |      ⊱ .чат ⊰      ⊱ .чай ⊰           |", Other) + '\n';
            ev.Reply += ColorText("      ╰─────────── · · ୨୧ · · ───────────╯", Other) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭─────────── · · ୨୧ · · ───────────╮", Other) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда связи: .bug             |", Other) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет сообщить тех. отделу  |", Other) + '\n';
            ev.Reply += ColorText("      |      о найденном вами баге.          |", Other) + '\n';
            ev.Reply += ColorText("      |      Обычные администраторы не       |", Other) + '\n';
            ev.Reply += ColorText("      |      видят отправленные баги, поэтому|", Other) + '\n';
            ev.Reply += ColorText("      |      вытащить из текстур не смогут   |", Other) + '\n';
            ev.Reply += ColorText("      | ♦️    Аргументы:  『 сообщение 』      |", Other) + '\n';
            ev.Reply += ColorText("      | ➺   Пример:  「 .баг Я дед инсайд 」 |", Other) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:               |", Other) + '\n';
            ev.Reply += ColorText("      |                ⊱ .баг ⊰              |", Other) + '\n';
            ev.Reply += ColorText("      ╰─────────── · · ୨୧ · · ───────────╯", Other) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭─────────── · · ୨୧ · · ───────────╮", Other) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда интересностей: .tps     |", Other) + '\n';
            ev.Reply += ColorText("      | ☆   Позволяет посмотреть TPS сервера|", Other) + '\n';
            ev.Reply += ColorText("      ╰─────────── · · ୨୧ · · ───────────╯", Other) + '\n';
            ev.Reply += "\n";

            ev.Reply += ColorText("      ╭──────── · · ☠ · · ────────╮", Other) + '\n';
            ev.Reply += ColorText("      | ⋮♯   Команда ####: .kys       |", Other) + '\n';
            ev.Reply += ColorText("      | ☆   Покиньте этот бренный    |", Other) + '\n';
            ev.Reply += ColorText("      |      мир быстро и без мучений |", Other) + '\n';
            ev.Reply += ColorText("      | ☣   Доступные алиасы:        |", Other) + '\n';
            ev.Reply += ColorText("      |           ⊱ .kill ⊰           |", Other) + '\n';
            ev.Reply += ColorText("      ╰──────── · · ☠ · · ────────╯", Other) + '\n';
            ev.Reply += "\n";


            ev.Reply += ColorText("─── ⋆⋅ ✝ ⋅⋆ ─── / Команды сервера \\ ─── ⋆⋅ ✝ ⋅⋆ ───", Main);


            static string ColorText(string original, Color color)
            {
                return $"<color={color.ToHex()}>{original}</color>";
            }
        }

        private static void LevelSet(GameConsoleCommandEvent ev)
        {
#if MRP
            if (!ev.Player.UserInformation.UserId.IsPrikols() &&
                ev.Player.UserInformation.UserId != "76561199255713504@steam") // danone twink
                return;
#elif NR
            if (!ev.Player.UserInformation.UserId.IsPrikols())
                return;
#endif

            ev.Allowed = false;

            if (ev.Args.Length == 0)
            {
                ev.Reply = "Должен быть один аргумент => level: uint";
                ev.Color = "red";
                return;
            }

            if (!uint.TryParse(ev.Args[0], out uint set_to))
            {
                ev.Reply = $"Не удалось пропарсить первый аргумент в uint: {ev.Args[0]}";
                ev.Color = "red";
                return;
            }

            if (set_to > 99)
            {
                ev.Reply = $"Переменная set_to не может быть больше 99: {set_to}";
                ev.Color = "red";
                return;
            }

            Core.Socket.Emit("database.internal.unsafe.set_level", new object[] { ev.Player.UserInformation.UserId, set_to });
            ev.Reply = "Запрос отправлен...";
            ev.Color = "white";
        }

        private static void Suicide(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            var role = ev.Player.RoleInformation.Role;

            if (role is RoleTypeId.Overwatch or RoleTypeId.Filmmaker or RoleTypeId.Spectator)
                return;

            if (role is RoleTypeId.ClassD)
            {
                string tag = " NotForce";
                ev.Player.Tag += tag;
                MEC.Timing.CallDelayed(1f, () => ev.Player.Tag.Replace(tag, ""));
            }
            ev.Player.HealthInformation.Kill("Вскрыты вены");
        }

        private static void TPS(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            ev.Reply = $"TPS: {Core.TicksMinutes}";
            ev.Player.Client.SendConsole($"Альтернативный TPS: {Server.Tps}", "white");
        }

        private static void Size(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (!ThisAdmin(ev.Player.UserInformation.UserId) && ev.Player.UserInformation.UserId != "76561199298395982@steam")
            {
                ev.Reply = "Отказано в доступе";
                return;
            }
            if (ev.Args.Length < 3)
            {
                ev.Reply = "Вводите 3 аргумента {x} {y} {z}";
                return;
            }
            if (!float.TryParse(ev.Args[0], out float x)/* || x < 0.1*/)
            {
                ev.Reply = "Неправильно указана координата {x}";
                return;
            }
            if (!float.TryParse(ev.Args[1], out float y)/* || y < 0.1*/)
            {
                ev.Reply = "Неправильно указана координата {y}";
                return;
            }
            if (!float.TryParse(ev.Args[2], out float z)/* || z < 0.1*/)
            {
                ev.Reply = "Неправильно указана координата {z}";
                return;
            }
            var target = ev.Player;
            try
            {
                string name = string.Join(" ", ev.Args.Skip(3));
                if (name.Trim() != "")
                {
                    var pl = name.GetPlayer();
                    if (pl != null) target = pl;
                }
            }
            catch { }
            target.MovementState.Scale = new(x, y, z);
            ev.Reply = $"Успешно изменен размер у {target.UserInformation.Nickname}";
        }

        private static bool ThisAdmin(string userid)
        {
            try
            {
                if (Data.Users.TryGetValue(userid, out var data) &&
                    (data.id == 1 || data.it ||
                     data.trainee || data.helper || data.mainhelper || data.admin || data.mainadmin || data.control || data.maincontrol)) return true;

                return false;
            }
            catch { return false; }
        }

#if NR
        static void Event(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (!ThisAdmin(ev.Player.UserInformation.UserId))
            {
                ev.Reply = "Отказано в доступе";
                return;
            }
            AlphaController.ChangeState(true, true);
            EventMode = true;
            ev.Reply = "Режим ивента включен:\n" +
                       "Авто-Боеголовка отключена\n" +
                       "У донатеров отключен доступ к админ-панели\n" +
                       "\nДля отключения - перезапустите раунд";
        }

        static void RoundFF(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (!ThisAdmin(ev.Player.UserInformation.UserId))
            {
                ev.Reply = "Отказано в доступе";
                return;
            }
            Server.FriendlyFire = !Server.FriendlyFire;
            ev.Reply = Server.FriendlyFire ? "Friendly Fire был включен" : "Friendly Fire был выключен";
        }

        static void TextMute(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (!ThisAdmin(ev.Player.UserInformation.UserId))
            {
                ev.Reply = "Отказано в доступе";
                return;
            }

            Chat.Main.GloballyMuted = !Chat.Main.GloballyMuted;

            ev.Reply = "Текстовой чат успешно " + (Chat.Main.GloballyMuted ? "замьючен" : "размьючен");
        }

        static bool Muted { get; set; } = false;
        static void RoundMute(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (!ThisAdmin(ev.Player.UserInformation.UserId))
            {
                ev.Reply = "Отказано в доступе";
                return;
            }

            Muted = !Muted;

            foreach (var pl in Player.List)
            {
                if (pl == ev.Player)
                    continue;

                VcMuteFlags flags = VoiceChatMutes.GetFlags(pl.ReferenceHub);
                if (Muted)
                    VoiceChatMutes.SetFlags(pl.ReferenceHub, flags |= VcMuteFlags.GlobalRegular);
                else
                    VoiceChatMutes.SetFlags(pl.ReferenceHub, flags &= ~VcMuteFlags.GlobalRegular);
            }

            ev.Reply = "Игроки успешно " + (Muted ? "замьючены" : "размьючены");
        }

        static void PicksCount(GameConsoleCommandEvent ev)
        {
            ev.Allowed = false;
            if (ev.Player.UserInformation.UserId != "76561198840787587@steam" &&
                ev.Player.UserInformation.UserId != "SERVER CONSOLE" &&
                ev.Player.UserInformation.UserId != "ID_Dedicated")
                return;
            var picks = Pickup.List;
            ev.Reply = $"All items: {picks.Count}\nClear items: {picks.Count(x => !Clear.Pickups.Contains(x.Serial))}";
        }
#endif

        [EventMethod(ServerEvents.GameConsoleCommand)]
        private static void Console(GameConsoleCommandEvent ev)
        {
#if MRP
            if (ev.Player.UserInformation.UserId is "76561198840787587@steam" or "ID_Dedicated")
#elif NR
            if (ev.Player.UserInformation.UserId is "76561198840787587@steam" or "76561199298395982@steam" or "ID_Dedicated")
#endif
            {
                switch (ev.Name)
                {
                    case "sh":
                        {
                            ev.Allowed = false;
                            if (!int.TryParse(ev.Args[0], out int mode))
                                mode = 0;

                            Player pl = null;
                            try
                            {
                                string name = string.Join(" ", ev.Args.Skip(1));
                                pl = name.GetPlayer();
                            }
                            catch { }
                            pl ??= ev.Player;

                            SerpentsHand.SpawnOne(pl, mode);
                            return;
                        }
                    case "cpir" or "csir":
                        {
                            ev.Allowed = false;
                            Player pl = null;
                            try
                            {
                                string name = string.Join(" ", ev.Args);
                                pl = name.GetPlayer();
                            }
                            catch { }
                            pl ??= ev.Player;

                            CPIR.Spawn(pl);
                            return;
                        }
                    case "co2":
                        {
                            ev.Allowed = false;
                            Player pl = null;
                            try
                            {
                                string name = string.Join(" ", ev.Args);
                                pl = name.GetPlayer();
                            }
                            catch { }
                            pl ??= ev.Player;

                            SpawnManager.SpawnProtect(pl);
                            pl.Tag += CO2.Tag;
                            pl.RoleInformation.Role = RoleTypeId.NtfCaptain;
                            Timing.CallDelayed(0.5f, () =>
                            {
                                pl.Inventory.Clear();
                                pl.GetAmmo();
                                pl.Inventory.AddItem(ItemType.KeycardMTFCaptain);
                                pl.Inventory.AddItem(ItemType.GunE11SR);
                                pl.Inventory.AddItem(ItemType.SCP500);
                                pl.Inventory.AddItem(ItemType.Radio);
                                pl.Inventory.AddItem(ItemType.GrenadeHE);
                                pl.Inventory.AddItem(ItemType.Adrenaline);
                                pl.Inventory.AddItem(ItemType.Flashlight);
                                pl.Inventory.AddItem(ItemType.ArmorCombat);
                                pl.UserInformation.CustomInfo = "Капитан | Отряд CO2";
                            });
                            pl.Client.Broadcast($"<size=70%><color=#6f6f6f>Вы - <color=#0033ff>Капитан</color> " +
                                                $"отряда активации <color=#14b1e0>CO2</color> <color=#0047ec>МОГ</color>\n" +
                                                $"Ваша задача - обеспечить активацию CO2 в комплексе</color></size>", 10, true);
                            return;
                        }
#if NR
                    case "co2group":
                        {
                            ev.Allowed = false;
                            ev.Reply = CO2.SpawnGroupFromAdmin();
                            return;
                        }
#endif
                }
            }

            if (ev.Player.UserInformation.UserId is not ("76561198840787587@steam" or "ID_Dedicated"))
                return;

            switch (ev.Name)
            {
                case "clear":
                    {
                        foreach (Pickup item in Pickup.List)
                            item.Destroy();

                        foreach (var doll in Map.Corpses)
                            doll.Destroy();

                        break;
                    }
                case "door":
                    {
                        foreach (Door door in Map.Doors)
                            Object.Destroy(door.DoorVariant);

                        break;
                    }
                case "tesla":
                    {
                        foreach (Tesla tesla in Map.Teslas)
                            tesla.Destroy();

                        break;
                    }
                case "end":
                    {
#if NR
                        ev.Allowed = false;
#endif

                        Round.End();
                        break;
                    }
#if MRP
                case "fm" or "facilitymanager":
                    {
                        if (ev.Args.Count() == 0)
                            return;

                        ev.Allowed = false;

                        FacilityManager.Spawn(string.Join(" ", ev.Args).Trim().GetPlayer());
                        ev.Reply = "Успешно";

                        break;
                    }
#endif
                case "auth":
                    {
#if NR
                        ev.Allowed = false;
#endif

                        foreach (var pl in Player.List)
                        {
                            try
                            {
                                pl.Connection.Send(new RoundRestartMessage(RoundRestartType.RedirectRestart, 10f, Core.Port, true, false));
                            }
                            catch { }
                        }

#if NR
                        Timing.CallDelayed(1f, Server.Restart);
#endif
                        break;
                    }
                case "ping":
                    {
                        ev.Allowed = false;
                        Log.Info(ev.Player.Ping);
                        ev.Reply = $"{ev.Player.Ping}";
                        break;
                    }
                case "airdrop":
                    {
                        ev.Allowed = false;
                        AirDrop.Call();
                        break;
                    }

                #region До слияния были только на NR. На MRP их не было
                case "test1":
                    {
                        ev.Allowed = false;
                        Vector3 pos = ev.Player.ReferenceHub.PlayerCameraReference.position;
                        for (float i = 0; i < 30; i++)
                        {
                            new Primitive(PrimitiveType.Cube, pos + Vector3.down * (i * 0.1f), Color.red,
                                size: new(0.1f, 0.01f, 0.1f));
                        }

                        for (int i = 0; i < 30; i++)
                        {
                            new Primitive(PrimitiveType.Cube, pos + Vector3.up * (i * 0.1f), Color.blue,
                                size: new(0.1f, 0.01f, 0.1f));
                        }

                        for (int i = 0; i < 30; i++)
                        {
                            new Primitive(PrimitiveType.Cube, pos + Vector3.left * (i * 0.1f), Color.green,
                                size: new(0.01f, 0.1f, 0.01f));
                        }

                        for (int i = 0; i < 30; i++)
                        {
                            new Primitive(PrimitiveType.Cube, pos + Vector3.right * (i * 0.1f), Color.yellow,
                                size: new(0.01f, 0.1f, 0.01f));
                        }

                        break;
                    }
                case "test2":
                    {
                        ev.Allowed = false;
                        Door door = new(ev.Player.MovementState.Position, DoorPrefabs.DoorHCZ);
                        Timing.RunCoroutine(Teleport());

                        IEnumerator<float> Teleport()
                        {
                            for (; ; )
                            {
                                try
                                {
                                    door.Position = ev.Player.MovementState.Position;
                                }
                                catch
                                {
                                    yield break;
                                }

                                yield return Timing.WaitForSeconds(1);
                            }
                        }

                        break;
                    }
                case "test3":
                    {
                        ev.Allowed = false;
                        Door door = new(ev.Player.MovementState.Position - Vector3.right * 2, DoorPrefabs.DoorHCZ);
                        for (int i = 0; i < door.GameObject.transform.childCount; i++)
                        {
                            Transform child = door.GameObject.transform.GetChild(i);
                            if (child.name.Contains("Button")) Timing.RunCoroutine(Teleport(child));
                        }

                        Timing.RunCoroutine(Update());

                        IEnumerator<float> Teleport(Transform tr)
                        {
                            for (; ; )
                            {
                                try
                                {
                                    tr.localPosition = ev.Player.MovementState.Position - door.Position;
                                }
                                catch
                                {
                                    yield break;
                                }

                                yield return Timing.WaitForSeconds(1);
                            }
                        }

                        IEnumerator<float> Update()
                        {
                            yield return Timing.WaitForSeconds(1);
                            for (; ; )
                            {
                                try
                                {
                                    NetworkServer.UnSpawn(door.GameObject);
                                    NetworkServer.Spawn(door.GameObject);
                                }
                                catch
                                {
                                    yield break;
                                }

                                yield return MEC.Timing.WaitForSeconds(5);
                            }
                        }

                        break;
                    }
                case "test4":
                    {
                        ev.Allowed = false;
                        Log.Info(ev.Player.Ping);
                        ev.Reply = $"{ev.Player.Ping}";
                        break;
                    }
                case "test6":
                    {
                        ev.Allowed = false;
                        ev.Player.Connection.Send(new RoundRestartMessage(RoundRestartType.RedirectRestart, 1f, 7666,
                            true, false));
                        break;
                    }
                case "test7":
                    {
                        ev.Allowed = false;
                        var target = ev.Player;
                        try
                        {
                            string name = string.Join(" ", ev.Args);
                            if (name.Trim() != "")
                            {
                                var pl = name.GetPlayer();
                                if (pl != null) target = pl;
                            }
                        }
                        catch
                        {
                        }

                        Log.Info(target.UserInformation.Nickname);
                        target.Connection.Send(new SceneMessage
                        {
                            sceneName = "MainMenuRemastered",
                            sceneOperation = SceneOperation.Normal,
                            customHandling = false
                        });
                        break;
                    }
                case "test8":
                    {
                        ev.Allowed = false;
                        AirDrop.Call();
                        break;
                    }
                case "test9":
                    {
                        ev.Allowed = false;
                        Stats.AddMoney(ev.Player, int.Parse(ev.Args[0]));
                        break;
                    }
                #endregion

                case "test10":
                    {
                        ev.Allowed = false;

                        int prims = 0;
                        foreach (var prim in Object.FindObjectsOfType<AdminToys.PrimitiveObjectToy>())
                        {
                            try
                            {
                                prims++;
                                NetworkServer.Destroy(prim.gameObject);
                            }
                            catch { }
                        }

                        int lights = 0;
                        foreach (var light in Object.FindObjectsOfType<AdminToys.LightSourceToy>())
                        {
                            try
                            {
                                NetworkServer.Destroy(light._light.gameObject);
                                lights++;
                            }
                            catch { }
                        }

                        ev.Reply = $"Destroyed: {prims} prims & {lights} lights";
                        break;
                    }
                case "test11":
                    {
                        ev.Allowed = false;

                        int prims = 0;
                        foreach (var prim in Object.FindObjectsOfType<AdminToys.PrimitiveObjectToy>())
                        {
                            try
                            {
                                Object.Destroy(prim);
                                prims++;
                            }
                            catch { }
                        }

                        int lights = 0;
                        foreach (var light in Object.FindObjectsOfType<AdminToys.LightSourceToy>())
                        {
                            try
                            {
                                Object.Destroy(light);
                                lights++;
                            }
                            catch { }
                        }

                        ev.Reply = $"Destroyed: {prims} prims & {lights} lights";
                        break;
                    }
                case "test12":
                    {
                        foreach (Room room in Map.Rooms)
                        {
                            if (room.NetworkIdentity is not null)
                                Log.Info(room.Type);
                        }
                        break;
                    }
                case "test13":
                    {
                        foreach (var component in ev.Player.GamePlay.Room.GameObject.GetComponentsInChildren<Component>())
                        {
                            try
                            {
                                if (component.name.Contains("SCP-079") || component.name.Contains("CCTV"))
                                {
                                    //Log.Debug($"Prevent from destroying: {component.name} {component.tag} {component.GetType().FullName}");
                                    continue;
                                }

                                if (component.GetComponentsInParent<Component>()
                                    .Any(c => c.name.Contains("SCP-079") || c.name.Contains("CCTV")))
                                {
                                    //Log.Debug($"Prevent from destroying: {component.name} {component.tag} {component.GetType().FullName}");
                                    continue;
                                }

                                //Log.Debug($"Destroying component: {component.name} {component.tag} {component.GetType().FullName}");

                                Object.Destroy(component);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                        ev.Player.Client.Reconnect();
                        break;
                    }
                case "test14":
                    {
                        ev.Player.GamePlay.Room.Position = Vector3.zero;
                        break;
                    }
                case "test15":
                    {
                        foreach (Room room in Map.Rooms)
                            room.Position = Vector3.zero;

                        ev.Player.Client.Reconnect();
                        break;
                    }
                case "test16":
                    {
                        ev.Reply = $"lift: {ev.Player.GamePlay.Lift}; is_null: {ev.Player.GamePlay.Lift is null}";
                        break;
                    }
                case "test17":
                    {
                        ev.Reply = $"value: {ev.Player.RoleInformation.Scp106.StalkAbility.StalkActive};";
                        ev.Player.RoleInformation.Scp106.StalkAbility.StalkActive = true;
                        ev.Player.RoleInformation.Scp106.StalkAbility.UpdateServerside();
                        break;
                    }
#if MRP
                case "test18":
                    {
                        Scp096Rework.PutCell(ev.Player);
                        break;
                    }
#endif
                case "test19":
                    {
                        ev.Allowed = false;
                        Vector2 size = Worker.CalculateContentSize(string.Join("", ev.Args));
                        ev.Reply = $"{size}";
                        break;
                    }
                case "prims":
                    {
                        ev.Reply = $"Prims: {Map.Primitives.Count};\n" +
                            $"nonstatic prims: {Map.Primitives.Count(x => !x.IsStatic)};";
                        break;
                    }
                case "shsquad":
                    {
                        ev.Allowed = false;
                        SerpentsHand.Spawn();
                        break;
                    }
                case "hehe":
                    {
                        ev.Allowed = false;
                        ev.Player.RoleInformation.Scp079.Lvl = 5;
                        ev.Player.RoleInformation.Scp079.MaxEnergy = 200;
                        ev.Player.RoleInformation.Scp079.Energy = 200;
                        break;
                    }
                case "name":
                    {
                        ev.Allowed = false;
                        ev.Player.UserInformation.Nickname = string.Join(" ", ev.Args);
                        break;
                    }
            }
        }
    }
}