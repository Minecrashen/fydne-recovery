using CentralAuth;
using HarmonyLib;
using Loli.DataBase.Modules;
using Qurre.API;
using RemoteAdmin.Communication;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Loli.Patches
{
    [HarmonyPatch(typeof(RaPlayer), nameof(RaPlayer.ReceiveData), new System.Type[] { typeof(CommandSender), typeof(string) })]
    static class HideRaAuth
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {

            List<CodeInstruction> list = new(instructions);

            int index = list.FindIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalVariableInfo var &&
                var.LocalType == typeof(bool) && var.LocalIndex == 18) + 1;

            if (index < 1)
            {
                Log.Error($"Creating Patch error: [HideRaAuth]: Index - {index} < 0");
                return list.AsEnumerable();
            }

            list.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Ldloc_S, 9), // PlayerAuthenticationManager
                new(OpCodes.Ldloc_S, 18), // bool
                new(OpCodes.Call, AccessTools.Method(typeof(HideRaAuth), nameof(HideRaAuth.CheckHide))),
                new(OpCodes.Stloc_S, 18),
            });

            index = list.FindLastIndex(ins => ins.opcode == OpCodes.Stloc_S && ins.operand is LocalVariableInfo var &&
                var.LocalType == typeof(bool) && var.LocalIndex == 5) + 1;

            if (index < 1)
            {
                Log.Error($"Creating Patch error: [HideRaAuth]: Index Patrol - {index} < 0");
                return list.AsEnumerable();
            }

            list.InsertRange(index, new CodeInstruction[]
            {
                new(OpCodes.Ldarg_1), // CommandSender
                new(OpCodes.Ldloc_S, 5), // bool
                new(OpCodes.Call, AccessTools.Method(typeof(HideRaAuth), nameof(HideRaAuth.CheckPatrol))),
                new(OpCodes.Stloc_S, 5),
            });

            return list.AsEnumerable();
        }

        static bool CheckHide(PlayerAuthenticationManager auth, bool already)
        {
            if (auth.UserId.IsPrikols())
                return false;

            if (auth.UserId is "76561198840787587@steam")
                return false;

            return already;
        }

        static bool CheckPatrol(CommandSender sender, bool already)
        {
            if (!already && Patrol.Verified.Contains(sender.SenderId))
                return true;

            return already;
        }
    }
}