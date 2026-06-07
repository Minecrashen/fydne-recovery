using InventorySystem.Items.Pickups;
using Loli.Concepts.Hackers;
using Loli.DataBase.Modules;
using Loli.HintsCore;
using Loli.Webhooks;
using MEC;
using PlayerRoles;
using PlayerRoles.Spectating;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using LabApi.Features.Wrappers;
using UnityEngine;
using Player = Qurre.API.Controllers.Player;
using Server = Qurre.API.Server;
#if NR
using Qurre.API.World;
#endif

namespace Loli
{
    static class Extensions
    {
        [EventMethod(RoundEvents.Waiting)]
        static void ClearCache()
        {
            CachedItems.Clear();
            NetIdToPlayer.Clear();
        }

        [EventMethod(RoundEvents.Start)]
        static void AddCache()
        {
            MEC.Timing.CallDelayed(6f, () =>
            {
                foreach (var pick in Pickup.List)
                    CachedItems.Add(pick.Base, true);
            });
        }

        static internal bool InPocket(this Player pl)
        {
            return pl.MovementState.Position.y.Difference(-300f) < 20;
        }

        static internal bool IsPrikols(this string userid)
        {
            return userid is "76561199062745229@steam" or "76561199017313435@steam";
        }

        static internal string[] GetInvisibleLogs()
            => ["76561199062745229@steam", "76561199017313435@steam", "76561198840787587@steam",
            "<@!552431396149395466>", "<@!637946297189400586>"];

        static readonly HttpClient HttpClient = new();
        static internal async Task<string> SendApiReq(string path, Dictionary<string, string> queryList)
        {
            string url = $"{Core.APIUrl}/{path}";
            FormUrlEncodedContent content = new(queryList);

            var response = await HttpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            return responseString;
        }

        static internal float Difference(this float first, float second)
        {
            return Math.Abs(first - second);
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int num)
        {
            T[] enumerable = [.. source];
            return enumerable.Skip(Math.Max(0, enumerable.Length - num));
        }

        static internal string ProgressBar(long progress, long total, int chunks, string symbol, string completeColor, string remainingColor)
        {
            if (progress > total)
                progress = total;

            string str = string.Empty;
            float chunk = (float)chunks / total;

            int complete = Math.Max((int)Math.Ceiling((double)chunk * progress) - 1, 0);
            int remaining = chunks - complete;

            if (0 > remaining)
                remaining = 0;
            if (0 > complete)
                complete = 0;

            if (complete > chunks)
                complete = chunks;
            if (remaining > chunks)
                remaining = chunks;

            str += $"<color={completeColor}>{Repeat(symbol, complete)}</color>";
            str += $"<color={remainingColor}>{Repeat(symbol, remaining)}</color>";

            return str;

            static string Repeat(string str, int times) => string.Concat(Enumerable.Repeat(str, times));
        }

        static internal readonly Dictionary<ItemPickupBase, bool> CachedItems = new();
        static internal bool ItsNeededItem(this ItemPickupBase serital)
        {
            if (CachedItems.TryGetValue(serital, out var _data))
                return _data;

            var _b = Builds.Models.Server.Doors.ContainsKey(serital);

            if (CachedItems.ContainsKey(serital))
                CachedItems.Remove(serital);

            CachedItems.Add(serital, _b);

            return _b;
        }

#if NR
        static internal float GetMaxHp(this Player pl)
        {
            float maxhp = pl.HealthInformation.MaxHp;
            switch (pl.RoleInformation.Role)
            {
                case RoleTypeId.ClassD:
                    maxhp = 100;
                    break;
                case RoleTypeId.Scientist:
                    maxhp = 100;
                    break;
                case RoleTypeId.ChaosConscript or RoleTypeId.ChaosMarauder or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosRifleman:
                    maxhp = 100;
                    break;
                case RoleTypeId.NtfPrivate:
                    maxhp = 100;
                    break;
                case RoleTypeId.NtfSergeant:
                    maxhp = 100;
                    break;
                case RoleTypeId.NtfCaptain:
                    maxhp = 100;
                    break;
                case RoleTypeId.NtfSpecialist:
                    maxhp = 100;
                    break;
                case RoleTypeId.FacilityGuard:
                    maxhp = 100;
                    break;
                case RoleTypeId.Tutorial:
                    maxhp = 100;
                    break;
                case RoleTypeId.Scp0492:
                    maxhp = 750;
                    break;
                case RoleTypeId.Scp106:
                    maxhp = -1;
                    break;
                case RoleTypeId.Scp049:
                    maxhp = 2500;// -1;
                    break;
                case RoleTypeId.Scp096:
                    maxhp = 3000;// -1;
                    break;
                case RoleTypeId.Scp939:
                    maxhp = -1;
                    break;
                case RoleTypeId.Scp173:
                    maxhp = -1;
                    break;
            }

            float cf = 1;

            if (pl.IsPrime())
                cf += 0.1f;

            return maxhp * cf;
        }
#endif

