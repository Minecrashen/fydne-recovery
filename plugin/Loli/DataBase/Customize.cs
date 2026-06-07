using HarmonyLib;
using InventorySystem.Items;
using InventorySystem.Items.Usables;
using MEC;
using Newtonsoft.Json;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using Qurre.API.Controllers;
using UnityEngine;

namespace Loli.DataBase
{
#if NR
    [HarmonyPatch(typeof(Adrenaline), "OnEffectsActivated")]
    static class Customize_Patch
    {
        internal static bool Prefix(Adrenaline __instance)
        {
            var pl = __instance.Owner.GetPlayer();

            if (!Customize.Customizes.ContainsKey(pl.UserInformation.UserId))
                return true;

            var gens = Customize.GetGens(pl);
            if (!gens.AdrenalineCompatible)
                return true;

            Customize.UseAdrenaline(pl, true);
            return false;
        }
    }
#endif

    static class Customize
    {
        [EventMethod(RoundEvents.Waiting)]
        static void Refresh()
        {
            Customizes.Clear();
        }

        static internal readonly Dictionary<string, BdData> Customizes = new();

        static Customize()
        {
            Core.Socket.On("database.get.donate.customize", obj =>
            {
                string[] hash = obj[1].ToString().Split(':');
                string userid = hash[0];
                Player pl = userid.GetPlayer();
                if (pl is null || $"{pl.UserInformation.Id}" != hash[1]) return;
                BdData json = JsonConvert.DeserializeObject<BdData>(obj[0].ToString());
                if (Customizes.ContainsKey(pl.UserInformation.UserId)) Customizes.Remove(pl.UserInformation.UserId);
                Customizes.Add(pl.UserInformation.UserId, json);
            });
        }

        [EventMethod(PlayerEvents.Leave)]
        static void Leave(LeaveEvent ev)
        {
            if (!Customizes.ContainsKey(ev.Player.UserInformation.UserId))
                return;

            Customizes.Remove(ev.Player.UserInformation.UserId);
        }

        [EventMethod(PlayerEvents.Spawn)]
        static void Spawn(SpawnEvent ev)
        {
            if (!Customizes.ContainsKey(ev.Player.UserInformation.UserId))
                return;

            Vector3 scale = GetScale(ev.Player, ev.Role);

#if MRP
            if (scale == Vector3.one)
                return;
#endif

            ev.Player.MovementState.Scale = scale;

#if MRP
			Timing.CallDelayed(2f, () => ev.Player.MovementState.Scale = scale);
#elif NR
            Timing.CallDelayed(0.5f, () => ev.Player.MovementState.Scale = scale);
#endif
        }

        [EventMethod(PlayerEvents.ChangeRole)]
        static void Spawn(ChangeRoleEvent ev)
        {
            if (!Customizes.ContainsKey(ev.Player.UserInformation.UserId))
                return;

#if MRP
            Timing.CallDelayed(2f, () =>
            {
                Vector3 scale = GetScale(ev.Player, ev.Role);
                if (scale != Vector3.one)
                    ev.Player.MovementState.Scale = scale;
            });
#elif NR
            Timing.CallDelayed(0.5f, () => ev.Player.MovementState.Scale = GetScale(ev.Player, ev.Role));
#endif
        }

#if NR
        [EventMethod(PlayerEvents.Attack, -1)]
        static void Damage(AttackEvent ev)
        {
            if (ev.FriendlyFire && !Server.FriendlyFire)
                return;

            if (!ev.Allowed)
                return;

            if (ev.Damage < 0)
                return;

            if (!Customizes.ContainsKey(ev.Target.UserInformation.UserId))
                return;

            var gens = GetGens(ev.Target);
            if (gens.NativeArmor)
                ev.Damage *= Random.Range(0.85f, 1);

            if (!gens.AdrenalineRush)
                return;

            if (ev.Damage > 100)
                return;

            if (Random.Range(0, 100) < 20)
                return;

            if (ev.Target.Effects.CheckActive<CustomPlayerEffects.Invigorated>())
                return;

            if (ev.Target.HealthInformation.Hp - ev.Damage < 2)
            {
                ev.Damage = 0;
                ev.Allowed = false;
                ev.Target.HealthInformation.Hp = 1;
                UseAdrenaline(ev.Target, gens.AdrenalineCompatible);
            }
        }

