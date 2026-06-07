using CustomPlayerEffects;
using Loli.Addons;
using Loli.Concepts;
using Loli.Controllers;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Qurre.API.World;
using Player = Qurre.API.Controllers.Player;
using Round = Qurre.API.World.Round;

#if MRP
using Loli.Addons.RolePlay;
using Loli.Modules.Voices;
using LabApi.Features.Wrappers;
#endif

namespace Loli.Spawns
{
    static class MobileTaskForces
    {
        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "MTF");
        static internal string AudioPathFirst { get; } = Path.Combine(DirectoryPath, "first.raw");
        static internal string AudioPathEmergency { get; } = Path.Combine(DirectoryPath, "emergency.raw");
        static internal string AudioPathStandart { get; } = Path.Combine(DirectoryPath, "standart.raw");
#if MRP
		static internal string AudioPathSweep { get; } = Path.Combine(DirectoryPath, "sweep.raw");
		static internal string AudioPathO5 { get; } = Path.Combine(DirectoryPath, "o5.raw");
#endif

        static MobileTaskForces()
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            (Core.CDNUrl + "/qurre/audio/mtf/first.raw").DownloadAudio(AudioPathFirst);
            (Core.CDNUrl + "/qurre/audio/mtf/emergency.raw").DownloadAudio(AudioPathEmergency);
            (Core.CDNUrl + "/qurre/audio/mtf/standart.raw").DownloadAudio(AudioPathStandart);
#if MRP
			(Core.CDNUrl + "/qurre/audio/mtf/sweep.raw").DownloadAudio(AudioPathSweep);
			(Core.CDNUrl + "/qurre/audio/mtf/o5.raw").DownloadAudio(AudioPathO5);
#endif
        }

        static internal readonly List<string> _usedUnits = new();
        static internal int Squads = 0;

        [EventMethod(RoundEvents.Waiting)]
        [EventMethod(RoundEvents.Start)]
        static void Refresh()
        {
            _usedUnits.Clear();
            Squads = 0;
        }

        [EventMethod(PlayerEvents.Dead)]
        static void Dead(DeadEvent ev)
        {
            if (ev.Target.Tag.Contains(" Gunner"))
            {
                ev.Target.Tag = ev.Target.Tag.Replace(" Gunner", "");
                ev.Target.HealthInformation.AhpActiveProcesses.ForEach(x => x.DecayRate = 0);
            }
            ev.Target.Tag = ev.Target.Tag.Replace(" Shiper", "").Replace(" Engineer", "");
        }

        [EventMethod(PlayerEvents.Attack)]
        static void Attack(AttackEvent ev)
        {
            if (ev.Attacker.Tag.Contains("Shiper") && ev.DamageType == DamageTypes.E11SR)
                ev.Damage *= 1.25f;
        }

        static public void SpawnMtf()
        {
#if NR
            if (Commands.EventMode)
                return;
#endif

            if (ConceptsController.IsActivated)
                return;

            if (Alpha.Detonated)
                return;

#if MRP
			if (Round.ElapsedTime.TotalSeconds > Addons.RolePlay.Sweep.WaitingSweep)
				return;
#endif

            if ((DateTime.Now - SpawnManager.LastEnter).TotalSeconds < 30)
                return;

            SpawnManager.LastEnter = DateTime.Now;

            Respawn.CallMtfHelicopter();

            Timing.CallDelayed(15f, () =>
            {
                if (Alpha.Detonated)
                    return;

                List<Player> list = Player.List.Where(x => x.RoleInformation.Role is RoleTypeId.Spectator).ToList();
                if (list.Count == 0)
                    return;

                list.Shuffle();

                if (CO2.CheckSpawnGroup(list))
                    return;

                int count = 0;
                Squads++;

                if (Squads == 1)
                {
#if NR
                    AudioExtensions.PlayInIntercom(AudioPathFirst, "C.A.S.S.I.E.");
#elif MRP
					VoiceCore.PlayInIntercom(AudioPathFirst, "C.A.S.S.I.E.");
#endif

                    CustomUnits.AddUnit("Разведгруппа", "#0089c7");
                    foreach (Player pl in list)
                    {
                        pl.Variables["UNIT"] = "Разведгруппа";
                        count++;
                        try
                        {
                            if (count == 1)
                                SpawnFirstOne(pl, Type.Commander);
                            else if (count < 7)
                                SpawnFirstOne(pl, Type.Lieutenant);
                            else
                                SpawnFirstOne(pl, Type.Cadet);

                            Timing.CallDelayed(0.4f, () =>
                            {
                                pl.UserInformation.CustomInfo = "Разведгруппа";
                                pl.UserInformation.InfoToShow &= ~PlayerInfoArea.UnitName;
                            });
                        }
                        catch { }
                    }
                }
                else if (Squads == 2)
                {
#if NR
                    AudioExtensions.PlayInIntercom(AudioPathEmergency, "C.A.S.S.I.E.");
#elif MRP
					VoiceCore.PlayInIntercom(AudioPathEmergency, "C.A.S.S.I.E.");
#endif

                    CustomUnits.AddUnit("Аварийный отряд", "#ff8f00");
                    foreach (Player pl in list)
                    {
                        pl.Variables["UNIT"] = "Аварийный отряд";
                        count++;
                        try
                        {
                            switch (count)
                            {
                                case 1: SpawnSecondOne(pl, SecondType.Commander); break;

#if MRP
								case 2 or 8 or 9: SpawnSecondOne(pl, SecondType.Engineer); break;
#elif NR
                                case 2: SpawnSecondOne(pl, SecondType.Engineer); break;
#endif

                                case 3: SpawnSecondOne(pl, SecondType.Sniper); break;
                                case 4: SpawnSecondOne(pl, SecondType.QuietSniper); break;
                                case 5: SpawnSecondOne(pl, SecondType.Gunner); break;
                                case 6: SpawnSecondOne(pl, SecondType.Physician); break;
                                case 7: SpawnSecondOne(pl, SecondType.Destroyer); break;

#if MRP
								case < 15: SpawnSecondOne(pl, SecondType.Lieutenant); break; // < 13
#elif NR
                                case < 13: SpawnSecondOne(pl, SecondType.Lieutenant); break;
#endif

                                default: SpawnSecondOne(pl, SecondType.Cadet); break;
                            }
                        }
                        catch { }
                    }
                }
                else
                {
                    string unit = "";
                    do
                    {
                        string code = Respawning.NamingRules.NineTailedFoxNamingRule.PossibleCodes[
                            UnityEngine.Random.Range(0, Respawning.NamingRules.NineTailedFoxNamingRule.PossibleCodes.Length - 1)
                            ];
                        int number = UnityEngine.Random.Range(1, 19);
                        unit = $"{code}-{number}";
                    }
                    while (_usedUnits.Contains(unit));
                    _usedUnits.Add(unit);

                    CustomUnits.AddUnit(unit, "#0074ff");

#if NR
                    AudioExtensions.PlayInIntercom(AudioPathStandart, "C.A.S.S.I.E.");
#elif MRP
					VoiceCore.PlayInIntercom(AudioPathStandart, "C.A.S.S.I.E.");
#endif

                    foreach (Player pl in list)
                    {
                        pl.Variables["UNIT"] = unit;
                        count++;
                        try
                        {
                            if (count == 1) SpawnOne(pl, Type.Commander);
                            else if (count < 7) SpawnOne(pl, Type.Lieutenant);
                            else SpawnOne(pl, Type.Cadet);
                        }
                        catch { }
                    }

                    Timing.CallDelayed(0.4f, () =>
                    {
                        foreach (Player pl in list)
                        {
                            pl.UserInformation.CustomInfo = unit;
                            pl.UserInformation.InfoToShow &= ~PlayerInfoArea.UnitName;
                        }
                    });
                }
            });
        }

        static public void SpawnOne(Player pl, Type type)
        {
            SpawnManager.SpawnProtect(pl);
            switch (type)
            {
                case Type.Commander:
                    pl.RoleInformation.Role = RoleTypeId.NtfCaptain;
                    break;
                case Type.Lieutenant:
                    pl.RoleInformation.Role = RoleTypeId.NtfSergeant;
                    break;
                case Type.Cadet:
                    pl.RoleInformation.Role = RoleTypeId.NtfPrivate;
                    break;
            }
        }

        static public void SpawnFirstOne(Player pl, Type type)
        {
            SpawnManager.SpawnProtect(pl);
            switch (type)
            {
                case Type.Commander:
                    pl.RoleInformation.Role = RoleTypeId.NtfCaptain;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFCaptain);
                        pl.Inventory.AddItem(ItemType.GunE11SR);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.GrenadeFlash);
                        pl.Inventory.AddItem(ItemType.Medkit);

