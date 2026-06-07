#if MRP
using Loli.Addons.Hints;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;

namespace Loli.Addons.RolePlay
{
    static class RPNames
    {
        /*
        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev)
            => Timing.CallDelayed(2f, () => SpawnDo(ev.Player));
        */

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
            => Timing.CallDelayed(1f, () => SpawnDo(ev.Player));

        static void SpawnDo(Player pl)
        {
            RoleTypeId role = pl.RoleInformation.Role;

            if (role is RoleTypeId.ClassD)
                return;

            if (pl.GetTeam() is Team.SCPs)
            {
                pl.Variables.Remove("RPName");
                MainInfo.UpdateName(pl, string.Empty);
                return;
            }

            if (role is RoleTypeId.Spectator or RoleTypeId.Filmmaker or RoleTypeId.Overwatch)
            {
                pl.Variables.Remove("RPName");
                pl.UserInformation.CustomInfo = "";
                MainInfo.UpdateName(pl, string.Empty);
                return;
            }

            if (pl.UserInformation.CustomInfo.Contains("|"))
                pl.UserInformation.CustomInfo = pl.UserInformation.CustomInfo.Split('|')[0];

            string name = $"{Names.RandomItem()} {Subnames.RandomItem()}";

            if (pl.Variables["RPName"] is string oldName)
            {
                pl.UserInformation.CustomInfo = pl.UserInformation.CustomInfo.Replace("| " + oldName, "").Replace(oldName, "");
            }

            pl.Variables["RPName"] = name;
            pl.UserInformation.CustomInfo += (!string.IsNullOrEmpty(pl.UserInformation.CustomInfo) ? $" | " : "") + name;
            pl.UserInformation.InfoToShow = PlayerInfoArea.Nickname | PlayerInfoArea.Badge | PlayerInfoArea.CustomInfo |
                PlayerInfoArea.Role | PlayerInfoArea.UnitName | PlayerInfoArea.PowerStatus;

            pl.Client.SendConsole($"Ваше игровое имя: {name}", "red");

            MainInfo.UpdateName(pl, name);
        }

        static readonly string[] Names = new[]
        {
            "Генри",
            "Чанг",
            "Алексей",
            "Борис",
            "Райан",
            "Михаил",
            "Клим",
            "Адольф",
            "Джекет",
            "Рихтер",
            "Максим",
            "Тарасов",
            "Чарли",
            "Алекс",
            "Ди",
            "Джованни",
            "Лука",
            "Виктор",
            "Элай",
            "Михаил",
            "Панчо",
            "Айзек",
            "Джонатан",
            "Гарри",
            "Даниэль",
            "Адриан",
            "Марк",
            "Александр",
            "Дерек",
            "Эйрон",
            "Логан",
            "Ксавьер",
            "Этан",
            "Гарет",
            "Пол",
            "Патрик",
            "Мэтью",
            "Френсин",
            "Люк",
            "Патрик",
            "Лукас",
            "Данил",
            "Джеймс",
            "Гавриил",
            "Сэт",
            "Энтони",
            "Арнольд",
            "Патрик",
            "Исаак",
            "Льюис",
            "Джереми",
            "Люк",
            "Джеймс",
            "Ной",
            "Кристофер",
            "Пол",
            "Колин",
            "Брайден",
            "Тристан",
            "Алекс",
            "Томас",
            "Алекс",
            "Чарльз",
            "Улисс",
            "Джозеф",
            "Кристиан",
            "Готхильф",
            "Герт",
            "Фалько",
            "Давид",
            "Кристиан",
            "Фернандо",
            "Рафаэль",
            "Пабло",
            "Андрес",
            "Диего",
            "Хуан",
            "Патрик",
            "Эдуардо",
            "Тайлер",
            "Хоси",
            "Ичиро",
            "Хироки",
            "Коичи",
            "Луи",
            "Ксавье",
            "Юбер",
            "Анатоль",
            "Ален",
            "Хейкки",
            "Каарло",
            "Нестеров",
            "Гуляев"
        };

        static readonly string[] Subnames = new[]
        {
            "Рихмэнс",
            "Моллсон",
            "Вишнёв",
            "Трийсон",
            "Гослинг",
            "Каспер",
            "Сергеев",
            "Чаксон",
            "Молс",
            "Крайсон",
            "Яблоков",
            "Борис",
            "Экрис",
            "Дэниэль",
            "Лоренцо",
            "Леони",
            "Клайд",
            "Кукурузов",
            "Вэнс",
            "Деревьев",
            "Гонзало",
            "Мокровский",
            "Бласковиц",
            "Мовский",
            "Кровский",
            "Шепард",
            "Твенсон",
            "Мокров",
            "Томас",
            "Тайлер",
            "Дикинсон",
            "Кроссман",
            "Смит",
            "Браун",
            "Грин",
            "Клиффорд",
            "Клэптон",
            "Девис",
            "Фултон",
            "Хейг",
            "Бейкер",
            "Картер",
            "Албертсон",
            "Янг",
            "Хэлл",
            "Гилл",
            "Бозуорт",
            "Эдвардс",
            "Франсис",
            "Гитлер",
            "Артурз",
            "Фиршес",
            "Эдвардс",
            "Арчибелд",
            "Фриман",
            "Галбрейт",
            "Дей",
            "Артурз",
            "Форман",
            "Нельсон",
            "Кэррол",
            "Далтонон",
            "Далган",
            "Мартинес",
            "Эдуард",
            "Эриксон",
            "Брауэр",
            "Хазе",
            "Штейман",
            "Фидлер",
            "Глёкнер",
            "Аккер",
            "Маэстро",
            "Карденас",
            "Акоста",
            "Сото",
            "Ортис",
            "Мартинес",
            "Бейтман",
            "Домингес",
            "Дерден",
            "Ёсида",
            "Саито",
            "Адо",
            "Яманака",
            "Бланшар",
            "Блондир",
            "Лепин",
            "Шарбонно",
            "Форте",
            "Миккола",
            "Виртанен",
            "Дамир"
        };
    }
}
#endif