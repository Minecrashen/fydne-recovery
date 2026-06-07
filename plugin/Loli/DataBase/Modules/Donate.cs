using Loli.Addons;

#if NR
using Loli.Concepts.Hackers;
using Loli.Concepts.Scp008;
using Loli.Spawns;
using PlayerRoles;
using Qurre.API;
#endif

using Qurre.API.Attributes;

#if NR
using Qurre.API.Controllers;
#endif

using Qurre.Events;
using Qurre.Events.Structs;

#if NR
using System;
using System.Linq;
using Qurre.API.World;
#endif

namespace Loli.DataBase.Modules
{
    static class Donate
    {
#if NR
        internal static int DonateLimint => 5;

        static internal int Calls = 0;
        static internal DateTime LastCall = DateTime.Now;
        static internal Faction LastFaction = Faction.Unclassified;

#endif

#if NR
        [EventMethod(RoundEvents.Waiting)]
        static void NullCall()
        {
            Calls = 0;
            LastCall = DateTime.Now;
            LastFaction = Faction.Unclassified;
        }
#endif

        static Donate()
        {
            CommandsSystem.RegisterRemoteAdmin("hidetag", AntiHideTag);
        }
        static void AntiHideTag(RemoteAdminCommandEvent ev)
        {
            ev.Prefix = "HIDETAG";
            ev.Allowed = false;
            ev.Success = false;
            ev.Reply = "Недоступно";
        }

#if NR
        [EventMethod(ServerEvents.RemoteAdminCommand, 2)]
        static void Ra(RemoteAdminCommandEvent ev)
        {
            if (!ev.Allowed)
                return;

            switch (ev.Sender.SenderId)
            {
                case "SERVER CONSOLE": return;
                case "GAME CONSOLE": return;
                case "Effects Controller": return;
            }

            switch (ev.Name)
            {
                case "give":
                    {
                        if (ev.Sender.SenderId == "76561198840787587@steam")
                            return;

                        ev.Prefix = "GIVE";
                        Data.Roles.TryGetValue(ev.Sender.SenderId, out var roles);
                        if (!(roles.Mage || roles.Sage || roles.Star || roles.Hand || roles.Benefactor ||
                              (Data.giver.TryGetValue(ev.Sender.SenderId, out var ___) && ___)))
                            return;
                        ev.Allowed = false;
                        if (Commands.EventMode)
                        {
                            ev.Reply = "Включен режим ивента. Админ-панель недоступна.";
                            ev.Success = false;
                            return;
                        }

                        if (Round.Waiting)
                        {
                            ev.Reply = "Раунд еще не начался";
                            ev.Success = false;
                            return;
                        }

                        if (ev.Player.RoleInformation.Team == Team.SCPs)
                        {
                            ev.Reply = "Вы играете за SCP. За SCP нельзя выдавать предметы.";
                            ev.Success = false;
                            return;
                        }

                        if (ev.Player.Inventory.ItemsCount == 8)
                        {
                            ev.Reply = "У вас заполнен инвентарь";
                            ev.Success = false;
                            return;
                        }

                        if (!Data.giveway.ContainsKey(ev.Player.UserInformation.UserId))
                            Data.giveway.Add(ev.Player.UserInformation.UserId, 0);
                        Data.giveway.TryGetValue(ev.Player.UserInformation.UserId, out var giver);
                        if (giver >= DonateLimint)
                        {
                            ev.Reply = $"Вы уже выдали {DonateLimint} предметов";
                            ev.Success = false;
                            return;
                        }

                        var itemN = -1;
                        if (ev.Args.Length > 1)
                        {
                            try
                            {
                                itemN = Convert.ToInt32(ev.Args[1].Split('.')[0]);
                            }
                            catch
                            {
                            }
                        }
                        else
                        {
                            try
                            {
                                itemN = Convert.ToInt32(ev.Args[0].Split('.')[0]);
                            }
                            catch
                            {
                            }
                        }

                        if ((itemN < 0 || itemN > 43) && itemN != 54)
                        {
                            ev.Reply = "ID предмета не найден";
                            ev.Success = false;
                            return;
                        }

                        ItemType item = (ItemType)itemN;
                        if (item == ItemType.Ammo12gauge || item == ItemType.Ammo44cal || item == ItemType.Ammo556x45 ||
                            item == ItemType.Ammo762x39 || item == ItemType.Ammo9x19)
                        {
                            ev.Player.GetAmmo();
                            Data.giveway[ev.Player.UserInformation.UserId]++;
                            ev.Reply = "Успешно";
                            return;
                        }

                        if (item == ItemType.MicroHID)
                        {
                            ev.Reply = "MicroHid только 1";
                            ev.Success = false;
                            return;
                        }

                        if (item == ItemType.AntiSCP207)
                        {
                            ev.Reply = "Предмет в блеклисте";
                            ev.Success = false;
                            return;
                        }
                        else if ((ev.Player.RoleInformation.Role == RoleTypeId.ClassD ||
                                  ev.Player.RoleInformation.Role == RoleTypeId.Scientist) &&
                                 (item == ItemType.KeycardGuard || item == ItemType.KeycardMTFPrivate ||
                                  item == ItemType.GunCOM15 || item == ItemType.GunCOM18
                                  || item == ItemType.KeycardChaosInsurgency ||
                                  item == ItemType.KeycardContainmentEngineer || item == ItemType.KeycardFacilityManager
                                  || item == ItemType.KeycardMTFCaptain || item == ItemType.KeycardMTFOperative ||
                                  item == ItemType.KeycardO5 ||
                                  item == ItemType.SCP018 || item == ItemType.GrenadeHE || item == ItemType.GrenadeFlash) &&
                                 (3 >= Round.ElapsedTime.TotalMinutes))
                        {
                            ev.Reply = "3 минуты не прошло";
                            ev.Success = false;
                            return;
                        }
                        else if ((ev.Player.RoleInformation.Role == RoleTypeId.ClassD ||
                                  ev.Player.RoleInformation.Role == RoleTypeId.Scientist) &&
                                 (item == ItemType.GunCrossvec || item == ItemType.GunFSP9 ||
                                  item == ItemType.GunRevolver) &&
                                 (4 >= Round.ElapsedTime.TotalMinutes))
                        {
                            ev.Reply = "4 минуты не прошло";
                            ev.Success = false;
                            return;
                        }
                        else if ((ev.Player.RoleInformation.Role == RoleTypeId.ClassD ||
                                  ev.Player.RoleInformation.Role == RoleTypeId.Scientist) && (item == ItemType.GunAK ||
                                     item == ItemType.GunLogicer || item == ItemType.GunShotgun ||
                                     item == ItemType.GunE11SR) &&
                                 (5 >= Round.ElapsedTime.TotalMinutes))
                        {
                            ev.Reply = "5 минут не прошло";
                            ev.Success = false;
                            return;
                        }

                        double CoolDown = 2;
                        if (!Data.gives.ContainsKey(ev.Player.UserInformation.UserId))
                            Data.gives.Add(ev.Player.UserInformation.UserId, DateTime.Now);
                        else if ((DateTime.Now - Data.gives[ev.Sender.SenderId]).TotalSeconds < 0)
                        {
                            var wait = Math.Round((Data.gives[ev.Sender.SenderId] - DateTime.Now).TotalSeconds);
                            ev.Reply =
                                $"Предметы можно выдавать раз в {CoolDown} минуты\nОсталось подождать {wait} секунд(ы)";
                            ev.Success = false;
                            return;
                        }

                        ev.Player.Inventory.AddItem(item);
                        ev.Reply = "Успешно";
                        Data.gives[ev.Sender.SenderId] = DateTime.Now.AddMinutes(CoolDown);
                        Data.giveway[ev.Player.UserInformation.UserId]++;
                        return;
                    }
                case "forceclass":
                    {
                        ev.Prefix = "FORCECLASS";
                        Data.Roles.TryGetValue(ev.Sender.SenderId, out var roles);
                        if (!(roles.Priest || roles.Mage || roles.Sage || roles.Star || roles.Hand || roles.Benefactor ||
                              (Data.forcer.TryGetValue(ev.Sender.SenderId, out var ___) && ___))) return;
                        ev.Allowed = false;
                        if (Commands.EventMode)
                        {
                            ev.Reply = "Включен режим ивента. Админ-панель недоступна.";
                            ev.Success = false;
                            return;
                        }

                        if (Round.Waiting)
                        {
                            ev.Reply = "Раунд еще не начался";
                            ev.Success = false;
                            return;
                        }

                        if (!Data.force.ContainsKey(ev.Player.UserInformation.UserId))
                            Data.force.Add(ev.Player.UserInformation.UserId, 0);
                        Data.force.TryGetValue(ev.Player.UserInformation.UserId, out var forcer);
                        if (forcer >= DonateLimint)
                        {
                            ev.Reply = $"Вы уже меняли роль {DonateLimint} раз";
                            ev.Success = false;
                            return;
                        }

                        RoleTypeId role = RoleTypeId.None;
                        try
                        {
                            role = (RoleTypeId)Enum.Parse(typeof(RoleTypeId), ev.Args[1]);
                        }
                        catch
                        {
                        }

                        if (role == RoleTypeId.None)
                        {
                            ev.Reply = $"Произошла ошибка при получении роли. Проверьте правильность команды";
                            ev.Success = false;
                            return;
                        }

                        var team = role.GetTeam();
                        if (role is RoleTypeId.Filmmaker)
                        {
                            ev.Success = false;
                            ev.Reply = "Недоступен";
                            return;
                        }

                        if (Round.ElapsedTime.TotalMinutes < 2 && role == RoleTypeId.Scp0492)
                        {
                            ev.Reply = $"За SCP-049-2 нельзя заспавниться в первые 2 минуты игры";
                            ev.Success = false;
                            return;
                        }

                        if (Round.ElapsedTime.TotalMinutes < 2 &&
                            (team == Team.ChaosInsurgency ||
                             (team == Team.FoundationForces && role != RoleTypeId.FacilityGuard) ||
                             role == RoleTypeId.Tutorial))
                        {
                            ev.Reply = $"Подождите {(120 - Round.ElapsedTime.TotalSeconds)} секунд.";
                            ev.Success = false;
                            return;
                        }

                        if (Alpha.Detonated)
                        {
                            ev.Reply = "Нельзя заспавниться после взрыва боеголовки";
                            ev.Success = false;
                            return;
                        }

                        int scps = Player.List.Where(x => x.RoleInformation.Team == Team.SCPs).Count();
                        if (team == Team.SCPs && scps > 2 && Player.List.Count() / scps < 6)
                        {
                            ev.Success = false;
                            ev.Reply = "SCP слишком много";
                            return;
                        }

                        if (team == Team.SCPs)
                        {
                            if (!Data.scp_play.ContainsKey(ev.Player.UserInformation.UserId))
                                Data.scp_play.Add(ev.Player.UserInformation.UserId, false);
                            if (Data.scp_play[ev.Player.UserInformation.UserId])
                            {
                                ev.Success = false;
                                ev.Reply = "Вы уже играли за SCP";
                                return;
                            }

                            if (Force.SpawnedSCPs.Contains(role))
                            {
                                ev.Success = false;
                                ev.Reply = "Данный SCP уже был в раунде";
                                return;
                            }

                            if (Player.List.Any(x => x.RoleInformation.Role == role))
                            {
                                ev.Success = false;
                                ev.Reply = "Данный SCP уже играет";
                                return;
                            }
                        }

                        int Scp173 = 0;
                        int Scp106 = 0;
                        int Scp049 = 0;
                        int Scp079 = 0;
                        int Scp096 = 0;
                        int Scp939 = 0;
                        int Scp3114 = 0;
                        foreach (var pl in Player.List)
                        {
                            switch (pl.RoleInformation.Role)
                            {
                                case RoleTypeId.Scp173: Scp173++; break;
                                case RoleTypeId.Scp106: Scp106++; break;
                                case RoleTypeId.Scp049: Scp049++; break;
                                case RoleTypeId.Scp079: Scp079++; break;
                                case RoleTypeId.Scp096: Scp096++; break;
                                case RoleTypeId.Scp939: Scp939++; break;
                                case RoleTypeId.Scp3114: Scp3114++; break;
                            }
                        }

                        if ((role == RoleTypeId.Scp939 && Scp939 > 0) || (role == RoleTypeId.Scp096 && Scp096 > 0) ||
                            (role == RoleTypeId.Scp079 && Scp079 > 0) || (role == RoleTypeId.Scp049 && Scp049 > 0) ||
                            (role == RoleTypeId.Scp106 && Scp106 > 0) || (role == RoleTypeId.Scp173 && Scp173 > 0))
                        {
                            ev.Success = false;
                            ev.Reply = "Этот SCP уже есть";
                            return;
                        }

                        if (role == RoleTypeId.Scp3114 && Scp3114 > 1)
                        {
                            ev.Success = false;
                            ev.Reply = "SCP-3114 в раунде более одного";
                            return;
                        }

                        double CoolDown = 2;
                        if (!Data.forces.ContainsKey(ev.Player.UserInformation.UserId))
                            Data.forces.Add(ev.Player.UserInformation.UserId, DateTime.Now);
                        else if ((DateTime.Now - Data.forces[ev.Sender.SenderId]).TotalSeconds < 0)
                        {
                            var wait = Math.Round((Data.forces[ev.Sender.SenderId] - DateTime.Now).TotalSeconds);
                            ev.Reply = $"Спавниться можно раз в {CoolDown} минуты\nОсталось подождать {wait} секунд(ы)";
                            ev.Success = false;
                            return;
                        }

                        ev.Reply = $"Успешно\nЛимит: {forcer + 1}/{DonateLimint}";
                        if (team == Team.ChaosInsurgency ||
                            (role != RoleTypeId.FacilityGuard && team == Team.FoundationForces))
                        {
                            ev.Player.Tag += "DonateSpawnPoint";
                            MEC.Timing.CallDelayed(1f, () => ev.Player.Tag.Replace("DonateSpawnPoint", ""));
                            //SpawnManager.SpawnProtect(ev.Player);
                        }

                        if (role == RoleTypeId.Tutorial)
                        {
                            if (roles.Hand || roles.Benefactor)
                            {
                                ev.Player.Tag += "DonateSpawnPoint";
                                MEC.Timing.CallDelayed(1f, () => ev.Player.Tag.Replace("DonateSpawnPoint", ""));
                                SerpentsHand.SpawnOne(ev.Player);
                                Data.forces[ev.Sender.SenderId] = DateTime.Now.AddMinutes(CoolDown);
                                Data.force[ev.Player.UserInformation.UserId]++;
                                return;
                            }

                            ev.Success = false;
                            ev.Reply = "Отключен";
                            return;
                        }

                        if (role == RoleTypeId.ChaosConscript && (roles.Hand || roles.Benefactor))
                        {
                            Hacker.Spawn(ev.Player);
                            Data.forces[ev.Sender.SenderId] = DateTime.Now.AddMinutes(CoolDown);
                            Data.force[ev.Player.UserInformation.UserId]++;
                            return;
                        }

                        ev.Player.RoleInformation.SetNew(role, RoleChangeReason.Respawn);
                        Data.forces[ev.Sender.SenderId] = DateTime.Now.AddMinutes(CoolDown);
                        Data.force[ev.Player.UserInformation.UserId]++;
                        return;
                    }
                case "pfx":
                    {
                        ev.Prefix = "EFFECT";
                        Data.Roles.TryGetValue(ev.Sender.SenderId, out var roles);
                        if (!(roles.Sage || roles.Star || roles.Benefactor ||
                              (Data.effecter.TryGetValue(ev.Sender.SenderId, out var ___) && ___)))
                            return;
                        ev.Allowed = false;
                        if (Commands.EventMode)
                        {
                            ev.Reply = "Включен режим ивента. Админ-панель недоступна.";
                            ev.Success = false;
                            return;
                        }

                        double CoolDown = 3;
                        if (!Data.effect.ContainsKey(ev.Player.UserInformation.UserId))
                            Data.effect.Add(ev.Player.UserInformation.UserId, DateTime.Now);
                        else if ((DateTime.Now - Data.effect[ev.Sender.SenderId]).TotalSeconds < 0)
                        {
                            var wait = Math.Round((Data.effect[ev.Sender.SenderId] - DateTime.Now).TotalSeconds);
                            ev.Reply =
                                $"Эффекты можно использовать раз в {CoolDown} минуты\nОсталось подождать {wait} секунд(ы)";
                            ev.Success = false;
                            return;
                        }

                        string lowerName = ev.Args[0].ToLower();

                        if (ev.Player.RoleInformation.Team == Team.SCPs)
                        {
                            ev.Reply = "Эффекты за SCP недоступны.";
                            ev.Success = false;
                            return;
                        }

                        /*
                        if(!(lowerName == "amnesiaitems" || lowerName == "amnesiavision" || lowerName == "asphyxiated" || lowerName == "bleeding" ||
                            lowerName == "bleeding" || lowerName == "bleeding" || lowerName == "bleeding"))
                        */


                        if (lowerName == "bodyshotreduction" || lowerName == "damagereduction"
                                                             || lowerName == "spawnprotected" || lowerName == "scp1853" ||
                                                             lowerName == "vitality" || lowerName == "sugarrush"
                                                             || lowerName == "ghostly" || lowerName == "spicy" ||
                                                             lowerName == "marshmalloweffect" || lowerName == "metal"
                                                             || lowerName == "orangecandy")
                        {
                            ev.Reply = "Данный эффект слишком сильно влияет на баланс";
                            ev.Success = false;
                            return;
                        }

                        if (lowerName == "movementboost")
                        {
                            if (ev.Player.RoleInformation.Team == Team.SCPs)
                            {
                                ev.Reply = "За SCP нельзя увеличить себе скорость таким образом";
                                ev.Success = false;
                                return;
                            }

                            byte timeMB = 0;
                            try
                            {
                                timeMB = byte.Parse(ev.Args[2]);
                            }
                            catch
                            {
                                ev.Success = false;
                                ev.Reply = "Произошла ошибка при парсинге времени 0-255";
                                return;
                            }

                            timeMB = Math.Min(Math.Max((byte)1, timeMB), (byte)15);


                            byte cfMB = 0;
                            try
                            {
                                cfMB = byte.Parse(ev.Args[1]);
                            }
                            catch
                            {
                                ev.Success = false;
                                ev.Reply = "Произошла ошибка при парсинге скорости 0-255";
                                return;
                            }

                            if (ev.Player.Effects.Controller.TryGetEffect(ev.Args[0], out var _statusMB))
                            {
                                ev.Player.Effects.Enable(_statusMB, timeMB, false);
                                _statusMB.Intensity = cfMB;
                                ev.Player.HealthInformation.Damage(cfMB * 3,
                                    "На сколько ускоришься ты - такую обратную реакцию и получишь");
                            }

                            return;
                        }

                        if (lowerName == "invisible")
                        {
                            ev.Reply = "Данный эффект недоступен";
                            ev.Success = false;
                            return;
                        }

                        if (lowerName == "invisible" && ev.Player.RoleInformation.Team == Team.SCPs)
                        {
                            ev.Reply = "За SCP нельзя выдавать эффект невидимости";
                            ev.Success = false;
                            return;
                        }

                        if (lowerName == "scp207" && ev.Args[1] != "0")
                        {
                            if (ev.Player.RoleInformation.Role == RoleTypeId.Scp939)
                            {
                                ev.Reply = "За SCP-939 нельзя выдать эффект колы";
                                ev.Success = false;
                                return;
                            }

                            if (ev.Player.RoleInformation.Team == Team.SCPs)
                            {
                                ev.Args[1] = "1";
                            }
                            else
                            {
                                ev.Args[1] = "2";
                            }
                        }
                        else if (lowerName == "invisible" && ev.Args[1] != "0") ev.Args[2] = "30";

                        int time = 0;
                        try
                        {
                            time = int.Parse(ev.Args[2]);
                        }
                        catch
                        {
                            ev.Success = false;
                            ev.Reply = "Произошла ошибка при парсинге времени";
                            return;
                        }

                        byte intensivity = 1;
                        try
                        {
                            intensivity = byte.Parse(ev.Args[1]);
                        }
                        catch
                        {
                        }

                        if (intensivity > 3) intensivity = 3;
                        if (time > 60) time = 60;


                        if (ev.Player.Effects.Controller.TryGetEffect(ev.Args[0], out var _status))
                        {
                            ev.Player.Effects.Enable(_status, time, false);
                            _status.Intensity = intensivity;
                        }

                        Data.effect[ev.Sender.SenderId] = DateTime.Now.AddMinutes(CoolDown);
                        ev.Reply = "Успешно";
                        return;
                    }
                case "server_event":
                    {
                        ev.Prefix = "SERVER_EVENT";
                        Data.Roles.TryGetValue(ev.Sender.SenderId, out var roles);
                        if (!roles.Star && !roles.Priest && !roles.Hand && !roles.Benefactor) return;
                        ev.Allowed = false;
                        if (Commands.EventMode)
                        {
                            ev.Reply = "Включен режим ивента. Админ-панель недоступна.";
                            ev.Success = false;
                            return;
                        }

                        if (Calls > 7)
                        {
                            ev.Success = false;
                            ev.Reply = "Уже было вызвано более 7 отрядов";
                            return;
                        }

                        if (Round.ElapsedTime.TotalMinutes < 3)
                        {
                            ev.Success = false;
                            ev.Reply = "Нельзя вызвать отряд в первые 3 минуты раунда";
                            return;
                        }

                        if ((DateTime.Now - LastCall).TotalSeconds < 90)
                        {
                            ev.Success = false;
                            var w = Math.Round(90 - (DateTime.Now - LastCall).TotalSeconds);
                            ev.Reply = $"Последний отряд был вызван менее 90 секунд назад\nПодождите {w} сек";
                            return;
                        }

                        if ((DateTime.Now - SpawnManager.LastEnter).TotalSeconds < 60)
                        {
                            ev.Success = false;
                            var w = Math.Round(60 - (DateTime.Now - SpawnManager.LastEnter).TotalSeconds);
                            ev.Reply = $"Последний отряд приехал менее 60 секунд назад\nПодождите {w} сек";
                            return;
                        }

                        string _lower = ev.Args[0].ToLower();
                        if (_lower == "respawn_mtf")
                        {
                            if (LastFaction == Faction.FoundationStaff)
                            {
                                ev.Success = false;
                                ev.Reply = "Последний отряд, вызванный донатером, был МТФ";
                                return;
                            }

                            Calls++;
                            LastFaction = Faction.FoundationStaff;
                            LastCall = DateTime.Now;
                            ev.Success = true;
                            ev.Reply = "Успешно";
                            MobileTaskForces.SpawnMtf();
                            return;
                        }

                        if (_lower == "respawn_ci")
                        {
                            if (LastFaction == Faction.FoundationEnemy)
                            {
                                ev.Success = false;
                                ev.Reply = "Последний отряд, вызванный донатером, был Хаос";
                                return;
                            }

                            Calls++;
                            LastFaction = Faction.FoundationEnemy;
                            LastCall = DateTime.Now;
                            ev.Success = true;
                            ev.Reply = "Успешно";
                            ChaosInsurgency.SpawnCI();
                            return;
                        }

                        break;
                    }
            }
        }
#endif
    }
}