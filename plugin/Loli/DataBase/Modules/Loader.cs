#if NR
using Loli.DataBase.Modules.Controllers;
#endif

using Loli.Patches;
using MEC;
using Newtonsoft.Json;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using Qurre.API.Controllers;

namespace Loli.DataBase.Modules
{
    static class Loader
    {
        [EventMethod(RoundEvents.Waiting)]
        static void NullCall() { }

        static Loader()
        {
            InitializeSocket();
            InitLoadClans();
            InitLoader();
            InitDonates();
        }

        static internal void LoadClans()
        {
            Core.Socket.Emit("server.database.clans", []);
        }

        static bool _initLoadClans = false;
        static void InitLoadClans()
        {
            if (_initLoadClans) return;
            Core.Socket.On("socket.database.clans", data =>
            {
                var tags = ((System.Collections.IEnumerable)data[0]).Cast<object>().Select(x => x.ToString());
                new Thread(() => { foreach (string tag in tags) try { AddClan(tag); } catch { } }).Start();
            });
            _initLoadClans = true;
        }
        static void AddClan(string tag)
        {
            List<int> Users = new();
            List<int> Boosts = new();
            {
                var url = $"{Core.APIUrl}/clan?tag={tag}&token={Core.ApiToken}&type=all_users";
                var request = WebRequest.Create(url);
                request.Method = "POST";
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();
                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();
                ClanUsersJson json = JsonConvert.DeserializeObject<ClanUsersJson>(data);
                foreach (int user in json.lvl1) Users.Add(user);
                foreach (int user in json.lvl2) Users.Add(user);
                foreach (int user in json.lvl3) Users.Add(user);
                foreach (int user in json.lvl4) Users.Add(user);
                foreach (int user in json.lvl5) Users.Add(user);
            }
            {
                var url = $"{Core.APIUrl}/clan?tag={tag}&token={Core.ApiToken}&type=boosts";
                var request = WebRequest.Create(url);
                request.Method = "POST";
                using var webResponse = request.GetResponse();
                using var webStream = webResponse.GetResponseStream();
                using var reader = new StreamReader(webStream);
                var data = reader.ReadToEnd();
                ClanBoostsJson json = JsonConvert.DeserializeObject<ClanBoostsJson>(data);
                foreach (int boost in json.boosts) Boosts.Add(boost);
            }
            Data.Clans.Add(tag, new Clan { Tag = tag, Users = Users, Boosts = Boosts });
        }

