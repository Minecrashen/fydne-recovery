using Loli.DataBase.Modules;
using Loli.HintsCore;
using Newtonsoft.Json;
using PlayerRoles;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Loli.Addons.Hints;

static class ClansRecs
{
    static readonly DisplayBlock Block;
    static ClanInfo Clan;

    static ClansRecs()
    {
        Block = new(new(600, 530), new(350, 300));
        TaskRun();
    }

    [EventMethod(PlayerEvents.Spawn)]
    static void Spawn(SpawnEvent ev)
    {
        if (!ev.Player.Variables.TryGetAndParse(Constants.VariableTag, out PlayerDisplay display))
            return;

        if (ev.Role is RoleTypeId.Spectator or RoleTypeId.Overwatch)
            display.AddBlock(Block);
        else
            display.RemoveBlock(Block);

    }

    static void TaskRun()
    {
        new Task(async () =>
        {
            while (true)
            {
                var tags = Data.Clans.Select(x => x.Key);
                string tag = tags.ElementAt(Random.Range(0, tags.Count() - 1));

                Task<string> resp = Extensions.SendApiReq($"clan?tag={tag}&type=info", new());
                resp.Wait();

                if (resp.IsCompleted)
                {
                    try
                    {
                        Clan = JsonConvert.DeserializeObject<ClanInfo>(resp.Result);

                        Block.Contents.Clear();
                        Block.Contents.Add(new(
                            Clan.Name.Replace("<", "{").Replace(">", "}").Replace("\u003c", "{").Replace("\u003e", "}"),
                            Clan.Color.ColorFromHex(), "70%"
                            ));
                        Block.Contents.Add(new($"{Clan.Money} 💰 | {Clan.Tag} | 💳 {Clan.Balance}",
                            new Color32(201, 198, 2, 255), "70%") // #c9c602
                            );
                        Block.Contents.Add(new($"{Clan.Boosts} бустов 🚀",
                            new Color32(255, 174, 0, 255), "70%") // #ffae00
                            );

                        await Task.Delay(60000);
                    }
                    catch
                    {
                        await Task.Delay(400);
                    }
                }
                else
                {
                    await Task.Delay(400);
                }
            }
        }).Start();
    }

    internal class ClanInfo
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("tag")]
        public string Tag { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("public")]
        public bool Public { get; set; }

        [JsonProperty("desc")]
        public string Desc { get; set; }

        [JsonProperty("boosts")]
        public int Boosts { get; set; }

        [JsonProperty("money")]
        public int Money { get; set; }

        [JsonProperty("balance")]
        public int Balance { get; set; }

        [JsonIgnore]
        public string FormatedDesc { get; set; } = "";
    } // end ClanInfo

}