        internal static void UseAdrenaline(Player pl, bool best)
        {
            float d1 = 50;
            float d2 = 8;
            if (best)
            {
                d1 = 75;
                d2 = 16;
            }

            pl.HealthInformation.Stamina = 100f;
            pl.HealthInformation.Ahp = d1;
            pl.Effects.Enable<CustomPlayerEffects.Invigorated>(d2, true);
            pl.Effects.Controller.UseMedicalItem(Server.InventoryHost.CreateItemInstance(new(ItemType.Adrenaline, ItemSerialGenerator.GenerateNext()), false));

            if (best)
                Timing.RunCoroutine(PostFix(pl));

            static IEnumerator<float> PostFix(Player pl)
            {
                if (pl == null)
                    yield break;

                var rand = Random.Range(15, 20);
                for (int i = 0; i < rand; i++)
                {
                    yield return Timing.WaitForSeconds(1);
                    pl.HealthInformation.Heal(Random.Range(2, 4), false);
                }

                yield break;
            }
        }
#endif

        static internal Vector3 GetScale(Player pl, RoleTypeId role)
        {
            if (!Customizes.TryGetValue(pl.UserInformation.UserId, out BdData customize))
                return Vector3.one;

            Scales data = customize.Scales;
            switch (role)
            {
                case RoleTypeId.ClassD:
                    {
                        float cmz = (data.ClassD > 80 ? data.ClassD : 80) / 100;
                        return new Vector3(cmz, cmz, cmz);
                    }

                case RoleTypeId.Scientist:
                    {
                        float cmz = (data.Scientist > 80 ? data.Scientist : 80) / 100;
                        return new Vector3(cmz, cmz, cmz);
                    }

                case RoleTypeId.FacilityGuard:
                    {
                        float cmz = (data.Guard > 80 ? data.Guard : 80) / 100;
                        return new Vector3(cmz, cmz, cmz);
                    }

                case RoleTypeId.NtfCaptain or RoleTypeId.NtfPrivate
                or RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist:
                    {
                        float cmz = (data.Mtf > 80 ? data.Mtf : 80) / 100;
                        return new Vector3(cmz, cmz, cmz);
                    }

                case RoleTypeId.ChaosConscript or RoleTypeId.ChaosMarauder
                or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosRifleman:
                    {
                        float cmz = (data.Chaos > 80 ? data.Chaos : 80) / 100;
                        return new Vector3(cmz, cmz, cmz);
                    }

                case RoleTypeId.Tutorial:
                    {
                        float cmz = (data.Serpents > 80 ? data.Serpents : 80) / 100;
                        return new Vector3(cmz, cmz, cmz);
                    }
                default:
                    {
                        return Vector3.one;
                    }
            }
        }

#if NR
        static internal GensMod GetGens(Player pl)
        {
            if (!Customizes.ContainsKey(pl.UserInformation.UserId)) return new();
            var data = Customizes[pl.UserInformation.UserId].Genetics;
            return pl.RoleInformation.Role switch
            {
                RoleTypeId.ClassD => data.ClassD,
                RoleTypeId.Scientist => data.Scientist,
                RoleTypeId.FacilityGuard => data.Guard,
                RoleTypeId.NtfCaptain or RoleTypeId.NtfPrivate
                    or RoleTypeId.NtfSergeant or RoleTypeId.NtfSpecialist => data.Mtf,
                RoleTypeId.ChaosConscript or RoleTypeId.ChaosMarauder
                    or RoleTypeId.ChaosRepressor or RoleTypeId.ChaosRifleman => data.Chaos,
                RoleTypeId.Tutorial => data.Serpents,
                _ => new(),
            };
        }
#endif

        internal class BdData
        {
            [JsonProperty("genetics")]
            public Gens Genetics { get; set; } = new();

            [JsonProperty("scales")]
            public Scales Scales { get; set; } = new();
        }

        internal class GensMod
        {
            [JsonProperty("adrenaline_compatible")]
            public bool AdrenalineCompatible { get; set; } = false;

            [JsonProperty("adrenaline_rush")]
            public bool AdrenalineRush { get; set; } = false;

            [JsonProperty("native_armor")]
            public bool NativeArmor { get; set; } = false;
        }

        internal class Scales
        {
            public float ClassD { get; set; } = 100;
            public float Scientist { get; set; } = 100;
            public float Guard { get; set; } = 100;
            public float Mtf { get; set; } = 100;
            public float Chaos { get; set; } = 100;
            public float Serpents { get; set; } = 100;
        }
        internal class Gens
        {
            public GensMod ClassD { get; set; } = new();
            public GensMod Scientist { get; set; } = new();
            public GensMod Guard { get; set; } = new();
            public GensMod Mtf { get; set; } = new();
            public GensMod Chaos { get; set; } = new();
            public GensMod Serpents { get; set; } = new();
        }
    }
}