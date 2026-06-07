/*
using HarmonyLib;
using PlayerRoles;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace Loli.Patches
{
    static class AntiNewYear
    {
        [EventMethod(PlayerEvents.ChangeRole, int.MaxValue)]
        static void RemoveCringe(ChangeRoleEvent ev)
        {
            if (ev.Role is RoleTypeId.Flamingo or RoleTypeId.AlphaFlamingo or RoleTypeId.ZombieFlamingo)
            {
                ev.Allowed = false;
            }
        }

        [EventMethod(EffectEvents.Enabled, int.MaxValue)]
        static void RemoveCringe(EffectEnabledEvent ev)
        {
            if (ev.Type is EffectType.BecomingFlamingo)
            {
                ev.Allowed = false;
            }
        }

        [EventMethod(PlayerEvents.Damage, int.MaxValue)]
        static void DeleteSnows(DamageEvent ev)
        {
            if (ev.LiteType == LiteDamageTypes.Snowball)
            {
                ev.Allowed = false;
            }
        }
    }


    [HarmonyPatch(typeof(Scp956Pinata), nameof(Scp956Pinata.Update))]
    static class RemoveScp956
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }


    [HarmonyPatch(typeof(Scp2536GiftController), nameof(Scp2536GiftController.TryGrantWeapon))]
    static class ElkaNormalizeWeapon
    {
        [HarmonyPrefix]
        static bool Call()
            => false;
    }


    //[HarmonyPatch(typeof(Scp2536Controller), nameof(Scp2536Controller.Awake))]
    static class ElkaNormalize
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0); // instance [Scp2536Controller]
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ElkaNormalize), nameof(ElkaNormalize.Awake)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        static void Awake(Scp2536Controller instance)
        {
            Scp2536Controller.Singleton = instance;
            foreach (Material material in instance._mats)
            {
                material.SetFloat(Scp559Cake.ShaderDissolveProperty, 0f);
            }

            SpawnElka(instance);

            if (Scp2536Controller._init)
                return;

            Scp2536Controller._init = true;

            foreach (Scp2536Controller.SoundPerTeam soundPerTeam in instance._sounds)
            {
                Scp2536Controller.TeamClips.Add(soundPerTeam.Team, soundPerTeam.Clip);
            }
        }

        static void SpawnElka(Scp2536Controller instance)
        {
            Vector3 pos = GetRandomPosition();
            instance.RpcPlayTeamSpawn(PlayerRoles.Team.OtherAlive, pos);
            instance.GiftController.ServerPrepareGifts(true);
            instance.RpcMoveTree(pos, Quaternion.identity, (byte)UnityEngine.Random.Range(0, 256));
        }

        static Vector3 GetRandomPosition()
        {
            if (Scp2536Spawnpoint.Spawnpoints.Any())
            {
                return RandomElement.RandomItem(Scp2536Spawnpoint.Spawnpoints.ToArray()).Position;
            }

            return Map.Rooms.Find(x => x.Type == RoomType.HczCrossing)?.Position + Vector3.up ?? Vector3.zero;
        }
    }
}
*/