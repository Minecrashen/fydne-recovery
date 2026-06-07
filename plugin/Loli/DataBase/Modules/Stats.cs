using Newtonsoft.Json;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;

namespace Loli.DataBase.Modules;

static class Stats
{
    [EventMethod(RoundEvents.Waiting)]
    static void NullCall() { }

    static Stats()
    {
        Core.Socket.On("database.get.stats", obj =>
        {
            string userid = obj[1].ToString();
            var pl = userid.GetPlayer();

            if (pl is null)
                return;

            SocketStatsData json = JsonConvert.DeserializeObject<SocketStatsData>(obj[0].ToString());

            if (!Data.Users.TryGetValue(userid, out var data))
                return;

            int oldlvl = data.lvl;
            data.xp = json.xp;
            data.lvl = json.lvl;
            data.to = json.to;
            data.money = json.money;

            if (oldlvl != json.lvl)
            {
                pl.Client.Broadcast(10, $"<color=#fdffbb>Вы получили {json.lvl} уровень!\nДо следующего уровня вам не хватает {json.to - json.xp}xp.</color>");
                Levels.SetPrefix(pl);
            }

        });

        Core.Socket.On("database.update.zero.money", obj =>
        {
            bool itsDiscord = $"{obj[0]}" == "true";
            string userid = obj[1].ToString() + (itsDiscord ? "@discord" : "@steam");

            if (!Data.Users.TryGetValue(userid, out var data))
                return;

            data.money = 0;

            var pl = userid.GetPlayer();

            if (pl is null)
                return;

            Levels.ShowHint(pl, "0 💰");
            pl.Client.SendConsole("У вас закончились монетки", "red");
        });
    }

    static internal void Add(Player pl, int xp, int money)
    {
        Core.Socket.Emit("database.add.stats", new object[] { pl.UserInformation.UserId.Replace("@steam", "").Replace("@discord", ""),
            pl.UserInformation.UserId.Contains("discord"), xp, money, pl.UserInformation.UserId});
    }

    static internal void AddMoney(Player pl, int money)
        => Add(pl, 0, money);

    static internal void AddXP(Player pl, int xp)
        => Add(pl, xp, 0);

}