using Interactables.Interobjects.DoorUtils;
using Loli.Addons;
using Loli.Controllers;
using Loli.DataBase;
using Loli.Modules.Voices;
using Loli.Scps.Scp294.Drinks;
using Loli.Spawns;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Addons.Audio;
using Qurre.API.Addons.Models;
using Qurre.API.Attributes;
using Qurre.API.Controllers;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using Respawning;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Concepts
{
    static class CO2
    {
        static bool AllowSpawn { get; } = true;// Random.Range(0, 100) > 50;

        static string DirectoryPath { get; } = Path.Combine(Path.Combine(Paths.Plugins, "Audio"), "CO2");
        static string AudioPath { get; } = Path.Combine(DirectoryPath, "CO2.raw");
        static string AudioCancelPath { get; } = Path.Combine(DirectoryPath, "CO2Cancel.raw");
        static string AudioGroupPath { get; } = Path.Combine(DirectoryPath, "CO2Group.raw");

        internal const string Tag = " MTFCO2Group";
        const string Coroutine = "CO2ConceptCoroutine";

        static AudioPlayerBot _audioPlayer = null;

        static bool _spawned = false;
        static readonly List<Player> _activatedPlayers = new();

        static Primitive prim1;
        static Primitive prim2;

        static WorkStation station1;
        static WorkStation station2;

        static internal bool Activated { get; private set; }
        static bool AllowCancel { get; set; }
        static bool ActivatedDoor1 { get; set; }
        static bool ActivatedDoor2 { get; set; }

        static CO2()
        {
            if (!Directory.Exists(DirectoryPath))
                Directory.CreateDirectory(DirectoryPath);

            (Core.CDNUrl + "/qurre/audio/CO2.raw").DownloadAudio(AudioPath);
            (Core.CDNUrl + "/qurre/audio/CO2Cancel.raw").DownloadAudio(AudioCancelPath);
            (Core.CDNUrl + "/qurre/audio/CO2Group.raw").DownloadAudio(AudioGroupPath);
        }

        [EventMethod(RoundEvents.Waiting)]
        static void NullCall()
        {
            var room = RoomType.HczHid.GetRoom();
            Model hid = new("Hid", room.Position, room.Rotation.eulerAngles);
            Model root = new("CO2", new(0, 1.43f, -7.0817f), Vector3.zero, hid);

            Vector3 scale = new(0.3f, 0.2f, 0.1f);
            Color32 color = new(161, 157, 148, 255);
            new ModelPrimitive(root, PrimitiveType.Cube, color, Vector3.right * -1.3f, Vector3.zero, scale);
            new ModelPrimitive(root, PrimitiveType.Cube, color, Vector3.right * 1.3f, Vector3.zero, scale);

            prim1 = new ModelPrimitive(root, PrimitiveType.Cube, color, new(-1.3f, -0.2255f, 0.075f), Vector3.zero, new(0.5f, 0.277f, 0.1f)).Primitive;
            prim2 = new ModelPrimitive(root, PrimitiveType.Cube, color, new(1.3f, -0.2255f, 0.075f), Vector3.zero, new(0.5f, 0.277f, 0.1f)).Primitive;

            station1 = new ModelWorkStation(root, new(-1.2759f, -0.3467f, 0.0458f), Vector3.zero, new(0.193f, 0.232f, 0.06f)).WorkStation;
            station2 = new ModelWorkStation(root, new(1.3208f, -0.3467f, 0.0458f), Vector3.zero, new(0.193f, 0.232f, 0.06f)).WorkStation;

            StationsManager.Register(station1, Interact);
            StationsManager.Register(station2, Interact);

            AllowCancel = true;
            Activated = false;
            ActivatedDoor1 = false;
            ActivatedDoor2 = false;

            _activatedPlayers.Clear();
            _spawned = false;
            Timing.KillCoroutines(Coroutine);
        }

        static internal bool CheckSpawnGroup(List<Player> list)
        {
            if (_spawned)
                return false;

            if (!AllowSpawn)
                return false;

            if (RoundSummary.EscapedScientists == 0)
                return false;

            if (list.Count < 7)
                return false;

            if (Player.List.Any(x => x.RoleInformation.Role == RoleTypeId.Scientist))
                return false;

            SpawnGroup(list);
            return true;
        }

        static internal string SpawnGroupFromAdmin()
        {
            if (_spawned)
                return "Отряд СО2 уже приезжал";

            if (Player.List.Count(x => x.RoleInformation.Role is RoleTypeId.Spectator) < 4)
                return "Недостаточно персонала";

            Respawn.CallMtfHelicopter();

            Timing.CallDelayed(15f, () =>
            {
                List<Player> list = Player.List.Where(x => x.RoleInformation.Role is RoleTypeId.Spectator).ToList();

                if (list.Count < 4)
                    return;

                list.Shuffle();

                SpawnGroup(list);
            });

            return "Успешно";
        }

        static internal void SpawnGroup(List<Player> list)
        {
#if MRP
            VoiceCore.PlayInIntercom(AudioGroupPath, "C.A.S.S.I.E.");
#elif NR
            AudioExtensions.PlayInIntercom(AudioGroupPath, "C.A.S.S.I.E.");
#endif

            CustomUnits.AddUnit("Отряд CO2", "#14b1e0");
            _spawned = true;

            int _count = 0;
            foreach (Player pl in list)
            {
                if (_count > 10)
                    break;

                _count++;

                SpawnManager.SpawnProtect(pl);
                pl.Variables["UNIT"] = "Отряд CO2";
                pl.Tag += Tag;
                switch (_count)
                {
                    case 1:
                        {
                            pl.RoleInformation.Role = RoleTypeId.NtfCaptain;
                            Timing.CallDelayed(0.5f, () =>
                            {
                                pl.Inventory.Clear();
                                pl.GetAmmo();
                                pl.Inventory.AddItem(ItemType.KeycardMTFCaptain);
                                pl.Inventory.AddItem(ItemType.GunE11SR);
                                pl.Inventory.AddItem(ItemType.ParticleDisruptor);
                                pl.Inventory.AddItem(ItemType.SCP500);
                                pl.Inventory.AddItem(ItemType.Adrenaline);
                                pl.Inventory.AddItem(ItemType.AntiSCP207);
                                pl.Inventory.AddItem(ItemType.Radio);
                                pl.Inventory.AddItem(ItemType.ArmorCombat);
                                pl.UserInformation.CustomInfo = "Капитан | Отряд CO2";
                            });
                            BcRec(pl, "<color=#0033ff>Капитан</color>", "обеспечить активацию CO2 в комплексе");
                            break;
                        }
                    case < 8:
                        {
                            pl.RoleInformation.Role = RoleTypeId.NtfSpecialist;
                            Timing.CallDelayed(0.5f, () =>
                            {
                                pl.Inventory.Clear();
                                pl.GetAmmo();
                                pl.Inventory.AddItem(ItemType.KeycardMTFOperative);
                                pl.Inventory.AddItem(ItemType.GunE11SR);
                                pl.Inventory.AddItem(ItemType.SCP500);
                                pl.Inventory.AddItem(ItemType.Adrenaline);
                                pl.Inventory.AddItem(ItemType.Medkit);
                                pl.Inventory.AddItem(ItemType.AntiSCP207);
                                pl.Inventory.AddItem(ItemType.Radio);
                                pl.Inventory.AddItem(ItemType.ArmorCombat);
                                pl.UserInformation.CustomInfo = "Специалист | Отряд CO2";
                            });
                            BcRec(pl, "<color=#0d6fff>Сержант</color>", "следуя приказам Капитана, активировать CO2 в комплексе");
                            break;
                        }
                    default:
                        {
                            pl.RoleInformation.Role = RoleTypeId.NtfPrivate;
                            Timing.CallDelayed(0.5f, () => pl.UserInformation.CustomInfo = "Кадет | Отряд CO2");
                            BcRec(pl, "<color=#00bdff>Кадет</color>", "помочь высшим по рангу активировать CO2 в комплексе");
                            break;
                        }
                }
            }


            static void BcRec(Player pl, string umm, string desc)
            {
                pl.Client.Broadcast($"<size=70%><color=#6f6f6f>Вы - {umm} отряда активации <color=#14b1e0>CO2</color> <color=#0047ec>МОГ</color>\n" +
                    $"Ваша задача - {desc}</color></size>", 10, true);
            }
        }


        [EventMethod(PlayerEvents.Dead)]
        static void Dead(DeadEvent ev)
        {
            if (ev.Target is null)
                return;

            if (!ev.Target.Tag.Contains(Tag))
                return;

            ev.Target.Tag = ev.Target.Tag.Replace(Tag, "");
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev)
        {
            if (ev.Player is null)
                return;

            if (ev.Role is RoleTypeId.NtfCaptain or RoleTypeId.NtfSpecialist or RoleTypeId.NtfPrivate)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            if (ev.Player is null)
                return;

            if (ev.Role is RoleTypeId.NtfCaptain or RoleTypeId.NtfSpecialist or RoleTypeId.NtfPrivate)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(Tag, "");
        }


        static void CheckCancel(InteractWorkStationEvent ev)
        {
            if (!AllowCancel)
                return;

            if (ev.Player.Tag.Contains(Tag))
                return;

            if (_activatedPlayers.Contains(ev.Player))
                return;

            ev.Station.Status = WorkstationStatus.PoweringDown;

            if (ev.Station == station1)
            {
                _activatedPlayers.Add(ev.Player);
                ActivatedDoor1 = false;
                try { prim1.Color = new Color32(161, 157, 148, 255); } catch { }

                if (!ActivatedDoor2)
                {
                    Deactivate();
                    return;
                }
                Timing.CallDelayed(2f, () =>
                {
                    if (!Activated)
                        return;

                    ActivatedDoor1 = true;

                    ev.Station.Status = WorkstationStatus.Offline;

                    _activatedPlayers.Remove(ev.Player);

                    try { prim1.Color = new Color32(20, 177, 224, 255); } catch { }
                });
                return;
            }

            if (ev.Station == station2)
            {
                _activatedPlayers.Add(ev.Player);
                ActivatedDoor2 = false;
                try { prim1.Color = new Color32(161, 157, 148, 255); } catch { }

                if (!ActivatedDoor1)
                {
                    Deactivate();
                    return;
                }
                Timing.CallDelayed(2f, () =>
                {
                    if (!Activated)
                        return;

                    ActivatedDoor2 = true;

                    ev.Station.Status = WorkstationStatus.Offline;

                    _activatedPlayers.Remove(ev.Player);

                    try { prim2.Color = new Color32(20, 177, 224, 255); } catch { }
                });
                return;
            }

            static void Deactivate()
            {
                GlobalLights.TurnOff(5);
                Timing.CallDelayed(2f, () =>
                {
                    GlobalLights.SetToDefault(true, true);
                    DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadCancel);
                });

                AlphaController.DisableLock();
                AlphaController.ChangeState(false);
                ConceptsController.Disable();

                _audioPlayer?.DestroySelf();

#if MRP
                _audioPlayer = VoiceCore.PlayInIntercom(AudioCancelPath, "CO2");
#elif NR
                _audioPlayer = AudioExtensions.PlayInIntercom(AudioCancelPath, "CO2");
#endif

                Cassie.Lock = false;

                Activated = false;

                Timing.KillCoroutines(Coroutine);
            }
        }

        static internal void Interact(InteractWorkStationEvent ev)
        {
            if (ev.Station != station1 && ev.Station != station2)
                return;

            ev.Allowed = false;

            if (Activated)
            {
                CheckCancel(ev);
                return;
            }

            if (ConceptsController.IsActivated)
                return;

            if (!ev.Player.Tag.Contains(Tag))
                return;

            if (_activatedPlayers.Contains(ev.Player))
                return;

            ev.Station.Status = WorkstationStatus.PoweringUp;

            if (ev.Station == station1)
            {
                _activatedPlayers.Add(ev.Player);
                ActivatedDoor1 = true;
                try { prim1.Color = new Color32(20, 177, 224, 255); } catch { }

                if (ActivatedDoor2)
                {
                    Activate();
                    return;
                }
                Timing.CallDelayed(2f, () =>
                {
                    if (Activated)
                        return;

                    ActivatedDoor1 = false;

                    ev.Station.Status = WorkstationStatus.Offline;

                    _activatedPlayers.Remove(ev.Player);

                    try { prim1.Color = new Color32(161, 157, 148, 255); } catch { }
                });
                return;
            }

            if (ev.Station == station2)
            {
                _activatedPlayers.Add(ev.Player);
                ActivatedDoor2 = true;
                try { prim2.Color = new Color32(20, 177, 224, 255); } catch { }

                if (ActivatedDoor1)
                {
                    Activate();
                    return;
                }
                Timing.CallDelayed(2f, () =>
                {
                    if (Activated)
                        return;

                    ActivatedDoor2 = false;

                    ev.Station.Status = WorkstationStatus.Offline;

                    _activatedPlayers.Remove(ev.Player);

                    try { prim2.Color = new Color32(161, 157, 148, 255); } catch { }
                });
                return;
            }
        }

        static void GetXP()
        {
            foreach (var player in _activatedPlayers)
            {
                player.AddStats(500, 50, "выполнение своей задачи за МОГ отряда CO2");
            }
        }

        static void Activate()
        {
            Activated = true;

            try { GetXP(); } catch { }
            _activatedPlayers.Clear();

            GlobalLights.TurnOff(5);
            Timing.CallDelayed(2f, () =>
            {
                GlobalLights.ChangeColor(new Color32(20, 177, 224, 255), true, true, true);
                DoorEventOpenerExtension.TriggerAction(DoorEventOpenerExtension.OpenerEventType.WarheadStart);
                station1.Status = WorkstationStatus.Offline;
                station2.Status = WorkstationStatus.Offline;
            });

            try { Alpha.Stop(); } catch { }
            AlphaController.ChangeState(true, true);
            ConceptsController.Activate();

            int roundId = Round.CurrentRound;

            try { _audioPlayer?.DestroySelf(); } catch { }

#if MRP
            _audioPlayer = VoiceCore.PlayInIntercom(AudioPath, "CO2");
#elif NR
            _audioPlayer = AudioExtensions.PlayInIntercom(AudioPath, "CO2");
#endif

            Cassie.Lock = true;

            Timing.RunCoroutine(Process(), Coroutine);

            IEnumerator<float> Process()
            {
                yield return Timing.WaitForSeconds(160f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                AllowCancel = false;


                yield return Timing.WaitForSeconds(36f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                Timing.RunCoroutine(KillEveryone(), Coroutine);

                Builds.Models.Lift.Locked = true;
                foreach (var door in Map.Doors.Where(
                    x => x.Type is DoorType.ElevatorGateA or DoorType.ElevatorGateB))
                {
                    door.Open = false;
                    door.Lock = true;
                }


                yield return Timing.WaitForSeconds(60f);

                if (!Activated || Round.CurrentRound != roundId)
                    yield break;

                Round.End();
            }

            IEnumerator<float> KillEveryone()
            {
                while (Activated && Round.CurrentRound == roundId)
                {
                    try
                    {
                        foreach (var pl in Player.List)
                        {

                            try
                            {
                                if (pl.RoleInformation.IsScp)
                                    continue;

                                pl.HealthInformation.Damage(10, "Отравление угарным газом (CO2)");
                            }
                            catch { }
                        }
                    }
                    catch { }
                    yield return Timing.WaitForSeconds(1);
                }
                yield break;
            }
        }
    }
}