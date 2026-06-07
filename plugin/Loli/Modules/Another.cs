using Loli.DataBase;
using Loli.DataBase.Modules;
using Loli.HintsCore;
using Loli.Webhooks;
using MEC;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Modules
{
    static class Another
    {
        [EventMethod(PlayerEvents.Leave)]
        static void Leave(LeaveEvent ev)
        {
            if (!Round.Ended)
                Core.Socket.Emit("server.leave", new object[] { Core.ServerID, ev.Player.UserInformation.UserId });
        }

        [EventMethod(RoundEvents.End)]
        static void ClearIps() => Core.Socket.Emit("server.clearips", new object[] { Core.ServerID });

        [EventMethod(RoundEvents.Waiting)]
        static void WaitingPlayers()
        {
            //Log.Info(StringUtils.Base64Encode(ServerConsole.singleton.RefreshServerNameSafe()).Replace('+', '-'));
            try { GameObject.Find("StartRound").transform.localScale = Vector3.zero; } catch { }
            //try { Server.Host.ReferenceHub.nicknameSync.Network_displayName = "C.A.S.S.I.E"; } catch { }
        }

        [EventMethod(MapEvents.TriggerTesla)]
        static void Tesla(TriggerTeslaEvent ev)
        {
#if MRP
            if (ev.Player.RoleInformation.Faction == Faction.FoundationStaff)
                ev.Allowed = false;
#elif NR
            try { if (!Alpha.Active) ev.Allowed = false; }
            catch { ev.Allowed = false; }
#endif
        }

        [EventMethod(RoundEvents.End)]
        static void RoundEnd()
        {
            Timing.CallDelayed(0.5f, () =>
            {
                foreach (Player player in Player.List)
                    Levels.SetPrefix(player);
            });
        }

#if NR
        [EventMethod(PlayerEvents.Attack)]
        static void EndFF(AttackEvent ev)
        {
            if (!Round.Ended)
                return;

            ev.Allowed = true;
            ev.FriendlyFire = false;
            if (ev.Damage == 0)
                ev.Damage = 10f;
        }
#endif

        [EventMethod(PlayerEvents.Join)]
        static void DoSpawn(JoinEvent ev)
        {
            DoSpawn(ev.Player);
        }


        [EventMethod(PlayerEvents.Dead)]
        static void DoSpawn(DeadEvent ev)
        {
            if (ev.Attacker.RoleInformation.Role != RoleTypeId.Scp049
                && ev.Attacker.RoleInformation.Role != RoleTypeId.Scp0492)
            {
                DoSpawn(ev.Target);
            }
        }

        static void DoSpawn(Player pl)
        {
            if (Round.ElapsedTime.TotalSeconds < 90 && Round.Started)
            {
                Timing.CallDelayed(0.5f, () =>
                {
                    if (pl.RoleInformation.Role == RoleTypeId.Spectator && !pl.Tag.Contains("NotForce"))
                        pl.RoleInformation.SetNew(RoleTypeId.ClassD, RoleChangeReason.Respawn);
                });
            }
        }

        [EventMethod(PlayerEvents.Attack, int.MinValue)]
        static void DamageHint(AttackEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Target == ev.Attacker)
                return;

            if (ev.Target.GamePlay.GodMode)
                return;


            string color;
            if (20 > ev.Damage)
                color = "#2dd300";
            else if (50 > ev.Damage)
                color = "#ff9c00";
            else
                color = "#ff0000";

            if (ev.Damage == -1)
                color = "#ff0000";


            if (!ev.Attacker.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
                return;

            var block = new DisplayBlock(new(UnityEngine.Random.Range(-500, 500), UnityEngine.Random.Range(-400, 400)), new(200, 100));
            var message = new MessageBlock($"{(ev.Damage == -1 ? "Убит" : Math.Round(ev.Damage))}", color.ColorFromHex(), "180%");

            display.AddBlock(block);
            block.Contents.Add(message);

            Timing.CallDelayed(2f, () =>
            {
                block.Contents.Remove(message);
                display.RemoveBlock(block);

                block = null;
                message = null;
            });
        }

        [EventMethod(PlayerEvents.Join)]
        static void ChangeNick(JoinEvent ev)
        {
            if (ev.Player.UserInformation.UserId is "76561198840787587@steam")
            {
                //ev.Player.UserInformation.Nickname = "Epodopef";
            }
        }



        static readonly string[] WhiteList = [];
        static readonly List<string> CachedSended = [];

        [EventMethod(PlayerEvents.Join)]
        static void BlockKids(JoinEvent ev)
        {
            string userId = ev.Player.UserInformation.UserId;

            if (!userId.EndsWith("@steam"))
                return;

            if (WhiteList.Contains(userId))
                return;

            new Thread(() =>
            {
                DoubfulData data = getData();

                if (data is null)
                {
                    return;
                }

                if (data.Banned && data.DaysSinceLastBan < 30 && (data.GameHours < 3600 || data.Level < 3))
                {
                    bool kick = data.GameHours < 3000;
                    int tryis = 0;

                    while (kick && tryis < 5)
                    {
                        tryis++;

                        if (Data.Users.TryGetValue(userId, out var imain))
                        {
                            kick = imain.lvl < 7;
                            if (imain.lvl != -1)
                                break;
                        }

                        Thread.Sleep(1000);
                    }

                    DoRun(data, kick, text: "Недавно была получена игровая блокировка");
                    return;
                }

                var created_array = data.CreatedFormatted.Split('.');
                int created_year = int.Parse(created_array[2]);
                DateTime register = new(created_year, int.Parse(created_array[1]), int.Parse(created_array[0]));
                TimeSpan elapsed = DateTime.Now - register;

                if (elapsed.TotalDays < 3)
                {
                    DoRun(data, text: "Аккаунт создан совсем недавно");
                    return;
                }

                if (data.GameHours < 1200 && data.Level < 3 && created_year != 1970 && elapsed.TotalDays < 30)
                {
                    DoRun(data, false, text: "Мало часов в SCPSL, низкий уровень Steam, аккаунт создан недавно");
                    return;
                }

                if (created_year == 1970 || elapsed.TotalDays < 16 || !data.IsSetup)
                {
                    GeoIP geoip = GetGeoIP();

                    if (geoip is null)
                    {
                        return;
                    }

                    if (Check(geoip, "AS16345 PJSC \"Vimpelcom\"", city: "Saratov"))
                        return;

                    if (Check(geoip, "AS16345 PJSC \"Vimpelcom\"", city: "Lyubertsy"))
                        return;

                    bool Check(GeoIP json, string org, string city = "", string region = "")
                    {
                        if ((json.City == city || json.Region == region) && json.Org == org)
                        {
                            DoRun(data, text: $"Подключение с провайдера в ЧС\nПровайдер: ||{json.Org}||\nГород: ||{json.City} ({json.Country})||");
                            return true;
                        }
                        return false;
                    }
                }

            }).Start();

            DoubfulData getData()
            {
                try
                {
                    var url = $"{Core.APIUrl}/doubtful?steam={userId.Replace("@steam", "")}";
                    var request = WebRequest.Create(url);
                    request.Method = "POST";

                    using var webResponse = request.GetResponse();
                    using var webStream = webResponse.GetResponseStream();
                    using var reader = new StreamReader(webStream);

                    var data = reader.ReadToEnd();
                    DoubfulData json = JsonConvert.DeserializeObject<DoubfulData>(data);
                    return json;
                }
                catch
                {
                    return null;
                }
            }
            ;

            GeoIP GetGeoIP()
            {
                try
                {
                    var url = $"{Core.APIUrl}/geoip?ip={ev.Player.UserInformation.Ip}";
                    var request = WebRequest.Create(url);
                    request.Method = "POST";
                    using var webResponse = request.GetResponse();
                    using var webStream = webResponse.GetResponseStream();
                    using var reader = new StreamReader(webStream);
                    var data = reader.ReadToEnd();
                    GeoIP json = JsonConvert.DeserializeObject<GeoIP>(data);
                    return json;
                }
                catch
                {
                    return null;
                }
            }
            ;

            void DoRun(DoubfulData data, bool disconnect = true, string text = "")
            {
                if (disconnect)
                {
                    disconnect = false;
                    Log.Info(ev.Player.UserInformation.Nickname + " connected with AC Warning");
                    // ev.Player.Client.Disconnect("<b><color=red>Вам был закрыт доступ на сервер</color>\n" +
                    //     "<color=#00ff19>Определение автоматизировано,\n" +
                    //     "Если вас случайно занесло в черный список,\n" +
                    //     "Откройте тикет на сервере в Discord</color></b>"
                    // );
                }

                if (CachedSended.Contains(userId))
                    return;

                CachedSended.Add(userId);

                SteamMainInfoApi json = new() { Name = ev.Player.UserInformation.Nickname };
                int serverLvl = -1;

                try
                {
                    var url = "https://" + $"api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Core.SteamToken}&format=json&steamids=" + data.Id;
                    var request = WebRequest.Create(url);
                    request.Method = "GET";
                    using var webResponse = request.GetResponse();
                    using var webStream = webResponse.GetResponseStream();
                    using var reader = new StreamReader(webStream);
                    var reply = reader.ReadToEnd();
                    var privacy = JObject.Parse(reply);
                    var pls = privacy["response"]["players"];
                    json = pls.ToObject<SteamMainInfoApi[]>()[0];
                }
                catch { }

                if (Data.Users.TryGetValue(userId, out var imain))
                    serverLvl = imain.lvl;

                string hook = "https://discord.com/api/webhooks/1203544299879538708/Kw37zdaE1r36S1rQs-OjTI9WRKWbVlBRdrP649CORtdYTdo_t8DblxVD9j_cUgB_GSaa";
                Dishook webhk = new(hook);
                Embed embed = new()
                {
                    Title = "Попытка подключения",
                    Url = "https://steamcommunity.com/profiles/" + data.Id,
                    Color = disconnect ? 16202518 : 16753920,
                    Thumbnail = new()
                    {
                        Url = json.AvatarFull
                    },
                    Description = $"**Ник:** {json.Name ?? ev.Player.UserInformation.Nickname} \n" +
                    $"**UserID:** {userId} \n" +
                    $"**IP:** ||{ev.Player.UserInformation.Ip}|| \n" +
                    $"**Аккаунт создан:** {data.CreatedFormatted} \n" +
                    $"**Уровень в Steam:** {data.Level} \n" +
                    $"**Уровень на сервере:** {serverLvl} \n" +
                    $"**Часов в SCPSL:** {(data.GameHours == 0 ? "Неизвестно" : Math.Floor((decimal)(data.GameHours / 60)))} \n" +
                    $"**Настроен ли профиль в Steam:** {(data.IsSetup ? "Да" : "Нет")} \n" +
                    $"**Наличие банов в Steam:** {(data.Banned ? "Имеются" : "Нет")} \n" +
                    (data.Banned ? $"**Прошло с последнего бана в Steam:** {data.DaysSinceLastBan} дней \n" : string.Empty) +
                    $" \n" +
                    $"Причина репорта: \n" +
                    $"{text}",
                    Footer = new()
                    {
                        Text = disconnect ? "🛑 доступ на сервер закрыт" : "⚠️ предупреждение для администрации",
                    },
                    TimeStamp = DateTimeOffset.Now
                };
                List<Embed> listEmbed = new() { embed };
                webhk.Send("Замечена попытка подключения на сервер мутной личности.", Core.ServerName, null, false, embeds: listEmbed);
            }
            ;
        }
    }
}