        [EventMethod(PlayerEvents.Join)]
        static void Join(JoinEvent ev)
        {
#if NR
            if (!Data.force.ContainsKey(ev.Player.UserInformation.UserId)) Data.force.Add(ev.Player.UserInformation.UserId, 0);
            if (!Data.giveway.ContainsKey(ev.Player.UserInformation.UserId)) Data.giveway.Add(ev.Player.UserInformation.UserId, 0);
            if (!Data.effect.ContainsKey(ev.Player.UserInformation.UserId)) Data.effect.Add(ev.Player.UserInformation.UserId, DateTime.Now);
            if (!Data.gives.ContainsKey(ev.Player.UserInformation.UserId)) Data.gives.Add(ev.Player.UserInformation.UserId, DateTime.Now);
            if (!Data.forces.ContainsKey(ev.Player.UserInformation.UserId)) Data.forces.Add(ev.Player.UserInformation.UserId, DateTime.Now);
            if (Data.forcer.ContainsKey(ev.Player.UserInformation.UserId)) Data.forcer[ev.Player.UserInformation.UserId] = false;
            else Data.forcer.Add(ev.Player.UserInformation.UserId, false);
            if (Data.giver.ContainsKey(ev.Player.UserInformation.UserId)) Data.giver[ev.Player.UserInformation.UserId] = false;
            else Data.giver.Add(ev.Player.UserInformation.UserId, false);
            if (Data.effecter.ContainsKey(ev.Player.UserInformation.UserId)) Data.effecter[ev.Player.UserInformation.UserId] = false;
            else Data.effecter.Add(ev.Player.UserInformation.UserId, false);
            if (!Data.scp_play.ContainsKey(ev.Player.UserInformation.UserId)) Data.scp_play.Add(ev.Player.UserInformation.UserId, false);
#endif

            try { LoadData(ev.Player); } catch (Exception ex) { Log.Error(ex); }
        }
        static void InitLoader()
        {
            Core.Socket.On("database.get.data", obj =>
            {
                string userid = obj[1].ToString();
                var pl = userid.GetPlayer();
                if (pl is null) return;
                UserData json = JsonConvert.DeserializeObject<UserData>(obj[0].ToString());
                if (Data.Users.ContainsKey(userid)) Data.Users.Remove(userid);
                json.entered = DateTime.Now;

                if (pl.UserInformation.UserId is "76561198950196047@steam" || pl.UserInformation.UserId.IsPrikols())
                    json.anonym = true;

#if MRP
				if (pl.UserInformation.UserId == "yagodnick@northwood")
				{
					try
					{
						json.gradient.prefix = "НОРТВУД СТУДИОС";
						json.gradient.fromA = "#FF007A";
						json.gradient.toA = "#0094FF";
						json.gradient.fromB = "#00FFB3";
						json.gradient.toB = "#BB00FF";

						var component = pl.ReferenceHub.GetComponent<GradientColorTag>() ?? pl.GameObject.AddComponent<GradientColorTag>();
						component._player = pl;
						component._data = json;
						component.Setup();

						json.prefix += " %gradient%";
						json.prefix = json.prefix.Trim();
					}
					catch { }
				}
#endif


                Data.Users.Add(userid, json);
                Timing.CallDelayed(0.1f, () => LoadRoles(pl, json));
                Timing.CallDelayed(0.5f, () => Levels.SetPrefix(pl));

                if (json.lvl > 2)
                {
                    FixSpoiled.Whitelist.Add(pl.ReferenceHub);
                }
            });
        }
        static internal void LoadData(Player pl)
        {
            if (Data.Roles.ContainsKey(pl.UserInformation.UserId)) Data.Roles.Remove(pl.UserInformation.UserId);
            Data.Roles.Add(pl.UserInformation.UserId, new DonateRoles());
            if (Data.Users.ContainsKey(pl.UserInformation.UserId)) Data.Users.Remove(pl.UserInformation.UserId);

            string userid = pl.UserInformation.UserId.Replace("@steam", "").Replace("@discord", "");

            if (userid == "yagodnick@northwood")
                userid = "76561198198122841";

            Core.Socket.Emit("database.get.data", [ userid,
                pl.UserInformation.UserId.Contains("discord"), 1, pl.UserInformation.UserId ]);
        }
        static void LoadRoles(Player player, UserData data)
        {
#if MRP
			Core.Socket.Emit("database.get.donate.roles", [data.id, -1, player.UserInformation.UserId + ":" + player.UserInformation.Id]);
#elif NR
            Core.Socket.Emit("database.get.donate.roles", [data.id, Core.DonateID, player.UserInformation.UserId + ":" + player.UserInformation.Id]);
#endif

            Core.Socket.Emit("database.get.donate.customize", [data.login, player.UserInformation.UserId + ":" + player.UserInformation.Id]);
            Core.Socket.Emit("database.get.nitro", [data.discord]);
            if (data.trainee)
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("sta"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "sta");
                Levels.SetPrefix(player);
            }
            if (data.helper)
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("helper"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "helper");
                Levels.SetPrefix(player);
            }
            if (data.mainhelper)
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("glhelper"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "glhelper");
                Levels.SetPrefix(player);
            }
#if MRP
			if (data.admin || data.control || data.id == 6546)
#elif NR
            if (data.admin || data.control)
