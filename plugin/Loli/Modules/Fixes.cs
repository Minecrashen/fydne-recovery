using CustomPlayerEffects;

#if NR
using InventorySystem.Configs;
#endif

using Loli.Addons;
using MEC;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using LabApi.Features.Wrappers;
using UnityEngine;
using Map = Qurre.API.World.Map;
using Player = Qurre.API.Controllers.Player;
using Round = Qurre.API.World.Round;
using Server = Qurre.API.Server;

namespace Loli.Modules
{
    static class Fixes
    {
        static readonly DateTime _init = DateTime.Now;

        static TimeSpan Elapsed => DateTime.Now - _init;

        [EventMethod(RoundEvents.Waiting)]
        static void Waiting()
        {
            _frz = 0;
            _lastRound = -1;
        }

        [EventMethod(RoundEvents.End)]
        static void FixFastEnd()
        {
            if (Elapsed.TotalSeconds < 60)
                return;

            if (Round.ElapsedTime.TotalSeconds < 60)
            {
                Timing.RunCoroutine(Run());

                static IEnumerator<float> Run()
                {
                    yield return Timing.WaitForSeconds(3f);
                    Round.DimScreen();
                    yield return Timing.WaitForSeconds(1f);
                    Extensions.RestartCrush("Раунд закончился в первую минуту игры");
                    yield break;
                }
            }
        }

        static int _frz = 0;
        static int _lastRound = -1;
        static internal void FixRoundFreeze()
        {
            if (Elapsed.TotalSeconds < 10)
                return;

            if (Round.Started)
                return;

            if (_lastRound == -1)
                return;

            if (GameCore.RoundStart.singleton.NetworkTimer != _lastRound)
            {
                _frz = 0;
                _lastRound = GameCore.RoundStart.singleton.NetworkTimer;
                return;
            }

            _frz++;
            if (_frz > 20)
                Extensions.RestartCrush("Зафризился таймер начала раунда");
        }

        static int _bcdead = 0;
        static internal void CheckBcAlive()
        {
            if (Elapsed.TotalSeconds < 30)
                return;

            try
            {
                if (Broadcast.Singleton is null)
                    throw new NullReferenceException("");

                _bcdead = 0;
            }
            catch
            {
                _bcdead++;
            }

            if (_bcdead > 10)
                Extensions.RestartCrush("Умерли игровые скрипты, проверка на броадкастах");
        }

        static int ffe_process = 0;
        static internal void FixFreezeEnd()
        {
            if (!Round.Started || Round.Waiting)
            {
                ffe_process = 0;
                return;
            }

            if (Player.List.Any())
            {
                ffe_process = 0;
                return;
            }

            if (ffe_process < 30)
            {
                ffe_process++;
                return;
            }

            Extensions.RestartCrush("Фриз сервера при отсутствии игроков");
        }

        [EventMethod(PlayerEvents.Spawn, int.MinValue)]
        static void FixZombieSpawn(SpawnEvent ev)
        {
            if (ev.Role != RoleTypeId.Scp0492)
                return;

            if (Vector3.Distance(ev.Position, Vector3.zero) > 3 &&
#if MRP
				Vector3.Distance(ev.Position, Vector3.down * 300) > 3)
#elif NR
                Vector3.Distance(ev.Position, Vector3.down * 2000) > 3)
#endif
            {
                return;
            }

            ev.Position = GetZombiePoint();

            static Vector3 GetZombiePoint()
            {
                if (Player.List.TryFind(out var doctor, x => x.RoleInformation.Role == RoleTypeId.Scp049))
                    return doctor.MovementState.Position;
                if (Map.Rooms.TryFind(out var room, x => x.Type == RoomType.Hcz049))
                    return room.Position + (Vector3.up * 2);

                return new(86, 989, -69);
            }
        }

#if NR
        [EventMethod(PlayerEvents.Damage)]
        static void AntiLiftKills(DamageEvent ev)
        {
            if (ev.LiteType is not LiteDamageTypes.Scp018 and not LiteDamageTypes.Explosion)
            {
                return;
            }

            if (!Map.Doors.Any(x => Vector3.Distance(x.Position, ev.Target.MovementState.Position) < 2f))
            {
                return;
            }

            ev.Allowed = false;
        }
#endif