#if NR
                        pl.Inventory.AddItem(ItemType.Painkillers);
#endif

                        pl.Inventory.AddItem(ItemType.Flashlight);
                        pl.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
						Item cuff = pl.AddCuff();
						cuff.AddRandomCuffAbility();
#endif
                    });
                    BcRec(pl, "<color=#0033ff>Командир</color>");
                    break;
                case Type.Lieutenant:
                    pl.RoleInformation.Role = RoleTypeId.NtfSergeant;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                        pl.Inventory.AddItem(ItemType.GunE11SR);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.GrenadeFlash);
                        pl.Inventory.AddItem(ItemType.Medkit);

#if NR
                        pl.Inventory.AddItem(ItemType.Painkillers);
#endif

                        pl.Inventory.AddItem(ItemType.Flashlight);
                        pl.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
						pl.AddCuff();
#endif
                    });
                    BcRec(pl, "<color=#0d6fff>Сержант</color>");
                    break;
                case Type.Cadet:
                    pl.RoleInformation.Role = RoleTypeId.NtfPrivate;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();

#if MRP
						pl.Inventory.AddItem(ItemType.KeycardMTFPrivate);
#elif NR
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
#endif

                        pl.Inventory.AddItem(ItemType.GunCrossvec);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.GrenadeFlash);
                        pl.Inventory.AddItem(ItemType.Medkit);

