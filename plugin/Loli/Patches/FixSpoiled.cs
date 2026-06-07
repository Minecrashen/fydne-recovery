using HarmonyLib;
using Loli.Addons;
using Mirror;
using Qurre.API;
using Qurre.API.Attributes;
using Qurre.Events;
using Qurre.Events.Structs;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Qurre.API.Controllers;
using UnityEngine;
using VoiceChat.Codec;
using VoiceChat.Codec.Enums;
using VoiceChat.Networking;
using VoiceChat.Playbacks;

namespace Loli.Patches;

[HarmonyPatch(typeof(VoiceTransceiver), nameof(VoiceTransceiver.ServerReceiveMessage))]
static class FixSpoiled
{
    static internal readonly HashSet<ReferenceHub> Whitelist = [];

    static readonly OpusDecoder Decoder = new();
    static readonly OpusEncoder Encoder = new(OpusApplicationType.Voip);

    static readonly ConcurrentDictionary<ReferenceHub, int> ExploitMessages = [];

    [HarmonyPrefix]
    static bool Call(NetworkConnection conn, ref VoiceMessage msg)
    {
        try
        {
            if (msg.Speaker == null || conn.identity.netId != msg.Speaker.netId)
                return false;

            if (Whitelist.Contains(msg.Speaker))
                return true;

            var samples = new float[24000];
            int length = Decoder.Decode(msg.Data, msg.DataLength, samples);
            if (length != 480) return false;

            var (min, max) = (samples.Min(), samples.Max());
            float maxVolume = Mathf.Max(max, -min);

            if (maxVolume > 1)
            {
                if (maxVolume > 100 && !PersonalRadioPlayback.IsTransmitting(msg.Speaker))
                {
                    int exploitAttempts = ExploitMessages.AddOrUpdate(msg.Speaker, 1, (_, count) => count + 1);
                    if (exploitAttempts >= 10)
                    {
                        Player pl = msg.Speaker.GetPlayer();

                        ServerConsole.Disconnect(msg.Speaker.gameObject, "You have been Globally Banned.");
                        pl.ReportPlayer($"Spoiled Voice\nИгрок кикнут и изолирован от общества.\nПопыток превышения: {exploitAttempts}");
                    }
                }

                ScaleSamples(samples, 1 / maxVolume);
                msg = msg with { Data = Encode(samples) };
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error while checking voice message: {e}");
            return false;
        }

        return true;
    }

    static void ScaleSamples(float[] samples, float scale)
    {
        for (var i = 0; i < samples.Length; i++)
            samples[i] *= scale;
    }

    static byte[] Encode(float[] samples)
    {
        var data = new byte[512];
        Encoder.Encode(samples, data);
        return data;
    }

    [EventMethod(PlayerEvents.Leave)]
    static void Leave(LeaveEvent ev)
    {
        Whitelist.Remove(ev.Player.ReferenceHub);
    }
}