        [EventMethod(PlayerEvents.Escape)]
        static void AntiEscapeBag(EscapeEvent ev)
        {
            if (Round.ElapsedTime.TotalSeconds < 10)
                ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.Dies)]
        static void Dies(DiesEvent ev)
        {
            if (!Round.Started && Round.ElapsedTime.TotalSeconds < 1)
                ev.Allowed = false;
        }

#if NR
        [EventMethod(PlayerEvents.Cuff)]
        static void AntiTeamCuff(CuffEvent ev)
        {
            if (ev.Cuffer.GetTeam().GetFaction() == ev.Target.GetTeam().GetFaction())
                ev.Allowed = false;
        }

        [EventMethod(PlayerEvents.DropAmmo)]
        static void AntiFlood(DropAmmoEvent ev) => ev.Allowed = false;

        [EventMethod(MapEvents.CreatePickup, int.MinValue)]
        static void AntiAmmo(CreatePickupEvent ev)
        {
            if (ev.Info.ItemId == ItemType.Ammo12gauge || ev.Info.ItemId == ItemType.Ammo44cal ||
                ev.Info.ItemId == ItemType.Ammo556x45 || ev.Info.ItemId == ItemType.Ammo762x39 ||
                ev.Info.ItemId == ItemType.Ammo9x19) ev.Allowed = false;
        }
#endif

        [EventMethod(PlayerEvents.ChangeItem)]
        static void FixInvisible(ChangeItemEvent ev)
        {
            if (ev.NewItem == null)
                return;

            if (!ev.Player.Effects.CheckActive<Invisible>())
                return;

            if (ev.NewItem.Category != ItemCategory.Firearm && ev.NewItem.Category != ItemCategory.Grenade &&
                ev.NewItem.Category != ItemCategory.SCPItem && ev.NewItem.Category != ItemCategory.SpecialWeapon)
                return;

            ev.Player.Effects.Disable<Invisible>();
        }

        [EventMethod(MapEvents.CorpseSpawned)]
        static void FixRagdollScale(CorpseSpawnedEvent ev)
        {
            if (ev.Corpse.Owner == Server.Host)
                return;

            var s1 = ev.Corpse.Scale;
            var s2 = ev.Corpse.Owner.MovementState.Scale;
            ev.Corpse.Scale = new(s1.x * s2.x, s1.y * s2.y, s1.z * s2.z);
        }

#if NR
        [EventMethod(RoundEvents.Waiting)]
        static void FixItemsLimits()
        {
            InventoryLimits.StandardAmmoLimits[ItemType.Ammo9x19] = 9999;
            InventoryLimits.StandardAmmoLimits[ItemType.Ammo556x45] = 9999;
            InventoryLimits.StandardAmmoLimits[ItemType.Ammo762x39] = 9999;
            InventoryLimits.StandardAmmoLimits[ItemType.Ammo44cal] = 9999;
            InventoryLimits.StandardAmmoLimits[ItemType.Ammo12gauge] = 9999;

            InventoryLimits.StandardCategoryLimits[ItemCategory.Armor] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.Grenade] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.Keycard] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.Medical] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.SpecialWeapon] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.Radio] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.SCPItem] = 8;
            InventoryLimits.StandardCategoryLimits[ItemCategory.Firearm] = 8;
        }
#endif

        [EventMethod(PlayerEvents.PickupItem)]
        static void FixFreeze(PickupItemEvent ev)
        {
            if (ev.Player.Inventory.Base.UserInventory.Items.Any(x => x.Value.ItemSerial == ev.Pickup.Info.Serial))
            {
                ev.Allowed = false;
                var item = ev.Player.Inventory.AddItem(ev.Pickup.Info.ItemId);
                var ser = ev.Pickup.Info.Serial;
                Pickup.Get(ev.Pickup).Destroy();
                if (Clear.Pickups.Contains(ser))
                    Clear.Pickups.Add(item.ItemSerial);
            }
        }

        [EventMethod(ServerEvents.RemoteAdminCommand, int.MaxValue)]
        static void FixCrashes(RemoteAdminCommandEvent ev)
        {
            if (ev.Sender.SenderId != "SERVER CONSOLE")
                return;

            switch (ev.Name)
            {
                case "forceclass": ev.Allowed = false; return;
                case "give": ev.Allowed = false; return;
                default: return;
            }
        }

        [EventMethod(RoundEvents.Start)]
        static void FixNotSpawn()
        {
            Timing.CallDelayed(1f, () =>
            {
                int pls = Player.List.Count();
                int pls2 = Player.List.Count(x => x.RoleInformation.IsAlive || x.RoleInformation.Role == RoleTypeId.Overwatch);
                if (pls == 0 || pls / 1.5 > pls2)
                {
                    Timing.CallDelayed(5f, () =>
                    {
                        foreach (Player pl in Player.List)
                            pl.Client.DimScreen();
                        Timing.CallDelayed(1f, () => Extensions.RestartCrush("Не заспавнило больше половины игроков"));
                    });
                    try { RoundSummary.singleton.RpcShowRoundSummary(RoundSummary.singleton.classlistStart, default, RoundSummary.LeadingTeam.Draw, 0, 0, 0, 5, 1); } catch { }
                }
            });
        }


