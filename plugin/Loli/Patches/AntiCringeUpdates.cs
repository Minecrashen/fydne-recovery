using HarmonyLib;
using InventorySystem.Items.Usables.Scp330;
using PlayerRoles;
using PlayerRoles.PlayableScps.Scp3114;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.API.Objects;
using Qurre.Events;
using Qurre.Events.Structs;
using System.Collections.Generic;
using System.Reflection.Emit;
using Qurre.API.World;
using UnityEngine;

namespace Loli.Patches
{
    static class FixSkeleton
    {
        [EventMethod(PlayerEvents.Spawn, int.MaxValue)]
        static void DontSpawnInLcz(SpawnEvent ev)
        {
            if (ev.Role is RoleTypeId.Scp3114)
            {
                ev.Position = Map.Rooms.Find(x => x.Type is RoomType.HczCrossing or RoomType.HczCurve or RoomType.HczStraight).Position + (Vector3.up * 2);
            }
        }
    }

    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerProcessCmd))]
    static class FixSkeletonAttack
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }

#if MRP
    [HarmonyPatch(typeof(Scp3114Spawner), nameof(Scp3114Spawner.OnPlayersSpawned))]
    static class FixSkeletonSpawn
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
#endif


    [HarmonyPatch(typeof(Scp3114Strangle), nameof(Scp3114Strangle.ServerUpdateTarget))]
    static class FixSkeletonUpdate
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0); // instance [Scp3114Strangle]
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(FixSkeletonUpdate), nameof(FixSkeletonUpdate.Update)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        static void Update(Scp3114Strangle instance)
        {
            if (instance.SyncTarget is null)
                return;

            instance.SyncTarget = null;
            instance._rpcType = Scp3114Strangle.RpcType.OutOfRange;
            instance.ServerSendRpc(true);
        }
    }


    [HarmonyPatch(typeof(CandyPink), nameof(CandyPink.ServerApplyEffects))]
    static class AntiCandyExplode
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ret);
        }
    }
}