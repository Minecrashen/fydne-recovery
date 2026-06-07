using Loli.DataBase.Modules;
using Loli.Webhooks;
using Newtonsoft.Json.Linq;
using PlayerRoles.FirstPersonControl;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Addons
{
    static class AntiCheat
    {
        static bool LczArmoryDoorOpenned { get; set; } = false;

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            LczArmoryDoorOpenned = false;
        }

        [EventMethod(PlayerEvents.InteractDoor, int.MaxValue)]
        static void OpenDoor(InteractDoorEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Door.Type is not DoorType.LczArmory)
                return;

            LczArmoryDoorOpenned = true;
        }

        [EventMethod(MapEvents.OpenDoor, int.MaxValue)]
        static void OpenDoor(OpenDoorEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Door.Type is not DoorType.LczArmory)
                return;

            LczArmoryDoorOpenned = true;
        }


        [EventMethod(ScpEvents.Scp106Attack, int.MinValue)]
        static void AntiScp106Exploit(Scp106AttackEvent ev)
        {
            float distance = Vector3.Distance(ev.Attacker.MovementState.Position, ev.Target.MovementState.Position);
            if (distance < 3.5f)
                return;

            RoomType targetRoom = ev.Target.GamePlay.Room.Type;

            if (targetRoom is RoomType.Pocket)
                return;

            ev.Allowed = false;

            if (distance > 30f)
            {
                string rt = $"{Round.ElapsedTime.Minutes:00}:{Round.ElapsedTime.Seconds:00}.{Round.ElapsedTime.Milliseconds:000}";
                ev.Attacker.ReportPlayer("Абуз эксплоита за SCP-106\n" +
                    $"Жертва: {ev.Target.UserInformation.Nickname} ({ev.Target.RoleInformation.Role}) {{{targetRoom}}}\n" +
                    $"SCP-106: {{{ev.Attacker.GamePlay.Room.Type}}}; Дальность: {distance} метров;\nДлительность раунда: {rt}\n" +
                    $"// Если только один раз, то вероятно ложное срабатывание", false);
            }

            try { ev.Attacker.RoleInformation.Scp106.Attack.SendCooldown(2f); } catch { }
        }

        [EventMethod(PlayerEvents.PickupItem, int.MinValue)]
        static void PickupExploit(PickupItemEvent ev)
        { // 4 metres
            if (!ev.Allowed)
                return;

            var room = ev.Player.GamePlay.Room;
            if (room.Type is not RoomType.LczArmory and not RoomType.Hcz079)
                return;

            Vector3 cameraPos = ev.Player.CameraTransform.position;
            Vector3 pickupPos = ev.Pickup.Position;

            if (!Physics.Linecast(cameraPos, pickupPos, FpcStateProcessor.Mask))
                return;

            if (!Physics.Linecast(pickupPos, cameraPos, FpcStateProcessor.Mask))
                return;

            if (!Physics.Linecast(cameraPos, pickupPos + (Vector3.up * 0.1f), FpcStateProcessor.Mask))
                return;

            ev.Allowed = false;

            float distance = Vector3.Distance(cameraPos, pickupPos);
            bool fullDetect = false;

            if (room.Type is RoomType.Hcz079)
            {
                GameObject go = new();
                go.transform.parent = room.Transform;

                go.transform.position = ev.Player.MovementState.Position;
                bool b1 = go.transform.localPosition.z > -3;

                go.transform.position = ev.Pickup.Position;
                bool b2 = go.transform.localPosition.z < -3;

                Object.Destroy(go);

                if (room.Doors.Any(x => x.Lock) && distance > 2 && b1 && b2)
                    fullDetect = true;
            }
            else if (room.Type is RoomType.LczArmory)
            {
                GameObject go = new();
                go.transform.parent = room.Transform;

                go.transform.position = ev.Player.MovementState.Position;
                bool b1 = go.transform.localPosition.x < -1.4f;

                go.transform.position = ev.Pickup.Position;
                bool b2 = go.transform.localPosition.x > -1.3f;

                Object.Destroy(go);

                if (!LczArmoryDoorOpenned && !room.Doors.Any(x => !x.Destroyed) && b1 && b2)
                    fullDetect = true;
            }

            if (!fullDetect)
                return;

            string rt = $"{Round.ElapsedTime.Minutes:00}:{Round.ElapsedTime.Seconds:00}.{Round.ElapsedTime.Milliseconds:000}";
            ev.Player.ReportPlayer($"Абуз эксплоита с подбором предметов " +
                $"({ev.Player.RoleInformation.Role}) [{ev.Pickup.Info.ItemId}] {{{ev.Player.GamePlay.Room.Type}}}\n" +
                $"Дальность: {distance} метров\nДлительность раунда: {rt}");
        }


        static readonly List<string> ReportedPlayers = [];

        static internal void ReportPlayer(this Player pl, string reason, bool check = true)
        {
            if (check && ReportedPlayers.Contains(pl.UserInformation.UserId))
                return;

            ReportedPlayers.Add(pl.UserInformation.UserId);

            new Thread(() =>
            {
                SteamMainInfoApi json = new() { Name = pl.UserInformation.Nickname };
                int lvl = -1;
                int serverLvl = -1;

                if (!pl.UserInformation.UserId.Contains("@discord"))
                {
                    {
                        var url = "https://" + $"api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={Core.SteamToken}&format=json&steamids=" +
                            pl.UserInformation.UserId.Replace("@steam", "");
                        var request = WebRequest.Create(url);
                        request.Method = "GET";
                        using var webResponse = request.GetResponse();
                        using var webStream = webResponse.GetResponseStream();
                        using var reader = new StreamReader(webStream);
                        var data = reader.ReadToEnd();
                        var privacy = JObject.Parse(data);
                        var pls = privacy["response"]["players"];
                        json = pls.ToObject<SteamMainInfoApi[]>()[0];
                    }
                    {
                        var url = "https://" + $"api.steampowered.com/IPlayerService/GetSteamLevel/v1/?key={Core.SteamToken}&steamid=" +
                            pl.UserInformation.UserId.Replace("@steam", "");
                        var request = WebRequest.Create(url);
                        request.Method = "GET";
                        using var webResponse = request.GetResponse();
                        using var webStream = webResponse.GetResponseStream();
                        using var reader = new StreamReader(webStream);
                        var data = reader.ReadToEnd();
                        if (!data.Contains("player_level")) lvl = 0;
                        else
                        {
                            var privacy = JObject.Parse(data);
                            lvl = privacy["response"]["player_level"].ToObject<int>();
                        }
                    }
                }

                if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var imain))
                    serverLvl = imain.lvl;

                Dishook webhk = new("https://discord.com/api/webhooks/1174263432720171018/h9g7a91dFR8onu63dFViAkxr-zmNo6I-mMaiSaL5waN5Ykr5JiFeDU6V5m9xoks49zLk");
                List<Embed> listEmbed = new();
                Embed embed = new()
                {
                    Color = 16729088,
                    Author = new()
                    {
                        Name = $"{json.Name} | {lvl} уровень в стиме | " +
                        $"{serverLvl} уровень на сервере | {pl.UserInformation.UserId}",
                        IconUrl = json.Avatar
                    },
                    Footer = new() { Text = Server.Ip + ":" + Core.Port },
                    TimeStamp = System.DateTimeOffset.Now,
                    Description = reason.Trim(),
                };
                listEmbed.Add(embed);
                webhk.Send("Cheat Detect", Core.ServerName, null, false, embeds: listEmbed);
            }).Start();
        }
    }
}