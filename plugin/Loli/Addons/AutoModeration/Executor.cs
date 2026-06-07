using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Loli.DataBase;
using Loli.DataBase.Modules;
using Loli.Webhooks;
using MEC;
using Newtonsoft.Json;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.World;
using Qurre.Events;
using UnityEngine;

#if MRP
using System.Text;
#elif NR
using System.IO;
#endif

namespace Loli.Addons.AutoModeration;

static class Executor
{
#if MRP
    private static readonly HttpClient _httpClient;
    private static readonly HashSet<string> _cachedSends = [];
    private static Task _cachedTask;
#endif

    private const string ThreadName = "Loli.Addons.AutoModeration.Executor";

    internal static readonly HashSet<string> Messages = [];
    internal static readonly HashSet<string> CachedMeta = [];

    static Executor()
    {
#if MRP
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            "TG9saSBQcm9qZWN0Ok00OD85fl4sSm0hczVgU2lYfkpUI0o5SH4sdzssR283PTdIbg==");
#endif
    }

    [EventMethod(RoundEvents.Start)]
    private static void Register()
    {
        Messages.Clear();
        CachedMeta.Clear();
        Timing.RunCoroutine(Сycle(), ThreadName);
        Timing.RunCoroutine(PullMetadata(), ThreadName);
    }

#if MRP
    [EventMethod(RoundEvents.Restart)]
    [EventMethod(RoundEvents.End)]
    private static void UnRegister()
    {
        Timing.KillCoroutines(ThreadName);
        _cachedTask = null;
    }
#elif NR
    [EventMethod(RoundEvents.End)]
    private static void UnRegister()
    {
        Timing.KillCoroutines(ThreadName);
    }
#endif

    static IEnumerator<float> PullMetadata()
    {
        for (; ; )
        {
            yield return Timing.WaitForSeconds(5f);
            try
            {
                HashSet<Metadata> metadata = [];
                long date = DateTimeOffset.Now.ToUnixTimeSeconds();

                foreach (Player pl in Player.List)
                {
                    Vector3 pos = pl.MovementState.Position;

                    int serverLvl = -1;
                    if (Data.Users.TryGetValue(pl.UserInformation.UserId, out UserData main))
                        serverLvl = main.lvl;

                    metadata.Add(new Metadata
                    {
#if NR
                        Date = date,
                        Round = Round.CurrentRound,
                        RoundStarted = new DateTimeOffset(Round.StartedTime).ToUnixTimeSeconds(),
#endif

                        Connected = new DateTimeOffset(pl.JoinedTime).ToUnixTimeSeconds(),
                        Spawned = new DateTimeOffset(pl.SpawnedTime).ToUnixTimeSeconds(),
                        Userid = pl.UserInformation.UserId,
                        PlayerId = pl.UserInformation.Id,
                        Level = serverLvl,
                        Nickname = pl.UserInformation.Nickname,
                        Admin = pl.Administrative.RemoteAdmin,
                        Role = pl.RoleInformation.Role.ToString(),
                        Team = pl.RoleInformation.Team.ToString(),
                        Faction = pl.RoleInformation.Faction.ToString(),
                        Position = $"x: {pos.x}, y: {pos.y}, z: {pos.z}",
                        Kills = pl.StatsInformation.KillsCount,
                        Deaths = pl.StatsInformation.DeathsCount,

#if MRP
                        KillsDesc = pl.StatsInformation.Kills.Select(x => x.ToString()).ToList(),
#endif

                        Health = $"{pl.HealthInformation.Hp}/{pl.HealthInformation.MaxHp}",
                        Ahp = $"{pl.HealthInformation.Ahp}/{pl.HealthInformation.MaxAhp}",
                        Stamina = pl.HealthInformation.Stamina,
                        Cuffed = pl.GamePlay.Cuffed,
                        Cuffer = pl.GamePlay.Cuffer?.UserInformation.Nickname,
                        Room = pl.GamePlay.Room.Name,
                        Zone = pl.GamePlay.CurrentZone.ToString()
                    });
                }

#if MRP
                GlobalMetadata globalMetadata = new()
                {
                    Date = date,
                    Round = Round.CurrentRound,
                    RoundStarted = new DateTimeOffset(Round.StartedTime).ToUnixTimeSeconds(),
                    Metadata = metadata
                };

                CachedMeta.Add(JsonConvert.SerializeObject(globalMetadata));

#elif NR
                CachedMeta.Add(JsonConvert.SerializeObject(metadata));
#endif
            }
            catch (Exception err)
            {
                Log.Error(err);
            }
        }
    }

    static IEnumerator<float> Сycle()
    {
        for (; ; )
        {
            yield return Timing.WaitForSeconds(60f);
            Log.Info("[AI] Checking start");

#if MRP
            if (_cachedTask != null)
                while (!_cachedTask.IsCompleted && !_cachedTask.IsCanceled && !_cachedTask.IsFaulted)
                    yield return Timing.WaitForSeconds(5f);
#endif

            try
            {

                string log = string.Join("\n", Messages);
                string meta = string.Join("\n", CachedMeta);

                Messages.Clear();
                CachedMeta.Clear();

                if (Round.Ended || string.IsNullOrWhiteSpace(log))
                    continue;

#if MRP
                var payload = new
                {
                    metadata = meta,
                    log      = log,
                    server   = $"mrp-{DateTime.Now:dd.MM.yyyy}"
                };
                
                StringContent content = new(
                    JsonConvert.SerializeObject(payload, Formatting.None),
                    Encoding.UTF8,
                    "application/json");
                
                _cachedTask = HandleRequest(_httpClient.PostAsync(
                    "http://188.227.21.10:5678/webhook/2835a0dc-00c6-4765-8af4-428435f95122",
                    content), "sending the command response");
#elif NR
                File.AppendAllText(Path.Combine(Paths.Logs, $"AI-Traine-{Server.Port}.txt"), $"[METADATA]\n{meta}\n[/METADATA]\n\n[LOGS]\n{log}\n[/LOGS]\n\n");
#endif
            }
            catch (Exception err)
            {
                Log.Error(err);
            }

            Log.Info("[AI] Checking end");
        }
    }