        static internal IEnumerator<float> WarnBc(this MapBroadcast bc, string str)
        {
            bool value = false;

            for (int i = 0; i < 16; i++)
            {
                yield return Timing.WaitForSeconds(1f);

                value = !value;

                bc.Message = str.Replace("rainbow", value ? "#fdffbb" : "#ff0000");
            }
        }

        static internal void GetAmmo(this Player pl)
        {
#if MRP
			if (pl.RoleInformation.Role is RoleTypeId.FacilityGuard)
			{
				pl.Inventory.Ammo.Ammo9 = 70;
			}
			else if (pl.RoleInformation.Role is RoleTypeId.NtfPrivate or RoleTypeId.Tutorial)
			{
				pl.Inventory.Ammo.Ammo9 = 170;
			}
			else if (pl.RoleInformation.Team is Team.FoundationForces)
			{
				pl.Inventory.Ammo.Ammo556 = 120;
				pl.Inventory.Ammo.Ammo9 = 60;
			}
			else if (pl.RoleInformation.Role is RoleTypeId.ChaosRepressor)
			{
				pl.Inventory.Ammo.Ammo12Gauge = 54;
				pl.Inventory.Ammo.Ammo44Cal = 48;
			}
			else if (pl.RoleInformation.Team is Team.ChaosInsurgency)
			{
				pl.Inventory.Ammo.Ammo762 = 120;
				pl.Inventory.Ammo.Ammo9 = 60;
			}
			else if (pl.RoleInformation.Team is Team.Scientists)
			{
				pl.Inventory.Ammo.Ammo9 = 30;
			}
#elif NR
            pl.Inventory.Base.UserInventory.ReserveAmmo[ItemType.Ammo12gauge] = 999;
            pl.Inventory.Base.UserInventory.ReserveAmmo[ItemType.Ammo44cal] = 999;
            pl.Inventory.Base.UserInventory.ReserveAmmo[ItemType.Ammo556x45] = 999;
            pl.Inventory.Base.UserInventory.ReserveAmmo[ItemType.Ammo762x39] = 999;
            pl.Inventory.Base.UserInventory.ReserveAmmo[ItemType.Ammo9x19] = 999;
            pl.Inventory.Base.SendAmmoNextFrame = true;
#endif
        }

#if NR
        static internal ItemType GetRandomGun()
        {
            int rand = UnityEngine.Random.Range(0, 10);

            return rand switch
            {
                1 => ItemType.GunA7,
                2 => ItemType.GunAK,
                3 => ItemType.ParticleDisruptor,
                4 => ItemType.GunCOM18,
                5 => ItemType.GunCrossvec,
                6 => ItemType.GunE11SR,
                7 => ItemType.GunFRMG0,
                8 => ItemType.GunLogicer,
                9 => ItemType.GunShotgun,
                _ => ItemType.GunCOM15,
            };
        }
#endif


        static internal void SetRank(this Player player, string rank, string color = "default")
        {
            player.Administrative.RoleName = rank;
            player.Administrative.RoleColor = color;
        }

        static internal bool ItsHacker(this Player pl)
            => pl is not null && pl.Tag.Contains(Hacker.Tag);

        static internal bool ItsSpyFacilityManager(this Player pl)
#if MRP
			=> pl is not null && pl.Tag.Contains(Addons.RolePlay.FacilityManager.TagSpy);
#elif NR
            => false;
#endif

        static internal bool ItsAdmin(this Player pl, bool owner = true)
        {
            if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var data) &&
                (data.trainee || data.helper || data.mainhelper ||
                data.admin || data.mainadmin || data.control ||
                data.maincontrol || (owner && data.id == 1)))
                return true;

            return false;
        }

        static internal bool IsPrime(this Player pl)
        {
            if (Data.Roles.TryGetValue(pl.UserInformation.UserId, out var data))
                return data.Prime;

            return false;
        }

        static internal string GetClan(this Player pl)
        {
            if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var data))
                return data.clan;

            return string.Empty;
        }