#endif
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("admin"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "admin");
                Levels.SetPrefix(player);
            }
            if (data.mainadmin || data.id == 6040)
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("gladmin"), false, true);
                if (ServerStatic.PermissionsHandler.Members.ContainsKey(player.UserInformation.UserId)) ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "gladmin");
                Levels.SetPrefix(player);
            }
            if (data.maincontrol || data.id == 1)
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("owner"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "owner");
                Levels.SetPrefix(player);
            }
            if (data.it)
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("tech"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "tech");
                Levels.SetPrefix(player);
            }
            if (player.UserInformation.UserId == "76561199298395982@steam")
            {
                player.Administrative.ServerRoles.SetGroup(ServerStatic.PermissionsHandler.GetGroup("yt"), false, true);
                ServerStatic.PermissionsHandler.Members.Remove(player.UserInformation.UserId);
                ServerStatic.PermissionsHandler.Members.Add(player.UserInformation.UserId, "yt");
                Levels.SetPrefix(player);
            }
        }
        internal static readonly Dictionary<string, RainbowTagController> RainbowRoles = [];
        internal static readonly Dictionary<string, GradientColorTag> GradientRoles = [];
        internal static void InitializeSocket()
        {
            Core.Socket.On("ChangeFreezeSCPServer", data =>
            {
                BDDonateRoles doc = JsonConvert.DeserializeObject<BDDonateRoles>($"{data[0]}");

#if MRP
				if (doc.server != -1) return;
#elif NR
                if (doc.server != -1 && doc.server != Core.DonateID) return;
#endif

                foreach (var user in Data.Users.Where(x => x.Key is not null && x.Value is not null && x.Value.id == doc.owner))
                {
                    var pl = user.Key.GetPlayer();
                    if (pl is null) continue;
                    if (Data.Roles.TryGetValue(user.Key, out var role))
                    {
                        if (doc.freezed)
                        {
                            switch (doc.id)
                            {
                                case 1:
                                    {
                                        role._rainbows--;
                                        if (!role.Rainbow && RainbowRoles.TryGetValue(user.Key, out var _rainbow))
                                        {
                                            UnityEngine.Object.Destroy(_rainbow);
                                        }
                                        break;
                                    }
                                case 2:
                                    role._primes--;
                                    break;

#if NR
                                case 3:
                                    role._priests--;
                                    break;
                                case 4:
                                    role._mages--;
                                    break;
                                case 5:
                                    role._sages--;
                                    break;
                                case 6:
                                    role._stars--;
                                    break;
                                case 7:
                                    role._hand--;
                                    break;
#endif

                                case 8:
                                    {
                                        role._gradients--;
                                        if (!role.Gradients && !role.Benefactor && GradientRoles.TryGetValue(user.Key, out var gradient))
                                        {
                                            UnityEngine.Object.Destroy(gradient);
                                        }
                                        break;
                                    }
                                case 9:
                                    {
                                        role._benefactors--;
                                        if (!role.Gradients && !role.Benefactor && GradientRoles.TryGetValue(user.Key, out var gradient))
                                        {
                                            UnityEngine.Object.Destroy(gradient);
                                        }
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            switch (doc.id)
                            {
                                case 1:
                                    {
                                        if (!role.Rainbow)
                                        {
                                            var component = pl.ReferenceHub.GetComponent<RainbowTagController>()
                                            ?? pl.GameObject.AddComponent<RainbowTagController>();

                                            if (RainbowRoles.ContainsKey(user.Key))
                                            {
                                                UnityEngine.Object.Destroy(RainbowRoles[pl.UserInformation.UserId]);
                                                RainbowRoles.Remove(user.Key);
                                            }

                                            RainbowRoles.Add(pl.UserInformation.UserId, component);
                                        }
                                        role._rainbows++;
                                        break;
                                    }
                                case 2:
                                    role._primes++;
                                    break;

#if NR
                                case 3:
                                    role._priests++;
                                    break;
                                case 4:
                                    role._mages++;
                                    break;
                                case 5:
                                    role._sages++;
                                    break;
                                case 6:
                                    role._stars++;
                                    break;
                                case 7:
                                    role._hand++;
                                    break;
#endif

                                case 8:
                                    {
                                        if (!role.Gradients && !role.Benefactor)
                                        {
                                            var component = GradientColorTag.Create(pl, user.Value);

                                            if (GradientRoles.ContainsKey(pl.UserInformation.UserId))
                                            {
                                                UnityEngine.Object.Destroy(GradientRoles[pl.UserInformation.UserId]);
                                                GradientRoles.Remove(pl.UserInformation.UserId);
                                            }

                                            GradientRoles.Add(pl.UserInformation.UserId, component);
                                        }

                                        role._gradients++;

                                        break;
                                    }
                                case 9:
                                    {
                                        if (!role.Gradients && !role.Benefactor)
                                        {
                                            var component = GradientColorTag.Create(pl, user.Value);

                                            if (GradientRoles.ContainsKey(pl.UserInformation.UserId))
                                            {
                                                UnityEngine.Object.Destroy(GradientRoles[pl.UserInformation.UserId]);
                                                GradientRoles.Remove(pl.UserInformation.UserId);
                                            }

                                            GradientRoles.Add(pl.UserInformation.UserId, component);
                                        }

                                        role._benefactors++;

                                        break;
                                    }
                            }
                        }

#if NR
                        UpdateRole(pl);
#endif

                        try { Levels.SetPrefix(pl); } catch { }
                    }
                }
            });
        }

#if NR
        static internal void UpdateRole(Player pl)
        {
            UpdateRa();
            UpdateStar();
            void UpdateStar()
            {
                if (!Data.Roles.TryGetValue(pl.UserInformation.UserId, out var data)) return;
                if (!data.Star)
                {
                    var __ = Star.Get(pl);
                    if (__ is null) return;
                    __.Break();
                    return;
                }
                if (Star.Get(pl) is not null) return;
                new Star(pl);
            }

            void UpdateRa()
            {
                if (!Data.Roles.TryGetValue(pl.UserInformation.UserId, out var data)) return;
                if (pl.Administrative.RemoteAdmin && !(data.Priest || data.Mage || data.Sage || data.Star || data.Hand || data.Benefactor))
                {
                    if (pl.UserInformation.UserId.Contains("@northwood")) return;
                    if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var data2) && (data2.donater || data2.id == 1 || data2.id == 6040 ||
                            data2.trainee || data2.helper || data2.mainhelper || data2.admin || data2.mainadmin || data2.it || data2.control || data2.maincontrol)) return;
                    if (Patrol.Verified.Contains(pl.UserInformation.UserId)) return;
                    pl.Administrative.RaLogout();
                    return;
                }
                if (!pl.Administrative.RemoteAdmin && (data.Priest || data.Mage || data.Sage || data.Star || data.Hand || data.Benefactor))
                {
                    pl.Administrative.RaLogin();
                    return;
                }
            }
        }