#if NR
        [EventMethod(PlayerEvents.Damage)]
        static void Scp106PocketEnter(DamageEvent ev)
        {
            if (!ev.Allowed)
                return;
            if (ev.Attacker.RoleInformation.Role != RoleTypeId.Scp106)
                return;

            ev.Target.Effects.Enable(EffectType.PocketCorroding);
            ev.Target.Effects.Enable(EffectType.Sinkhole);
        }
#endif


        static internal void CheckPlayersPing()
        {
            string pings = string.Empty;

            foreach (var pl in Player.List)
            {
                if (pl.RoleInformation.Role is not RoleTypeId.Spectator and not RoleTypeId.Overwatch and not RoleTypeId.Filmmaker and not RoleTypeId.None and not RoleTypeId.Scp079 && (Time.time - pl.LastSynced) > 1f)
                {
                    Timing.RunCoroutine(Coroutine());
                    IEnumerator<float> Coroutine()
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            yield return Timing.WaitForSeconds(0.5f);

                            if (pl.Disconnected)
                                yield break;

                            if ((Time.time - pl.LastSynced) < 1f)
                                yield break;

#if NR
                            Log.Custom($"{pl.UserInformation.Nickname}: {Time.time - pl.LastSynced}", "FIX NW #" + (i + 1), ConsoleColor.Blue);
#endif
                        }

                        FastReconnect.Process(pl);
                    }
                    pings += $"{pl.UserInformation.Nickname}: {Time.time - pl.LastSynced}; ";
                }
            }

            if (!string.IsNullOrEmpty(pings))
                Log.Debug(pings);
        }


        static int AntiAllFreeze_Count = 0;
        static int AntiAllFreeze_NeedPlayers
            => Math.Max(Math.Max(Player.List.Count(), 1) / 6 * 4, 5);

        static internal void AntiAllFreeze()
        {
            int laggedCount = 0;

            var list = Player.List.Where(x => (DateTime.Now - x.JoinedTime).TotalMinutes > 1 && x.RoleInformation.IsAlive && !x.Disconnected);

            if (list.Count() < 5)
            {
                AntiAllFreeze_Count = 0;
                return;
            }

            if (Round.Waiting)
            {
                AntiAllFreeze_Count = 0;
                return;
            }

            if (Round.Ended)
            {
                AntiAllFreeze_Count = 0;
                return;
            }

            foreach (var pl in list)
            {
                Vector3 currentPos = pl.MovementState.Position;
                Vector3 currentRot = pl.MovementState.Rotation;

                if (pl.Variables["AntiFreeze_CachePos"] is Vector3 cachedPos &&
                    pl.Variables["AntiFreeze_CacheRot"] is Vector3 cachedRot)
                {
                    if (Vector3.Distance(cachedPos, currentPos) < 0.1f &&
                        Vector3.Distance(cachedRot, currentRot) < 0.1f)
                        laggedCount++;
                }

                pl.Variables["AntiFreeze_CachePos"] = currentPos;
                pl.Variables["AntiFreeze_CacheRot"] = currentRot;
            }

            if (laggedCount >= AntiAllFreeze_NeedPlayers)
            {
                AntiAllFreeze_Count++;
            }
            else
            {
                AntiAllFreeze_Count = 0;
            }

            if (AntiAllFreeze_Count == 20)
            {
                Map.Broadcast("<b><color=red>Замечен фриз всех игроков</color></b>\nПодвигайтесь, если это не так", 10, true);
            }

            if (AntiAllFreeze_Count > 30)
            {
                Extensions.RestartCrush("Замечен фриз игроков при проверке позиции и ротации");
            }
        }
    }
}