#if MRP
    private static async Task HandleRequest(Task<HttpResponseMessage> task, string job = "unknown")
    {
        try
        {
            HttpResponseMessage response = await task;

            string responseString = await response.Content.ReadAsStringAsync();

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Log.Error($"Caused error when {job}:\n{responseString}");
                return;
            }
            
            Log.Info(responseString);
            
            Output json = JsonConvert.DeserializeObject<Output>(responseString);
            List<Embed> listEmbed = [];
            
            Log.Info($"Json output: {json.Out.Count}");
            
            foreach (var data in json.Out)
            {
                bool banned = false;
                string userid = data.Key;
                
                if (!userid.Contains("@"))
                    userid = $"{userid}@steam";
                
                Player pl = userid.GetPlayer();
                
                if (pl is null)
                    continue;

                int serverLvl = -1;
                if (Data.Users.TryGetValue(pl.UserInformation.UserId, out UserData imain))
                    serverLvl = imain.lvl;

                if (data.Value.AutoBan && !pl.UserInformation.UserId.EndsWith("@northwood") &&
                    !pl.Administrative.RemoteAdmin && !pl.RoleInformation.IsScp && (serverLvl < 10 ||
                        pl.StatsInformation.Kills.Any(x => x.Target.Role.GetTeam() == x.Killer.Role.GetTeam())))
                {
                    pl.Administrative.Ban(43_200,
                        $"{data.Value.Message}<br><br>-- AI DETECTED --<br>Если вас забанило по ошибке, откройте тикет.",
                        "AI MODERATOR");
                    banned = true;
                }

                if (string.IsNullOrWhiteSpace(data.Value.Message) || data.Value.Potentially == 0)
                    continue;
                
                if (!_cachedSends.Add($"{data.Value.AutoBan}:{data.Value.Message}:{data.Value.Potentially}"))
                    continue;
                
                Embed embed = new()
                {
                    Color = banned ? 16202518 : 16753920,
                    Author = new EmbedAuthor
                    {
                        Name = $"{pl.UserInformation.Nickname} | {serverLvl} уровень на сервере | {pl.UserInformation.UserId}"
                    },
                    Footer = new EmbedFooter { Text = Server.Ip + ":" + Core.Port },
                    TimeStamp = DateTimeOffset.Now,
                    Description = $"{data.Value.Message}\nВероятность нарушения: {data.Value.Potentially * 100}%",
                };
                listEmbed.Add(embed);
            }
            
            Log.Info($"Embed count: {listEmbed.Count}; Any: {listEmbed.Any()}");

            if (!listEmbed.Any())
                return;
            
            new Dishook("https://discord.com/api/webhooks/1174263432720171018/h9g7a91dFR8onu63dFViAkxr-zmNo6I-mMaiSaL5waN5Ykr5JiFeDU6V5m9xoks49zLk")
                .Send("Детект нарушения правил", Core.ServerName, embeds: listEmbed);
        }
        catch (Exception ex)
        {
            Log.Error($"Caused error in {job}:\n{ex}");
        }
    }
#endif

    [Serializable]
    internal class Output
    {
        [JsonProperty("output")]
        public Dictionary<string, Warns> Out { get; set; }
    }

    [Serializable]
    internal class Warns
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("auto_ban")]
        public bool AutoBan { get; set; }

        [JsonProperty("potentially")]
        public float Potentially { get; set; }
    }

#if MRP
    [Serializable]
    internal class GlobalMetadata
    {
        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("round")]
        public int Round { get; set; }

        [JsonProperty("roundStarted")]
        public long RoundStarted { get; set; }
        
        [JsonProperty("playersMetadata")]
        public HashSet<Metadata> Metadata { get; set; }
    }
#endif

    [Serializable]
    internal class Metadata
    {
#if NR
        [JsonProperty("date")]
        public long Date { get; set; }

        [JsonProperty("round")]
        public int Round { get; set; }

        [JsonProperty("roundStarted")]
        public long RoundStarted { get; set; }
#endif

        [JsonProperty("connectedDate")]
        public long Connected { get; set; }

        [JsonProperty("spawnedTime")]
        public long Spawned { get; set; }

        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        [JsonProperty("userid")]
        public string Userid { get; set; }

        [JsonProperty("playerId")]
        public int PlayerId { get; set; }

        [JsonProperty("level")]
        public int Level { get; set; }

        [JsonProperty("isAdmin")]
        public bool Admin { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("Faction")]
        public string Faction { get; set; }

        [JsonProperty("position")]
        public string Position { get; set; }

        [JsonProperty("kills")]
        public int Kills { get; set; }

        [JsonProperty("deaths")]
        public int Deaths { get; set; }

#if MRP
        [JsonProperty("killsDesc")]
        public List<string> KillsDesc { get; set; }
#endif

        [JsonProperty("health")]
        public string Health { get; set; }

        [JsonProperty("artifict_health")]
        public string Ahp { get; set; }

        [JsonProperty("stamina")]
        public float Stamina { get; set; }

        [JsonProperty("cuffed")]
        public bool Cuffed { get; set; }

        [JsonProperty("cuffer_nickname")]
        public string Cuffer { get; set; }

        [JsonProperty("room")]
        public string Room { get; set; }

        [JsonProperty("zone")]
        public string Zone { get; set; }
    }
}