#if MRP
using Loli.Modules.Voices;
using Loli.Spawns;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using Qurre.API.World;
using UnityEngine;
using Cassie = Qurre.API.Controllers.Cassie;
using Map = Qurre.API.World.Map;
using Player = Qurre.API.Controllers.Player;
using Round = Qurre.API.World.Round;

namespace Loli.Addons.RolePlay
{
    static class Sweep
    {
        internal static float WaitingSweep => 1800f;
        internal static string SweepTag => "SweepGroup";

        [EventMethod(RoundEvents.Start)]
        static void RoundStart()
        {
            int round = Round.CurrentRound;
            Timing.RunCoroutine(DoRun());
            IEnumerator<float> DoRun()
            {
                yield return Timing.WaitForSeconds(WaitingSweep);
                bool spawned = false;
                while (!spawned)
                {
                    if (round != Round.CurrentRound) yield break;
                    Timing.RunCoroutine(SpawnSwapGroup());
                    yield return Timing.WaitForSeconds(30f);
                }
                IEnumerator<float> SpawnSwapGroup()
                {
                    if (!Player.List.Any(x => x.RoleInformation.Role == RoleTypeId.Spectator && !x.GamePlay.Overwatch)) yield break;
                    Respawn.CallMtfHelicopter();
                    yield return Timing.WaitForSeconds(15f);
                    List<Player> list = Player.List.Where(x => x.RoleInformation.Role == RoleTypeId.Spectator && !x.GamePlay.Overwatch).ToList();
                    if (list.Count == 0) yield break;
                    list.Shuffle();
                    spawned = true;
                    CustomUnits.AddUnit("Группа зачистки", "#ff0000");
                    foreach (Player pl in list) SpawnOne(pl);
                    VoiceCore.PlayInIntercom(MobileTaskForces.AudioPathSweep, "C.A.S.S.I.E.");
                    Timing.RunCoroutine(CheckSwapGroup());
                    Timing.RunCoroutine(LightsOff());
                    void SpawnOne(Player pl)
                    {
                        SpawnManager.SpawnProtect(pl);
                        pl.RoleInformation.SetNew(RoleTypeId.NtfCaptain, RoleChangeReason.Respawn);
                        Timing.CallDelayed(0.5f, () =>
                        {
                            pl.Inventory.Clear();
                            pl.GetAmmo();
                            pl.Inventory.AddItem(ItemType.KeycardFacilityManager);
                            pl.Inventory.AddItem(ItemType.GunE11SR);
                            pl.Inventory.AddItem(ItemType.Radio);
                            pl.Inventory.AddItem(ItemType.GrenadeHE);
                            pl.Inventory.AddItem(ItemType.SCP500);
                            pl.Inventory.AddItem(ItemType.Flashlight);
                            pl.Inventory.AddItem(ItemType.ArmorHeavy);
                            pl.Tag += SweepTag;
                            pl.HealthInformation.MaxHp = 200;
                            pl.HealthInformation.Hp = 200;

                            Item cuff = pl.AddCuff();
                            cuff.GetCell096();
                            cuff.GetCell173();
                        });
                        pl.Client.Broadcast($"<size=70%><color=#6f6f6f>Вы из <color=#ff0000>Группы зачистки</color> <color=#0047ec>МОГ</color>\n" +
                            "Ваша задача - зачистить комплекс, уничтожив всё живое в нем.\n(Кроме своих товарищей, конечно)</color></size>", 10, true);
                        pl.UserInformation.CustomInfo = "Группа зачистки";
                        pl.Variables["UNIT"] = "Группа зачистки";
                    }
                }
                IEnumerator<float> LightsOff()
                {
                    while (round == Round.CurrentRound)
                    {
                        GlobalLights.TurnOff(666666);
                        yield return Timing.WaitForSeconds(1f);
                        GlobalLights.ChangeColor(Color.black, true, true, true);
                        yield return Timing.WaitForSeconds(60f);
                    }
                }
                IEnumerator<float> CheckSwapGroup()
                {
                    yield return Timing.WaitForSeconds(1f);
                    while (Player.List.Where(x => x.Tag.Contains(SweepTag)).Count() > 2)
                        yield return Timing.WaitForSeconds(1f);
                    if (round != Round.CurrentRound) yield break;
                    Cassie.Send(".g1 g.2 . .g4 .g6");
                    Map.Broadcast($"<size=70%><color=#000000>Группа совета О5</color><color=#6f6f6f> выдвинулась в комплекс\n" +
                        "Объявляется эвакуация, в противном случае, вы будете устранены</color></size>", 10, true);
                    yield return Timing.WaitForSeconds(300f);
                    bool spawnedO5 = false;
                    while (!spawnedO5)
                    {
                        if (round != Round.CurrentRound) yield break;
                        Timing.RunCoroutine(SpawnO5Group());
                        yield return Timing.WaitForSeconds(30f);
                    }
                    IEnumerator<float> SpawnO5Group()
                    {
                        if (!Player.List.Any(x => x.RoleInformation.Role == RoleTypeId.Spectator && !x.GamePlay.Overwatch)) yield break;
                        Respawn.CallMtfHelicopter();
                        yield return Timing.WaitForSeconds(15f);
                        List<Player> list = Player.List.Where(x => x.RoleInformation.Role == RoleTypeId.Spectator && !x.GamePlay.Overwatch).ToList();
                        if (list.Count == 0) yield break;
                        list.Shuffle();
                        spawnedO5 = true;
                        CustomUnits.AddUnit("Группа совета О5", "#000000");
                        foreach (Player pl in list) SpawnOne(pl);
                        VoiceCore.PlayInIntercom(MobileTaskForces.AudioPathO5, "C.A.S.S.I.E.");
                        Timing.RunCoroutine(CheckSwapGroup());
                        void SpawnOne(Player pl)
                        {
                            SpawnManager.SpawnProtect(pl);
                            pl.RoleInformation.SetNew(RoleTypeId.NtfCaptain, RoleChangeReason.Respawn);
                            Timing.CallDelayed(0.5f, () =>
                            {
                                pl.Inventory.Clear();
                                pl.GetAmmo();
                                pl.Inventory.AddItem(ItemType.KeycardFacilityManager);
                                pl.Inventory.AddItem(ItemType.GunE11SR);
                                pl.Inventory.AddItem(ItemType.Radio);
                                pl.Inventory.AddItem(ItemType.GrenadeHE);
                                pl.Inventory.AddItem(ItemType.SCP500);
                                pl.Inventory.AddItem(ItemType.SCP500);
                                pl.Inventory.AddItem(ItemType.ArmorHeavy);
                                pl.HealthInformation.MaxAhp = 500;
                                pl.HealthInformation.MaxHp = 350;
                                pl.HealthInformation.Ahp = 500;
                                pl.HealthInformation.Hp = 350;
                                pl.DamageArmor(-1200);

                                Item cuff = pl.AddCuff();
                                cuff.GetCell096();
                                cuff.GetCell173();
                            });
                            pl.Client.Broadcast($"<size=30%><color=#6f6f6f>Вы из <color=#000000>Группы совета О5</color>\n" +
                                "Ваша задача - зачистить комплекс, уничтожив всё живое в нем.\n(Кроме своих товарищей, конечно)</color></size>", 10, true);
                            pl.UserInformation.CustomInfo = "Группа совета О5";
                            pl.Variables["UNIT"] = "Группа совета О5";
                        }
                    }
                }
            }
        }

        [EventMethod(PlayerEvents.Dead)]
        static void SweepFixTag(DeadEvent ev)
        {
            if (ev.Target == null)
                return;

            if (!ev.Target.Tag.Contains(SweepTag))
                return;

            ev.Target.Tag = ev.Target.Tag.Replace(SweepTag, "");
        }

        [EventMethod(PlayerEvents.ChangeRole, -2)]
        static void SweepFixTag(ChangeRoleEvent ev)
        {
            if (!ev.Allowed)
                return;

            if (ev.Player == null)
                return;

            if (ev.Role.GetTeam() == Team.FoundationForces)
                return;

            if (!ev.Player.Tag.Contains(SweepTag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(SweepTag, "");
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void SweepFixTag(SpawnEvent ev)
        {
            if (ev.Player == null)
                return;

            if (ev.Role.GetTeam() == Team.FoundationForces)
                return;

            if (!ev.Player.Tag.Contains(SweepTag))
                return;

            ev.Player.Tag = ev.Player.Tag.Replace(SweepTag, "");
        }
    }
}
#endif