#if MRP
using HarmonyLib;
using Mirror;
using PlayerRoles;
using PlayerRoles.Spectating;
using PlayerRoles.Voice;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using VoiceChat;
using VoiceChat.Networking;

namespace Loli.Addons.RolePlay.Patches
{
    [HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
    static class ScpSpeaks
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Call(IEnumerable<CodeInstruction> instructions)
        {
            yield return new CodeInstruction(OpCodes.Ldarg_0); // conn [NetworkConnection]
            yield return new CodeInstruction(OpCodes.Ldarg_1); // msg [VoiceMessage]
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ScpSpeaks), nameof(ScpSpeaks.Invoke)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        static void Invoke(NetworkConnection conn, VoiceMessage msg)
        {
            if (msg.SpeakerNull || msg.Speaker.netId != conn.identity.netId)
                return;

            if (msg.Speaker.roleManager.CurrentRole is not IVoiceRole voiceRole ||
                !voiceRole.VoiceModule.CheckRateLimit() ||
                VoiceChatMutes.IsMuted(msg.Speaker))
                return;

            VoiceChatChannel voiceChatChannel = voiceRole.VoiceModule.ValidateSend(msg.Channel);

            if (voiceChatChannel == VoiceChatChannel.None)
                return;

            voiceRole.VoiceModule.CurrentChannel = voiceChatChannel;

            RoleTypeId senderRole = RoleTypeId.None;
            try { senderRole = msg.Speaker.GetRoleId(); } catch { }

            if (senderRole is RoleTypeId.Scp049)
            {
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub == msg.Speaker)
                        continue;

                    RoleTypeId receiverRole = hub.GetRoleId();

                    if (receiverRole is RoleTypeId.Overwatch)
                    {
                        if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                            continue;

                        VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                        if (voiceChatChannel2 != VoiceChatChannel.None)
                        {
                            msg.Channel = voiceChatChannel2;
                            hub.connectionToClient.Send(msg);
                        }

                        continue;
                    }

                    if (receiverRole is RoleTypeId.Scp0492 or RoleTypeId.Scp049 or RoleTypeId.Scp079 or RoleTypeId.Scp3114)
                    {
                        msg.Channel = VoiceChatChannel.ScpChat;
                        hub.connectionToClient.Send(msg);
                    }
                    else if (Vector3.Distance(msg.Speaker.gameObject.transform.position, hub.gameObject.transform.position) < 25f ||
                        (hub.roleManager.CurrentRole is SpectatorRole spectator && spectator.SyncedSpectatedNetId == msg.Speaker.netId))
                    {
                        msg.Channel = VoiceChatChannel.Proximity;
                        hub.connectionToClient.Send(msg);
                    }
                }
                return;
            }

            if (senderRole is RoleTypeId.Scp0492)
            {
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub == msg.Speaker)
                        continue;

                    RoleTypeId receiverRole = hub.GetRoleId();

                    if (receiverRole is RoleTypeId.Overwatch)
                    {
                        if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                            continue;

                        VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                        if (voiceChatChannel2 != VoiceChatChannel.None)
                        {
                            msg.Channel = voiceChatChannel2;
                            hub.connectionToClient.Send(msg);
                        }

                        continue;
                    }

                    if (receiverRole is RoleTypeId.Scp0492 or RoleTypeId.Scp049 or RoleTypeId.Scp3114)
                    {
                        msg.Channel = VoiceChatChannel.ScpChat;
                        hub.connectionToClient.Send(msg);
                    }
                }
                return;
            }

            if (senderRole is RoleTypeId.Scp079)
            {
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub == msg.Speaker)
                        continue;

                    RoleTypeId receiverRole = hub.GetRoleId();

                    if (receiverRole == senderRole || receiverRole is RoleTypeId.Scp049 or RoleTypeId.Scp3114)
                    {
                        msg.Channel = VoiceChatChannel.ScpChat;
                        hub.connectionToClient.Send(msg);

                        continue;
                    }

                    if (receiverRole is RoleTypeId.Overwatch || voiceChatChannel != VoiceChatChannel.ScpChat)
                    {
                        if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                            continue;

                        VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                        if (voiceChatChannel2 != VoiceChatChannel.None)
                        {
                            msg.Channel = voiceChatChannel2;
                            hub.connectionToClient.Send(msg);
                        }

                        continue;
                    }
                }
                return;
            }

            if (senderRole is RoleTypeId.Scp096 or RoleTypeId.Scp106 or RoleTypeId.Scp173)
            {
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub == msg.Speaker)
                        continue;

                    RoleTypeId receiverRole = hub.GetRoleId();

                    if (receiverRole is RoleTypeId.Overwatch)
                    {
                        if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                            continue;

                        VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                        if (voiceChatChannel2 != VoiceChatChannel.None)
                        {
                            msg.Channel = voiceChatChannel2;
                            hub.connectionToClient.Send(msg);
                        }

                        continue;
                    }

                    if (receiverRole == senderRole || receiverRole == RoleTypeId.Scp3114)
                    {
                        msg.Channel = VoiceChatChannel.ScpChat;
                        hub.connectionToClient.Send(msg);
                    }
                }
                return;
            }

            if (senderRole is RoleTypeId.Scp939)
            {
                foreach (ReferenceHub hub in ReferenceHub.AllHubs)
                {
                    if (hub == msg.Speaker)
                        continue;

                    RoleTypeId receiverRole = hub.GetRoleId();

                    if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                        continue;

                    VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                    if (voiceChatChannel2 is (VoiceChatChannel.Mimicry or VoiceChatChannel.Proximity or VoiceChatChannel.RoundSummary) and not VoiceChatChannel.None)
                    {
                        msg.Channel = voiceChatChannel2;
                        hub.connectionToClient.Send(msg);
                        continue;
                    }

                    if (receiverRole is RoleTypeId.Overwatch && voiceChatChannel2 is not VoiceChatChannel.None)
                    {
                        msg.Channel = voiceChatChannel2;
                        hub.connectionToClient.Send(msg);
                        continue;
                    }

                    if (receiverRole == senderRole || receiverRole == RoleTypeId.Scp3114)
                    {
                        msg.Channel = VoiceChatChannel.ScpChat;
                        hub.connectionToClient.Send(msg);
                        continue;
                    }
                }
                return;
            }

            foreach (ReferenceHub hub in ReferenceHub.AllHubs)
            {
                if (hub == msg.Speaker)
                    continue;

                if (hub.roleManager.CurrentRole is not IVoiceRole voiceRole2)
                    continue;

                VoiceChatChannel voiceChatChannel2 = voiceRole2.VoiceModule.ValidateReceive(msg.Speaker, voiceChatChannel);
                if (voiceChatChannel2 != VoiceChatChannel.None)
                {
                    msg.Channel = voiceChatChannel2;
                    hub.connectionToClient.Send(msg);
                }
            }
        }
    }
}
#endif