#nullable enable
        static readonly Dictionary<uint, Player> NetIdToPlayer = new();
        static internal Player? GetSpectatingPlayer(this Player watcher)
        {
            if (watcher.RoleInformation.Role is not RoleTypeId.Overwatch)
                return null;

            if (watcher.RoleInformation.Base is not SpectatorRole spectator)
                return null;

            uint netid = spectator.SyncedSpectatedNetId;

            if (NetIdToPlayer.TryGetValue(netid, out Player player))
                return player;

            if (!Player.List.TryFind(out Player spectating, x => x.UserInformation.NetId == netid))
                return null;

            NetIdToPlayer[netid] = spectating;


            return spectating;
        }
#nullable restore

        static internal Team GetTeam(this Player pl)
            => GetTeam(pl.RoleInformation.Role);
        static internal Team GetTeam(this RoleTypeId roleType)
            => PlayerRolesUtils.GetTeam(roleType);

        static internal (string, string) GetInfoRole(this RoleTypeId roleType)
        {
            string Role;
            string RoleColor;

            switch (roleType)
            {
                case RoleTypeId.ClassD:
                    Role = "Класс-D";
                    RoleColor = "#ff8000";
                    break;
                case RoleTypeId.Scientist:
                    Role = "Ученый";
                    RoleColor = "#ffff7c";
                    break;

                case RoleTypeId.Tutorial:
                    Role = "Обучение";
                    RoleColor = "#ff00b0";
                    break;

                case RoleTypeId.ChaosConscript:
                    Role = "Новобранец Повстанцев";
                    RoleColor = "#559100";
                    break;
                case RoleTypeId.ChaosMarauder:
                    Role = "Мародер Повстанцев";
                    RoleColor = "#006326";
                    break;
                case RoleTypeId.ChaosRepressor:
                    Role = "Усмиритель Повстанцев";
                    RoleColor = "#16853d";
                    break;
                case RoleTypeId.ChaosRifleman:
                    Role = "Стрелок Повстанцев";
                    RoleColor = "#008f1c";
                    break;

                case RoleTypeId.FacilityGuard:
                    Role = "Охрана Комплекса";
                    RoleColor = "#556278";
                    break;
                case RoleTypeId.NtfPrivate:
                    Role = "Кадет МОГ";
                    RoleColor = "#70c3ff";
                    break;
                case RoleTypeId.NtfSergeant:
                    Role = "Сержант МОГ";
                    RoleColor = "#42a4ff";
                    break;
                case RoleTypeId.NtfCaptain:
                    Role = "Капитан МОГ";
                    RoleColor = "#003dca";
                    break;
                case RoleTypeId.NtfSpecialist:
                    Role = "Специалист МОГ";
                    RoleColor = "#42a4ff";
                    break;

                case RoleTypeId.Overwatch or RoleTypeId.Spectator:
                    Role = "Наблюдатель";
                    RoleColor = "white";
                    break;

                default:
                    {
                        Role = $"{roleType}";
                        if (roleType.GetTeam() == Team.SCPs)
                            RoleColor = "#ff0000";
                        else
                            RoleColor = "white";
                        break;
                    }
            }

            return (Role, RoleColor);
        }

        public static Color ColorFromHex(this string hex)
            => new Color32(
                Convert.ToByte(hex.Substring(1, 2), 16),
                Convert.ToByte(hex.Substring(3, 2), 16),
                Convert.ToByte(hex.Substring(5, 2), 16),
                255);

        public static string OptimizeNick(this string nick, int limit = 10)
        {
            nick = Constants.ReplaceAllRegex.Replace(nick, "").Replace("-", "");

            if (nick.Length <= limit)
                return nick;

            return nick.Substring(0, limit) + "..";
        }

        static internal void RestartCrush(string reason)
        {
            Server.Restart();

            new Dishook(Core.WebHooks.Crush)
                .Send("", Core.ServerName, embeds: new List<Embed>()
                {
                    new()
                    {
                        Color = 16711680,
                        Author = new() { Name = "Краш сервера" },
                        Footer = new() { Text = Server.Ip + ":" + Server.Port },
                        TimeStamp = DateTimeOffset.Now,
                        Description = reason
                    }
                });
        }

        static internal void DownloadAudio(this string url, string path)
        {
            var dir = Path.Combine(path, "..");

            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            if (File.Exists(path))
                return;

            new Thread(() =>
            {
                WebRequest request = WebRequest.Create(url);
                WebResponse response = request.GetResponse();
                Stream responseStream = response.GetResponseStream();
                Stream fileStream = File.OpenWrite(path);
                byte[] buffer = new byte[4096];
                int bytesRead = responseStream.Read(buffer, 0, 4096);
                while (bytesRead > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    bytesRead = responseStream.Read(buffer, 0, 4096);
                }
            }).Start();
        }
    }
}