#endif

        static internal void InitDonates()
        {
            Core.Socket.On("database.get.nitro", obj =>
            {
                string discord = $"{obj[0]}";
                if (discord.Length < 16) return;
                if (!Data.Users.TryFind(out var data, x => x.Value.discord == discord)) return;
                data.Value.isNitro = true;
            });

            Core.Socket.On("database.get.donate.roles", obj =>
            {
                string[] hash = obj[1].ToString().Split(':');
                string userid = hash[0];
                Player pl = userid.GetPlayer();
                if (pl is null || $"{pl.UserInformation.Id}" != hash[1]) return;
                int[] roles = JsonConvert.DeserializeObject<int[]>(obj[0].ToString());
                if (!Data.Roles.TryGetValue(pl.UserInformation.UserId, out var data))
                {
                    data = new DonateRoles();
                    Data.Roles.Add(pl.UserInformation.UserId, data);
                }
                foreach (int role in roles)
                {
                    switch (role)
                    {
                        case 1:
                            {
                                if (!data.Rainbow)
                                {
                                    var component = pl.ReferenceHub.GetComponent<RainbowTagController>() ?? pl.GameObject.AddComponent<RainbowTagController>();

                                    if (RainbowRoles.ContainsKey(pl.UserInformation.UserId))
                                    {
                                        UnityEngine.Object.Destroy(RainbowRoles[pl.UserInformation.UserId]);
                                        RainbowRoles.Remove(pl.UserInformation.UserId);
                                    }

                                    RainbowRoles.Add(pl.UserInformation.UserId, component);
                                }
                                data._rainbows++;
                                break;
                            }
                        case 2: data._primes++; break;

#if NR
                        case 3: data._priests++; break;
                        case 4: data._mages++; break;
                        case 5: data._sages++; break;
                        case 6: data._stars++; break;
                        case 7: data._hand++; break;
#endif

                        case 8:
                            {
                                if (!data.Gradients && !data.Benefactor)
                                {
                                    var component = GradientColorTag.Create(pl, Data.Users[pl.UserInformation.UserId]);

                                    if (GradientRoles.ContainsKey(pl.UserInformation.UserId))
                                    {
                                        UnityEngine.Object.Destroy(GradientRoles[pl.UserInformation.UserId]);
                                        GradientRoles.Remove(pl.UserInformation.UserId);
                                    }

                                    GradientRoles.Add(pl.UserInformation.UserId, component);
                                }

                                data._gradients++;
                                break;
                            }
                        case 9:
                            {
                                if (!data.Gradients && !data.Benefactor)
                                {
                                    var component = GradientColorTag.Create(pl, Data.Users[pl.UserInformation.UserId]);

                                    if (GradientRoles.ContainsKey(pl.UserInformation.UserId))
                                    {
                                        UnityEngine.Object.Destroy(GradientRoles[pl.UserInformation.UserId]);
                                        GradientRoles.Remove(pl.UserInformation.UserId);
                                    }

                                    GradientRoles.Add(pl.UserInformation.UserId, component);
                                }

                                data._benefactors++;
                                break;
                            }
                    }
                }

#if NR
                UpdateRole(pl);

#endif
                try { Levels.SetPrefix(pl); } catch { }
            });

#if NR
            Core.Socket.On("database.get.donate.ra", obj =>
            {
                string userid = obj[1].ToString();
                var pl = userid.GetPlayer();
                if (pl is null) return;
                BDDonateRA[] donates = JsonConvert.DeserializeObject<BDDonateRA[]>(obj[0].ToString());
                foreach (var donate in donates)
                {
                    if (!Data.Donates.TryGetValue(pl.UserInformation.UserId, out var data))
                    {
                        var _data = new DonateRA()
                        {
                            Force = donate.force,
                            Give = donate.give,
                            Effects = donate.effects,
                            ViewRoles = donate.players_roles
                        };
                        Data.Donates.Add(pl.UserInformation.UserId, _data);
                    }
                    else
                    {
                        if (!data.Force) data.Force = donate.force;
                        if (!data.Give) data.Give = donate.give;
                        if (!data.Effects) data.Effects = donate.effects;
                        if (!data.ViewRoles) data.ViewRoles = donate.players_roles;
                    }
                    pl.Administrative.RaLogin();
                    if (donate.force)
                    {
                        if (Data.forcer.ContainsKey(pl.UserInformation.UserId)) Data.forcer[pl.UserInformation.UserId] = true;
                        else Data.forcer.Add(pl.UserInformation.UserId, true);
                    }
                    if (donate.give)
                    {
                        if (Data.giver.ContainsKey(pl.UserInformation.UserId)) Data.giver[pl.UserInformation.UserId] = true;
                        else Data.giver.Add(pl.UserInformation.UserId, true);
                    }
                    if (donate.effects)
                    {
                        if (Data.effecter.ContainsKey(pl.UserInformation.UserId)) Data.effecter[pl.UserInformation.UserId] = true;
                        else Data.effecter.Add(pl.UserInformation.UserId, true);
                    }
                    if (!Module.Prefixs.TryGetValue(pl.UserInformation.UserId, out var _pd))
                    {
                        Module.Prefixs.Add(pl.UserInformation.UserId, new Module.RaPrefix()
                        {
                            prefix = donate.prefix,
                            color = donate.color,
                            gameplay_data = donate.players_roles
                        });
                    }
                    else
                    {
                        _pd.prefix = donate.prefix;
                        _pd.color = donate.color;
                        if (donate.players_roles) _pd.gameplay_data = donate.players_roles;
                    }
                    if (Data.Users.TryGetValue(pl.UserInformation.UserId, out var _d)) _d.donater = true;
                }
            });
#endif
        }
    }
}