#if NR
                        pl.Inventory.AddItem(ItemType.Painkillers);
#endif

                        pl.Inventory.AddItem(ItemType.Flashlight);
                        pl.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
						pl.AddCuff();
#endif
                    });
                    BcRec(pl, "<color=#00bdff>Кадет</color>");
                    break;
            }
            static void BcRec(Player pl, string umm)
            {
                pl.Client.Broadcast($"<size=70%><color=#6f6f6f>Вы - {umm} <color=#0089c7>разведгруппы <color=#0047ec>МОГ</color></color>\n" +
                    "Ваша задача - разведать ситуацию в комплексе.</color></size>", 10, true);
            }
        }
        static public void SpawnSecondOne(Player pl, SecondType type)
        {
            SpawnManager.SpawnProtect(pl);
            Timing.CallDelayed(0.4f, () =>
            {
                pl.UserInformation.CustomInfo = "Аварийный отряд";
                pl.UserInformation.InfoToShow &= ~PlayerInfoArea.UnitName;
            });
            switch (type)
            {
                case SecondType.Commander:
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

#if NR
                        pl.Inventory.AddItem(ItemType.Adrenaline);
#endif

                        pl.Inventory.AddItem(ItemType.Flashlight);
                        pl.Inventory.AddItem(ItemType.ArmorCombat);

#if MRP
						Item cuff = pl.AddCuff();
						cuff.GetCell173();
						cuff.GetCell096();
#endif

                        pl.UserInformation.CustomInfo = "Капитан | Аварийный отряд";
                    });
                    BcRec(pl, "<color=#0033ff>Командир</color>", "отдавать высокоуровневые приказы");
                    break;
                case SecondType.Engineer:
                    pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardContainmentEngineer);
                        pl.Inventory.AddItem(ItemType.GunCrossvec);
                        pl.Inventory.AddItem(ItemType.Adrenaline);
                        pl.Inventory.AddItem(ItemType.Medkit);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.GrenadeFlash);
                        pl.Inventory.AddItem(ItemType.Flashlight);
                        pl.Inventory.AddItem(ItemType.ArmorHeavy);
                        pl.Tag = pl.Tag.Replace(" Engineer", "") + " Engineer";
                        pl.UserInformation.CustomInfo = "Инженер | Аварийный отряд";
                    });
                    BcRec(pl, "<color=#ff4640>Инженер</color>", "починить неисправности в комплексе");
                    break;
                case SecondType.Sniper:
                    pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                        pl.Inventory.AddItem(ItemType.GunE11SR);//, 40, 4, 3, 1);
                        pl.Inventory.AddItem(ItemType.GrenadeFlash);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.Adrenaline);
                        pl.Inventory.AddItem(ItemType.Medkit);
                        pl.Inventory.AddItem(ItemType.ArmorHeavy);
                        pl.Tag = pl.Tag.Replace(" Shiper", "") + " Shiper";
                        pl.UserInformation.CustomInfo = "Снайпер | Аварийный отряд";
                    });
                    BcRec(pl, "<color=#94ff00>Снайпер</color>", "устранить дальние цели, до которых не долетают обычные пули");
                    break;
                case SecondType.QuietSniper:
                    pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                        pl.Inventory.AddItem(ItemType.GunE11SR);//, 40, 3, 1, 3);

#if NR
                        pl.Inventory.AddItem(ItemType.SCP207);
#endif

                        pl.Inventory.AddItem(ItemType.GrenadeFlash);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.Adrenaline);
                        pl.Inventory.AddItem(ItemType.Medkit);
                        pl.Inventory.AddItem(ItemType.ArmorHeavy);

#if MRP
						pl.AddCuff();
#endif

                        pl.Tag = pl.Tag.Replace(" Shiper", "") + " Shiper";
                        pl.UserInformation.CustomInfo = "Бесшумный Снайпер | Аварийный отряд";
                    });
                    BcRec(pl, "<color=#415261>Бесшумный</color> <color=#94ff00>Снайпер</color>", "устранить цели максимально незаметно");
                    break;
                case SecondType.Gunner:
                    pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                        pl.Inventory.AddItem(ItemType.GunFRMG0);
                        pl.Inventory.AddItem(ItemType.SCP500);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.Adrenaline);
                        pl.Inventory.AddItem(ItemType.Medkit);
                        pl.Inventory.AddItem(ItemType.Flashlight);
                        pl.Inventory.AddItem(ItemType.ArmorHeavy);
                        pl.HealthInformation.AhpActiveProcesses.ForEach(x => x.DecayRate = 0);
                        pl.HealthInformation.MaxAhp = 250;
                        pl.HealthInformation.Ahp = 250;
                        pl.Tag = pl.Tag.Replace(" Gunner", "") + " Gunner";
                        pl.UserInformation.CustomInfo = "Пулеметчик | Аварийный отряд";
                        pl.Effects.Enable<Slowness>();
                        pl.Effects.SetIntensity<Slowness>(25);
                    });
                    BcRec(pl, "<color=#0ac067>Пулеметчик</color>", "устранять цели на ближней дистанции");
                    break;
                case SecondType.Physician:
                    pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                        pl.Inventory.AddItem(ItemType.GunE11SR);
                        pl.Inventory.AddItem(ItemType.SCP500);
                        pl.Inventory.AddItem(ItemType.Medkit);
                        pl.Inventory.AddItem(ItemType.Medkit);

#if NR
                        pl.Inventory.AddItem(ItemType.Medkit);
#endif

                        pl.Inventory.AddItem(ItemType.Adrenaline);
                        pl.Inventory.AddItem(ItemType.Radio);

#if MRP
						pl.Inventory.AddItem(ItemType.ArmorHeavy);
#endif

                        pl.UserInformation.CustomInfo = "Врач | Аварийный отряд";
                    });
                    BcRec(pl, "<color=#ff2222>Врач</color>", "лечить союзников");
                    break;
                case SecondType.Destroyer:
                    pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                    Timing.CallDelayed(0.5f, () =>
                    {
                        pl.Inventory.Clear();
                        pl.GetAmmo();
                        pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                        pl.Inventory.AddItem(ItemType.GunFRMG0);
                        pl.Inventory.AddItem(ItemType.GrenadeHE);
                        pl.Inventory.AddItem(ItemType.GrenadeHE);
                        pl.Inventory.AddItem(ItemType.GrenadeHE);
                        pl.Inventory.AddItem(ItemType.GrenadeHE);
                        pl.Inventory.AddItem(ItemType.Radio);
                        pl.Inventory.AddItem(ItemType.ArmorHeavy);
                        pl.UserInformation.CustomInfo = "Разрушитель | Аварийный отряд";
                    });
                    BcRec(pl, "<color=#ff3b00>Разрушитель</color>", "уничтожить цели с высоким уровнем защиты");
                    break;
                case SecondType.Lieutenant:
                    pl.RoleInformation.Role = RoleTypeId.NtfSergeant;
                    Timing.CallDelayed(0.5f, () => pl.UserInformation.CustomInfo = "Сержант | Аварийный отряд");
                    BcRec(pl, "<color=#0d6fff>Сержант</color>", "исполнять приказы высших по рангу");
                    break;
                case SecondType.Cadet:
                    pl.RoleInformation.Role = RoleTypeId.NtfPrivate;
                    Timing.CallDelayed(0.5f, () => pl.UserInformation.CustomInfo = "Кадет | Аварийный отряд");
                    BcRec(pl, "<color=#00bdff>Кадет</color>", "исполнять приказы высших по рангу");
                    break;
            }
            static void BcRec(Player pl, string umm, string desc)
            {
                pl.Client.Broadcast($"<size=70%><color=#6f6f6f>Вы - {umm} <color=#ff8f00>аварийного</color> <color=#0089c7>отряда</color> <color=#0047ec>МОГ</color>\n" +
                    $"Ваша задача - {desc}.</color></size>", 10, true);
            }
        }
        public enum Type : byte
        {
            Commander,
            Lieutenant,
            Cadet,
        }
        public enum SecondType : byte
        {
            Commander,
            Engineer,
            Sniper,
            QuietSniper,
            Gunner,
            Physician,
            Destroyer,
            Lieutenant,
            Cadet,
        }
    }
}